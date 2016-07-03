using System;
using System.Runtime.Serialization;
using TheAirline.Infrastructure;

namespace TheAirline.Models.Airlines.AirlineCooperation
{
    //the class for a cooperation
    [Serializable]
    public class Cooperation : BaseModel
    {
        #region Constructors and Destructors

        public Cooperation(CooperationType type, Airline airline, DateTime built)
        {
            BuiltDate = built;
            Airline = airline;
            Type = type;
        }

        private Cooperation(SerializationInfo info, StreamingContext ctxt) :base(info, ctxt)
        {

        }

        #endregion

        #region Public Properties

        [Versioning("airline")]
        public Airline Airline { get; set; }

        [Versioning("built")]
        public DateTime BuiltDate { get; set; }

        [Versioning("type")]
        public CooperationType Type { get; set; }

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