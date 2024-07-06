using Discord.Interactions;

namespace KaffuDiscordBot.InteractionModules;

public class GeneralInteractionModule : InteractionModuleBase
{
    [SlashCommand("ping", "Pong!")]
    public async Task Ping() => await this.RespondAsync("Pong!");
}
