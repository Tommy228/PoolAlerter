using System;
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

        public PoolMonitor(
            IPoolAvailabilityChecker poolAvailabilityChecker,
            ILogger<PoolMonitor> logger,
            IDiscordNotifier discordNotifier,
            MonitorConfiguration monitorConfiguration)
        {
            _poolAvailabilityChecker = poolAvailabilityChecker ??
                                       throw new ArgumentNullException(nameof(poolAvailabilityChecker));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _discordNotifier = discordNotifier ?? throw new ArgumentNullException(nameof(discordNotifier));
            _monitorConfiguration =
                monitorConfiguration ?? throw new ArgumentNullException(nameof(monitorConfiguration));
            _timer = new Timer(MonitorPool, null, Timeout.Infinite, Timeout.Infinite);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Next();
            return Task.CompletedTask;
        }

        private void Next()
        {
            _timer?.Change(TimeSpan.FromSeconds(_monitorConfiguration.TimeBetweenChecksSeconds), TimeSpan.Zero);
        }

        private async void MonitorPool(object state)
        {
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