using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NodaTime;
using PoolAlerter.Code._1337.PoolCheck;
using PoolAlerter.Code.Discord.Notifications;
using PoolAlerter.Code.Monitor.Configuration;

namespace PoolAlerter.Code.Monitor
{
    internal class PoolMonitor : IHostedService
    {
        private readonly IPoolAvailabilityChecker _poolAvailabilityChecker;
        private readonly IDiscordNotifier _discordNotifier;
        private readonly ILogger<PoolMonitor> _logger;
        private readonly MonitorConfiguration _monitorConfiguration;
        private readonly IClock _clock;
        private readonly Timer _timer;

        private TimeSpan TimeBetweenChecks =>
            TimeSpan.FromSeconds(_monitorConfiguration.TimeBetweenChecksSeconds);

        public PoolMonitor(
            IPoolAvailabilityChecker poolAvailabilityChecker,
            ILogger<PoolMonitor> logger,
            IDiscordNotifier discordNotifier,
            MonitorConfiguration monitorConfiguration,
            IClock clock)
        {
            _poolAvailabilityChecker = poolAvailabilityChecker;
            _logger = logger;
            _discordNotifier = discordNotifier;
            _monitorConfiguration = monitorConfiguration;
            _clock = clock;
#pragma warning disable 4014
            _timer = new Timer(_ => MonitorPool(), null, Timeout.Infinite, Timeout.Infinite);
#pragma warning restore 4014
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await MonitorPool();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _timer.Change(Timeout.Infinite, 0);
            await _timer.DisposeAsync();
        }

        private async Task MonitorPool()
        {
            void Next() => _timer?.Change(TimeBetweenChecks, TimeSpan.Zero);

            _logger.LogInformation("Starting pool availability check");

            if (!ShouldMonitor())
            {
                _logger.LogInformation("Outside of monitoring range, aborting");
                Next();
                return;
            }

            if (_poolAvailabilityChecker.IsCheckInProgress)
            {
                _logger.LogInformation("Check already in progress, aborting");
                Next();
                return;
            }

            var (isPoolAvailable, context) = _poolAvailabilityChecker.CheckPoolAvailabilityAsync();
            if (isPoolAvailable.IsSuccess)
            {
                _logger.LogInformation("Pool availability check finished with result {Result}", isPoolAvailable);
                await _discordNotifier.NotifyPoolAvailabilityAsync(isPoolAvailable.Value, context);
            }
            else
            {
                _logger.LogError(
                    "Pool availability check failed with reasons {Reasons}",
                    string.Join(",", isPoolAvailable.Errors)
                );

                await _discordNotifier.NotifyErrorsAsync(
                    isPoolAvailable.Errors.Select(x => x.Message).ToList(),
                    context
                );
            }

            Next();
        }

        private bool ShouldMonitor()
        {
            var currentTime = _clock.GetCurrentInstant().InUtc();
            
            var startHour = _monitorConfiguration.StartHourUtc;
            var endHour = _monitorConfiguration.EndHourUtc;

            var startDate = new LocalDateTime(
                currentTime.Year,
                currentTime.Month,
                currentTime.Day,
                (int) startHour,
                0,
                0
            ).InUtc();

            var duration = startHour > endHour
                ? 24 - startHour + endHour
                : endHour - startHour;

            var endDate = startDate.PlusHours((int) duration);

            return currentTime.Date >= startDate.Date && currentTime.Date <= endDate.Date;
        }
    }
}