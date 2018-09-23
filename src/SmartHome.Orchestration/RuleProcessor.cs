using System;
using System.Collections.Generic;
using System.Threading;
using SmartHome.Lib;
using SmartHome.Lib.Environment;
using SmartHome.Lib.Lights;
using SmartHome.Lib.Sensors;

namespace SmartHome.Orchestration
{
    class RuleProcessor
    {
        private readonly Rule[] _rules;
        private readonly SmartHub _smartHub;

        private bool _active;
        private readonly Thread _workerThread;

        private int _lightRefreshCounter;

        private Dictionary<string, string> _lightRoomMap;
        private Dictionary<string, string> _sensorRoomMap;

        private Dictionary<string, DateTime> _activityMap;

        public RuleProcessor(Rule[] rules, SmartHub smartHub)
        {
            _workerThread = new Thread(UpdateLoop);

            _rules = rules;
            _smartHub = smartHub;

            InitializeMaps();
        }

        public void Run()
        {
            _active = true;
            _workerThread.Start();
        }

        public void Stop()
        {
            _active = false;
            _workerThread.Join();
        }

        private void InitializeMaps()
        {
            _lightRoomMap = new Dictionary<string, string>();
            _sensorRoomMap = new Dictionary<string, string>();
            _activityMap = new Dictionary<string, DateTime>();

            foreach (Room room in _smartHub.Environment.Rooms)
            {
                if (room.Lights != null)
                {
                    foreach (string lightId in room.Lights)
                    {
                        _lightRoomMap.Add(lightId, room.Id);
                    }
                }

                if (room.Sensors != null)
                {
                    foreach (string sensorId in room.Sensors)
                    {
                        _sensorRoomMap.Add(sensorId, room.Id);

                        if (!_activityMap.ContainsKey(room.Id))
                        {
                            _activityMap.Add(room.Id, DateTime.Now);
                        }
                    }
                }
            }
        }

        private void UpdateLoop()
        {
            Dictionary<string, ISmartSensor> previousState = GetSensorState();

            IList<ISmartLight> lights = _smartHub.PollLights();

            Console.WriteLine("Pulled initial sensor and light states");

            while (_active)
            {
                Dictionary<string, ISmartSensor> currentState = GetSensorState();

                if (_lightRefreshCounter > 15)
                {
                    lights = _smartHub.PollLights();

                    _lightRefreshCounter = 0;
                }

                int currentTime = DateTime.Now.Hour;

                foreach (Rule rule in _rules)
                {
                    int satisfiedRules = 0;

                    // Iterate over each condition and check if they are satisifed
                    foreach (RuleCondition condition in rule.Conditions)
                    {
                        switch (condition.Type)
                        {
                            case RuleConditionType.TimeRange:
                                if (condition.TimeRange.Start <= currentTime &&
                                    condition.TimeRange.End >= currentTime)
                                {
                                    satisfiedRules++;
                                }
                                break;
                            case RuleConditionType.MotionDetection:
                                if (!previousState[condition.RoomId].DetectMotion &&
                                    currentState[condition.RoomId].DetectMotion)
                                {
                                    satisfiedRules++;
                                }
                                break;
                            case RuleConditionType.NoMotionDetection:
                                TimeSpan timeSinceActivity = DateTime.Now - _activityMap[condition.RoomId];

                                if (timeSinceActivity.TotalSeconds > condition.DurationInSeconds)
                                {
                                    satisfiedRules++;
                                }
                                break;
                            case RuleConditionType.LightState:
                                foreach (ISmartLight light in lights)
                                {
                                    if (_lightRoomMap[light.Id] == condition.RoomId &&
                                        light.State.On == condition.LightState.On)
                                    {
                                        satisfiedRules++;
                                        break;
                                    }
                                }
                                break;
                        }
                    }

                    // All rules were satisfied, run the actions
                    if (satisfiedRules == rule.Conditions.Length)
                    {
                        foreach (RuleAction ruleAction in rule.Actions)
                        {
                            Console.WriteLine("Executing rule " + rule.Name);

                            if (ruleAction.RoomIds != null)
                            {
                                foreach (string roomId in ruleAction.RoomIds)
                                {
                                    _smartHub.UpdateLightsInRooms(new[] { roomId }, ruleAction.LightState);
                                }
                            }
                        }

                        // Force an update to light states
                        lights = _smartHub.PollLights();
                    }
                }

                _lightRefreshCounter++;
                previousState = currentState;

                Thread.Sleep(500);
            }
        }

        private Dictionary<string, ISmartSensor> GetSensorState()
        {
            var map = new Dictionary<string, ISmartSensor>();

            foreach (ISmartSensor sensor in _smartHub.PollSensors())
            {
                string roomId = _sensorRoomMap[sensor.Id];

                map.Add(roomId, sensor);

                if (sensor.DetectMotion)
                {
                    _activityMap[roomId] = DateTime.Now;
                }
            }

            return map;
        }
    }
}
