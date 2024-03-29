﻿
using DiscordBotApp.Modules;
using DiscordBotApp.Modules.DiscordElectionNotificateClasses;
using DiscordBotApp.Modules.ElectionModuleClasses;
using DSharpPlus;
using DSharpPlus.AsyncEvents;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Modules = DiscordBotApp.Modules;

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
            try
            {
                var discord = new DiscordClient(DiscordConfigurator.GetConfig());
                discord.UseInteractivity(DiscordConfigurator.GetInteractivityConfiguration());

                IServiceCollection serviceCollection = new ServiceCollection().AddSingleton<Random>();
                serviceCollection.AddTransient<ElectionResponce>();
                //serviceCollection.AddSingleton<ElectionSingleton>();
                var services = serviceCollection.BuildServiceProvider();

                var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
                {
                    StringPrefixes = new[] { "!" },
                    Services = services

                });
                commands.RegisterCommands<ElectionModule>();
                commands.RegisterCommands<StatisticModule>();
                commands.RegisterCommands<NotificationCycleModule>();
                //commands.SetHelpFormatter<CustomHelpFormatter>();

                discord.ComponentInteractionCreated += RegistrateInteractionEvent(services);

                await discord.ConnectAsync();

                Client = discord;

                await Task.Delay(-1);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.ToString());
            }
        }

        private static AsyncEventHandler<DiscordClient, ComponentInteractionCreateEventArgs> RegistrateInteractionEvent(ServiceProvider services)
        {
            return async (ctx, itteraction) =>
            {
                if (itteraction.Id.StartsWith("EL_"))
                {
                    if (services.GetService<Modules.ElectionModuleClasses.ElectionResponce>() is Modules.ElectionModuleClasses.ElectionResponce electionResponce)
                        electionResponce.Responce(itteraction);
                }
                else
                {

                }
            };
        }
    }

}

