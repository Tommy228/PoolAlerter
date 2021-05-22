using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
        private readonly Timer _timer;

        private TimeSpan TimeBetweenChecks =>
            TimeSpan.FromSeconds(_monitorConfiguration.TimeBetweenChecksSeconds);

        public PoolMonitor(
            IPoolAvailabilityChecker poolAvailabilityChecker,
            ILogger<PoolMonitor> logger,
            IDiscordNotifier discordNotifier,
            MonitorConfiguration monitorConfiguration)
        {
            _poolAvailabilityChecker = poolAvailabilityChecker;
            _logger = logger;
            _discordNotifier = discordNotifier;
            _monitorConfiguration = monitorConfiguration;
            #pragma warning disable 4014
            _timer = new Timer(_ => MonitorPool(_), null, Timeout.Infinite, Timeout.Infinite);
            #pragma warning restore 4014
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await MonitorPool(null);
        }
        
        private async Task MonitorPool(object state)
        {
            void Next() => _timer?.Change(TimeBetweenChecks, TimeSpan.Zero);
            
            _logger.LogInformation("Starting pool availability check");

            if (_poolAvailabilityChecker.IsCheckInProgress)
            {
                _logger.LogInformation("Check already in progress, aborting");
                Next();
                return;
            }

            var isPoolAvailable = _poolAvailabilityChecker.CheckPoolAvailabilityAsync();
            if (isPoolAvailable.IsSuccess)
            {
                _logger.LogInformation("Pool availability check finished with result {Result}", isPoolAvailable);
                await _discordNotifier.NotifyPoolAvailabilityAsync(isPoolAvailable.Value);
            }
            else
            {
                _logger.LogError(
                    "Pool availability check failed with reasons {Reasons}",
                    string.Join(",", isPoolAvailable.Errors)
                );

                await _discordNotifier.NotifyErrorsAsync(
                    isPoolAvailable.Errors.Select(x => x.Message).ToList()
                );
            }

            Next();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_timer != null)
            {
                _timer.Change(Timeout.Infinite, 0);
                await _timer.DisposeAsync();
            }
        }
    }
}