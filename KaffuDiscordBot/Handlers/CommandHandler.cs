using System.Reflection;
using Discord.Commands;
using Discord.WebSocket;
using KaffuDiscordBot.Models;

namespace KaffuDiscordBot.Handlers;

internal class CommandHandler(
	DiscordSocketClient discordSocketClient,
	CommandService commandService,
	Configuration configuration
)
{
	internal async Task InitializeAsync()
	{
		discordSocketClient.MessageReceived += this.HandleCommandAsync;

		await commandService.AddModulesAsync(Assembly.GetEntryAssembly(), null);
	}

	private async Task HandleCommandAsync(SocketMessage messageParam)
	{
		if (messageParam is not SocketUserMessage message)
			return;

		int argumentPosition = 0;

		if (
			!(
				message.HasStringPrefix(configuration.Prefix, ref argumentPosition)
				|| message.HasMentionPrefix(discordSocketClient.CurrentUser, ref argumentPosition)
			) || message.Author.IsBot
		)
		{
			return;
		}

		SocketCommandContext socketCommandContext = new SocketCommandContext(discordSocketClient, message);

		await commandService.ExecuteAsync(socketCommandContext, argumentPosition, null);
	}
}
