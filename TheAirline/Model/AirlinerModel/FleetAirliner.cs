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
    [Serializable]
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
        
        public DateTime LastAMaintenance { get; set; }
        
        public int AMaintenanceInterval { get; set; }
        
        public DateTime LastBMaintenance { get; set; }
        
        public int BMaintenanceInterval { get; set; }
        
        public DateTime LastCMaintenance { get; set; }
        public int CMaintenanceInterval { get; set; }
        public DateTime DueCMaintenance { get; set; }
        public DateTime LastDMaintenance { get; set; }
        public int DMaintenanceInterval { get; set; }
        public DateTime DueDMaintenance { get; set; }
        public DateTime SchedAMaintenance { get; set; }
        public DateTime SchedBMaintenance { get; set; }
        public DateTime SchedCMaintenance { get; set; }
        public DateTime SchedDMaintenance { get; set; }
        public DateTime OOSDate { get; set; }
        public List<RouteModel.Route> MaintRoutes { get; set; }
        public IDictionary<Invoice, String> MaintenanceHistory { get; set; }
        /*Changed for deleting routeairliner*/
        public enum AirlinerStatus { Stopped, On_route, On_service, Resting, To_homebase, To_route_start }
        public AirlinerStatus Status { get; set; }
        public Coordinates CurrentPosition { get; set; }
        public List<Route> Routes { get; private set; }
        public Flight CurrentFlight { get; set; }
        public DateTime GroundedToDate { get; set; }
        public List<Pilot> Pilots { get; set; }
        public List<AirlinerInsurance> InsurancePolicies { get; set; }
        public int NumberOfPilots {get {return this.Pilots.Count;} private set {;}}
        public FleetAirliner(PurchasedType purchased,DateTime purchasedDate, Airline airline,Airliner airliner,Airport homebase)
        {
            this.Airliner = airliner;
            this.Purchased = purchased;
            this.PurchasedDate = purchasedDate;
            this.Airliner.Airline = airline;
            this.Homebase = homebase;
            this.Name = airliner.TailNumber;
            this.Statistics = new AirlinerStatistics(this);
            this.LastCMaintenance = this.Airliner.BuiltDate;
            this.LastAMaintenance = this.Airliner.BuiltDate;
            this.LastBMaintenance = this.Airliner.BuiltDate;
            this.LastDMaintenance = this.Airliner.BuiltDate;
            this.Status = AirlinerStatus.Stopped;
            this.MaintRoutes = new List<RouteModel.Route>();

            this.CurrentPosition = new Coordinates(this.Homebase.Profile.Coordinates.Latitude, this.Homebase.Profile.Coordinates.Longitude);

            this.Routes = new List<Route>();
            this.Pilots = new List<Pilot>();
            this.InsurancePolicies = new List<AirlinerInsurance>();
            this.MaintenanceHistory = new Dictionary<Invoice, string>();
        }
        //adds a pilot to the airliner
        public void addPilot(Pilot pilot)
        {
            lock (this.Pilots)
            {
                this.Pilots.Add(pilot);
            }
        }
        //removes a pilot from the airliner
        public void removePilot(Pilot pilot)
        {
            lock (this.Pilots)
            {
                this.Pilots.Remove(pilot);
            }
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
