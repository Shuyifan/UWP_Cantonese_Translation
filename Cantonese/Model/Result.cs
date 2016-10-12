using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Cantonese.Model
{
    //用于VoiceToText的Jason反解析
    [DataContract]
    class Result
    {
        [DataMember]
        public string version { get; set; }
        [DataMember]
        public Header header { get; set; }
        [DataMember]
        public Results[] results { get; set; }
    }
    [DataContract]
    class Header
    {
        [DataMember]
        public string status { get; set; }
        [DataMember]
        public string scenario { get; set; }
        [DataMember]
        public string name { get; set; }
    }
    [DataContract]
    class Results
    {
        [DataMember]
        public string scenario { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string lexical { get; set; }
    }

}
