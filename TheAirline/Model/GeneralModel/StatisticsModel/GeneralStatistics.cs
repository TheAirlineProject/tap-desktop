
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
        public List<StatisticsValue> StatValues { get; set; }
        public GeneralStatistics()
        {
            this.StatValues = new List<StatisticsValue>();

        }
        //returns the value for a statistics type for a year
        public double getStatisticsValue(int year, StatisticsType type)
        {
            StatisticsValue item = this.StatValues.Find(s => s.Year == year && s.Stat.Shortname == type.Shortname);

            if (item == null)
                return 0;
            else
                return item.Value;

           
        }
        //returns the total value for a statistics type
        public double getStatisticsValue(StatisticsType type)
        {
            return this.StatValues.Where(s => s.Stat.Shortname == type.Shortname).Sum(s => s.Value);
           
        }
        //adds the value for a statistics type for a year
        public void addStatisticsValue(int year, StatisticsType type, double value)
        {
            StatisticsValue item = this.StatValues.Find(s => s.Year == year && s.Stat.Shortname == type.Shortname);

            if (item == null)
                this.StatValues.Add(new StatisticsValue(year, type, value));
            else
                item.Value +=value;
        }
        //sets the value for a statistics type for a year
        public void setStatisticsValue(int year, StatisticsType type, double value)
        {
            StatisticsValue item = this.StatValues.Find(s => s.Year == year && s.Stat.Shortname == type.Shortname);

            if (item == null)
                this.StatValues.Add(new StatisticsValue(year, type, value));
            else
                item.Value = value;

        }
        //returns all years with statistics
        public List<int> getYears()
        {
            return this.StatValues.Select(s => s.Year).Distinct().ToList();
           
            
        }
        //clears the statistics
        public void clear()
        {
            lock (this.StatValues)
            {
                this.StatValues.Clear();
            }
        }

    }
}
