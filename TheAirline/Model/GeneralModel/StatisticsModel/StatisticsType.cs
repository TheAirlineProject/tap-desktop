using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace TheAirline.Model.GeneralModel.StatisticsModel
{
    //the class for a type of statistics
    [Serializable]
    public class StatisticsType : BaseModel
    {
        #region Constructors and Destructors

        public StatisticsType(string name, string shortname)
        {
            Name = name;
            Shortname = shortname;
        }

        private StatisticsType(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("name")]
        public string Name { get; set; }

        [Versioning("shortname")]
        public string Shortname { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion
    }

    //the collection of statistics types
    public class StatisticsTypes
    {
        #region Static Fields

        private static Dictionary<string, StatisticsType> _types = new Dictionary<string, StatisticsType>();

        #endregion

        #region Public Methods and Operators

        public static void AddStatisticsType(StatisticsType type)
        {
            _types.Add(type.Shortname, type);
        }

        public static void Clear()
        {
            _types = new Dictionary<string, StatisticsType>();
        }

        //return a statistics type
        public static StatisticsType GetStatisticsType(string name)
        {
            return _types[name];
        }

        //returns the list of types
        public static List<StatisticsType> GetStatisticsTypes()
        {
            return _types.Values.ToList();
        }

        #endregion

        //clears the list

        //adds a type to the collection
    }
}