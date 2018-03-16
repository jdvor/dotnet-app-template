namespace App.Console.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using App.Abstractions.Messages;
    using EasyNetQ.AutoSubscribe;
    using Microsoft.Extensions.Logging;

    public class DeviceUninstallHandler : IConsumeAsync<DeviceUninstall>
    {
        public DeviceUninstallHandler(ILogger<DeviceUninstallHandler> logger, AppInfo appInfo)
        {
            this.logger = logger;
            this.appInfo = appInfo;
        }

        private readonly ILogger<DeviceUninstallHandler> logger;

        private readonly AppInfo appInfo;

        [AutoSubscriberConsumer(SubscriptionId = "q")]
        public async Task Consume(DeviceUninstall message)
        {
            await Task.Delay(50).ConfigureAwait(false); // simulate some I/O work

            var info = $"{message} in thread {Thread.CurrentThread.ManagedThreadId}, handler instance {GetHashCode()}";
            logger.LogInformation(info);
        }
    }
}
