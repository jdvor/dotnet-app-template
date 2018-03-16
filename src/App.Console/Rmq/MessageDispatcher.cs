namespace App.Console.Rmq
{
    using System;
    using System.Threading.Tasks;
    using EasyNetQ.AutoSubscribe;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    // https://github.com/EasyNetQ/EasyNetQ/wiki/Auto-Subscriber
    public class MessageDispatcher : IAutoSubscriberMessageDispatcher
    {
        public MessageDispatcher(IServiceProvider services, ILogger<MessageDispatcher> logger)
        {
            this.services = services;
            this.logger = logger;
        }

        private readonly IServiceProvider services;
        private readonly ILogger<MessageDispatcher> logger;

        public void Dispatch<TMessage, TConsumer>(TMessage message)
            where TMessage : class
            where TConsumer : IConsume<TMessage>
        {
            throw new NotSupportedException("non-async handling of a message is not supported");
        }

        public async Task DispatchAsync<TMessage, TConsumer>(TMessage message)
            where TMessage : class
            where TConsumer : IConsumeAsync<TMessage>
        {
            var handler = services.GetRequiredService<IConsumeAsync<TMessage>>();
            await handler.Consume(message).ConfigureAwait(false);
        }
    }
}
