using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.InvoicesModel;
using TheAirline.Model.PilotModel;
using TheAirline.Model.RouteModel;

namespace TheAirline.Model.AirlinerModel
{
    [Serializable]
    public class FleetAirliner : BaseModel
    {
        #region Constructors and Destructors

        public FleetAirliner(
            PurchasedType purchased,
            DateTime purchasedDate,
            Airline airline,
            Airliner airliner,
            Airport homebase)
        {
            Airliner = airliner;
            Purchased = purchased;
            PurchasedDate = purchasedDate;
            Airliner.Airline = airline;
            Homebase = homebase;
            Name = airliner.TailNumber;
            Statistics = new AirlinerStatistics(this);
            LastCMaintenance = Airliner.BuiltDate;
            LastAMaintenance = Airliner.BuiltDate;
            LastBMaintenance = Airliner.BuiltDate;
            LastDMaintenance = Airliner.BuiltDate;
            Status = AirlinerStatus.Stopped;
            MaintRoutes = new List<Route>();

            CurrentPosition = Homebase;
            //new GeoCoordinate(this.Homebase.Profile.Coordinates.Latitude,this.Homebase.Profile.Coordinates.Longitude);

            Routes = new List<Route>();
            Pilots = new List<Pilot>();
            InsurancePolicies = new List<AirlinerInsurance>();
            MaintenanceHistory = new Dictionary<Invoice, string>();

            Data = new OperatingData();

            if (Purchased == PurchasedType.Bought || Purchased == PurchasedType.BoughtDownPayment)
                Airliner.Owner = Airliner.Airline;
        }

        private FleetAirliner(SerializationInfo info, StreamingContext ctxt)
        {
            if (Version == 1)
                Data = new OperatingData();
        }

        #endregion

        #region Enums

        public enum AirlinerStatus
        {
            Stopped,

            OnRoute,

            OnService,

            OnCharter,

            Resting,

            ToHomebase,

            ToRouteStart
        }

        public enum PurchasedType
        {
            Bought,

            Leased,

            BoughtDownPayment
        }

        #endregion

        #region Public Properties

        [Versioning("ainterval")]
        public int AMaintenanceInterval { get; set; }

        [Versioning("airliner")]
        public Airliner Airliner { get; set; }

        [Versioning("binterval")]
        public int BMaintenanceInterval { get; set; }

        [Versioning("cinterval")]
        public int CMaintenanceInterval { get; set; }

        [Versioning("currentflight")]
        public Flight CurrentFlight { get; set; }

        [Versioning("currentposition")]
        public Airport CurrentPosition { get; set; }

        [Versioning("dinterval")]
        public int DMaintenanceInterval { get; set; }

        [Versioning("duecmaintenance")]
        public DateTime DueCMaintenance { get; set; }

        [Versioning("duedmaintenance")]
        public DateTime DueDMaintenance { get; set; }

        [Versioning("groundedto")]
        public DateTime GroundedToDate { get; set; }

        [Versioning("data", Version = 2)]
        public OperatingData Data { get; set; }

        public Boolean HasRoute
        {
            get { return Routes.Count > 0; }
        }

        [Versioning("homebase")]
        public Airport Homebase { get; set; }

        [Versioning("insurancepolicies")]
        public List<AirlinerInsurance> InsurancePolicies { get; set; }

        [Versioning("lastamaintenance")]
        public DateTime LastAMaintenance { get; set; }

        [Versioning("lastbmaintenance")]
        public DateTime LastBMaintenance { get; set; }

        [Versioning("lastcmaintenance")]
        public DateTime LastCMaintenance { get; set; }

        [Versioning("lastdmaintenance")]
        public DateTime LastDMaintenance { get; set; }

        [Versioning("maintroutes")]
        public List<Route> MaintRoutes { get; set; }

        [Versioning("maintenancehistory")]
        public IDictionary<Invoice, String> MaintenanceHistory { get; set; }

        [Versioning("name")]
        public string Name { get; set; }

        public int NumberOfPilots
        {
            get { return Pilots.Count; }
        }

        [Versioning("oosdate")]
        public DateTime OOSDate { get; set; }

        [Versioning("pilots")]
        public List<Pilot> Pilots { get; set; }

        [Versioning("purchased")]
        public PurchasedType Purchased { get; set; }

        [Versioning("date")]
        public DateTime PurchasedDate { get; set; }

        [Versioning("routes")]
        public List<Route> Routes { get; private set; }

        [Versioning("schedamaintenance")]
        public DateTime SchedAMaintenance { get; set; }

        [Versioning("schedbmaintenance")]
        public DateTime SchedBMaintenance { get; set; }

        [Versioning("schedcmaintenance")]
        public DateTime SchedCMaintenance { get; set; }

        [Versioning("scheddmaintenance")]
        public DateTime SchedDMaintenance { get; set; }

        [Versioning("statistics")]
        public AirlinerStatistics Statistics { get; set; }

        [Versioning("status")]
        public AirlinerStatus Status { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 3);

            base.GetObjectData(info, context);
        }

        //adds a pilot to the airliner
        public void AddPilot(Pilot pilot)
        {
            lock (Pilots)
            {
                Pilots.Add(pilot);
                pilot.Airliner = this;
            }
        }

        //removes a pilot from the airliner

        //adds a route to the airliner
        public void AddRoute(Route route)
        {
            Routes.Add(route);
        }

        public void RemovePilot(Pilot pilot)
        {
            lock (Pilots)
            {
                Pilots.Remove(pilot);
                pilot.Airliner = null;
            }
        }

        //removes a route from the airliner
        public void RemoveRoute(Route route)
        {
            Routes.Remove(route);
            route.TimeTable.Entries.RemoveAll(e => e.Airliner == this);
        }

        #endregion
    }
}