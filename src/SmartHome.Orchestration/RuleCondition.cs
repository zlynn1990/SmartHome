using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SmartHome.Lib.Lights;

namespace SmartHome.Orchestration
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RuleConditionType
    {
        TimeRange,
        LightState,
        MotionDetection,
        NoMotionDetection
    }

    public class RuleTimeRange
    {
        public int Start { get; set; }

        public int End { get; set; }
    }

    public class RuleCondition
    {
        public RuleConditionType Type { get; set; }

        public RuleTimeRange TimeRange { get; set; }

        public int DurationInMins { get; set; }

        public string RoomId { get; set; }

        public LightState LightState { get; set; }
    }
}
