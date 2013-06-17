
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel.StatisticsModel
{
    //the class for a type of statistics
    [Serializable]
    public class StatisticsType
    {
        
        public string Name { get; set; }
        
        public string Shortname { get; set; }
        public StatisticsType(string name, string shortname)
        {
            this.Name = name;
            this.Shortname = shortname;
        }
    }
    //the collection of statistics types
    public class StatisticsTypes
    {
        private static Dictionary<string, StatisticsType> types = new Dictionary<string, StatisticsType>();
        //clears the list
        public static void Clear()
        {
            types = new Dictionary<string, StatisticsType>();
        }
        //adds a type to the collection
        public static void AddStatisticsType(StatisticsType type)
        {
            types.Add(type.Shortname, type);
        }
        //return a statistics type
        public static StatisticsType GetStatisticsType(string name)
        {
            return types[name];
        }
        //returns the list of types
        public static List<StatisticsType> GetStatisticsTypes()
        {
            return types.Values.ToList();
        }
    }
}
