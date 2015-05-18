using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using TheAirline.Helpers;
using TheAirline.Infrastructure;
using TheAirline.Model.GeneralModel;
using TheAirline.Models.General;

namespace TheAirline.Models.Airports
{
    [Serializable]
    //the class for a facility at an airport
    public class AirportFacility : BaseModel
    {
        #region Fields

        [Versioning("price")] private double _aPrice;

        #endregion

        #region Constructors and Destructors

        public AirportFacility(
            string section,
            string uid,
            string shortname,
            FacilityType type,
            int buildingDays,
            int typeLevel,
            double price,
            int serviceLevel,
            int luxuryLevel)
        {
            Section = section;
            Uid = uid;
            Shortname = shortname;
            Price = price;
            LuxuryLevel = luxuryLevel;
            ServiceLevel = serviceLevel;
            BuildingDays = buildingDays;
            TypeLevel = typeLevel;
            Type = type;
        }

        private AirportFacility(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Enums

        public enum EmployeeTypes
        {
            Maintenance,

            Support
        }

        public enum FacilityType
        {
            Lounge,

            Service,

            CheckIn,

            SelfCheck,

            TicketOffice,

            Cargo
        }

        #endregion

        #region Public Properties

        public static string Section { get; set; }

        [Versioning("buildingdays")]
        public int BuildingDays { get; set; }

        [Versioning("employeetype")]
        public EmployeeTypes EmployeeType { get; set; }

        [Versioning("luxurylevel")]
        public int LuxuryLevel { get; set; }

        public virtual string Name => Translator.GetInstance().GetString(Section, Uid);

        [Versioning("employees")]
        public int NumberOfEmployees { get; set; }

        public double Price
        {
            get { return GeneralHelpers.GetInflationPrice(_aPrice); }
            set { _aPrice = value; }
        }

        [Versioning("servicelevel")]
        public int ServiceLevel { get; set; }

        [Versioning("shortname")]
        public string Shortname { get; set; }

        [Versioning("type")]
        public FacilityType Type { get; set; }

        [Versioning("typelevel")]
        public int TypeLevel { get; set; }

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

    //the collection of facilities
    public class AirportFacilities
    {
        #region Static Fields

        private static Dictionary<string, AirportFacility> _facilities = new Dictionary<string, AirportFacility>();

        #endregion

        #region Public Methods and Operators

        public static void AddFacility(AirportFacility facility)
        {
            _facilities.Add(facility.Shortname, facility);
        }

        public static void Clear()
        {
            _facilities = new Dictionary<string, AirportFacility>();
        }

        //returns a facility

        //returns the list of facilities
        public static List<AirportFacility> GetFacilities()
        {
            return _facilities.Values.ToList();
        }

        //returns all facilities of a specific type
        public static List<AirportFacility> GetFacilities(AirportFacility.FacilityType type)
        {
            return GetFacilities().FindAll((facility => facility.Type == type));
        }

        public static AirportFacility GetFacility(string shortname)
        {
            return _facilities[shortname];
        }

        #endregion

        //clears the list

        //adds a new facility to the collection
    }
}