
using System.Runtime.Serialization;

namespace KitsuSharp.Models
{
    [DataContract]
    public class ImageSet
    {
        public string TinyUrl => tiny;
        public string SmallUrl => small;
        public string MediumUrl => medium;
        public string LargeUrl => large;
        public string OriginalUrl => original;

        [DataMember]
        internal string tiny;
        [DataMember]
        internal string small;
        [DataMember]
        internal string medium;
        [DataMember]
        internal string large;
        [DataMember]
        internal string original;
    }
}
