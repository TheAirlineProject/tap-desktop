
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel.HistoricEventModel
{
    //the class which defines what a historic event influences
    [Serializable]
    public class HistoricEventInfluence
    {
        public enum InfluenceType { PassengerDemand, Stocks, FuelPrices }
        
        public InfluenceType Type { get; set; }
        
        public double Value { get; set; } //in percent
        
        public DateTime EndDate { get; set; }
        public HistoricEventInfluence(InfluenceType type, double value, DateTime endDate)
        {
            this.Type = type;
            this.EndDate = endDate;
            this.Value = value;
        }
    }
}
