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
    public class AirportStatistics
    {
        //private Dictionary<Airline, Dictionary<StatisticsType, int>> Stats;
        private Dictionary<int, List<AirportStatisticsValue>> Stats;
        public AirportStatistics()
        {
            //this.Stats = new Dictionary<Airline, Dictionary<StatisticsType, int>>();
            this.Stats = new Dictionary<int, List<AirportStatisticsValue>>();
        }
        //returns the value for a statistics type for an airline for a year
        public int getStatisticsValue(int year, Airline airline, StatisticsType type)
        {
            if (this.Stats.ContainsKey(year))
            {
                AirportStatisticsValue value = this.Stats[year].Find(asv => asv.Airline == airline && asv.Stat == type);
                if (value != null) return value.Value;
            }
            return 0;
        }
        /**
        //returns the value for a statistics type for an airline
        public int getStatisticsValue(Airline airline, StatisticsType type)
        {
            
            if (this.Stats.ContainsKey(airline) && this.Stats[airline].ContainsKey(type))
                return this.Stats[airline][type];
            else
                return 0;
        }
         */
        //adds the value for a statistics type to an airline for a year
        public void addStatisticsValue(int year, Airline airline, StatisticsType type, int value)
        {
            if (!(this.Stats.ContainsKey(year)))
                this.Stats.Add(year,new List<AirportStatisticsValue>());
            AirportStatisticsValue statValue = this.Stats[year].Find(asv => asv.Airline == airline && asv.Stat == type);
            if (statValue != null)
                statValue.Value += value;
            else
                this.Stats[year].Add(new AirportStatisticsValue(airline, type, value));
                    
         
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
        /**
        //adds the value for a statistics type to an airline
        public void addStatisticsValue(Airline airline, StatisticsType type, int value)
        {
            if (!this.Stats.ContainsKey(airline))
                this.Stats.Add(airline, new Dictionary<StatisticsType, int>());
            if (!this.Stats[airline].ContainsKey(type))
                this.Stats[airline].Add(type, 0);
            this.Stats[airline][type] += value;
        }
        //sets the value for a statistics type to an airline
        public void setStatisticsValue(Airline airline, StatisticsType type, int value)
        {
            if (!this.Stats.ContainsKey(airline))
                this.Stats.Add(airline, new Dictionary<StatisticsType, int>());
            if (!this.Stats[airline].ContainsKey(type))
                this.Stats[airline].Add(type, value);
            else
                this.Stats[airline][type] = value;
        }
         * */
        //returns the total value for a statistics type for a year
        public int getTotalValue(int year, StatisticsType type)
        {
            if (!this.Stats.ContainsKey(year))
                return 0;

            return (from s in this.Stats[year]
                    where s.Stat == type
                    select s.Value).Sum();

         
            
        
        }

        /*
        //returns the total value of a statistics type
        public int getTotalValue(StatisticsType type)
        {
            int value = 0;

            foreach (Airline airline in this.Stats.Keys)
            {
                if (this.Stats[airline].ContainsKey(type))
                    value += this.Stats[airline][type];
            }


            return value;
        }
        **/
    }
}
