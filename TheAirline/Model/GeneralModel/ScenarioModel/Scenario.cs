using ProtoBuf;
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
    [ProtoContract]
    public class Scenario
    {
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public Airline Airline { get; set; }
        [ProtoMember(3)]
        public Airport Homebase { get; set; }
        [ProtoMember(4)]
        public int StartYear { get; set; }
        [ProtoMember(5)]
        public long StartCash { get; set; }
        [ProtoMember(6)]
        public string Description { get; set; }
        [ProtoMember(7)]
        public List<ScenarioAirline> OpponentAirlines { get; set; }
        [ProtoMember(8)]
        public List<Airport> Destinations { get; set; }
        [ProtoMember(9)]
        public Dictionary<AirlinerType, int> Fleet { get; set; }
        [ProtoMember(10)]
        public DifficultyLevel Difficulty { get; set; }
        [ProtoMember(11)]
        public List<ScenarioAirlineRoute> Routes { get; set; }
        [ProtoMember(12)]
        public List<ScenarioFailure> Failures { get; set; }
        [ProtoMember(13)]
        public List<ScenarioPassengerDemand> PassengerDemands { get; set; }
        [ProtoMember(14)]
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
