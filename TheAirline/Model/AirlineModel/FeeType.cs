using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.AirlineModel
{
    /*! Fee Type.
     * This class is used for a fee for an airline.
     * The class needs parameters for type, name, defaultvalue, minvalue, maxvalue, percentage
     */
    public class FeeType
    {
        public enum eFeeType { Fee, Wage, FoodDrinks }
        public eFeeType Type { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public string Name { get; set; }
        public double DefaultValue { get; set; }
        public int Percentage { get; set; }
        public FeeType(eFeeType type, string name, double defaultValue, double minValue, double maxValue, int percentage)
        {
            this.Type = type;
            this.MinValue = minValue;
            this.MaxValue = maxValue;
            this.DefaultValue = defaultValue;
            this.Name = name;
            this.Percentage = percentage;
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
