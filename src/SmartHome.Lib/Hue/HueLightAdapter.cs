using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using SmartHome.Lib.Adapters;
using SmartHome.Lib.Lights;

namespace SmartHome.Lib.Hue
{
    public class HueLightAdapter : HueAdapaterBase, ILightAdapter
    {
        private readonly Dictionary<string, string> _lightMap;

        public HueLightAdapter()
        {
            _lightMap = new Dictionary<string, string>();
        }

        public IEnumerable<ISmartLight> PollLights()
        {
            var lights = new List<ISmartLight>();

            string lightJson = GetJson($"http://{BridgeIp}/api/{BridgeUser}/lights");

            var hueLights = JsonConvert.DeserializeObject<Dictionary<string, LightJson>>(lightJson);

            foreach (string lightId in hueLights.Keys)
            {
                LightJson hueLight = hueLights[lightId];

                // Add the current light to the response
                lights.Add(new HueLight
                {
                    Id = hueLight.Id,
                    Name = hueLight.Name,
                    LastUpdate = DateTime.UtcNow,
                    Type = GetStyle(hueLight.Type),
                    State = new LightState
                    {
                        On = hueLight.State.On,
                        Brightness = hueLight.State.Brightness,
                        Hue = hueLight.State.Hue,
                        Saturation = hueLight.State.Saturation
                    }
                });

                // Map the light for future lookups
                if (!_lightMap.ContainsKey(hueLight.Id))
                {
                    _lightMap.Add(hueLight.Id, lightId);
                }
            }

            return lights;
        }

        public void UpdateLight(string lightId, LightState state)
        {
            using (var client = new WebClient())
            {
                string lightUrl = $"http://{BridgeIp}/api/{BridgeUser}/lights/{_lightMap[lightId]}/state";

                string lightJson = JsonConvert.SerializeObject(Convert(state));

                client.UploadString(new Uri(lightUrl), "PUT", lightJson);
            }
        }

        public void UpdateRoom(string roomId, LightState state)
        {
            using (var client = new WebClient())
            {
                string lightUrl = $"http://{BridgeIp}/api/{BridgeUser}/groups/{roomId}/action";

                string lightJson = JsonConvert.SerializeObject(Convert(state));

                client.UploadString(new Uri(lightUrl), "PUT", lightJson);
            }
        }

        private static StateJson Convert(LightState state)
        {
            return new StateJson
            {
                On = state.On,
                Brightness = state.Brightness,
                Hue = state.Hue,
                Saturation = state.Saturation,
                Xy = state.Xy,
                TransitionTime = state.TransitionTime
            };
        }

        private static LightType GetStyle(string lightType)
        {
            if (lightType.Contains("color"))
            {
                return LightType.Color;
            }

            return LightType.White;
        }

        internal class LightJson
        {
            [JsonProperty("uniqueid")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("state")]
            public StateJson State { get; set; }
        }

        internal class StateJson
        {
            [JsonProperty("on")]
            public bool On { get; set; }

            [JsonProperty("bri")]
            public int Brightness { get; set; }

            [JsonProperty("hue")]
            public int Hue { get; set; }

            [JsonProperty("sat")]
            public int Saturation { get; set; }

            [JsonProperty("xy")]
            public float[] Xy { get; set; }

            [JsonProperty("transitiontime")]
            public int TransitionTime { get; set; }
        }
    }
}
