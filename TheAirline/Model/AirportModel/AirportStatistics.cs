
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel.StatisticsModel;

namespace TheAirline.Model.AirportModel
{
    /*! Airport statistics.
 * This is used for statistics for an airport.
 * The class needs no parameters
 */
    [DataContract]
    public class AirportStatistics
    {
        [DataMember]
        public List<AirportStatisticsValue> Stats { get; set; }
        public AirportStatistics()
        {
            this.Stats = new List<AirportStatisticsValue>();
        }
        //returns the value for a statistics type for an airline for a year
        public double getStatisticsValue(int year, Airline airline, StatisticsType type)
        {
            AirportStatisticsValue item = this.Stats.Find(s => s.Year == year && s.Airline == airline && s.Stat.Shortname == type.Shortname);

            if (item == null)
                return 0;
            else
                return item.Value;
           
        }
        //returns every year with statistics
        public List<int> getYears()
        {
            return this.Stats.Select(s => s.Year).Distinct().ToList();
        }
       
        //adds the value for a statistics type to an airline for a year
            public void addStatisticsValue(int year, Airline airline, StatisticsType type, int value)
        {
            AirportStatisticsValue item = this.Stats.Find(s => s.Year == year && s.Airline == airline && s.Stat.Shortname == type.Shortname);

            if (item == null)
                this.Stats.Add(new AirportStatisticsValue(airline, year, type, value));
            else
                item.Value += value;
        }
        //sets the value for a statistics type for an airline for a year
        public void setStatisticsValue(int year, Airline airline, StatisticsType type, int value)
        {
            lock (this.Stats)
            {
                AirportStatisticsValue item = this.Stats.Find(s => s.Year == year && s.Airline == airline && s.Stat.Shortname == type.Shortname);

                if (item == null)
                    this.Stats.Add(new AirportStatisticsValue(airline, year, type, value));
                else
                    item.Value = value;
            }
           
        }
     
        //returns the total value for a statistics type for a year
        public double getTotalValue(int year, StatisticsType type)
        {
            return this.Stats.Where(s => s.Year == year && s.Stat.Shortname == type.Shortname).Sum(s => s.Value);
           
            
        
        }

       
    }
}
