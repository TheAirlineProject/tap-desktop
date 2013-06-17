
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirportModel;

namespace TheAirline.Model.GeneralModel.ScenarioModel
{
    //the class for a scenario
    [Serializable]
    public class Scenario
    {
        
        public string Name { get; set; }
        
        public Airline Airline { get; set; }
        
        public Airport Homebase { get; set; }
        
        public int StartYear { get; set; }
        
        public long StartCash { get; set; }
        
        public string Description { get; set; }
        
        public List<ScenarioAirline> OpponentAirlines { get; set; }
        
        public List<Airport> Destinations { get; set; }
        
        public Dictionary<AirlinerType, int> Fleet { get; set; }
        
        public DifficultyLevel Difficulty { get; set; }
        
        public List<ScenarioAirlineRoute> Routes { get; set; }
        public List<ScenarioFailure> Failures { get; set; }
        public List<ScenarioPassengerDemand> PassengerDemands { get; set; }
        public int EndYear { get; set; }
        public Scenario(string name,string description, Airline airline, Airport homebase, int startyear, int endyear, long startcash,DifficultyLevel difficulty)
        {
            this.Name = name;
            this.Airline = airline;
            this.Homebase = homebase;
            this.StartYear = startyear;
            this.StartCash = startcash;
            this.Description = description;
            this.Difficulty = difficulty;
            this.EndYear = endyear;
            this.OpponentAirlines = new List<ScenarioAirline>();
            this.Destinations = new List<Airport>();
            this.Fleet = new Dictionary<AirlinerType, int>();
            this.Routes = new List<ScenarioAirlineRoute>();
            this.Failures = new List<ScenarioFailure>();
            this.PassengerDemands = new List<ScenarioPassengerDemand>();
        }
        //adds a passenger demand to the scenario
        public void addPassengerDemand(ScenarioPassengerDemand demand)
        {
            this.PassengerDemands.Add(demand);
        }
        //adds a route to the scenario
        public void addRoute(ScenarioAirlineRoute route)
        {
            this.Routes.Add(route);
        }
        //adds an airliner type with a quantity to the scenario
        public void addFleet(AirlinerType type, int quantity)
        {
            this.Fleet.Add(type, quantity);
        }
        //adds a destinaton to the scenario
        public void addDestination(Airport destination)
        {
            this.Destinations.Add(destination);
        }
        //adds an opponent airline to the scenario
        public void addOpponentAirline(ScenarioAirline airline)
        {
            this.OpponentAirlines.Add(airline);
        }
        //adds a failure to the scenario
        public void addScenarioFailure(ScenarioFailure failure)
        {
            this.Failures.Add(failure);
        }


    }
    //the list of scenario
    public class Scenarios
    {
        private static List<Scenario> scenarios = new List<Scenario>();
        //adds a scenario to the list
        public static void AddScenario(Scenario scenario)
        {
            scenarios.Add(scenario);
        }
        //returns all scenarios
        public static List<Scenario> GetScenarios()
        {
            return scenarios;
        }
        //returns a scenario with a name
        public static Scenario GetScenario(string name)
        {
            return scenarios.Find(s => s.Name == name);
        }
        //clears the list of scenarios
        public static void Clear()
        {
            scenarios.Clear();
        }
        //returns the number of scenarios
        public static int GetNumberOfScenarios()
        {
            return scenarios.Count;
        }
    }
}
