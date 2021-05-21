﻿using System;
using Discord;

namespace PoolAlerter.Code.Discord.Configuration
{
    public record DiscordConfiguration
    {
        public string Token { get; init; }
        
        public string LogSeverity { get; init; }
        
        public ulong ServerId { get; init; }
        
        public DiscordChannelsConfiguration Channels { get; init; }
        
        public LogSeverity ParseLogSeverity()
        {
            var isLogSeverityValid = Enum.TryParse(typeof(LogSeverity), LogSeverity, out var logSeverity);
            return isLogSeverityValid && logSeverity != null
                ? (LogSeverity) logSeverity
                : throw new InvalidOperationException($"Invalid Log Severity ${LogSeverity}");
        }
    }

    public record DiscordChannelsConfiguration
    {
        public ulong NotificationChannelId { get; init; }
    }
}