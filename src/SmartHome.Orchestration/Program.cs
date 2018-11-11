using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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

            HomeLogger.WriteLine("Loading rules");

            string path = "rules.json";

            if (!File.Exists(path))
            {
                path = "../../../../config/rules.json";

                if (!File.Exists(path))
                {
                    throw new Exception("Could not find rules!");
                }
            }

            var rules = JsonConvert.DeserializeObject<Rule[]>(File.ReadAllText(path));

            HomeLogger.WriteLine("Initializing smarthub");

            smartHub.Initialize();

            HomeLogger.WriteLine("Starting rule engine loop");

            var ruleProcessor = new RuleProcessor(rules, smartHub);
            ruleProcessor.Run();

            Console.ReadLine();

            ruleProcessor.Stop();
        }
    }
}
