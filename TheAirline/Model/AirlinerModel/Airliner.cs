
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

        public Country Registered { get { return Countries.GetCountryFromTailNumber(this.TailNumber);} private set { ;} }
        public string TailNumber { get; set; }
        
        public string ID { get; set; }
        
        public DateTime BuiltDate { get; set; }
        
        public AirlinerType Type { get; set; }
        
        public double Flown { get; set; } //distance flown by the airliner
        
        public Airline Airline { get; set; }
        
        public double LastServiceCheck { get; set; }  //the km were the airliner was last at service
        public long Price { get { return getPrice(); } private set { } }
        public long LeasingPrice { get { return getLeasingPrice(); } private set { } }
        
        public long FuelCapacity { get; set; }
        
        public double Condition { get; set; }
        public int Age { get { return getAge(); } private set { } }
        
        public List<AirlinerClass> Classes { get; set; }
        private Random rnd = new Random();
        public Airliner(string id, AirlinerType type, string tailNumber, DateTime builtDate)
        {
            this.ID = id;
            this.BuiltDate = new DateTime(builtDate.Year, builtDate.Month, builtDate.Day);
            this.Type = type;
            this.LastServiceCheck = 0;
            this.TailNumber = tailNumber;
            this.Flown = 0;
            this.Condition = rnd.Next(90, 100);
            this.Classes = new List<AirlinerClass>();

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


            return Convert.ToInt64(basePrice * devaluationPercent * (this.Condition / 100));
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