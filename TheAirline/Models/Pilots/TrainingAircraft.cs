using System;
using System.Runtime.Serialization;
using TheAirline.Helpers;
using TheAirline.Infrastructure;

namespace TheAirline.Models.Pilots
{
    //the class for a training aircraft
    [Serializable]
    public class TrainingAircraft : BaseModel
    {
        #region Constructors and Destructors

        public TrainingAircraft(TrainingAircraftType type, DateTime boughtDate, FlightSchool flighschool)
        {
            Type = type;
            FlightSchool = flighschool;
            BoughtDate = boughtDate;
        }

        private TrainingAircraft(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        public int Age => MathHelpers.GetAge(BoughtDate);

        [Versioning("bought")]
        public DateTime BoughtDate { get; set; }

        [Versioning("flighschool")]
        public FlightSchool FlightSchool { get; set; }

        [Versioning("type")]
        public TrainingAircraftType Type { get; set; }

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