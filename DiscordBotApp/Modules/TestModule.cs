using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

namespace DiscordBotApp.Commands
{
    [ModuleLifespan(ModuleLifespan.Transient)]
    internal class TestModule : BaseCommandModule
    {
#if DEBUG
        [Command("test")]
        public async Task SendMessage(CommandContext ctx, params string[] values)
        {
            new DropDownMaker().MakeMessage(ctx, values);
        }
#endif
        class DropDownMaker
        {
            public async void MakeMessage(CommandContext ctx, params string[] values)
            {
                var message = await ctx.Member.SendMessageAsync(await GiveDropdown(ctx, values));

                var res = await message.WaitForSelectAsync("drop", TimeSpan.FromSeconds(20));
                ctx.Message.DeleteAsync();
                message.DeleteAsync();



            }

            private async Task<DiscordMessageBuilder> GiveDropdown(CommandContext ctx, params string[] values)
            {
                DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder();

                messageBuilder.WithContent("Вот тебе менюхи");

                var option = new List<DiscordSelectComponentOption>()
                {
                new DiscordSelectComponentOption(
                    "Label, no description",
                    "label_no_desc"),

                new DiscordSelectComponentOption(
                    "Label, Description",
                    "label_with_desc",
                    "This is a description!"),

                new DiscordSelectComponentOption(
                    "Label, Description, Emoji",
                    "label_with_desc_emoji",
                    "This is a description!",
                    emoji: new DiscordComponentEmoji(854260064906117121)),

                new DiscordSelectComponentOption(
                    "Label, Description, Emoji (Default)",
                    "label_with_desc_emoji_default",
                    "This is a description!",
                    isDefault: true,
                    new DiscordComponentEmoji(854260064906117121))
                };

                var dropdown = new DiscordSelectComponent("drop", null, option, false, 1, 2);

                messageBuilder.AddComponents(dropdown);
                messageBuilder.AddComponents(new DiscordButtonComponent(DSharpPlus.ButtonStyle.Success, "test", "sending", false));

                return messageBuilder;
            }


        }

        
    }
}
