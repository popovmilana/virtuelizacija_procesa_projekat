using System;
using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class SessionMeta
    {
        [DataMember] public string SessionId { get; set; }
        [DataMember] public DateTime StartTime { get; set; }
    }
}
