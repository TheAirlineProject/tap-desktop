
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirlineModel
{
    //the class for an airlines facilities
    [Serializable]
    public class AirlineFacility
    {

        
        public static string Section { get; set; }
        
        public string Uid { get; set; }
        
        private double APrice;
        public double Price { get { return GeneralHelpers.GetInflationPrice(this.APrice); } set { this.APrice = value; } }
        
        public double MonthlyCost { get; set; }
        
        public int LuxuryLevel { get; set; } //for business customers
        
        public int ServiceLevel { get; set; } //for repairing airliners 
        
        public int FromYear { get; set; }
        public AirlineFacility(string section, string uid, double price, double monthlyCost,int fromYear, int serviceLevel, int luxuryLevel)
        {
            AirlineFacility.Section = section;
            this.Uid = uid;
            this.FromYear = fromYear;
            this.MonthlyCost = monthlyCost;
            this.Price = price;
            this.LuxuryLevel = luxuryLevel;
            this.ServiceLevel = serviceLevel;
        }
        public string Name
        {
            get { return Translator.GetInstance().GetString(AirlineFacility.Section, this.Uid); }
        }

        public string Shortname
        {
            get { return Translator.GetInstance().GetString(AirlineFacility.Section, this.Uid, "shortname"); }
        }
    }
    //the collection of facilities
    public class AirlineFacilities
    {
        private static List<AirlineFacility> facilities = new List<AirlineFacility>();
         //clears the list
        public static void Clear()
        {
            facilities = new List<AirlineFacility>();
        }
        //adds a new facility to the collection
        public static void AddFacility(AirlineFacility facility)
        {
            facilities.Add(facility);
        }
        //returns a facility
        public static AirlineFacility GetFacility(string uid)
        {
            return facilities.Find(f => f.Uid == uid);
        }
        //returns the list of facilities
        public static List<AirlineFacility> GetFacilities()
        {
            return facilities;
        }
        public static List<AirlineFacility> GetFacilities(Predicate<AirlineFacility> match)
        {
            return facilities.FindAll(match);
        }
    }
}
