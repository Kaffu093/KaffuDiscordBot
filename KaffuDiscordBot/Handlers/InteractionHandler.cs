using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using KaffuDiscordBot.InteractionModules;
using KaffuDiscordBot.Logging;
using KaffuDiscordBot.Models;

namespace KaffuDiscordBot.Handlers;

internal class InteractionHandler(
	DiscordSocketClient discordSocketClient,
	InteractionService interactionService,
	IServiceProvider serviceProvider,
	Configuration configuration
)
{
	private static readonly InteractionModuleBase[] InteractionModules = [new GeneralInteractionModule()];

	public void InitializeAsync()
	{
		discordSocketClient.Ready += this.RegisterInteractionsAsync;
		discordSocketClient.InteractionCreated += this.HandleInteraction;
	}
	private async Task RegisterInteractionsAsync()
	{
		foreach (InteractionModuleBase interactionModule in InteractionHandler.InteractionModules)
		{
			await Logger.LogAsync(
				new LogMessage(
					LogSeverity.Info,
					"InteractionHandler.cs",
					$"Registering interaction modules locally: {interactionModule.GetType()}..."
				)
			);

			await interactionService.AddModuleAsync(interactionModule.GetType(), serviceProvider);

			await Logger.LogAsync(
				new LogMessage(
					LogSeverity.Info,
					"InteractionHandler.cs",
					$"Registered interaction modules locally: {interactionModule.GetType()}."
				)
			);
		}

		await Logger.LogAsync(
			new LogMessage(
				LogSeverity.Info,
				"InteractionHandler.cs",
				"Registering all interaction modules to Discord in Development Guild..."
			)
		);

		await interactionService.RegisterCommandsToGuildAsync(configuration.GuildId);

		await Logger.LogAsync(
			new LogMessage(
				LogSeverity.Info,
				"InteractionHandler.cs",
				"Registered all interaction modules to Discord in Development Guild."
			)
		);	

		await Logger.LogAsync(
			new LogMessage(
				LogSeverity.Info,
				"InteractionHandler.cs",
				"Registering all interaction modules to Discord globally..."
			)
		);	

		await interactionService.RegisterCommandsGloballyAsync();

		await Logger.LogAsync(
			new LogMessage(
				LogSeverity.Info,
				"InteractionHandler.cs",
				"Registered all interaction modules to Discord globally."
			)
		);
	}

	private async Task HandleInteraction(SocketInteraction interaction)
	{
		try
		{
			SocketInteractionContext context = new SocketInteractionContext(discordSocketClient, interaction);
			await interactionService.ExecuteCommandAsync(context, serviceProvider);
		}
		catch (Exception exception)
		{
			await Logger.LogAsync(new LogMessage(LogSeverity.Error, exception.Source, exception.Message, exception));
		}
	}
}
