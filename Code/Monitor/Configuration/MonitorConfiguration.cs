namespace PoolAlerter.Code.Monitor.Configuration
{
    public record MonitorConfiguration
    {
        public uint TimeBetweenChecksSeconds { get; init; }
        
        public uint TimeBetweenHeartbeatsSeconds { get; init; }
        
        public string StartHourUtc { get; init; }
        
        public string EndHourUtc { get; init; }
    }
}