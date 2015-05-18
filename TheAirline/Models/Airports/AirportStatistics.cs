using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using TheAirline.Infrastructure;
using TheAirline.Model.GeneralModel;
using TheAirline.Models.General.Statistics;

namespace TheAirline.Models.Airports
{
    /*! Airport statistics.
 * This is used for statistics for an airport.
 * The class needs no parameters
 */

    [Serializable]
    public class AirportStatistics : BaseModel
    {
        #region Constructors and Destructors

        public AirportStatistics()
        {
            Stats = new List<AirportStatisticsValue>();
        }

        private AirportStatistics(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("stats")]
        public List<AirportStatisticsValue> Stats { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        public void AddStatisticsValue(int year, Airlines.Airline airline, StatisticsType type, int value)
        {
            AirportStatisticsValue item =
                Stats.Find(s => s.Year == year && s.Airline == airline && s.Stat.Shortname == type.Shortname);

            if (item == null)
            {
                Stats.Add(new AirportStatisticsValue(airline, year, type, value));
            }
            else
            {
                item.Value += value;
            }
        }

        //returns the value for a statistics type for an airline for a year
        public double GetStatisticsValue(int year, Airlines.Airline airline, StatisticsType type)
        {
            AirportStatisticsValue item =
                Stats.Find(s => s.Year == year && s.Airline == airline && s.Stat.Shortname == type.Shortname);

            if (item == null)
            {
                return 0;
            }
            return item.Value;
        }

        public double GetTotalValue(int year, StatisticsType type)
        {
            return Stats.Where(s => s.Year == year && s.Stat.Shortname == type.Shortname).Sum(s => s.Value);
        }

        //returns every year with statistics
        public List<int> GetYears()
        {
            return Stats.Select(s => s.Year).Distinct().ToList();
        }

        //adds the value for a statistics type to an airline for a year

        //sets the value for a statistics type for an airline for a year
        public void SetStatisticsValue(int year, Airlines.Airline airline, StatisticsType type, int value)
        {
            lock (Stats)
            {
                AirportStatisticsValue item =
                    Stats.Find(s => s.Year == year && s.Airline == airline && s.Stat.Shortname == type.Shortname);

                if (item == null)
                {
                    Stats.Add(new AirportStatisticsValue(airline, year, type, value));
                }
                else
                {
                    item.Value = value;
                }
            }
        }

        #endregion

        //returns the total value for a statistics type for a year
    }
}