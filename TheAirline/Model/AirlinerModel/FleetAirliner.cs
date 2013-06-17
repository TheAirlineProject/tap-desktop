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
using ProtoBuf;

namespace TheAirline.Model.AirlinerModel
{
    [ProtoContract]
    public class FleetAirliner
    {
        [ProtoMember(1)]
        public Airliner Airliner { get; set; }
        [ProtoMember(2)]
        public string Name { get; set; }
        [ProtoMember(3,AsReference=true)]
        public Airport Homebase { get; set; }
        public enum PurchasedType { Bought, Leased,BoughtDownPayment }
        [ProtoMember(4)]
        public DateTime PurchasedDate { get; set; }
        [ProtoMember(5)]
        public PurchasedType Purchased { get; set; }
        public Boolean HasRoute { get { return this.Routes.Count > 0; } set { ;} }
        [ProtoMember(6)]
        public AirlinerStatistics Statistics { get; set; }
        [ProtoMember(7)]
        public DateTime LastAMaintenance { get; set; }
        [ProtoMember(8)]
        public int AMaintenanceInterval { get; set; }
        [ProtoMember(9)]
        public DateTime LastBMaintenance { get; set; }
        [ProtoMember(10)]
        public int BMaintenanceInterval { get; set; }
        [ProtoMember(11)]
        public DateTime LastCMaintenance { get; set; }
        [ProtoMember(12)]
        public int CMaintenanceInterval { get; set; }
        [ProtoMember(13)]
        public DateTime DueCMaintenance { get; set; }
        [ProtoMember(14)]
        public DateTime LastDMaintenance { get; set; }
        [ProtoMember(15)]
        public int DMaintenanceInterval { get; set; }
        [ProtoMember(16)]
        public DateTime DueDMaintenance { get; set; }
        [ProtoMember(17)]
        public DateTime SchedAMaintenance { get; set; }
        [ProtoMember(18)]
        public DateTime SchedBMaintenance { get; set; }
        [ProtoMember(19)]
        public DateTime SchedCMaintenance { get; set; }
        [ProtoMember(20)]
        public DateTime SchedDMaintenance { get; set; }
        [ProtoMember(21)]
        public DateTime OOSDate { get; set; }
        [ProtoMember(22)]
        public List<RouteModel.Route> MaintRoutes { get; set; }
        [ProtoMember(23)]
        public IDictionary<Invoice, String> MaintenanceHistory { get; set; }
        /*Changed for deleting routeairliner*/
        public enum AirlinerStatus { Stopped, On_route, On_service, Resting, To_homebase, To_route_start }
        [ProtoMember(24)]
        public AirlinerStatus Status { get; set; }
        [ProtoMember(25)]
        public Coordinates CurrentPosition { get; set; }
        [ProtoMember(26,AsReference=true)]
        public List<Route> Routes { get; private set; }
        [ProtoMember(27)]
        public Flight CurrentFlight { get; set; }
        [ProtoMember(28)]
        public DateTime GroundedToDate { get; set; }
        [ProtoMember(29,AsReference=true)]
        public List<Pilot> Pilots { get; set; }
        [ProtoMember(30)]
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
