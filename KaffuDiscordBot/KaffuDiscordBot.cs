using System.Text.Json;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using KaffuDiscordBot.Handlers;
using KaffuDiscordBot.Logging;
using KaffuDiscordBot.Models;
using Microsoft.Extensions.DependencyInjection;

namespace KaffuDiscordBot;

internal abstract class KaffuDiscordBot
{
	private static readonly Configuration Configuration =
		JsonSerializer.Deserialize<Configuration>(File.ReadAllText("Configuration.json"))
		?? throw new NullReferenceException();

	private static DiscordSocketConfig DiscordSocketConfig
		=> new DiscordSocketConfig { GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent };

	private static CommandServiceConfig CommandServiceConfig => new CommandServiceConfig();

	private static InteractionServiceConfig InteractionServiceConfig => new InteractionServiceConfig();

	private static IServiceProvider serviceProvider = null!;

	internal static async Task Main()
	{
		// Register services
		KaffuDiscordBot.serviceProvider = new ServiceCollection()
			.AddSingleton(KaffuDiscordBot.Configuration)
			.AddSingleton<Logger>()
			.AddSingleton(KaffuDiscordBot.DiscordSocketConfig)
			.AddSingleton<DiscordSocketClient>()
			.AddSingleton(KaffuDiscordBot.CommandServiceConfig)
			.AddSingleton<CommandService>()
			.AddSingleton<CommandHandler>()
			.AddSingleton(KaffuDiscordBot.InteractionServiceConfig)
			.AddSingleton<InteractionService>(innerServiceProvider => new InteractionService(
				innerServiceProvider.GetRequiredService<DiscordSocketClient>(),
				KaffuDiscordBot.InteractionServiceConfig
			))
			.AddSingleton<InteractionHandler>()
			.BuildServiceProvider();

		// Get services
		DiscordSocketClient discordSocketClient =
			KaffuDiscordBot.serviceProvider.GetRequiredService<DiscordSocketClient>();

		CommandService commandService = KaffuDiscordBot.serviceProvider.GetRequiredService<CommandService>();

		CommandHandler commandHandler = KaffuDiscordBot.serviceProvider.GetRequiredService<CommandHandler>();

		InteractionService interactionService =
			KaffuDiscordBot.serviceProvider.GetRequiredService<InteractionService>();

		InteractionHandler interactionHandler =
			KaffuDiscordBot.serviceProvider.GetRequiredService<InteractionHandler>();

		// Configure logging
		discordSocketClient.Log += Logger.LogAsync;
		commandService.Log += Logger.LogAsync;
		interactionService.Log += Logger.LogAsync;

		// Start bot
		await discordSocketClient.LoginAsync(TokenType.Bot, KaffuDiscordBot.Configuration.Token);
		await discordSocketClient.StartAsync();

		// Register commands and interactions
		commandHandler.InitializeAsync();
		interactionHandler.InitializeAsync();

		// Keep bot running
		await Task.Delay(Timeout.Infinite);
	}
}
