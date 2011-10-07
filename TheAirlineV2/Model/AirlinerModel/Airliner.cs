using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirlineV2.Model.AirlineModel;
using TheAirlineV2.Model.GeneralModel;

namespace TheAirlineV2.Model.AirlinerModel
{
    //the class for an airliner
    public class Airliner
    {
        public string TailNumber { get; set; }
        public DateTime BuiltDate { get; set; }
        public AirlinerType Type { get; set; }
        public double Flown { get; set; } //km flown by the airliner
        public Airline Airline { get; set; }
        //private Dictionary<AirlinerFacility.FacilityType, AirlinerFacility> Facilities;
        public double LastServiceCheck { get; set; }  //the km were the airliner was last at service
        public long Price { get { return getPrice(); } private set { } }
        public long LeasingPrice { get { return getLeasingPrice(); } private set { } }
        public int Age { get { return getAge(); } private set { } }
        public List<AirlinerClass> Classes { get; set; }
        public Airliner(AirlinerType type, string tailNumber, DateTime builtDate)
        {
            this.BuiltDate = new DateTime(builtDate.Year,builtDate.Month,builtDate.Day);
            this.Type = type;
            this.LastServiceCheck = 0;
            this.TailNumber = tailNumber;
            this.Flown = 0;
            //this.Facilities = new Dictionary<AirlinerFacility.FacilityType, AirlinerFacility>();
            
            this.Classes = new List<AirlinerClass>();
            this.Classes.Add(new AirlinerClass(this,AirlinerClass.ClassType.Economy_Class, this.Type.MaxSeatingCapacity));
            
        }
        //gets the price for leasing the airliner per month
        public long getLeasingPrice()
        {

            double months = 20 * 12;
            double rate = 1.20;
            double leasingPrice = this.getPrice()*rate / months;
            return Convert.ToInt64(leasingPrice);
        }
        //gets the age of the airliner
        public int getAge()
        {
            return MathHelpers.CalculateAge(this.BuiltDate, GameObject.GetInstance().GameTime);
      
        }
        //gets the price for the airliner based on age
        public long getPrice()
        {
            double basePrice = this.Type.Price;

            /*
            AirlinerFacility audioFacility = this.getFacility(AirlinerFacility.FacilityType.Audio);
            AirlinerFacility videoFacility = this.getFacility(AirlinerFacility.FacilityType.Video);
            AirlinerFacility seatFacility = this.getFacility(AirlinerFacility.FacilityType.Seat);

            double audioPrice = audioFacility.PricePerSeat * audioFacility.PercentOfSeats * this.Type.SeatingCapacity;
            double videoPrice = videoFacility.PricePerSeat * videoFacility.PercentOfSeats * this.Type.SeatingCapacity;
            double seatPrice = seatFacility.PricePerSeat * seatFacility.PercentOfSeats * this.Type.SeatingCapacity;

            double facilityPrice = audioPrice + videoPrice + seatPrice;
            */
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

            int age=getAge();
            double devaluationPercent = 1 - (0.02 * (double)age);
            
            return Convert.ToInt64(basePrice * devaluationPercent);
        }
        
        /*
        //sets the facility for a facility type
        public void setFacility(AirlinerFacility facility)
        {
            this.Facilities[facility.Type] = facility;

            this.Airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime,Invoice.InvoiceType.Purchases,-facility.PricePerSeat * facility.PercentOfSeats * this.Type.SeatingCapacity));
        }
        //returns the current facility for a facility type
        public AirlinerFacility getFacility(AirlinerFacility.FacilityType type)
        {
            return this.Facilities[type];
        }
         * */
        //adds a new airliner class to the airliner
        public void addAirlinerClass(AirlinerClass airlinerClass)
        {
            this.Classes.Add(airlinerClass);
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
            int capacity=0;
            foreach (AirlinerClass aClass in this.Classes)
                capacity += aClass.SeatingCapacity;

            return capacity;
        }
        //returns the airliner class for the airliner
        public AirlinerClass getAirlinerClass(AirlinerClass.ClassType type)
        {
            return this.Classes.Find((delegate(AirlinerClass c) { return c.Type == type; }));
        }
       
    }
    //the list of airliners
    public class Airliners
    {
        private static Dictionary<string, Airliner> airliners = new Dictionary<string, Airliner>();
        //clears the list
        public static void Clear()
        {
            airliners = new Dictionary<string, Airliner>();
        }
        //adds an airliner to the list
        public static void AddAirliner(Airliner airliner)
        {
            airliners.Add(airliner.TailNumber, airliner);
        }
        //returns an airliner
        public static Airliner GetAirliner(string tailnumber)
        {
            return airliners[tailnumber];
        }
        //returns the list of airliners
        public static List<Airliner> GetAirliners()
        {
            return airliners.Values.ToList();
        }
        //returns the list of airliners for sell
        public static List<Airliner> GetAirlinersForSale()
        {
            return airliners.Values.ToList().FindAll((delegate(Airliner airliner) { return airliner.Airline == null; }));
        }
       
    }
    //the class for a passenger class on an airliner
    public class AirlinerClass
    {
        private Dictionary<AirlinerFacility.FacilityType, AirlinerFacility> Facilities;
        public enum ClassType { Economy_Class = 1, Business_Class = 2, First_Class = 3 }
        public ClassType Type { get; set; }
        public int RegularSeatingCapacity { get; set; }
        public int SeatingCapacity { get; set; }
        public Airliner Airliner { get; set; }
        public AirlinerClass(Airliner airliner,ClassType type, int seatingCapacity)
        {
            this.Type = type;
            this.Airliner = airliner;
            this.SeatingCapacity = seatingCapacity;
            this.RegularSeatingCapacity = seatingCapacity;
            this.Facilities = new Dictionary<AirlinerFacility.FacilityType, AirlinerFacility>();


            createBasicFacilities();
        }
        //sets the facility for a facility type
        public void setFacility(AirlinerFacility facility)
        {
            this.Facilities[facility.Type] = facility;

            if (this.Airliner.Airline != null)
               this.Airliner.Airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -facility.PricePerSeat * facility.PercentOfSeats / 100.0 * this.SeatingCapacity));
        }
        //force sets the facility for a facility type without cost
        public void forceSetFacility(AirlinerFacility facility)
        {
            this.Facilities[facility.Type] = facility;

        }
        //returns the current facility for a facility type
        public AirlinerFacility getFacility(AirlinerFacility.FacilityType type)
        {
            return this.Facilities[type];
        }
        //creates the basic facilities
        private void createBasicFacilities()
        {
            foreach (AirlinerFacility.FacilityType type in Enum.GetValues(typeof(AirlinerFacility.FacilityType)))
            {
                this.setFacility(AirlinerFacilities.GetBasicFacility(type));
                //this.Facilities.Add(type, AirlinerFacilities.GetBasicFacility(type));
            }

        }
    }
}