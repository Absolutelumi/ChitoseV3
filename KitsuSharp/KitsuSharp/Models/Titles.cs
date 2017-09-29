using System.Runtime.Serialization;

namespace KitsuSharp.Models
{
    [DataContract]
    public struct Titles
    {
        public string English => en;
        public string Romanized => en_jp;
        public string Japanese => ja_jp;

        [DataMember]
        internal string en;
        [DataMember]
        internal string en_jp;
        [DataMember]
        internal string ja_jp;
    }
}
