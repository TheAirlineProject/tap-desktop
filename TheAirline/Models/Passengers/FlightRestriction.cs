using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TheAirline.Infrastructure;
using TheAirline.Models.Airlines;
using TheAirline.Models.General.Countries;

namespace TheAirline.Models.Passengers
{
    /*
     * The class for flight restrictions with no flights between two countries or unions
     */

    [Serializable]
    public class FlightRestriction : BaseModel
    {
        #region Constructors and Destructors

        public FlightRestriction(RestrictionType type, DateTime startDate, DateTime endDate, BaseUnit from, BaseUnit to)
        {
            Type = type;
            StartDate = startDate;
            EndDate = endDate;
            From = from;
            To = to;
        }

        private FlightRestriction(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Enums

        public enum RestrictionType
        {
            Flights,

            Airlines
        }

        #endregion

        #region Public Properties

        [Versioning("enddate")]
        public DateTime EndDate { get; set; }

        [Versioning("from")]
        public BaseUnit From { get; set; }

        [Versioning("startdate")]
        public DateTime StartDate { get; set; }

        [Versioning("to")]
        public BaseUnit To { get; set; }

        [Versioning("type")]
        public RestrictionType Type { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion
    }

    //the list of flight restrictions
    public class FlightRestrictions
    {
        #region Static Fields

        private static readonly List<FlightRestriction> Restrictions = new List<FlightRestriction>();

        #endregion

        #region Public Methods and Operators

        public static void AddRestriction(FlightRestriction restriction)
        {
            Restrictions.Add(restriction);
        }

        //returns all restrictions

        //celars the list
        public static void Clear()
        {
            Restrictions.Clear();
        }

        public static List<FlightRestriction> GetRestrictions()
        {
            return Restrictions;
        }

        //returns if there is flight restrictions from one country to another
        public static bool HasRestriction(
            Country from,
            Country to,
            DateTime date,
            FlightRestriction.RestrictionType type)
        {
            FlightRestriction restriction =
                GetRestrictions()
                    .Find(
                        r =>
                        (r.From == from || (r.From is Union && ((Union) r.From).IsMember(from, date)))
                        && (r.To == to || (r.To is Union && ((Union) r.To).IsMember(to, date)))
                        && (date >= r.StartDate && date <= r.EndDate) && r.Type == type);

            //FlightRestriction res = GetRestrictions().Find(r => r.From == from && r.To == to && r.Type == type);

            return restriction != null;
        }

        //returns if there is flight restrictions between two countries
        public static bool HasRestriction(Country country1, Country country2, DateTime date)
        {
            return HasRestriction(country1, country2, date, FlightRestriction.RestrictionType.Flights)
                   || HasRestriction(country2, country1, date, FlightRestriction.RestrictionType.Flights);
        }

        //returns if there is flight restrictions for airlines to one of the destinations
        public static bool HasRestriction(Airline airline, Country dest1, Country dest2, DateTime date)
        {
            return HasRestriction(airline.Profile.Country, dest2, date, FlightRestriction.RestrictionType.Airlines)
                   || HasRestriction(airline.Profile.Country, dest1, date, FlightRestriction.RestrictionType.Airlines);
        }

        #endregion

        //adds a flight restriction to the list of restrictions
    }
}