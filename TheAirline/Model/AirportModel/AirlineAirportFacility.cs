using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlineModel;

namespace TheAirline.Model.AirportModel
{
    //the class for the an airport facility for an airline
    public class AirlineAirportFacility
    {
        public AirportFacility Facility { get; set; }
        public DateTime Date { get; set; }
        public Airline Airline{ get; set; }
        public AirlineAirportFacility(Airline airline, AirportFacility facility, DateTime date)
        {
            this.Airline = airline;
            this.Facility = facility;
            this.Date = date;
        }
    }
}
