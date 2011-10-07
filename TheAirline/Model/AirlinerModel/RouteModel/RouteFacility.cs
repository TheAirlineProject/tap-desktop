using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlineModel;

namespace TheAirline.Model.AirlinerModel.RouteModel
{
    //the class for the in flight facilities on a route
    public class RouteFacility
    {
        public enum ExpenseType { Random, Fixed }
        public ExpenseType EType { get; set; }
        public double ExpensePerPassenger { get; set; }
        public enum FacilityType { Food, Drinks }
        public FacilityType Type { get; set; }
        public string Name { get; set; }
        public int MinimumCabinCrew { get; set; }
        public int ServiceLevel { get; set; }
        public FeeType FeeType { get; set; }
        public RouteFacility(FacilityType type, string name, int minimumCabinCrew, int serviceLevel, ExpenseType eType, double expense, FeeType feeType)
        {
            this.Type = type;
            this.Name = name;
            this.MinimumCabinCrew = minimumCabinCrew;
            this.ServiceLevel = serviceLevel;
            this.EType = eType;
            this.ExpensePerPassenger = expense;
            this.FeeType = feeType;
        }

    }
    //the list of facilities for each type
    public class RouteFacilities
    {
        private static Dictionary<RouteFacility.FacilityType, List<RouteFacility>> facilities = new Dictionary<RouteFacility.FacilityType, List<RouteFacility>>();
        //clears the list
        public static void Clear()
        {
            facilities = new Dictionary<RouteFacility.FacilityType, List<RouteFacility>>();
        }
        //adds a facility to the list
        public static void AddFacility(RouteFacility facility)
        {
            if (!facilities.ContainsKey(facility.Type))
                facilities.Add(facility.Type, new List<RouteFacility>());
            facilities[facility.Type].Add(facility);
        }
        //returns all facilities for a specific type
        public static List<RouteFacility> GetFacilities(RouteFacility.FacilityType type)
        {
            return facilities[type];
        }
        //returns the basic facility for a specific type
        public static RouteFacility GetBasicFacility(RouteFacility.FacilityType type)
        {
            return facilities[type][0];
        }
    }
}
