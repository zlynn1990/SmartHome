using System.Linq;

namespace SmartHome.Lib.Environment
{
    public class Room
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string[] Lights { get; set; }

        public string[] Sensors { get; set; }

        public bool ContainsLight(string lightId)
        {
            if (Lights == null) return false;

            return Lights.Any(l => l.Equals(lightId));
        }

        public bool ContainsSensor(string sensorId)
        {
            if (Sensors == null) return false;

            return Sensors.Any(s => s.Equals(sensorId));
        }
    }
}
