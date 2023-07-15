
using DiscordBotApp.Commands;
using DiscordBotApp.HelpFormater;
using DSharpPlus;
using DSharpPlus.AsyncEvents;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Reflection;
using static DiscordBotApp.Commands.ElectionModule;

namespace DiscordApp
{
    public partial class DiscordBot
    {
        public static DiscordClient Client { get; private set; }

        public static void Main(string[] args) => DiscordBot.AsyncMain().GetAwaiter().GetResult();

        public static void Close()
        {
            Client.DisconnectAsync().GetAwaiter().GetResult();
        }

        public static async Task AsyncMain(params string[] args)
        {
            var discord = new DiscordClient(DiscordConfigurator.GetConfig());
            discord.UseInteractivity(DiscordConfigurator.GetInteractivityConfiguration());

            IServiceCollection serviceCollection = new ServiceCollection().AddSingleton<Random>();
            serviceCollection.AddSingleton<ElectionSingleton>();
            var services = serviceCollection.BuildServiceProvider();

            var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { "!" },
                Services = services

            });
            commands.RegisterCommands<ElectionModule>();
            commands.RegisterCommands<StatisticModule>();
            commands.RegisterCommands<TestModule>();
            //commands.SetHelpFormatter<CustomHelpFormatter>();

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
                        election.Responce(itteraction);
                }
                else
                {

                }
            };
        }
    }

}

