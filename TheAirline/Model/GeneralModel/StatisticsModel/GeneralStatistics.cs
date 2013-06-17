
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TheAirline.Model.AirlinerModel;

namespace TheAirline.Model.GeneralModel.StatisticsModel
{
    //the class for general statistics
    [DataContract]
    [KnownType(typeof(AirlinerStatistics))]
    public class GeneralStatistics
    {
        [DataMember]
        private Dictionary<int, List<StatisticsValue>> StatValues;
        public GeneralStatistics()
        {
            this.StatValues = new Dictionary<int, List<StatisticsValue>>();

        }
        //returns the value for a statistics type for a year
        public double getStatisticsValue(int year, StatisticsType type)
        {
            lock (this.StatValues)
            {
                if (this.StatValues.ContainsKey(year))
                {
                    StatisticsValue value = this.StatValues[year].Find(sv => sv.Stat == type);
                    if (value != null) return value.Value;
                }
                return 0;
            }

        }
        //returns the total valu for a statistics type
        public double getStatisticsValue(StatisticsType type)
        {
            double value = 0;
            foreach (int year in this.StatValues.Keys)
            {
                StatisticsValue statValue = this.StatValues[year].Find(sv => sv.Stat == type);
                if (statValue != null) value += statValue.Value;
            }
            return value;
        }
        //adds the value for a statistics type for a year
        public void addStatisticsValue(int year, StatisticsType type, double value)
        {
            lock (this.StatValues)
            {
                if (!this.StatValues.ContainsKey(year))
                    this.StatValues.Add(year, new List<StatisticsValue>());
                StatisticsValue statValue = this.StatValues[year].Find(sv => sv.Stat == type);
                if (statValue != null)
                    statValue.Value += value;
                else
                    this.StatValues[year].Add(new StatisticsValue(type, value));
            }
        }
        //sets the value for a statistics type for a year
        public void setStatisticsValue(int year, StatisticsType type, double value)
        {
            lock (this.StatValues)
            {
                if (!this.StatValues.ContainsKey(year))
                    this.StatValues.Add(year, new List<StatisticsValue>());
                StatisticsValue statValue = this.StatValues[year].Find(sv => sv.Stat == type);
                if (statValue != null)
                    statValue.Value = value;
                else
                    this.StatValues[year].Add(new StatisticsValue(type, value));
            }

        }
        //returns all years with statistics
        public List<int> getYears()
        {
            List<int> years = new List<int>();;
            if (this.StatValues != null)
            {
                lock (this.StatValues)
                {
                   years = new List<int>(this.StatValues.Keys);
                }
            }

            return years;
            
        }


    }
}
