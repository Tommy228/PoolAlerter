using FluentResults;

namespace PoolAlerter.Code._1337.PoolCheck
{
    internal interface IPoolAvailabilityChecker
    {
        public (Result<bool> Result, PoolAvailabilityResultContext Context) CheckPoolAvailabilityAsync();

        public bool IsCheckInProgress { get; }
    }
}