using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.PageModel.PageAirlineModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel.Helpers.WorkersModel;
using TheAirline.Model.GeneralModel.ScenarioModel;

namespace TheAirline.Model.GeneralModel.Helpers
{
    //the helpers class for scenarios
    public class ScenarioHelpers
    {
        //sets up a scenario
        public static void SetupScenario(Scenario scenario)
        {
            Airline airline = scenario.Airline;

            GameObject.GetInstance().DayRoundEnabled = true;
            GameObject.GetInstance().TimeZone = scenario.Homebase.Profile.TimeZone;
            GameObject.GetInstance().Difficulty = scenario.Difficulty;
            GameObject.GetInstance().GameTime = new DateTime(scenario.StartYear, 1, 1);
            GameObject.GetInstance().StartDate = GameObject.GetInstance().GameTime;
            //sets the fuel price
            GameObject.GetInstance().FuelPrice = Inflations.GetInflation(GameObject.GetInstance().GameTime.Year).FuelPrice;

            GameObject.GetInstance().HumanAirline = airline;
            GameObject.GetInstance().MainAirline = GameObject.GetInstance().HumanAirline;
            GameObject.GetInstance().HumanAirline.Money = scenario.StartCash;

            Airport airport = scenario.Homebase;

            SetupScenarioAirport(airline, airport);

            PassengerHelpers.CreateDestinationPassengers();

            AirlinerHelpers.CreateStartUpAirliners();

            int pilotsPool = 100 * Airlines.GetAllAirlines().Count;

            GeneralHelpers.CreatePilots(pilotsPool);

            int instructorsPool = 75 * Airlines.GetAllAirlines().Count;

            GeneralHelpers.CreateInstructors(instructorsPool);

            SetupScenarioAirlines(scenario);
            SetupScenario();

            GeneralHelpers.CreateHolidays(GameObject.GetInstance().GameTime.Year);
            GameTimer.GetInstance().start();
            GameObjectWorker.GetInstance().start();

            PageNavigator.NavigateTo(new PageAirline(GameObject.GetInstance().HumanAirline));

            PageNavigator.ClearNavigator();

            // GameObject.GetInstance().HumanAirline.Money = 1000000000;

            GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Standard_News, GameObject.GetInstance().GameTime, Translator.GetInstance().GetString("News", "1001"), string.Format(Translator.GetInstance().GetString("News", "1001", "message"), GameObject.GetInstance().HumanAirline.Profile.CEO, GameObject.GetInstance().HumanAirline.Profile.IATACode)));

        }
        //sets up the airlines for a scenario
        private static void SetupScenarioAirlines(Scenario scenario)
        {
            List<Airline> airlines = new List<Airline>();

            airlines.Add(scenario.Airline);

            foreach (ScenarioAirline airline in scenario.OpponentAirlines)
            {
                airlines.Add(airline.Airline);
                SetupOpponentAirline(airline);
            }

            Airlines.Clear();

            airlines.ForEach(a => Airlines.AddAirline(a));

            SetupHumanAirline(scenario);
        }
        //sets up an opponent airline
        private static void SetupOpponentAirline(ScenarioAirline airline)
        {
            airline.Homebase.Terminals.rentGate(airline.Airline);
            airline.Homebase.Terminals.rentGate(airline.Airline);

            AirportFacility checkinFacility = AirportFacilities.GetFacilities(AirportFacility.FacilityType.CheckIn).Find(f => f.TypeLevel == 1);
            AirportFacility facility = AirportFacilities.GetFacilities(AirportFacility.FacilityType.Service).Find((delegate(AirportFacility f) { return f.TypeLevel == 1; }));

            airline.Homebase.addAirportFacility(airline.Airline, facility, GameObject.GetInstance().GameTime);
            airline.Homebase.addAirportFacility(airline.Airline, checkinFacility, GameObject.GetInstance().GameTime);

            foreach (ScenarioAirlineRoute saroute in airline.Routes)
            {
                SetupScenarioAirport(airline.Airline, saroute.Destination1, saroute.Quantity);
                SetupScenarioAirport(airline.Airline, saroute.Destination2, saroute.Quantity);

                double price = PassengerHelpers.GetPassengerPrice(saroute.Destination1, saroute.Destination2);

                for (int i = 0; i < saroute.Quantity; i++)
                {
                    Guid id = Guid.NewGuid();

                    Route route = new Route(id.ToString(), saroute.Destination1, saroute.Destination2, price);

                    RouteClassesConfiguration configuration = AIHelpers.GetRouteConfiguration(route);

                    foreach (RouteClassConfiguration classConfiguration in configuration.getClasses())
                    {
                        route.getRouteAirlinerClass(classConfiguration.Type).FarePrice = price * GeneralHelpers.ClassToPriceFactor(classConfiguration.Type);

                        foreach (RouteFacility rfacility in classConfiguration.getFacilities())
                            route.getRouteAirlinerClass(classConfiguration.Type).addFacility(rfacility);
                    }

                    saroute.Destination1.Terminals.getEmptyGate(airline.Airline).HasRoute = true;
                    saroute.Destination2.Terminals.getEmptyGate(airline.Airline).HasRoute = true;

                    airline.Airline.addRoute(route);

                    FleetAirliner fAirliner = CreateAirliner(airline.Airline, saroute.AirlinerType);
                    airline.Airline.addAirliner(fAirliner);

                    fAirliner.addRoute(route);

                    AIHelpers.CreateRouteTimeTable(route, fAirliner);

                    fAirliner.Status = FleetAirliner.AirlinerStatus.To_route_start;
                    AirlineHelpers.HireAirlinerPilots(fAirliner);

                    route.LastUpdated = GameObject.GetInstance().GameTime;
                }
            }

        }
        //sets up an airport for an airline
        private static void SetupScenarioAirport(Airline airline, Airport airport, int quantity = 2)
        {
            for (int i = 0; i < quantity; i++)
            {
                airport.Terminals.rentGate(airline);
            }

            AirportFacility checkinFacility = AirportFacilities.GetFacilities(AirportFacility.FacilityType.CheckIn).Find(f => f.TypeLevel == 1);
            AirportFacility facility = AirportFacilities.GetFacilities(AirportFacility.FacilityType.Service).Find((delegate(AirportFacility f) { return f.TypeLevel == 1; }));

            airport.setAirportFacility(airline, facility, GameObject.GetInstance().GameTime);
            airport.setAirportFacility(airline, checkinFacility, GameObject.GetInstance().GameTime);

        }
        //sets up the human airline 
        private static void SetupHumanAirline(Scenario scenario)
        {
            foreach (Airport destination in scenario.Destinations)
            {
                SetupScenarioAirport(GameObject.GetInstance().HumanAirline, destination);
            }

            foreach (KeyValuePair<AirlinerType, int> fleetAirliner in scenario.Fleet)
            {
                for (int i = 0; i < fleetAirliner.Value; i++)
                {
                    GameObject.GetInstance().HumanAirline.addAirliner(CreateAirliner(GameObject.GetInstance().HumanAirline, fleetAirliner.Key));
                }
            }
        }
        //sets up the different scenario setting
        private static void SetupScenario()
        {
           
            Parallel.ForEach(Airports.GetAllAirports(), airport =>
            {
                foreach (Airline airline in Airlines.GetAllAirlines())
                {
                    foreach (AirportFacility.FacilityType type in Enum.GetValues(typeof(AirportFacility.FacilityType)))
                    {
                        AirportFacility noneFacility = AirportFacilities.GetFacilities(type).Find((delegate(AirportFacility facility) { return facility.TypeLevel == 0; }));

                        airport.addAirportFacility(airline, noneFacility, GameObject.GetInstance().GameTime);
                    }
                }
                AirportHelpers.CreateAirportWeather(airport);
            });

            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                if (airline.IsHuman)
                    airline.Money = GameObject.GetInstance().StartMoney;
                airline.StartMoney = airline.Money;
                airline.Fees = new AirlineFees();
                airline.addAirlinePolicy(new AirlinePolicy("Cancellation Minutes", 150));


            }

        }
        //creates an airliner for an airliner
        private static FleetAirliner CreateAirliner(Airline airline, AirlinerType type)
        {
            Airliner airliner = new Airliner(type, airline.Profile.Country.TailNumbers.getNextTailNumber(), GameObject.GetInstance().GameTime);
            Airliners.AddAirliner(airliner);

            FleetAirliner fAirliner = new FleetAirliner(FleetAirliner.PurchasedType.Bought, GameObject.GetInstance().GameTime, airline, airliner, airliner.TailNumber, airline.Airports[0]);

            airliner.clearAirlinerClasses();

            AirlinerHelpers.CreateAirlinerClasses(airliner);

            return fAirliner;

        }



    }
}
