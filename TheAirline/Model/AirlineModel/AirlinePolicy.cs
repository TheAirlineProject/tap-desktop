using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.AirlineModel
{
    [ProtoContract]
    //the class for a policy for an airline
    public class AirlinePolicy
    {
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public object PolicyValue { get; set; }
        public AirlinePolicy(string name, object policyValue)
        {
            this.Name = name;
            this.PolicyValue = policyValue;
        }
    }
}
