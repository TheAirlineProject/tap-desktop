using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirportModel;

namespace TheAirline.Model.GeneralModel.ScenarioModel
{
    //the class for passenger demand at a scenario
       [ProtoContract]
     public class ScenarioPassengerDemand
    {
           [ProtoMember(1)]
           public Country Country { get; set; }
           [ProtoMember(2)]
           public Airport Airport { get; set; }
           [ProtoMember(3)]
           public double Factor { get; set; }
           [ProtoMember(4)]
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
