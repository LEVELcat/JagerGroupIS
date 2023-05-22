
using DiscordBotApp.Commands;
using DiscordBotApp.HelpFormater;
using DSharpPlus;
using DSharpPlus.AsyncEvents;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using static DiscordBotApp.Commands.ElectionModule;

namespace DiscordApp
{
    public class DiscordBot
    {
        public static DiscordClient Client { get; private set; }

        public static void Main(string[] args) => new DiscordBot().AsyncMain().GetAwaiter().GetResult();

        private async Task AsyncMain(params string[] args)
        {
            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = "token",
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug,
                LogTimestampFormat = "MMM dd yyyy - hh:mm:ss tt",
                
            }) ;

            IServiceCollection serviceCollection = new ServiceCollection().AddSingleton<Random>();
            serviceCollection.AddSingleton<ElectionSingleton>();
            var services = serviceCollection.BuildServiceProvider();



            var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { "!" },
                Services = services

            });
            commands.RegisterCommands<ElectionModule>();
            commands.SetHelpFormatter<CustomHelpFormatter>();

            discord.ComponentInteractionCreated += RegistrateInteractionEvent(services);



            await discord.ConnectAsync();

            Client = discord;


            await Task.Delay(-1);
        }

        private static AsyncEventHandler<DiscordClient, ComponentInteractionCreateEventArgs> RegistrateInteractionEvent(ServiceProvider services)
        {
            return async (ctx, itteraction) =>
            {
                if (itteraction.Id.StartsWith("em_"))
                {
                    if (services.GetService<ElectionSingleton>() is ElectionSingleton election)
                        election.Responce(ctx, itteraction);
                }
                else
                {

                }
            };
        }
    }

}

