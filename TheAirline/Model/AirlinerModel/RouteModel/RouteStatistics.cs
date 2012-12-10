using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.GeneralModel.StatisticsModel;

namespace TheAirline.Model.AirlinerModel.RouteModel
{

    /*! Route statistics.
   * This is used for statistics for a route.
   * The class needs no parameters
   */
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
        //returns the total value of a statistics type
        public int getTotalValue(StatisticsType type)
        {
            int value = 0;

            foreach (RouteAirlinerClass aClass in this.Stats.Keys)
            {
                if (this.Stats[aClass].ContainsKey(type))
                    value += this.Stats[aClass][type];
            }


            return value;
        }
    }
}
