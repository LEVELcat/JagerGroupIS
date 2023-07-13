using DSharpPlus;
using DSharpPlus.Interactivity;

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
                    //Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents,
                    Intents = DiscordIntents.All,
                    MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Information,
                    LogTimestampFormat = "MMM dd yyyy - hh:mm:ss tt"
                };
            }

            public static InteractivityConfiguration GetInteractivityConfiguration()
            {
                return new InteractivityConfiguration()
                {
                    
                };
            }
        }
    }

}

