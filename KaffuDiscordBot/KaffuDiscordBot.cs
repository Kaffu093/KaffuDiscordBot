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

		DiscordSocketClient discordSocketClient =
			KaffuDiscordBot.serviceProvider.GetRequiredService<DiscordSocketClient>();

		CommandService commandService = KaffuDiscordBot.serviceProvider.GetRequiredService<CommandService>();

		CommandHandler commandHandler = KaffuDiscordBot.serviceProvider.GetRequiredService<CommandHandler>();

		InteractionService interactionService =
			KaffuDiscordBot.serviceProvider.GetRequiredService<InteractionService>();

		InteractionHandler interactionHandler =
			KaffuDiscordBot.serviceProvider.GetRequiredService<InteractionHandler>();

		discordSocketClient.Log += Logger.LogAsync;
		commandService.Log += Logger.LogAsync;
		interactionService.Log += Logger.LogAsync;

		await discordSocketClient.LoginAsync(TokenType.Bot, KaffuDiscordBot.Configuration.Token);
		await discordSocketClient.StartAsync();

		await commandHandler.InitializeAsync();
		await interactionHandler.InitializeAsync();

		await Task.Delay(Timeout.Infinite);
	}
}
