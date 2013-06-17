using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel.CountryModel
{
    //the class for the currency for a country
    [ProtoContract]
    public class CountryCurrency
    {
        public enum CurrencyPosition {Left, Right}
        [ProtoMember(1)]
        public string CurrencySymbol { get; set; }
        [ProtoMember(2)]
        public CurrencyPosition Position { get; set; }
        //dollars to currency
        [ProtoMember(3)]
        public double Rate { get; set; }
        [ProtoMember(4)]
        public DateTime DateFrom { get; set; }
        [ProtoMember(5)]
        public DateTime DateTo { get; set; }
        public CountryCurrency(DateTime datefrom, DateTime dateto, string currencysymbol,CurrencyPosition position,  double rate)
        {
            this.DateFrom = datefrom;
            this.DateTo = dateto;
            this.CurrencySymbol = currencysymbol;
            this.Rate = rate;
            this.Position = position;
        }
    }
}
