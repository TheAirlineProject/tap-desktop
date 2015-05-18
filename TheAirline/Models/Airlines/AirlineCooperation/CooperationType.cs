using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TheAirline.Helpers;
using TheAirline.Infrastructure;
using TheAirline.Model.GeneralModel;
using TheAirline.Models.General;

namespace TheAirline.Models.Airlines.AirlineCooperation
{
    //the class for a type of airline cooperation
    [Serializable]
    public class CooperationType : BaseModel
    {
        #region Fields

        [Versioning("incomeperpax")] private double _aIncomePerPax;

        [Versioning("monthlyprice")] private double _aMonthlyPrice;

        [Versioning("price")] private double _aPrice;

        #endregion

        #region Constructors and Destructors

        public CooperationType(
            string section,
            string uid,
            GeneralHelpers.Size airportsizerequired,
            int fromyear,
            double price,
            double monthlyprice,
            int servicelevel,
            double incomeperpax)
        {
            Section = section;
            AirportSizeRequired = airportsizerequired;
            Price = price;
            MonthlyPrice = monthlyprice;
            Uid = uid;
            FromYear = fromyear;
            ServiceLevel = servicelevel;
            IncomePerPax = incomeperpax;
        }

        private CooperationType(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        public static string Section { get; set; }

        [Versioning("airportsize")]
        public GeneralHelpers.Size AirportSizeRequired { get; set; }

        [Versioning("from")]
        public int FromYear { get; set; }

        public double IncomePerPax
        {
            get { return GeneralHelpers.GetInflationPrice(_aIncomePerPax); }
            set { _aIncomePerPax = value; }
        }

        public double MonthlyPrice
        {
            get { return GeneralHelpers.GetInflationPrice(_aMonthlyPrice); }
            set { _aMonthlyPrice = value; }
        }

        public string Name => Translator.GetInstance().GetString(Section, Uid);

        public double Price
        {
            get { return GeneralHelpers.GetInflationPrice(_aPrice); }
            set { _aPrice = value; }
        }

        [Versioning("servicelevel")]
        public int ServiceLevel { get; set; }

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

        //the expected income for a hotel per pax
    }

    //the list of types
    public class CooperationTypes
    {
        #region Static Fields

        private static readonly List<CooperationType> Types = new List<CooperationType>();

        #endregion

        #region Public Methods and Operators

        public static void AddCooperationType(CooperationType type)
        {
            Types.Add(type);
        }

        //returns the cooperation type with a specific uid

        //clears the list of types
        public static void Clear()
        {
            Types.Clear();
        }

        public static CooperationType GetCooperationType(string uid)
        {
            return Types.Find(t => t.Uid == uid);
        }

        //returns all types
        public static List<CooperationType> GetCooperationTypes()
        {
            return Types;
        }

        #endregion

        //adds a type to the list
    }
}