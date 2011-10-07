using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.AirlineModel
{
    //the class for an airlines facilities
    public class AirlineFacility
    {
        public string Name { get; set; }
        public string Shortname { get; set; }
        public double Price { get; set; }
        public int LuxuryLevel { get; set; } //for business customers
        public int ServiceLevel { get; set; } //for repairing airliners 
        public AirlineFacility(string name, string shortname, double price, int serviceLevel, int luxuryLevel)
        {
            this.Name = name;
            this.Shortname = shortname;
            this.Price = price;
            this.LuxuryLevel = luxuryLevel;
            this.ServiceLevel = serviceLevel;
        }
    }
    //the collection of facilities
    public class AirlineFacilities
    {
        private static Dictionary<string, AirlineFacility> facilities = new Dictionary<string, AirlineFacility>();
         //clears the list
        public static void Clear()
        {
            facilities = new Dictionary<string, AirlineFacility>();
        }
        //adds a new facility to the collection
        public static void AddFacility(AirlineFacility facility)
        {
            facilities.Add(facility.Shortname, facility);
        }
        //returns a facility
        public static AirlineFacility GetFacility(string shortname)
        {
            return facilities[shortname];
        }
        //returns the list of facilities
        public static List<AirlineFacility> GetFacilities()
        {
            return facilities.Values.ToList();
        }
    }
}
