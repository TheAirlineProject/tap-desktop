using System;
using System.Runtime.Serialization;
using TheAirline.Infrastructure;
using TheAirline.Models.Airlines;

namespace TheAirline.Models.Airports
{
    [Serializable]
    //the class for the an airport facility for an airline
    public class AirlineAirportFacility : BaseModel
    {
        #region Constructors and Destructors

        public AirlineAirportFacility(Airline airline, Airport airport, AirportFacility facility, DateTime date)
        {
            Airline = airline;
            Facility = facility;
            FinishedDate = date;
            Airport = airport;
        }

        private AirlineAirportFacility(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("airline")]
        public Airline Airline { get; set; }

        [Versioning("airport")]
        public Airport Airport { get; set; }

        [Versioning("facility")]
        public AirportFacility Facility { get; set; }

        [Versioning("finished")]
        public DateTime FinishedDate { get; set; }

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