using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlineModel;

namespace TheAirline.Model.AirportModel
{
    [ProtoContract]
    //the class for the an airport facility for an airline
    public class AirlineAirportFacility
    {
        [ProtoMember(1)]
        public AirportFacility Facility { get; set; }
        [ProtoMember(2)]
        public DateTime FinishedDate { get; set; }
        [ProtoMember(3,AsReference=true)]
        public Airline Airline { get; set; }
        [ProtoMember(4,AsReference=true)]
        public Airport Airport { get; set; }
        public AirlineAirportFacility(Airline airline, Airport airport, AirportFacility facility, DateTime date)
        {
            this.Airline = airline;
            this.Facility = facility;
            this.FinishedDate = date;
            this.Airport = airport;
        }
    }
}
