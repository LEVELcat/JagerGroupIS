using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.EventArgs;
using System.Security.Cryptography.X509Certificates;
using DSharpPlus.Interactivity;

namespace DiscordBotApp.Commands
{
    internal class ElectionModule : BaseCommandModule
    {
        [Command("createButton")]
        public async Task CreateElectionEvent(CommandContext ctx)
        {
             ElectionSingleton.CreateMessage(ctx);
        }

        public class ElectionSingleton
        {
            public static async Task CreateMessage(CommandContext ctx)
            {
                var message = new DiscordMessageBuilder()
                    .AddEmbed(ElectionCreateEmbedBuilder(ctx))
                    .AddComponents(ReturnButtonComponents);

                await ctx.RespondAsync(message);

                
            }

            public static async Task UpdateMessage(CommandContext ctx)
            {


            }

            private static DiscordEmbedBuilder ElectionCreateEmbedBuilder(CommandContext ctx)
            {
                DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
                builder.WithTitle("Голосование");
                builder.WithDescription("Голосуем");
                builder.AddField("✔️ Будут", "JG | Хороший мальчик\nJG |Хороший котик\nJG | Данжон мастер", true);
                builder.AddField("✖️ Отсуствуют", "JG | Сплюн\nJG | Работюн\nJG | Гладящий котиков", true);
                builder.AddField("🌈 Воздержавшиеся", GenerateList(ctx), true);
                return builder;
            }

            private static string GenerateList(CommandContext ctx)
            {
                string result = string.Empty;

                string roleName = "Jäger Group";

                var claners = (from user in ctx.Guild.Members.Values
                              where user.Roles.FirstOrDefault(x => x.Name == roleName) != null
                              select user.Mention).ToList();

                foreach(var user in claners)
                {
                    result += user;
                }
                return result;
            }

            private static DiscordComponent[] ReturnButtonComponents => new DiscordComponent[]
            {
                    new DiscordButtonComponent(DSharpPlus.ButtonStyle.Success, "em_aprove", "✔️"),
                    new DiscordButtonComponent(DSharpPlus.ButtonStyle.Danger, "em_deny", "✖️"),
                    new DiscordButtonComponent(DSharpPlus.ButtonStyle.Secondary, "em_edit", "Редактировать"),
                    new DiscordButtonComponent(DSharpPlus.ButtonStyle.Secondary, "em_delete", "Удалить"),
            };


            public async Task Responce(DiscordClient discord, ComponentInteractionCreateEventArgs componentInteraction)
            {
                await componentInteraction.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);
            }
        }
    }
}
