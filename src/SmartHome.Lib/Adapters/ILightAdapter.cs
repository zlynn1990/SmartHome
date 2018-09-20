using System.Collections.Generic;
using SmartHome.Lib.Lights;

namespace SmartHome.Lib.Adapters
{
    public interface ILightAdapter
    {
        IEnumerable<ISmartLight> PollLights();

        void UpdateLight(string lightId, LightState state);
        void UpdateRoom(string roomId, LightState state);
    }
}
