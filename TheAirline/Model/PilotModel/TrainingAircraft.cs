using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlineModel;

namespace TheAirline.Model.PilotModel
{
    //the class for a training aircraft
    public class TrainingAircraft
    {
        public TrainingAircraftType Type { get; set; }
        public Airline Airline { get; set; }
        public TrainingAircraft(TrainingAircraftType type, Airline airline)
        {
            this.Type = type;
            this.Airline = airline;
        }
    }
}
