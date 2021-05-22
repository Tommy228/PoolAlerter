using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using NodaTime;
using PoolAlerter.Code._1337.PoolCheck;
using PoolAlerter.Code.Discord.Channels;
using PoolAlerter.Code.Discord.Configuration;
using PoolAlerter.Code.Discord.Notifications.Screenshot;

namespace PoolAlerter.Code.Discord.Notifications
{
    internal class DiscordNotifier : IDiscordNotifier
    {
        private readonly DiscordConfiguration _discordConfiguration;
        private readonly ILogger<DiscordNotifier> _logger;
        private readonly IScreenshotConverter _screenshotConverter;
        private readonly IDiscordChannelsHolder _discordChannelsHolder;

        private readonly IClock _clock;

        public DiscordNotifier(
            DiscordConfiguration discordConfiguration,
            ILogger<DiscordNotifier> logger,
            IClock clock,
            IScreenshotConverter screenshotConverter,
            IDiscordChannelsHolder discordChannelsHolder
        )
        {
            _discordConfiguration = discordConfiguration;
            _logger = logger;
            _clock = clock;
            _screenshotConverter = screenshotConverter;
            _discordChannelsHolder = discordChannelsHolder;
        }

        public async Task NotifyPoolAvailabilityAsync(bool isAvailable, PoolAvailabilityResultContext context)
        {
            var channel = _discordChannelsHolder.NotificationsChannel;
            if (!channel.HasValue)
            {
                _logger.LogError("Could not find the notifications channel");
                return;
            }

            var image = _screenshotConverter.ConvertToJpeg(context.Screenshot);

            var message = isAvailable
                ? GetPositiveAvailabilityMessage()
                : GetNegativeAvailabilityMessage();

            await channel.Value.SendFileAsync(image, "screenshot.jpg", message);
        }

        /// <inheritdoc/>
        public async Task NotifyErrorsAsync(IEnumerable<string> errors, PoolAvailabilityResultContext context)
        {
            errors ??= new List<string>();

            var channel = _discordChannelsHolder.ErrorsChannel;
            if (!channel.HasValue)
            {
                _logger.LogError("Could not find the errors channel");
                return;
            }

            var image = _screenshotConverter.ConvertToJpeg(context.Screenshot);

            var message = new StringBuilder()
                .Append($"Pool check failed at {FormatCurrentInstant()} due to errors\n")
                .Append(string.Join('\n', errors.Select(e => $"> {e}")))
                .ToString();

            await channel.Value.SendFileAsync(image, "screenshot.jpg", message);
        }

        public async Task SendHeartbeat()
        {
            var channel = _discordChannelsHolder.HeartbeatsChannel;
            if (!channel.HasValue)
            {
                _logger.LogError("Could not find the heartbeat channel");
                return;
            }

            await channel.Value.SendMessageAsync("I'm alive!");
        }

        private string GetPositiveAvailabilityMessage() =>
            new StringBuilder().ToString();

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