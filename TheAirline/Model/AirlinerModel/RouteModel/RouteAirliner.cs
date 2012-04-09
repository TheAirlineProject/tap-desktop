using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.AirportModel;

namespace TheAirline.Model.AirlinerModel.RouteModel
{
    /*
    //the class for an airliner with a route
    public class RouteAirliner
    {
        public Route Route { get; set; }
        public FleetAirliner Airliner { get; set; }
        public enum AirlinerStatus { Stopped, On_route, On_service, Resting, To_homebase, To_route_start }
        public AirlinerStatus Status { get; set; }
        public Coordinates CurrentPosition { get; set; }
        public Flight CurrentFlight { get; set; }
        public RouteAirliner(FleetAirliner airliner, Route route)
        { 
            this.Route = route;
            this.Airliner = airliner;
            this.Route.Airliner = this;
            airliner.RouteAirliner = this;

            this.Status = AirlinerStatus.Stopped;

            this.CurrentPosition = new Coordinates(this.Airliner.Homebase.Profile.Coordinates.Latitude,this.Airliner.Homebase.Profile.Coordinates.Longitude);

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
  */
   
   
}
