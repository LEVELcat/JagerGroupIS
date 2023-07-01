using Microsoft.Extensions.Logging;

namespace WebApp.Services.RconScanerService
{
    public class DbUpdaterService
    {
        private TimeSpan defaultDelay = new TimeSpan(0, 1, 0, 0, 0);

        CancellationTokenSource cancelTokenSource;

        public void StartCycles() => StartCycles(defaultDelay);

        public async void StartCycles(TimeSpan DelayBetwenCycles)
        {
            ILogger logger = WebApp.Application.Services.GetService<ILogger>();

            EndCycles();

            cancelTokenSource = new CancellationTokenSource();

            logger.LogDebug("Запуск цикла");
            UpdateCycle(DelayBetwenCycles, cancelTokenSource);
        }

        public void EndCycles()
        {
            ILogger logger = WebApp.Application.Services.GetService<ILogger>();

            logger.LogDebug("Остановка цикла");
            cancelTokenSource?.Cancel();
        }

        private async Task UpdateCycle(TimeSpan Delay, CancellationTokenSource CancellationTokenSource)
        {
            ILogger logger = WebApp.Application.Services.GetService<ILogger>();

            CancellationToken token = CancellationTokenSource.Token;

            while (token.IsCancellationRequested == false)
            {
                await WebApp.Application.Services.GetService<RconUpdaterService>().UpdateStatisticDB(cancelTokenSource);

                await Task.Delay(Delay);
            }

            logger.LogDebug("Остановка цикла");

            CancellationTokenSource.Dispose();
        }
    }
}
