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

        //does the maintenance of a given type, sends the invoice, updates the last/next maintenance, and improves the aircraft's damage
        //make sure you pass this function a string value of either "A" "B" "C" or "D" or it will throw an error!
        public void DoMaintenance(FleetAirliner airliner)
        {
            Random rnd = new Random();
            if (airliner.SchedAMaintenance == GameObject.GetInstance().GameTime.Date)
            {
                double expense = (airliner.Airliner.getValue() * 0.01) + 2000;
                GameObject.GetInstance().HumanAirline.Money -= expense;
                Invoice maintCheck = new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Maintenances, expense);
                airliner.Airliner.Airline.addInvoice(maintCheck);
                airliner.Airliner.Damaged += rnd.Next(3, 10);
                if (airliner.Airliner.Damaged > 100) airliner.Airliner.Damaged = 100;
                airliner.LastAMaintenance = GameObject.GetInstance().GameTime;
                airliner.SchedAMaintenance = airliner.SchedAMaintenance.AddDays(airliner.AMaintenanceInterval);
                airliner.MaintenanceHistory.Add(maintCheck, "A");
            }

            if (airliner.SchedBMaintenance == GameObject.GetInstance().GameTime.Date)
            {
                double expense = (airliner.Airliner.getValue() * 0.02) + 4500;
                GameObject.GetInstance().HumanAirline.Money -= expense;
                Invoice maintCheck = new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Maintenances, expense);
                airliner.Airliner.Airline.addInvoice(maintCheck);
                airliner.Airliner.Damaged += rnd.Next(12, 20);
                if (airliner.Airliner.Damaged > 100) airliner.Airliner.Damaged = 100;
                airliner.LastBMaintenance = GameObject.GetInstance().GameTime;
                airliner.SchedBMaintenance = airliner.SchedBMaintenance.AddDays(airliner.BMaintenanceInterval);
                airliner.MaintenanceHistory.Add(maintCheck, "B");
            }

            if (airliner.SchedCMaintenance == GameObject.GetInstance().GameTime.Date)
            {
                double expense = (airliner.Airliner.getValue() * 0.025) + 156000;
                airliner.OOSDate = SchedCMaintenance.AddDays(airliner.Airliner.Damaged + 20);
                GameObject.GetInstance().HumanAirline.Money -= expense;
                Invoice maintCheck = new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Maintenances, expense);
                airliner.Airliner.Airline.addInvoice(maintCheck);
                airliner.Airliner.Damaged += rnd.Next(20, 30);
                if (airliner.Airliner.Damaged > 100) airliner.Airliner.Damaged = 100;
                airliner.LastCMaintenance = GameObject.GetInstance().GameTime;
                airliner.SchedCMaintenance = airliner.CMaintenanceInterval > -1 ?  airliner.SchedCMaintenance.AddMonths(CMaintenanceInterval) : DueCMaintenance = GameObject.GetInstance().GameTime.AddMonths(18);
                airliner.MaintenanceHistory.Add(maintCheck, "C");
                foreach (RouteModel.Route r in airliner.Routes.ToList())
                {
                    airliner.MaintRoutes.Add(r);
                    airliner.Routes.Remove(r);
                }
            }

            if (airliner.SchedDMaintenance == GameObject.GetInstance().GameTime.Date)
            {
                double expense = (airliner.Airliner.getValue() * 0.03) + 1200000;
                airliner.OOSDate = SchedDMaintenance.AddDays(airliner.Airliner.Damaged + 50);
                GameObject.GetInstance().HumanAirline.Money -= expense;
                Invoice maintCheck = new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Maintenances, expense);
                airliner.Airliner.Airline.addInvoice(maintCheck);
                airliner.Airliner.Damaged += rnd.Next(35, 50);
                if (airliner.Airliner.Damaged > 100) airliner.Airliner.Damaged = 100;
                airliner.LastDMaintenance = GameObject.GetInstance().GameTime;
                airliner.SchedDMaintenance = airliner.DMaintenanceInterval > -1 ? airliner.SchedDMaintenance.AddMonths(DMaintenanceInterval) : DueDMaintenance = GameObject.GetInstance().GameTime.AddMonths(60);
                airliner.DueDMaintenance = GameObject.GetInstance().GameTime.AddMonths(60);
                airliner.MaintenanceHistory.Add(maintCheck, "D");
                foreach (RouteModel.Route r in airliner.Routes.ToList())
                {
                    airliner.MaintRoutes.Add(r);
                    airliner.Routes.Remove(r);
                }
            }
        }

        //restores routes removed for maintenance
        public void RestoreMaintRoutes(FleetAirliner airliner)
        {
            if (airliner.OOSDate <= GameObject.GetInstance().GameTime)
            {
                foreach (RouteModel.Route r in airliner.MaintRoutes)
                {
                    airliner.Routes.Add(r);
                }

                airliner.MaintRoutes.Clear();
            }
        }

        //sets A and B check intervals
        public void SetMaintenanceIntervals(FleetAirliner airliner, int a, int b)
        {
            airliner.AMaintenanceInterval = a;
            airliner.BMaintenanceInterval = b;
            airliner.CMaintenanceInterval = -1;
            airliner.DMaintenanceInterval = -1;
        }

        public void SetMaintenanceIntervals(FleetAirliner airliner, int a, int b, int c)
        {
            if (airliner.CMaintenanceInterval == -1)
            {
                airliner.AMaintenanceInterval = a;
                airliner.BMaintenanceInterval = b;
                airliner.DMaintenanceInterval = c;
            }

            else if (airliner.DMaintenanceInterval == -1)
            {
                airliner.AMaintenanceInterval = a;
                airliner.BMaintenanceInterval = b;
                airliner.CMaintenanceInterval = c;
            }
        }

        public void SetMaintenanceIntervals(FleetAirliner airliner, int a, int b, int c, int d)
        {
            airliner.AMaintenanceInterval = a;
            airliner.BMaintenanceInterval = b;
            airliner.CMaintenanceInterval = c;
            airliner.DMaintenanceInterval = d;
        }
       
    }
  
}
