namespace DiscordBotApp.Modules
{

#if DEBUG
    [ModuleLifespan(ModuleLifespan.Transient)]
    internal class TestModule : BaseCommandModule
    {

        [Command("test")]
        public async Task SendMessage(CommandContext ctx, params string[] values)
        {
            new DropDownMaker().MakeMessage(ctx, values);
        }

    class DropDownMaker
        {
            public async void MakeMessage(CommandContext ctx, params string[] values)
            {
                try
                {
                    DiscordMessageBuilder builder = new DiscordMessageBuilder();

                    builder.WithContent("Test");

                    builder.AddComponents(new DiscordButtonComponent(DSharpPlus.ButtonStyle.Primary, "but", "press"));

                    var mess = await ctx.Channel.SendMessageAsync(builder);

                    var resp = await mess.WaitForButtonAsync(TimeSpan.FromSeconds(20));

                    //DiscordFollowupMessageBuilder followupMessageBuilder = new DiscordFollowupMessageBuilder();
                    //followupMessageBuilder.WithContent("test");
                    ////followupMessageBuilder.AddComponents(new DiscordButtonComponent(DSharpPlus.ButtonStyle.Primary, "but", "press"));
                    //var foll = await resp.Result.Interaction.CreateFollowupMessageAsync(followupMessageBuilder);

                    DiscordInteractionResponseBuilder responseBuilder = new DiscordInteractionResponseBuilder();
                    responseBuilder.WithContent("Responce");
                    responseBuilder.AsEphemeral(true);
                    //responseBuilder.AddComponents(new DiscordButtonComponent(DSharpPlus.ButtonStyle.Primary, "but", "press"));

                    //await resp.Result.Interaction.DeferAsync(true);
                    resp.Result.Interaction.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, responseBuilder);

                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    ctx.Channel.SendMessageAsync(ex.Message);
                    ctx.Channel.SendMessageAsync(ex.ToString());
                }


            }
        }
    
    }

#endif
}
