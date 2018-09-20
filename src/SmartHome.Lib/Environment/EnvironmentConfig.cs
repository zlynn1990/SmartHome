using System;
using System.IO;
using Newtonsoft.Json;

namespace SmartHome.Lib.Environment
{
    public class EnvironmentConfig
    {
        public Room[] Rooms { get; set; }

        public void Save(string path)
        {
            string environmentJson = JsonConvert.SerializeObject(this);

            File.WriteAllText(path, environmentJson);
        }

        public static EnvironmentConfig Load()
        {
            string path = "smarthome.json";

            if (!File.Exists(path))
            {
                path = "../../../../config/smarthome.json";

                if (!File.Exists(path))
                {
                    throw new Exception("Could not find environment config path!");
                }
            }

            string environmentJson = File.ReadAllText(path);

            return JsonConvert.DeserializeObject<EnvironmentConfig>(environmentJson);
        }
    }
}
