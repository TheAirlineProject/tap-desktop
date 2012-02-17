using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace TheAirline.Model.GeneralModel.StatisticsModel
{
    public class StatisticsValue
    {
        public StatisticsType Stat { get; set; }
        public int Value { get; set; }
        public StatisticsValue(StatisticsType stat, int value)
        {
            this.Value = value;
            this.Stat = stat;
        }
    }
}
