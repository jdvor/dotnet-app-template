namespace App.Console.Config
{
    using App.Abstractions;
    using App.Abstractions.Messages;
    using App.Console.Handlers;
    using App.Console.Rmq;
    using EasyNetQ.AutoSubscribe;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class RmqInstaller : IServiceInstaller
    {
        public void Install(IServiceCollection services, IConfiguration config)
        {
            services.AddStronglyTypedConfig<RmqOptions>(config, "rabbit");
            services.AddTransient<IConsumeAsync<DeviceHeartBeat>, DeviceHeartBeatHandler>();
            services.AddTransient<IConsumeAsync<DeviceUninstall>, DeviceUninstallHandler>();
        }
    }
}
