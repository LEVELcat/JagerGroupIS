using DbLibrary.JagerDsModel;
using DiscordApp;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Logging;

namespace DiscordBotApp.Modules.ElectionModuleClasses
{

    public class ElectionFactory
    {
        public async Task CreateElectionAsync(CommandContext ctx)
        {
            DiscordMessageBuilder messageBuilder = null;

            var election = await ConstructElectionAsync(ctx, messageBuilder);


            //var option = await ConstructElectionAsync(ctx);

            //if (option.Item1 == null) return;

            //DiscordMessageBuilder messageBuilder = option.Item1;
            //messageBuilder.AddComponents(ReturnButtonComponents());

            //Election election = option.Item2;

            //using (JagerDbContext dbContext = new JagerDbContext())
            //{
            //    foreach (var roleSetup in election.RoleSetups)
            //    {
            //        roleSetup.Roles = await FixRolesAsync(roleSetup.Roles);
            //    }

            //    var chanel = await ctx.Guild.CreateChannelAsync(messageBuilder.Embed.Title, ChannelType.Text, ctx.Guild.Channels[election.ChanelID]);

            //    var electionMessage = await chanel.SendMessageAsync(messageBuilder);

            //    election.ChanelID = chanel.Id;
            //    election.MessageID = electionMessage.Id;

            //    dbContext.Elections.Add(election);
            //    await dbContext.SaveChangesAsync();

            //    async Task<Role> FixRolesAsync(Role roles)
            //    {
            //        Role? result = await dbContext.Roles.FirstOrDefaultAsync(r =>
            //                                             r.GuildID == roles.GuildID && r.RoleDiscordID == roles.RoleDiscordID);
            //        if (result == null)
            //        {
            //            result = roles;
            //            dbContext.Roles.Add(roles);
            //        }
            //        return result;
            //    }
            //}

            //DiscordComponent[] ReturnButtonComponents() => new DiscordComponent[]
            //{
            //            new DiscordButtonComponent(ButtonStyle.Success, $"EL_APROVE", string.Empty, emoji: new DiscordComponentEmoji(941666424324239430)),
            //            new DiscordButtonComponent(ButtonStyle.Danger, $"EL_DENY", string.Empty, emoji: new DiscordComponentEmoji(941666407513473054)),
            //            new DiscordButtonComponent(ButtonStyle.Secondary, $"EL_UPDATE", "Обновить список"),
            //            //new DiscordButtonComponent(ButtonStyle.Secondary, $"EL_EDIT", "Редактировать событие"),
            //            new DiscordButtonComponent(ButtonStyle.Secondary, $"EL_DELETE", "🗑️")
            //};
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
            ctx.Message.DeleteAsync();

            Election election = new Election()
            {
                BitMaskSettings = BitMaskElection.AgreeList | BitMaskElection.RejectList | BitMaskElection.NotVotedList,
                GuildID = ctx.Guild.Id

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

                                responseBuilder.WithCustomId("menu1");

                                responseBuilder.AddComponents(new TextInputComponent("Название", "name"));
                                responseBuilder.AddComponents(new TextInputComponent("Дата события", "date", value: DateTime.Now.ToString()));
                                responseBuilder.AddComponents(new TextInputComponent("Описание события", "desc", style: TextInputStyle.Paragraph));
                                responseBuilder.AddComponents(new TextInputComponent("URL большой картинки снизу", "image", required: false));
                                responseBuilder.AddComponents(new TextInputComponent("URL картинки справа", "thumb", required: false));

                                respond.Result.Interaction.CreateResponseAsync(InteractionResponseType.Modal, responseBuilder);

                                var input = ctx.Client.GetInteractivity();

                                var txtResponce = await input.WaitForModalAsync("menu1", TimeSpan.FromMinutes(20));

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
                                responseBuilder2.WithCustomId("menu2");

                                responseBuilder2.AddComponents(new TextInputComponent("Подпись снизу", "footerText", required: false));
                                responseBuilder2.AddComponents(new TextInputComponent("URL Иконки возле подписи", "footerIcon", required: false));
                                responseBuilder2.AddComponents(new TextInputComponent("Цвета слева", "color", value: "#000000", required: false));

                                respond.Result.Interaction.CreateResponseAsync(InteractionResponseType.Modal, responseBuilder2);

                                var input2 = ctx.Client.GetInteractivity();

                                var txtResponce2 = await input2.WaitForModalAsync("menu2", TimeSpan.FromMinutes(20));

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

        class ElectionSettings
        {
            public BitMaskElection BitMaskElection
            {
                get => Election.BitMaskSettings;
                set
                {
                    Election.BitMaskSettings = value;

                    EmbedBuilder.ClearFields();

                    if (BitMaskElection.HasFlag(BitMaskElection.AgreeList))
                        EmbedBuilder.AddField("<:emoji_134:941666424324239430>", "empty", true);

                    if (BitMaskElection.HasFlag(BitMaskElection.RejectList))
                        EmbedBuilder.AddField("<:1_:941666407513473054>", "empty", true);

                    if (BitMaskElection.HasFlag(BitMaskElection.NotVotedList))
                        EmbedBuilder.AddField("<a:load:1112311359548444713>", "empty", true);
                }
            }

            private string title;
            public string Title
            {
                get => title;
                set
                {
                    title = value;
                    EmbedBuilder.Title = value;
                }
            }

            private string description;
            public string Description
            {
                get => description;
                set
                {
                    description = value;
                    EmbedBuilder.Description = FullDescription;
                }
            }

            private DateTime endDate;
            public DateTime EndDate
            {
                get => endDate;
                set
                {
                    endDate = value;
                    Election.EndTime = value;
                    EmbedBuilder.Description = FullDescription;
                }
            }

            private string FullDescription
            {
                get => Description + "\n\n" + EndDate.ToString();
            }

            private string? mainPictureURL;
            public string? MainPictureURL
            {
                get => mainPictureURL;
                set
                {
                    mainPictureURL = value;
                    EmbedBuilder.ImageUrl = MainPictureURL;
                }
            }

            private string? thumbnailPictureURL;
            public string? ThumbnailPictureURL
            {
                get => thumbnailPictureURL;
                set
                {
                    thumbnailPictureURL = value;
                    EmbedBuilder.Thumbnail.Url = ThumbnailPictureURL;
                }
            }

            private DiscordColor color;
            public DiscordColor Color
            {
                get => color;
                set
                {
                    color = value;
                    EmbedBuilder.Color = Color;
                }
            }

            private string? footerText;
            public string? FooterText
            {
                get => footerText;
                set
                {
                    footerText = value;
                    EmbedBuilder.WithFooter(FooterText);
                }
            }

            private string? footerUrl;
            public string? FooterUrl
            {
                get => footerUrl;
                set
                {
                    footerUrl = value;
                    if (EmbedBuilder.Footer != null)
                        EmbedBuilder.WithFooter(FooterText, FooterUrl);
                }
            }

            private DateTime? timeStamp;
            public DateTime? TimeStamp
            {
                get => timeStamp;
                set
                {
                    timeStamp = value;
                    EmbedBuilder.WithTimestamp(timeStamp);
                }
            }

            public Election Election { get; private set; } = new Election();

            public DiscordEmbedBuilder EmbedBuilder { get; private set; } = new DiscordEmbedBuilder();

            private DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder();
            public DiscordMessageBuilder MessageBuilder
            {
                get
                {
                    messageBuilder.Embed = EmbedBuilder;
                    return messageBuilder;
                }
                private set => messageBuilder = value;
            }

            private DiscordMessage roleSelectMessage;
            private DiscordMessage RoleSelectMessage
            {
                get => roleSelectMessage;
                set
                {
                    if (roleSelectMessage != null)
                        roleSelectMessage.DeleteAsync();

                    roleSelectMessage = value;
                }
            }

            private DiscordMessage electionListMessage;
            private DiscordMessage ElectionListMessage
            {
                get => electionListMessage;
                set
                {
                    if (roleSelectMessage != null)
                        roleSelectMessage.DeleteAsync();

                    roleSelectMessage = value;
                }
            }

            private DiscordMessage chanelSelectMessage;
            private DiscordMessage ChanelSelectMessage
            {
                get => chanelSelectMessage;
                set
                {
                    if (chanelSelectMessage != null)
                        chanelSelectMessage.DeleteAsync();

                    chanelSelectMessage = value;
                }
            }

            //TODO: Сделать
            public bool IsReadyToPublish
            {
                get
                {
                    return false;
                }
            }

            DiscordMessage? viewMessage { get; set; }


            public ElectionSettings(CommandContext ctx)
            {
                Election.GuildID = ctx.Guild.Id;

                SetDefaultSettings();
            }

            private void SetDefaultSettings()
            {
                Title = "Название";
                Description = "Описание";
                EndDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 20, 00, 00);
                BitMaskElection = BitMaskElection.AgreeList | BitMaskElection.RejectList | BitMaskElection.NotVotedList;
                Color = new DiscordColor("#FFFFFF");
            }

            private async void EditViewMessage()
            {
                viewMessage?.ModifyAsync(MessageBuilder);
            }

            public async void ShowViewMessage(CommandContext ctx)
            {
                if (viewMessage == null)
                {
                    viewMessage = ctx.Channel.SendMessageAsync(MessageBuilder).Result;
                }
                else
                {
                    EditViewMessage();
                }
            }

            public async void DeleteViewMessageAsync()
            {
                viewMessage?.DeleteAsync();
            }

            public async void ShowMainSettingInteraction(CommandContext ctx, InteractivityResult<ComponentInteractionCreateEventArgs> eventArgs)
            {
                eventArgs.Result.Interaction.CreateResponseAsync(InteractionResponseType.Modal, _GetBulder());

                var input = ctx.Client.GetInteractivity();

                var txtResponce = await input.WaitForModalAsync("menu1", TimeSpan.FromMinutes(20));

                if (txtResponce.TimedOut)
                    return;

                var values = txtResponce.Result.Values;
                var errorResponceStr = string.Empty;

                if (values["name"] != string.Empty)
                    this.Title = values["name"];

                if (values["desc"] != string.Empty)
                    this.Description = values["desc"];

                if (values["date"] != string.Empty)
                    if (DateTime.TryParse(values["date"], out DateTime result))
                        this.EndDate = result;
                    else
                        errorResponceStr += "Некорректная дата\n";

                if (values["image"] != string.Empty)
                    this.MainPictureURL = values["image"];

                if (values["thumb"] != string.Empty)
                    this.ThumbnailPictureURL = values["image"];

                if (errorResponceStr == string.Empty)
                    txtResponce.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);
                else
                    CreateErrorResponceAsync(txtResponce, errorResponceStr);

                EditViewMessage();

                DiscordInteractionResponseBuilder _GetBulder()
                {
                    DiscordInteractionResponseBuilder responseBuilder = new DiscordInteractionResponseBuilder();

                    responseBuilder.WithTitle("Настройки #1");

                    responseBuilder.WithCustomId("menu1");

                    responseBuilder.AddComponents(new TextInputComponent("Название", "name", value: this.Title));
                    responseBuilder.AddComponents(new TextInputComponent("Дата события", "date", value: this.EndDate.ToString()));
                    responseBuilder.AddComponents(new TextInputComponent("Описание события", "desc", value: this.Description, style: TextInputStyle.Paragraph));
                    responseBuilder.AddComponents(new TextInputComponent("URL большой картинки снизу", "image", required: false,
                                                                         value: MainPictureURL != null ? MainPictureURL : null));
                    responseBuilder.AddComponents(new TextInputComponent("URL картинки справа", "thumb", required: false,
                                                                         value: ThumbnailPictureURL != null ? ThumbnailPictureURL : null));

                    return responseBuilder;
                }
            }

            public async void ShowOtherSettingInteraction(CommandContext ctx, InteractivityResult<ComponentInteractionCreateEventArgs> eventArgs)
            {
                eventArgs.Result.Interaction.CreateResponseAsync(InteractionResponseType.Modal, _GetBulder());

                var input = ctx.Client.GetInteractivity();

                var txtResponce = await input.WaitForModalAsync("menu2", TimeSpan.FromMinutes(20));

                if (txtResponce.TimedOut)
                    return;

                var values = txtResponce.Result.Values;
                var errorString = string.Empty;

                if (values["footerText"] != string.Empty)
                    this.FooterText = values["footerText"];

                if (values["footerIcon"] != string.Empty)
                    this.FooterUrl = values["footerIcon"];


                try
                {
                    if (values["color"] != string.Empty)
                        Color = new DiscordColor(values["color"]);
                }
                catch
                {
                    errorString += "Некорректный Hex код цвета\n";
                }

                if (errorString == string.Empty)
                    txtResponce.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);
                else
                    CreateErrorResponceAsync(txtResponce, errorString);


                EditViewMessage();

                DiscordInteractionResponseBuilder _GetBulder()
                {
                    DiscordInteractionResponseBuilder responseBuilder = new DiscordInteractionResponseBuilder();

                    responseBuilder.WithTitle("Настройки #1");

                    responseBuilder.WithCustomId("menu2");

                    responseBuilder.AddComponents(new TextInputComponent("Подпись снизу", "footerText", required: false,
                                                                         value: FooterText != null ? FooterText : null));
                    responseBuilder.AddComponents(new TextInputComponent("URL Иконки возле подписи", "footerIcon", required: false,
                                                                         value: FooterUrl != null ? FooterUrl : null));
                    responseBuilder.AddComponents(new TextInputComponent("Цвета слева", "color", required: false,
                                                                         value: $"#{Color.Value.ToString("X")}"));

                    return responseBuilder;
                }
            }

            public async void ShowRoleSelectSettingMessage(CommandContext ctx, InteractivityResult<ComponentInteractionCreateEventArgs> eventArgs)
            {
                //eventArgs.Result.Interaction.CreateResponseAsync(InteractionResponseType.);

                DiscordInteractionResponseBuilder _GetBuilder()
                {
                    DiscordInteractionResponseBuilder builder = new DiscordInteractionResponseBuilder();

                    return builder;
                }
            }

            public async void ShowElectionListSettingMessage(CommandContext ctx, InteractivityResult<ComponentInteractionCreateEventArgs> eventArgs)
            {

            }

            public async void ShowChanelSelectSettingMessage(CommandContext ctx, InteractivityResult<ComponentInteractionCreateEventArgs> eventArgs)
            {

            }

            private async void CreateErrorResponceAsync(InteractivityResult<ModalSubmitEventArgs> txtResponce, string errorString)
            {
                DiscordInteractionResponseBuilder responseBuilder = new DiscordInteractionResponseBuilder();
                responseBuilder.WithContent(errorString);
                responseBuilder.AsEphemeral();

                txtResponce.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, responseBuilder);
            }


            //TODO: Сделать
            public static bool TryParse(DiscordMessageBuilder discordMessage, out ElectionSettings electionSettings)
            {
                electionSettings = null;
                return false;
            }

            public static DiscordMessageBuilder DefaultMenuPanelMessage
            {
                get
                {
                    var result = new DiscordMessageBuilder();

                    result.WithContent("Настройки голосования");
                    result.AddComponents(DefaultButtonMenuRows.AsEnumerable());

                    return result;
                }
            }

            private static DiscordActionRowComponent[] DefaultButtonMenuRows
            {
                get
                {
                    return new DiscordActionRowComponent[]
                    {
                        new DiscordActionRowComponent(_firstPart()),
                        new DiscordActionRowComponent(_secondPart())
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
        }

        private async Task<Election> ConstructElectionAsync(CommandContext ctx, DiscordMessageBuilder outMessageBuilder)
        {

            ctx.Message.DeleteAsync();

            ElectionSettings electionSettings = new ElectionSettings(ctx);

            electionSettings.ShowViewMessage(ctx);
            var menuMessage = await ctx.Channel.SendMessageAsync(ElectionSettings.DefaultMenuPanelMessage);

            bool isExit = false;

            for (bool isFinished = false; isFinished == false;)
            {
                var respond = await menuMessage.WaitForButtonAsync(TimeSpan.FromMinutes(30));

                if (respond.TimedOut == true)
                    isExit = true;
                else
                {

                    switch (respond.Result.Id)
                    {
                        case "menu1":
                            electionSettings.ShowMainSettingInteraction(ctx, respond);
                            break;
                        case "menu2":
                            electionSettings.ShowOtherSettingInteraction(ctx, respond);
                            break;
                        case "menu3":
                            break;
                        case "menu4":
                            break;
                        case "menu5":
                            break;
                        case "menu8":
                            isFinished = true;
                            break;
                        case "menu9":
                            isExit = true;
                            break;

                    }
                }
                if (isExit == true)
                    break;
            }

            if (isExit == true)
            {
                electionSettings.DeleteViewMessageAsync();
                menuMessage.DeleteAsync();
                return null;
            }
            else
            {
                outMessageBuilder = electionSettings.MessageBuilder;
                return electionSettings.Election;
            }
        }
    }
}

