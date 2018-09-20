using SmartHome.Lib.Lights;

namespace SmartHome.Orchestration
{
    public class RuleAction
    {
        public string[] RoomIds { get; set; }

        public LightState LightState { get; set; }
    }
}
