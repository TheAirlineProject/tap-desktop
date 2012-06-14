using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.PassengerModel
{
    /*
     * The class for flight restrictions with no flights between two countries
     */
    public class FlightRestriction
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Country Country1 { get; set; }
        public Country Country2 { get; set; }
        public FlightRestriction(DateTime startDate, DateTime endDate, Country country1, Country country2)
        {
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.Country1 = country1;
            this.Country2 = country2;
        }
    }
    //the list of flight restrictions
    public class FlightRestrictions
    {
        private static List<FlightRestriction> restrictions = new List<FlightRestriction>();
        //adds a flight restriction to the list of restrictions
        public static void AddRestriction(FlightRestriction restriction)
        {
            restrictions.Add(restriction);
        }
        //returns all restrictions
        public static List<FlightRestriction> GetRestrictions()
        {
            return restrictions;
        }
        //returns the restrictions for a country
        public static List<FlightRestriction> GetRestrictions(Country country)
        {
            return restrictions.FindAll(r => r.Country2 == country || r.Country1 == country);
        }
        //returns if there is flight restrictions between two countries
        public static Boolean HasRestriction(Country country1, Country country2, DateTime date)
        {
            FlightRestriction restriction = GetRestrictions().Find(r=>((r.Country1 == country1 || r.Country2 == country1) && (r.Country1 == country2 || r.Country2 == country2) && (date>r.StartDate && date<r.EndDate)));
        
            return restriction != null;
        }
    }
}
