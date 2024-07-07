using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KaffuDiscordBot.CommandModules;
using KaffuDiscordBot.Logging;
using KaffuDiscordBot.Models;

namespace KaffuDiscordBot.Handlers;

internal class CommandHandler(
	DiscordSocketClient discordSocketClient,
	CommandService commandService,
	Configuration configuration
)
{
	private static readonly ModuleBase<SocketCommandContext>[] CommandModules = new ModuleBase<SocketCommandContext>[]
	{
		new GeneralCommandModule()
	};

	internal void InitializeAsync()
	{
		discordSocketClient.MessageReceived += this.HandleCommandAsync;
		discordSocketClient.Ready += this.RegisterCommandsAsync;
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

	private async Task RegisterCommandsAsync()
	{
		await Logger.LogAsync(new LogMessage(LogSeverity.Info, "CommandHandler.cs", "Registering command modules..."));

		foreach (ModuleBase<SocketCommandContext> module in CommandHandler.CommandModules)
		{
			await Logger.LogAsync(
				new LogMessage(
					LogSeverity.Info,
					"CommandHandler.cs",
					$"Registering command module: {module.GetType().Name}..."
				)
			);
			await commandService.AddModuleAsync(module.GetType(), null);

			await Logger.LogAsync(
				new LogMessage(
					LogSeverity.Info,
					"CommandHandler.cs",
					$"Registered command module: {module.GetType().Name}..."
				)
			);
		}

		await Logger.LogAsync(new LogMessage(LogSeverity.Info, "CommandHandler.cs", "Registered all command modules."));
	}
}
