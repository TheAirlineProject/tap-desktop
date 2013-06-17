
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirportModel;

namespace TheAirline.Model.AirlinerModel.RouteModel
{
    [Serializable]
    //the class for the stop over routes
    public class StopoverRoute
    {
        public List<Route> Legs { get; set; }
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
