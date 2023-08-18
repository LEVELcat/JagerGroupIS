using DbLibrary.JagerDsModel;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Numerics;

namespace DiscordBotApp.Modules.DiscordElectionNotificateClasses
{
    public class ElectionNotificate
    {

        public async Task SendNotificationToPeopleAsync(CancellationTokenSource CancellationTokenSource)
        {
            CancellationToken token = CancellationTokenSource.Token;
            var logger = DiscordApp.DiscordBot.Client.Logger;

            logger.LogDebug("Получение списка актуальных голосовалок");

            Election[] elections = null;
            //TimeSpan

            using (JagerDbContext electionContext = new JagerDbContext())
            {
                elections = await (from el in electionContext.Elections
                                   where el.BitMaskSettings >= BitMaskElection.NotificationBefore_1Mounth
                                   select el).AsNoTracking().ToArrayAsync();

                logger.LogInformation("Список событий получен");

                logger.LogDebug("Освобождение ресурсов electionContext");

                await electionContext.DisposeAsync();
            }

            Dictionary<uint, BitMaskElection> modifyedBitMask = new Dictionary<uint, BitMaskElection>();

            foreach (var election in elections)
            {
                if ((election.BitMaskSettings.HasFlag(BitMaskElection.NotificationForAgree) &&
                     election.BitMaskSettings.HasFlag(BitMaskElection.NotificationForNotVoted)
                    ) == false)
                    continue;

                BitMaskElection newBitmask = election.BitMaskSettings;

                var settingsMaskes = Enum.GetValues(typeof(BitMaskElection));
                Array.Reverse(settingsMaskes);

                bool notificationWasSended = false;
                TimeSpan substractTime;

                foreach (BitMaskElection settingMask in settingsMaskes)
                {
                    if (election.BitMaskSettings.HasFlag(settingMask))
                    {
                        switch (settingMask)
                        {
                            case BitMaskElection.NotificationBefore_15Minutes:
                                substractTime = new TimeSpan(0, 15, 0);
                                if (election.EndTime.Value.Subtract(substractTime).CompareTo(DateTime.Now) <= 0)
                                {
                                    newBitmask &= (~settingMask);
                                    notificationWasSended = true;

                                    SendMessageNotificationToEventAsync(election, true, eventIsNow: true);
                                }
                                break;

                            case BitMaskElection.NotificationBefore_1Hour:
                                substractTime = new TimeSpan(1, 0, 0);
                                CheckParams(substractTime, election, ref newBitmask, settingMask, false, ref notificationWasSended);
                                break;

                            case BitMaskElection.NotificationBefore_2Hour:
                                substractTime = new TimeSpan(2, 0, 0);
                                CheckParams(substractTime, election, ref newBitmask, settingMask, false, ref notificationWasSended);
                                break;

                            case BitMaskElection.NotificationBefore_6Hour:
                                substractTime = new TimeSpan(6, 0, 0);
                                CheckParams(substractTime, election, ref newBitmask, settingMask, false, ref notificationWasSended);
                                break;

                            case BitMaskElection.NotificationBefore_12Hour:
                                substractTime = new TimeSpan(12, 0, 0);
                                CheckParams(substractTime, election, ref newBitmask, settingMask, false, ref notificationWasSended);
                                break;

                            case BitMaskElection.NotificationBefore_24Hour:
                                substractTime = new TimeSpan(1, 0, 0, 0);
                                CheckParams(substractTime, election, ref newBitmask, settingMask, true, ref notificationWasSended);
                                break;

                            case BitMaskElection.NotificationBefore_48Hour:
                                substractTime = new TimeSpan(2, 0, 0, 0);
                                CheckParams(substractTime, election, ref newBitmask, settingMask, false, ref notificationWasSended);
                                break;

                            case BitMaskElection.NotificationBefore_1Week:
                                substractTime = new TimeSpan(7, 0, 0, 0);
                                CheckParams(substractTime, election, ref newBitmask, settingMask, false, ref notificationWasSended);
                                break;

                            case BitMaskElection.NotificationBefore_2Week:
                                substractTime = new TimeSpan(14, 0, 0, 0);
                                CheckParams(substractTime, election, ref newBitmask, settingMask, false, ref notificationWasSended);
                                break;

                            case BitMaskElection.NotificationBefore_1Mounth:
                                substractTime = new TimeSpan(30, 0, 0, 0);
                                CheckParams(substractTime, election, ref newBitmask, settingMask, false, ref notificationWasSended);
                                break;
                        }
                    }
                }

                if (newBitmask != election.BitMaskSettings)
                    modifyedBitMask.Add(election.ID, newBitmask);
            }

            using (JagerDbContext electionBitMaskContext = new JagerDbContext())
            {
                var changedElections = await (from e in electionBitMaskContext.Elections
                                       where modifyedBitMask.Keys.Contains(e.ID)
                                       select e).ToArrayAsync();

                foreach(var elec in changedElections)
                    elec.BitMaskSettings = modifyedBitMask[elec.ID];

                await electionBitMaskContext.SaveChangesAsync();

                electionBitMaskContext.DisposeAsync();
            }

            //TODO: поменяй маску

            GC.Collect();

            void CheckParams(TimeSpan BeforeTime, Election election, ref BitMaskElection newBitmask, BitMaskElection settingMask, bool sendForAgreeUser, ref bool notificationWasSended)
            {
                if (election.EndTime.Value.Subtract(BeforeTime).CompareTo(DateTime.Now) <= 0)
                {
                    newBitmask &= (~settingMask);
                    if (notificationWasSended == false)
                    {
                        notificationWasSended = true;
                        SendMessageNotificationToEventAsync(election, sendForAgreeUser);
                    }

                }
            }
        }

        private async Task SendMessageNotificationToEventAsync(Election election, bool sendForAgreeUser, bool eventIsNow = false)
        {

            DiscordClient? client = null;

            DiscordGuild guild = null;
            DiscordChannel channel = null;
            DiscordMessage message = null;

            string title = string.Empty;

            try
            {
                client = DiscordApp.DiscordBot.Client;
                guild = await client.GetGuildAsync(election.GuildID);
                channel = guild.GetChannel(election.ChanelID);
                message = await channel.GetMessageAsync(election.MessageID);
                title = message.Embeds[0].Title;
            }
            catch
            {
                return;
            }

            ulong[] includeRolesID = null;
            ulong[] excludeRolesID = null;

            string[] botSpeechForBadGuy = null;
            Random random = null;

            List<DiscordMember> banList = new List<DiscordMember>();

            if (eventIsNow)
            {
                random = new Random();

                using (JagerDbContext speechContext = new JagerDbContext())
                {
                    botSpeechForBadGuy = await (from t in speechContext.TextsForBadGuy
                                                select t.BotSpeech).AsNoTracking().ToArrayAsync();

                    speechContext.DisposeAsync();
                }

            }

            using (JagerDbContext roleSetupsContext = new JagerDbContext())
            {
                includeRolesID = await (from r in roleSetupsContext.RoleSetups.AsNoTracking()
                                        where r.ElectionID == election.ID && r.IsTakingPart == true
                                        select r.Roles.RoleDiscordID).ToArrayAsync();

                excludeRolesID = await (from r in roleSetupsContext.RoleSetups.AsNoTracking()
                                        where r.ElectionID == election.ID && r.IsTakingPart == false
                                        select r.Roles.RoleDiscordID).ToArrayAsync();

                roleSetupsContext.DisposeAsync();
            }

            var members = (from m in await (await client.GetGuildAsync(election.GuildID)).GetAllMembersAsync()
                           let rolesId = m.Roles.Select(r => r.Id).ToArray()
                           where
                                (Array.Exists<ulong>(rolesId, r => includeRolesID.Contains(r)) == true)
                                &&
                                (Array.Exists<ulong>(rolesId, r => excludeRolesID.Contains(r)) == false)
                           select m).ToArray();

            using (JagerDbContext voteContext = new JagerDbContext())
            {
                if (election.BitMaskSettings.HasFlag(BitMaskElection.NotificationForNotVoted))
                {
                    var votes = (from v in voteContext.Votes
                                 where v.ElectionID == election.ID
                                 orderby v.VoteDateTime
                                 group v by v.MemberID).AsNoTracking().ToArray();

                    var IDsOfVoted = (from v in votes
                                      let vL = v.Last()
                                      where vL.VoteValue != null
                                      select vL.MemberID).ToArray();

                    var notVotedMember = members.Where(m => IDsOfVoted.Contains(m.Id) == false);

                    List<ulong> notVotedID = new List<ulong>();

                    foreach (var member in notVotedMember)
                    {
                        try
                        {
                            if (eventIsNow)
                            {
                                notVotedID.Add(member.Id);
                                var banCheck = member.SendMessageAsync(notificationForNotVoted(true)).Result;
                            }
                            else
                            {
                                var banCheck = member.SendMessageAsync(notificationForNotVoted()).Result;
                            }
                        }
                        catch (Exception ex)
                        {
                            //banList.Add(member);
                        }
                    }

                    using(JagerDbContext badGuyContext  = new JagerDbContext())
                    {
                        foreach(var id in notVotedID)
                        {
                            badGuyContext.BadGuys.Add(
                                new BadGuy()
                                {
                                    ElectionID = election.ID,
                                    DiscordMemberID = id
                                });
                        }

                        badGuyContext.DisposeAsync();
                    }
                }

                if (sendForAgreeUser && (election.BitMaskSettings.HasFlag(BitMaskElection.NotificationForAgree) == true))
                {
                    var votes = (from v in voteContext.Votes
                                 where v.ElectionID == election.ID
                                 orderby v.VoteDateTime
                                 group v by v.MemberID).ToArray();


                    var IDsOfAgreeVote = (from v in votes
                                          let vL = v.Last()
                                          where vL.VoteValue == true
                                          select vL.MemberID).ToArray();

                    var agreeVotedMember = members.Where(m => IDsOfAgreeVote.Contains(m.Id) == true);

                    foreach (var member in agreeVotedMember)
                    {
                        try
                        {
                            if (eventIsNow)
                            {
                                var banCheck = member.SendMessageAsync(notificationForVoted(true)).Result;
                            }
                            else
                            {
                                var banCheck = member.SendMessageAsync(notificationForVoted()).Result;
                            }
                        }
                        catch (Exception ex)
                        {
                            //banList.Add(member);
                        }
                    }
                }

                voteContext.DisposeAsync();
            }

            if(banList.Count > 0)
            {
                DiscordMessageBuilder builder = new DiscordMessageBuilder();

                builder.WithContent("Так-так-так, бота забанили в лс? Ну будем разбираться\n\n" + String.Join('\n', banList.Select(x => x.Mention)));

                channel.SendMessageAsync(builder);
            }



            GC.Collect();

            DiscordMessageBuilder notificationForNotVoted(bool eventIsNow = false)
            {
                DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder();

                if(eventIsNow)
                    messageBuilder.WithContent(botSpeechForBadGuy[random.Next(0, botSpeechForBadGuy.Length)]);
                else
                    messageBuilder.WithContent($"Скоро {Formatter.Timestamp(election.EndTime.Value)} начнется событие {title}\n\nНо вашей метки нет\n Рекомендую проголосовать сейчас,\nчто бы не забыть");
                return messageBuilder;
            }

            DiscordMessageBuilder notificationForVoted(bool eventIsNow = false)
            {
                DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder();

                if(eventIsNow)
                    messageBuilder.WithContent($"Сейчас ({Formatter.Timestamp(election.EndTime.Value)}) начнется событие {title}\nБлагодарим что пользуетесь нашей напоминалкой");
                else
                    messageBuilder.WithContent($"Скоро {Formatter.Timestamp(election.EndTime.Value)} начнется событие {title}\nБлагодарим что пользуетесь нашей напоминалкой");
                return messageBuilder;
            }
        }

    }
}
