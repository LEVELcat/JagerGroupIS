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
using DiscordApp;

namespace DiscordBotApp.Commands
{
    [ModuleLifespan(ModuleLifespan.Transient)]
    internal class ElectionModule : BaseCommandModule
    {
        //[Command("matonAll")]
        //public async Task MentaonAll(CommandContext ctx)
        //{
        //    var button = new DiscordButtonComponent(ButtonStyle.Secondary, "ido", String.Empty,
        //        emoji: new DiscordComponentEmoji(ctx.Guild.Emojis.FirstOrDefault(x => x.Value.Name == "jscatpawscratch").Value));

        //    await ctx.RespondAsync(new DiscordMessageBuilder().WithContent("отобразись блядь").AddComponents(button));
        //}

        [Command("createButton")]
        public async Task CreateElectionEvent(CommandContext ctx, params string[] values)
        {
            new ElectionSingleton().CreateMessage(ctx, values);
        }

        public class ElectionSingleton
        {
            public async void CreateMessage(CommandContext ctx, params string[] values)
            {
                DiscordMessageBuilder message;

                DiscordChannel channel = ctx.Channel;

                if(DateTime.TryParse(values[0] + " " + values[1], out DateTime result))
                {
                    var electionEmbed = ElectionCreateEmbedBuilder(ctx, result, values[2], values);

                    message = new DiscordMessageBuilder()
                        .WithEmbed(electionEmbed)
                        .AddComponents(ReturnButtonComponents);

                    channel = (from category in ctx.Guild.Channels.Values
                               where category.IsCategory && category.Name == "📢 Мероприятия 📢"
                               select category).First();

                    channel = await ctx.Guild.CreateChannelAsync(result.ToString("f"), ChannelType.Text, channel);
                }
                else
                {
                    message = new DiscordMessageBuilder().WithContent("Некоректное значение даты");
                }

                DiscordBot.Client.SendMessageAsync(channel, message);
                ctx.Message.DeleteAsync();
            }

            public void UpdateMessage(CommandContext ctx)
            {


            }

            private DiscordEmbedBuilder ElectionCreateEmbedBuilder(CommandContext ctx, DateTime dateTime, string Title, params string[] description)
            {
                DiscordEmbedBuilder builder = new DiscordEmbedBuilder();

                builder.WithTitle(Title);

                string fullDiscript = string.Join('\n', description, 3, description.Length - 3) 
                    +"\n\nВремя" + Formatter.Timestamp(dateTime, TimestampFormat.LongDateTime) + " " + Formatter.Timestamp(dateTime).ToString();

                builder.WithDescription(fullDiscript);
                builder.AddField("✔️ Будут", "empty", true);

                List<DiscordMember> claners;
                GenerateElectionList(ctx, out claners);

                CreateTechnicalChanelAndSetData(ctx, dateTime, claners);

                string waiterListStr = string.Join('\n',claners.Select(x => x.DisplayName));
                builder.AddField("✖️ Отсуствуют", "empty", true);
                builder.AddField("🌈 Воздержавшиеся", "empty", true);

                builder.Fields[2].Value = waiterListStr;
                return builder;
            }

            private async void CreateTechnicalChanelAndSetData(CommandContext ctx, DateTime dateTime, List<DiscordMember> members)
            {
                var discordChannel = (from chanel in ctx.Guild.Channels.Values
                                                where chanel.IsCategory && chanel.Name == "💻ботерская💻"
                                                select chanel).FirstOrDefault();

                var curentChanel = await ctx.Guild.CreateChannelAsync(dateTime.ToString("f"), ChannelType.Text, discordChannel);

                string message = "yes\n\nno\n\nwait\n" + string.Join(' ', members.Select(x => x.Id));

                await DiscordBot.Client.SendMessageAsync(curentChanel, new DiscordMessageBuilder().WithContent(message));
            }

            private void GenerateElectionList(CommandContext ctx, out List<DiscordMember> claners)
            {
                var role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name == "Jäger Group").Value;
                var notIncludedRole = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name == "Отпуск").Value;

                var userList = ctx.Guild.GetAllMembersAsync().GetAwaiter().GetResult();

                claners = (from user in userList
                           where user.Roles.Contains(role) && !user.Roles.Contains(notIncludedRole)
                           select user).ToList();
            }

            private DiscordComponent[] ReturnButtonComponents => new DiscordComponent[]
            {
                    new DiscordButtonComponent(DSharpPlus.ButtonStyle.Success, "em_aprove", "✔️"),
                    new DiscordButtonComponent(DSharpPlus.ButtonStyle.Danger, "em_deny", "✖️"),
                    new DiscordButtonComponent(DSharpPlus.ButtonStyle.Secondary, "em_edit", "Редактировать"),
                    new DiscordButtonComponent(DSharpPlus.ButtonStyle.Secondary, "em_delete", "Удалить"),
            };


            public async Task Responce(DiscordClient discord, ComponentInteractionCreateEventArgs componentInteraction)
            {
                componentInteraction.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);





            }





        }
    }
}
