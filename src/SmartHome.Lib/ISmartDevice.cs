﻿using System;

namespace SmartHome.Lib
{
    public interface ISmartDevice
    {
        string Id { get; set; }

        string Name { get; set; }

        DateTime LastUpdate { get; set; }
    }
}
