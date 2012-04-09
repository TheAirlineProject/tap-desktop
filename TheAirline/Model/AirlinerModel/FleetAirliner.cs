using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel.StatisticsModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirlinerModel
{
    public class FleetAirliner
    {
        public Airliner Airliner { get; set; }
        public Airline Airline { get; set; }
        public string Name { get; set; }
        public Airport Homebase { get; set; }
        //public RouteAirliner RouteAirliner { get; set; }
        public enum PurchasedType { Bought, Leased,BoughtDownPayment }
        public PurchasedType Purchased { get; set; }
        public Boolean HasRoute { get { return this.Route != null; } set { ;} }
        public GeneralStatistics Statistics { get; set; }

        /*Changed for deleting routeairliner*/
        public enum AirlinerStatus { Stopped, On_route, On_service, Resting, To_homebase, To_route_start }
        public AirlinerStatus Status { get; set; }
        public Coordinates CurrentPosition { get; set; }
        public Route Route { get; set; }
        public Flight CurrentFlight { get; set; }
           

        public FleetAirliner(PurchasedType purchased, Airline airline,Airliner airliner, string name, Airport homebase)
        {
            this.Airliner = airliner;
            this.Purchased = purchased;
            this.Airliner.Airline = airline;
            this.Airline = airline;
            this.Homebase = homebase;
            this.Name = name;
            this.Statistics = new GeneralStatistics();

            this.Status = AirlinerStatus.Stopped;

            this.CurrentPosition = new Coordinates(this.Homebase.Profile.Coordinates.Latitude, this.Homebase.Profile.Coordinates.Longitude);
        }
        //returns the next destination
        public Airport getNextDestination()
        {
            return this.CurrentFlight.Entry.Destination.Airport == this.Route.Destination1 ? this.Route.Destination2 : this.Route.Destination1;
        }
        //returns the departure location
        public Airport getDepartureAirport()
        {
            return getNextDestination();

        }
    }
  
}
