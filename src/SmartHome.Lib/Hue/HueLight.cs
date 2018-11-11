using System;
using SmartHome.Lib.Lights;

namespace SmartHome.Lib.Hue
{
    public class HueLight : ISmartLight
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public DateTime LastUpdate { get; set; }

        public LightType Type { get; set; }
        public LightState State { get; set; }
    }
}
