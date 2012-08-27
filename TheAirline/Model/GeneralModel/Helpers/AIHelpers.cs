using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.PassengerModel;
using System.Collections;

namespace TheAirline.Model.GeneralModel.Helpers
{
    //the helpers class for the AI
    public class AIHelpers
    {
        private static Random rnd = new Random();
        //updates a cpu airline
        public static void UpdateCPUAirline(Airline airline)
        {
            CheckForNewHub(airline);
            CheckForNewRoute(airline);
            CheckForUpdateRoute(airline);
            //CheckForOrderOfAirliners(airline);
            CheckForAirlinersWithoutRoutes(airline);
            CheckForAirlineAlliance(airline);

        }
        //checks for any airliners without routes
        private static void CheckForAirlinersWithoutRoutes(Airline airline)
        {
            int i = 0;

            int max = airline.Fleet.FindAll(a => a.Airliner.BuiltDate <= GameObject.GetInstance().GameTime && !a.HasRoute).Count;
            while (i < max && airline.Fleet.FindAll(a => !a.HasRoute).Count > 0)
            {
                CreateNewRoute(airline);
                i++;
            }

        }
        //checks for ordering new airliners
        private static void CheckForOrderOfAirliners(Airline airline)
        {
            int newAirlinersInterval = 0;

            int airliners = airline.Fleet.Count + 1;
            int airlinersWithoutRoute = airline.Fleet.Count(a => !a.HasRoute) + 1;

            switch (airline.Mentality)
            {
                case Airline.AirlineMentality.Aggressive:
                    newAirlinersInterval = 10000;
                    break;
                case Airline.AirlineMentality.Moderate:
                    newAirlinersInterval = 100000;
                    break;
                case Airline.AirlineMentality.Safe:
                    newAirlinersInterval = 1000000;
                    break;
            }
            Boolean newAirliners = rnd.Next(newAirlinersInterval * (airliners / 2) * airlinersWithoutRoute) == 0;

            if (newAirliners)
            {
                //order new airliners for the airline
                OrderAirliners(airline);

            }
        }
        //orders new airliners for an airline
        private static void OrderAirliners(Airline airline)
        {
            int airliners = airline.Fleet.Count;
            int airlinersWithoutRoute = airline.Fleet.Count(a => !a.HasRoute);

            int numberToOrder = rnd.Next(1, 3 - (int)airline.Mentality);

            List<Airport> homeAirports = airline.Airports.FindAll(a => a.getCurrentAirportFacility(airline, AirportFacility.FacilityType.Service).TypeLevel > 0);

            Dictionary<Airport, int> airportsList = new Dictionary<Airport, int>();
            homeAirports.ForEach(a => airportsList.Add(a, (int)a.Profile.Size));

            Airport homeAirport = AIHelpers.GetRandomItem(airportsList);

            List<AirlinerType> types = AirlinerTypes.GetTypes(t => t.Produced.From <= GameObject.GetInstance().GameTime && t.Produced.To >= GameObject.GetInstance().GameTime && t.Price * numberToOrder < airline.Money);
            types = types.OrderBy(t => t.Price).ToList();

            Dictionary<AirlinerType, int> list = new Dictionary<AirlinerType, int>();
            types.ForEach(t => list.Add(t, (int)((t.Range / (t.Price / 100000)))));

            if (list.Keys.Count > 0)
            {
                AirlinerType type = AIHelpers.GetRandomItem(list);

                Dictionary<AirlinerType, int> orders = new Dictionary<AirlinerType, int>();
                orders.Add(type, numberToOrder);


                int days = rnd.Next(30);
                AirlineHelpers.OrderAirliners(airline, orders, homeAirport, GameObject.GetInstance().GameTime.AddMonths(3).AddDays(days));
            }



        }
        //checks for etablishing a new hub
        private static void CheckForNewHub(Airline airline)
        {

            int hubs = Airports.GetAllActiveAirports().Sum(a => a.Hubs.Count(h => h.Airline == airline));

            int newHubInterval = 0;
            switch (airline.Mentality)
            {
                case Airline.AirlineMentality.Aggressive:
                    newHubInterval = 100000;
                    break;
                case Airline.AirlineMentality.Moderate:
                    newHubInterval = 1000000;
                    break;
                case Airline.AirlineMentality.Safe:
                    newHubInterval = 10000000;
                    break;
            }

            Boolean newHub = rnd.Next(newHubInterval * hubs) == 0;

            if (newHub)
            {
                //creates a new hub for the airline
                CreateNewHub(airline);

            }
        }
        //creates a new hub for an airline
        private static void CreateNewHub(Airline airline)
        {

            List<Airport> airports = airline.Airports.FindAll(a => CanCreateHub(airline, a));

            if (airports.Count > 0)
            {
                Airport airport = (from a in airports orderby a.Profile.Size descending select a).First();

                airport.Hubs.Add(new Hub(airline));

                AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, airport.getHubPrice());


            }

        }
        //checks if it is possible to create a hub at an airport
        private static Boolean CanCreateHub(Airline airline, Airport airport)
        {
            int airlineValue = (int)airline.getAirlineValue() + 1;

            int totalAirlineHubs = Airports.GetAllActiveAirports().Sum(a => a.Hubs.Count(h => h.Airline == airline));
            double airlineGatesPercent = Convert.ToDouble(airport.Terminals.getNumberOfGates(airline)) / Convert.ToDouble(airport.Terminals.getNumberOfGates()) * 100;
            Boolean airlineHub = airport.Hubs.Count(h => h.Airline == airline) > 0;

            return (airline.Money > airport.getHubPrice()) && (!airlineHub) && (airlineGatesPercent > 20) && (totalAirlineHubs < airlineValue) && (airport.Hubs.Count < (int)airport.Profile.Size) && (airport.getCurrentAirportFacility(GameObject.GetInstance().HumanAirline, AirportFacility.FacilityType.Service).ServiceLevel >= Hub.MinimumServiceFacility.ServiceLevel);


        }
        //checks for the creating of alliance / join existing alliance for an airline
        private static void CheckForAirlineAlliance(Airline airline)
        {
            int airlineAlliances = airline.Alliances.Count;

            if (airlineAlliances == 0)
            {
                int newAllianceInterval = 10000;
                Boolean newAlliance = rnd.Next(newAllianceInterval) == 0;

                if (newAlliance)
                {
                    Alliance alliance = GetAirlineAlliance(airline);

                    if (alliance == null)
                    {
                        //creates a new alliance for the airline
                        CreateNewAlliance(airline);
                    }
                    //joins an existing alliance
                    else
                    {

                        if (alliance.Members.Contains(GameObject.GetInstance().HumanAirline))
                        {
                            alliance.addPendingMember(new PendingAllianceMember(GameObject.GetInstance().GameTime, alliance, airline, PendingAllianceMember.AcceptType.Request));
                            GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Alliance_News, GameObject.GetInstance().GameTime, "Request to join alliance", string.Format("{0} has requested to joined {1}. The request can be accepted or declined on the alliance page", airline.Profile.Name, alliance.Name)));

                        }
                        else
                        {
                            if (CanJoinAlliance(airline, alliance))
                            {
                                alliance.addMember(airline);
                                GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Alliance_News, GameObject.GetInstance().GameTime, "Joined alliance", string.Format("{0} has joined {1}", airline.Profile.Name, alliance.Name)));
                            }
                        }
                    }

                }
            }
            else
            {
                CheckForInviteToAlliance(airline);
            }
        }
        //checks for inviting airlines to the an alliance for an airline
        private static void CheckForInviteToAlliance(Airline airline)
        {
            Alliance alliance = airline.Alliances[0];

            int members = alliance.Members.Count;
            int inviteToAllianceInterval = 100000;
            Boolean inviteToAlliance = rnd.Next(inviteToAllianceInterval * members) == 0;

            if (inviteToAlliance)
                InviteToAlliance(airline, alliance);
        }
        //invites an airline to an alliance
        private static void InviteToAlliance(Airline airline, Alliance alliance)
        {
            Airline bestFitAirline = GetAllianceAirline(alliance);

            if (bestFitAirline != null)
            {
                if (bestFitAirline == GameObject.GetInstance().HumanAirline)
                {
                    alliance.addPendingMember(new PendingAllianceMember(GameObject.GetInstance().GameTime, alliance, bestFitAirline, PendingAllianceMember.AcceptType.Invitation));
                    GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Alliance_News, GameObject.GetInstance().GameTime, "Invitation to join alliance", string.Format("{0} has invited you to join {1}. The invitation can be accepted or declined on the alliance page", airline.Profile.Name, alliance.Name)));

                }
                else
                {
                    if (DoAcceptAllianceInvitation(bestFitAirline, alliance))
                    {
                        GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Alliance_News, GameObject.GetInstance().GameTime, "Joined alliance", string.Format("{0} has joined {1}", bestFitAirline.Profile.Name, alliance.Name)));
                        alliance.addMember(bestFitAirline);
                    }
                }
            }
        }
        //returns a "good" alliance for an airline to join
        private static Alliance GetAirlineAlliance(Airline airline)
        {
            Alliance bestAlliance = (from a in Alliances.GetAlliances() where !a.Members.Contains(airline) orderby GetAirlineAllianceScore(airline, a, true) descending select a).FirstOrDefault();

            if (bestAlliance != null && GetAirlineAllianceScore(airline, bestAlliance, true) > 50)
                return bestAlliance;
            else
                return null;
        }
        //returns the "score" for an airline compared to an alliance
        private static double GetAirlineAllianceScore(Airline airline, Alliance alliance, Boolean forAlliance)
        {
            IEnumerable<Country> sameCountries = alliance.Members.SelectMany(m => m.Airports).Select(a => a.Profile.Country).Distinct().Intersect(airline.Airports.Select(a => a.Profile.Country).Distinct());
            IEnumerable<Airport> sameDestinations = alliance.Members.SelectMany(m => m.Airports).Distinct().Intersect(airline.Airports);

            double airlineRoutes = airline.Routes.Count;
            double allianceRoutes = alliance.Members.SelectMany(m => m.Routes).Count();

            double coeff = forAlliance ? allianceRoutes * 10 : airlineRoutes * 10;

            double score = coeff + (5 - sameCountries.Count()) * 5 + (5 - sameDestinations.Count()) * 5;

            return score;

        }
        //returns the best fit airline for an alliance
        private static Airline GetAllianceAirline(Alliance alliance)
        {
            Airline bestAirline = (from a in Airlines.GetAllAirlines() where !alliance.Members.Contains(a) && a.Alliances.Count == 0 orderby GetAirlineAllianceScore(a, alliance, false) descending select a).FirstOrDefault();

            if (GetAirlineAllianceScore(bestAirline, alliance, false) > 50)
                return bestAirline;
            else
                return null;
        }
        //creates a new alliance for an airline
        private static void CreateNewAlliance(Airline airline)
        {
            string name = Alliance.GenerateAllianceName();
            Airport headquarter = airline.Airports.FindAll(a => a.getCurrentAirportFacility(airline, AirportFacility.FacilityType.Service).TypeLevel > 0)[0];
            Alliance alliance = new Alliance(GameObject.GetInstance().GameTime, Alliance.AllianceType.Full, name, headquarter);
            alliance.addMember(airline);

            Alliances.AddAlliance(alliance);

            GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Standard_News, GameObject.GetInstance().GameTime, "New alliance", string.Format("A new alliance: {0} has been created by {1}", name, airline.Profile.Name)));

            InviteToAlliance(airline, alliance);

        }
        //checks for updating of an existing route for an airline
        private static void CheckForUpdateRoute(Airline airline)
        {
            int totalHours = rnd.Next(24 * 7, 24 * 13);
            foreach (Route route in airline.Routes.FindAll(r => GameObject.GetInstance().GameTime.Subtract(r.LastUpdated).TotalHours > totalHours))
            {
                if (route.HasAirliner)
                {
                    double balance = route.getBalance(route.LastUpdated, GameObject.GetInstance().GameTime);
                    if (balance < -1000)
                    {
                        if (route.IncomePerPassenger < 0 && route.FillingDegree > 0.50)
                        {
                            foreach (RouteAirlinerClass rac in route.Classes)
                            {
                                rac.FarePrice += 10;
                            }
                            route.LastUpdated = GameObject.GetInstance().GameTime;
                        }
                        if (route.FillingDegree < 0.25)
                        {
                            airline.removeRoute(route);

                            if (route.HasAirliner)
                                route.getAirliners().ForEach(a => a.removeRoute(route));

                            route.Destination1.Terminals.getUsedGate(airline).Route = null; 
                            route.Destination2.Terminals.getUsedGate(airline).Route = null;

                            if (airline.Routes.Count == 0)
                                CreateNewRoute(airline);
                        }
                    }
                }
                if (route.Banned)
                {

                    airline.removeRoute(route);

                    if (route.HasAirliner)
                        route.getAirliners().ForEach(a => a.removeRoute(route));

                    route.Destination1.Terminals.getUsedGate(airline).Route = null;
                    route.Destination2.Terminals.getUsedGate(airline).Route = null;

                    if (airline.Routes.Count == 0)
                        CreateNewRoute(airline);
                }

            }
        }
        //checks for a new route for an airline
        private static void CheckForNewRoute(Airline airline)
        {
            int airlinersInOrder = airline.Fleet.Count(a => a.Airliner.BuiltDate > GameObject.GetInstance().GameTime);

            int newRouteInterval = 0;
            switch (airline.Mentality)
            {
                case Airline.AirlineMentality.Aggressive:
                    newRouteInterval = 100000;
                    break;
                case Airline.AirlineMentality.Moderate:
                    newRouteInterval = 1000000;
                    break;
                case Airline.AirlineMentality.Safe:
                    newRouteInterval = 10000000;
                    break;
            }

            Boolean newRoute = rnd.Next(newRouteInterval * (airlinersInOrder + 1)) / 1100 == 0;//ændres 1100->110

            if (newRoute)
            {
                //creates a new route for the airline
                CreateNewRoute(airline);

            }
        }
        //creates a new route for an airline
        private static void CreateNewRoute(Airline airline)
        {
            List<Airport> homeAirports = airline.Airports.FindAll(a => a.getCurrentAirportFacility(airline, AirportFacility.FacilityType.Service).TypeLevel > 0);
            homeAirports.AddRange(airline.Airports.FindAll(a => a.Hubs.Count(h => h.Airline == airline) > 0)); //hubs

            Airport airport = homeAirports.Find(a => a.Terminals.getFreeGates(airline) > 0);

            if (airport == null)
            {
                airport = homeAirports.Find(a => a.Terminals.getFreeGates() > 0);
                if (airport != null)
                    airport.Terminals.rentGate(airline);
                else
                {
                    airport = GetServiceAirport(airline);
                    if (airport != null)
                        airport.Terminals.rentGate(airline);
                }

            }

            if (airport != null)
            {
                Airport destination = GetDestinationAirport(airline, airport);


                if (destination != null)
                {
                    FleetAirliner fAirliner;

                    KeyValuePair<Airliner, Boolean>? airliner = GetAirlinerForRoute(airline, airport, destination);
                    fAirliner = GetFleetAirliner(airline, airport, destination);

                    if (airliner.HasValue || fAirliner != null)
                    {
                        if (destination.Terminals.getFreeGates(airline) == 0) destination.Terminals.rentGate(airline);

                        if (!airline.Airports.Contains(destination)) airline.addAirport(destination);

                        double price = PassengerHelpers.GetPassengerPrice(airport, destination);

                        Guid id = Guid.NewGuid();

                        Route route = new Route(id.ToString(), airport, destination, price);

                        foreach (AirlinerClass.ClassType type in Enum.GetValues(typeof(AirlinerClass.ClassType)))
                        {
                            route.getRouteAirlinerClass(type).FarePrice = price * GeneralHelpers.ClassToPriceFactor(type);
                            route.getRouteAirlinerClass(type).CabinCrew = 2;
                            route.getRouteAirlinerClass(type).DrinksFacility = RouteFacilities.GetFacilities(RouteFacility.FacilityType.Drinks)[rnd.Next(RouteFacilities.GetFacilities(RouteFacility.FacilityType.Drinks).Count)];// RouteFacilities.GetBasicFacility(RouteFacility.FacilityType.Drinks);
                            route.getRouteAirlinerClass(type).FoodFacility = RouteFacilities.GetFacilities(RouteFacility.FacilityType.Food)[rnd.Next(RouteFacilities.GetFacilities(RouteFacility.FacilityType.Food).Count)];//RouteFacilities.GetBasicFacility(RouteFacility.FacilityType.Food);
                        }

                        airline.addRoute(route);

                        airport.Terminals.getEmptyGate(airline).Route = route;
                        destination.Terminals.getEmptyGate(airline).Route = route;

                        if (fAirliner == null)
                        {

                            if (Countries.GetCountryFromTailNumber(airliner.Value.Key.TailNumber).Name != airline.Profile.Country.Name)
                                airliner.Value.Key.TailNumber = airline.Profile.Country.TailNumbers.getNextTailNumber();


                            if (airliner.Value.Value) //loan
                            {
                                double amount = airliner.Value.Key.getPrice() - airline.Money + 20000000;

                                Loan loan = new Loan(GameObject.GetInstance().GameTime, amount, 120, GeneralHelpers.GetAirlineLoanRate(airline));

                                double payment = loan.getMonthlyPayment();

                                airline.addLoan(loan);
                                AirlineHelpers.AddAirlineInvoice(airline, loan.Date, Invoice.InvoiceType.Loans, loan.Amount);


                            }
                            else
                                AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -airliner.Value.Key.getPrice());

                            if (airport != airline.Airports[0])
                            {
                                AirportFacility facility = airport.getCurrentAirportFacility(airline, AirportFacility.FacilityType.Service);
                                string service = airport.Profile.Name;
                                service = facility.Shortname;
                                service = destination.Profile.Name;
                            }

                            fAirliner = new FleetAirliner(FleetAirliner.PurchasedType.Bought, GameObject.GetInstance().GameTime, airline, airliner.Value.Key, airliner.Value.Key.TailNumber, airport);
                            airline.Fleet.Add(fAirliner);

                        }

                        fAirliner.addRoute(route);
                        CreateRouteTimeTable(route, fAirliner);


                        fAirliner.Status = FleetAirliner.AirlinerStatus.To_route_start;

                        route.LastUpdated = GameObject.GetInstance().GameTime;
                    }
                }
            }
        }
        //returns an airliner from the fleet which fits a route
        private static FleetAirliner GetFleetAirliner(Airline airline, Airport destination1, Airport destination2)
        {
            //Order new airliner
            var fleet = airline.Fleet.FindAll(f => !f.HasRoute && f.Airliner.BuiltDate <= GameObject.GetInstance().GameTime && f.Airliner.Type.Range > MathHelpers.GetDistance(destination1.Profile.Coordinates, destination2.Profile.Coordinates));

            if (fleet.Count > 0)
                return (from f in fleet orderby f.Airliner.Type.Range select f).First();
            else
                return null;
        }
        //returns the sorted list of possible destinations for an airline with a start airport
        public static List<Airport> GetDestinationAirports(Airline airline, Airport airport)
        {
            double maxDistance = (from a in Airliners.GetAirlinersForSale()
                                  select a.Type.Range).Max();

            double minDistance = (from a in Airports.GetAirports(a => a != airport) select MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates)).Min();


            List<Airport> airports = Airports.GetAirports(a => airline.Airports.Find(ar => ar.Profile.Town == a.Profile.Town) == null && !FlightRestrictions.HasRestriction(a.Profile.Country, airport.Profile.Country, GameObject.GetInstance().GameTime, FlightRestriction.RestrictionType.Flights) && !FlightRestrictions.HasRestriction(airport.Profile.Country, a.Profile.Country, GameObject.GetInstance().GameTime, FlightRestriction.RestrictionType.Flights) && !FlightRestrictions.HasRestriction(airline, a.Profile.Country, airport.Profile.Country, GameObject.GetInstance().GameTime));
            List<Route> routes = airline.Routes.FindAll(r => r.Destination1 == airport || r.Destination2 == airport);

            switch (airline.MarketFocus)
            {
                case Airline.AirlineMarket.Global:
                    airports = airports.FindAll(a => AIHelpers.IsRouteInCorrectArea(airport, a) && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) > 100 && airport.Profile.Town != a.Profile.Town && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) < maxDistance && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) > 100);
                    break;
                case Airline.AirlineMarket.Local:
                    airports = airports.FindAll(a => AIHelpers.IsRouteInCorrectArea(airport, a) && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) < Math.Max(minDistance, 1000) && airport.Profile.Town != a.Profile.Town && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) > 50);
                    break;
                case Airline.AirlineMarket.Regional:
                    airports = airports.FindAll(a => a.Profile.Country.Region == airport.Profile.Country.Region && AIHelpers.IsRouteInCorrectArea(airport, a) && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) < maxDistance && airport.Profile.Town != a.Profile.Town && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) > 100);
                    break;
            }

         
            if (airports.Count == 0)
            {
                airports = (from a in Airports.GetAirports(a => AIHelpers.IsRouteInCorrectArea(airport, a) && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) < 5000 && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) > 50) orderby a.Profile.Size descending select a).ToList();

            }

          

            return (from a in airports where routes.Find(r=>r.Destination1 == a || r.Destination2 == a)==null && (a.Terminals.getFreeGates()>0 || a.Terminals.getFreeGates(airline)>0) orderby a.Profile.Size descending select a).ToList();
        }
        //returns the destination for an airline with a start airport
        public static Airport GetDestinationAirport(Airline airline, Airport airport)
        {
            double maxDistance = (from a in Airliners.GetAirlinersForSale()
                                  select a.Type.Range).Max();

            double minDistance = (from a in Airports.GetAirports(a => a != airport) select MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates)).Min();


            List<Airport> airports = Airports.GetAirports(a => airline.Airports.Find(ar => ar.Profile.Town == a.Profile.Town) == null && !FlightRestrictions.HasRestriction(a.Profile.Country, airport.Profile.Country, GameObject.GetInstance().GameTime, FlightRestriction.RestrictionType.Flights) && !FlightRestrictions.HasRestriction(airport.Profile.Country, a.Profile.Country, GameObject.GetInstance().GameTime, FlightRestriction.RestrictionType.Flights) && !FlightRestrictions.HasRestriction(airline, a.Profile.Country, airport.Profile.Country, GameObject.GetInstance().GameTime));
            List<Route> routes = airline.Routes.FindAll(r => r.Destination1 == airport || r.Destination2 == airport);

            switch (airline.MarketFocus)
            {
                case Airline.AirlineMarket.Global:
                    airports = airports.FindAll(a => AIHelpers.IsRouteInCorrectArea(airport, a) && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) > 100 && airport.Profile.Town != a.Profile.Town && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) < maxDistance && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) > 100);
                    break;
                case Airline.AirlineMarket.Local:
                    airports = airports.FindAll(a => AIHelpers.IsRouteInCorrectArea(airport, a) && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) < Math.Max(minDistance, 1000) && airport.Profile.Town != a.Profile.Town && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) > 50);
                    break;
                case Airline.AirlineMarket.Regional:
                    airports = airports.FindAll(a => a.Profile.Country.Region == airport.Profile.Country.Region && AIHelpers.IsRouteInCorrectArea(airport, a) && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) < maxDistance && airport.Profile.Town != a.Profile.Town && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) > 100);
                    break;
            }

            Airport destination = null;
            int counter = 0;

            if (airports.Count == 0)
            {
                airports = (from a in Airports.GetAirports(a => AIHelpers.IsRouteInCorrectArea(airport, a) && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) < 5000 && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) > 50) orderby a.Profile.Size descending select a).ToList();

            }

            airports = (from a in airports orderby a.Profile.Size descending select a).ToList();

            while (destination == null && counter < airports.Count)
            {
                destination = airports[counter];

                if ((routes.Find(r => r.Destination1 == destination || r.Destination2 == destination) != null) || (destination.Terminals.getFreeGates() == 0 && destination.Terminals.getFreeGates(airline) == 0) || (destination == airport)) destination = null;
                counter++;

            }
            return destination;
        }
        //returns if the two destinations are in the correct area (the airport types are ok)
        public static Boolean IsRouteInCorrectArea(Airport dest1, Airport dest2)
        {
            double distance = MathHelpers.GetDistance(dest1.Profile.Coordinates, dest2.Profile.Coordinates);

            return (dest1.Profile.Country == dest2.Profile.Country || distance < 1000 || (dest1.Profile.Country.Region == dest2.Profile.Country.Region && (dest1.Profile.Type == AirportProfile.AirportType.Short_Haul_International || dest1.Profile.Type == AirportProfile.AirportType.Long_Haul_International) && (dest2.Profile.Type == AirportProfile.AirportType.Short_Haul_International || dest2.Profile.Type == AirportProfile.AirportType.Long_Haul_International)) || (dest1.Profile.Type == AirportProfile.AirportType.Long_Haul_International && dest2.Profile.Type == AirportProfile.AirportType.Long_Haul_International));

        }
        //returns the best fit for an airliner for sale for a route true for loan
        public static KeyValuePair<Airliner, Boolean>? GetAirlinerForRoute(Airline airline, Airport destination1, Airport destination2)
        {

            double maxLoanTotal = 100000000;
            double distance = MathHelpers.GetDistance(destination1.Profile.Coordinates, destination2.Profile.Coordinates);

            AirlinerType.TypeRange rangeType = GeneralHelpers.ConvertDistanceToRangeType(distance);

            List<Airliner> airliners = Airliners.GetAirlinersForSale().FindAll(a => a.getPrice() < airline.Money - 1000000 && a.getAge() < 10 && distance < a.Type.Range && rangeType == a.Type.RangeType);

            if (airliners.Count > 0)
                return new KeyValuePair<Airliner, Boolean>((from a in airliners orderby a.Type.Range select a).First(), false);
            else
            {
                if (airline.Mentality == Airline.AirlineMentality.Aggressive)
                {
                    double airlineLoanTotal = airline.Loans.Sum(l => l.PaymentLeft);

                    if (airlineLoanTotal < maxLoanTotal)
                    {
                        List<Airliner> loanAirliners = Airliners.GetAirlinersForSale().FindAll(a => a.getPrice() < airline.Money + maxLoanTotal - airlineLoanTotal && a.getAge() < 10 && distance < a.Type.Range && rangeType == a.Type.RangeType);

                        if (loanAirliners.Count > 0)
                            return new KeyValuePair<Airliner, Boolean>((from a in loanAirliners orderby a.Price select a).First(), true);
                        else
                            return null;
                    }
                    else
                        return null;

                }
                else
                    return null;
            }


        }
        //sets the homebase for an airliner
        public static void SetAirlinerHomebase(FleetAirliner airliner)
        {

            Airport homebase = GetServiceAirport(airliner.Airliner.Airline);

            if (homebase == null)
                homebase = GetDestinationAirport(airliner.Airliner.Airline, airliner.Homebase);
 
            if (homebase.Terminals.getNumberOfGates(airliner.Airliner.Airline) == 0)
                homebase.Terminals.rentGate(airliner.Airliner.Airline);

            airliner.Homebase = homebase;

        }
        //finds an airport and creates a basic service facility for an airline
        private static Airport GetServiceAirport(Airline airline)
        {

            AirportFacility facility = AirportFacilities.GetFacilities(AirportFacility.FacilityType.Service).Find(f => f.TypeLevel == 1);

            var airports = from a in airline.Airports.FindAll(aa => aa.Terminals.getFreeGates() > 0) orderby a.Profile.Size descending select a;

            if (airports.Count() > 0)
            {
                Airport airport = airports.First();

                airport.setAirportFacility(airline, facility, GameObject.GetInstance().GameTime.AddDays(facility.BuildingDays));

                double price = facility.Price;

                if (airport.Profile.Country != airline.Profile.Country)
                    price = price * 1.25;

                AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -price);

                return airport;
            }

            return null;

        }
        //creates the time table for an route for an airliner
        public static void CreateRouteTimeTable(Route route, FleetAirliner airliner)
        {
            Random rnd = new Random();

            TimeSpan minFlightTime = MathHelpers.GetFlightTime(route.Destination1.Profile.Coordinates, route.Destination2.Profile.Coordinates, airliner.Airliner.Type).Add(new TimeSpan(RouteTimeTable.MinTimeBetweenFlights.Ticks));


            if (minFlightTime.Hours < 12 && minFlightTime.Days < 1)
            {
                int numberOfFlightsPerDay = Convert.ToInt16(24 * 60 / (2 * minFlightTime.TotalMinutes));

                string flightCode1 = airliner.Airliner.Airline.getNextFlightCode();
                string flightCode2 = airliner.Airliner.Airline.getNextFlightCode();

                int startMinutes = Convert.ToInt16((24 * 60) - (minFlightTime.TotalMinutes * numberOfFlightsPerDay * 2));

                if (startMinutes < 0) startMinutes = 0;

                TimeSpan flightTime = new TimeSpan(0, 0, 0).Add(new TimeSpan(0, startMinutes / 2, 0));

                for (int i = 0; i < numberOfFlightsPerDay; i++)
                {

                    route.TimeTable.addDailyEntries(new RouteEntryDestination(route.Destination2, flightCode1), flightTime);

                    flightTime = flightTime.Add(minFlightTime);

                    route.TimeTable.addDailyEntries(new RouteEntryDestination(route.Destination1, flightCode2), flightTime);

                    flightTime = flightTime.Add(minFlightTime);
                }
            }
            else
            {
                DayOfWeek day = 0;

                int outTime = 15 * rnd.Next(-12, 12);
                int homeTime = 15 * rnd.Next(-12, 12);

                string flightCode1 = airliner.Airliner.Airline.getNextFlightCode();


                for (int i = 0; i < 3; i++)
                {
                    route.TimeTable.addEntry(new RouteTimeTableEntry(route.TimeTable, day, new TimeSpan(12, 0, 0).Add(new TimeSpan(0, outTime, 0)), new RouteEntryDestination(route.Destination2, flightCode1)));

                    day += 2;
                }

                string flightCode2 = airliner.Airliner.Airline.getNextFlightCode();


                day = (DayOfWeek)1;

                for (int i = 0; i < 3; i++)
                {
                    route.TimeTable.addEntry(new RouteTimeTableEntry(route.TimeTable, day, new TimeSpan(12, 0, 0).Add(new TimeSpan(0, homeTime, 0)), new RouteEntryDestination(route.Destination1, flightCode2)));

                    day += 2;
                }

            }

            foreach (RouteTimeTableEntry e in route.TimeTable.Entries.FindAll(e => e.Airliner == null))
                e.Airliner = airliner;


        }
        //check if an airline can join an alliance
        public static Boolean CanJoinAlliance(Airline airline, Alliance alliance)
        {
            IEnumerable<Country> sameCountries = alliance.Members.SelectMany(m => m.Airports).Select(a => a.Profile.Country).Distinct().Intersect(airline.Airports.Select(a => a.Profile.Country).Distinct());
            IEnumerable<Airport> sameDestinations = alliance.Members.SelectMany(m => m.Airports).Distinct().Intersect(airline.Airports);

            double airlineDestinations = airline.Airports.Count;
            double airlineRoutes = airline.Routes.Count;
            double airlineCountries = airline.Airports.Select(a => a.Profile.Country).Distinct().Count();
            double airlineAlliances = airline.Alliances.Count;

            double allianceRoutes = alliance.Members.SelectMany(m => m.Routes).Count();

            //declines if airline is much smaller than alliance
            if (airlineRoutes * 5 < allianceRoutes)
                return false;

            //declines if there is a match for 75% of the airline and alliance destinations
            if (sameDestinations.Count() >= airlineDestinations * 0.75)
                return false;

            //declines if there is a match for 75% of the airline and alliance countries
            if (sameCountries.Count() >= airlineCountries * 0.75)
                return false;

            return true;

        }
        //check if an airline accepts an invitation to an alliance
        public static Boolean DoAcceptAllianceInvitation(Airline airline, Alliance alliance)
        {

            IEnumerable<Country> sameCountries = alliance.Members.SelectMany(m => m.Airports).Select(a => a.Profile.Country).Distinct().Intersect(airline.Airports.Select(a => a.Profile.Country).Distinct());
            IEnumerable<Airport> sameDestinations = alliance.Members.SelectMany(m => m.Airports).Distinct().Intersect(airline.Airports);

            double airlineDestinations = airline.Airports.Count;
            double airlineRoutes = airline.Routes.Count;
            double airlineCountries = airline.Airports.Select(a => a.Profile.Country).Distinct().Count();
            double airlineAlliances = airline.Alliances.Count;

            double allianceRoutes = alliance.Members.SelectMany(m => m.Routes).Count();

            //declines if invited airline is much larger than alliance
            if (airlineRoutes > 2 * allianceRoutes)
                return false;

            //declines if there is a match for 50% of the airline and alliance destinations
            if (sameDestinations.Count() >= airlineDestinations * 0.50)
                return false;

            //declines if there is a match for 75% of the airline and alliance countries
            if (sameCountries.Count() >= airlineCountries * 0.75)
                return false;

            //declines if the airline already are in "many" alliances - many == 2
            if (airlineAlliances > 2)
                return false;

            return true;
        }
        //returns a random item based on a weighted value
        public static T GetRandomItem<T>(Dictionary<T, int> list)
        {

            List<T> tList = new List<T>();

            foreach (T item in list.Keys)
            {
                for (int i = 0; i < list[item]; i++)
                    tList.Add(item);
            }

            return tList[rnd.Next(tList.Count)];
        }
    }
}
