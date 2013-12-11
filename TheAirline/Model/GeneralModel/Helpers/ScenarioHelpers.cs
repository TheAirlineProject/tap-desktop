using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.GUIModel.PagesModel.AirlinePageModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel.Helpers.WorkersModel;
using TheAirline.Model.GeneralModel.ScenarioModel;
using TheAirline.Model.GeneralModel.StatisticsModel;

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

            GameObject.GetInstance().setHumanAirline(airline);
            GameObject.GetInstance().MainAirline = GameObject.GetInstance().HumanAirline;
            GameObject.GetInstance().HumanAirline.Money = scenario.StartCash;

            GameObject.GetInstance().Scenario = new ScenarioObject(scenario);

            Airport airport = scenario.Homebase;

            SetupScenarioAirport(airline, airport);


           // PassengerHelpers.CreateDestinationPassengers();
         
            AirlinerHelpers.CreateStartUpAirliners();

            int pilotsPool = 100 * Airlines.GetAllAirlines().Count;

            GeneralHelpers.CreatePilots(pilotsPool);

            int instructorsPool = 75 * Airlines.GetAllAirlines().Count;

            GeneralHelpers.CreateInstructors(instructorsPool);

            SetupScenarioAirlines(scenario);
            SetupScenario();
            Setup.SetupAlliances();

            PassengerHelpers.CreateAirlineDestinationDemand();

            GeneralHelpers.CreateHolidays(GameObject.GetInstance().GameTime.Year);
            GameObjectWorker.GetInstance().start();

            PageNavigator.NavigateTo(new PageAirline(GameObject.GetInstance().HumanAirline));

            PageNavigator.ClearNavigator();

            // GameObject.GetInstance().HumanAirline.Money = 1000000000;

            GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Standard_News, GameObject.GetInstance().GameTime, Translator.GetInstance().GetString("News", "1001"), string.Format(Translator.GetInstance().GetString("News", "1001", "message"), GameObject.GetInstance().HumanAirline.Profile.CEO, GameObject.GetInstance().HumanAirline.Profile.IATACode)));

            
                Action<object> action = (object obj) =>
                {
                    
                    PassengerHelpers.CreateDestinationDemand();

                    SetupScenarioPassengerDemand(scenario);

                };

           
                Task t2 = Task.Factory.StartNew(action, "passengers");
        }
        //sets up the passenger demand for a scenario
        private static void SetupScenarioPassengerDemand(Scenario scenario)
        {
            foreach (ScenarioPassengerDemand demand in scenario.PassengerDemands)
            {
                if (demand.Airport != null)
                    PassengerHelpers.ChangePaxDemand(demand.Factor);
                if (demand.Country != null)
                    PassengerHelpers.ChangePaxDemand(Airports.GetAllAirports(a => a.Profile.Country == demand.Country),demand.Factor);

            }
            
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
            AirportHelpers.RentGates(airline.Homebase, airline.Airline);
          
            AirportFacility checkinFacility = AirportFacilities.GetFacilities(AirportFacility.FacilityType.CheckIn).Find(f => f.TypeLevel == 1);
            AirportFacility facility = AirportFacilities.GetFacilities(AirportFacility.FacilityType.Service).Find((delegate(AirportFacility f) { return f.TypeLevel == 1; }));

            airline.Homebase.addAirportFacility(airline.Airline, facility, GameObject.GetInstance().GameTime);
            airline.Homebase.addAirportFacility(airline.Airline, checkinFacility, GameObject.GetInstance().GameTime);

            foreach (ScenarioAirlineRoute saroute in airline.Routes)
            {
                SetupScenarioRoute(saroute, airline.Airline);
            }

        }
        //sets up an scenario airline route
        private static void SetupScenarioRoute(ScenarioAirlineRoute saroute, Airline airline)
        {
            SetupScenarioAirport(airline, saroute.Destination1, saroute.Quantity);
            SetupScenarioAirport(airline, saroute.Destination2, saroute.Quantity);

            double price = PassengerHelpers.GetPassengerPrice(saroute.Destination1, saroute.Destination2);

            for (int i = 0; i < saroute.Quantity; i++)
            {
                Guid id = Guid.NewGuid();

                PassengerRoute route = new PassengerRoute(id.ToString(), saroute.Destination1, saroute.Destination2, price);

                RouteClassesConfiguration configuration = AIHelpers.GetRouteConfiguration(route);

                foreach (RouteClassConfiguration classConfiguration in configuration.getClasses())
                {
                    route.getRouteAirlinerClass(classConfiguration.Type).FarePrice = price * GeneralHelpers.ClassToPriceFactor(classConfiguration.Type);

                    foreach (RouteFacility rfacility in classConfiguration.getFacilities())
                        route.getRouteAirlinerClass(classConfiguration.Type).addFacility(rfacility);
                }

                 airline.addRoute(route);

                FleetAirliner fAirliner = AirlineHelpers.CreateAirliner(airline, saroute.AirlinerType);
                airline.addAirliner(fAirliner);

                fAirliner.addRoute(route);

                AIHelpers.CreateRouteTimeTable(route, fAirliner);

                fAirliner.Status = FleetAirliner.AirlinerStatus.To_route_start;
                AirlineHelpers.HireAirlinerPilots(fAirliner);

                route.LastUpdated = GameObject.GetInstance().GameTime;
            }
        }
        //sets up an airport for an airline
        private static void SetupScenarioAirport(Airline airline, Airport airport, int quantity = 2)
        {
            for (int i = 0; i < quantity; i++)
            {
                if (!AirportHelpers.HasFreeGates(airport,airline))
                    AirportHelpers.RentGates(airport, airline);
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
                    GameObject.GetInstance().HumanAirline.addAirliner(AirlineHelpers.CreateAirliner(GameObject.GetInstance().HumanAirline, fleetAirliner.Key));
                }
            }
            foreach (ScenarioAirlineRoute route in scenario.Routes)
            {
                SetupScenarioRoute(route, GameObject.GetInstance().HumanAirline);
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
       
        //updates the pax demands for the scenario
        private static void UpdatePassengerDemands(ScenarioObject scenario)
        {
            
            foreach (ScenarioPassengerDemand demand in scenario.Scenario.PassengerDemands)
                if (GameObject.GetInstance().GameTime.ToShortDateString() == demand.EndDate.ToShortDateString())
                {
                    if (demand.Airport != null)
                        PassengerHelpers.ChangePaxDemand(-demand.Factor);
                    if (demand.Country != null)
                        PassengerHelpers.ChangePaxDemand(Airports.GetAllAirports(a => a.Profile.Country == demand.Country), -demand.Factor);

                }
              
        }
        //checks for the different failure scenarios
        public static void UpdateScenario(ScenarioObject scenario)
        {
            double monthsSinceStart = MathHelpers.GetMonthsBetween(GameObject.GetInstance().StartDate, GameObject.GetInstance().GameTime);
            
            var failuresToCheck = scenario.Scenario.Failures.FindAll(f => f.CheckMonths == 1 || f.CheckMonths == monthsSinceStart);
            foreach (ScenarioFailure failure in failuresToCheck)
            {
                Boolean failureOk = true;
                if (failure.Type == ScenarioFailure.FailureType.Cash)
                {
                    double totalMoney = GameObject.GetInstance().MainAirline.Money;

                    totalMoney += GameObject.GetInstance().MainAirline.Subsidiaries.Sum(s => s.Money);

                    failureOk = totalMoney > Convert.ToInt64(failure.Value);
                }
                if (failure.Type == ScenarioFailure.FailureType.Fleet)
                {
                    int fleetSize = GameObject.GetInstance().MainAirline.Fleet.Count + GameObject.GetInstance().MainAirline.Subsidiaries.Sum(s => s.Fleet.Count);
                    failureOk = fleetSize > Convert.ToInt32(failure.Value);
                }
                if (failure.Type == ScenarioFailure.FailureType.Domestic)
                {
                    int domesticDestinations = GameObject.GetInstance().MainAirline.Airports.FindAll(a => a.Profile.Country == GameObject.GetInstance().MainAirline.Profile.Country).Count;

                    domesticDestinations += GameObject.GetInstance().MainAirline.Subsidiaries.Sum(s => s.Airports.Count(a => a.Profile.Country == s.Profile.Country));

                    failureOk = domesticDestinations > Convert.ToInt32(failure.Value);
                }
                if (failure.Type == ScenarioFailure.FailureType.Intl)
                {
                    int intlDestinations = GameObject.GetInstance().MainAirline.Airports.FindAll(a => a.Profile.Country != GameObject.GetInstance().MainAirline.Profile.Country).Count;

                    intlDestinations += GameObject.GetInstance().MainAirline.Subsidiaries.Sum(s => s.Airports.Count(a => a.Profile.Country != s.Profile.Country));
                    
                    failureOk = intlDestinations > Convert.ToInt32(failure.Value);
                }
                if (failure.Type == ScenarioFailure.FailureType.PaxGrowth)
                {
                    double paxLastYear = GameObject.GetInstance().MainAirline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year - 2, StatisticsTypes.GetStatisticsType("Passengers")) + GameObject.GetInstance().MainAirline.Subsidiaries.Sum(s=>s.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year- 2, StatisticsTypes.GetStatisticsType("Passengers")));
                    double paxCurrentYear = GameObject.GetInstance().MainAirline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year - 1, StatisticsTypes.GetStatisticsType("Passengers")) + GameObject.GetInstance().MainAirline.Subsidiaries.Sum(s => s.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year - 1, StatisticsTypes.GetStatisticsType("Passengers")));

                    double change = (paxCurrentYear - paxLastYear) / paxLastYear * 100;

                    failureOk = change > Convert.ToDouble(failure.Value);
                }
                if (failure.Type == ScenarioFailure.FailureType.FleetAge)
                {
                    double avgFleetAge = (GameObject.GetInstance().MainAirline.getAverageFleetAge() + GameObject.GetInstance().MainAirline.Subsidiaries.Sum(s=>s.getAverageFleetAge())) / (1 + GameObject.GetInstance().MainAirline.Subsidiaries.Count); 
                    failureOk = Convert.ToDouble(failure.Value) > avgFleetAge;
                }
                if (failure.Type == ScenarioFailure.FailureType.Pax)
                {
                    double totalPassengers = GameObject.GetInstance().MainAirline.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Passengers")) + GameObject.GetInstance().MainAirline.Subsidiaries.Sum(s => s.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Passengers")));

                    failureOk = Convert.ToDouble(failure.Value) * 1000 < totalPassengers;

                }
                if (failure.Type == ScenarioFailure.FailureType.Bases)
                {
                    int homeBases = GameObject.GetInstance().MainAirline.Airports.FindAll(a => a.getCurrentAirportFacility(GameObject.GetInstance().MainAirline, AirportFacility.FacilityType.Service).TypeLevel > 0).Count;
                    homeBases += GameObject.GetInstance().MainAirline.Subsidiaries.Sum(s => s.Airports.Count(a => a.getCurrentAirportFacility(s, AirportFacility.FacilityType.Service).TypeLevel > 0));

                    failureOk = homeBases <= Convert.ToInt32(failure.Value);
                }
                if (failure.Type == ScenarioFailure.FailureType.Debt)
                {
                    double debt =GameObject.GetInstance().MainAirline.Loans.Sum(l=>l.PaymentLeft) + GameObject.GetInstance().MainAirline.Money;
                    debt += GameObject.GetInstance().MainAirline.Subsidiaries.Sum(s => s.Loans.Sum(l => l.PaymentLeft) + s.Money);


                    failureOk = debt <= Convert.ToDouble(failure.Value);
                }

                if (failure.Type == ScenarioFailure.FailureType.JetRation)
                {
                    double totalFleet = Convert.ToDouble(GameObject.GetInstance().MainAirline.Fleet.Count) + Convert.ToDouble(GameObject.GetInstance().MainAirline.Subsidiaries.Sum(s => s.Fleet.Count));
                    double totalJets = Convert.ToDouble(GameObject.GetInstance().MainAirline.Fleet.Count(f=>f.Airliner.Type.Engine == AirlinerType.EngineType.Jet)) + Convert.ToDouble(GameObject.GetInstance().MainAirline.Subsidiaries.Sum(s=>s.Fleet.Count(f=>f.Airliner.Type.Engine == AirlinerType.EngineType.Jet)));

                    double jetRation = totalJets / totalFleet;

                    failureOk = jetRation >= Convert.ToDouble(failure.Value);

                }

                if (!failureOk)
                {
                    if (failure.MonthsOfFailure == 1)
                    {
                        EndScenario(failure);
                    }
                    else
                    {
                        Boolean failingScenario = UpdateFailureValue(scenario,failure);

                        if (failingScenario)
                            EndScenario(failure);
                    }
                }
                //( Safety, Debt, Security,  Crime)
            }
            UpdatePassengerDemands(GameObject.GetInstance().Scenario);

            if (GameObject.GetInstance().Scenario.Scenario.EndYear == GameObject.GetInstance().GameTime.Year)
                GameObject.GetInstance().Scenario.IsSuccess = true;
        }
        //adds another month for where the scenario parameter has not been fulfilled and returns if failing scenario
        private static Boolean UpdateFailureValue(ScenarioObject scenario, ScenarioFailure failure)
        {
            ScenarioFailureObject failureObject = scenario.getScenarioFailure(failure);
            int monthsSinceLastFailure = MathHelpers.GetMonthsBetween(failureObject.LastFailureTime,GameObject.GetInstance().GameTime);

            if (monthsSinceLastFailure == 1)
                failureObject.Failures++;
            else
                failureObject.Failures = 1;

            failureObject.LastFailureTime = GameObject.GetInstance().GameTime;

            return failureObject.Failures == failure.MonthsOfFailure;

        }
        //ends a scenario
        private static void EndScenario(ScenarioFailure failure)
        {
            GameObject.GetInstance().Scenario.ScenarioFailed = failure;
          
        }


    }
}
