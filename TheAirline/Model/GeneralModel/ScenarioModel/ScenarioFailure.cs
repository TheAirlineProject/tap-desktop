
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel.ScenarioModel
{
    //the class for a failure at a scenario
       [Serializable]
     public class ScenarioFailure
    {
        public enum FailureType { Cash, Safety, Debt, Security, Fleet, Domestic, Intl, PaxGrowth, Crime, FleetAge , Pax, Bases, JetRation}
        
        public FailureType Type { get; set; }
        //1 - means check each month
        
        public int CheckMonths { get; set; }
        
        public object Value { get; set; }
        
        public string FailureText { get; set; }
        
        public int MonthsOfFailure { get; set; }
        
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
