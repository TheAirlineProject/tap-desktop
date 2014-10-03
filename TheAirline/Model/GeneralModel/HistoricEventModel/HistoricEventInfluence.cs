using System;
using System.Runtime.Serialization;

namespace TheAirline.Model.GeneralModel.HistoricEventModel
{
    //the class which defines what a historic event influences
    [Serializable]
    public class HistoricEventInfluence : BaseModel
    {
        #region Constructors and Destructors

        public HistoricEventInfluence(InfluenceType type, double value, DateTime endDate)
        {
            Type = type;
            EndDate = endDate;
            Value = value;
        }

        private HistoricEventInfluence(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Enums

        public enum InfluenceType
        {
            PassengerDemand,

            Stocks,

            FuelPrices
        }

        #endregion

        #region Public Properties

        [Versioning("enddate")]
        public DateTime EndDate { get; set; }

        [Versioning("type")]
        public InfluenceType Type { get; set; }

        [Versioning("value")]
        public double Value { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion

        //in percent
    }
}