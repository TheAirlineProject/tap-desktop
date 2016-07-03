using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using TheAirline.Infrastructure;
using TheAirline.Models.Airlines;

namespace TheAirline.Models.Routes
{
    //the class for the in flight facilities on a route
    [Serializable]
    public class RouteFacility : BaseModel
    {
        #region Constructors and Destructors

        public RouteFacility(
            string uid,
            FacilityType type,
            string name,
            int serviceLevel,
            ExpenseType eType,
            double expense,
            FeeType feeType)
            : this(uid, type, name, serviceLevel, eType, expense, feeType, null)
        {
        }

        public RouteFacility(
            string uid,
            FacilityType type,
            string name,
            int serviceLevel,
            ExpenseType eType,
            double expense,
            FeeType feeType,
            AirlineFacility requires)
        {
            Type = type;
            Name = name;
            Uid = uid;
            ServiceLevel = serviceLevel;
            EType = eType;
            ExpensePerPassenger = expense;
            FeeType = feeType;
            Requires = requires;
        }

        private RouteFacility(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Enums

        public enum ExpenseType
        {
            Random,

            Fixed
        }

        public enum FacilityType
        {
            Food,

            Drinks,

            AlcoholicDrinks,

            Newspapers = 1950,

            Magazines = 1955,

            WiFi = 2007
        }

        #endregion

        #region Public Properties

        [Versioning("etype")]
        public ExpenseType EType { get; set; }

        [Versioning("expensesperpassenger")]
        public double ExpensePerPassenger { get; set; }

        [Versioning("feetype")]
        public FeeType FeeType { get; set; }

        [Versioning("name")]
        public string Name { get; set; }

        [Versioning("requires")]
        public AirlineFacility Requires { get; set; }

        [Versioning("servicelevel")]
        public int ServiceLevel { get; set; }

        [Versioning("type")]
        public FacilityType Type { get; set; }

        [Versioning("uid")]
        public string Uid { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion
    }

    //the list of facilities for each type
    public class RouteFacilities
    {
        #region Static Fields

        private static List<RouteFacility> _facilities = new List<RouteFacility>();

        #endregion

        #region Public Methods and Operators

        public static void AddFacility(RouteFacility facility)
        {
            _facilities.Add(facility);
        }

        public static void Clear()
        {
            _facilities = new List<RouteFacility>();
        }

        //returns all facilities

        //Return all facilities
        public static List<RouteFacility> GetAllFacilities()
        {
            return _facilities;
        }

        //returns the basic facility for a specific type
        public static RouteFacility GetBasicFacility(RouteFacility.FacilityType type)
        {
            return _facilities.FindAll(f => f.Type == type).OrderBy(f => f.ServiceLevel).First();
        }

        public static List<RouteFacility> GetFacilities()
        {
            return _facilities;
        }

        //returns all facilities for a specific type
        public static List<RouteFacility> GetFacilities(RouteFacility.FacilityType type)
        {
            return _facilities.FindAll(f => f.Type == type);
        }

        //returns the facility with an uid
        public static RouteFacility GetFacility(string uid)
        {
            return _facilities.Find(f => f.Uid == uid);
        }

        #endregion

        //private static Dictionary<RouteFacility.FacilityType, List<RouteFacility>> facilities = new Dictionary<RouteFacility.FacilityType, List<RouteFacility>>();
        //clears the list

        //adds a facility to the list
    }
}