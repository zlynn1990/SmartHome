using System.Collections.Generic;
using System.Threading;
using SmartHome.Lib;
using SmartHome.Lib.Adapters;
using SmartHome.Lib.Environment;
using SmartHome.Lib.Hue;
using SmartHome.Lib.Lights;
using SmartHome.Lib.Sensors;

namespace SmartHome.Console
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

            smartHub.Initialize();

            IList<ISmartLight> lights = smartHub.PollLights();

            IList<ISmartSensor> sensors = smartHub.PollSensors();

            foreach (ISmartLight light in lights)
            {
                light.State.On = false;
                light.State.Brightness = 199;

                smartHub.UpdateLight(light);
            }

            Thread.Sleep(5000);

            var lightState = new LightState
            {
                On = true,
                Brightness = 255,
                Saturation = 255,
                Hue = 0
            };

            smartHub.UpdateLightsInRooms(new []{ smartHub.Environment.Rooms[0].Id }, lightState);

            System.Console.ReadLine();
        }
    }
}
