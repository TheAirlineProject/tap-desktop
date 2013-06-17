using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel.CountryModel;


namespace TheAirline.Model.PassengerModel
{
    /*
     * The class for flight restrictions with no flights between two countries or unions
     */
    [Serializable]
    public class FlightRestriction
    {
        public enum RestrictionType { Flights, Airlines }
        
        public RestrictionType Type { get; set; }
        
        public DateTime StartDate { get; set; }
        
        public DateTime EndDate { get; set; }
        
        public BaseUnit From { get; set; }
        
        public BaseUnit To { get; set; }
        public FlightRestriction(RestrictionType type, DateTime startDate, DateTime endDate, BaseUnit from, BaseUnit to)
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
        //celars the list
        public static void Clear()
        {
            restrictions.Clear();
        }
        //returns if there is flight restrictions from one country to another
        public static Boolean HasRestriction(Country from, Country to, DateTime date, FlightRestriction.RestrictionType type)
        {
        
            FlightRestriction restriction = GetRestrictions().Find(r=>(r.From == from || (r.From is Union && ((Union)r.From).isMember(from,date))) && (r.To == to || (r.To is Union && ((Union)r.To).isMember(to,date))) && (date>=r.StartDate && date<=r.EndDate) && r.Type == type);

            //FlightRestriction res = GetRestrictions().Find(r => r.From == from && r.To == to && r.Type == type);

            return restriction != null;
        }
        //returns if there is flight restrictions between two countries
        public static Boolean HasRestriction(Country country1, Country country2, DateTime date)
        {
            return HasRestriction(country1, country2, date, FlightRestriction.RestrictionType.Flights) || HasRestriction(country2, country1, date, FlightRestriction.RestrictionType.Flights);
        }
        //returns if there is flight restrictions for airlines to one of the destinations
        public static Boolean HasRestriction(Airline airline, Country dest1, Country dest2, DateTime date)
        {
            return HasRestriction(airline.Profile.Country, dest2, date, FlightRestriction.RestrictionType.Airlines) || HasRestriction(airline.Profile.Country,dest1,date,FlightRestriction.RestrictionType.Airlines);
        }
    }
}
