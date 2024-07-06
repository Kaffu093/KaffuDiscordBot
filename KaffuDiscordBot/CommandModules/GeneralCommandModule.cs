using Discord.Commands;

namespace KaffuDiscordBot.CommandModules;

public class GeneralCommandModule : ModuleBase<SocketCommandContext>
{
    [Command("ping")]
    public async Task Ping() => await this.ReplyAsync("Pong!");
}
