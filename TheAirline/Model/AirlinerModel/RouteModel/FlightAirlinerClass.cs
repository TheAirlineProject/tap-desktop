using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.PassengerModel;

namespace TheAirline.Model.AirlinerModel.RouteModel
{
    /*! Flight airliner class.
   * This class is used for an airliner class onboard of a flight
   * The class needs parameters for type of class and the number of passengers
   */
    [ProtoContract]
    public class FlightAirlinerClass
    {
        [ProtoMember(1)]
        public RouteAirlinerClass AirlinerClass { get; set; }
        [ProtoMember(2)]
        public int Passengers { get; set; }
        public FlightAirlinerClass(RouteAirlinerClass aClass, int passengers)
        {
            this.AirlinerClass = aClass;
            this.Passengers = passengers;
        }
       
    }
}
