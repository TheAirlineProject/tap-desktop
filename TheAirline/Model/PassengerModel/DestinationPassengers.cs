using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.PassengerModel
{
    //the class for the passengers rate for an airport/destination
    public class DestinationPassengers
    {
        public GeneralHelpers.Rate Rate { get; set; }
        public Airport Destination { get; set; }
        public DestinationPassengers(Airport destination, GeneralHelpers.Rate rate)
        {
            this.Rate = rate;
            this.Destination = destination;
        }
            
    }
}
