
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TheAirline.Model.AirlinerModel.RouteModel;

namespace TheAirline.Model.AirlineModel.SubsidiaryModel
{
    //the class for a subsidiary airline for an airline
    [Serializable]
    public class SubsidiaryAirline :  Airline 
    {
        public Airline Airline { get; set; }
        public SubsidiaryAirline(Airline airline,AirlineProfile profile, AirlineMentality mentality, AirlineFocus market, AirlineLicense license,Route.RouteType routefocus)
            : base(profile, mentality, market,license,routefocus)
        {
            this.Airline = airline;
            this.Profile.Logos = this.Airline.Profile.Logos;
        }
        public override bool isHuman()
        {
            return this.Airline != null && this.Airline.isHuman();
        }
        public override bool isSubsidiaryAirline()
        {
            return this.Airline != null;
        }
   
    }
}
