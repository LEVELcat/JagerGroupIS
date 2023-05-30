
using System.Globalization;
using System.Collections.Generic;
using System.Collections;

class MainProgram
{
    private async static Task Main(string[] args)
    {
        CultureInfo ci = new CultureInfo("ru-RU");
        Thread.CurrentThread.CurrentCulture = ci;
        Thread.CurrentThread.CurrentUICulture = ci;

        Console.WriteLine("Hello World");

        WebApp.Program.Main(args);
        Console.WriteLine("Web App Start");

        //DiscordApp.DiscordBot.Main(args);
        Console.WriteLine("Discord Bot Start");


        await Task.Delay(-1);
    }
}