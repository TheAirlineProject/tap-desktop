
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirportModel;

namespace TheAirline.Model.GeneralModel.ScenarioModel
{
    //the class for an airline (opponent) in a scenario
   [Serializable]
   public class ScenarioAirline
    {
       
       public Airline Airline { get; set; }
       
       public Airport Homebase { get; set; }
       
       public List<ScenarioAirlineRoute> Routes { get; set; }
        public ScenarioAirline(Airline airline, Airport homebase)
        {
            this.Airline = airline;
            this.Homebase = homebase;
            this.Routes = new List<ScenarioAirlineRoute>();
        }
        //adds a route to the scenario airline
        public void addRoute(ScenarioAirlineRoute route)
        {
            this.Routes.Add(route);
        }
    }
    [Serializable]
    //a route for an scenario airline
    public class ScenarioAirlineRoute
    {
        public Airport Destination1 { get; set; }
        public Airport Destination2 { get; set; }
        public AirlinerType AirlinerType { get; set; }
        public int Quantity { get; set; }
        public ScenarioAirlineRoute(Airport destination1, Airport destination2, AirlinerType airlinertype, int quantity)
        {
            this.Destination1 = destination1;
            this.Destination2 = destination2;
            this.AirlinerType = airlinertype;
            this.Quantity = quantity;
        }
    }
}
