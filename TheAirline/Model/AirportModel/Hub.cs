using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TheAirline.Model.AirlineModel;

namespace TheAirline.Model.AirportModel
{
    [DataContract]
    //the class for a hub at an airport
    public class Hub
    {
        [DataMember]
        public Airline Airline { get; set; }
        [DataMember]
        public HubType Type { get; set; }
        public static AirportFacility MinimumServiceFacility = AirportFacilities.GetFacility("Basic ServiceCenter"); //AirportFacilities.GetFacility("Large ServiceCenter");
        public Hub(Airline airline, HubType type)
        {
            this.Airline = airline;
            this.Type = type;
       
        }
      
    }

}
