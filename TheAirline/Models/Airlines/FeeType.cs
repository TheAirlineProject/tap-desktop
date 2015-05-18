using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using TheAirline.Helpers;
using TheAirline.Infrastructure;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Models.Airlines
{
    /*! Fee Type.
     * This class is used for a fee for an airline.
     * The class needs parameters for type, name, defaultvalue, minvalue, maxvalue, percentage
     */

    [Serializable]
    public class FeeType : BaseModel
    {
        #region Fields

        [Versioning("default")] private double _aDefaultValue;

        [Versioning("maxvalue")] private double _aMaxValue;

        [Versioning("minvalue")] private double _aMinValue;

        #endregion

        #region Constructors and Destructors

        public FeeType(
            EFeeType type,
            string name,
            double defaultValue,
            double minValue,
            double maxValue,
            int percentage,
            int fromYear)
        {
            Type = type;
            MinValue = minValue;
            MaxValue = maxValue;
            DefaultValue = defaultValue;
            Name = name;
            Percentage = percentage;
            FromYear = fromYear;
        }

        public FeeType(
            EFeeType type,
            string name,
            double defaultValue,
            double minValue,
            double maxValue,
            int percentage)
            : this(type, name, defaultValue, minValue, maxValue, percentage, 1900)
        {
        }

        private FeeType(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Enums

        public enum EFeeType
        {
            Fee,

            Wage,

            FoodDrinks,

            Discount
        }

        #endregion

        #region Public Properties

        public double DefaultValue
        {
            get { return GeneralHelpers.GetInflationPrice(_aDefaultValue); }
            set { _aDefaultValue = value; }
        }

        [Versioning("fromyear")]
        public int FromYear { get; set; }

        public double MaxValue
        {
            get { return GeneralHelpers.GetInflationPrice(_aMaxValue); }
            set { _aMaxValue = value; }
        }

        public double MinValue
        {
            get { return GeneralHelpers.GetInflationPrice(_aMinValue); }
            set { _aMinValue = value; }
        }

        [Versioning("name")]
        public string Name { get; set; }

        [Versioning("percentage")]
        public int Percentage { get; set; }

        [Versioning("type")]
        public EFeeType Type { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion
    }

    public class FeeTypes
    {
        #region Static Fields

        private static readonly Dictionary<string, FeeType> Types = new Dictionary<string, FeeType>();

        #endregion

        #region Public Methods and Operators

        public static void AddType(FeeType type)
        {
            Types.Add(type.Name, type);
        }

        //clears the list
        public static void Clear()
        {
            Types.Clear();
        }

        public static FeeType GetType(string name)
        {
            return Types[name];
        }

        //returns the list of fees of a specific type
        public static List<FeeType> GetTypes(FeeType.EFeeType type)
        {
            return GetTypes().FindAll(t => t.Type == type);
        }

        //returns the list of fee types
        public static List<FeeType> GetTypes()
        {
            return Types.Values.ToList();
        }

        #endregion

        //adds a type to the list

        //returns a fee type
    }
}