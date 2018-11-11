namespace SmartHome.Lib.Lights
{
    public interface ISmartLight : ISmartDevice
    {
        LightType Type { get; set; }

        LightState State { get; set; }
    }
}
