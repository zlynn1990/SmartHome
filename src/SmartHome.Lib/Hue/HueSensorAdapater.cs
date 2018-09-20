using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SmartHome.Lib.Adapters;
using SmartHome.Lib.Sensors;

namespace SmartHome.Lib.Hue
{
    public class HueSensorAdapater : HueAdapaterBase, ISensorAdapter
    {
        private readonly Dictionary<string, string> _sensorMap;

        public HueSensorAdapater()
        {
            _sensorMap = new Dictionary<string, string>();
        }

        public IEnumerable<ISmartSensor> PollSensors()
        {
            var sensors = new List<ISmartSensor>();

            string sensorJson = GetJson($"http://{BridgeIp}/api/{BridgeUser}/sensors");

            var hueSensors = JsonConvert.DeserializeObject<Dictionary<string, SensorJson>>(sensorJson);

            foreach (string sensorId in hueSensors.Keys)
            {
                SensorJson hueSensor = hueSensors[sensorId];

                // Only include presence sensors for now
                if (!string.IsNullOrEmpty(hueSensor.Type) && hueSensor.Type.Contains("Presence"))
                {
                    sensors.Add(new HueSensor
                    {
                        Id = hueSensor.Id,
                        Name = hueSensor.Name,
                        Type = hueSensor.Type,
                        DetectMotion = hueSensor.State.Presence,
                        LastUpdate = DateTime.Parse(hueSensor.State.LastUpdated),
                        Config = hueSensor.Config
                    });
                }
            }

            return sensors;
        }

        internal class SensorJson
        {
            [JsonProperty("uniqueid")]
            public string Id { get; set; }

            public string Name { get; set; }

            public string Type { get; set; }

            public SensorState State { get; set; }

            public SensorConfig Config { get; set; }
        }
    }
}
