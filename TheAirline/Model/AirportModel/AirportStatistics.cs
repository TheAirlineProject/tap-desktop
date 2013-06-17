
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel.StatisticsModel;

namespace TheAirline.Model.AirportModel
{
    /*! Airport statistics.
 * This is used for statistics for an airport.
 * The class needs no parameters
 */
    [Serializable]
    public class AirportStatistics
    {
        
        private Dictionary<int, List<AirportStatisticsValue>> Stats;
        public AirportStatistics()
        {
            this.Stats = new Dictionary<int, List<AirportStatisticsValue>>();
        }
        //returns the value for a statistics type for an airline for a year
        public double getStatisticsValue(int year, Airline airline, StatisticsType type)
        {
            if (this.Stats.ContainsKey(year))
            {
                AirportStatisticsValue value = this.Stats[year].Find(asv => asv.Airline == airline && asv.Stat == type);
                if (value != null) return value.Value;
            }
            return 0;
        }
        //returns every year with statistics
        public List<int> getYears()
        {
            return this.Stats.Keys.ToList();
        }
       
        //adds the value for a statistics type to an airline for a year
            public void addStatisticsValue(int year, Airline airline, StatisticsType type, int value)
        {
            lock (this.Stats)
            {
                if (!(this.Stats.ContainsKey(year)))
                    this.Stats.Add(year, new List<AirportStatisticsValue>());
                AirportStatisticsValue statValue = this.Stats[year].Find(asv => asv.Airline == airline && asv.Stat == type);
                if (statValue != null)
                    statValue.Value += value;
                else
                    this.Stats[year].Add(new AirportStatisticsValue(airline, type, value));
            }
                    
         
        }
        //sets the value for a statistics type for an airline for a year
        public void setStatisticsValue(int year, Airline airline, StatisticsType type, int value)
        {
            if (!(this.Stats.ContainsKey(year)))
                this.Stats.Add(year, new List<AirportStatisticsValue>());
            AirportStatisticsValue statValue = this.Stats[year].Find(asv => asv.Airline == airline && asv.Stat == type);
            if (statValue != null)
                statValue.Value = value;
            else
                this.Stats[year].Add(new AirportStatisticsValue(airline, type, value));
           
        }
     
        //returns the total value for a statistics type for a year
        public double getTotalValue(int year, StatisticsType type)
        {
            if (!this.Stats.ContainsKey(year))
                return 0;

            return (from s in this.Stats[year]
                    where s.Stat == type
                    select s.Value).Sum();

         
            
        
        }

       
    }
}
