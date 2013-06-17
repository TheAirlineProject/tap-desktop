using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirlineModel
{
    /*! Fee Type.
     * This class is used for a fee for an airline.
     * The class needs parameters for type, name, defaultvalue, minvalue, maxvalue, percentage
     */
    [ProtoContract]
    public class FeeType
    {
        public enum eFeeType { Fee, Wage, FoodDrinks, Discount }
        [ProtoMember(1)]
        public eFeeType Type { get; set; }
        [ProtoMember(2)]
        private double AMinValue;
        public double MinValue { get { return GeneralHelpers.GetInflationPrice(this.AMinValue); } set { this.AMinValue = value; } }
        [ProtoMember(3)]
        private double AMaxValue;
        public double MaxValue { get { return GeneralHelpers.GetInflationPrice(this.AMaxValue); } set { this.AMaxValue = value; } }
        [ProtoMember(4)]
        public string Name { get; set; }
        [ProtoMember(5)]
        private double ADefaultValue;
        public double DefaultValue { get { return GeneralHelpers.GetInflationPrice(this.ADefaultValue); } set { this.ADefaultValue = value; } }
        [ProtoMember(6)]
        public int Percentage { get; set; }
        [ProtoMember(7)]
        public int FromYear { get; set; }
        public FeeType(eFeeType type, string name, double defaultValue, double minValue, double maxValue, int percentage,int fromYear)
        {
            this.Type = type;
            this.MinValue = minValue;
            this.MaxValue = maxValue;
            this.DefaultValue = defaultValue;
            this.Name = name;
            this.Percentage = percentage;
            this.FromYear = fromYear;
        }
        public FeeType(eFeeType type, string name, double defaultValue, double minValue, double maxValue, int percentage)
            : this(type, name, defaultValue, minValue, maxValue, percentage, 1900)
        {
        }
    }
    public class FeeTypes
    {
        private static Dictionary<string, FeeType> types = new Dictionary<string, FeeType>();
        //adds a type to the list
        public static void AddType(FeeType type)
        {
            types.Add(type.Name, type);
        }
        //clears the list
        public static void Clear()
        {
            types = new Dictionary<string, FeeType>();
        }
        //returns the list of fees of a specific type
        public static List<FeeType> GetTypes(FeeType.eFeeType type)
        {
            return GetTypes().FindAll(delegate(FeeType t) { return t.Type == type; });
        }
        //returns the list of fee types
        public static List<FeeType> GetTypes()
        {
            return types.Values.ToList();
        }
        //returns a fee type
        public static FeeType GetType(string name)
        {
            return types[name];
        }

    }
   
}
