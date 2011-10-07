using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.AirlinerModel
{
    //the class for a facility in an airliner
    public class AirlinerFacility
    {
        public enum FacilityType { Audio, Video, Seat }
        public string Name { get; set; }
        public double PricePerSeat { get; set; }
        public double PercentOfSeats { get; set; }
        public FacilityType Type { get; set; }
        public int ServiceLevel { get; set; }
        public int FromYear { get; set; }
        public double SeatUses { get; set; }
        public AirlinerFacility(FacilityType type, string name,int fromYear, int serviceLevel, double percentOfSeats, double pricePerSeat, double seatUses)
        {
            this.Name = name;
            this.FromYear = fromYear;
            this.PricePerSeat = pricePerSeat;
            this.PercentOfSeats = percentOfSeats;
            this.Type = type;
            this.ServiceLevel = serviceLevel;
            this.SeatUses = seatUses;
        }
    }
    //lists of airliner facilities
    public class AirlinerFacilities
    {
        private static Dictionary<AirlinerFacility.FacilityType, List<AirlinerFacility>> facilities = new Dictionary<AirlinerFacility.FacilityType, List<AirlinerFacility>>();
        //clears the list
        public static void Clear()
        {
            facilities = new Dictionary<AirlinerFacility.FacilityType, List<AirlinerFacility>>();
        }
        //adds a facility to the list
        public static void AddFacility(AirlinerFacility facility)
        {
            if (!facilities.ContainsKey(facility.Type))
                facilities.Add(facility.Type, new List<AirlinerFacility>());

            facilities[facility.Type].Add(facility);
        }
        //returns the list of facilities for a specific type after a specific year
        public static List<AirlinerFacility> GetFacilities(AirlinerFacility.FacilityType type, int year)
        {
            if (facilities.ContainsKey(type))
                return facilities[type].FindAll((delegate(AirlinerFacility f) { return f.FromYear<=year; }));

            else
                return new List<AirlinerFacility>();
             }
        //returns the list of facilities for a specific type
        public static List<AirlinerFacility> GetFacilities(AirlinerFacility.FacilityType type)
        {
            if (facilities.ContainsKey(type))
                return facilities[type];
            else
                return new List<AirlinerFacility>();
        }
        //returns the basic facility for a specific type
        public static AirlinerFacility GetBasicFacility(AirlinerFacility.FacilityType type)
        {
            return facilities[type][0];
        }
    }
}
