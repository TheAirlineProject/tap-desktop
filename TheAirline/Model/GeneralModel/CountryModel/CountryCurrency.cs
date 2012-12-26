using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel.CountryModel
{
    //the class for the currency for a country
    public class CountryCurrency
    {
        public string CurrencySymbol { get; set; }
        //dollars to currency
        public double Rate { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public CountryCurrency(DateTime datefrom, DateTime dateto, string currencysymbol, double rate)
        {
            this.DateFrom = datefrom;
            this.DateTo = dateto;
            this.CurrencySymbol = currencysymbol;
            this.Rate = rate;
        }
    }
}
