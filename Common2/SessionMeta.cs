using System;
using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class SessionMeta
    {
        [DataMember] public string SessionId { get; set; }
        //[DataMember] public DateTime StartTime { get; set; }
        [DataMember] public DateTime DateTime { get; set; }
        [DataMember] public double Volume { get; set; }
        [DataMember] public double LightLevel { get; set; }
        [DataMember] public double RelativeHumidity { get; set; }
        [DataMember] public double AirQuality { get; set; }
    }
}
