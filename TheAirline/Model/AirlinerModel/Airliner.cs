using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirlinerModel
{
    //the class for an airliner
    public class Airliner
    {
        public string TailNumber { get; set; }
        public DateTime BuiltDate { get; set; }
        public AirlinerType Type { get; set; }
        public double Flown { get; set; } //distance flown by the airliner
        public Airline Airline { get; set; }
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
             
            this.Classes = new List<AirlinerClass>();
            
            if (this.Type.TypeAirliner == AirlinerType.TypeOfAirliner.Passenger)
                this.Classes.Add(new AirlinerClass(this,AirlinerClass.ClassType.Economy_Class, ((AirlinerPassengerType)this.Type).MaxSeatingCapacity));

            if (this.Type.TypeAirliner == AirlinerType.TypeOfAirliner.Cargo)
                this.Classes.Add(new AirlinerClass(this, AirlinerClass.ClassType.Economy_Class, 0));
            
        }
        // chs, 2011-10-10 changed the leasing price to 5 years
        //gets the price for leasing the airliner per month
        public long getLeasingPrice()
        {

            double months = 20 * 5;
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
        private static List<Airliner> airliners = new List<Airliner>();
        //clears the list
        public static void Clear()
        {
            airliners = new List<Airliner>();
        }
        //adds an airliner to the list
        public static void AddAirliner(Airliner airliner)
        {
            airliners.Add(airliner);
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

        //returns the list of airliners for sell
        public static List<Airliner> GetAirlinersForSale()
        {
            return airliners.FindAll((delegate(Airliner airliner) { return airliner.Airline == null; }));
        }
       
  
       
    }
   
}