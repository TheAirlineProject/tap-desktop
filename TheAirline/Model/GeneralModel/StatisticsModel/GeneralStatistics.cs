using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel.StatisticsModel
{
    //the class for general statistics
    public class GeneralStatistics
    {
        private Dictionary<StatisticsType, int> Stats;
          public GeneralStatistics()
        {
            this.Stats = new Dictionary<StatisticsType, int>();
        
          }
        //returns the value for a statistics type
        public int getStatisticsValue( StatisticsType type)
        {
            if (this.Stats.ContainsKey(type))
                return this.Stats[type];
            else
                return 0;
        }
        //adds the value for a statistics type
        public void addStatisticsValue(StatisticsType type,int value)
        {
            if (!this.Stats.ContainsKey(type))
                this.Stats.Add(type, 0);
            this.Stats[type] += value;
        }
        //sets the value for a statistics type
        public void setStatisticsValue(StatisticsType type, int value)
        {
            if (!this.Stats.ContainsKey(type))
                this.Stats.Add(type, value);
            else
                this.Stats[type] = value;
        }
       

    }
}
