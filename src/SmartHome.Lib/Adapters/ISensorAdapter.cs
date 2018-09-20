using System.Collections.Generic;
using SmartHome.Lib.Sensors;

namespace SmartHome.Lib.Adapters
{
    public interface ISensorAdapter
    {
        IEnumerable<ISmartSensor> PollSensors();
    }
}
