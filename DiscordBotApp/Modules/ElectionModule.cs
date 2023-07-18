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
using DbLibrary.JagerDsModel;
using DSharpPlus.Interactivity.Extensions;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DiscordBotApp.Commands
{
    [ModuleLifespan(ModuleLifespan.Transient)]
    internal class ElectionModule : BaseCommandModule
    {
        [Command("ev")]
        public async Task EventCommandInvoke(CommandContext ctx)
        {
            new ElectionFactory().CreateElectionAsync(ctx);
        }

        public class ElectionFactory
        {
            public async Task CreateElectionAsync(CommandContext ctx)
            {
                var option = await ConstructElectionAsync(ctx);

                DiscordMessageBuilder messageBuilder = option.Item1;
                messageBuilder.AddComponents(ReturnButtonComponents());

                Election election = option.Item2;

                using(JagerDbContext dbContext = new JagerDbContext())
                {
                    foreach(var  roleSetup in election.RoleSetups)
                    {
                        roleSetup.Roles = await FixRolesAsync(roleSetup.Roles);
                    }

                    var chanel = await ctx.Guild.CreateChannelAsync(messageBuilder.Embed.Title, ChannelType.Text, ctx.Guild.Channels[election.ChanelID]);

                    var electionMessage = await chanel.SendMessageAsync(messageBuilder);

                    election.ChanelID = chanel.Id;
                    election.MessageID = electionMessage.Id;

                    dbContext.Elections.Add(election);
                    await dbContext.SaveChangesAsync();

                    async Task<Role> FixRolesAsync(Role roles)
                    {
                        Role? result = await dbContext.Roles.FirstOrDefaultAsync(r =>
                                                             r.GuildID == roles.GuildID && r.RoleDiscordID == roles.RoleDiscordID);
                        if(result == null)
                        {
                            result = roles;
                            dbContext.Roles.Add(roles);
                        }
                        return result;
                    }
                }

                DiscordComponent[] ReturnButtonComponents() => new DiscordComponent[]
                {
                        new DiscordButtonComponent(ButtonStyle.Success, $"EL_APROVE", string.Empty, emoji: new DiscordComponentEmoji(941666424324239430)),
                        new DiscordButtonComponent(ButtonStyle.Danger, $"EL_DENY", string.Empty, emoji: new DiscordComponentEmoji(941666407513473054)),
                        new DiscordButtonComponent(ButtonStyle.Secondary, $"EL_UPDATE", "Обновить список"),
                        //new DiscordButtonComponent(ButtonStyle.Secondary, $"EL_EDIT", "Редактировать событие"),
                        new DiscordButtonComponent(ButtonStyle.Secondary, $"EL_DELETE", "🗑️")
                };
            }





            [Flags]
            enum ElectionReadyEnum : byte
            {
                None = 0,
                Name = 1,
                EndDate = 2,
                RoleSelect = 4,
                ChanelSelect = 8,
            }
            private async Task<(DiscordMessageBuilder, Election)> ConstructElectionAsync(CommandContext ctx)
            {
                Guid guid = new Guid();

                ctx.Message.DeleteAsync();

                Election election = new Election()
                {
                    BitMaskSettings = BitMaskElection.AgreeList | BitMaskElection.RejectList | BitMaskElection.NotVotedList
                };

                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();

                //embedBuilder.AddField("<:emoji_134:941666424324239430>", "empty", true);
                //embedBuilder.AddField("<:1_:941666407513473054>", "empty", true);
                //embedBuilder.AddField("<a:load:1112311359548444713>", "empty", true);

                embedBuilder.Title = "Название";
                //embedBuilder.Timestamp = DateTime.Now;
                //embedBuilder.WithFooter("Тут какой то отступ c иконкой", "https://media.istockphoto.com/vectors/vector-icon-add-user-add-person-or-add-friend-on-blue-background-flat-vector-id1050964578?k=20&m=1050964578&s=612x612&w=0&h=tK0SFWQVYJdACEGZRRbrKsPw7JkXghBRn_AzBDHcT84=");
                embedBuilder.WithDescription("Описание");
                //mbedBuilder.WithThumbnail("https://media.istockphoto.com/vectors/vector-icon-add-user-add-person-or-add-friend-on-blue-background-flat-vector-id1050964578?k=20&m=1050964578&s=612x612&w=0&h=tK0SFWQVYJdACEGZRRbrKsPw7JkXghBRn_AzBDHcT84=");
                //embedBuilder.WithImageUrl("https://w.forfun.com/fetch/42/429f04f158ff611068a6eba8af8fe776.jpeg?w=1470&r=0.5625");
                embedBuilder.WithColor(new DiscordColor("#000000"));


                DiscordMessageBuilder viewMessageBuilder = new DiscordMessageBuilder();
                viewMessageBuilder.WithEmbed(embedBuilder);

                DiscordMessageBuilder menuMessageBuilder = new DiscordMessageBuilder();
                menuMessageBuilder.WithContent("Тут менюшка");
                menuMessageBuilder.AddComponents(GetMenuButtonRows().AsEnumerable());

                var viewMessage = await ctx.Channel.SendMessageAsync(viewMessageBuilder);
                var menuMessage = await ctx.Channel.SendMessageAsync(menuMessageBuilder);

                DiscordChannel channel = viewMessage.Channel;

                bool isExit = false;
                ElectionReadyEnum electionReady = ElectionReadyEnum.None;

                while (true)
                {
                    viewMessage.ModifyAsync(viewMessageBuilder);

                    var respond = await menuMessage.WaitForButtonAsync(TimeSpan.FromMinutes(10));

                    if (respond.TimedOut)
                    {
                        isExit = true;
                    }
                    else
                    {
                        try
                        {

                            switch (respond.Result.Id)
                            {
                                case "menu1":
                                    DiscordInteractionResponseBuilder responseBuilder = new DiscordInteractionResponseBuilder();
                                    responseBuilder.WithTitle("Настройки #1");

                                    guid = Guid.NewGuid();
                                    responseBuilder.WithCustomId(guid.ToString());

                                    responseBuilder.AddComponents(new TextInputComponent("Название", "name"));
                                    responseBuilder.AddComponents(new TextInputComponent("Дата события", "date", value: DateTime.Now.ToString()));
                                    responseBuilder.AddComponents(new TextInputComponent("Описание события", "desc", style: TextInputStyle.Paragraph));
                                    responseBuilder.AddComponents(new TextInputComponent("URL большой картинки снизу", "image", required: false));
                                    responseBuilder.AddComponents(new TextInputComponent("URL картинки справа", "thumb", required: false));

                                    respond.Result.Interaction.CreateResponseAsync(InteractionResponseType.Modal, responseBuilder);

                                    var input = ctx.Client.GetInteractivity();

                                    var txtResponce = await input.WaitForModalAsync(guid.ToString(), TimeSpan.FromMinutes(20));

                                    if (txtResponce.TimedOut)
                                    {
                                        isExit = true;
                                    }
                                    else
                                    {
                                        txtResponce.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);

                                        var values = txtResponce.Result.Values;

                                        if (values["name"] != string.Empty)
                                        {
                                            embedBuilder.Title = values["name"];
                                            electionReady |= ElectionReadyEnum.Name;
                                        }


                                        if (values["desc"] != string.Empty)
                                            embedBuilder.Description = values["desc"];

                                        if (values["date"] != string.Empty)
                                        {
                                            election.StartTime = DateTime.Now;
                                            if (DateTime.TryParse(values["date"], out DateTime result))
                                            {
                                                election.EndTime = result;

                                                embedBuilder.Description += "\n" + "Начало : " + Formatter.Timestamp(result, TimestampFormat.LongDateTime) + " " +
                                                                            " " + Formatter.Timestamp(result).ToString();

                                                electionReady |= ElectionReadyEnum.EndDate;
                                            }
                                        }

                                        if (values["image"] != string.Empty)
                                            embedBuilder.ImageUrl = values["image"];

                                        if (values["thumb"] != string.Empty)
                                            embedBuilder.WithThumbnail(values["thumb"]);

                                        viewMessageBuilder.Embed = embedBuilder;
                                    }
                                    break;
                                case "menu2":
                                    DiscordInteractionResponseBuilder responseBuilder2 = new DiscordInteractionResponseBuilder();
                                    responseBuilder2.WithTitle("Настройки #2");
                                    guid = Guid.NewGuid();
                                    responseBuilder2.WithCustomId(guid.ToString());

                                    responseBuilder2.AddComponents(new TextInputComponent("Подпись снизу", "footerText", required: false));
                                    responseBuilder2.AddComponents(new TextInputComponent("URL Иконки возле подписи", "footerIcon", required: false));
                                    responseBuilder2.AddComponents(new TextInputComponent("Цвета слева", "color", value: "#000000", required: false));

                                    respond.Result.Interaction.CreateResponseAsync(InteractionResponseType.Modal, responseBuilder2);

                                    var input2 = ctx.Client.GetInteractivity();

                                    var txtResponce2 = await input2.WaitForModalAsync(guid.ToString(), TimeSpan.FromMinutes(20));

                                    if (txtResponce2.TimedOut)
                                    {
                                        isExit = true;
                                    }
                                    else
                                    {
                                        txtResponce2.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);

                                        var values = txtResponce2.Result.Values;

                                        if (values["footerText"] != string.Empty && values["footerIcon"] != string.Empty)
                                            embedBuilder.WithFooter(values["footerText"], values["footerIcon"]);
                                        else if (values["footerText"] != string.Empty)
                                            embedBuilder.WithFooter(text: values["footerText"]);
                                        //NOT WORKING WITHOUT FOOTER_TEXT
                                        //else if (values["footerIcon"] != string.Empty)
                                        //    embedBuilder.WithFooter(iconUrl: values["footerIcon"]);

                                        if (values["color"] != string.Empty)
                                            embedBuilder.WithColor(new DiscordColor(values["color"]));

                                        viewMessageBuilder.Embed = embedBuilder;
                                    }
                                    break;
                                case "menu3":

                                    respond.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);
                                    DiscordMessageBuilder roleSelectMessageBuilder = new DiscordMessageBuilder();
                                    roleSelectMessageBuilder.WithContent("Выберите роли участвующие в событии");
                                    roleSelectMessageBuilder.AddComponents(new DiscordRoleSelectComponent("roles", "выберите роль", maxOptions: 10));

                                    var roleSelectMessage = await ctx.Channel.SendMessageAsync(roleSelectMessageBuilder);

                                    var roleSelectResponce = await roleSelectMessage.WaitForSelectAsync("roles", TimeSpan.FromMinutes(5));

                                    if (roleSelectResponce.TimedOut)
                                    {
                                        isExit = true;
                                    }
                                    else
                                    {
                                        roleSelectResponce.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);

                                        if (roleSelectResponce.Result.Values.Length > 0)
                                            electionReady |= ElectionReadyEnum.RoleSelect;

                                        if (election.RoleSetups == null)
                                            election.RoleSetups = new List<RoleSetup>();

                                        election.RoleSetups.Clear();

                                        string newContent = string.Empty;

                                        foreach (var id_str in roleSelectResponce.Result.Values)
                                        {
                                            election.RoleSetups.Add(
                                                new RoleSetup()
                                                {
                                                    Roles = new Role()
                                                    {
                                                        GuildID = ctx.Guild.Id,
                                                        RoleDiscordID = ulong.Parse(id_str)
                                                    }
                                                });
                                            newContent += ctx.Guild.Roles[ulong.Parse(id_str)].Mention;
                                        }
                                        viewMessageBuilder.Content = newContent;
                                    }
                                    roleSelectMessage.DeleteAsync();

                                    break;
                                case "menu4":

                                    respond.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);

                                    DiscordMessageBuilder listElectionMessageBuilder = new DiscordMessageBuilder();
                                    listElectionMessageBuilder.WithContent("Выберите нужные настройки");

                                    var opions = new DiscordSelectComponentOption[]
                                    {
                                    new DiscordSelectComponentOption("Список согласных", ((ulong)BitMaskElection.AgreeList).ToString()),
                                    new DiscordSelectComponentOption("Список отказавшихся", ((ulong)BitMaskElection.RejectList).ToString()),
                                    new DiscordSelectComponentOption("Список воздержавшихся", ((ulong)BitMaskElection.NotVotedList).ToString()),
                                    new DiscordSelectComponentOption("Уведомление для согласившихся", ((ulong)BitMaskElection.NotificationForAgree).ToString()),
                                    new DiscordSelectComponentOption("Уведомление для воздежавшихся", ((ulong)BitMaskElection.NotificationForNotVoted).ToString()),
                                    };
                                    listElectionMessageBuilder.AddComponents(new DiscordSelectComponent("listElection", null, opions, maxOptions: 5));

                                    var listElectionMessage = await ctx.Channel.SendMessageAsync(listElectionMessageBuilder);

                                    var listElectionResponce = await listElectionMessage.WaitForSelectAsync("listElection", TimeSpan.FromMinutes(5));

                                    if (listElectionResponce.TimedOut)
                                    {
                                        isExit = true;
                                    }
                                    else
                                    {
                                        listElectionResponce.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);

                                        BitMaskElection result = BitMaskElection.None;

                                        foreach (var select in listElectionResponce.Result.Values)
                                            result |= (BitMaskElection)ulong.Parse(select);

                                        election.BitMaskSettings = result;

                                        embedBuilder.ClearFields();

                                        if (result.HasFlag(BitMaskElection.AgreeList))
                                            embedBuilder.AddField("<:emoji_134:941666424324239430>", "empty", true);

                                        if (result.HasFlag(BitMaskElection.RejectList))
                                            embedBuilder.AddField("<:1_:941666407513473054>", "empty", true);

                                        if (result.HasFlag(BitMaskElection.NotVotedList))
                                            embedBuilder.AddField("<a:load:1112311359548444713>", "empty", true);

                                        viewMessageBuilder.Embed = embedBuilder;

                                        electionReady |= ElectionReadyEnum.RoleSelect;
                                    }
                                    listElectionMessage.DeleteAsync();
                                    break;
                                case "menu5":

                                    respond.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);

                                    DiscordMessageBuilder chanelSelectMessageBuilder = new DiscordMessageBuilder();
                                    chanelSelectMessageBuilder.WithContent("Выберите категорию, в которой будет опубликовано событие");
                                    chanelSelectMessageBuilder.AddComponents(new DiscordChannelSelectComponent("chanels", "выберите канал",
                                                                             new ChannelType[] { ChannelType.Category }));

                                    var chanelSelectMessage = await ctx.Channel.SendMessageAsync(chanelSelectMessageBuilder);

                                    var chanelSelectResponce = await chanelSelectMessage.WaitForSelectAsync("chanels", TimeSpan.FromMinutes(5));

                                    if (chanelSelectResponce.TimedOut)
                                    {
                                        isExit = true;
                                    }
                                    else
                                    {
                                        chanelSelectResponce.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);

                                        election.ChanelID = ulong.Parse(chanelSelectResponce.Result.Values[0]);

                                        electionReady |= ElectionReadyEnum.ChanelSelect;
                                    }
                                    chanelSelectMessage.DeleteAsync();
                                    break;
                                case "menu8":

                                    respond.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);


                                    if (electionReady == (ElectionReadyEnum.Name | ElectionReadyEnum.EndDate | ElectionReadyEnum.RoleSelect | ElectionReadyEnum.ChanelSelect))
                                    {
                                        for (int i = 0; i < embedBuilder.Fields.Count; i++)
                                            embedBuilder.Fields[i].Value = string.Empty;

                                        viewMessage.DeleteAsync();
                                        menuMessage.DeleteAsync();
                                        return (viewMessageBuilder, election);
                                    }
                                    else
                                    {

                                    }

                                    break;
                                case "menu9":
                                    isExit = true;
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            DiscordBot.Client.Logger.LogError(ex.Message);
                            DiscordBot.Client.Logger.LogError(ex.ToString());
                        }
                    }
                    if (isExit == true)
                    {
                        viewMessage.DeleteAsync();
                        menuMessage.DeleteAsync();
                        return (null, null);
                    }
                }

                DiscordActionRowComponent[] GetMenuButtonRows() => new DiscordActionRowComponent[]
                {
                    new DiscordActionRowComponent(_firstPart()),
                    new DiscordActionRowComponent(_secondPart()),
                };
                DiscordComponent[] _firstPart() => new DiscordComponent[]
                {
                    new DiscordButtonComponent
                    (
                        ButtonStyle.Secondary,
                        "menu1",
                        "Настройка #1"
                    ),
                    new DiscordButtonComponent
                    (
                        ButtonStyle.Secondary,
                        "menu2",
                        "Настройка #2"
                    ),
                    new DiscordButtonComponent
                    (
                        ButtonStyle.Secondary,
                        "menu3",
                        "Настроить роли участвующие в событии"
                    ),
                    new DiscordButtonComponent
                    (
                        ButtonStyle.Secondary,
                        "menu4",
                        "Настроить списки участников на событие"
                    ),

                    new DiscordButtonComponent
                    (
                        ButtonStyle.Secondary,
                        "menu5",
                        "Выбрать канал для публикации"
                    ),

                };
                DiscordComponent[] _secondPart() => new DiscordComponent[]
                {
                    new DiscordButtonComponent
                    (
                        ButtonStyle.Success,
                        "menu8",
                        "Опубликовать голосование"
                    ),
                    new DiscordButtonComponent
                    (
                        ButtonStyle.Danger,
                        "menu9",
                        "Удалить"
                    )
                };
            }
        }

        public class ElectionResponce
        {
            public async Task Responce(ComponentInteractionCreateEventArgs componentInteraction)
            {
                componentInteraction.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);

                using(JagerDbContext dbContext = new JagerDbContext())
                {
                    Election? election = dbContext.Elections.FirstOrDefault(e => e.GuildID == componentInteraction.Guild.Id &&
                                                                                e.ChanelID == componentInteraction.Channel.Id &&
                                                                                e.MessageID == componentInteraction.Message.Id);

                    if (election == null)
                        return;

                    ulong[] allowedRolesID = election.RoleSetups.Select(x => x.Roles.RoleDiscordID).ToArray();

                    Vote? lastVote = election.Votes.LastOrDefault(v => v.MemberID == componentInteraction.User.Id);

                    var member = await componentInteraction.Guild.GetMemberAsync(componentInteraction.User.Id);

                    bool isAllowed = false;

                    foreach(var role in member.Roles)
                    {
                        if (allowedRolesID.Contains(role.Id))
                        {
                            isAllowed = true;
                            break;
                        }
                    }

                    if (isAllowed == false)
                        return;

                    switch (componentInteraction.Id)
                    {
                        case "EL_APROVE":
                            if(lastVote == null || lastVote.VoteValue == false)
                            {
                                dbContext.Votes.Add(new Vote()
                                {
                                    MemberID = componentInteraction.User.Id,
                                    Election = election,
                                    VoteDateTime = DateTime.UtcNow,
                                    VoteValue = true
                                });
                            }
                            else
                            {
                                dbContext.Votes.Add(new Vote()
                                {
                                    MemberID = componentInteraction.User.Id,
                                    Election = election,
                                    VoteDateTime = DateTime.UtcNow,
                                    VoteValue = null
                                });
                            }
                            break;
                        case "EL_DENY":
                            if (lastVote == null || lastVote.VoteValue == true)
                            {
                                dbContext.Votes.Add(new Vote()
                                {
                                    MemberID = componentInteraction.User.Id,
                                    Election = election,
                                    VoteDateTime = DateTime.UtcNow,
                                    VoteValue = false
                                });
                            }
                            else
                            {
                                dbContext.Votes.Add(new Vote()
                                {
                                    MemberID = componentInteraction.User.Id,
                                    Election = election,
                                    VoteDateTime = DateTime.UtcNow,
                                    VoteValue = null
                                });
                            }
                            break;
                        case "EL_UPDATE":
                            break;
                        case "EL_DELETE":
                            break;
                    }






                    dbContext.DisposeAsync();
                    GC.Collect();
                }
            }
        }
    }
}
