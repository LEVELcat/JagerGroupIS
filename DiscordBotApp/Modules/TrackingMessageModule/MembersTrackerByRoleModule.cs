using DbLibrary.MetricsModel;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace DiscordBotApp.Modules.TrackingMessageModule
{
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class MembersTrackerByRoleModule : BaseCommandModule
    {
        [Command("roleTracking")]
        public async Task CreateRoleTracker(CommandContext ctx, DiscordRole roleToBeTracked)
        {
            MembersTrackerByRole.CreateTrackingMessage(ctx.Guild, ctx.Channel, roleToBeTracked);

            ctx.Message.DeleteAsync();
        }
    }

    public class MembersTrackerByRole
    {
        public static async Task CreateTrackingMessage(DiscordGuild Guild, DiscordChannel Channel, DiscordRole Role)
        {
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();

            embedBuilder.Title = "Состав роли:";

            embedBuilder.Description = Role.Mention;

            var message = await Channel.SendMessageAsync(embedBuilder);

            TrackingMessage trackingMessage = new TrackingMessage()
            {
                GuildID = Guild.Id,
                ChanelID = Channel.Id,
                MessageID = message.Id,
                MessageType = MessageType.MembersTrackedByRole
            };

            MessageTrackersManager.AddTracker(trackingMessage);
        }

        public static async Task UpdateMessage(TrackingMessage trackingMessage)
        {
            if (trackingMessage.MessageType == MessageType.MembersTrackedByRole)
            {
                try
                {
                    await UpdateMessage(trackingMessage.GuildID, trackingMessage.ChanelID, trackingMessage.MessageID);
                }
                catch
                {
                    throw new Exception("Сообщение не найдено");
                }
            }
        }

        static async Task UpdateMessage(ulong GuildID, ulong ChanelID, ulong MessageID)
        {
            var guild = await DiscordApp.DiscordBot.Client.GetGuildAsync(GuildID);

            if (guild != null)
            {
                var channel = guild.GetChannel(ChanelID);
                if (channel != null)
                {
                    var message = await channel.GetMessageAsync(MessageID);
                    if (message != null)
                    {
                        UpdateMessage(guild, channel, message);
                        return;
                    }
                }
            }

            throw new Exception("Сообщение не найдено");

        }

        public static async Task UpdateMessage(DiscordGuild Guild, DiscordChannel Channel, DiscordMessage Message)
        {

            DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder(Message);

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder(messageBuilder.Embed);

            var regexObj = new Regex(@"[^\d]");
            var str = regexObj.Replace(embedBuilder.Description, "");
            ulong roleID = ulong.Parse(str);

            var role = Guild.GetRole(roleID);

            var members = (from m in Guild.Members.Values
                           where m.Roles.Contains(role)
                           orderby m.DisplayName
                           select m.Mention).ToArray();

            var listsRequired = ((members.Length - 1) / 40) + 1;

            if (listsRequired != embedBuilder.Fields.Count)
            {
                embedBuilder.ClearFields();

                for (int i = 0; i < listsRequired; i++)
                    embedBuilder.AddField("ㅤ", "empty", true);
            }

            byte chunkID = 0;
            foreach (var chunk in members.Chunk(members.Length / listsRequired + (members.Length % listsRequired == 0 ? 0 : 1)))
            {
                embedBuilder.Fields[chunkID].Value = string.Join("\n", chunk);
                chunkID++;
            }
            messageBuilder.Embed = embedBuilder;

            Message.ModifyAsync(messageBuilder);
        }
    }
}
