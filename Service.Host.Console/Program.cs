using System;
using Roniz.Diagnostics.Logging;

namespace Roniz.Networking.Service.Host.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                System.Console.WriteLine("press any key to start server");
                System.Console.ReadKey();
                //var serviceHost = new ServiceHost(typeof(ServiceResolver));
                ServiceResolver serviceResolver = new ServiceResolver();
                serviceResolver.Open();
                //serviceHost.Open();
                System.Console.WriteLine("server is opened.. press Enter to stop server");
                System.Console.ReadLine();
                serviceResolver.Close();
            }
            catch (Exception exception)
            {
                LogManager.GetCurrentClassLogger().Fatal(exception);
                System.Console.ReadLine();
            }
        }
    }
}
