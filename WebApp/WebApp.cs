using Microsoft.AspNetCore.HttpOverrides;
using System.Data.Entity;
using System.Globalization;
using WebApp.DbContexts;
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

            DiscordApp.DiscordBot.AsyncMain(args);
            Console.WriteLine("Discord Bot Start");

            AspAppMain(args);


            Console.ReadKey();
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
            builder.Services.AddSingleton<RconUpdaterService>()
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
                Console.WriteLine("Проверка соединения с БД");
                Console.WriteLine(string.Concat(db.Servers.AsNoTracking().Select(x => x.Description)));

                db.DisposeAsync();
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
            CloseUpdateCycles,
            CheckDbConnection
        }

        static Dictionary<Command, string[]> commandDict = new Dictionary<Command, string[]>
        {
            { Command.None, new[] { "" } }, //ALWAYS MUST BE FIRST IN COLLECTION
            { Command.Exit, new[] {"exit", "close", "shutdown", "ex"} },
            { Command.UpdateRconDB, new [] { "update", "up" } },
            { Command.BeginUpdateCycles, new [] { "cycle_start", "cS" } },
            { Command.CloseUpdateCycles, new [] { "cycle_end", "cE" } },
            { Command.CheckDbConnection, new [] { "check_db_connection", "ch_db" } }
        };

        private static async Task ConsoleControlCycle(WebApplication app)
        {
            while (true)
            {
                Console.WriteLine("Введите команду:");
                string cmd = Console.ReadLine();

                switch (commandDict.FirstOrDefault(x => x.Value.Contains(cmd)).Key)
                {
                    case Command.Exit:
                        await app.StopAsync();
                        DiscordApp.DiscordBot.Close();
                        goto exitGOTO;
                    case Command.UpdateRconDB:

                        break;
                    case Command.BeginUpdateCycles:
                        app.Services.GetService<DbUpdaterService>()?.StartCycles();
                        break;
                    case Command.CloseUpdateCycles:
                        app.Services.GetService<DbUpdaterService>()?.EndCycles();
                        break;
                    case Command.CheckDbConnection:
                        using (StatisticDbContext db = new StatisticDbContext())
                        {
                            Console.WriteLine("Проверка соединения с БД");
                            Console.WriteLine(string.Concat(db.Servers.AsNoTracking().Select(x => x.Description)));
                            Console.WriteLine(db.Servers.AsNoTracking().Count());
                            db.DisposeAsync();
                        }
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