﻿using DSharpPlus.CommandsNext;
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

            Console.WriteLine(DiscordApp.DiscordBot.Client);
        }
#endif
        class DropDownMaker
        {
            public async void MakeMessage(CommandContext ctx, params string[] values)
            {
                //var message = await ctx.Member.SendMessageAsync(await GiveDropdown(ctx, values));
                var message = await ctx.Channel.SendMessageAsync(await GiveDropdown(ctx, values));

                var res = await message.WaitForSelectAsync("drop", TimeSpan.FromSeconds(20));

                ctx.Message.DeleteAsync();
                //message.DeleteAsync();
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

                //messageBuilder.AddComponents(dropdown);

                var roles = new DiscordRoleSelectComponent("afsfa", "ffff");

                messageBuilder.AddComponents(roles);

                messageBuilder.AddComponents(new DiscordButtonComponent(DSharpPlus.ButtonStyle.Success, "test", "sending", false));

                return messageBuilder;
            }


        }

        [Command("drops")]
        public async Task  SendDrop(CommandContext ctx, params string[] values)
        {
            new DropMaker().MakeMessage(ctx, values);
        }

        class DropMaker
        {
            public async void MakeMessage(CommandContext ctx, params string[] values)
            {
                DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder();

                //var roles = ctx.Guild.Roles.ToArray();

                //var option = new List<DiscordSelectComponentOption>();

                //foreach(var role in roles) 
                //{
                //    option.Add(
                //        new DiscordSelectComponentOption(
                //            role.Value.Name, 
                //            role.Value.Id.ToString()
                //            ));
                //}

                var dropdown = new DiscordRoleSelectComponent("roles", null);

                //var dropdown = new DiscordSelectComponent("roles", null, option, minOptions: 1, maxOptions: roles.Length);

                messageBuilder.WithContent("asda");
                messageBuilder.AddComponents(dropdown);

                var message = await ctx.Channel.SendMessageAsync(messageBuilder);

                var res = await message.WaitForSelectAsync("roles", TimeSpan.FromMinutes(2));

                ctx.Message.DeleteAsync();
                //message.DeleteAsync();
            }




        }

        
    }
}