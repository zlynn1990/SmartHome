namespace SmartHome.Lib.Sensors
{
    public interface ISmartSensor : ISmartDevice
    {
        bool DetectMotion { get; set; }

        SensorConfig Config { get; set; }
    }
}
