namespace App.Console.Rmq
{
    using System;
    using System.Reflection;
    using EasyNetQ;
    using EasyNetQ.AutoSubscribe;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using IServiceProvider = System.IServiceProvider;

    // This is root object; the only one which should be allowed to resolve services.
    // All others should recieve them through ctor.
    public sealed class RmqConsumers : IDisposable
    {
        public RmqConsumers(IServiceProvider services)
        {
            var options = services.GetRequiredService<RmqOptions>();
            var cs = Environment.ExpandEnvironmentVariables(options.ConnectionString);
            bus = RabbitHutch.CreateBus(cs);
            SubscribeHandlers(bus, services);
        }

        private readonly IBus bus;

        private bool disposed;

        // https://github.com/EasyNetQ/EasyNetQ/wiki/Auto-Subscriber
        private static void SubscribeHandlers(IBus bus, IServiceProvider services)
        {
            var dispatcherLogger = services.GetRequiredService<ILogger<MessageDispatcher>>();
            var subscriber = new AutoSubscriber(bus, "auto")
            {
                AutoSubscriberMessageDispatcher = new MessageDispatcher(services, dispatcherLogger),
            };
            subscriber.SubscribeAsync(Assembly.GetExecutingAssembly());
        }

        // Just to allow publishing messages from the app itself, so handlers firing can be demonstrated.
        internal void Publish<T>(T message)
            where T : class
        {
            bus.Publish(message);
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            bus?.SafeDispose();
        }
    }
}
