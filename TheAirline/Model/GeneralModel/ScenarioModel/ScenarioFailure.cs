using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel.ScenarioModel
{
    //the class for a failure at a scenario
       [ProtoContract]
     public class ScenarioFailure
    {
        public enum FailureType { Cash, Safety, Debt, Security, Fleet, Domestic, Intl, PaxGrowth, Crime, FleetAge , Pax, Bases, JetRation}
        [ProtoMember(1)]
        public FailureType Type { get; set; }
        //1 - means check each month
        [ProtoMember(2)]
        public int CheckMonths { get; set; }
        [ProtoMember(3)]
        public object Value { get; set; }
        [ProtoMember(4)]
        public string FailureText { get; set; }
        [ProtoMember(5)]
        public int MonthsOfFailure { get; set; }
        [ProtoMember(6)]
        public string ID { get; set; }
        public ScenarioFailure(string id, FailureType type, int checkMonths, object value, string failureText, int monthsOfFailure)
        {
            this.ID = id;
            this.Type = type;
            this.CheckMonths = checkMonths;
            this.Value = value;
            this.FailureText = failureText;
            this.MonthsOfFailure = monthsOfFailure;
        }
    }
   
}
