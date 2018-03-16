namespace App.Console
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class Extensions
    {
        public static TConfig AddStronglyTypedConfig<TConfig>(this IServiceCollection services, IConfiguration configuration, string key)
            where TConfig : class, new()
        {
            var config = new TConfig();
            configuration.Bind(key, config);
            services.AddSingleton(config);
            return config;
        }
    }
}
