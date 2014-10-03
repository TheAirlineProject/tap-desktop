using System;
using System.Runtime.Serialization;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.StatisticsModel;

namespace TheAirline.Model.AirportModel
{
    //the class for an airport statistics value
    [Serializable]
    public class AirportStatisticsValue : StatisticsValue
    {
        #region Constructors and Destructors

        public AirportStatisticsValue(Airline airline, int year, StatisticsType stat, int value)
            : base(year, stat, value)
        {
            Airline = airline;
        }

        public AirportStatisticsValue(Airline airline, int year, StatisticsType stat)
            : this(airline, year, stat, 0)
        {
        }

        private AirportStatisticsValue(SerializationInfo info, StreamingContext ctxt)
            : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("airline")]
        public Airline Airline { get; set; }

        #endregion

        #region Public Methods and Operators

        public new void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion
    }
}