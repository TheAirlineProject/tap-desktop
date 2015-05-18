using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TheAirline.Helpers;
using TheAirline.Infrastructure;
using TheAirline.Model.GeneralModel;
using TheAirline.Models.General;
using TheAirline.Models.Pilots;

namespace TheAirline.Models.Airlines
{
    //the class for an airlines facilities
    [Serializable]
    public class AirlineFacility : BaseModel
    {
        #region Constructors and Destructors

        public AirlineFacility(
            string section,
            string uid,
            double price,
            double monthlyCost,
            int fromYear,
            int serviceLevel,
            int luxuryLevel)
        {
            Section = section;
            Uid = uid;
            FromYear = fromYear;
            MonthlyCost = monthlyCost;
            Price = price;
            LuxuryLevel = luxuryLevel;
            ServiceLevel = serviceLevel;
        }

        public AirlineFacility(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        public static string Section { get; set; }

        [Versioning("monthlycost")]
        public double AMonthlyCost { get; set; }

        [Versioning("price")]
        public double APrice { get; set; }

        //for repairing airliners 

        [Versioning("fromyear")]
        public int FromYear { get; set; }

        [Versioning("luxury")]
        public int LuxuryLevel { get; set; }

        public double MonthlyCost
        {
            get { return GeneralHelpers.GetInflationPrice(AMonthlyCost); }
            set { AMonthlyCost = value; }
        }

        public virtual string Name => Translator.GetInstance().GetString(Section, Uid);

        public double Price
        {
            get { return GeneralHelpers.GetInflationPrice(APrice); }
            set { APrice = value; }
        }

        [Versioning("service")]
        public int ServiceLevel { get; set; }

        public virtual string Shortname => Translator.GetInstance().GetString(Section, Uid, "shortname");

        [Versioning("uid")]
        public string Uid { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (!(this is PilotTrainingFacility))
            {
                info.AddValue("version", 1);
            }

            base.GetObjectData(info, context);
        }

        #endregion
    }

    //the collection of facilities
    public class AirlineFacilities
    {
        #region Static Fields

        private static readonly List<AirlineFacility> Facilities = new List<AirlineFacility>();

        #endregion

        #region Public Methods and Operators

        public static void AddFacility(AirlineFacility facility)
        {
            Facilities.Add(facility);
        }

        public static void Clear()
        {
            Facilities.Clear();
        }

        //returns a facility

        //returns the list of facilities
        public static List<AirlineFacility> GetFacilities()
        {
            return Facilities;
        }

        public static List<AirlineFacility> GetFacilities(Predicate<AirlineFacility> match)
        {
            return Facilities.FindAll(match);
        }

        public static AirlineFacility GetFacility(string uid)
        {
            return Facilities.Find(f => f.Uid == uid);
        }

        #endregion

        //clears the list

        //adds a new facility to the collection
    }
}