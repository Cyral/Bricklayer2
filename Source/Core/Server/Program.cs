using System;

namespace Bricklayer.Core.Server
{
    /// <summary>
    /// The main starting point of the server.
    /// </summary>
    internal static class Program
    {
        static void Main(string[] args)
        {
            Console.Title = Constants.Strings.ServerTitle;

            // Create an instance of the server and run it.
            new Server().Start().Wait();
        }
    }
}
