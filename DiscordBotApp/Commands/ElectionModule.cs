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
        [Command("echo")]
        public async Task SendMessage(CommandContext ctx, params string[] values)
        {
            ctx.Member.SendMessageAsync(string.Join(' ', values));
        }


        [Command("event")]
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
                    var electionEmbed = ElectionCreateEmbedBuilder(ctx, result, values[2], values.ToList());

                    message = new DiscordMessageBuilder()
                        .WithContent("<@&898534300377567252>")
                        .WithAllowedMention(new RoleMention(ctx.Guild.Roles.Values.First(x => x.Name == "Jäger Group")))
                        .WithEmbed(electionEmbed)
                        .AddComponents(ReturnButtonComponents);


                    channel = (from category in ctx.Guild.Channels.Values
                               where category.IsCategory && category.Name == "📢 Мероприятия 📢"
                               select category).First();

                    channel = await ctx.Guild.CreateChannelAsync(result.ToString("ddd d MMM yyyy HH-mm"), ChannelType.Text, channel);
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

            private DiscordEmbedBuilder ElectionCreateEmbedBuilder(CommandContext ctx, DateTime dateTime, string Title, List<string> description)
            {
                DiscordEmbedBuilder builder = new DiscordEmbedBuilder();

                builder.WithTitle(Title);

                var imageURL = description.FirstOrDefault(x => x.StartsWith("URL="));
                if(imageURL != string.Empty)
                {
                    description.Remove(imageURL);
                    imageURL = imageURL.Split("URL=")[1];
                }


                string fullDiscript = string.Join('\n', description.GetRange(3, description.Count - 3))
                    +"\n\nНачало : " + Formatter.Timestamp(dateTime, TimestampFormat.LongDateTime) + " " + Formatter.Timestamp(dateTime).ToString();

                builder.WithDescription(fullDiscript);
                builder.AddField("<:emoji_134:941666424324239430>", "empty", true); //Будут

                List<DiscordMember> claners;
                GenerateElectionList(ctx.Guild, out claners);

                CreateTechnicalChanelAndSetData(ctx, dateTime, claners);

                string waiterListStr = string.Join('\n', claners.Select(x => '\\' + x.DisplayName));
                builder.AddField("<:1_:941666407513473054>", "empty", true); //отсуствуют
                builder.AddField("<a:load:1112311359548444713>", "empty", true); //воздрежавшиеся.

                builder.Fields[0].Value = string.Empty;
                builder.Fields[1].Value = string.Empty;
                builder.Fields[2].Value = waiterListStr;

                builder.Color = new DiscordColor(255, 165, 0);

                if (imageURL == string.Empty)
                    builder.WithImageUrl("https://cdn.discordapp.com/attachments/897797696516141057/1110576983018049667/5_.png");
                else
                    builder.WithImageUrl(imageURL);


                return builder;
            }

            private async void CreateTechnicalChanelAndSetData(CommandContext ctx, DateTime dateTime, List<DiscordMember> members)
            {
                var discordChannel = (from chanel in ctx.Guild.Channels.Values
                                                where chanel.IsCategory && chanel.Name == "💻ботерская💻"
                                                select chanel).FirstOrDefault();

                var curentChanel = await ctx.Guild.CreateChannelAsync(dateTime.ToString("ddd d MMM yyyy HH-mm"), ChannelType.Text, discordChannel);

                string message = "yes\n\nno\n\nwait\n" + string.Join(' ', members.Select(x => x.Id)) +"\nEnd";

                await DiscordBot.Client.SendMessageAsync(curentChanel, new DiscordMessageBuilder().WithContent(message));
            }

            private void GenerateElectionList(DiscordGuild guild, out List<DiscordMember> claners)
            {
                var role = guild.Roles.FirstOrDefault(x => x.Value.Name == "Jäger Group").Value;
                var notIncludedRole = guild.Roles.FirstOrDefault(x => x.Value.Name == "Отпуск").Value;

                var userList = guild.GetAllMembersAsync().GetAwaiter().GetResult();

                claners = (from user in userList
                           where user.Roles.Contains(role) && !user.Roles.Contains(notIncludedRole)
                           select user).ToList();
            }

            private DiscordComponent[] ReturnButtonComponents => new DiscordComponent[]
            {
                    new DiscordButtonComponent(DSharpPlus.ButtonStyle.Success, "em_aprove", string.Empty, emoji: new DiscordComponentEmoji(941666424324239430)),
                    new DiscordButtonComponent(DSharpPlus.ButtonStyle.Danger, "em_deny", string.Empty, emoji: new DiscordComponentEmoji(941666407513473054)),
                    new DiscordButtonComponent(DSharpPlus.ButtonStyle.Secondary, "em_update", "Обновить список"),
                    new DiscordButtonComponent(DSharpPlus.ButtonStyle.Secondary, "em_delete", "🗑️")
            };


            public async Task Responce(ComponentInteractionCreateEventArgs componentInteraction)
            {
                componentInteraction.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);

                var techChannel = componentInteraction.Guild.Channels.Values.First(x => x.IsCategory && x.Name == "💻ботерская💻")
                    .Children.First(x => x.Name == componentInteraction.Channel.Name);

                GenerateElectionList(componentInteraction.Guild, out List<DiscordMember> claners);

                List<ulong> yesList = new List<ulong>(), noList = new List<ulong>(), waitList = new List<ulong>();

                var techMessage = await techChannel.GetMessagesAsync(1);

                lock(this)
                {
                    var content = techMessage.First().Content;

                    string[] values = content.Split('\n');

                    if (values[1] != string.Empty)
                        yesList = Array.ConvertAll(values[1].Split(' '), x => ulong.Parse(x)).ToList();

                    if (values[3] != string.Empty)
                        noList = Array.ConvertAll(values[3].Split(' '), x => ulong.Parse(x)).ToList();

                    if (values[5] != string.Empty)
                        waitList = Array.ConvertAll(values[5].Split(' '), x => ulong.Parse(x)).ToList();

                    List<ulong> removedFromElection = searchRemovedFromElection();
                    List<ulong> addedToElection = searchAddedToElection();
                    List<ulong> searchRemovedFromElection()
                    {

                        return (from id in yesList.Concat(noList).Concat(waitList)
                                where claners.Select(x => x.Id).Contains(id) == false
                                select id).ToList();
                    }
                    List<ulong> searchAddedToElection()
                    {
                        return (from user in claners
                                where (yesList.Contains(user.Id) || noList.Contains(user.Id) || waitList.Contains(user.Id)) == false
                                select user.Id).ToList();
                    }

                    waitList.AddRange(addedToElection);

                    foreach (var removed in removedFromElection)
                    {
                        RemoveFromLists(removed);
                    }

                    void RemoveFromLists(ulong removed)
                    {
                        yesList.Remove(removed);
                        noList.Remove(removed);
                        waitList.Remove(removed);
                    }

                    ulong selectedId = componentInteraction.User.Id;

                    switch (componentInteraction.Id)
                    {
                        case "em_aprove":
                            if (yesList.Contains(selectedId) == false)
                            {
                                RemoveFromLists(componentInteraction.User.Id);
                                yesList.Add(selectedId);
                            }
                            else
                            {
                                RemoveFromLists(selectedId);
                                waitList.Add(selectedId);
                            }
                            break;
                        case "em_deny":
                            if (noList.Contains(selectedId) == false)
                            {
                                RemoveFromLists(componentInteraction.User.Id);
                                noList.Add(selectedId);
                            }
                            else
                            {
                                RemoveFromLists(selectedId);
                                waitList.Add(selectedId);
                            }
                            break;
                        case "em_edit":
                            break;
                        case "em_delete":
                            DiscordMember member = componentInteraction.Guild.Members.Values.FirstOrDefault(x => x.Id == selectedId);
                            if (member.Roles.Contains(componentInteraction.Guild.Roles.Values.First(x => x.Name == "Администратор")))
                            {
                                componentInteraction.Channel.DeleteAsync();
                                techChannel.DeleteAsync();
                                return;
                            }
                            break;
                        case "em_update":
                            break;
                    }
                    string message = $"yes\n{string.Join(' ', yesList)}\nno\n{string.Join(' ', noList)}\nwait\n{string.Join(' ', waitList)}\nEnd";
                    var mes = techMessage.First().ModifyAsync(message).GetAwaiter().GetResult();
                }

                DiscordEmbed embed = componentInteraction.Message.Embeds[0];

                //Будут
                embed.Fields[0].Name = "<:emoji_134:941666424324239430> " + yesList.Count;
                embed.Fields[0].Value = string.Join('\n', (from user in claners
                                                                                            where yesList.Contains(user.Id)
                                                                                            select user.DisplayName));

                embed.Fields[0].Value = embed.Fields[0].Value.Replace("_", "\\_");

                //Отсуствуют
                embed.Fields[1].Name = "<:1_:941666407513473054> " + noList.Count;
                embed.Fields[1].Value = string.Join('\n', (from user in claners
                                                                                            where noList.Contains(user.Id)
                                                                                            select user.DisplayName));

                embed.Fields[1].Value = embed.Fields[1].Value.Replace("_", "\\_");

                //Воздержавшиеся
                embed.Fields[2].Name = "<a:load:1112311359548444713> " + waitList.Count;
                embed.Fields[2].Value = string.Join('\n', (from user in claners
                                                                                            where waitList.Contains(user.Id)
                                                                                            select user.DisplayName ));
                embed.Fields[2].Value = embed.Fields[2].Value.Replace("_", "\\_");

                DiscordMessageBuilder builder = new DiscordMessageBuilder(componentInteraction.Message);
                builder.Embed = embed;
                builder.ClearComponents();
                builder.AddComponents(ReturnButtonComponents);

                //componentInteraction.Message.ModifyAsync(embed);
                componentInteraction.Message.ModifyAsync(builder);
            }
        }
    }


}
