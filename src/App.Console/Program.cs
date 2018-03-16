namespace App.Console
{
    using System;
    using App.Abstractions.Messages;
    using App.Console.Config;
    using App.Console.Rmq;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    internal static class Program
    {
        internal static void Main(string[] args)
        {
            var services = Boostrapper.CreateServiceProvider(
                            configFilePaths: new[] { "config.json" },
                            commandLineArgs: args,
                            installers: new RmqInstaller());

            logger = services.GetRequiredService<ILogger>();

            using (var rmq = new RmqConsumers(services))
            {
                Console.WriteLine("Press any <Enter> to publish message or anything else to terminate ...");
                Console.WriteLine();

                ConsoleKey key;
                while ((key = Console.ReadKey().Key) == ConsoleKey.Enter)
                {
                    PublishSomeMessage(rmq);
                }
            }
        }

        private static void PublishSomeMessage(RmqConsumers rmq)
        {
            counter += 1;
            if (counter % 2 == 1)
            {
                var dhb = new DeviceHeartBeat
                {
                    AppId = "AV",
                    Version = "1.2.33",
                    BuildNumber = counter,
                    DeviceFingerprint = Guid.NewGuid().ToString("N"),
                };
                logger.LogInformation($"publishing {counter}: {dhb}");
                rmq.Publish(dhb);
                return;
            }

            var du = new DeviceUninstall
            {
                AppId = "AV",
                DeviceFingerprint = Guid.NewGuid().ToString("N"),
            };
            logger.LogInformation($"publishing {counter}: {du}");
            rmq.Publish(du);
        }

        private static int counter;
        private static ILogger logger;
    }
}
