using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.AirlineModel;

namespace TheAirline.Model.PassengerModel
{
    /*
     * The class for flight restrictions with no flights between two countries
     */
    public class FlightRestriction
    {
        public enum RestrictionType { Flights, Airlines }
        public RestrictionType Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Country From { get; set; }
        public Country To { get; set; }
        public FlightRestriction(RestrictionType type, DateTime startDate, DateTime endDate, Country from, Country to)
        {
            this.Type = type;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.From = from;
            this.To = to;
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
            return restrictions.FindAll(r => r.To == country || r.From == country);
        }
        //returns if there is flight restrictions from one country to another
        public static Boolean HasRestriction(Country from, Country to, DateTime date, FlightRestriction.RestrictionType type)
        {
            FlightRestriction restriction = GetRestrictions().Find(r=>((r.From == from) && (r.To == to) && (date>r.StartDate && date<r.EndDate) && r.Type == type));
        
            return restriction != null;
        }
        //returns if there is flight restrictions for airlines to one of the destinations
        public static Boolean HasRestriction(Airline airline, Country dest1, Country dest2, DateTime date)
        {
            return HasRestriction(airline.Profile.Country, dest2, date, FlightRestriction.RestrictionType.Airlines) || HasRestriction(airline.Profile.Country,dest1,date,FlightRestriction.RestrictionType.Airlines);
        }
    }
}
