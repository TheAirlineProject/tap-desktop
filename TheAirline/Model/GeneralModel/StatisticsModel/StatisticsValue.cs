
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace TheAirline.Model.GeneralModel.StatisticsModel
{
    [Serializable]
    public class StatisticsValue
    {
        
        public StatisticsType Stat { get; set; }
        
        public double Value { get; set; }
        public StatisticsValue(StatisticsType stat, double value)
        {
            this.Value = value;
            this.Stat = stat;
        }
    }
}
