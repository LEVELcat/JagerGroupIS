
using System.Globalization;
using System.Collections.Generic;
using System.Collections;

class MainProgram
{
    private async static Task Main(string[] args)
    {
        WebApp.WebApp.Main(args);
        Console.WriteLine("Web App Start");

        await Task.Delay(-1);
    }
}