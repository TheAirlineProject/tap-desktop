using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirlinerModel
{
    //the class for an airliner
    [Serializable]
    public class Airliner
    {
        public string TailNumber { get; set; }
        public string ID { get; set; }
        public DateTime BuiltDate { get; set; }
        public AirlinerType Type { get; set; }
        public double Flown { get; set; } //distance flown by the airliner
        public Airline Airline { get; set; }
        public double LastServiceCheck { get; set; }  //the km were the airliner was last at service
        public DateTime LastAMaintenance { get; set; }
        public int AMaintenanceInterval { get; set; }
        public DateTime LastBMaintenance { get; set; }
        public int BMaintenanceInterval { get; set; }
        public DateTime LastCMaintenance { get; set; }
        public DateTime DueCMaintenance { get; set; }
        public DateTime LastDMaintenance { get; set; }
        public DateTime DueDMaintenance { get; set; }
        public DateTime SchedAMaintenance { get; set; }
        public DateTime SchedBMaintenance { get; set; }
        public DateTime SchedCMaintenance { get; set; }
        public DateTime SchedDMaintenance { get; set; }
        public DateTime OOSDate { get; set; }
        public List<RouteModel.Route> MaintRoutes { get; set; }
        public IDictionary<Invoice, String> MaintenanceHistory { get; set; }
        public long Price { get { return getPrice(); } private set { } }
        public long LeasingPrice { get { return getLeasingPrice(); } private set { } }
        public long FuelCapacity { get; set; }
        public double Damaged { get; set; }
        public int Age { get { return getAge(); } private set { } }
        public List<AirlinerClass> Classes { get; set; }
        private Random rnd = new Random();
        public Airliner(string id, AirlinerType type, string tailNumber, DateTime builtDate)
        {
            this.ID = id;
            this.BuiltDate = new DateTime(builtDate.Year, builtDate.Month, builtDate.Day);
            this.Type = type;
            this.LastServiceCheck = 0;
            this.LastCMaintenance = this.BuiltDate;
            this.LastAMaintenance = this.BuiltDate;
            this.LastBMaintenance = this.BuiltDate;
            this.LastDMaintenance = this.BuiltDate;
            this.TailNumber = tailNumber;
            this.Flown = 0;
            this.Damaged = rnd.Next(90, 100);
            this.Classes = new List<AirlinerClass>();
            this.MaintRoutes = new List<RouteModel.Route>();

            if (this.Type.TypeAirliner == AirlinerType.TypeOfAirliner.Passenger)
            {
                AirlinerClass aClass = new AirlinerClass(AirlinerClass.ClassType.Economy_Class, ((AirlinerPassengerType)this.Type).MaxSeatingCapacity);
                aClass.createBasicFacilities(this.Airline);
                this.Classes.Add(aClass);
            }

            if (this.Type.TypeAirliner == AirlinerType.TypeOfAirliner.Cargo)
            {
                AirlinerClass aClass = new AirlinerClass(AirlinerClass.ClassType.Economy_Class, 0);
                aClass.createBasicFacilities(this.Airline);
                this.Classes.Add(aClass);

            }

        }
        // mjd 08/30/12 leasing price recalculated for 15 years
        //gets the price for leasing the airliner per month
        public long getLeasingPrice()
        {


            double months = 20 * 15;
            double rate = 1.20;

            double leasingPrice = (this.getPrice() * rate / months);
            return Convert.ToInt64(leasingPrice);
        }
        //gets the age of the airliner
        public int getAge()
        {
            return MathHelpers.CalculateAge(this.BuiltDate, GameObject.GetInstance().GameTime);

        }

        //returns depreciated airliner value (3% per year or 20% value if over 25 years old)
        public long getValue()
        {
            if (getAge() < 25)
            {
                return getPrice() * (1 - (long)getAge() * (3 / 100));
            }
            else return getPrice() * (20 / 100);
        }

        //gets the price for the airliner based on age
        public long getPrice()
        {
            double basePrice = this.Type.Price;

            double facilityPrice = 0;

            foreach (AirlinerClass aClass in this.Classes)
            {
                AirlinerFacility audioFacility = aClass.getFacility(AirlinerFacility.FacilityType.Audio);
                AirlinerFacility videoFacility = aClass.getFacility(AirlinerFacility.FacilityType.Video);
                AirlinerFacility seatFacility = aClass.getFacility(AirlinerFacility.FacilityType.Seat);

                double audioPrice = audioFacility.PricePerSeat * audioFacility.PercentOfSeats * aClass.SeatingCapacity;
                double videoPrice = videoFacility.PricePerSeat * videoFacility.PercentOfSeats * aClass.SeatingCapacity;
                double seatPrice = seatFacility.PricePerSeat * seatFacility.PercentOfSeats * aClass.SeatingCapacity;

                facilityPrice += audioPrice + videoPrice + seatPrice;
            }


            basePrice += facilityPrice;

            int age = getAge();
            double devaluationPercent = 1 - (0.02 * (double)age);


            return Convert.ToInt64(basePrice * devaluationPercent * (this.Damaged / 100));
        }


        //adds a new airliner class to the airliner
        public void addAirlinerClass(AirlinerClass airlinerClass)
        {
            this.Classes.Add(airlinerClass);

            if (airlinerClass.getFacilities().Count == 0)
                airlinerClass.createBasicFacilities(this.Airline);
        }
        //removes an airliner class from the airliner
        public void removeAirlinerClass(AirlinerClass airlinerClass)
        {
            this.Classes.Remove(airlinerClass);
        }
        //clear the list of airliner classes
        public void clearAirlinerClasses()
        {
            this.Classes.Clear();
        }
        //returns the total amount of seat capacity
        public int getTotalSeatCapacity()
        {
            int capacity = 0;
            foreach (AirlinerClass aClass in this.Classes)
                capacity += aClass.SeatingCapacity;

            return capacity;
        }
        //returns the cargo capacity of the airliner
        public double getCargoCapacity()
        {
            if (this.Type is AirlinerCargoType)
                return ((AirlinerCargoType)this.Type).CargoSize;
            else
                return 0;
        }
        //returns the airliner class for the airliner
        public AirlinerClass getAirlinerClass(AirlinerClass.ClassType type)
        {
            if (this.Classes.Exists(c => c.Type == type))
                return this.Classes.Find(c => c.Type == type);
            else
                return this.Classes[0];
        }

        //does the maintenance of a given type, sends the invoice, updates the last/next maintenance, and improves the aircraft's damage
        //make sure you pass this function a string value of either "A" "B" "C" or "D" or it will throw an error!
        public void doMaintenance(FleetAirliner airliner)
        {
            Random rnd = new Random();
            if (airliner.Airliner.SchedAMaintenance == GameObject.GetInstance().GameTime.Date)
            {
                double expense = (airliner.Airliner.getValue() * 0.01) + 2000;
                GameObject.GetInstance().HumanAirline.Money -= expense;
                Invoice maintCheck = new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Maintenances, expense);
                airliner.Airliner.Airline.addInvoice(maintCheck);
                airliner.Airliner.Damaged += rnd.Next(3, 10);
                if (airliner.Airliner.Damaged > 100) airliner.Airliner.Damaged = 100;
                airliner.Airliner.LastAMaintenance = GameObject.GetInstance().GameTime;
                airliner.Airliner.SchedAMaintenance = airliner.Airliner.SchedAMaintenance.AddDays(airliner.Airliner.AMaintenanceInterval);
                airliner.Airliner.MaintenanceHistory.Add(maintCheck, "A");
            }

            if (airliner.Airliner.SchedBMaintenance == GameObject.GetInstance().GameTime.Date)
            {
                double expense = (airliner.Airliner.getValue() * 0.02) + 4500;
                GameObject.GetInstance().HumanAirline.Money -= expense;
                Invoice maintCheck = new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Maintenances, expense);
                airliner.Airliner.Airline.addInvoice(maintCheck);
                airliner.Airliner.Damaged += rnd.Next(12, 20);
                if (airliner.Airliner.Damaged > 100) airliner.Airliner.Damaged = 100;
                airliner.Airliner.LastBMaintenance = GameObject.GetInstance().GameTime;
                airliner.Airliner.SchedBMaintenance = airliner.Airliner.SchedBMaintenance.AddDays(airliner.Airliner.BMaintenanceInterval);
                airliner.Airliner.MaintenanceHistory.Add(maintCheck, "B");
            }

            if (airliner.Airliner.SchedCMaintenance == GameObject.GetInstance().GameTime.Date)
            {
                double expense = (airliner.Airliner.getValue() * 0.025) + 156000;
                airliner.Airliner.OOSDate = SchedCMaintenance.AddDays(airliner.Airliner.Damaged + 20);
                GameObject.GetInstance().HumanAirline.Money -= expense;
                Invoice maintCheck = new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Maintenances, expense);
                airliner.Airliner.Airline.addInvoice(maintCheck);
                airliner.Airliner.Damaged += rnd.Next(20, 30);
                if (airliner.Airliner.Damaged > 100) airliner.Airliner.Damaged = 100;
                airliner.Airliner.LastCMaintenance = GameObject.GetInstance().GameTime;
                airliner.Airliner.DueCMaintenance = GameObject.GetInstance().GameTime.AddMonths(18);
                airliner.Airliner.MaintenanceHistory.Add(maintCheck, "C");
                foreach (RouteModel.Route r in airliner.Routes.ToList())
                {
                    airliner.Airliner.MaintRoutes.Add(r);
                    airliner.Routes.Remove(r);
                }
            }

            if (airliner.Airliner.SchedDMaintenance == GameObject.GetInstance().GameTime.Date)
            {
                double expense = (airliner.Airliner.getValue() * 0.03) + 1200000;
                airliner.Airliner.OOSDate = SchedDMaintenance.AddDays(airliner.Airliner.Damaged + 50);
                GameObject.GetInstance().HumanAirline.Money -= expense;
                Invoice maintCheck = new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Maintenances, expense);
                airliner.Airliner.Airline.addInvoice(maintCheck);
                airliner.Airliner.Damaged += rnd.Next(35, 50);
                if (airliner.Airliner.Damaged > 100) airliner.Airliner.Damaged = 100;
                airliner.Airliner.LastDMaintenance = GameObject.GetInstance().GameTime;
                airliner.Airliner.DueDMaintenance = GameObject.GetInstance().GameTime.AddMonths(60);
                airliner.Airliner.MaintenanceHistory.Add(maintCheck, "D");
                foreach (RouteModel.Route r in airliner.Routes.ToList())
                {
                    airliner.Airliner.MaintRoutes.Add(r);
                    airliner.Routes.Remove(r);
                }
            }
        }

        //restores routes removed for maintenance
        public void RestoreMaintRoutes(FleetAirliner airliner)
        {
            if (airliner.Airliner.OOSDate <= GameObject.GetInstance().GameTime)
            {
                foreach (RouteModel.Route r in airliner.Airliner.MaintRoutes)
                {
                    airliner.Routes.Add(r);
                }

                airliner.Airliner.MaintRoutes.Clear();
            }
        }

        //sets A and B check intervals
        public void SetMaintenanceIntervals(Airliner airliner, int a, int b)
        {
            airliner.AMaintenanceInterval = a;
            airliner.BMaintenanceInterval = b;
        }

    }
    //the list of airliners
    public class Airliners
    {
        private static List<Airliner> airliners = new List<Airliner>();
        //clears the list
        public static void Clear()
        {
            airliners = new List<Airliner>();
        }
        //adds an airliner to the list
        public static void AddAirliner(Airliner airliner)
        {
            lock (airliners)
            {
                //if (airliners.Exists(a => a.ID == airliner.ID))
                //  throw new Exception("Airliner element already exists exception");

                airliners.Add(airliner);
            }
        }
        //returns an airliner
        public static Airliner GetAirliner(string tailnumber)
        {
            return airliners.Find(delegate(Airliner airliner) { return airliner.TailNumber == tailnumber; });
        }
        //returns the list of airliners
        public static List<Airliner> GetAllAirliners()
        {
            return airliners;
        }

        //returns the list of airliners for sale
        public static List<Airliner> GetAirlinersForSale()
        {
            return airliners.FindAll((delegate(Airliner airliner) { return airliner.Airline == null; }));
        }
        //returns the list of airliners for sale
        public static List<Airliner> GetAirlinersForSale(Predicate<Airliner> match)
        {
            return airliners.FindAll(a => a.Airline == null).FindAll(match);
        }
        //removes an airliner from the list
        public static void RemoveAirliner(Airliner airliner)
        {
            airliners.Remove(airliner);
        }


    }
}