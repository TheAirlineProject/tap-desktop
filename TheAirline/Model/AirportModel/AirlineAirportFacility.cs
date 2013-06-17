
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlineModel;

namespace TheAirline.Model.AirportModel
{
    [Serializable]
    //the class for the an airport facility for an airline
    public class AirlineAirportFacility
    {
        
        public AirportFacility Facility { get; set; }
        
        public DateTime FinishedDate { get; set; }
        public Airline Airline { get; set; }
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
