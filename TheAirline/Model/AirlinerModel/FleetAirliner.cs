using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel.StatisticsModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.PilotModel;

namespace TheAirline.Model.AirlinerModel
{
    public class FleetAirliner
    {
        public Airliner Airliner { get; set; }
        public string Name { get; set; }
        public Airport Homebase { get; set; }
        public enum PurchasedType { Bought, Leased,BoughtDownPayment }
        public DateTime PurchasedDate { get; set; }
        public PurchasedType Purchased { get; set; }
        public Boolean HasRoute { get { return this.Routes.Count > 0; } set { ;} }
        public AirlinerStatistics Statistics { get; set; }

        /*Changed for deleting routeairliner*/
        public enum AirlinerStatus { Stopped, On_route, On_service, Resting, To_homebase, To_route_start }
        public AirlinerStatus Status { get; set; }
        public Coordinates CurrentPosition { get; set; }
        public List<Route> Routes { get; private set; }
        public Flight CurrentFlight { get; set; }
        public DateTime GroundedToDate { get; set; }
        public List<Pilot> Pilots { get; set; }
        public int NumberOfPilots {get {return this.Pilots.Count;} private set {;}}
        public FleetAirliner(PurchasedType purchased,DateTime purchasedDate, Airline airline,Airliner airliner,string name, Airport homebase)
        {
            this.Airliner = airliner;
            this.Purchased = purchased;
            this.PurchasedDate = purchasedDate;
            this.Airliner.Airline = airline;
            this.Homebase = homebase;
            this.Name = name;
            this.Statistics = new AirlinerStatistics(this);
          
            this.Status = AirlinerStatus.Stopped;

            this.CurrentPosition = new Coordinates(this.Homebase.Profile.Coordinates.Latitude, this.Homebase.Profile.Coordinates.Longitude);

            this.Routes = new List<Route>();
            this.Pilots = new List<Pilot>();
        }
        //adds a pilot to the airliner
        public void addPilot(Pilot pilot)
        {
            this.Pilots.Add(pilot);
        }
        //removes a pilot from the airliner
        public void removePilot(Pilot pilot)
        {
            this.Pilots.Remove(pilot);
        }
        //adds a route to the airliner
        public void addRoute(Route route)
        {
            this.Routes.Add(route);

       
        }
        //removes a route from the airliner
        public void removeRoute(Route route)
        {
            this.Routes.Remove(route);

            route.TimeTable.Entries.RemoveAll(e => e.Airliner == this);
         

        }
       
    }
  
}
