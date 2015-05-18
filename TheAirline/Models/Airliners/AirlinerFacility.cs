using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using TheAirline.Helpers;
using TheAirline.Infrastructure;
using TheAirline.Model.GeneralModel;
using TheAirline.Models.General;

namespace TheAirline.Models.Airliners
{
    //the class for a facility in an airliner
    [Serializable]
    public class AirlinerFacility : BaseModel
    {
        #region Fields

        [Versioning("price")] private double _aPricePerSeat;

        #endregion

        #region Constructors and Destructors

        public AirlinerFacility(
            string section,
            string uid,
            FacilityType type,
            int fromYear,
            int serviceLevel,
            double percentOfSeats,
            double pricePerSeat,
            double seatUses)
        {
            Section = section;
            Uid = uid;
            FromYear = fromYear;
            PricePerSeat = pricePerSeat;
            PercentOfSeats = percentOfSeats;
            Type = type;
            ServiceLevel = serviceLevel;
            SeatUses = seatUses;
        }

        private AirlinerFacility(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Enums

        public enum FacilityType
        {
            Audio,

            Video,

            Seat
        }

        #endregion

        #region Public Properties

        public static string Section { get; set; }

        [Versioning("fromyear")]
        public int FromYear { get; set; }

        public string Name => Translator.GetInstance().GetString(Section, Uid);

        [Versioning("percent")]
        public double PercentOfSeats { get; set; }

        public double PricePerSeat
        {
            get { return GeneralHelpers.GetInflationPrice(_aPricePerSeat); }
            set { _aPricePerSeat = value; }
        }

        [Versioning("seatuses")]
        public double SeatUses { get; set; }

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

    //lists of airliner facilities
    public class AirlinerFacilities
    {
        #region Static Fields

        private static readonly Dictionary<AirlinerFacility.FacilityType, List<AirlinerFacility>> Facilities =
            new Dictionary<AirlinerFacility.FacilityType, List<AirlinerFacility>>();

        #endregion

        #region Public Methods and Operators

        public static void AddFacility(AirlinerFacility facility)
        {
            if (!Facilities.ContainsKey(facility.Type))
            {
                Facilities.Add(facility.Type, new List<AirlinerFacility>());
            }

            Facilities[facility.Type].Add(facility);
        }

        public static void Clear()
        {
            Facilities.Clear();
        }

        //returns the list of all facilities
        public static List<AirlinerFacility> GetAllFacilities()
        {
            return Facilities.Values.SelectMany(v => v).ToList();
        }

        public static AirlinerFacility GetBasicFacility(AirlinerFacility.FacilityType type)
        {
            return Facilities[type][0];
        }

        //returns the list of facilities for a specific type after a specific year
        public static List<AirlinerFacility> GetFacilities(AirlinerFacility.FacilityType type, int year)
        {
            return Facilities.ContainsKey(type) ? Facilities[type].FindAll((f => f.FromYear <= year)) : new List<AirlinerFacility>();
        }

        //returns the list of facilities for a specific type
        public static List<AirlinerFacility> GetFacilities(AirlinerFacility.FacilityType type)
        {
            if (Facilities.ContainsKey(type))
            {
                return Facilities[type];
            }
            return new List<AirlinerFacility>();
        }

        // chs, 2011-13-10 added function to return a specific airliner facility
        //returns a facility based on name and type
        public static AirlinerFacility GetFacility(AirlinerFacility.FacilityType type, string uid)
        {
            return GetFacilities(type).Count > 0 ? GetFacilities(type).Find((f => f.Uid == uid)) : null;
        }

        #endregion

        //clears the list

        //adds a facility to the list

        //returns the basic facility for a specific type
    }
}