using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TheAirline.Infrastructure;
using TheAirline.Models.Airliners;
using TheAirline.Models.Airlines;
using TheAirline.Models.Airports;

namespace TheAirline.Models.General.Scenarios
{
    //the class for a scenario
    [Serializable]
    public class Scenario : BaseModel
    {
        #region Constructors and Destructors

        public Scenario(
            string name,
            string description,
            Airline airline,
            Airport homebase,
            int startyear,
            int endyear,
            long startcash,
            DifficultyLevel difficulty)
        {
            Name = name;
            Airline = airline;
            Homebase = homebase;
            StartYear = startyear;
            StartCash = startcash;
            Description = description;
            Difficulty = difficulty;
            EndYear = endyear;
            OpponentAirlines = new List<ScenarioAirline>();
            Destinations = new List<Airport>();
            Fleet = new Dictionary<AirlinerType, int>();
            Routes = new List<ScenarioAirlineRoute>();
            Failures = new List<ScenarioFailure>();
            PassengerDemands = new List<ScenarioPassengerDemand>();
        }

        private Scenario(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("airline")]
        public Airline Airline { get; set; }

        [Versioning("description")]
        public string Description { get; set; }

        [Versioning("destinations")]
        public List<Airport> Destinations { get; set; }

        [Versioning("difficulty")]
        public DifficultyLevel Difficulty { get; set; }

        [Versioning("endyear")]
        public int EndYear { get; set; }

        [Versioning("failures")]
        public List<ScenarioFailure> Failures { get; set; }

        [Versioning("fleet")]
        public Dictionary<AirlinerType, int> Fleet { get; set; }

        [Versioning("homebase")]
        public Airport Homebase { get; set; }

        [Versioning("name")]
        public string Name { get; set; }

        [Versioning("opponents")]
        public List<ScenarioAirline> OpponentAirlines { get; set; }

        [Versioning("demands")]
        public List<ScenarioPassengerDemand> PassengerDemands { get; set; }

        [Versioning("routes")]
        public List<ScenarioAirlineRoute> Routes { get; set; }

        [Versioning("startcash")]
        public long StartCash { get; set; }

        [Versioning("startyear")]
        public int StartYear { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        //adds a destinaton to the scenario
        public void AddDestination(Airport destination)
        {
            Destinations.Add(destination);
        }

        public void AddFleet(AirlinerType type, int quantity)
        {
            Fleet.Add(type, quantity);
        }

        //adds an opponent airline to the scenario
        public void AddOpponentAirline(ScenarioAirline airline)
        {
            OpponentAirlines.Add(airline);
        }

        public void AddPassengerDemand(ScenarioPassengerDemand demand)
        {
            PassengerDemands.Add(demand);
        }

        //adds a route to the scenario
        public void AddRoute(ScenarioAirlineRoute route)
        {
            Routes.Add(route);
        }

        //adds a failure to the scenario
        public void AddScenarioFailure(ScenarioFailure failure)
        {
            Failures.Add(failure);
        }

        #endregion
    }

    //the list of scenario
    public class Scenarios
    {
        #region Static Fields

        private static readonly List<Scenario> scenarios = new List<Scenario>();

        #endregion

        #region Public Methods and Operators

        public static void AddScenario(Scenario scenario)
        {
            scenarios.Add(scenario);
        }

        //returns all scenarios

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

        public static Scenario GetScenario(string name)
        {
            return scenarios.Find(s => s.Name == name);
        }

        public static List<Scenario> GetScenarios()
        {
            return scenarios;
        }

        #endregion

        //adds a scenario to the list
    }
}