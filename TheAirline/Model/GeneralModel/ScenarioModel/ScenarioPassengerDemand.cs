
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirportModel;

namespace TheAirline.Model.GeneralModel.ScenarioModel
{
    //the class for passenger demand at a scenario
       [Serializable]
     public class ScenarioPassengerDemand
    {
           
           public Country Country { get; set; }
           
           public Airport Airport { get; set; }
           
           public double Factor { get; set; }
           
           public DateTime EndDate { get; set; }
        public ScenarioPassengerDemand(double factor, DateTime enddate, Country country, Airport airport)
        {
            this.Country = country;
            this.Factor = factor;
            this.EndDate = enddate;
            this.Airport = airport;
        }
    }
}
