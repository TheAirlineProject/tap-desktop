
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirlinerModel
{
    //the class for a facility in an airliner
    [Serializable]
    public class AirlinerFacility
    {
        public static string Section { get; set; }
        
        public string Uid { get; set; }
        public enum FacilityType { Audio, Video, Seat }
        
        private double APricePerSeat;
        public double PricePerSeat { get { return GeneralHelpers.GetInflationPrice(this.APricePerSeat); } set { this.APricePerSeat = value; } }
        
        public double PercentOfSeats { get; set; }
        
        public FacilityType Type { get; set; }
        
        public int ServiceLevel { get; set; }
        
        public int FromYear { get; set; }
        
        public double SeatUses { get; set; }
        public AirlinerFacility(string section, string uid, FacilityType type, int fromYear, int serviceLevel, double percentOfSeats, double pricePerSeat, double seatUses)
        {
            AirlinerFacility.Section = section;
            this.Uid = uid;
            this.FromYear = fromYear;
            this.PricePerSeat = pricePerSeat;
            this.PercentOfSeats = percentOfSeats;
            this.Type = type;
            this.ServiceLevel = serviceLevel;
            this.SeatUses = seatUses;
        }
        public string Name
        {
            get { return Translator.GetInstance().GetString(AirlinerFacility.Section, this.Uid); }
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
                return facilities[type].FindAll((delegate(AirlinerFacility f) { return f.FromYear <= year; }));

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
        // chs, 2011-13-10 added function to return a specific airliner facility
        //returns a facility based on name and type
        public static AirlinerFacility GetFacility(AirlinerFacility.FacilityType type, string uid)
        {
            if (GetFacilities(type).Count > 0)
                return GetFacilities(type).Find((delegate(AirlinerFacility f) { return f.Uid == uid; }));
            else
                return null;
        }
        //returns the basic facility for a specific type
        public static AirlinerFacility GetBasicFacility(AirlinerFacility.FacilityType type)
        {
            return facilities[type][0];
        }
    }
}
