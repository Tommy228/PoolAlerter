using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using PoolAlerter.Code.Discord.Notifications;
using PoolAlerter.Code.Monitor.Configuration;

namespace PoolAlerter.Code.Monitor
{
    internal class HeartbeatNotifier : IHostedService
    {
        private readonly IDiscordNotifier _discordNotifier;
        private readonly Timer _timer;
        private readonly MonitorConfiguration _monitorConfiguration;

        private TimeSpan TimeBetweenHeartbeats =>
            TimeSpan.FromSeconds(_monitorConfiguration.TimeBetweenHeartbeatsSeconds);

        public HeartbeatNotifier(IDiscordNotifier discordNotifier, MonitorConfiguration monitorConfiguration)
        {
            _discordNotifier = discordNotifier;
            _monitorConfiguration = monitorConfiguration;
            #pragma warning disable 4014
            _timer = new Timer(_ => SendHeartbeat(), null, Timeout.Infinite, Timeout.Infinite);
            #pragma warning restore 4014
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer.Change(TimeSpan.Zero, TimeBetweenHeartbeats);
            return Task.CompletedTask;
        }

        private async Task SendHeartbeat()
        {
            await _discordNotifier.SendHeartbeat();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            await _timer.DisposeAsync();
        }
    }
}