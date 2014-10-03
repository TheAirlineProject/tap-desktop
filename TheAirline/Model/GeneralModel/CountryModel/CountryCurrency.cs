using System;
using System.Runtime.Serialization;

namespace TheAirline.Model.GeneralModel.CountryModel
{
    //the class for the currency for a country
    [Serializable]
    public class CountryCurrency : BaseModel
    {
        #region Constructors and Destructors

        public CountryCurrency(
            DateTime datefrom,
            DateTime dateto,
            string currencysymbol,
            CurrencyPosition position,
            double rate)
        {
            DateFrom = datefrom;
            DateTo = dateto;
            CurrencySymbol = currencysymbol;
            Rate = rate;
            Position = position;
        }

        private CountryCurrency(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Enums

        public enum CurrencyPosition
        {
            Left,

            Right
        }

        #endregion

        #region Public Properties

        [Versioning("symbol")]
        public string CurrencySymbol { get; set; }

        [Versioning("from")]
        public DateTime DateFrom { get; set; }

        [Versioning("to")]
        public DateTime DateTo { get; set; }

        [Versioning("position")]
        public CurrencyPosition Position { get; set; }

        //dollars to currency
        [Versioning("rate")]
        public double Rate { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion
    }
}