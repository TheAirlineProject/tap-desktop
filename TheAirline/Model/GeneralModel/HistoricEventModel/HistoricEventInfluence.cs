using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel.HistoricEventModel
{
    //the class which defines what a historic event influences
    [ProtoContract]
    public class HistoricEventInfluence
    {
        public enum InfluenceType { PassengerDemand, Stocks, FuelPrices }
        [ProtoMember(1)]
        public InfluenceType Type { get; set; }
        [ProtoMember(2)]
        public double Value { get; set; } //in percent
        [ProtoMember(3)]
        public DateTime EndDate { get; set; }
        public HistoricEventInfluence(InfluenceType type, double value, DateTime endDate)
        {
            this.Type = type;
            this.EndDate = endDate;
            this.Value = value;
        }
    }
}
