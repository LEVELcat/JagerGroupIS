
class MainProgram
{
    private async static Task Main(string[] args)
    {
        Console.WriteLine("Hello World");

        WebApp.Program.Main(args);
        Console.WriteLine("Web App Start");

        DiscordApp.DiscordBot.Main(args);
        Console.WriteLine("Discord Bot Start");


        await Task.Delay(-1);
    }
}