using System;

using HttpPack;

namespace DasBuildAgent
{
    class Program
    {
        static void Main()
        {
            var agent = new Agent();
            agent.Start();

            Console.WriteLine("Press a key to stop");
            Console.ReadKey();
        }
    }
}
