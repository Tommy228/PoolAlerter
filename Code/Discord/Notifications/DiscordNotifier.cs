using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using NodaTime;
using PoolAlerter.Code.Discord.Configuration;

namespace PoolAlerter.Code.Discord.Notifications
{
    internal class DiscordNotifier : IDiscordNotifier
    {
        private readonly DiscordSocketClient _discordSocketClient;
        private readonly DiscordConfiguration _discordConfiguration;
        private readonly ILogger<DiscordNotifier> _logger;
        private readonly IClock _clock;

        private SocketGuild Guild => _discordSocketClient.GetGuild(_discordConfiguration.ServerId);

        public DiscordNotifier(
            DiscordSocketClient discordSocketClient,
            DiscordConfiguration discordConfiguration,
            ILogger<DiscordNotifier> logger,
            IClock clock)
        {
            _discordSocketClient = discordSocketClient ?? throw new ArgumentNullException(nameof(discordSocketClient));
            _discordConfiguration =
                discordConfiguration ?? throw new ArgumentNullException(nameof(discordConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public async Task NotifyPoolAvailabilityAsync(bool isAvailable)
        {
            var channel = Guild?.GetTextChannel(_discordConfiguration.Channels.NotificationChannelId);
            if (channel == null)
            {
                _logger.LogError(
                    "Could not find a channel with serverId {ServerId} and channelId {Channel}",
                    _discordConfiguration.ServerId,
                    _discordConfiguration.Channels.NotificationChannelId
                );
                return;
            }

            await channel.SendMessageAsync(
                isAvailable
                    ? GetPositiveAvailabilityMessage()
                    : GetNegativeAvailabilityMessage()
            );
        }

        /// <inheritdoc/>
        public async Task NotifyErrorsAsync([NotNull] IEnumerable<string> errors)
        {
            if (errors == null) throw new ArgumentNullException(nameof(errors));

            var channel = Guild?.GetTextChannel(_discordConfiguration.Channels.ErrorsChannelId);
            if (channel == null)
            {
                _logger.LogError(
                    "Could not find a channel with serverId {ServerId} and channelId {Channel}",
                    _discordConfiguration.ServerId,
                    _discordConfiguration.Channels.ErrorsChannelId
                );
                return;
            }

            var message = new StringBuilder()
                .Append($"Pool check failed at {FormatCurrentInstant()} due to errors\n")
                .Append(string.Join('\n', errors.Select(e => $"> {e}")))
                .ToString();

            await channel.SendMessageAsync(message);
        }

        private string GetPositiveAvailabilityMessage() => new StringBuilder().ToString();

        private string GetNegativeAvailabilityMessage() =>
            new StringBuilder()
                .Append("**[Not available]** Pool check finished at ")
                .Append(FormatCurrentInstant())
                .ToString();

        private string FormatCurrentInstant()
        {
            var currentInstant = this._clock.GetCurrentInstant();
            var timeZoneName = _discordConfiguration.TimeZone;
            var timeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZoneName) ?? DateTimeZone.Utc;
            var dateTimeZone = currentInstant.InZone(timeZone, CalendarSystem.Gregorian);
            var formattedDate = dateTimeZone.ToString("HH:mm", null);
            return $"{formattedDate} ({timeZone})";
        }
    }
}