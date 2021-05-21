namespace PoolAlerter.Code.Monitor.Configuration
{
    public record MonitorConfiguration
    {
        public uint TimeBetweenChecksSeconds { get; init; }
    }
}