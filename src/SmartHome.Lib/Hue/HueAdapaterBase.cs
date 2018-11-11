using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace SmartHome.Lib.Hue
{
    public abstract class HueAdapaterBase
    {
        protected string BridgeIp { get { return _settings.BridgeIp; } }
        protected string BridgeUser { get { return _settings.BridgeUser; } }

        private readonly AdapterSettings _settings;

        protected HueAdapaterBase()
        {
            string path = "hueAdapter.json";

            if (!File.Exists(path))
            {
                path = "../../../../config/hueAdapter.json";

                if (!File.Exists(path))
                {
                    throw new Exception("Could not find adapter settings!");
                }
            }

            string adapterJson = File.ReadAllText(path);

            _settings = JsonConvert.DeserializeObject<AdapterSettings>(adapterJson);
        }

        protected static string GetJson(string url)
        {
            using (var client = new WebClient())
            {
                return client.DownloadString(url);
            }
        }

        internal class AdapterSettings
        {
            public string BridgeIp { get; set; }

            public string BridgeUser { get; set; }

            public Dictionary<string, string> LightMap { get; set; }
        }
    }
}
