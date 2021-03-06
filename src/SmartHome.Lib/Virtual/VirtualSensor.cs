﻿using System;
using SmartHome.Lib.Sensors;

namespace SmartHome.Lib.Virtual
{
    public class VirtualSensor : ISmartSensor
    {
        public string Id { get; set; }

        public string Name { get; set; }
        public string Type { get; set; }

        public DateTime LastUpdate { get; set; }

        public bool DetectMotion { get; set; }
        public SensorConfig Config { get; set; }
    }
}
