using DbLibrary.JagerDsModel;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace DiscordBotApp.Modules.ElectionModuleClasses
{
    public class ElectionResponce
    {
        public async Task Responce(ComponentInteractionCreateEventArgs componentInteraction)
        {
            using (JagerDbContext dbContext = new JagerDbContext())
            {
                Election? election = dbContext.Elections.FirstOrDefault(e => e.GuildID == componentInteraction.Guild.Id &&
                                                                            e.ChanelID == componentInteraction.Channel.Id &&
                                                                            e.MessageID == componentInteraction.Message.Id);

                if (election == null)
                    return;

                ulong[] includedRolesID = election.RoleSetups.Where(x => x.IsTakingPart == true).Select(x => x.Roles.RoleDiscordID).ToArray();
                ulong[] excludedRolesID = election.RoleSetups.Where(x => x.IsTakingPart == false).Select(x => x.Roles.RoleDiscordID).ToArray();

                Vote? lastVote = election.Votes.LastOrDefault(v => v.MemberID == componentInteraction.User.Id);

                var member = await componentInteraction.Guild.GetMemberAsync(componentInteraction.User.Id);

                bool isAllowed = false;


                var idRoldes = member.Roles.Select(x => x.Id).ToArray();

                if (Array.Exists<ulong>(idRoldes, r => includedRolesID.Contains(r)) == true)
                    if (Array.Exists<ulong>(idRoldes, r => excludedRolesID.Contains(r)) == false)
                        isAllowed = true;


                if (isAllowed == false)
                    return;

                switch (componentInteraction.Id)
                {
                    case "EL_APROVE":
                        if (lastVote == null || lastVote.VoteValue == false || lastVote.VoteValue == null)
                        {
                            dbContext.Votes.Add(new Vote()
                            {
                                MemberID = componentInteraction.User.Id,
                                Election = election,
                                VoteDateTime = DateTime.UtcNow,
                                VoteValue = true
                            });
                            await dbContext.SaveChangesAsync();
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
                            await dbContext.SaveChangesAsync();
                        }
                        break;
                    case "EL_DENY":
                        if (lastVote == null || lastVote.VoteValue == true || lastVote.VoteValue == null)
                        {
                            dbContext.Votes.Add(new Vote()
                            {
                                MemberID = componentInteraction.User.Id,
                                Election = election,
                                VoteDateTime = DateTime.UtcNow,
                                VoteValue = false
                            });
                            await dbContext.SaveChangesAsync();
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
                            await dbContext.SaveChangesAsync();
                        }
                        break;
                    case "EL_UPDATE":
                        break;
                    case "EL_DAYOFF":

                        try
                        {
                            componentInteraction.Interaction.CreateResponseAsync(InteractionResponseType.Modal, _GetBuilder());

                            var input = DiscordApp.DiscordBot.Client.GetInteractivity();

                            var txtResponce = await input.WaitForModalAsync("el_responce4", TimeSpan.FromMinutes(5));

                            if (txtResponce.TimedOut)
                                return;

                            var values = txtResponce.Result.Values;
                            var txt = values["text"];

                            txtResponce.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);

                            var chanel = componentInteraction.Guild.GetChannel(1143478141071929364);

                            await chanel.SendMessageAsync(_GetDayOffBuilderMessage());

                            var vote = dbContext.Votes.FirstOrDefaultAsync(x => x.ElectionID == election.ID && x.MemberID == componentInteraction.User.Id && x.VoteValue != false);

                            if (vote != null)
                                goto case "EL_DENY";

                            DiscordMessageBuilder _GetDayOffBuilderMessage()
                            {
                                DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder();

                                DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder();

                                discordEmbed.Title = "Отгул";

                                discordEmbed.WithDescription(string.Empty);

                                discordEmbed.Description += componentInteraction.User.Mention + "\n";
                                discordEmbed.Description += "На событие: " + componentInteraction.Channel.Mention + "\n";
                                discordEmbed.Description += "По причние:\n";
                                discordEmbed.Description += txt;

                                messageBuilder.AddEmbed(discordEmbed);

                                return messageBuilder;
                            }

                            DiscordInteractionResponseBuilder _GetBuilder()
                            {
                                DiscordInteractionResponseBuilder responseBuilder = new DiscordInteractionResponseBuilder();

                                responseBuilder.WithTitle("Оформить отгул");
                                responseBuilder.WithCustomId("el_responce4");

                                responseBuilder.AddComponents(new TextInputComponent("Причина для отгула", "text", value: string.Empty));

                                return responseBuilder;
                            }
                        }
                        catch (Exception ex) 
                        {
                            Console.WriteLine(ex.Message);
                            Console.WriteLine(ex.ToString());
                        }

                        

                        break;
                }


                DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder(componentInteraction.Message);

                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder(messageBuilder.Embed);

                //PROBLEM FIXER
                embedBuilder.ClearFields();
                embedBuilder.AddField("<:emoji_134:941666424324239430> ", "ㅤ", true);
                embedBuilder.AddField("<:1_:941666407513473054> ", "ㅤ", true);
                embedBuilder.AddField("<a:load:1112311359548444713>  ", "ㅤ", true);

                embedBuilder.AddField("ㅤ", "ㅤ", true);
                embedBuilder.AddField("ㅤ", "ㅤ", true);
                embedBuilder.AddField("ㅤ", "ㅤ", true);

                embedBuilder.AddField("ㅤ", "ㅤ", true);
                embedBuilder.AddField("ㅤ", "ㅤ", true);
                embedBuilder.AddField("ㅤ", "ㅤ", true);

                byte columnIndex = 0;
                const byte maxRows = 3;
                const byte mentionsInField = 40;

                var fullMembers = componentInteraction.Guild.Members.ToArray();

                var members = (from m in fullMembers
                               let rolesId = m.Value.Roles.Select(r => r.Id).ToArray()
                               where
                                    (Array.Exists<ulong>(rolesId, r => includedRolesID.Contains(r)) == true) 
                                     && 
                                    (Array.Exists<ulong>(rolesId, r => excludedRolesID.Contains(r)) == false)
                               select new { m.Value.Id, m.Value.Mention }).ToList();

                var votes = (from v in election.Votes
                             orderby v.VoteDateTime
                             group v by v.MemberID).ToArray();

                if (election.BitMaskSettings.HasFlag(BitMaskElection.AgreeList))
                {
                    //embedBuilder.Fields[columnIndex + (rowIndex * 3)].Value = string.Empty;

                    var yesList = (from v in votes
                                   let vL = v.Last()
                                   where vL.VoteValue == true
                                   orderby vL.VoteDateTime
                                   join m in members on vL.MemberID equals m.Id
                                   select new { m.Id, m.Mention }).ToArray();

                    foreach (var v in yesList)
                        members.RemoveAll(m => m.Id == v.Id);


                    embedBuilder.Fields[columnIndex].Name = "<:emoji_134:941666424324239430> " + yesList.Length;

                    for(int i = 0; i < maxRows; i++)
                    {
                        embedBuilder.Fields[columnIndex + (i * 3)].Value = string.Join("\n", yesList.Skip(i * mentionsInField)
                                                                                                    .Take(mentionsInField)
                                                                                                    .Select(x => x.Mention));
                    }

                    columnIndex++;
                }

                if (election.BitMaskSettings.HasFlag(BitMaskElection.RejectList))
                {
                    //embedBuilder.Fields[columnIndex + (rowIndex * 3)].Value = string.Empty;

                    var noList = (from v in votes
                                  let vL = v.Last()
                                  where vL.VoteValue == false
                                  orderby vL.VoteDateTime
                                  join m in members on vL.MemberID equals m.Id
                                  select new { m.Id, m.Mention }).ToArray();

                    foreach (var v in noList)
                        members.RemoveAll(m => m.Id == v.Id);

                    embedBuilder.Fields[columnIndex].Name = "<:1_:941666407513473054> " + noList.Length;

                    for(int i = 0; i < maxRows; i++)
                    {
                        embedBuilder.Fields[columnIndex + (i * 3)].Value = string.Join("\n", noList.Skip(i * mentionsInField)
                                                                                                   .Take(mentionsInField)
                                                                                                   .Select(x => x.Mention));
                    }

                    columnIndex++;
                }

                if (election.BitMaskSettings.HasFlag(BitMaskElection.NotVotedList))
                {
                    embedBuilder.Fields[columnIndex].Name = "<a:load:1112311359548444713> " + members.Count;

                    for(int i = 0; i < maxRows; i++)
                    {
                        embedBuilder.Fields[columnIndex + (i * 3)].Value = string.Join("\n", members.Skip(i * mentionsInField)
                                                                                                    .Take(mentionsInField)
                                                                                                    .Select(x => x.Mention));
                    }
                }

                messageBuilder.Embed = embedBuilder;

                await componentInteraction.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);

                componentInteraction.Message.ModifyAsync(messageBuilder);

                dbContext.DisposeAsync();
                GC.Collect();
            }
        }
    }

}
