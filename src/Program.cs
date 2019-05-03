using System;

using HttpPack.Server;

namespace DasBuildAgent
{
    class Program
    {
        static void Main(string[] args)
        {
            var agent = new Agent();
            agent.Start();

            Console.WriteLine("Press a key to stop");
            Console.ReadKey();
        }
    }
}
