using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.AirlinerModel;
using ProtoBuf;

namespace TheAirline.Model.PassengerModel
{
    [ProtoContract]
    //the class for the demand rate for an airport/destination
    public class DestinationDemand
    {
        [ProtoMember(1)]
        public ushort Rate { get; set; }
        [ProtoMember(2, AsReference=true)]
        public Airport Destination { get; set; }
         public DestinationDemand(Airport destination, ushort rate)
        {
             this.Rate = rate;
            this.Destination = destination;
        }
       
            
    }
   
}
