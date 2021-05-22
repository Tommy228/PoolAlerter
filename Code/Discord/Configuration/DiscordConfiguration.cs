using System;
using System.Collections.Generic;
using Discord;

namespace PoolAlerter.Code.Discord.Configuration
{
    public record DiscordConfiguration
    {
        public string Token { get; init; }
        
        public string LogSeverity { get; init; }
        
        public ulong ServerId { get; init; }
        
        public string TimeZone { get; init; }
        
        public DiscordChannelsConfiguration Channels { get; init; }

        public ICollection<ulong> UserIdsToWarn { get; init; }
        
        public LogSeverity ParseLogSeverity()
        {
            var isLogSeverityValid = Enum.TryParse(typeof(LogSeverity), LogSeverity, out var logSeverity);
            return isLogSeverityValid
                ? (LogSeverity) logSeverity!
                : throw new InvalidOperationException($"Invalid Log Severity ${LogSeverity}");
        }
    }

    public record DiscordChannelsConfiguration
    {
        public ulong NotificationChannelId { get; init; }
        
        public ulong ErrorsChannelId { get; init; }
        
        public ulong HeartbeatsChannelId { get; init; }
    }
}