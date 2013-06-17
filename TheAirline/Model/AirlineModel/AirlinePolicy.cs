
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.AirlineModel
{
    [Serializable]
    //the class for a policy for an airline
    public class AirlinePolicy
    {
        
        public string Name { get; set; }
        
        public object PolicyValue { get; set; }
        public AirlinePolicy(string name, object policyValue)
        {
            this.Name = name;
            this.PolicyValue = policyValue;
        }
    }
}
