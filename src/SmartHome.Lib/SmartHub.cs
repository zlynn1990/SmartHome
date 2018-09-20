using System;
using System.Collections.Generic;
using SmartHome.Lib.Adapters;
using SmartHome.Lib.Environment;
using SmartHome.Lib.Lights;
using SmartHome.Lib.Sensors;

namespace SmartHome.Lib
{
    public class SmartHub
    {
        public EnvironmentConfig Environment { get; set; }

        public IList<ILightAdapter> LightAdapters { get; set; }
        public IList<ISensorAdapter> SensorAdapters { get; set; }

        private readonly Dictionary<string, int> _lightByAdapterMap;
        private readonly Dictionary<string, int> _sensorByAdapterMap;

        private readonly Dictionary<string, Room> _roomMapping;

        public SmartHub()
        {
            _lightByAdapterMap = new Dictionary<string, int>();
            _sensorByAdapterMap = new Dictionary<string, int>();

            _roomMapping = new Dictionary<string, Room>();
        }

        public void Initialize()
        {
            foreach (Room room in Environment.Rooms)
            {
                _roomMapping.Add(room.Id, room);
            }

            for (int i = 0; i < LightAdapters.Count; i++)
            {
                ILightAdapter adapter = LightAdapters[i];

                foreach (ISmartLight light in adapter.PollLights())
                {
                    if (_lightByAdapterMap.ContainsKey(light.Id))
                    {
                        throw new Exception($"Duplicate light '{light.Id}' detected!");
                    }

                    _lightByAdapterMap.Add(light.Id, i);
                }
            }

            for (int i = 0; i < SensorAdapters.Count; i++)
            {
                ISensorAdapter adapter = SensorAdapters[i];

                foreach (ISmartSensor sensor in adapter.PollSensors())
                {
                    if (_sensorByAdapterMap.ContainsKey(sensor.Id))
                    {
                        throw new Exception($"Duplicate sensor '{sensor.Id}' detected!");
                    }

                    _sensorByAdapterMap.Add(sensor.Id, i);
                }
            }
        }

        public IList<ISmartLight> PollLights()
        {
            var lights = new List<ISmartLight>();

            foreach (ILightAdapter adapter in LightAdapters)
            {
                lights.AddRange(adapter.PollLights());
            }

            return lights;
        }

        public void UpdateLight(ISmartLight light)
        {
            UpdateLight(light.Id, light.State);
        }

        public void UpdateLight(string lightId, LightState state)
        {
            if (!_lightByAdapterMap.ContainsKey(lightId))
            {
                throw new Exception($"Light '{lightId}' was not found by any adapter!");
            }

            int adapterId = _lightByAdapterMap[lightId];

            LightAdapters[adapterId].UpdateLight(lightId, state);
        }

        public void UpdateLightsInRooms(IEnumerable<string> roomIds, LightState state)
        {
            foreach (string roomId in roomIds)
            {
                if (!_roomMapping.ContainsKey(roomId))
                {
                    throw new Exception($"Room '{roomId}' was not found in the environment configuration!");
                }

                foreach (ILightAdapter adapter in LightAdapters)
                {
                    adapter.UpdateRoom(roomId, state);
                }
            }
        }

        public IList<ISmartSensor> PollSensors()
        {
            var sensors = new List<ISmartSensor>();

            foreach (ISensorAdapter adapter in SensorAdapters)
            {
                sensors.AddRange(adapter.PollSensors());
            }

            return sensors;
        }
    }
}
