using System.Runtime.Serialization;

namespace KitsuSharp.Models
{
    [DataContract]
    internal struct Links
    {
        [DataMember]
        internal string self;
    }
}
