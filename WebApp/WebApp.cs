using System.Globalization;
using WebApp.DbContexts;
using Microsoft.AspNetCore.HttpOverrides;

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

            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllersWithViews();
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
            builder.Services.AddTransient<StatisticDbContext>();

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


            app.Run();
        }
    }
}