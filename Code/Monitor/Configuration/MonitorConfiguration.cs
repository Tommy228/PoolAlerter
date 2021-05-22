namespace PoolAlerter.Code.Monitor.Configuration
{
    public record MonitorConfiguration
    {
        public uint TimeBetweenChecksSeconds { get; init; }
        
        public uint TimeBetweenHeartbeatsSeconds { get; init; }
        
        public uint StartHourUtc { get; init; }
        
        public uint EndHourUtc { get; init; }
    }
}