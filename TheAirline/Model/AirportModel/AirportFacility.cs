using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirportModel
{
    //the class for a facility at an airport
    public class AirportFacility
    {
        public static string Section { get; set; }
        public string Uid { get; set; }
        public enum FacilityType { Lounge, Service, CheckIn, SelfCheck, TicketOffice }
        public FacilityType Type { get; set; }
        public string Shortname { get; set; }
        public double Price { get; set; }
        public int TypeLevel { get; set; }
        public int LuxuryLevel { get; set; } //for business customers
        public int ServiceLevel { get; set; } //for repairing airliners 
        public AirportFacility(string section, string uid, string shortname,FacilityType type,int typeLevel, double price, int serviceLevel, int luxuryLevel)
        {
            AirportFacility.Section = section;
            this.Uid = uid;
            this.Shortname = shortname;
            this.Price = price;
            this.LuxuryLevel = luxuryLevel;
            this.ServiceLevel = serviceLevel;
            this.TypeLevel = typeLevel;
            this.Type = type;
        }

        public string Name
        {
            get { return Translator.GetInstance().GetString(AirportFacility.Section, this.Uid); }
        }
    }
    //the collection of facilities
    public class AirportFacilities
    {
        private static Dictionary<string, AirportFacility> facilities = new Dictionary<string, AirportFacility>();
        //clears the list
        public static void Clear()
        {
            facilities = new Dictionary<string, AirportFacility>();
        }
        //adds a new facility to the collection
        public static void AddFacility(AirportFacility facility)
        {
            facilities.Add(facility.Shortname, facility);
        }
        //returns a facility
        public static AirportFacility GetFacility(string shortname)
        {
            return facilities[shortname];
        }
        //returns the list of facilities
        public static List<AirportFacility> GetFacilities()
        {
            return facilities.Values.ToList();
        }
        //returns all facilities of a specific type
        public static List<AirportFacility> GetFacilities(AirportFacility.FacilityType type)
        {
            return GetFacilities().FindAll((delegate(AirportFacility facility) { return facility.Type ==type; }));
        }
    }
   
}
