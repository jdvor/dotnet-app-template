namespace App.Console
{
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;

    public sealed class AppInfo
    {
        public AppInfo(string[] args, string name, string env, string fqdn, string ver)
        {
            Args = new ReadOnlyCollection<string>(args ?? Array.Empty<string>());

            Name = string.IsNullOrEmpty(name)
                ? DefaultAppName()
                : name;

            HostFqdn = string.IsNullOrEmpty(fqdn)
                ? Util.GetHostFqdn()
                : fqdn.ToLowerInvariant();

            Env = string.IsNullOrEmpty(env)
                ? GuessEnvFromFqdn(HostFqdn)
                : env.ToLowerInvariant();

            Version = string.IsNullOrEmpty(ver)
                ? Util.GetAppVersion()
                : ver;
        }

        public AppInfo(IConfiguration cfg, string[] args)
            : this(args,  cfg.GetValue<string>("app:name", null), cfg.GetValue<string>("app:env", null), null, null)
        {
        }

        private static string DefaultAppName()
        {
            return AppContext.BaseDirectory
                .Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries)
                .Last();
        }

        [SuppressMessage(
            "Layout Rules",
            "SA1503:CurlyBracketsMustNotBeOmitted",
            Justification = "It would be much less readable with braces.")]
        private static string GuessEnvFromFqdn(string fqdn)
        {
            var f = fqdn.ToLowerInvariant();

            if (f.Contains(CommonName.Production)) return CommonName.Production;
            if (f.Contains(CommonName.Stage)) return CommonName.Stage;
            if (f.Contains(CommonName.PerformanceTest)) return CommonName.PerformanceTest;
            if (f.Contains(CommonName.Test)) return CommonName.Test;
            return CommonName.Development;
        }

        public string Name { get; }

        public string Env { get; }

        public string HostFqdn { get; }

        public string Version { get; }

        public ReadOnlyCollection<string> Args { get; }

        public override string ToString()
        {
            return $"{Name} (ver: {Version}, env: {Env}, fqdn: {HostFqdn})";
        }

        public static class CommonName
        {
            public const string Development = "dev";
            public const string Test = "test";
            public const string PerformanceTest = "perf";
            public const string Stage = "stage";
            public const string Production = "prod";
        }
    }
}
