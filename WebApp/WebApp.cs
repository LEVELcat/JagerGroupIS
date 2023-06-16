using System.Globalization;
using WebApp.DbContexts;
using Microsoft.AspNetCore.HttpOverrides;
using WebApp.Services.RconScanerService;

namespace WebApp
{
    public class WebApp
    {
        public static async Task Main(string[] args)
        {
            CultureInfo ci = new CultureInfo("ru-RU");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
            Console.WriteLine("Hello World");

            //DiscordApp.DiscordBot.AsyncMain(args);
            Console.WriteLine("Discord Bot Start");

            AspAppMain(args);
        }

        public static WebApplication Application;

        private static async void AspAppMain(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllersWithViews();


            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
            builder.Services.AddTransient<StatisticDbContext>()
                            .AddSingleton<RconUpdaterService>()
                            .AddSingleton<DbUpdaterService>();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
                app.UseForwardedHeaders();
            }


            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            using (StatisticDbContext db = new StatisticDbContext())
            {

                Console.WriteLine(string.Concat(db.Servers.Select(x => x.Description)));

            }

            app.RunAsync();

            Application = app;

            await ConsoleControlCycle(app);
        }

        enum Command
        {
            None,
            Exit,
            UpdateRconDB,
            BeginUpdateCycles,
            CloseUpdateCycles
        }

        static Dictionary<Command, string[]> commandDict = new Dictionary<Command, string[]>
        {
            { Command.None, new[] { "" } }, //ALWAYS MUST BE FIRST IN COLLECTION
            { Command.Exit, new[] {"exit", "close", "shutdown", "ex"} },
            { Command.UpdateRconDB, new [] { "update", "up" } },
            { Command.BeginUpdateCycles, new [] { "cycle_start", "cS" } },
            { Command.CloseUpdateCycles, new [] { "cycle_end", "cE" } },
        };

        private static async Task ConsoleControlCycle(WebApplication app)
        {
            while (true)
            {
                Console.WriteLine("Введите команду:");
                string cmd = Console.ReadLine();

                switch ( commandDict.FirstOrDefault(x => x.Value.Contains(cmd)).Key)
                {
                    case Command.Exit:
                        await app.StopAsync();
                        goto exitGOTO;
                    case Command.UpdateRconDB:
                        
                        break;
                    case Command.BeginUpdateCycles:
                        app.Services.GetService<DbUpdaterService>()?.StartCycles();
                        break;
                    case Command.CloseUpdateCycles:
                        app.Services.GetService<DbUpdaterService>()?.EndCycles();
                        break;

                    default:
                        Console.WriteLine($"Команда {cmd} отсуствует");
                        break;
                }
            }

        exitGOTO:

            Console.WriteLine("exit from app");
        }
    }
}