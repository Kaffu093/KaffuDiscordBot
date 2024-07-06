using System.Reflection;
using Discord.Interactions;
using Discord.WebSocket;
using KaffuDiscordBot.Logging;

namespace KaffuDiscordBot.Handlers;

internal class InteractionHandler(
	DiscordSocketClient discordSocketClient,
	InteractionService interactionService,
	IServiceProvider serviceProvider
)
{
	public async Task InitializeAsync()
	{
		discordSocketClient.InteractionCreated += this.HandleInteraction;
		discordSocketClient.Ready += async () => await interactionService.RegisterCommandsGloballyAsync();

		await interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), serviceProvider);
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
			await Logger.LogAsync(exception.Message);
		}
	}
}
