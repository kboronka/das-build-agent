using System;

using HttpPack.Server;

namespace das_build_agent
{
    class Program
    {
        static void Main(string[] args)
        {
            var port = int.Parse(Environment.GetEnvironmentVariable("PORT") ?? "4700");
            var secret = Environment.GetEnvironmentVariable("SECRET") ?? "secret-goes-here";
            var sandbox = Environment.GetEnvironmentVariable("SANDBOX_PATH") ?? @"c:\temp";

            var server = new HttpServer(port, null, null);

            Console.WriteLine("Press a key to stop");
            Console.ReadKey();
        }
    }
}
