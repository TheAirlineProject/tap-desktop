using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Device.Location;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel.StatisticsModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.PilotModel;
using System.Runtime.Serialization;
using System.Reflection;


namespace TheAirline.Model.AirlinerModel
{
    [Serializable]
    public class FleetAirliner : ISerializable
    {
        [Versioning("airliner")]
        public Airliner Airliner { get; set; }
        [Versioning("name")]
        public string Name { get; set; }
        [Versioning("homebase")]
        public Airport Homebase { get; set; }
        public enum PurchasedType { Bought, Leased, BoughtDownPayment }
        [Versioning("date")]
        public DateTime PurchasedDate { get; set; }
        [Versioning("purchased")]
        public PurchasedType Purchased { get; set; }
        public Boolean HasRoute { get { return this.Routes.Count > 0; } set { ;} }
        [Versioning("statistics")]
        public AirlinerStatistics Statistics { get; set; }
        [Versioning("lastamaintenance")]
        public DateTime LastAMaintenance { get; set; }
          [Versioning("ainterval")]
        public int AMaintenanceInterval { get; set; }
          [Versioning("lastbmaintenance")]
        public DateTime LastBMaintenance { get; set; }
          [Versioning("binterval")]
        public int BMaintenanceInterval { get; set; }
          [Versioning("lastcmaintenance")]
        public DateTime LastCMaintenance { get; set; }
          [Versioning("cinterval")]
        public int CMaintenanceInterval { get; set; }
          [Versioning("duecmaintenance")]
        public DateTime DueCMaintenance { get; set; }
          [Versioning("lastdmaintenance")]
        public DateTime LastDMaintenance { get; set; }
          [Versioning("dinterval")]
        public int DMaintenanceInterval { get; set; }
          [Versioning("duedmaintenance")]
        public DateTime DueDMaintenance { get; set; }
          [Versioning("schedamaintenance")]
        public DateTime SchedAMaintenance { get; set; }
          [Versioning("schedbmaintenance")]
        public DateTime SchedBMaintenance { get; set; }
          [Versioning("schedcmaintenance")]
        public DateTime SchedCMaintenance { get; set; }
          [Versioning("scheddmaintenance")]
        public DateTime SchedDMaintenance { get; set; }
          [Versioning("oosdate")]
        public DateTime OOSDate { get; set; }
          [Versioning("maintroutes")]
        public List<RouteModel.Route> MaintRoutes { get; set; }
          [Versioning("maintenancehistory")]
        public IDictionary<Invoice, String> MaintenanceHistory { get; set; }
        /*Changed for deleting routeairliner*/
        public enum AirlinerStatus { Stopped, On_route, On_service, Resting, To_homebase, To_route_start }
          [Versioning("status")]
        public AirlinerStatus Status { get; set; }
          [Versioning("currentposition")]
        public Airport CurrentPosition { get; set; }
          [Versioning("routes")]
        public List<Route> Routes { get; private set; }
          [Versioning("currentflight")]
        public Flight CurrentFlight { get; set; }
          [Versioning("groundedto")]
        public DateTime GroundedToDate { get; set; }
          [Versioning("pilots")]
        public List<Pilot> Pilots { get; set; }
          [Versioning("insurancepolicies")]
        public List<AirlinerInsurance> InsurancePolicies { get; set; }
        public int NumberOfPilots { get { return this.Pilots.Count; } private set { ;} }
        public FleetAirliner(PurchasedType purchased, DateTime purchasedDate, Airline airline, Airliner airliner, Airport homebase)
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

            this.CurrentPosition = this.Homebase;//new GeoCoordinate(this.Homebase.Profile.Coordinates.Latitude,this.Homebase.Profile.Coordinates.Longitude);
      
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
                pilot.Airliner = this;
            }
        }
        //removes a pilot from the airliner
        public void removePilot(Pilot pilot)
        {
            lock (this.Pilots)
            {
                this.Pilots.Remove(pilot);
                pilot.Airliner = null;
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
          private FleetAirliner(SerializationInfo info, StreamingContext ctxt)
        {
            int version = info.GetInt16("version");

            var fields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null);

            IList<PropertyInfo> props = new List<PropertyInfo>(this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null));

            var propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

            foreach (SerializationEntry entry in info)
            {
                MemberInfo prop = propsAndFields.FirstOrDefault(p => ((Versioning)p.GetCustomAttribute(typeof(Versioning))).Name == entry.Name);


                if (prop != null)
                {
                    if (prop is FieldInfo)
                        ((FieldInfo)prop).SetValue(this, entry.Value);
                    else
                        ((PropertyInfo)prop).SetValue(this, entry.Value);
                }
            }

            var notSetProps = propsAndFields.Where(p => ((Versioning)p.GetCustomAttribute(typeof(Versioning))).Version > version);

            foreach (MemberInfo notSet in notSetProps)
            {
                Versioning ver = (Versioning)notSet.GetCustomAttribute(typeof(Versioning));

                if (ver.AutoGenerated)
                {
                    if (notSet is FieldInfo)
                        ((FieldInfo)notSet).SetValue(this, ver.DefaultValue);
                    else
                        ((PropertyInfo)notSet).SetValue(this, ver.DefaultValue);

                }

            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            Type myType = this.GetType();

            var fields = myType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null);

            IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null));

            var propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

            foreach (MemberInfo member in propsAndFields)
            {
                object propValue;

                if (member is FieldInfo)
                    propValue = ((FieldInfo)member).GetValue(this);
                else
                    propValue = ((PropertyInfo)member).GetValue(this, null);

                Versioning att = (Versioning)member.GetCustomAttribute(typeof(Versioning));

                info.AddValue(att.Name, propValue);
            }
        }


    }

}
