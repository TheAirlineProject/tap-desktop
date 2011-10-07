using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel.StatisticsModel;

namespace TheAirline.Model.AirlinerModel
{
    public class FleetAirliner
    {
        public Airliner Airliner { get; set; }
        public Airline Airline { get; set; }
        public string Name { get; set; }
        public Airport Homebase { get; set; }
        public RouteAirliner RouteAirliner { get; set; }
        public enum PurchasedType { Bought, Leased,BoughtDownPayment }
        public PurchasedType Purchased { get; set; }
        public Boolean HasRoute { get { return this.RouteAirliner != null; } set { ;} }
        public GeneralStatistics Statistics { get; set; }
        public FleetAirliner(PurchasedType purchased, Airline airline,Airliner airliner, string name, Airport homebase)
        {
            this.Airliner = airliner;
            this.Purchased = purchased;
            this.Airliner.Airline = airline;
            this.Airline = airline;
            this.Homebase = homebase;
            this.Name = name;
            this.Statistics = new GeneralStatistics();
        }
    }
  
}
