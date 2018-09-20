namespace SmartHome.Lib.Lights
{
    public interface ISmartLight : ISmartDevice
    {
        LightState State { get; set; }
    }
}
