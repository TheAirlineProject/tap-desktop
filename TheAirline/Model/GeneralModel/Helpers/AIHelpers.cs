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
using TheAirline.Model.AirlineModel.SubsidiaryModel;
using System.Threading.Tasks;
using System.Diagnostics;
using TheAirline.GUIModel.HelpersModel;


namespace TheAirline.Model.GeneralModel.Helpers
{
    //the helpers class for the AI
    public class AIHelpers
    {
        private static Random rnd = new Random();
        //updates a cpu airline
        public static void UpdateCPUAirline(Airline airline)
        {


            Parallel.Invoke(() =>
            {
                CheckForNewRoute(airline);
            },

                            () =>
                            {
                                CheckForNewHub(airline);
                            },

                            () =>
                            {
                                CheckForUpdateRoute(airline);
                            },

                            () =>
                            {
                                CheckForAirlinersWithoutRoutes(airline);
                            },
                            () =>
                            {
                                CheckForOrderOfAirliners(airline);
                            },
                            () =>
                            {
                                CheckForAirlineAlliance(airline);
                            },

                            () =>
                            {
                                CheckForSubsidiaryAirline(airline);
                            },
                            () =>
                            {
                                CheckForAirlineAirportFacilities(airline);
                            },
                            () =>
                            {
                                CheckForOrderOfAirliners(airline);
                            }
                        ); //close parallel.invoke


        }
        //checks for building airport facilities for the airline
        private static void CheckForAirlineAirportFacilities(Airline airline)
        {
            int minRoutesForTicketOffice = 3 + (int)airline.Mentality;
            List<Airport> airports = airline.Airports.FindAll(a => AirlineHelpers.GetAirportOutboundRoutes(airline, a) >= minRoutesForTicketOffice);



            foreach (Airport airport in airports)
            {
                int airlineticketoffice = 0;
                //Check if someone in the alliance has an Ticket office there, else build one if needed
                if (airline.Alliances != null)
                {
                    foreach (Alliance alliance in airline.Alliances.Where(a => a.Type == Alliance.AllianceType.Full))
                    {
                        foreach (AllianceMember a in alliance.Members.Where(x => airport.getAirlineAirportFacility(x.Airline, AirportFacility.FacilityType.TicketOffice).Facility.TypeLevel > 0))
                        {
                            airlineticketoffice++;
                            break;
                        }
                    }
                }

                if (airport.getAirlineAirportFacility(airline, AirportFacility.FacilityType.TicketOffice).Facility.TypeLevel == 0 && airlineticketoffice == 0)
                {
                    AirportFacility facility = AirportFacilities.GetFacilities(AirportFacility.FacilityType.TicketOffice).Find(f => f.TypeLevel == 1);
                    double price = facility.Price;

                    if (airport.Profile.Country != airline.Profile.Country)
                        price = price * 1.25;

                    AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -price);

                    airport.addAirportFacility(airline, facility, GameObject.GetInstance().GameTime.AddDays(facility.BuildingDays));

                }
            }
        }
        //checks for any airliners without routes
        private static void CheckForAirlinersWithoutRoutes(Airline airline)
        {
            lock (airline.Fleet)
            {
                var fleet = new List<FleetAirliner>(airline.Fleet.FindAll(a => a.Airliner.BuiltDate <= GameObject.GetInstance().GameTime && !a.HasRoute));
                int max = fleet.Count;
                if (max > 0)
                    CreateNewRoute(airline);
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
            Parallel.ForEach(homeAirports, a =>
                {
                    airportsList.Add(a, (int)a.Profile.Size);
                });

            Airport homeAirport = AIHelpers.GetRandomItem(airportsList);

            List<AirlinerType> types = AirlinerTypes.GetTypes(t => t.Produced.From <= GameObject.GetInstance().GameTime && t.Produced.To >= GameObject.GetInstance().GameTime && t.Price * numberToOrder < airline.Money);

            if (airline.AirlineRouteFocus == Route.RouteType.Cargo)
                types.RemoveAll(a => a.TypeAirliner == AirlinerType.TypeOfAirliner.Passenger);

            if (airline.AirlineRouteFocus == Route.RouteType.Passenger)
                types.RemoveAll(a => a.TypeAirliner == AirlinerType.TypeOfAirliner.Cargo);
            
            types = types.OrderBy(t => t.Price).ToList();

            Dictionary<AirlinerType, int> list = new Dictionary<AirlinerType, int>();
            
            foreach (AirlinerType type in types)
                list.Add(type,(int)((type.Range / (Convert.ToDouble(type.Price) / 100000))));
            /*
            Parallel.ForEach(types, t =>
                {
                    list.Add(t, (int)((t.Range / (t.Price / 100000))));
                });*/

            if (list.Keys.Count > 0)
            {
                AirlinerType type = AIHelpers.GetRandomItem(list);

               
                List<AirlinerOrder> orders = new List<AirlinerOrder>();
                orders.Add(new AirlinerOrder(type, AirlinerHelpers.GetAirlinerClasses(type), numberToOrder, false));

                DateTime deliveryDate = AirlinerHelpers.GetOrderDeliveryDate(orders);
                AirlineHelpers.OrderAirliners(airline, orders, homeAirport, deliveryDate);
            }



        }
        //checks for etablishing a new hub
        private static void CheckForNewHub(Airline airline)
        {

            int hubs = airline.getHubs().Count;

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
            HubType.TypeOfHub type = HubType.TypeOfHub.Focus_city;
            List<Airport> airports = new List<Airport>();

            if (airline.MarketFocus == Airline.AirlineFocus.Domestic || airline.MarketFocus == Airline.AirlineFocus.Local)
                type = HubType.TypeOfHub.Focus_city;

            if (airline.MarketFocus == Airline.AirlineFocus.Global)
                type = HubType.TypeOfHub.Hub;

            if (airline.MarketFocus == Airline.AirlineFocus.Regional)
                type = HubType.TypeOfHub.Regional_hub;

            airports = airline.Airports.FindAll(a => AirlineHelpers.CanCreateHub(airline, a, HubTypes.GetHubType(type)));

            if (airports.Count > 0)
            {
                HubType hubtype = HubTypes.GetHubType(type);

                Airport airport = (from a in airports orderby a.Profile.Size descending select a).First();

                airport.addHub(new Hub(airline, hubtype));

                AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, AirportHelpers.GetHubPrice(airport, hubtype)); ;

                NewsFeeds.AddNewsFeed(new NewsFeed(GameObject.GetInstance().GameTime, string.Format(Translator.GetInstance().GetString("NewsFeed", "1003"), airline.Profile.Name, new AirportCodeConverter().Convert(airport), airport.Profile.Town.Name, airport.Profile.Town.Country.ShortName)));

            }

        }
        //checks for the creation of a subsidiary airline for an airline
        private static void CheckForSubsidiaryAirline(Airline airline)
        {
            int subAirlines = airline.Subsidiaries.Count;

            double newSubInterval = 0;
            switch (airline.Mentality)
            {
                case Airline.AirlineMentality.Aggressive:
                    newSubInterval = 100000;
                    break;
                case Airline.AirlineMentality.Moderate:
                    newSubInterval = 1000000;
                    break;
                case Airline.AirlineMentality.Safe:
                    newSubInterval = 10000000;
                    break;
            }
            newSubInterval *= GameObject.GetInstance().Difficulty.AILevel;

            //newSubInterval = 0;

            Boolean newSub = !airline.IsSubsidiary && rnd.Next(Convert.ToInt32(newSubInterval) * (subAirlines + 1)) == 0 && airline.FutureAirlines.Count > 0 && airline.Money > airline.StartMoney / 5;

            if (newSub)
            {
                //creates a new subsidiary airline for the airline
                CreateSubsidiaryAirline(airline);
            }
        }
        //creates a new subsidiary airline for the airline
        private static void CreateSubsidiaryAirline(Airline airline)
        {
            FutureSubsidiaryAirline futureAirline = airline.FutureAirlines[rnd.Next(airline.FutureAirlines.Count)];

            airline.FutureAirlines.Remove(futureAirline);

            SubsidiaryAirline sAirline = AirlineHelpers.CreateSubsidiaryAirline(airline, airline.Money / 5, futureAirline.Name, futureAirline.IATA, futureAirline.Mentality, futureAirline.Market, futureAirline.AirlineRouteFocus, futureAirline.PreferedAirport);
            sAirline.Profile.addLogo(new AirlineLogo(futureAirline.Logo));
            sAirline.Profile.Color = airline.Profile.Color;

            CreateNewRoute(sAirline);

            GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Airline_News, GameObject.GetInstance().GameTime, "Created subsidiary", string.Format("[LI airline={0}] has created a new subsidiary airline [LI airline={1}]", airline.Profile.IATACode, sAirline.Profile.IATACode)));


        }
        //checks for the creation of alliance / join existing alliance for an airline
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

                        if (alliance.IsHumanAlliance)
                        {
                            alliance.addPendingMember(new PendingAllianceMember(GameObject.GetInstance().GameTime, alliance, airline, PendingAllianceMember.AcceptType.Request));
                            GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Alliance_News, GameObject.GetInstance().GameTime, "Request to join alliance", string.Format("[LI airline={0}] has requested to joined {1}. The request can be accepted or declined on the alliance page", airline.Profile.IATACode, alliance.Name)));

                        }
                        else
                        {
                            if (CanJoinAlliance(airline, alliance))
                            {
                                alliance.addMember(new AllianceMember(airline, GameObject.GetInstance().GameTime));
                                GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Alliance_News, GameObject.GetInstance().GameTime, "Joined alliance", string.Format("[LI airline={0}] has joined {1}", airline.Profile.IATACode, alliance.Name)));
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
                    GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Alliance_News, GameObject.GetInstance().GameTime, "Invitation to join alliance", string.Format("[LI airline={0}] has invited you to join {1}. The invitation can be accepted or declined on the alliance page", airline.Profile.IATACode, alliance.Name)));

                }
                else
                {
                    if (DoAcceptAllianceInvitation(bestFitAirline, alliance))
                    {
                        GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Alliance_News, GameObject.GetInstance().GameTime, "Joined alliance", string.Format("[LI airline={0}] has joined {1}", bestFitAirline.Profile.IATACode, alliance.Name)));
                        alliance.addMember(new AllianceMember(bestFitAirline, GameObject.GetInstance().GameTime));
                    }
                }
            }
        }
        //returns a "good" alliance for an airline to join
        private static Alliance GetAirlineAlliance(Airline airline)
        {
            Alliance bestAlliance = (from a in Alliances.GetAlliances() where !a.Members.ToList().Exists(m => m.Airline == airline) orderby GetAirlineAllianceScore(airline, a, true) descending select a).FirstOrDefault();

            if (bestAlliance != null && GetAirlineAllianceScore(airline, bestAlliance, true) > 50)
                return bestAlliance;
            else
                return null;
        }
        //returns the "score" for an airline compared to an alliance
        private static double GetAirlineAllianceScore(Airline airline, Alliance alliance, Boolean forAlliance)
        {
            IEnumerable<Country> sameCountries = alliance.Members.SelectMany(m => m.Airline.Airports).Select(a => a.Profile.Country).Distinct().Intersect(airline.Airports.Select(a => a.Profile.Country).Distinct());
            IEnumerable<Airport> sameDestinations = alliance.Members.SelectMany(m => m.Airline.Airports).Distinct().Intersect(airline.Airports);

            double airlineRoutes = airline.Routes.Count;
            double allianceRoutes = alliance.Members.SelectMany(m => m.Airline.Routes).Count();

            double coeff = forAlliance ? allianceRoutes * 10 : airlineRoutes * 10;

            double score = coeff + (5 - sameCountries.Count()) * 5 + (5 - sameDestinations.Count()) * 5;

            return score;

        }
        //returns the best fit airline for an alliance
        private static Airline GetAllianceAirline(Alliance alliance)
        {
            Airline bestAirline = (from a in Airlines.GetAllAirlines() where !alliance.Members.ToList().Exists(m => m.Airline == a) && a.Alliances.Count == 0 orderby GetAirlineAllianceScore(a, alliance, false) descending select a).FirstOrDefault();

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
            alliance.addMember(new AllianceMember(airline, GameObject.GetInstance().GameTime));

            Alliances.AddAlliance(alliance);

            GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Standard_News, GameObject.GetInstance().GameTime, "New alliance", string.Format("A new alliance: {0} has been created by [LI airline={1}]", name, airline.Profile.IATACode)));

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

                    Route.RouteType routeType = route.Type;

                    if (routeType == Route.RouteType.Mixed || routeType == Route.RouteType.Passenger)
                    {
                        if (balance < -1000)
                        {
                            if (route.FillingDegree > 0.50 && (((PassengerRoute)route).IncomePerPassenger < 0.50))
                            {
                                foreach (RouteAirlinerClass rac in ((PassengerRoute)route).Classes)
                                {
                                    rac.FarePrice += 10;
                                }
                                route.LastUpdated = GameObject.GetInstance().GameTime;
                            }
                            if (route.FillingDegree >= 0.2 && route.FillingDegree <= 0.50)
                                ChangeRouteServiceLevel((PassengerRoute)route);
                            if (route.FillingDegree < 0.2)
                            {

                                airline.removeRoute(route);

                                if (route.HasAirliner)
                                    route.getAirliners().ForEach(a => a.removeRoute(route));

                                if (airline.Routes.Count == 0)
                                    CreateNewRoute(airline);

                                NewsFeeds.AddNewsFeed(new NewsFeed(GameObject.GetInstance().GameTime, string.Format(Translator.GetInstance().GetString("NewsFeed", "1002"), airline.Profile.Name, new AirportCodeConverter().Convert(route.Destination1), new AirportCodeConverter().Convert(route.Destination2))));

                            }
                        }
                    }
                    else
                    {
                        if (balance < -1000)
                        {
                            if (route.FillingDegree > 0.45)
                                ((CargoRoute)route).PricePerUnit += 10;
                            if (route.FillingDegree <= 0.45)
                            {

                                airline.removeRoute(route);

                                if (route.HasAirliner)
                                    route.getAirliners().ForEach(a => a.removeRoute(route));

                                if (airline.Routes.Count == 0)
                                    CreateNewRoute(airline);

                                NewsFeeds.AddNewsFeed(new NewsFeed(GameObject.GetInstance().GameTime, string.Format(Translator.GetInstance().GetString("NewsFeed", "1002"), airline.Profile.Name, new AirportCodeConverter().Convert(route.Destination1), new AirportCodeConverter().Convert(route.Destination2))));

                            }
                        }
                    }
                }
                if (route.Banned)
                {

                    airline.removeRoute(route);

                    if (route.HasAirliner)
                        route.getAirliners().ForEach(a => a.removeRoute(route));

                    if (airline.Routes.Count == 0)
                        CreateNewRoute(airline);

                    NewsFeeds.AddNewsFeed(new NewsFeed(GameObject.GetInstance().GameTime, string.Format(Translator.GetInstance().GetString("NewsFeed", "1002"), airline.Profile.Name, new AirportCodeConverter().Convert(route.Destination1), new AirportCodeConverter().Convert(route.Destination2))));

                }

            }
        }
        //checks for a new route for an airline
        private static void CheckForNewRoute(Airline airline)
        {
            int airlinersInOrder;
            lock (airline.Fleet)
            {
                List<FleetAirliner> fleet = new List<FleetAirliner>(airline.Fleet);
                airlinersInOrder = fleet.Count(a => a.Airliner.BuiltDate > GameObject.GetInstance().GameTime);
            }

            int newRouteInterval = 0;
            switch (airline.Mentality)
            {
                case Airline.AirlineMentality.Aggressive:
                    newRouteInterval = 10000;
                    break;
                case Airline.AirlineMentality.Moderate:
                    newRouteInterval = 100000;
                    break;
                case Airline.AirlineMentality.Safe:
                    newRouteInterval = 1000000;
                    break;
            }

            Boolean newRoute = rnd.Next(newRouteInterval * (airlinersInOrder + 1)) / 1100 == 0;

            if (newRoute)
            {
                //creates a new route for the airline
                CreateNewRoute(airline);

            }
        }
        //creates a new route for an airline
        private static void CreateNewRoute(Airline airline)
        {
            Airport airport = GetRouteStartDestination(airline);

            if (airport != null)
            {
                Airport destination;


                destination = GetDestinationAirport(airline, airport);

                if (destination != null)
                {
                    Boolean doLeasing = rnd.Next(5) > 1 || airline.Money < 10000000;

                    FleetAirliner fAirliner;

                    KeyValuePair<Airliner, Boolean>? airliner = GetAirlinerForRoute(airline, airport, destination, doLeasing, airline.AirlineRouteFocus == Route.RouteType.Cargo);

                    fAirliner = GetFleetAirliner(airline, airport, destination);

                    if (airliner.HasValue || fAirliner != null)
                    {
                        if (!AirportHelpers.HasFreeGates(destination, airline)) AirportHelpers.RentGates(destination, airline);

                        if (!airline.Airports.Contains(destination)) airline.addAirport(destination);

                        Guid id = Guid.NewGuid();

                        Route route = null;

                        if (airline.AirlineRouteFocus == Route.RouteType.Passenger)
                        {
                            double price = PassengerHelpers.GetPassengerPrice(airport, destination);

                            route = new PassengerRoute(id.ToString(), airport, destination, price);

                            RouteClassesConfiguration configuration = GetRouteConfiguration((PassengerRoute)route);

                            foreach (RouteClassConfiguration classConfiguration in configuration.getClasses())
                            {
                                ((PassengerRoute)route).getRouteAirlinerClass(classConfiguration.Type).FarePrice = price * GeneralHelpers.ClassToPriceFactor(classConfiguration.Type);

                                foreach (RouteFacility facility in classConfiguration.getFacilities())
                                    ((PassengerRoute)route).getRouteAirlinerClass(classConfiguration.Type).addFacility(facility);
                            }
                        }

                        if (airline.AirlineRouteFocus == Route.RouteType.Cargo)
                        {
                            route = new CargoRoute(id.ToString(), airport, destination, PassengerHelpers.GetCargoPrice(airport, destination));

                        }

                        Boolean isDeptOk = true;
                        Boolean isDestOk = true;

                        if (!AirportHelpers.HasFreeGates(airport, airline))
                            isDeptOk = AirportHelpers.RentGates(airport, airline);

                        if (!AirportHelpers.HasFreeGates(destination, airline))
                            isDestOk = AirportHelpers.RentGates(airport, airline);

                        if (isDestOk && isDeptOk)
                        {

                            Boolean humanHasRoute = Airlines.GetHumanAirlines().SelectMany(a => a.Routes).ToList().Exists(r => (r.Destination1 == route.Destination1 && r.Destination2 == route.Destination2) || (r.Destination1 == route.Destination2 && r.Destination2 == route.Destination1));

                            if (humanHasRoute && Settings.GetInstance().MailsOnAirlineRoutes)
                                GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Airline_News, GameObject.GetInstance().GameTime, Translator.GetInstance().GetString("News", "1013"), string.Format(Translator.GetInstance().GetString("News", "1013", "message"), airline.Profile.IATACode, route.Destination1.Profile.IATACode, route.Destination2.Profile.IATACode)));

                            Country newDestination = airline.Routes.Count(r => r.Destination1.Profile.Country == airport.Profile.Country || r.Destination2.Profile.Country == airport.Profile.Country) == 0 ? airport.Profile.Country : null;

                            newDestination = airline.Routes.Count(r => r.Destination1.Profile.Country == destination.Profile.Country || r.Destination2.Profile.Country == destination.Profile.Country) == 0 ? destination.Profile.Country : newDestination;

                            if (newDestination != null && Settings.GetInstance().MailsOnAirlineRoutes)
                                GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Airline_News, GameObject.GetInstance().GameTime, Translator.GetInstance().GetString("News", "1014"), string.Format(Translator.GetInstance().GetString("News", "1014", "message"), airline.Profile.IATACode, ((Country)new CountryCurrentCountryConverter().Convert(newDestination)).Name)));

                            if (!AirportHelpers.HasFreeGates(airport, airline))
                                AirportHelpers.RentGates(airport, airline);

                            if (!AirportHelpers.HasFreeGates(destination, airline))
                                AirportHelpers.RentGates(airport, airline);

                          
                            //Console.WriteLine("{3}: {0} has created a route between {1} and {2}", airline.Profile.Name, route.Destination1.Profile.Name, route.Destination2.Profile.Name,GameObject.GetInstance().GameTime.ToShortDateString());

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
                                {
                                    if (doLeasing)
                                        AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Rents, -airliner.Value.Key.LeasingPrice * 2);
                                    else
                                        AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -airliner.Value.Key.getPrice());

                                }


                                fAirliner = new FleetAirliner(doLeasing ? FleetAirliner.PurchasedType.Leased : FleetAirliner.PurchasedType.Bought, GameObject.GetInstance().GameTime, airline, airliner.Value.Key, airport);
                                airline.Fleet.Add(fAirliner);

                                AirlinerHelpers.CreateAirlinerClasses(fAirliner.Airliner);


                            }

                            //NewsFeeds.AddNewsFeed(new NewsFeed(GameObject.GetInstance().GameTime, string.Format(Translator.GetInstance().GetString("NewsFeed", "1001"), airline.Profile.Name, new AirportCodeConverter().Convert(route.Destination1), new AirportCodeConverter().Convert(route.Destination2))));

                          
                            if (route.Type == Route.RouteType.Passenger || route.Type == Route.RouteType.Mixed)
                            {

                                //creates a business route
                                if (IsBusinessRoute(route, fAirliner))
                                    CreateBusinessRouteTimeTable(route, fAirliner);
                                else
                                    CreateRouteTimeTable(route, fAirliner);
                            }
                            if (route.Type == Route.RouteType.Cargo)
                                CreateCargoRouteTimeTable(route, fAirliner);

                            fAirliner.Status = FleetAirliner.AirlinerStatus.To_route_start;
                            AirlineHelpers.HireAirlinerPilots(fAirliner);

                            route.LastUpdated = GameObject.GetInstance().GameTime;
                        }
                        airline.addRoute(route);

                        fAirliner.addRoute(route);


                        AirportFacility checkinFacility = AirportFacilities.GetFacilities(AirportFacility.FacilityType.CheckIn).Find(f => f.TypeLevel == 1);
                        AirportFacility cargoTerminal = AirportFacilities.GetFacilities(AirportFacility.FacilityType.Cargo).Find(f => f.TypeLevel > 0);

                        if (destination.getAirportFacility(airline, AirportFacility.FacilityType.CheckIn).TypeLevel == 0)
                        {
                            destination.addAirportFacility(airline, checkinFacility, GameObject.GetInstance().GameTime);
                            AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -checkinFacility.Price);

                        }
                        if (airport.getAirportFacility(airline, AirportFacility.FacilityType.CheckIn).TypeLevel == 0)
                        {
                            airport.addAirportFacility(airline, checkinFacility, GameObject.GetInstance().GameTime);
                            AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -checkinFacility.Price);

                        }

                        if (destination.getAirportFacility(airline, AirportFacility.FacilityType.Cargo).TypeLevel == 0 && destination.getAirportFacility(null, AirportFacility.FacilityType.Cargo).TypeLevel == 0 && route.Type == Route.RouteType.Cargo)
                        {
                            destination.addAirportFacility(airline, cargoTerminal, GameObject.GetInstance().GameTime);
                            AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -cargoTerminal.Price);

                        }

                        if (airport.getAirportFacility(airline, AirportFacility.FacilityType.Cargo).TypeLevel == 0 && airport.getAirportFacility(null, AirportFacility.FacilityType.Cargo).TypeLevel == 0 && route.Type == Route.RouteType.Cargo)
                        {
                            airport.addAirportFacility(airline, cargoTerminal, GameObject.GetInstance().GameTime);
                            AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -cargoTerminal.Price);

                        }

                    }

                }


            }

        }
        //returns if a given route is a business route
        private static Boolean IsBusinessRoute(Route route, FleetAirliner airliner)
        {
            double maxBusinessRouteTime = new TimeSpan(2, 0, 0).TotalMinutes;

            TimeSpan minFlightTime = MathHelpers.GetFlightTime(route.Destination1.Profile.Coordinates, route.Destination2.Profile.Coordinates, airliner.Airliner.Type).Add(new TimeSpan(FleetAirlinerHelpers.GetMinTimeBetweenFlights(airliner).Ticks));

            return minFlightTime.TotalMinutes <= maxBusinessRouteTime;
        }
        //returns the start destination / homebase for a route
        private static Airport GetRouteStartDestination(Airline airline)
        {
            List<Airport> homeAirports;
            
            lock (airline.Airports)
            {
                homeAirports = airline.Airports.FindAll(a => a.getCurrentAirportFacility(airline, AirportFacility.FacilityType.Service).TypeLevel > 0);
            }
            homeAirports.AddRange(airline.getHubs());

            Airport airport = homeAirports.Find(a => AirportHelpers.HasFreeGates(a, airline));

            if (airport == null)
            {
                airport = homeAirports.Find(a => a.Terminals.getFreeGates() > 0);
                if (airport != null)
                    AirportHelpers.RentGates(airport, airline);
                else
                {
                    airport = GetServiceAirport(airline);
                    if (airport != null)
                        AirportHelpers.RentGates(airport, airline);
                }

            }

            return airport;
        }
        //returns the sorted list of possible destinations for an airline with a start airport
        public static List<Airport> GetDestinationAirports(Airline airline, Airport airport)
        {
            var airliners = from a in Airliners.GetAirlinersForSale()
                                  select a.Type.Range;
            double maxDistance = airliners.Count() == 0 ? 5000 : airliners.Max();

            double minDistance = (from a in Airports.GetAirports(a => a != airport) select MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates)).Min();

            List<Airport> airports = Airports.GetAirports(a => airline.Airports.Find(ar => ar.Profile.Town == a.Profile.Town) == null && AirlineHelpers.HasAirlineLicens(airline, airport, a) && !FlightRestrictions.HasRestriction(a.Profile.Country, airport.Profile.Country, GameObject.GetInstance().GameTime, FlightRestriction.RestrictionType.Flights) && !FlightRestrictions.HasRestriction(airport.Profile.Country, a.Profile.Country, GameObject.GetInstance().GameTime, FlightRestriction.RestrictionType.Flights) && !FlightRestrictions.HasRestriction(airline, a.Profile.Country, airport.Profile.Country, GameObject.GetInstance().GameTime));
            List<Route> routes = airline.Routes.FindAll(r => r.Destination1 == airport || r.Destination2 == airport);

            Airline.AirlineFocus marketFocus = airline.MarketFocus;

            if (airline.Airports.Count < 4)
            {
                List<Airline.AirlineFocus> focuses = new List<Airline.AirlineFocus>();
                focuses.Add(Airline.AirlineFocus.Local);
                focuses.Add(Airline.AirlineFocus.Local);
                focuses.Add(Airline.AirlineFocus.Local);
                focuses.Add(marketFocus);

                marketFocus = focuses[rnd.Next(focuses.Count)];
            }

            switch (marketFocus)
            {
                case Airline.AirlineFocus.Domestic:
                    airports = airports.FindAll(a => a.Profile.Country == airport.Profile.Country);
                    break;
                case Airline.AirlineFocus.Global:
                    airports = airports.FindAll(a => AIHelpers.IsRouteInCorrectArea(airport, a) && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) > 100 && airport.Profile.Town != a.Profile.Town && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) < maxDistance && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) > 100);
                    break;
                case Airline.AirlineFocus.Local:
                    airports = airports.FindAll(a => AIHelpers.IsRouteInCorrectArea(airport, a) && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) < Math.Max(minDistance, 1000) && airport.Profile.Town != a.Profile.Town && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) > 50);
                    break;
                case Airline.AirlineFocus.Regional:
                    airports = airports.FindAll(a => a.Profile.Country.Region == airport.Profile.Country.Region && AIHelpers.IsRouteInCorrectArea(airport, a) && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) < maxDistance && airport.Profile.Town != a.Profile.Town && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) > 100);
                    break;
            }

            if (airports.Count == 0)
            {
                airports = (from a in Airports.GetAirports(a => AIHelpers.IsRouteInCorrectArea(airport, a) && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) < 5000 && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) > 50) orderby a.Profile.Size descending select a).ToList();
            }

            return (from a in airports where routes.Find(r => r.Destination1 == a || r.Destination2 == a) == null && (a.Terminals.getFreeGates() > 0 || AirportHelpers.HasFreeGates(a, airline)) orderby ((int)airport.getDestinationPassengersRate(a, AirlinerClass.ClassType.Economy_Class)) + ((int)a.getDestinationPassengersRate(airport, AirlinerClass.ClassType.Economy_Class)) descending select a).ToList();
        }
        //returns the destination for an airline with a start airport
        public static Airport GetDestinationAirport(Airline airline, Airport airport)
        {

            var airports = GetDestinationAirports(airline, airport);
            if (airports.Count == 0)
                return null;
            else
                return airports[0];
        }

        //returns if the two destinations are in the correct area (the airport types are ok)
        public static Boolean IsRouteInCorrectArea(Airport dest1, Airport dest2)
        {
            double distance = MathHelpers.GetDistance(dest1.Profile.Coordinates, dest2.Profile.Coordinates);

            Boolean isOk = (dest1.Profile.Country == dest2.Profile.Country || distance < 1000 || (dest1.Profile.Country.Region == dest2.Profile.Country.Region && (dest1.Profile.Type == AirportProfile.AirportType.Short_Haul_International || dest1.Profile.Type == AirportProfile.AirportType.Long_Haul_International) && (dest2.Profile.Type == AirportProfile.AirportType.Short_Haul_International || dest2.Profile.Type == AirportProfile.AirportType.Long_Haul_International)) || (dest1.Profile.Type == AirportProfile.AirportType.Long_Haul_International && dest2.Profile.Type == AirportProfile.AirportType.Long_Haul_International));

            return isOk;
        }
        //returns if two destinations for a cargo route is correct
        public static Boolean IsCargoRouteDestinationsCorrect(Airport dest1, Airport dest2, Airline airline)
        {
            return dest1.getAirportFacility(airline, AirportFacility.FacilityType.Cargo, true).TypeLevel > 0 && dest2.getAirportFacility(airline, AirportFacility.FacilityType.Cargo, true).TypeLevel > 0;
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
        //returns the best fit for an airliner for sale for a route true for loan
        public static KeyValuePair<Airliner, Boolean>? GetAirlinerForRoute(Airline airline, Airport destination1, Airport destination2, Boolean doLeasing, Boolean forCargo, Boolean forStartdata = false)
        {

            double maxLoanTotal = 100000000;
            double distance = MathHelpers.GetDistance(destination1.Profile.Coordinates, destination2.Profile.Coordinates);

            AirlinerType.TypeRange rangeType = GeneralHelpers.ConvertDistanceToRangeType(distance);

            List<Airliner> airliners;

            if (forCargo)
            {
                if (doLeasing)
                    airliners = Airliners.GetAirlinersForSale(a => a.Type is AirlinerCargoType).FindAll(a => a.LeasingPrice * 2 < airline.Money && a.getAge() < 10 && distance < a.Type.Range && rangeType == a.Type.RangeType);
                else
                    airliners = Airliners.GetAirlinersForSale(a => a.Type is AirlinerCargoType).FindAll(a => a.getPrice() < airline.Money - 1000000 && a.getAge() < 10 && distance < a.Type.Range && rangeType == a.Type.RangeType);

            }
            else
            {
                if (doLeasing)
                    airliners = Airliners.GetAirlinersForSale(a => a.Type is AirlinerPassengerType).FindAll(a => a.LeasingPrice * 2 < airline.Money && a.getAge() < 10 && distance < a.Type.Range && rangeType == a.Type.RangeType);
                else
                    airliners = Airliners.GetAirlinersForSale(a => a.Type is AirlinerPassengerType).FindAll(a => a.getPrice() < airline.Money - 1000000 && a.getAge() < 10 && distance < a.Type.Range && rangeType == a.Type.RangeType);
            }

            if (airliners.Count > 0)
                return new KeyValuePair<Airliner, Boolean>((from a in airliners orderby a.Type.Range select a).First(), false);
            else
            {
                if (airline.Mentality == Airline.AirlineMentality.Aggressive || airline.Fleet.Count == 0 || forStartdata)
                {
                    double airlineLoanTotal = airline.Loans.Sum(l => l.PaymentLeft);

                    if (airlineLoanTotal < maxLoanTotal)
                    {
                        List<Airliner> loanAirliners;
                        if (forCargo)
                            loanAirliners = Airliners.GetAirlinersForSale(a => a.Type is AirlinerCargoType).FindAll(a => a.getPrice() < airline.Money + maxLoanTotal - airlineLoanTotal && distance < a.Type.Range);

                        else
                            loanAirliners = Airliners.GetAirlinersForSale(a => a.Type is AirlinerPassengerType).FindAll(a => a.getPrice() < airline.Money + maxLoanTotal - airlineLoanTotal && distance < a.Type.Range);

                        if (loanAirliners.Count > 0)
                        {
                            var airliner = (from a in loanAirliners orderby a.Price select a).First();

                            if (airliner == null)
                                return null;

                            return new KeyValuePair<Airliner, Boolean>(airliner, true);
                        }
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
            {
                AirportHelpers.RentGates(homebase, airliner.Airliner.Airline);
                AirportFacility checkinFacility = AirportFacilities.GetFacilities(AirportFacility.FacilityType.CheckIn).Find(f => f.TypeLevel == 1);


                if (homebase.getAirportFacility(airliner.Airliner.Airline, AirportFacility.FacilityType.CheckIn).TypeLevel == 0)
                {
                    homebase.addAirportFacility(airliner.Airliner.Airline, checkinFacility, GameObject.GetInstance().GameTime);
                    AirlineHelpers.AddAirlineInvoice(airliner.Airliner.Airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -checkinFacility.Price);

                }
            }

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

                if (airport.getAirlineAirportFacility(airline, AirportFacility.FacilityType.Service).Facility.TypeLevel == 0)
                {
                    airport.addAirportFacility(airline, facility, GameObject.GetInstance().GameTime.AddDays(facility.BuildingDays));

                    double price = facility.Price;

                    if (airport.Profile.Country != airline.Profile.Country)
                        price = price * 1.25;

                    AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -price);
                }
                return airport;
            }

            return null;

        }
        //creates the time table for route for a number of airliners returns if successed
        public static Boolean CreateRouteTimeTable(Route route, List<FleetAirliner> airliners)
        {
            TimeSpan totalFlightTime = new TimeSpan(airliners.Sum(a => route.getFlightTime(a.Airliner.Type).Ticks));
            TimeSpan maxFlightTime = new TimeSpan(airliners.Max(a => route.getFlightTime(a.Airliner.Type)).Ticks);

            int maxHours = 22 - 6 - (int)Math.Ceiling(maxFlightTime.TotalMinutes); //from 06.00 to 22.00

            if (totalFlightTime.TotalMinutes > maxHours)
                return false;

            TimeSpan startTime = new TimeSpan(6, 0, 0);

            foreach (FleetAirliner airliner in airliners)
            {
                string flightCode1 = airliner.Airliner.Airline.getNextFlightCode(0);
                string flightCode2 = airliner.Airliner.Airline.getNextFlightCode(1);

                CreateAirlinerRouteTimeTable(route, airliner, 1, true, (int)FleetAirlinerHelpers.GetMinTimeBetweenFlights(airliner).TotalMinutes, startTime, flightCode1, flightCode2);

                startTime = startTime.Add(route.getFlightTime(airliner.Airliner.Type));
            }

            return true;
        }
        //creates the time table for a cargo airliner
        public static void CreateCargoRouteTimeTable(Route route, FleetAirliner airliner)
        {
            TimeSpan routeFlightTime = MathHelpers.GetFlightTime(route.Destination1.Profile.Coordinates, route.Destination2.Profile.Coordinates, airliner.Airliner.Type);
            TimeSpan minFlightTime = routeFlightTime.Add(new TimeSpan(FleetAirlinerHelpers.GetMinTimeBetweenFlights(airliner).Ticks));

            int maxHours = 20 - 8; //from 08.00 to 20.00

            int flightsPerDay = Convert.ToInt16(maxHours * 60 / (2 * minFlightTime.TotalMinutes));

            string flightCode1 = airliner.Airliner.Airline.getNextFlightCode(0);
            string flightCode2 = airliner.Airliner.Airline.getNextFlightCode(1);


            route.TimeTable = CreateAirlinerRouteTimeTable(route, airliner, flightsPerDay, flightCode1, flightCode2);

        }
        //creates the time table for a route for an airliner
        public static void CreateRouteTimeTable(Route route, FleetAirliner airliner)
        {

            TimeSpan routeFlightTime = MathHelpers.GetFlightTime(route.Destination1.Profile.Coordinates, route.Destination2.Profile.Coordinates, airliner.Airliner.Type);
            TimeSpan minFlightTime = routeFlightTime.Add(new TimeSpan(FleetAirlinerHelpers.GetMinTimeBetweenFlights(airliner).Ticks));

            int maxHours = 22 - 6; //from 06.00 to 22.00

            int flightsPerDay = Convert.ToInt16(maxHours * 60 / (2 * minFlightTime.TotalMinutes));

            string flightCode1 = airliner.Airliner.Airline.getNextFlightCode(0);
            string flightCode2 = airliner.Airliner.Airline.getNextFlightCode(1);


            route.TimeTable = CreateAirlinerRouteTimeTable(route, airliner, flightsPerDay, flightCode1, flightCode2);
        }
        public static RouteTimeTable CreateAirlinerRouteTimeTable(Route route, FleetAirliner airliner, int flightsPerDay, string flightCode1, string flightCode2)
        {
            int startHour = 6;
            int endHour = 22;

            TimeSpan routeFlightTime = route.getFlightTime(airliner.Airliner.Type);

            TimeSpan minFlightTime = routeFlightTime.Add(FleetAirlinerHelpers.GetMinTimeBetweenFlights(airliner));

            int minDelayMinutes = (int)FleetAirlinerHelpers.GetMinTimeBetweenFlights(airliner).TotalMinutes;

            int startMinutes = Convert.ToInt16(((endHour - startHour) * 60) - (minFlightTime.TotalMinutes * flightsPerDay * 2));

            if (startMinutes < 0) startMinutes = 0;

            TimeSpan flightTime = new TimeSpan(startHour, 0, 0).Add(new TimeSpan(0, startMinutes / 2, 0));


            return CreateAirlinerRouteTimeTable(route, airliner, flightsPerDay, true, minDelayMinutes, flightTime, flightCode1, flightCode2);

        }
        public static RouteTimeTable CreateAirlinerRouteTimeTable(Route route, FleetAirliner airliner, int numberOfFlights, Boolean isDaily, int delayMinutes, TimeSpan startTime, string flightCode1, string flightCode2)
        {
            TimeSpan delayTime = new TimeSpan(0, delayMinutes, 0);
            RouteTimeTable timeTable = new RouteTimeTable(route);

            TimeSpan routeFlightTime = route.getFlightTime(airliner.Airliner.Type);

            TimeSpan minFlightTime = routeFlightTime.Add(delayTime);

            if (minFlightTime.Hours < 12 && minFlightTime.Days < 1 && isDaily)
            {

                TimeSpan flightTime = new TimeSpan(startTime.Hours, startTime.Minutes, startTime.Seconds);//new TimeSpan(startHour, 0, 0).Add(new TimeSpan(0, startMinutes / 2, 0));

                for (int i = 0; i < numberOfFlights; i++)
                {

                    timeTable.addDailyEntries(new RouteEntryDestination(route.Destination2, flightCode1), flightTime);

                    flightTime = flightTime.Add(minFlightTime);

                    timeTable.addDailyEntries(new RouteEntryDestination(route.Destination1, flightCode2), flightTime);

                    flightTime = flightTime.Add(minFlightTime);
                }
            }
            else
            {
                if (isDaily || minFlightTime.Hours >= 12 || minFlightTime.Days >= 1)
                {

                    DayOfWeek day = 0;

                    int outTime = 15 * rnd.Next(-12, 12);
                    int homeTime = 15 * rnd.Next(-12, 12);

                    for (int i = 0; i < 3; i++)
                    {
                        Gate outboundGate = GetFreeAirlineGate(airliner.Airliner.Airline,route.Destination1,day, new TimeSpan(12, 0, 0).Add(new TimeSpan(0, outTime, 0)));
                        timeTable.addEntry(new RouteTimeTableEntry(timeTable, day, new TimeSpan(12, 0, 0).Add(new TimeSpan(0, outTime, 0)), new RouteEntryDestination(route.Destination2, flightCode1),outboundGate));

                        day += 2;
                    }

                    day = (DayOfWeek)1;

                    for (int i = 0; i < 3; i++)
                    {
                        Gate outboundGate = GetFreeAirlineGate(airliner.Airliner.Airline,route.Destination2, day, new TimeSpan(12, 0, 0).Add(new TimeSpan(0, homeTime, 0)));
              
                        timeTable.addEntry(new RouteTimeTableEntry(timeTable, day, new TimeSpan(12, 0, 0).Add(new TimeSpan(0, homeTime, 0)), new RouteEntryDestination(route.Destination1, flightCode2),outboundGate));

                        day += 2;
                    }
                }
                else
                {
                    DayOfWeek day = (DayOfWeek)(7 - numberOfFlights / 2);


                    for (int i = 0; i < numberOfFlights; i++)
                    {
                        TimeSpan flightTime = new TimeSpan(startTime.Hours, startTime.Minutes, startTime.Seconds);

                        timeTable.addEntry(new RouteTimeTableEntry(timeTable, day, flightTime, new RouteEntryDestination(route.Destination2, flightCode1)));

                        flightTime = flightTime.Add(minFlightTime);

                        timeTable.addEntry(new RouteTimeTableEntry(timeTable, day, flightTime, new RouteEntryDestination(route.Destination1, flightCode2)));

                        day++;

                        if (((int)day) > 6)
                            day = 0;
                    }


                }

            }

            foreach (RouteTimeTableEntry e in timeTable.Entries)
                e.Airliner = airliner;

            return timeTable;

        }
        //creates the time table for a business route
        private static void CreateBusinessRouteTimeTable(Route route, FleetAirliner airliner)
        {

            TimeSpan minFlightTime = route.getFlightTime(airliner.Airliner.Type).Add(new TimeSpan(FleetAirlinerHelpers.GetMinTimeBetweenFlights(airliner).Ticks));

            int maxHours = 10 - 6; //from 06:00 to 10:00 and from 18:00 to 22:00

            int flightsPerDay = Convert.ToInt16(maxHours * 60 / (2 * minFlightTime.TotalMinutes));

            string flightCode1 = airliner.Airliner.Airline.getNextFlightCode(0);
            string flightCode2 = airliner.Airliner.Airline.getNextFlightCode(1);

            route.TimeTable = CreateBusinessRouteTimeTable(route, airliner, flightsPerDay, flightCode1, flightCode2);
        }
        //creates a time table for a business route
        public static RouteTimeTable CreateBusinessRouteTimeTable(Route route, FleetAirliner airliner, int flightsPerDay, string flightCode1, string flightCode2)
        {

            RouteTimeTable timeTable = new RouteTimeTable(route);

            TimeSpan minFlightTime = MathHelpers.GetFlightTime(route.Destination1.Profile.Coordinates, route.Destination2.Profile.Coordinates, airliner.Airliner.Type).Add(new TimeSpan(FleetAirlinerHelpers.GetMinTimeBetweenFlights(airliner).Ticks));

            int startHour = 6;
            int endHour = 10;

            int maxHours = endHour - startHour; //entries.Airliners == null

            int startMinutes = Convert.ToInt16((maxHours * 60) - (minFlightTime.TotalMinutes * flightsPerDay * 2));

            if (startMinutes < 0) startMinutes = 0;

            //morning
            TimeSpan flightTime = new TimeSpan(startHour, 0, 0).Add(new TimeSpan(0, startMinutes / 2, 0));

            for (int i = 0; i < flightsPerDay; i++)
            {

                timeTable.addWeekDailyEntries(new RouteEntryDestination(route.Destination2, flightCode1), flightTime);

                flightTime = flightTime.Add(minFlightTime);

                timeTable.addWeekDailyEntries(new RouteEntryDestination(route.Destination1, flightCode2), flightTime);

                flightTime = flightTime.Add(minFlightTime);
            }
            //evening
            startHour = 18;
            flightTime = new TimeSpan(startHour, 0, 0).Add(new TimeSpan(0, startMinutes / 2, 0));
            for (int i = 0; i < flightsPerDay; i++)
            {

                timeTable.addWeekDailyEntries(new RouteEntryDestination(route.Destination2, flightCode1), flightTime);

                flightTime = flightTime.Add(minFlightTime);

                timeTable.addWeekDailyEntries(new RouteEntryDestination(route.Destination1, flightCode2), flightTime);

                flightTime = flightTime.Add(minFlightTime);
            }

            if (timeTable.Entries.Count == 0)
                flightCode1 = "TT";

            foreach (RouteTimeTableEntry e in timeTable.Entries)
                e.Airliner = airliner;

            return timeTable;

        }
        //returns a free gate for an airline
        private static Gate GetFreeAirlineGate(Airline airline, Airport airport, DayOfWeek day, TimeSpan time)
        {
            var airlineGates = airport.Terminals.getGates(airline);

            return airlineGates.FirstOrDefault();

        }
        //check if an airline can join an alliance
        public static Boolean CanJoinAlliance(Airline airline, Alliance alliance)
        {
            IEnumerable<Country> sameCountries = alliance.Members.SelectMany(m => m.Airline.Airports).Select(a => a.Profile.Country).Distinct().Intersect(airline.Airports.Select(a => a.Profile.Country).Distinct());
            IEnumerable<Airport> sameDestinations = alliance.Members.SelectMany(m => m.Airline.Airports).Distinct().Intersect(airline.Airports);

            double airlineDestinations = airline.Airports.Count;
            double airlineRoutes = airline.Routes.Count;
            double airlineCountries = airline.Airports.Select(a => a.Profile.Country).Distinct().Count();
            double airlineAlliances = airline.Alliances.Count;

            double allianceRoutes = alliance.Members.SelectMany(m => m.Airline.Routes).Count();

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

            IEnumerable<Country> sameCountries = alliance.Members.SelectMany(m => m.Airline.Airports).Select(a => a.Profile.Country).Distinct().Intersect(airline.Airports.Select(a => a.Profile.Country).Distinct());
            IEnumerable<Airport> sameDestinations = alliance.Members.SelectMany(m => m.Airline.Airports).Distinct().Intersect(airline.Airports);

            double airlineDestinations = airline.Airports.Count;
            double airlineRoutes = airline.Routes.Count;
            double airlineCountries = airline.Airports.Select(a => a.Profile.Country).Distinct().Count();
            double airlineAlliances = airline.Alliances.Count;

            double allianceRoutes = alliance.Members.SelectMany(m => m.Airline.Routes).Count();

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
        //changes the service level for a route
        private static void ChangeRouteServiceLevel(PassengerRoute route)
        {

            var oRoutes = new List<Route>(Airlines.GetAirlines(a => a != route.Airline).SelectMany(a => a.Routes));

            var sameRoutes = oRoutes.Where(r => (r.Type == Route.RouteType.Mixed || r.Type == Route.RouteType.Passenger) && (r.Destination1 == route.Destination1 && r.Destination2 == route.Destination2) || (r.Destination2 == route.Destination1 && r.Destination1 == route.Destination2));


            if (sameRoutes.Count() > 0)
            {
                double avgServiceLevel = sameRoutes.Where(r => r is PassengerRoute).Average(r => ((PassengerRoute)r).getServiceLevel(AirlinerClass.ClassType.Economy_Class));

                RouteClassesConfiguration configuration = GetRouteConfiguration(route);

                var types = Enum.GetValues(typeof(RouteFacility.FacilityType));

                int ct = 0;
                while (avgServiceLevel > route.getServiceLevel(AirlinerClass.ClassType.Economy_Class) && ct < types.Length)
                {
                    RouteFacility.FacilityType type = (RouteFacility.FacilityType)types.GetValue(ct);

                    RouteFacility currentFacility = route.getRouteAirlinerClass(AirlinerClass.ClassType.Economy_Class).getFacility(type);

                    List<RouteFacility> facilities = RouteFacilities.GetFacilities(type).OrderBy(f => f.ServiceLevel).ToList();

                    int index = facilities.IndexOf(currentFacility);

                    if (index + 1 < facilities.Count)
                        route.getRouteAirlinerClass(AirlinerClass.ClassType.Economy_Class).addFacility(facilities[index + 1]);

                    ct++;

                }
            }
            else
            {
                var types = Enum.GetValues(typeof(RouteFacility.FacilityType));
                double currentServiceLevel = route.getServiceLevel(AirlinerClass.ClassType.Economy_Class);

                int ct = 0;
                while (currentServiceLevel + 50 > route.getServiceLevel(AirlinerClass.ClassType.Economy_Class) && ct < types.Length)
                {
                    RouteFacility.FacilityType type = (RouteFacility.FacilityType)types.GetValue(ct);

                    RouteFacility currentFacility = route.getRouteAirlinerClass(AirlinerClass.ClassType.Economy_Class).getFacility(type);

                    List<RouteFacility> facilities = RouteFacilities.GetFacilities(type).OrderBy(f => f.ServiceLevel).ToList();

                    int index = facilities.IndexOf(currentFacility);

                    if (index + 1 < facilities.Count)
                        route.getRouteAirlinerClass(AirlinerClass.ClassType.Economy_Class).addFacility(facilities[index + 1]);

                    ct++;

                }
            }

        }

        //returns the prefered configuration for a spefic route
        public static RouteClassesConfiguration GetRouteConfiguration(PassengerRoute route)
        {
            double distance = MathHelpers.GetDistance(route.Destination1, route.Destination2);

            if (distance < 500)
                return (RouteClassesConfiguration)Configurations.GetStandardConfiguration("100");
            if (distance < 2000)
                return (RouteClassesConfiguration)Configurations.GetStandardConfiguration("101");
            if (route.Destination1.Profile.Country == route.Destination2.Profile.Country)
                return (RouteClassesConfiguration)Configurations.GetStandardConfiguration("102");
            if (route.Destination1.Profile.Country != route.Destination2.Profile.Country)
                return (RouteClassesConfiguration)Configurations.GetStandardConfiguration("103");

            return null;
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
