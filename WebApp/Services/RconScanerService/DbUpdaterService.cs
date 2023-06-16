namespace WebApp.Services.RconScanerService
{
    public class DbUpdaterService
    {
        private TimeSpan defaultDelay = new TimeSpan(0, 1, 0, 0, 0);

        CancellationTokenSource cancelTokenSource;

        public void StartCycles() => StartCycles(defaultDelay);

        public async void StartCycles(TimeSpan DelayBetwenCycles)
        {
            EndCycles();

            cancelTokenSource = new CancellationTokenSource();

            UpdateCycle(DelayBetwenCycles, cancelTokenSource);
        }

        public void EndCycles()
        {
            cancelTokenSource?.Cancel();
        }

        private async Task UpdateCycle(TimeSpan Delay, CancellationTokenSource CancellationTokenSource)
        {
            CancellationToken token = CancellationTokenSource.Token;

            while (token.IsCancellationRequested == false)
            {
                await WebApp.Application.Services.GetService<RconUpdaterService>().UpdateStatisticDB(cancelTokenSource);

                await Task.Delay(Delay);
            }

            Console.WriteLine("Cycle Is Terminated");

            CancellationTokenSource.Dispose();
        }
    }
}
