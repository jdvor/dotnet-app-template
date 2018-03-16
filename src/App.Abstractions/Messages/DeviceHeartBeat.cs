namespace App.Abstractions.Messages
{
    using System.Runtime.Serialization;
    using EasyNetQ;

    [Queue("device_feed", ExchangeName = "device_feed")]
    [DataContract]
    public class DeviceHeartBeat
    {
        [DataMember]
        public string DeviceFingerprint { get; set; }

        [DataMember]
        public string AppId { get; set; }

        [DataMember]
        public string Version { get; set; }

        [DataMember]
        public int BuildNumber { get; set; }

        public override string ToString()
        {
            return $"dhb: {AppId} {Version}.{BuildNumber} {DeviceFingerprint}";
        }
    }
}
