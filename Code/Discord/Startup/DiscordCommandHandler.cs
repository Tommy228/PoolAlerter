using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;

namespace PoolAlerter.Code.Discord.Startup
{
    public class DiscordCommandHandler : IHostedService
    {
        private readonly DiscordSocketClient _discordSocketClient;
        private readonly CommandService _commands;
        private readonly IServiceProvider _serviceProvider;

        public DiscordCommandHandler(
            DiscordSocketClient discordSocketClient,
            CommandService commands,
            IServiceProvider serviceProvider)
        {
            _discordSocketClient = discordSocketClient;
            _commands = commands;
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _discordSocketClient.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), _serviceProvider);
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            if (messageParam is not SocketUserMessage message || !IsCommand(message, out var argPos)) return;
            var context = new SocketCommandContext(_discordSocketClient, message);
            await _commands.ExecuteAsync(context, argPos, _serviceProvider);
        }

        private bool IsCommand(SocketUserMessage message, out int argPos)
        {
            argPos = 0;
            return (
                       message.HasCharPrefix('!', ref argPos) ||
                       message.HasMentionPrefix(_discordSocketClient.CurrentUser, ref argPos)
                   ) &&
                   !message.Author.IsBot;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}