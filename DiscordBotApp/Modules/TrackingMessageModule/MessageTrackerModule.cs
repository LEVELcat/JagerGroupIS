using DbLibrary.MetricsModel;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DiscordBotApp.Modules.TrackingMessageModule
{
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class MessageTrackerModule : BaseCommandModule
    {

        [Command("Tracker")]
        public async Task InvokeTrackers(CommandContext ctx, string value)
        {
            ExecuteCommand(ctx, value);

            ctx.Message.DeleteAsync();
        }

        static async Task ExecuteCommand(CommandContext ctx, string value)
        {
            switch (value)
            {
                case "load":
                    MessageTrackersManager.LoadTrackers();
                    break;
                case "break":
                    MessageTrackersManager.StopTrackers();
                    break;
            }
        }
    }

    public class MessageTracker
    {
        public TrackingMessage TrackingMessage { get; set; }

        public CancellationTokenSource CancellationTokenSource { get; set; }

        MessageTracker(TrackingMessage trackingMessage)
        {
            TrackingMessage = trackingMessage;
        }

        public static async Task<MessageTracker> CreateTrackerAsync(TrackingMessage trackingMessage)
        {
            var tracker = new MessageTracker(trackingMessage);
            tracker.CancellationTokenSource = new CancellationTokenSource();
            tracker.TrackerCycle(tracker.CancellationTokenSource);

            return tracker;
        }

        async Task TrackerCycle(CancellationTokenSource cancellationToken)
        {
            try
            {
                var token = cancellationToken.Token;

                while (token.IsCancellationRequested == false)
                {
                    if (TrackingMessage == null)
                        break;

                    switch (TrackingMessage.MessageType)
                    {
                        case MessageType.MembersTrackedByRole:
                            await MembersTrackerByRole.UpdateMessage(TrackingMessage);
                            break;
                    }
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
            }
            catch
            {
                cancellationToken.Cancel();
                MessageTrackersManager.RemoveTrackers(this);
            }
        }
    }

    public class MessageTrackersManager
    {
        static List<MessageTracker> trackedMessages = new List<MessageTracker>();

        public static async Task LoadTrackers()
        {
            using (MetricsDbContext metricsDbContext = new MetricsDbContext())
            {
                var trackersInBd = await metricsDbContext.TrackingMessages.AsNoTracking().ToArrayAsync();

                foreach(var item in trackersInBd)
                {
                    trackedMessages.Add(await MessageTracker.CreateTrackerAsync(item));
                    await Task.Delay(TimeSpan.FromMinutes(2));
                }
                metricsDbContext.DisposeAsync();
            }
        }

        public static async Task StopTrackers()
        {
            foreach (var track in trackedMessages)
                track.CancellationTokenSource.Cancel();

            trackedMessages.Clear();
        }

        public static async Task RemoveTrackers(MessageTracker messageTracker)
        {
            using(MetricsDbContext metricsDbContext = new MetricsDbContext())
            {
                var dbTracker = await metricsDbContext.TrackingMessages.Where(x => x.ID == messageTracker.TrackingMessage.ID).ToArrayAsync();

                metricsDbContext.TrackingMessages.RemoveRange(dbTracker);

                metricsDbContext.SaveChangesAsync();
            }

            trackedMessages.Remove(messageTracker);
        }

        public static async Task AddTracker(TrackingMessage trackingMessage)
        {
            TrackingMessage? objAsNoTracking = null;

            using (MetricsDbContext metricsDbContext = new MetricsDbContext())
            {

                metricsDbContext.TrackingMessages.Add(trackingMessage);

                await metricsDbContext.SaveChangesAsync();

                objAsNoTracking = await (from t in metricsDbContext.TrackingMessages
                                         where t.ID == trackingMessage.ID
                                         select t).AsNoTracking().FirstOrDefaultAsync();

                metricsDbContext.DisposeAsync();
            }
            if(objAsNoTracking != null)
                trackedMessages.Add(await MessageTracker.CreateTrackerAsync(trackingMessage));
        }
    }
}
