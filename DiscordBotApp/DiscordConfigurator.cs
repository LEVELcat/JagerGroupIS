using DSharpPlus;

namespace DiscordApp
{
    public partial class DiscordBot
    {
        class DiscordConfigurator
        {
            public static DiscordConfiguration GetConfig()
            {
                return new DiscordConfiguration()
                {
                    Token = "token",
                    TokenType = TokenType.Bot,
                    Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents,
                    MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug,
                    LogTimestampFormat = "MMM dd yyyy - hh:mm:ss tt"
                };
            }
        }
    }

}

