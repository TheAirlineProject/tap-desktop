using System;
using System.Runtime.Serialization;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.RouteModel
{
    /*! RouteEntryDestination.
  * This is used for destination for the route.
  * The class needs parameter for the destination airport and the flight code
  */

    [Serializable]
    public class RouteEntryDestination : BaseModel, IComparable<RouteEntryDestination>
    {
        #region Constructors and Destructors

        public RouteEntryDestination(Airport airport, string flightCode)
            : this(airport, flightCode, null)
        {
        }

        public RouteEntryDestination(Airport airport, string flightCode, Gate inboundgate)
        {
            Airport = airport;
            FlightCode = flightCode;
            Gate = inboundgate;
        }

        private RouteEntryDestination(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("airport")]
        public Airport Airport { get; set; }

        [Versioning("flightcode")]
        public string FlightCode { get; set; }

        [Versioning("gate")]
        public Gate Gate { get; set; }

        #endregion

        #region Public Methods and Operators

        public int CompareTo(RouteEntryDestination entry)
        {
            int compare = String.Compare(entry.FlightCode, FlightCode, StringComparison.Ordinal);
            if (compare == 0)
            {
                return String.Compare(entry.Airport.Profile.IATACode, Airport.Profile.IATACode, StringComparison.Ordinal);
            }
            return compare;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion
    }
}