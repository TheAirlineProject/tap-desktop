using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirportModel;

namespace TheAirline.Model.AirlineModel.SubsidiaryModel
{
    [ProtoContract]
    //the class for a future subsidiary airline for an airline
    public class FutureSubsidiaryAirline
    {
          [ProtoMember(1)]
        public string Name { get; set; }
          [ProtoMember(2)]
          public string IATA { get; set; }
          [ProtoMember(3)]
          public Airline.AirlineFocus Market { get; set; }
          [ProtoMember(4)]
          public Airline.AirlineMentality Mentality { get; set; }
          [ProtoMember(5)]
          public Airport PreferedAirport { get; set; }
          [ProtoMember(6)]
          public string Logo { get; set; }
          [ProtoMember(7)]
          public Route.RouteType AirlineRouteFocus { get; set; }
        public FutureSubsidiaryAirline(string name, string iata,Airport airport, Airline.AirlineMentality mentality, Airline.AirlineFocus market, Route.RouteType airlineRouteFocus, string logo)
        {
            this.Name = name;
            this.IATA = iata;
            this.PreferedAirport = airport;
            this.Mentality = mentality;
            this.Market = market;
            this.Logo = logo;
            this.AirlineRouteFocus = airlineRouteFocus;
        }
    }
}
