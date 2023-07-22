using DbLibrary.JagerDsModel;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotApp.Modules.ElectionModuleClasses
{
    public class ElectionResponce
    {
        public async Task Responce(ComponentInteractionCreateEventArgs componentInteraction)
        {
            await componentInteraction.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);

            using (JagerDbContext dbContext = new JagerDbContext())
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

                foreach (var role in member.Roles)
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

                byte fieldIndex = 0;

                var members = (from m in componentInteraction.Guild.Members.Values
                               where m.Roles.Any(r => allowedRolesID.Contains(r.Id))
                               select new { m.Id, m.DisplayName }).ToList();

                //IT'S REMOVED ITALICS FONT
                for (int i = 0; i < members.Count; i++)
                    members[i].DisplayName.Replace("_", "\\_");

                if (election.BitMaskSettings.HasFlag(BitMaskElection.AgreeList))
                {
                    embedBuilder.Fields[fieldIndex].Value = string.Empty;

                    var yesList = (from v in election.Votes.GroupBy(d => d.MemberID)
                                   let vL = v.Last()
                                   where vL.VoteValue == true
                                   orderby vL.VoteDateTime
                                   join m in members on vL.MemberID equals m.Id
                                   select new { m.Id, m.DisplayName }).ToArray();

                    foreach (var v in yesList)
                        members.RemoveAll(m => m.Id == v.Id);


                    embedBuilder.Fields[fieldIndex].Name = "<:emoji_134:941666424324239430> " + yesList.Length;
                    embedBuilder.Fields[fieldIndex].Value = string.Join('\n', yesList.Select(n => n.DisplayName).AsEnumerable());
                    fieldIndex++;
                }

                if (election.BitMaskSettings.HasFlag(BitMaskElection.RejectList))
                {
                    embedBuilder.Fields[fieldIndex].Value = string.Empty;

                    var noList = (from v in election.Votes.GroupBy(d => d.MemberID)
                                  let vL = v.Last()
                                  where vL.VoteValue == false
                                  orderby vL.VoteDateTime
                                  join m in members on vL.MemberID equals m.Id
                                  select new { m.Id, m.DisplayName }).ToArray();

                    foreach (var v in noList)
                        members.RemoveAll(m => m.Id == v.Id);

                    embedBuilder.Fields[fieldIndex].Name = "<:1_:941666407513473054> " + noList.Length;
                    embedBuilder.Fields[fieldIndex].Value = string.Join('\n', noList.Select(n => n.DisplayName).AsEnumerable());
                    fieldIndex++;
                }

                if (election.BitMaskSettings.HasFlag(BitMaskElection.NotVotedList))
                {
                    embedBuilder.Fields[fieldIndex].Name = "<a:load:1112311359548444713> " + members.Count;
                    embedBuilder.Fields[fieldIndex].Value = string.Join('\n', members.Select(n => n.DisplayName).AsEnumerable());
                }

                messageBuilder.Embed = embedBuilder;
                componentInteraction.Message.ModifyAsync(messageBuilder);

                dbContext.DisposeAsync();
                GC.Collect();
            }
        }
    }

}
