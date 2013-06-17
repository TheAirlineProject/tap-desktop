using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace TheAirline.Model.GeneralModel.StatisticsModel
{
    [ProtoContract]
    public class StatisticsValue
    {
        [ProtoMember(1)]
        public StatisticsType Stat { get; set; }
        [ProtoMember(2)]
        public double Value { get; set; }
        public StatisticsValue(StatisticsType stat, double value)
        {
            this.Value = value;
            this.Stat = stat;
        }
    }
}
