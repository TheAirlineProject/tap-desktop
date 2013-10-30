using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.AirlinerModel;


namespace TheAirline.Model.PassengerModel
{
    [Serializable]
    //the class for the demand rate for an airport/destination
    public class DestinationDemand
    {
        
        public ushort Rate { get; set; }
        public string Destination { get; set; }
         public DestinationDemand(string destination, ushort rate)
        {
             this.Rate = rate;
            this.Destination = destination;
        }
       
            
    }
   
}
