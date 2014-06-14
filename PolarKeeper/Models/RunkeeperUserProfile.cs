using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace PolarKeeper.Models
{
    [DataContract]
    public class RunkeeperUserProfile
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "elite")]
        public string Elite { get; set; }
        [DataMember(Name = "profile")]
        public string Profile { get; set; }
    }
}