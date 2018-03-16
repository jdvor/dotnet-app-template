namespace App.Console
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Reflection;
    using Microsoft.Extensions.Configuration;

    public static class Util
    {
        /// <summary>
        /// https://stackoverflow.com/questions/804700/how-to-find-fqdn-of-local-machine-in-c-net#804719
        /// </summary>
        public static string GetHostFqdn()
        {
            string fqdn;
            var ipProps = IPGlobalProperties.GetIPGlobalProperties();
            try
            {
                var domainSufix = "." + ipProps.DomainName;
                var hostName = Dns.GetHostName();
                fqdn = hostName.EndsWith(domainSufix)
                    ? hostName
                    : hostName + domainSufix;
            }
            catch (SocketException ex)
            {
                // DNS request failure => fallback to NETBIOS name; might work identical in some cases
                Debug.WriteLine($"error getting host FQDN via DNS - {ex.GetType().Name}: {ex.Message}");
                fqdn = $"{ipProps.DomainName}.{ipProps.HostName}";
            }

            return fqdn.ToLowerInvariant();
        }

        public static string GetAppVersion()
        {
            return "1.0.0.123 gitsha1";
        }

        public static IPAddress GetValue(this IConfiguration config, string key, IPAddress @default = null)
        {
            return config.GetValue<string>(key).AsIPAddress(@default);
        }

        public static IPAddress AsIPAddress(this string s, IPAddress fallback = null)
        {
            if (IPAddress.TryParse(s, out IPAddress ip))
            {
                return ip;
            }

            if (fallback != null)
            {
                return fallback;
            }

            throw new InvalidCastException("string could not be parsed as IPAddress");
        }
    }
}
