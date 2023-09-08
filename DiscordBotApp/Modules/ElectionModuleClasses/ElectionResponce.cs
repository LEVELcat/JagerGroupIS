using DbLibrary.JagerDsModel;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
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
                    case "EL_DELETE":
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

                messageBuilder.ClearComponents();
                messageBuilder.AddComponents(ElectionFactory.ReturnButtonComponents());

                componentInteraction.Message.ModifyAsync(messageBuilder);

                dbContext.DisposeAsync();
                GC.Collect();
            }
        }
    }

}
