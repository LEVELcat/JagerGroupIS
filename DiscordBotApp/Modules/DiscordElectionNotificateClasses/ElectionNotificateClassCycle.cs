using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotApp.Modules.DiscordElectionNotificateClasses
{
    public class ElectionNotificateClassCycle
    {
        private TimeSpan defaultDelay = new TimeSpan(0, 0, 1, 0, 0);

        CancellationTokenSource cancellationTokenSource;

        public async void StartCycles() => StartCycles(defaultDelay);

        public async void StartCycles(TimeSpan? CycleDelay = null, TimeSpan? DelayBeforeStart = null)
        {
            if(CycleDelay == null)
                CycleDelay = defaultDelay;

            if(DelayBeforeStart != null)
                await Task.Delay(DelayBeforeStart.Value);

            StartCycles(CycleDelay);
        }

        public async void StartCycles(TimeSpan DelayBetwenCycles)
        {
            Microsoft.Extensions.Logging.ILogger<DSharpPlus.BaseDiscordClient> logger = DiscordApp.DiscordBot.Client.Logger;

            EndCycles();

            cancellationTokenSource = new CancellationTokenSource();

            logger.LogDebug("Запуск цикла");
        }

        public async void EndCycles()
        {
            Microsoft.Extensions.Logging.ILogger<DSharpPlus.BaseDiscordClient> logger = DiscordApp.DiscordBot.Client.Logger;

            logger.LogDebug("Остановка цикла");
            cancellationTokenSource?.Cancel();
        }

        private async Task UpdateCycle(TimeSpan Delay, CancellationTokenSource cancellationTokenSource)
        {
            Microsoft.Extensions.Logging.ILogger<DSharpPlus.BaseDiscordClient> logger = DiscordApp.DiscordBot.Client.Logger;

            CancellationToken token = cancellationTokenSource.Token;

            while(token.IsCancellationRequested == false)
            {
                new ElectionNotificate().SendNotificationToPeopleAsync(cancellationTokenSource);

                await Task.Delay(Delay);
            }

            logger.LogDebug("Остановка цикла");

            cancellationTokenSource.Dispose();
        }

    }
}
