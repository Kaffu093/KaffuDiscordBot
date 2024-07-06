using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace KaffuDiscordBot.Logging;

internal class Logger
{
    internal Logger(DiscordSocketClient discordSocketClient, CommandService commandService)
    {
        discordSocketClient.Log += Logger.LogAsync;
        commandService.Log += Logger.LogAsync;
    }

    internal static Task LogAsync(LogMessage logMessage)
    {
        Console.ForegroundColor = logMessage.Severity switch
        {
            LogSeverity.Info => ConsoleColor.White,
            LogSeverity.Debug => ConsoleColor.White,
            LogSeverity.Verbose => ConsoleColor.White,
            LogSeverity.Warning => ConsoleColor.Yellow,
            LogSeverity.Error => ConsoleColor.Red,
            LogSeverity.Critical => ConsoleColor.Red,
            _ => Console.ForegroundColor
        };

        Console.WriteLine(
            logMessage.Exception switch
            {
                CommandException commandException
                    => $"[Command - {logMessage.Severity}] "
                        + $"The user {commandException.Context.User.GlobalName} "
                        + $"tried to execute the command {commandException.Command.Aliases[0]} "
                        + $"but failed, in the channel {commandException.Context.Channel.Name} "
                        + $"from {commandException.Context.Guild.Name} guild.",
                _ => $"[GeneralInteractionModule - {logMessage.Severity}] {logMessage.Message}"
            }
        );

        Console.ResetColor();

        return Task.CompletedTask;
    }

	internal static Task LogAsync(string message)
	{
		Console.WriteLine(message);
		return Task.CompletedTask;
	}
}
