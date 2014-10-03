using System;
using System.Runtime.Serialization;

namespace TheAirline.Model.GeneralModel.StatisticsModel
{
    [Serializable]
    public class StatisticsValue : BaseModel
    {
        #region Constructors and Destructors

        public StatisticsValue(int year, StatisticsType stat, double value)
        {
            Value = value;
            Stat = stat;
            Year = year;
        }

        protected StatisticsValue(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("stat")]
        public StatisticsType Stat { get; set; }

        [Versioning("value")]
        public double Value { get; set; }

        [Versioning("year")]
        public int Year { get; set; }

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