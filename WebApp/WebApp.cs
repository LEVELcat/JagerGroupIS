using WebApp.DbContexts;

namespace WebApp
{
    public class WebApp
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllersWithViews();
            builder.Services.AddTransient<StatisticDbContext>();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
                app.UseHttpsRedirection();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            await app.RunAsync();
        }
    }
}