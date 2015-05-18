using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using TheAirline.Infrastructure;
using TheAirline.Models.Airliners;
using TheAirline.Models.General.Statistics;

namespace TheAirline.Models.Routes
{
    /*! Route statistics.
   * This is used for statistics for a route.
   * The class needs no parameters
   */

    [Serializable]
    public class RouteStatistics : BaseModel
    {
        #region Constructors and Destructors

        public RouteStatistics()
        {
            Stats = new List<RouteStatisticsItem>();
        }

        private RouteStatistics(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("stats")]
        public List<RouteStatisticsItem> Stats { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        public void AddStatisticsValue(RouteAirlinerClass aClass, StatisticsType type, int value)
        {
            RouteStatisticsItem item = Stats.Find(
                i => i.Type.Shortname == type.Shortname && i.RouteClass == aClass);

            if (item == null)
            {
                Stats.Add(new RouteStatisticsItem(aClass, type, value));
            }
            else
            {
                item.Value += value;
            }
        }

        public void AddStatisticsValue(StatisticsType type, int value)
        {
            var aClass = new RouteAirlinerClass(
                AirlinerClass.ClassType.EconomyClass,
                RouteAirlinerClass.SeatingType.FreeSeating,
                0);

            AddStatisticsValue(aClass, type, value);
        }

        //clears the list
        public void Clear()
        {
            Stats.Clear();
        }

        //returns the value for a statistics type for a route class
        public long GetStatisticsValue(RouteAirlinerClass aClass, StatisticsType type)
        {
            RouteStatisticsItem item =
                Stats.Find(i => i.Type.Shortname == type.Shortname && i.RouteClass.Type == aClass.Type);

            if (item == null)
            {
                return 0;
            }
            return item.Value;
        }

        public long GetStatisticsValue(StatisticsType type)
        {
            var aClass = new RouteAirlinerClass(
                AirlinerClass.ClassType.EconomyClass,
                RouteAirlinerClass.SeatingType.FreeSeating,
                0);

            return GetStatisticsValue(aClass, type);
        }

        public long GetTotalValue(StatisticsType type)
        {
            long value;

            lock (Stats)
            {
                value = Stats.Where(s => s.Type.Shortname == type.Shortname).Sum(s => s.Value);
            }

            return value;
        }

        //adds the value for a statistics type to a route class

        //sets the value for a statistics type to a route class
        public void SetStatisticsValue(RouteAirlinerClass aClass, StatisticsType type, int value)
        {
            RouteStatisticsItem item = Stats.Find(
                i => i.Type.Shortname == type.Shortname && i.RouteClass == aClass);

            if (item == null)
            {
                Stats.Add(new RouteStatisticsItem(aClass, type, value));
            }
            else
            {
                item.Value = value;
            }
        }

        public void SetStatisticsValue(StatisticsType type, int value)
        {
            var aClass = new RouteAirlinerClass(
                AirlinerClass.ClassType.EconomyClass,
                RouteAirlinerClass.SeatingType.FreeSeating,
                0);

            SetStatisticsValue(aClass, type, value);
        }

        #endregion

        //returns the total value of a statistics type
    }

    /*
    public class RouteStatistics
    {
        
        private Dictionary<RouteAirlinerClass, Dictionary<StatisticsType, int>> Stats;

        public RouteStatistics()
        {
            this.Stats = new Dictionary<RouteAirlinerClass, Dictionary<StatisticsType, int>>();

        }
        //clears the statistics
        public void clear()
        {
            this.Stats = new Dictionary<RouteAirlinerClass, Dictionary<StatisticsType, int>>();
        }
        //returns the value for a statistics type for a route class
        public int getStatisticsValue(RouteAirlinerClass aClass, StatisticsType type)
        {
            if (this.Stats.ContainsKey(aClass) && this.Stats[aClass].ContainsKey(type))
                return this.Stats[aClass][type];
            else
                return 0;
        }
        public int getStatisticsValue(StatisticsType type)
        {
            RouteAirlinerClass aClass = new RouteAirlinerClass(AirlinerClass.ClassType.Economy_Class, RouteAirlinerClass.SeatingType.Free_Seating, 0);

            return getStatisticsValue(aClass, type);
        }
        //adds the value for a statistics type to a route class
        public void addStatisticsValue(RouteAirlinerClass aClass, StatisticsType type, int value)
        {
            lock (this.Stats)
            {
                if (!this.Stats.ContainsKey(aClass))
                    this.Stats.Add(aClass, new Dictionary<StatisticsType, int>());
                if (!this.Stats[aClass].ContainsKey(type))
                    this.Stats[aClass].Add(type, 0);
                this.Stats[aClass][type] += value;
            }
        }
        public void addStatisticsValue(StatisticsType type, int value)
        {
            RouteAirlinerClass aClass = new RouteAirlinerClass(AirlinerClass.ClassType.Economy_Class, RouteAirlinerClass.SeatingType.Free_Seating, 0);

            addStatisticsValue(aClass,type,value);
        }
        //sets the value for a statistics type to a route class
        public void setStatisticsValue(RouteAirlinerClass aClass, StatisticsType type, int value)
        {
            if (!this.Stats.ContainsKey(aClass))
                this.Stats.Add(aClass, new Dictionary<StatisticsType, int>());
            if (!this.Stats[aClass].ContainsKey(type))
                this.Stats[aClass].Add(type, value);
            else
                this.Stats[aClass][type] = value;
        }
        public void setStatisticsValue(StatisticsType type, int value)
        {
            RouteAirlinerClass aClass = new RouteAirlinerClass(AirlinerClass.ClassType.Economy_Class,RouteAirlinerClass.SeatingType.Free_Seating, 0);

            setStatisticsValue(aClass, type, value);
        }
        //returns the total value of a statistics type
        public int getTotalValue(StatisticsType type)
        {
            int value = 0;

            lock (this.Stats)
            {
                foreach (RouteAirlinerClass aClass in this.Stats.Keys)
                {
                    if (this.Stats[aClass].ContainsKey(type))
                        value += this.Stats[aClass][type];
                }
            }

            return value;
        }
    }
     * */
    //the statistics item for route
    [Serializable]
    public class RouteStatisticsItem
    {
        #region Constructors and Destructors

        public RouteStatisticsItem(RouteAirlinerClass routeClass, StatisticsType type, int value)
        {
            RouteClass = routeClass;
            Type = type;
            Value = value;
        }

        #endregion

        #region Public Properties

        public RouteAirlinerClass RouteClass { get; set; }

        public StatisticsType Type { get; set; }

        public long Value { get; set; }

        #endregion
    }
}