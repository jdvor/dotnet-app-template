namespace App.Abstractions.Messages
{
    using System.Runtime.Serialization;
    using EasyNetQ;

    [Queue("device_feed", ExchangeName = "device_feed")]
    [DataContract]
    public class DeviceUninstall
    {
        [DataMember]
        public string DeviceFingerprint { get; set; }

        [DataMember]
        public string AppId { get; set; }

        public override string ToString()
        {
            return $"uninstall: {AppId} {DeviceFingerprint}";
        }
    }
}
