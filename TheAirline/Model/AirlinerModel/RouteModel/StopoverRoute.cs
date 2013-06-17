using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirportModel;

namespace TheAirline.Model.AirlinerModel.RouteModel
{
    [ProtoContract]
    //the class for the stop over routes
    public class StopoverRoute
    {
        [ProtoMember(1,AsReference=true)]
        public List<Route> Legs { get; set; }
        [ProtoMember(2,AsReference=true)]
        public Airport Stopover { get; set; }
        public StopoverRoute(Airport stopover)
        {
            this.Legs = new List<Route>();
            this.Stopover = stopover;
        }
        //adds a leg to the stopover route
        public void addLeg(Route leg)
        {
            this.Legs.Add(leg);
        }
       
    }
}
