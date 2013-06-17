
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel.StatisticsModel;

namespace TheAirline.Model.AirportModel
{
    //the class for an airport statistics value
    [Serializable]
    public class AirportStatisticsValue : StatisticsValue
    {
        
        public Airline Airline { get; set; }
        public AirportStatisticsValue(Airline airline, StatisticsType stat, int value) : base(stat, value) 
        {
            this.Airline = airline;
        }
        public AirportStatisticsValue(Airline airline, StatisticsType stat)
            : this(airline, stat, 0)
        {
        }
    }
}
