
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirportModel;

namespace TheAirline.Model.AirlinerModel.RouteModel
{
    /*! RouteEntryDestination.
  * This is used for destination for the route.
  * The class needs parameter for the destination airport and the flight code
  */
       [Serializable]
     public class RouteEntryDestination : IComparable<RouteEntryDestination>
    {
           
           public Airport Airport { get; set; }
           
           public string FlightCode { get; set; }
        public RouteEntryDestination(Airport airport, string flightCode)
        {
            this.Airport = airport;
            this.FlightCode = flightCode;


        }

        public int CompareTo(RouteEntryDestination entry)
        {
            int compare = entry.FlightCode.CompareTo(this.FlightCode);
            if (compare == 0)
                return entry.Airport.Profile.IATACode.CompareTo(this.Airport.Profile.IATACode);
            return compare;
        }
    }
}
