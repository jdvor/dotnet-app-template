namespace App.Console.Config
{
    using App.Abstractions;
    using App.Metrics;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NLog.Extensions.Logging;
    using System;
    using System.Collections.Generic;

    public static class Boostrapper
    {
        public static IServiceProvider CreateServiceProvider(
            IEnumerable<string> configFilePaths = null,
            string[] commandLineArgs = null,
            params IServiceInstaller[] installers)
        {
            var services = new ServiceCollection();

            // configuration
            var config = CreateConfig(configFilePaths, commandLineArgs);
            services.AddSingleton(config);
            services.AddOptions();

            // logging
            var loggerFactory = CreateLogging();
            services.AddSingleton(loggerFactory);
            services.AddLogging();
            services.AddSingleton(loggerFactory.CreateLogger("default")); // for Program.cs

            // metrics root
            var metrics = CreateMetrics();
            services.AddSingleton(metrics);

            // app context
            var appInfo = new AppInfo(config, commandLineArgs);
            services.AddSingleton(appInfo);

            // custom installers
            if (installers != null)
            {
                foreach (var installer in installers)
                {
                    installer.Install(services, config);
                }
            }

            return services.BuildServiceProvider(validateScopes: true);
        }

        public static IConfiguration CreateConfig(IEnumerable<string> configFilePaths = null, string[] commandLineArgs = null)
        {
            var cfg = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory);

            if (configFilePaths != null)
            {
                foreach (var path in configFilePaths)
                {
                    cfg.AddJsonFile(path, optional: true, reloadOnChange: false);
                }
            }

            cfg.AddEnvironmentVariables();

            if (commandLineArgs != null)
            {
                cfg.AddCommandLine(commandLineArgs);
            }

            return cfg.Build();
        }

        public static ILoggerFactory CreateLogging()
        {
            var options = new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true };
            var loggerFactory = new LoggerFactory().AddNLog(options);
            loggerFactory.ConfigureNLog("nlog.config");

            return loggerFactory;
        }

        private static IMetrics CreateMetrics()
        {
            return new MetricsBuilder()
                .Configuration
                    .Configure(options =>
                    {
                        options.Enabled = true;
                        options.ReportingEnabled = true;
                        options.GlobalTags = new GlobalMetricTags
                        {
                            { "app", typeof(Boostrapper).Namespace },
                            { "host", Environment.MachineName },
                        };
                    })
                .Build();
        }
    }
}
