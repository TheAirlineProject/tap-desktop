using System;
using System.Runtime.Serialization;
using TheAirline.Infrastructure;
using TheAirline.Models.Airlines;

namespace TheAirline.Models.Airports
{
    [Serializable]
    //the class for a hub at an airport
    public class Hub : BaseModel
    {
        #region Static Fields

        public static AirportFacility MinimumServiceFacility = AirportFacilities.GetFacility("Basic ServiceCenter");
        //AirportFacilities.GetFacility("Large ServiceCenter");

        #endregion

        #region Constructors and Destructors

        public Hub(Airline airline, HubType type)
        {
            Airline = airline;
            Type = type;
        }

        private Hub(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("airline")]
        public Airline Airline { get; set; }

        [Versioning("type")]
        public HubType Type { get; set; }

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