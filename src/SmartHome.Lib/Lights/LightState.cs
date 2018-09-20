namespace SmartHome.Lib.Lights
{
    public class LightState
    {
        public bool On { get; set; }

        public int Hue { get; set; }
        public int Saturation { get; set; }
        public int Brightness { get; set; }

        public float[] Xy { get; set; }

        public int TransitionTime { get; set; }

        public static LightState LightsOff = new LightState
        {
            On = false,
            Brightness = 0,
            Hue = 14948,
            Xy = new []{ 0.4573f,  0.41f },
            Saturation = 143, TransitionTime = 4
        };

        public static LightState LightsOn = new LightState
        {
            On = true,
            Brightness = 255,
            Hue = 14948,
            Saturation = 143,
            Xy = new[] { 0.4573f, 0.41f },
            TransitionTime = 4
        };
    }
}
