using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlineModel;

namespace TheAirline.Model.AirportModel
{
    //the class for a hub at an airport
    public class Hub
    {
        public Airline Airline { get; set; }
        public Hub(Airline airline)
        {
            this.Airline = airline;

        }
    }
}
