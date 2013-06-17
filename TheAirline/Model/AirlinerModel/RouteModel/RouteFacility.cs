using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlineModel;

namespace TheAirline.Model.AirlinerModel.RouteModel
{
    //the class for the in flight facilities on a route
    [ProtoContract]
    public class RouteFacility
    {
        public enum ExpenseType { Random, Fixed }
        [ProtoMember(1)]
        public ExpenseType EType { get; set; }
        [ProtoMember(2)]
        public double ExpensePerPassenger { get; set; }
        public enum FacilityType { Food, Drinks, Alcoholic_Drinks, Newspapers = 1950, Magazines = 1955, WiFi = 2007 }
        [ProtoMember(3)]
        public FacilityType Type { get; set; }
        [ProtoMember(4)]
        public string Name { get; set; }
        [ProtoMember(5)]
        public int ServiceLevel { get; set; }
        [ProtoMember(6)]
        public FeeType FeeType { get; set; }
        [ProtoMember(7)]
        public string Uid { get; set; }
        [ProtoMember(8)]
        public AirlineFacility Requires { get; set; }
        public RouteFacility(string uid, FacilityType type, string name, int serviceLevel, ExpenseType eType, double expense, FeeType feeType) : this(uid,type,name,serviceLevel,eType,expense,feeType,null)
        {
        }
        public RouteFacility(string uid, FacilityType type, string name, int serviceLevel, ExpenseType eType, double expense, FeeType feeType,AirlineFacility requires)
        {
            this.Type = type;
            this.Name = name;
            this.Uid = uid;
            this.ServiceLevel = serviceLevel;
            this.EType = eType;
            this.ExpensePerPassenger = expense;
            this.FeeType = feeType;
            this.Requires = requires;
        }

    }
    //the list of facilities for each type
    public class RouteFacilities
    {
        private static List<RouteFacility> facilities = new List<RouteFacility>();
        //private static Dictionary<RouteFacility.FacilityType, List<RouteFacility>> facilities = new Dictionary<RouteFacility.FacilityType, List<RouteFacility>>();
        //clears the list
        public static void Clear()
        {
            facilities = new List<RouteFacility>();
        }
        //adds a facility to the list
        public static void AddFacility(RouteFacility facility)
        {
            facilities.Add(facility);
        }
        //returns all facilities for a specific type
        public static List<RouteFacility> GetFacilities(RouteFacility.FacilityType type)
        {
            return facilities.FindAll(f => f.Type == type);
        }
        //returns the basic facility for a specific type
        public static RouteFacility GetBasicFacility(RouteFacility.FacilityType type)
        {
            return facilities.FindAll(f => f.Type == type).OrderBy(f => f.ServiceLevel).First();
        }
        //returns the facility with an uid
        public static RouteFacility GetFacility(string uid)
        {
            return facilities.Find(f => f.Uid == uid);
        }
    }
}
