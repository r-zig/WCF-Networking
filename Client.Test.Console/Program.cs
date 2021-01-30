using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using Roniz.Diagnostics.Logging;
using Roniz.Networking.Common.ServiceAddressResolvers;
using System.Threading;

namespace Roniz.Networking.Client.Test.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                System.Console.WriteLine("press any key to start client");
                System.Console.ReadKey();

                ConsoleKeyInfo key;
                do
                {
                    System.Console.WriteLine("Choose D to use dynamic resolving or S to use static resolving...");
                    key = System.Console.ReadKey();
                    System.Console.WriteLine();
                } while (key.Key != ConsoleKey.D && key.Key != ConsoleKey.S);

                IEndpointResolver endpointResolver;

                //Dynamic
                if(key.Key == ConsoleKey.D)
                {
                    endpointResolver = EndpointResolverFactory.CreateDynamicEndpointResovler();
                }
                //static
                else
                {
                    System.Console.WriteLine("write the IP address of the remote server");
                    var ipAddress = IPAddress.Parse(System.Console.ReadLine());

                    System.Console.WriteLine("write the port of the remote server");
                    int port = int.Parse(System.Console.ReadLine());
                    endpointResolver = new EndpointResolver(new StaticServiceAddressResolver(ipAddress, port));
                }
                
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
#if true
                ManualResetEvent waitHandle = new ManualResetEvent(false);
#endif
                endpointResolver.ResolveExternalV4EndpointCompleted +=
                    (o, eventArgs) =>
                        {
#if true
                            waitHandle.Set();
#endif
                            stopwatch.Stop();
                            PrintResponse(eventArgs);
                            System.Console.WriteLine("Resolving duration {0} , CurrentThread: {1}", stopwatch.Elapsed, Thread.CurrentThread.ManagedThreadId);
                        };
                System.Console.WriteLine("start resolve asynchronously CurrentThread: {0}", Thread.CurrentThread.ManagedThreadId);
                endpointResolver.ResolveExternalV4EndpointAsync();
#if true
                var signal = waitHandle.WaitOne(TimeSpan.FromMinutes(1));
                if (!signal)
                {
                    System.Console.WriteLine("Timeout !");
                    return;
                }
#endif
                System.Console.WriteLine("press Enter to cancel asynchronous operation");

                System.Console.ReadLine();
                
                endpointResolver.ResolveExternalV4EndpointAsyncCancel();
                
                //sync operation - ok
                //var response = endpointResolver.ResolveExternalV4Endpoint();
                //stopwatch.Stop();
                //System.Console.WriteLine("ExternalAddress {0}", response);
                //System.Console.WriteLine("Resolving duration {0}", stopwatch.Elapsed);

                System.Console.WriteLine("press Enter to exit");
                System.Console.ReadLine();
            }
            catch (Exception exception)
            {
                LogManager.GetCurrentClassLogger().Fatal(exception);
            }
            System.Console.ReadLine();
        }

        static void PrintResponse(ResolveExternalEndpointCompletedEventArgs e)
        {
            StringBuilder stringBuilder = new StringBuilder("Resolve Response:\n");
            if (!e.Cancelled)
            {
                stringBuilder.AppendFormat("\te.EndPoint: {0}\n", e.EndPoint);
            }
            stringBuilder.AppendFormat("\te.Cancelled: {0}\n", e.Cancelled);
            stringBuilder.AppendFormat("\te.UserState: {0}\n", e.UserState);

            var logger = LogManager.GetCurrentClassLogger();
            logger.Debug(stringBuilder.ToString());
            if(e.Error != null)
                logger.Warn(e.Error);
        }
    }
}
