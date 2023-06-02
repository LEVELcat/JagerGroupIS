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
                            .AddSingleton<RconUpdaterService>();

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

        static Dictionary<string, string[]> commandDict = new Dictionary<string, string[]>
        {
            { "exit", new[] {"exit", "close", "shutdown"} }
        };

        private static async Task ConsoleControlCycle(WebApplication app)
        {
            while (true)
            {
                Console.WriteLine("Введите команду:");
                string cmd = Console.ReadLine();

                switch(commandDict.First(x => x.Value.Contains(cmd)).Key)
                {
                    case "exit":
                        await app.StopAsync();
                        goto exitGOTO;






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