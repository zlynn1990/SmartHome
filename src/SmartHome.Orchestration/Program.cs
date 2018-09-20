using System;
using System.Collections.Generic;
using SmartHome.Lib;
using SmartHome.Lib.Adapters;
using SmartHome.Lib.Environment;
using SmartHome.Lib.Hue;
using SmartHome.Lib.Lights;

namespace SmartHome.Orchestration
{
    class Program
    {
        static void Main(string[] args)
        {
            var entryRule = new Rule
            {
                Id = "1",
                Name = "Entry Lights On",
                Conditions = new[]
                {
                    new RuleCondition
                    {
                        Type = RuleConditionType.TimeRange,
                        TimeRange = new RuleTimeRange
                        {
                            Start = 0,
                            End = 19
                        }
                    },
                    new RuleCondition
                    {
                        Type = RuleConditionType.MotionDetection,
                        RoomId = "1"
                    },
                    new RuleCondition
                    {
                        Type = RuleConditionType.LightState,
                        RoomId = "1",
                        LightState = LightState.LightsOff
                    }
                },
                Actions = new[]
                {
                    new RuleAction
                    {
                        RoomIds = new[] {"1", "2"},
                        LightState = LightState.LightsOn
                    }
                }
            };

            var laundryOn = new Rule
            {
                Id = "1",
                Name = "Laundry Light On",
                Conditions = new[]
                {
                    new RuleCondition
                    {
                        Type = RuleConditionType.MotionDetection,
                        RoomId = "4"
                    },
                    new RuleCondition
                    {
                        Type = RuleConditionType.LightState,
                        RoomId = "4",
                        LightState = LightState.LightsOff
                    }
                },
                Actions = new[]
                {
                    new RuleAction
                    {
                        RoomIds = new[] {"4"},
                        LightState = LightState.LightsOn
                    }
                }
            };

            var laundryOff = new Rule
            {
                Id = "1",
                Name = "Laundry Light Off",
                Conditions = new[]
                {
                    new RuleCondition
                    {
                        Type = RuleConditionType.NoMotionDetection,
                        DurationInMins = 1,
                        RoomId = "4"
                    },
                    new RuleCondition
                    {
                        Type = RuleConditionType.LightState,
                        LightState = LightState.LightsOn,
                        RoomId = "4"
                    }
                },
                Actions = new[]
                {
                    new RuleAction
                    {
                        RoomIds = new[] {"4"},
                        LightState = LightState.LightsOff
                    }
                }
            };

            var bedroomRule = new Rule
            {
                Id = "1",
                Name = "Bedroom Light On",
                Conditions = new[]
                {
                    new RuleCondition
                    {
                        Type = RuleConditionType.MotionDetection,
                        RoomId = "3"
                    },
                    new RuleCondition
                    {
                        Type = RuleConditionType.LightState,
                        RoomId = "3",
                        LightState = LightState.LightsOff
                    }
                },
                Actions = new[]
                {
                    new RuleAction
                    {
                        RoomIds = new[] {"3"},
                        LightState = LightState.LightsOn
                    }
                }
            };

            var smartHub = new SmartHub
            {
                Environment = EnvironmentConfig.Load(),
                LightAdapters = new List<ILightAdapter>
                {
                    new HueLightAdapter()
                },
                SensorAdapters = new List<ISensorAdapter>
                {
                    new HueSensorAdapater()
                }
            };

            Console.WriteLine("Initializing smarthub");

            smartHub.Initialize();

            Console.WriteLine("Starting rule engine loop");

            var ruleProcessor = new RuleProcessor(new[] { entryRule, laundryOn, laundryOff, bedroomRule }, smartHub);
            ruleProcessor.Run();

            Console.ReadLine();

            ruleProcessor.Stop();
        }
    }
}
