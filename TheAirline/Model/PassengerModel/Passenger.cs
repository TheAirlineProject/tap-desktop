using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.PassengerModel
{
    //the class for a passenger
    public class Passenger
    {
        public enum PassengerType { Business, Tourist, Other }
        public PassengerType CurrentType { get; set; }
        public PassengerType PrimaryType { get; set; }
        public string ID { get; set; }
        public Airport Destination { get; set; }
        public DateTime Updated { get; set; }
        public Passenger(string id, PassengerType type, Airport airport)
        {
            this.ID = id;
            this.CurrentType = type;
            this.PrimaryType = type;

           // this.Destination = PassengerHelpers.GetPassengerDestination(airport);
        }
    }
}
