namespace WebApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {


            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
                app.UseHttpsRedirection();

            //app.UseDefaultFiles();
            //app.UseStaticFiles();

            app.RunAsync();

            await Task.Delay(-1);
        }
    }
}