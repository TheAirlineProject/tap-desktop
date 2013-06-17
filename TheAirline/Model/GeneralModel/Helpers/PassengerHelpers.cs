using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel.StatisticsModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.Model.PassengerModel;
using TheAirline.Model.GeneralModel.HolidaysModel;
using TheAirline.Model.GeneralModel.WeatherModel;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace TheAirline.Model.GeneralModel
{
    //the helper class for the passengers
    public class PassengerHelpers
    {
        private static Dictionary<Airline, double> HappinessPercent = new Dictionary<Airline, double>();
        private static Dictionary<GeneralHelpers.Size, double> CargoFactors = new Dictionary<GeneralHelpers.Size, double>();
        private static Random rnd = new Random();

        //returns the passengers happiness for an airline
        public static double GetPassengersHappiness(Airline airline)
        {

            double passengers = 0;
            foreach (int year in airline.Statistics.getYears())
                passengers += Convert.ToDouble(airline.Statistics.getStatisticsValue(year, StatisticsTypes.GetStatisticsType("Passengers")));
            double value = GetHappinessValue(airline);

            if (passengers == 0)
                return 0;
            else
                return value / passengers * 100.0;


        }
        //adds happiness to an airline
        public static void AddPassengerHappiness(Airline airline)
        {
            lock (HappinessPercent)
            {
                if (HappinessPercent.ContainsKey(airline))
                    HappinessPercent[airline] += 1;
                else
                    HappinessPercent.Add(airline, 1);
            }
        }
        // chs, 2011-13-10 added for loading of passenger happiness
        public static void SetPassengerHappiness(Airline airline, double value)
        {
            if (HappinessPercent.ContainsKey(airline))
                HappinessPercent[airline] = value;
            else
                HappinessPercent.Add(airline, value);
        }
        public static double GetHappinessValue(Airline airline)
        {
            if (HappinessPercent.ContainsKey(airline))
                return HappinessPercent[airline];
            else
                return 0;
        }

        //returns a random destination from an airport
        private static Airport GetRandomDestination(Airport currentAirport)
        {
            Dictionary<Airport, int> airportsList = new Dictionary<Airport, int>();
            Airports.GetAirports(a => currentAirport != null && a != currentAirport && !FlightRestrictions.HasRestriction(currentAirport.Profile.Country, a.Profile.Country, GameObject.GetInstance().GameTime, FlightRestriction.RestrictionType.Flights)).ForEach(a => airportsList.Add(a, (int)a.Profile.Size * (a.Profile.Country == currentAirport.Profile.Country ? 7 : 3)));

            if (airportsList.Count > 0)
                return AIHelpers.GetRandomItem(airportsList);
            else
                return null;
        }
        //returns the passenger demand from nearby airport with no routes for a destination
        private static double GetNearbyPassengerDemand(Airport airportCurrent, Airport airportDestination, FleetAirliner airliner, AirlinerClass.ClassType type)
        {

            TimeSpan flightTime = MathHelpers.GetFlightTime(airportCurrent, airportDestination, airliner.Airliner.Type);

            double maxDistance = (flightTime.TotalHours * 0.5) * 100;

            var nearbyAirports = AirportHelpers.GetAirportsNearAirport(airportCurrent, maxDistance).DefaultIfEmpty().Where(a => !AirportHelpers.HasRoute(a, airportDestination));

            double demand = 0;

            foreach (Airport airport in nearbyAirports)
            {
                if (airport != null)
                {
                    double distance = MathHelpers.GetDistance(airportCurrent, airport);

                    double airportDemand = (double)airport.getDestinationPassengersRate(airportDestination, type);

                    if (distance < 150)
                        demand += airportDemand * 0.75;

                    if (distance >= 150 && distance < 225)
                        demand += airportDemand * 0.5;

                    if (distance >= 225 && distance < 300)
                        demand += airportDemand * 0.25;

                    if (distance >= 300 && distance < 400)
                        demand += airportDemand * 0.10;
                }
            }

            return demand;
        }
        //returns the passenger demand for routes with airportdestination as connection point
        private static double GetFlightConnectionPassengers(Airport airportCurrent, Airport airportDestination, FleetAirliner airliner, AirlinerClass.ClassType type)
        {
            double legDistance = MathHelpers.GetDistance(airportCurrent, airportDestination);

            double demandOrigin = 0;
            double demandDestination = 0;

            var routesFromDestination = airliner.Airliner.Airline.Routes.FindAll(r => ((r.Destination2 == airportDestination || r.Destination1 == airportDestination) && (r.Destination1 != airportCurrent && r.Destination2 != airportCurrent)));
            var routesToOrigin = airliner.Airliner.Airline.Routes.FindAll(r => ((r.Destination1 == airportCurrent || r.Destination2 == airportCurrent) && (r.Destination2 != airportDestination && r.Destination1 != airportDestination)));

            foreach (PassengerRoute route in routesFromDestination)
            {
                Airport tDest = route.Destination1 == airportDestination ? route.Destination2 : route.Destination1;

                double totalDistance = MathHelpers.GetDistance(airportCurrent, tDest);

                int directRoutes = AirportHelpers.GetAirportRoutes(airportCurrent, tDest).Count;

                if (route.getDistance() + legDistance < totalDistance * 3 && directRoutes < 2)
                {
                    double demand = (double)airportCurrent.getDestinationPassengersRate(tDest, type);
                    demandDestination += (demand * 0.25);
                }
            }

            foreach (PassengerRoute route in routesToOrigin)
            {
                Airport tDest = route.Destination1 == airportCurrent ? route.Destination2 : route.Destination1;

                double totalDistance = MathHelpers.GetDistance(tDest, airportDestination);

                int directRoutes = AirportHelpers.GetAirportRoutes(tDest, airportDestination).Count;

                if (route.getDistance() + legDistance < totalDistance * 3 && directRoutes < 2)
                {
                    double demand = (double)tDest.getDestinationPassengersRate(airportDestination, type);
                    demandOrigin += (demand * 0.25);
                }
            }
            //alliances
            if (airliner.Airliner.Airline.Alliances.Count > 0)
            {
                var allianceRoutesFromDestination = airliner.Airliner.Airline.Alliances.SelectMany(a => a.Members.Where(m => m.Airline != airliner.Airliner.Airline).SelectMany(m => m.Airline.Routes.FindAll(r => ((r.Destination2 == airportDestination || r.Destination1 == airportDestination) && (r.Destination1 != airportCurrent && r.Destination2 != airportCurrent)))));
                var allianceRoutesToOrigin = airliner.Airliner.Airline.Alliances.SelectMany(a => a.Members.Where(m => m.Airline != airliner.Airliner.Airline).SelectMany(m => m.Airline.Routes.FindAll(r => ((r.Destination1 == airportCurrent || r.Destination2 == airportCurrent) && (r.Destination2 != airportDestination && r.Destination1 != airportDestination)))));

                foreach (PassengerRoute route in allianceRoutesFromDestination)
                {
                    Airport tDest = route.Destination1 == airportDestination ? route.Destination2 : route.Destination1;

                    double totalDistance = MathHelpers.GetDistance(airportCurrent, tDest);

                    int directRoutes = AirportHelpers.GetAirportRoutes(airportCurrent, tDest).Count;

                    if (route.getDistance() + legDistance < totalDistance * 3 && directRoutes < 2)
                    {
                        double demand = (double)airportCurrent.getDestinationPassengersRate(tDest, type);
                        demandDestination += demand;
                    }
                }

                foreach (PassengerRoute route in allianceRoutesToOrigin)
                {
                    Airport tDest = route.Destination1 == airportCurrent ? route.Destination2 : route.Destination1;

                    double totalDistance = MathHelpers.GetDistance(tDest, airportDestination);

                    int directRoutes = AirportHelpers.GetAirportRoutes(tDest, airportDestination).Count;


                    if (route.getDistance() + legDistance < totalDistance * 3 && directRoutes < 2)
                    {
                        double demand = (double)tDest.getDestinationPassengersRate(airportDestination, type);
                        demandOrigin += demand;
                    }
                }
            }

            return demandOrigin + demandDestination;



        }
        //returns the number of passengers between two destinations
        public static int GetFlightPassengers(Airport airportCurrent, Airport airportDestination, FleetAirliner airliner, AirlinerClass.ClassType type)
        {

            double distance = MathHelpers.GetDistance(airportCurrent, airportDestination);

            var currentRoute = airliner.Routes.Find(r => r.Stopovers.SelectMany(s => s.Legs).ToList().Exists(l => (l.Destination1 == airportCurrent || l.Destination1 == airportDestination) && (l.Destination2 == airportDestination || l.Destination2 == airportCurrent)) || (r.Destination1 == airportCurrent || r.Destination1 == airportDestination) && (r.Destination2 == airportDestination || r.Destination2 == airportCurrent));

            if (currentRoute == null)
                return 0;

            double basicPrice = GetPassengerPrice(currentRoute.Destination1, currentRoute.Destination2, type);
            double routePrice = ((PassengerRoute)currentRoute).getFarePrice(type);

            double priceDiff = basicPrice / routePrice;

            double demand = (double)airportCurrent.getDestinationPassengersRate(airportDestination, type);

  

            double passengerDemand = (demand + GetFlightConnectionPassengers(airportCurrent, airportDestination, airliner, type) + GetNearbyPassengerDemand(airportCurrent, airportDestination, airliner, type)) * GetSeasonFactor(airportDestination) * GetHolidayFactor(airportDestination) * GetHolidayFactor(airportCurrent);

            passengerDemand *= GameObject.GetInstance().Difficulty.PassengersLevel;

            if (airliner.Airliner.Airline.MarketFocus == Airline.AirlineFocus.Global && distance > 3000 && airportCurrent.Profile.Country != airportDestination.Profile.Country)
                passengerDemand = passengerDemand * (115 / 100);

            if (airliner.Airliner.Airline.MarketFocus == Airline.AirlineFocus.Regional && distance < 1500)
                passengerDemand = passengerDemand * (115 / 100);

            if (airliner.Airliner.Airline.MarketFocus == Airline.AirlineFocus.Domestic && distance < 1500 && airportDestination.Profile.Country == airportCurrent.Profile.Country)
                passengerDemand = passengerDemand * (115 / 100);

            if (airliner.Airliner.Airline.MarketFocus == Airline.AirlineFocus.Local && distance < 1000)
                passengerDemand = passengerDemand * (115 / 100);


            var hub = airportDestination.getHubs().Find(h => h.Airline == airliner.Airliner.Airline);
            
            if (hub != null)
            {
                switch (hub.Type.Type)
                {
                    case HubType.TypeOfHub.Focus_city:
                        if (airportDestination.Profile.Country == airportCurrent.Profile.Country)
                            passengerDemand = passengerDemand * 1.15;
                        break;
                    case HubType.TypeOfHub.Regional_hub:
                        if (airportDestination.Profile.Country.Region == airportCurrent.Profile.Country.Region)
                            passengerDemand = passengerDemand * 1.20;
                        break;
                    case HubType.TypeOfHub.Hub:
                        passengerDemand = passengerDemand * 1.20;
                        break;
                    case HubType.TypeOfHub.Fortress_hub:
                        passengerDemand = passengerDemand * 1.30;
                        break;
                }

            }

            List<Route> routes = new List<Route>();

            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                var aRoutes = new List<Route>(airline.Routes);

                routes.AddRange(aRoutes.Where(r => r.Type == Route.RouteType.Passenger || r.Type == Route.RouteType.Mixed).Where(r => (r.HasAirliner) && (r.Destination1 == airportCurrent || r.Destination1 == airportDestination) && (r.Destination2 == airportDestination || r.Destination2 == airportCurrent)));
                routes.AddRange(aRoutes.Where(r => r.Type == Route.RouteType.Mixed || r.Type == Route.RouteType.Passenger).Where(r => r.Stopovers.SelectMany(s => s.Legs.Where(l => r.HasAirliner && (l.Destination1 == airportCurrent || l.Destination1 == airportDestination) && (l.Destination2 == airportDestination || l.Destination2 == airportCurrent))).Count() > 0));
            }

            double flightsPerDay = Convert.ToDouble(routes.Sum(r => r.TimeTable.Entries.Count)) / 7;

            passengerDemand = passengerDemand / flightsPerDay;

            double totalCapacity = 0;
            if (routes.Count > 0 && routes.Count(r => r.HasAirliner) > 0)
                totalCapacity = routes.Where(r => r.HasAirliner).Sum(r => r.getAirliners().Max(a => a.Airliner.getTotalSeatCapacity()));//SelectMany(r => r.Stopovers.Where(s=>s.Legs.Count >0))).Sum(s=>s.;//a => a.Routes.SelectMany(r=>r.Stopovers.SelectMany(s=>s.Legs.Where(l=>r.HasAirliner && (l.Destination1 == airportCurrent || l.Destination1 == airportDestination) && (l.Destination2 == airportDestination || l.Destination2 == airportCurrent))).Sum(r=>r.getAirliners().Max(a=>a.Airliner.getTotalSeatCapacity())); 
            else
                totalCapacity = 0;// routes.Where(r => r.HasAirliner).Sum(r => r.getAirliners().Max(a => a.Airliner.getTotalSeatCapacity()));

            double capacityPercent = passengerDemand > totalCapacity ? 1 : passengerDemand / totalCapacity;

            Dictionary<Route, double> rations = new Dictionary<Route, double>();

            foreach (Route route in routes)
            {
                double level = ((PassengerRoute)route).getServiceLevel(type) / ((PassengerRoute)route).getFarePrice(type);

                rations.Add(route, level);
            }

            double totalRatio = rations.Values.Sum();

            double routeRatioPercent = 1;

            if (rations.ContainsKey(currentRoute))
                routeRatioPercent = Math.Max(1, rations[currentRoute] / Math.Max(1, totalRatio));

            double routePriceDiff = priceDiff < 0.5 ? priceDiff : 1;

            routePriceDiff *= GameObject.GetInstance().Difficulty.PriceLevel;

            double randomPax = Convert.ToDouble(rnd.Next(97, 103)) / 100;

            int pax = (int)Math.Min(airliner.Airliner.getAirlinerClass(type).SeatingCapacity, (airliner.Airliner.getAirlinerClass(type).SeatingCapacity * routeRatioPercent * capacityPercent * routePriceDiff * randomPax));

            if (pax < 0)
                totalCapacity = 100;

            return pax;
        }
        //returns the number of passengers for a flight
        public static int GetFlightPassengers(FleetAirliner airliner, AirlinerClass.ClassType type)
        {

            Airport airportCurrent = airliner.CurrentFlight.getDepartureAirport();
            Airport airportDestination = airliner.CurrentFlight.Entry.Destination.Airport;

            return GetFlightPassengers(airportCurrent, airportDestination, airliner, type);
        }
        //returns the number of passengers between two airports on a stopover route
        public static int GetStopoverFlightPassengers(FleetAirliner airliner, AirlinerClass.ClassType type, Airport dept, Airport dest, List<Route> routes, Boolean isInbound)
        {
            Route currentRoute = routes.Find(r => (r.Destination1 == dept && r.Destination2 == dest) || (r.Destination2 == dept && r.Destination1 == dest));
            int index = routes.IndexOf(currentRoute);

            int passengers = 0;
            for (int i = 0; i <= index; i++)
            {
                if (isInbound)
                {
                    passengers += GetFlightPassengers(routes[i].Destination2, dest, airliner, type);
                }
                else
                {
                    passengers += GetFlightPassengers(routes[i].Destination1, dest, airliner, type);

                }





            }

            return (int)Math.Min(airliner.Airliner.getAirlinerClass(type).SeatingCapacity, passengers);

        }
        //returns the number of passengers for a flight on a stopover route
        public static int GetStopoverFlightPassengers(FleetAirliner airliner, AirlinerClass.ClassType type)
        {
            RouteTimeTableEntry mainEntry = airliner.CurrentFlight.Entry.MainEntry;
            RouteTimeTableEntry entry = airliner.CurrentFlight.Entry;

            List<Route> legs = mainEntry.TimeTable.Route.Stopovers.SelectMany(s => s.Legs).ToList();

            Boolean isInbound = mainEntry.DepartureAirport == mainEntry.TimeTable.Route.Destination2;

            int passengers;
            //inboound
            if (isInbound)
            {
                legs.Reverse();
                passengers = GetFlightPassengers(mainEntry.TimeTable.Route.Destination2, mainEntry.TimeTable.Route.Destination1, airliner, type);
            }
            else
                passengers = GetFlightPassengers(mainEntry.TimeTable.Route.Destination1, mainEntry.TimeTable.Route.Destination2, airliner, type);

            int index = legs.IndexOf(entry.TimeTable.Route);

            for (int i = index; i < legs.Count; i++)
            {
                if (isInbound)
                    passengers += GetFlightPassengers(entry.TimeTable.Route.Destination1, legs[i].Destination1, airliner, type);
                else
                    passengers += GetFlightPassengers(entry.TimeTable.Route.Destination1, legs[i].Destination2, airliner, type);

            }

            return (int)Math.Min(airliner.Airliner.getAirlinerClass(type).SeatingCapacity, passengers);


        }
        //returns the cargo for a stopover flight
        public static double GetStopoverFlightCargo(FleetAirliner airliner, Airport dept, Airport dest, List<Route> routes, Boolean isInbound)
        {
            Route currentRoute = routes.Find(r => (r.Destination1 == dept && r.Destination2 == dest) || (r.Destination2 == dept && r.Destination1 == dest));
            int index = routes.IndexOf(currentRoute);

            double cargo = 0;
            for (int i = 0; i <= index; i++)
            {
                if (isInbound)
                {
                    cargo += GetFlightCargo(routes[i].Destination2, dest, airliner);
                }
                else
                {
                    cargo += GetFlightCargo(routes[i].Destination1, dest, airliner);

                }



            }

            return Math.Min(airliner.Airliner.getCargoCapacity(), cargo);
        }
        //returns the cargo for a flight
        public static double GetFlightCargo(FleetAirliner airliner)
        {
            Airport airportCurrent = airliner.CurrentFlight.getDepartureAirport();
            Airport airportDestination = airliner.CurrentFlight.Entry.Destination.Airport;

            return GetFlightCargo(airportCurrent, airportDestination, airliner);
        }
        public static double GetFlightCargo(Airport airportCurrent, Airport airportDestination, FleetAirliner airliner)
        {
            double destinationFacilityFactor = airportDestination.getAirportFacility(GameObject.GetInstance().HumanAirline,AirportFacility.FacilityType.Cargo,true).ServiceLevel;
            
            double distance = MathHelpers.GetDistance(airportCurrent, airportDestination);

            double capacity = ((AirlinerCargoType)airliner.Airliner.Type).CargoSize;

            double demand = (double)airportCurrent.getDestinationCargoRate(airportDestination) * (destinationFacilityFactor / 100);

            double cargoDemand = demand * 1000;

            List<Route> routes = new List<Route>();

            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                var aRoutes = new List<Route>(airline.Routes);

                routes.AddRange(aRoutes.Where(r => r.Type == Route.RouteType.Cargo || r.Type == Route.RouteType.Mixed).Where(r => (r.HasAirliner) && (r.Destination1 == airportCurrent || r.Destination1 == airportDestination) && (r.Destination2 == airportDestination || r.Destination2 == airportCurrent)));
                routes.AddRange(aRoutes.Where(r => r.Type == Route.RouteType.Mixed || r.Type == Route.RouteType.Cargo).Where(r => r.Stopovers.SelectMany(s => s.Legs.Where(l => r.HasAirliner && (l.Destination1 == airportCurrent || l.Destination1 == airportDestination) && (l.Destination2 == airportDestination || l.Destination2 == airportCurrent))).Count() > 0));
            }

            double flightsPerDay = Convert.ToDouble(routes.Sum(r => r.TimeTable.Entries.Count)) / 7;

            cargoDemand = cargoDemand / flightsPerDay;

            double totalCapacity = 0;
            if (routes.Count > 0 && routes.Count(r => r.HasAirliner) > 0)
                totalCapacity = routes.Where(r => r.HasAirliner).Sum(r => r.getAirliners().Max(a => ((AirlinerCargoType)a.Airliner.Type).CargoSize));//SelectMany(r => r.Stopovers.Where(s=>s.Legs.Count >0))).Sum(s=>s.;//a => a.Routes.SelectMany(r=>r.Stopovers.SelectMany(s=>s.Legs.Where(l=>r.HasAirliner && (l.Destination1 == airportCurrent || l.Destination1 == airportDestination) && (l.Destination2 == airportDestination || l.Destination2 == airportCurrent))).Sum(r=>r.getAirliners().Max(a=>a.Airliner.getTotalSeatCapacity())); 
            else
                totalCapacity = 0;// routes.Where(r => r.HasAirliner).Sum(r => r.getAirliners().Max(a => a.Airliner.getTotalSeatCapacity()));

            double capacityPercent = cargoDemand > totalCapacity ? 1 : cargoDemand / totalCapacity;

            double randomCargo = Convert.ToDouble(rnd.Next(97, 103)) / 100;

            int cargo = (int)Math.Min(capacity, (capacity * capacityPercent * randomCargo));

            if (cargo < 0)
                totalCapacity = 100;

            return cargo;


        }
        //returns the holiday factor for an airport
        private static double GetHolidayFactor(Airport airport)
        {
            if (HolidayYear.IsHoliday(airport.Profile.Country, GameObject.GetInstance().GameTime))
            {
                HolidayYearEvent holiday = HolidayYear.GetHoliday(airport.Profile.Country, GameObject.GetInstance().GameTime);

                if (holiday.Holiday.Travel == Holiday.TravelType.Both || holiday.Holiday.Travel == Holiday.TravelType.Travel)
                    return 150 / 100;
            }
            return 1;
        }
        //returns the season factor for an airport
        private static double GetSeasonFactor(Airport airport)
        {
            Boolean isSummer = GameObject.GetInstance().GameTime.Month >= 3 && GameObject.GetInstance().GameTime.Month < 9;

            if (airport.Profile.Season == Weather.Season.All_Year)
                return 1;
            if (airport.Profile.Season == Weather.Season.Summer)
                if (isSummer) return 150 / 100;
                else return 50 / 100;
            if (airport.Profile.Season == Weather.Season.Winter)
                if (isSummer) return 50 / 100;
                else return 150 / 100;

            return 1;
        }
        //returns the suggested cargo price for a route
        public static double GetCargoPrice(Airport dest1, Airport dest2)
        {
            double dist = MathHelpers.GetDistance(dest1, dest2);

            double ticketPrice = dist * GeneralHelpers.GetInflationPrice(0.0078);

            return ticketPrice * 2;

        }
        //returns the suggested passenger price for a route
        public static double GetPassengerPrice(Airport dest1, Airport dest2)
        {
            double dist = MathHelpers.GetDistance(dest1, dest2);

            double ticketPrice = dist * GeneralHelpers.GetInflationPrice(0.0078);

            double minimumTicketPrice = GeneralHelpers.GetInflationPrice(18);

            if (ticketPrice < minimumTicketPrice)
                ticketPrice = minimumTicketPrice + (ticketPrice / 4);

            return ticketPrice;
        }
        public static double GetPassengerPrice(Airport dest1, Airport dest2, AirlinerClass.ClassType type)
        {
            return GetPassengerPrice(dest1, dest2) * GeneralHelpers.ClassToPriceFactor(type);
        }
        //creates the random airport destination for a list of destinations
        private static void CreateDestinationPassengers(Airport airport, List<Airport> subAirports)
        {
            var largestAirports = subAirports.FindAll(a => a.Profile.Size == GeneralHelpers.Size.Largest || a.Profile.Size == GeneralHelpers.Size.Very_large);

            int maxValue = Math.Max(2, (int)Math.Ceiling(airport.Profile.Pax / 2));

            if (largestAirports.Count > 0)
            {
                foreach (var lAirport in largestAirports)
                    airport.addDestinationPassengersRate(new DestinationDemand(lAirport, (ushort)rnd.Next(1, maxValue)));

            }
            else
            {
                subAirports = subAirports.OrderByDescending(a => a.Profile.Size).ToList();
                airport.addDestinationPassengersRate(new DestinationDemand(subAirports[0], (ushort)rnd.Next(1, maxValue)));
            }


        }
         //creates the airport destination passengers a destination
        public static void CreateDestinationPassengers(Airport airport)
        {
            var airports = Airports.GetAirports(a => a != airport && a.Profile.Town != airport.Profile.Town && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) > 50);
            //Parallel.ForEach(airports, dAirport =>
            foreach (Airport dAirport in airports)
            {
                CreateDestinationPassengers(airport, dAirport);
            }//);
            if (airport.getDestinationsPassengers().Sum(d => d.Rate) == 0)
            {
                var subAirports = airports.FindAll(a => a.Profile.Country == airport.Profile.Country).DefaultIfEmpty().ToList();
                subAirports.RemoveAll(a => a == null);
                if (subAirports != null && subAirports.Count() > 0)
                {
                    CreateDestinationPassengers(airport, subAirports);
                }
                else
                {
                    subAirports = airports.FindAll(a => a.Profile.Country.Region == airport.Profile.Country.Region).ToList();
                    CreateDestinationPassengers(airport, subAirports);
                }
            }
        }
        //creates the airport destinations demand for all destination served by an airline
        public static void CreateAirlineDestinationDemand()
        {
            //sets up the factors if they are not already setted up

            if (CargoFactors.Count == 0)
            {

                CargoFactors.Add(GeneralHelpers.Size.Largest, 0.23);
                CargoFactors.Add(GeneralHelpers.Size.Very_large, 0.23);
                CargoFactors.Add(GeneralHelpers.Size.Large, 0.23);
                CargoFactors.Add(GeneralHelpers.Size.Medium, 0.23);
                CargoFactors.Add(GeneralHelpers.Size.Small, 0.23);
                CargoFactors.Add(GeneralHelpers.Size.Very_small, 0.23);
                CargoFactors.Add(GeneralHelpers.Size.Smallest, 0.23);


            }

            var airports = Airlines.GetAllAirlines().SelectMany(a => a.Airports);

            foreach (Airport airport in airports)
            {
                Parallel.ForEach(Airports.GetAllAirports(), dAirport =>
                    {
                        airport.Statics.addDistance(dAirport, MathHelpers.GetDistance(airport, dAirport));

                        if (airport != dAirport && airport.Profile.Town != dAirport.Profile.Town && MathHelpers.GetDistance(airport, dAirport) > 50)
                        {
                            CreateDestinationPassengers(airport, dAirport);

                            CreateDestinationCargo(airport, dAirport);
                        }
                    });
                if (airport.getDestinationPassengersSum() == 0)
                {
                    var subAirports = Airports.GetAllAirports(a => a.Profile.Country == airport.Profile.Country).DefaultIfEmpty().ToList();
                    subAirports.RemoveAll(a => a == null);

                    if (subAirports != null && subAirports.Count() > 0)
                    {

                        CreateDestinationPassengers(airport, subAirports);
                    }
                    else
                    {

                        subAirports = Airports.GetAllAirports(a => a.Profile.Country.Region == airport.Profile.Country.Region).ToList();
                        CreateDestinationPassengers(airport, subAirports);
                    }


                }
            }


        }
        //creates the airport destinations passenger for all destinations
        public static void CreateDestinationDemand()
        {
            if (CargoFactors.Count == 0)
            {

                CargoFactors.Add(GeneralHelpers.Size.Largest, 0.23);
                CargoFactors.Add(GeneralHelpers.Size.Very_large, 0.23);
                CargoFactors.Add(GeneralHelpers.Size.Large, 0.23);
                CargoFactors.Add(GeneralHelpers.Size.Medium, 0.23);
                CargoFactors.Add(GeneralHelpers.Size.Small, 0.23);
                CargoFactors.Add(GeneralHelpers.Size.Very_small, 0.23);
                CargoFactors.Add(GeneralHelpers.Size.Smallest, 0.23);


            }
            var airports = Airports.GetAllAirports(a => a.Statics.getDestinationPassengersSum() == 0);
            int count = airports.Count;

            //var airports = Airports.GetAirports(a => a != airport && a.Profile.Town != airport.Profile.Town && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) > 50);

            Parallel.For(0, count - 1, i =>
                {
                    Parallel.For(i + 1, count, j =>
                        {
                            if (airports[i].Profile.Town != airports[j].Profile.Town && MathHelpers.GetDistance(airports[i], airports[j]) > 50)
                            {
                                airports[j].Statics.addDistance(airports[i], MathHelpers.GetDistance(airports[j], airports[i]));

                                CreateDestinationPassengers(airports[j], airports[i]);
                                CreateDestinationPassengers(airports[i], airports[j]);

                                CreateDestinationCargo(airports[j], airports[i]);
                                CreateDestinationCargo(airports[i], airports[j]);
                            }
                        });

                    if (airports[i].getDestinationPassengersSum() == 0)
                    {
                        var subAirports = airports.FindAll(a => a.Profile.Country == airports[i].Profile.Country).DefaultIfEmpty().ToList();
                        subAirports.RemoveAll(a => a == null);

                        if (subAirports != null && subAirports.Count() > 0)
                        {

                            CreateDestinationPassengers(airports[i], subAirports);
                        }
                        else
                        {

                            subAirports = airports.FindAll(a => a.Profile.Country.Region == airports[i].Profile.Country.Region).ToList();
                            CreateDestinationPassengers(airports[i], subAirports);
                        }


                    }
                });

        }
        //returns the cargo mass per day for an airport
        private static double GetAirportCargoMass(Airport airport)
        {
            //density cargo 3000 kg/cu m.
            //1 cu m. vejer 3000 kg. eller 3.0 metric ton 

            /*--------------------READ THE TWO LINES BELOW --- VERY USEFUL ----------------*/

            //for reference, most commercial aircraft have capacity of about 5.5(5.3-5.8)lb per cu ft
            //to convert values to cu meters, multiply the cargo cu footage * 35.3 (194lb/m3 and 88kg/m3 at standard density)

            //total cargo volume from/to airport in tonnes
            double totalCargoVolume = airport.Profile.CargoVolume == 0 ? ((int)airport.Profile.Cargo + 1) * 10000 : airport.Profile.CargoVolume * 1000; //in metric ton
            double cargoDensity = 3000; //kg/cu m

            //in cargo mass
            double cargoMass = totalCargoVolume / (cargoDensity / 1000);

            double cargoMassPerDay = cargoMass / 365;

            return cargoMassPerDay;
        }

        //sets the cargo sizes based on the volume
        public static void SetCargoSize()
        {
            List<Airport> airports = new List<Airport>();
            airports = Airports.GetAllActiveAirports();

            foreach (Airport airport in airports)
            {
                int volume = (int)airport.Profile.CargoVolume;
                if (volume > 4500) {
                    airport.Profile.Cargo = GeneralHelpers.Size.Largest;
                }
                else if (volume > 2500)
                {
                    airport.Profile.Cargo = GeneralHelpers.Size.Very_large;
                }
                else if (volume < 2500)
                {
                    airport.Profile.Cargo = GeneralHelpers.Size.Large;
                }
                else if (volume < 1500)
                {
                    airport.Profile.Cargo = GeneralHelpers.Size.Medium;
                }
                else if (volume < 750)
                {
                    airport.Profile.Cargo = GeneralHelpers.Size.Small;
                }
                else if (volume < 400)
                {
                    airport.Profile.Cargo = GeneralHelpers.Size.Very_small;
                }
                else if (volume < 200)
                {
                    airport.Profile.Cargo = GeneralHelpers.Size.Smallest;
                }
            }
        }
        //creates the airport destinations cargo between two destinations in cu m.
        public static void CreateDestinationCargo(Airport airport, Airport dAirport)
        {
            //origin airport out
            double originMassPerDay = GetAirportCargoMass(airport) / 2;
            double originDemand = (originMassPerDay * CargoFactors[dAirport.Profile.Cargo]) / Airports.CargoAirportsSizes[dAirport.Profile.Cargo];
            //destination airport in
            double destinationMassPerDay = GetAirportCargoMass(dAirport) / 2;
            double destinationDemand = (destinationMassPerDay * CargoFactors[airport.Profile.Cargo]) / Airports.CargoAirportsSizes[airport.Profile.Cargo];

            double distance = MathHelpers.GetDistance(airport, dAirport);

            double distanceFactor = distance > 5000 ? 1 : 0.5;
            double sameCountryFactor = airport.Profile.Country == dAirport.Profile.Country ? 0.75 : 1;
            double sameRegionFactor = sameCountryFactor == 1 && airport.Profile.Country.Region == dAirport.Profile.Country.Region ? 0.5 : 1;


            double minMass = Math.Min(originMassPerDay, destinationMassPerDay);

            double volume = minMass / distanceFactor;
            volume = volume / sameCountryFactor;
            volume = volume / sameRegionFactor;
            //converts to pounds of cargo
            volume *= 35.1;

            if (volume >= 1)
                airport.addDestinationCargoRate(new DestinationDemand(dAirport, (ushort)volume));

        }
        //creates the airport destinations passengers between two destinations 
        public static void CreateDestinationPassengers(Airport airport, Airport dAirport)
        {
            Array values = Enum.GetValues(typeof(GeneralHelpers.Size));

            double estimatedPassengerLevel = 0;
            Boolean isSameCountry = airport.Profile.Country == dAirport.Profile.Country;
            Boolean isSameContinent = airport.Profile.Country.Region == dAirport.Profile.Country.Region && !isSameCountry;

            String dAirportSize = dAirport.Profile.Size.ToString();
            String airportSize = airport.Profile.Size.ToString();
            double dist = MathHelpers.GetDistance(dAirport, airport);


            if (airport.Profile.MajorDestionations.Keys.Contains(dAirport.Profile.IATACode))
            {
                estimatedPassengerLevel = (Convert.ToDouble(airport.Profile.MajorDestionations[dAirport.Profile.IATACode]) * 1000) / 365;
                estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
            }
            else
            {
                switch (airportSize)
                {
                    #region Origin"Largest" switches
                    case "Largest":
                        switch (dAirportSize)
                        {
                            case "Largest":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxLargest = airport.Profile.Pax * 0.21 / Airports.LargestAirports;
                                    paxLargest *= MathHelpers.GetRandomDoubleNumber(0.9, 1.11);
                                    estimatedPassengerLevel = (paxLargest * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                                }
                                else
                                {
                                    double paxLargest = 40000 * 0.21 / Airports.LargestAirports;
                                    paxLargest *= MathHelpers.GetRandomDoubleNumber(0.9, 1.11);
                                    estimatedPassengerLevel = (paxLargest * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                                }
                                break;
                            case "Very_large":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxVeryLarge = airport.Profile.Pax * 0.24 / Airports.VeryLargeAirports;
                                    paxVeryLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.12);
                                    estimatedPassengerLevel = paxVeryLarge * 1000 / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxVeryLarge = 40000 * 0.24 / Airports.VeryLargeAirports;
                                    paxVeryLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.12);
                                    estimatedPassengerLevel = (paxVeryLarge * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                break;
                            case "Large":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxLarge = airport.Profile.Pax * 0.24 / Airports.LargeAirports;
                                    paxLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.14);
                                    estimatedPassengerLevel = (paxLarge * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxLarge = 40000 * 0.24 / Airports.LargeAirports;
                                    paxLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.14);
                                    estimatedPassengerLevel = (paxLarge * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                break;
                            case "Medium":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxMedium = airport.Profile.Pax * 0.15 / Airports.MediumAirports;
                                    paxMedium *= MathHelpers.GetRandomDoubleNumber(0.9, 1.16);
                                    estimatedPassengerLevel = (paxMedium * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxMedium = 40000 * 0.15 / Airports.MediumAirports;
                                    paxMedium *= MathHelpers.GetRandomDoubleNumber(0.9, 1.16);
                                    estimatedPassengerLevel = (paxMedium * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                break;
                            case "Small":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxSmall = airport.Profile.Pax * 0.10 / Airports.SmallAirports;
                                    paxSmall *= MathHelpers.GetRandomDoubleNumber(0.95, 1.10);
                                    estimatedPassengerLevel = (paxSmall * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxSmall = 40000 * 0.10 / Airports.SmallAirports;
                                    paxSmall *= MathHelpers.GetRandomDoubleNumber(0.95, 1.10);
                                    estimatedPassengerLevel = (paxSmall * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                break;
                            case "Very_small":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxVery_small = airport.Profile.Pax * 0.04 / Airports.VerySmallAirports;
                                    paxVery_small *= MathHelpers.GetRandomDoubleNumber(0.97, 1.06);
                                    estimatedPassengerLevel = (paxVery_small * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxVery_small = 40000 * 0.04 / Airports.VerySmallAirports;
                                    paxVery_small *= MathHelpers.GetRandomDoubleNumber(0.97, 1.06);
                                    estimatedPassengerLevel = (paxVery_small * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                break;
                            case "Smallest":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxSmallest = airport.Profile.Pax * 0.02 / Airports.SmallestAirports;
                                    paxSmallest *= MathHelpers.GetRandomDoubleNumber(0.98, 1.04);
                                    estimatedPassengerLevel = (paxSmallest * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxSmallest = 40000 * 0.02 / Airports.SmallestAirports;
                                    paxSmallest *= MathHelpers.GetRandomDoubleNumber(0.98, 1.04);
                                    estimatedPassengerLevel = (paxSmallest * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                break;
                        }
                        break;
                    #endregion
                    #region Origin "Very_large" switches
                    case "Very_large":
                        switch (dAirportSize)
                        {
                            case "Largest":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxLargest = airport.Profile.Pax * 0.21 / Airports.LargestAirports;
                                    paxLargest *= MathHelpers.GetRandomDoubleNumber(0.9, 1.11);
                                    estimatedPassengerLevel = paxLargest * 1000 / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxLargest = 20000 * 0.21 / Airports.LargestAirports;
                                    paxLargest *= MathHelpers.GetRandomDoubleNumber(0.9, 1.11);
                                    estimatedPassengerLevel = (paxLargest * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                                }
                                break;

                            case "Very_large":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxVeryLarge = airport.Profile.Pax * 0.24 / Airports.VeryLargeAirports;
                                    paxVeryLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.12);
                                    estimatedPassengerLevel = paxVeryLarge * 1000 / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxVeryLarge = 20000 * 0.24 / Airports.VeryLargeAirports;
                                    paxVeryLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.12);
                                    estimatedPassengerLevel = (paxVeryLarge * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                break;

                            case "Large":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxLarge = airport.Profile.Pax * 0.24 / Airports.LargeAirports;
                                    paxLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.14);
                                    estimatedPassengerLevel = (paxLarge * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxLarge = 20000 * 0.24 / Airports.LargeAirports;
                                    paxLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.14);
                                    estimatedPassengerLevel = (paxLarge * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                break;

                            case "Medium":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxMedium = airport.Profile.Pax * 0.15 / Airports.MediumAirports;
                                    paxMedium *= MathHelpers.GetRandomDoubleNumber(0.9, 1.16);
                                    estimatedPassengerLevel = (paxMedium * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxMedium = 20000 * 0.15 / Airports.MediumAirports;
                                    paxMedium *= MathHelpers.GetRandomDoubleNumber(0.9, 1.16);
                                    estimatedPassengerLevel = (paxMedium * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                break;

                            case "Small":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxSmall = airport.Profile.Pax * 0.10 / Airports.SmallAirports;
                                    paxSmall *= MathHelpers.GetRandomDoubleNumber(0.95, 1.10);
                                    estimatedPassengerLevel = (paxSmall * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxSmall = 20000 * 0.10 / Airports.SmallAirports;
                                    paxSmall *= MathHelpers.GetRandomDoubleNumber(0.95, 1.10);
                                    estimatedPassengerLevel = (paxSmall * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                break;

                            case "Very_small":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxVery_small = airport.Profile.Pax * 0.04 / Airports.VerySmallAirports;
                                    paxVery_small *= MathHelpers.GetRandomDoubleNumber(0.97, 1.06);
                                    estimatedPassengerLevel = (paxVery_small * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxVery_small = 20000 * 0.04 / Airports.VerySmallAirports;
                                    paxVery_small *= MathHelpers.GetRandomDoubleNumber(0.97, 1.06);
                                    estimatedPassengerLevel = (paxVery_small * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                break;

                            case "Smallest":

                                if (airport.Profile.Pax > 0)
                                {
                                    double paxSmallest = airport.Profile.Pax * 0.02 / Airports.SmallestAirports;
                                    paxSmallest *= MathHelpers.GetRandomDoubleNumber(0.98, 1.04);
                                    estimatedPassengerLevel = (paxSmallest * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxSmallest = 20000 * 0.02 / Airports.SmallestAirports;
                                    paxSmallest *= MathHelpers.GetRandomDoubleNumber(0.98, 1.04);
                                    estimatedPassengerLevel = (paxSmallest * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                break;
                        }
                        break;
                    #endregion
                    #region Origin "Large" switches
                    case "Large":
                        switch (dAirportSize)
                        {
                            case "Largest":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxLargest = airport.Profile.Pax * 0.21 / Airports.LargestAirports;
                                    paxLargest *= MathHelpers.GetRandomDoubleNumber(0.9, 1.11);
                                    estimatedPassengerLevel = paxLargest * 1000 / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxLargest = 10000 * 0.21 / Airports.LargestAirports;
                                    paxLargest *= MathHelpers.GetRandomDoubleNumber(0.9, 1.11);
                                    estimatedPassengerLevel = (paxLargest * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                                }
                                break;

                            case "Very_large":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxVeryLarge = airport.Profile.Pax * 0.24 / Airports.VeryLargeAirports;
                                    paxVeryLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.12);
                                    estimatedPassengerLevel = paxVeryLarge * 1000 / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxVeryLarge = 10000 * 0.24 / Airports.VeryLargeAirports;
                                    paxVeryLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.12);
                                    estimatedPassengerLevel = (paxVeryLarge * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                break;

                            case "Large":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxLarge = airport.Profile.Pax * 0.24 / Airports.LargeAirports;
                                    paxLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.14);
                                    estimatedPassengerLevel = (paxLarge * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxLarge = 10000 * 0.24 / Airports.LargeAirports;
                                    paxLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.14);
                                    estimatedPassengerLevel = (paxLarge * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                break;

                            case "Medium":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxMedium = airport.Profile.Pax * 0.15 / Airports.MediumAirports;
                                    paxMedium *= MathHelpers.GetRandomDoubleNumber(0.9, 1.16);
                                    estimatedPassengerLevel = (paxMedium * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxMedium = 10000 * 0.15 / Airports.MediumAirports;
                                    paxMedium *= MathHelpers.GetRandomDoubleNumber(0.9, 1.16);
                                    estimatedPassengerLevel = (paxMedium * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                break;

                            case "Small":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxSmall = airport.Profile.Pax * 0.10 / Airports.SmallAirports;
                                    paxSmall *= MathHelpers.GetRandomDoubleNumber(0.95, 1.10);
                                    estimatedPassengerLevel = (paxSmall * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxSmall = 10000 * 0.10 / Airports.SmallAirports;
                                    paxSmall *= MathHelpers.GetRandomDoubleNumber(0.95, 1.10);
                                    estimatedPassengerLevel = (paxSmall * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                break;

                            case "Very_small":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxVery_small = airport.Profile.Pax * 0.04 / Airports.VerySmallAirports;
                                    paxVery_small *= MathHelpers.GetRandomDoubleNumber(0.97, 1.06);
                                    estimatedPassengerLevel = (paxVery_small * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxVery_small = 10000 * 0.04 / Airports.VerySmallAirports;
                                    paxVery_small *= MathHelpers.GetRandomDoubleNumber(0.97, 1.06);
                                    estimatedPassengerLevel = (paxVery_small * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                break;

                            case "Smallest":

                                if (airport.Profile.Pax > 0)
                                {
                                    double paxSmallest = airport.Profile.Pax * 0.02 / Airports.SmallestAirports;
                                    paxSmallest *= MathHelpers.GetRandomDoubleNumber(0.98, 1.04);
                                    estimatedPassengerLevel = (paxSmallest * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxSmallest = 10000 * 0.02 / Airports.SmallestAirports;
                                    paxSmallest *= MathHelpers.GetRandomDoubleNumber(0.98, 1.04);
                                    estimatedPassengerLevel = (paxSmallest * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                break;
                        }
                        break;
                    #endregion
                    #region Origin "Medium" switches
                    case "Medium":
                        switch (dAirportSize)
                        {
                            case "Largest":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxLargest = airport.Profile.Pax * 0.21 / Airports.LargestAirports;
                                    paxLargest *= MathHelpers.GetRandomDoubleNumber(0.9, 1.11);
                                    estimatedPassengerLevel = paxLargest * 1000 / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxLargest = 6000 * 0.21 / Airports.LargestAirports;
                                    paxLargest *= MathHelpers.GetRandomDoubleNumber(0.9, 1.11);
                                    estimatedPassengerLevel = (paxLargest * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                                }
                                break;

                            case "Very_large":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxVeryLarge = airport.Profile.Pax * 0.24 / Airports.VeryLargeAirports;
                                    paxVeryLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.12);
                                    estimatedPassengerLevel = paxVeryLarge * 1000 / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxVeryLarge = 6000 * 0.24 / Airports.VeryLargeAirports;
                                    paxVeryLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.12);
                                    estimatedPassengerLevel = (paxVeryLarge * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                break;

                            case "Large":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxLarge = airport.Profile.Pax * 0.24 / Airports.LargeAirports;
                                    paxLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.14);
                                    estimatedPassengerLevel = (paxLarge * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxLarge = 6000 * 0.24 / Airports.LargeAirports;
                                    paxLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.14);
                                    estimatedPassengerLevel = (paxLarge * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                break;

                            case "Medium":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxMedium = airport.Profile.Pax * 0.15 / Airports.MediumAirports;
                                    paxMedium *= MathHelpers.GetRandomDoubleNumber(0.9, 1.16);
                                    estimatedPassengerLevel = (paxMedium * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxMedium = 6000 * 0.15 / Airports.MediumAirports;
                                    paxMedium *= MathHelpers.GetRandomDoubleNumber(0.9, 1.16);
                                    estimatedPassengerLevel = (paxMedium * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                break;

                            case "Small":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxSmall = airport.Profile.Pax * 0.10 / Airports.SmallAirports;
                                    paxSmall *= MathHelpers.GetRandomDoubleNumber(0.95, 1.10);
                                    estimatedPassengerLevel = (paxSmall * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxSmall = 6000 * 0.10 / Airports.SmallAirports;
                                    paxSmall *= MathHelpers.GetRandomDoubleNumber(0.95, 1.10);
                                    estimatedPassengerLevel = (paxSmall * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                break;

                            case "Very_small":

                                if (airport.Profile.Pax > 0)
                                {
                                    double paxVery_small = airport.Profile.Pax * 0.04 / Airports.VerySmallAirports;
                                    paxVery_small *= MathHelpers.GetRandomDoubleNumber(0.97, 1.06);
                                    estimatedPassengerLevel = (paxVery_small * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxVery_small = 6000 * 0.04 / Airports.VerySmallAirports;
                                    paxVery_small *= MathHelpers.GetRandomDoubleNumber(0.97, 1.06);
                                    estimatedPassengerLevel = (paxVery_small * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                break;

                            case "Smallest":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxSmallest = airport.Profile.Pax * 0.02 / Airports.SmallestAirports;
                                    paxSmallest *= MathHelpers.GetRandomDoubleNumber(0.98, 1.04);
                                    estimatedPassengerLevel = (paxSmallest * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxSmallest = 6000 * 0.02 / Airports.SmallestAirports;
                                    paxSmallest *= MathHelpers.GetRandomDoubleNumber(0.98, 1.04);
                                    estimatedPassengerLevel = (paxSmallest * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                break;
                        }
                        break;
                    #endregion
                    #region Origin "Small" switches
                    case "Small":
                        switch (dAirportSize)
                        {
                            case "Largest":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxLargest = airport.Profile.Pax * 0.21 / Airports.LargestAirports;
                                    paxLargest *= MathHelpers.GetRandomDoubleNumber(0.9, 1.11);
                                    estimatedPassengerLevel = paxLargest * 1000 / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxLargest = 1250 * 0.21 / Airports.LargestAirports;
                                    paxLargest *= MathHelpers.GetRandomDoubleNumber(0.9, 1.11);
                                    estimatedPassengerLevel = (paxLargest * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                                }
                                break;

                            case "Very_large":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxVeryLarge = airport.Profile.Pax * 0.24 / Airports.VeryLargeAirports;
                                    paxVeryLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.12);
                                    estimatedPassengerLevel = paxVeryLarge * 1000 / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxVeryLarge = 1250 * 0.24 / Airports.VeryLargeAirports;
                                    paxVeryLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.12);
                                    estimatedPassengerLevel = (paxVeryLarge * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                break;

                            case "Large":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxLarge = airport.Profile.Pax * 0.24 / Airports.LargeAirports;
                                    paxLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.14);
                                    estimatedPassengerLevel = (paxLarge * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxLarge = 1250 * 0.24 / Airports.LargeAirports;
                                    paxLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.14);
                                    estimatedPassengerLevel = (paxLarge * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                break;

                            case "Medium":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxMedium = airport.Profile.Pax * 0.15 / Airports.MediumAirports;
                                    paxMedium *= MathHelpers.GetRandomDoubleNumber(0.9, 1.16);
                                    estimatedPassengerLevel = (paxMedium * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxMedium = 1250 * 0.15 / Airports.MediumAirports;
                                    paxMedium *= MathHelpers.GetRandomDoubleNumber(0.9, 1.16);
                                    estimatedPassengerLevel = (paxMedium * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                break;

                            case "Small":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxSmall = airport.Profile.Pax * 0.10 / Airports.SmallAirports;
                                    paxSmall *= MathHelpers.GetRandomDoubleNumber(0.95, 1.10);
                                    estimatedPassengerLevel = (paxSmall * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxSmall = 1250 * 0.10 / Airports.SmallAirports;
                                    paxSmall *= MathHelpers.GetRandomDoubleNumber(0.95, 1.10);
                                    estimatedPassengerLevel = (paxSmall * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                break;

                            case "Very_small":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxVery_small = airport.Profile.Pax * 0.04 / Airports.VerySmallAirports;
                                    paxVery_small *= MathHelpers.GetRandomDoubleNumber(0.97, 1.06);
                                    estimatedPassengerLevel = (paxVery_small * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxVery_small = 1250 * 0.04 / Airports.VerySmallAirports;
                                    paxVery_small *= MathHelpers.GetRandomDoubleNumber(0.97, 1.06);
                                    estimatedPassengerLevel = (paxVery_small * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                break;

                            case "Smallest":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxSmallest = airport.Profile.Pax * 0.02 / Airports.SmallestAirports;
                                    paxSmallest *= MathHelpers.GetRandomDoubleNumber(0.98, 1.04);
                                    estimatedPassengerLevel = (paxSmallest * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxSmallest = 1250 * 0.02 / Airports.SmallestAirports;
                                    paxSmallest *= MathHelpers.GetRandomDoubleNumber(0.98, 1.04);
                                    estimatedPassengerLevel = (paxSmallest * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                break;
                        }
                        break;
                    #endregion
                    #region Origin "Very_small" switches
                    case "Very_small":
                        switch (dAirportSize)
                        {
                            case "Largest":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxLargest = airport.Profile.Pax * 0.21 / Airports.LargestAirports;
                                    paxLargest *= MathHelpers.GetRandomDoubleNumber(0.9, 1.11);
                                    estimatedPassengerLevel = paxLargest * 1000 / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxLargest = 225 * 0.21 / Airports.LargestAirports;
                                    paxLargest *= MathHelpers.GetRandomDoubleNumber(0.9, 1.11);
                                    estimatedPassengerLevel = (paxLargest * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                                }
                                break;

                            case "Very_large":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxVeryLarge = airport.Profile.Pax * 0.24 / Airports.VeryLargeAirports;
                                    paxVeryLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.12);
                                    estimatedPassengerLevel = paxVeryLarge * 1000 / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxVeryLarge = 225 * 0.24 / Airports.VeryLargeAirports;
                                    paxVeryLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.12);
                                    estimatedPassengerLevel = (paxVeryLarge * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                break;

                            case "Large":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxLarge = airport.Profile.Pax * 0.24 / Airports.LargeAirports;
                                    paxLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.14);
                                    estimatedPassengerLevel = (paxLarge * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxLarge = 225 * 0.24 / Airports.LargeAirports;
                                    paxLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.14);
                                    estimatedPassengerLevel = (paxLarge * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                break;

                            case "Medium":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxMedium = airport.Profile.Pax * 0.15 / Airports.MediumAirports;
                                    paxMedium *= MathHelpers.GetRandomDoubleNumber(0.9, 1.16);
                                    estimatedPassengerLevel = (paxMedium * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxMedium = 225 * 0.15 / Airports.MediumAirports;
                                    paxMedium *= MathHelpers.GetRandomDoubleNumber(0.9, 1.16);
                                    estimatedPassengerLevel = (paxMedium * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                break;

                            case "Small":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxSmall = airport.Profile.Pax * 0.10 / Airports.SmallAirports;
                                    paxSmall *= MathHelpers.GetRandomDoubleNumber(0.95, 1.10);
                                    estimatedPassengerLevel = (paxSmall * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxSmall = 225 * 0.10 / Airports.SmallAirports;
                                    paxSmall *= MathHelpers.GetRandomDoubleNumber(0.95, 1.10);
                                    estimatedPassengerLevel = (paxSmall * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                break;

                            case "Very_small":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxVery_small = airport.Profile.Pax * 0.04 / Airports.VerySmallAirports;
                                    paxVery_small *= MathHelpers.GetRandomDoubleNumber(0.97, 1.06);
                                    estimatedPassengerLevel = (paxVery_small * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxVery_small = 225 * 0.04 / Airports.VerySmallAirports;
                                    paxVery_small *= MathHelpers.GetRandomDoubleNumber(0.97, 1.06);
                                    estimatedPassengerLevel = (paxVery_small * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                break;

                            case "Smallest":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxSmallest = airport.Profile.Pax * 0.02 / Airports.SmallestAirports;
                                    paxSmallest *= MathHelpers.GetRandomDoubleNumber(0.98, 1.04);
                                    estimatedPassengerLevel = (paxSmallest * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxSmallest = 225 * 0.02 / Airports.SmallestAirports;
                                    paxSmallest *= MathHelpers.GetRandomDoubleNumber(0.98, 1.04);
                                    estimatedPassengerLevel = (paxSmallest * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                break;
                        }
                        break;
                    #endregion
                    #region Origin "Smallest" switches
                    case "Smallest":
                        switch (dAirportSize)
                        {
                            case "Largest":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxLargest = airport.Profile.Pax * 0.25 / Airports.LargestAirports;
                                    paxLargest *= MathHelpers.GetRandomDoubleNumber(0.9, 1.11);
                                    estimatedPassengerLevel = paxLargest * 1000 / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                                }
                                else
                                {
                                    double paxLargest = 50 * 0.25 / Airports.LargestAirports;
                                    paxLargest *= MathHelpers.GetRandomDoubleNumber(0.9, 1.11);
                                    estimatedPassengerLevel = (paxLargest * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }

                                break;

                            case "Very_large":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxVeryLarge = airport.Profile.Pax * 0.32 / Airports.VeryLargeAirports;
                                    paxVeryLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.12);
                                    estimatedPassengerLevel = paxVeryLarge * 1000 / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                                }
                                else
                                {
                                    double paxVeryLarge = 50 * 0.32 / Airports.VeryLargeAirports;
                                    paxVeryLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.12);
                                    estimatedPassengerLevel = (paxVeryLarge * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }

                                break;

                            case "Large":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxLarge = airport.Profile.Pax * 0.32 / Airports.LargeAirports;
                                    paxLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.14);
                                    estimatedPassengerLevel = (paxLarge * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                else
                                {
                                    double paxLarge = 50 * 0.32 / Airports.LargeAirports;
                                    paxLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.14);
                                    estimatedPassengerLevel = (paxLarge * 1000) / 365;

                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;

                                }
                                break;

                            case "Medium":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxMedium = airport.Profile.Pax * 0.09 / Airports.MediumAirports;
                                    paxMedium *= MathHelpers.GetRandomDoubleNumber(0.9, 1.16);
                                    estimatedPassengerLevel = (paxMedium * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                                    estimatedPassengerLevel *= 2;

                                }
                                else
                                {
                                    double paxMedium = 50 * 0.09 / Airports.MediumAirports;
                                    paxMedium *= MathHelpers.GetRandomDoubleNumber(0.9, 1.16);
                                    estimatedPassengerLevel = (paxMedium * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                                    estimatedPassengerLevel *= 2;
                                }
                                break;

                            case "Small":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxSmall = airport.Profile.Pax * 0.02 / Airports.SmallAirports;
                                    paxSmall *= MathHelpers.GetRandomDoubleNumber(0.95, 1.10);
                                    estimatedPassengerLevel = (paxSmall * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                                    estimatedPassengerLevel *= 1.2;
                                }
                                else
                                {
                                    double paxSmall = 50 * 0.02 / Airports.SmallAirports;
                                    paxSmall *= MathHelpers.GetRandomDoubleNumber(0.95, 1.10);
                                    estimatedPassengerLevel = (paxSmall * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                                    estimatedPassengerLevel *= 1.2;
                                }
                                break;

                            case "Very_small":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxVery_small = airport.Profile.Pax * 0 / Airports.VerySmallAirports;
                                    paxVery_small *= MathHelpers.GetRandomDoubleNumber(0.97, 1.06);
                                    estimatedPassengerLevel = (paxVery_small * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                                }
                                else
                                {
                                    double paxVery_small = 50 * 0 / Airports.VerySmallAirports;
                                    paxVery_small *= MathHelpers.GetRandomDoubleNumber(0.97, 1.06);
                                    estimatedPassengerLevel = (paxVery_small * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                                }
                                break;

                            case "Smallest":
                                if (airport.Profile.Pax > 0)
                                {
                                    double paxSmallest = airport.Profile.Pax * 0 / Airports.SmallestAirports;
                                    paxSmallest *= MathHelpers.GetRandomDoubleNumber(0.98, 1.04);
                                    estimatedPassengerLevel = (paxSmallest * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                                }
                                else
                                {
                                    double paxSmallest = 50 * 0 / Airports.SmallestAirports;
                                    paxSmallest *= MathHelpers.GetRandomDoubleNumber(0.98, 1.04);
                                    estimatedPassengerLevel = (paxSmallest * 1000) / 365;
                                    estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                                }
                                break;

                        } break;
                    #endregion
                }
            }

            #region Demand with "if" statements
            //PLEASE don't change the same country/continent/international values. Most of these were specifically calculated and are not yet calculated 
            //by the program itself! Based largely on US airport system values.
            /*     #region largest airports
                 if (dAirportSize.Equals("Largest") && airportSize.Equals("Largest"))
                 {
                     if (airport.Profile.Pax > 0)
                     {
                         double paxLargest = airport.Profile.Pax * 0.21 / Airports.LargestAirports;
                         paxLargest *= MathHelpers.GetRandomDoubleNumber(0.9, 1.11);
                         estimatedPassengerLevel = (paxLargest * 1000) / 365;
                         estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                         if (isSameCountry)
                             estimatedPassengerLevel *= 1.67;
                         if (isSameContinent)
                             estimatedPassengerLevel *= 1.39;
                         if (!isSameContinent && !isSameCountry)
                             estimatedPassengerLevel *= 0.55;
                     }
                     else
                     {
                         double paxLargest = 40000 * 0.21 / Airports.LargestAirports;
                         paxLargest *= MathHelpers.GetRandomDoubleNumber(0.9, 1.11);
                         estimatedPassengerLevel = (paxLargest * 1000) / 365;
                         estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                         if (isSameCountry)
                             estimatedPassengerLevel *= 1.67;
                         if (isSameContinent)
                             estimatedPassengerLevel *= 1.39;
                         if (!isSameContinent && !isSameCountry)
                             estimatedPassengerLevel *= 0.55;
                     }
                 }

                 if (dAirportSize.Equals("Very_large") && airportSize.Equals("Largest"))
                 {
                     if (airport.Profile.Pax == 0)
                     {
                         double paxVeryLarge = 40000 * 0.24 / Airports.VeryLargeAirports;
                         paxVeryLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.12);
                         estimatedPassengerLevel = paxVeryLarge * 1000 / 365;
                         estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                         if (isSameCountry)
                             estimatedPassengerLevel *= 1.67;
                         if (isSameContinent)
                             estimatedPassengerLevel *= 1.39;
                         if (!isSameContinent && !isSameCountry)
                             estimatedPassengerLevel *= 0.55;
                     }
                     else
                     {
                         double paxVeryLarge = airport.Profile.Pax * 0.24 / Airports.VeryLargeAirports;
                         paxVeryLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.12);
                         estimatedPassengerLevel = (paxVeryLarge * 1000) / 365;
                         estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                         if (isSameCountry)
                             estimatedPassengerLevel *= 1.67;
                         if (isSameContinent)
                             estimatedPassengerLevel *= 1.39;
                         if (!isSameContinent && !isSameCountry)
                             estimatedPassengerLevel *= 0.55;
                     }
                 }

                 if (dAirportSize.Equals("Large") && airportSize.Equals("Largest"))
                 {
                     if (airport.Profile.Pax == 0)
                     {
                         double paxLarge = 40000 * 0.24 / Airports.LargeAirports;
                         paxLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.14);
                         estimatedPassengerLevel = (paxLarge * 1000) / 365;
                         estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                         if (isSameCountry)
                             estimatedPassengerLevel *= 1.67;
                         if (isSameContinent)
                             estimatedPassengerLevel *= 1.39;
                         if (!isSameContinent && !isSameCountry)
                             estimatedPassengerLevel *= 0.55;
                     }
                     else
                     {
                         double paxLarge = airport.Profile.Pax * 0.24 / Airports.LargeAirports;
                         paxLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.14);
                         estimatedPassengerLevel = (paxLarge * 1000) / 365;
                         estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                         if (isSameCountry)
                             estimatedPassengerLevel *= 1.67;
                         if (isSameContinent)
                             estimatedPassengerLevel *= 1.39;
                         if (!isSameContinent && !isSameCountry)
                             estimatedPassengerLevel *= 0.55;
                     }
                 }

                 if (dAirportSize.Equals("Medium") && airportSize.Equals("Largest"))
                 {
                     if (airport.Profile.Pax == 0)
                     {
                         double paxMedium = 40000 * 0.15 / Airports.MediumAirports;
                         paxMedium *= MathHelpers.GetRandomDoubleNumber(0.9, 1.16);
                         estimatedPassengerLevel = (paxMedium * 1000) / 365;
                         estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                         if (isSameCountry)
                             estimatedPassengerLevel *= 1.67;
                         if (isSameContinent)
                             estimatedPassengerLevel *= 1.39;
                         if (!isSameContinent && !isSameCountry)
                             estimatedPassengerLevel *= 0.55;
                     }
                     else
                     {
                         double paxMedium = airport.Profile.Pax * 0.15 / Airports.MediumAirports;
                         paxMedium *= MathHelpers.GetRandomDoubleNumber(0.9, 1.16);
                         estimatedPassengerLevel = (paxMedium * 1000) / 365;
                         estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                         if (isSameCountry)
                             estimatedPassengerLevel *= 1.67;
                         if (isSameContinent)
                             estimatedPassengerLevel *= 1.39;
                         if (!isSameContinent && !isSameCountry)
                             estimatedPassengerLevel *= 0.55;
                     }
                 }

                 if (dAirportSize.Equals("Small") && airportSize.Equals("Largest"))
                 {
                     if (airport.Profile.Pax == 0)
                     {
                         double paxSmall = 40000 * 0.10 / Airports.SmallAirports;
                         paxSmall *= MathHelpers.GetRandomDoubleNumber(0.95, 1.10);
                         estimatedPassengerLevel = (paxSmall * 1000) / 365;
                         estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                         if (isSameCountry)
                             estimatedPassengerLevel *= 1.67;
                         if (isSameContinent)
                             estimatedPassengerLevel *= 1.39;
                         if (!isSameContinent && !isSameCountry)
                             estimatedPassengerLevel *= 0.2;
                     }
                     else
                     {
                         double paxSmall = airport.Profile.Pax * 0.10 / Airports.SmallAirports;
                         paxSmall *= MathHelpers.GetRandomDoubleNumber(0.95, 1.10);
                         estimatedPassengerLevel = (paxSmall * 1000) / 365;
                         estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                         if (isSameCountry)
                             estimatedPassengerLevel *= 1.67;
                         if (isSameContinent)
                             estimatedPassengerLevel *= 1.39;
                         if (!isSameContinent && !isSameCountry)
                             estimatedPassengerLevel *= 0.2;
                     }
                 }

                 if (dAirportSize.Equals("Very_small") && airportSize.Equals("Largest"))
                 {
                     if (airport.Profile.Pax == 0)
                     {
                         double paxVery_small = 40000 * 0.04 / Airports.VerySmallAirports;
                         paxVery_small *= MathHelpers.GetRandomDoubleNumber(0.97, 1.06);
                         estimatedPassengerLevel = (paxVery_small * 1000) / 365;
                         estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                         if (isSameCountry)
                             estimatedPassengerLevel *= 1.67;
                         if (isSameContinent)
                             estimatedPassengerLevel *= 1.39;
                         if (!isSameContinent && !isSameCountry)
                             estimatedPassengerLevel *= 0.1;
                     }
                     else
                     {
                         double paxVery_small = airport.Profile.Pax * 0.04 / Airports.VerySmallAirports;
                         paxVery_small *= MathHelpers.GetRandomDoubleNumber(0.97, 1.06);
                         estimatedPassengerLevel = (paxVery_small * 1000) / 365;
                         estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                         if (isSameCountry)
                             estimatedPassengerLevel *= 1.67;
                         if (isSameContinent)
                             estimatedPassengerLevel *= 1.39;
                         if (!isSameContinent && !isSameCountry)
                             estimatedPassengerLevel *= 0.1;
                     }
                 }

                 if (dAirportSize.Equals("Smallest") && airportSize.Equals("Largest"))
                 {
                     if (dist > 1600)
                     { estimatedPassengerLevel = 0; }

                     else if (airport.Profile.Pax == 0)
                     {
                         double paxSmallest = 40000 * 0.02 / Airports.SmallestAirports;
                         paxSmallest *= MathHelpers.GetRandomDoubleNumber(0.98, 1.04);
                         estimatedPassengerLevel = (paxSmallest * 1000) / 365;
                         estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                         if (isSameCountry)
                             estimatedPassengerLevel *= 1.8;
                         if (isSameContinent)
                             estimatedPassengerLevel *= 0.8;
                         if (!isSameContinent && !isSameCountry)
                             estimatedPassengerLevel *= 0;
                     }
                     else
                     {
                         double paxSmallest = 40000 * 0.02 / Airports.SmallestAirports;
                         paxSmallest *= MathHelpers.GetRandomDoubleNumber(0.98, 1.04);
                         estimatedPassengerLevel = (paxSmallest * 1000) / 365;
                         estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                         if (isSameCountry)
                             estimatedPassengerLevel *= 1.8;
                         if (isSameContinent)
                             estimatedPassengerLevel *= 0.8;
                         if (!isSameContinent && !isSameCountry)
                             estimatedPassengerLevel *= 0;
                     }
                 }
             }
                 #endregion
             #region very large airports
             if (dAirportSize.Equals("Largest") && airportSize.Equals("Very_large"))
             {
                 if (airport.Profile.Pax == 0)
                 {
                     double paxLargest = 20000 * 0.21 / Airports.LargestAirports;
                     paxLargest *= MathHelpers.GetRandomDoubleNumber(0.9, 1.11);
                     estimatedPassengerLevel = paxLargest * 1000 / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
                 else
                 {
                     double paxLargest = airport.Profile.Pax * 0.21 / Airports.LargestAirports;
                     paxLargest *= MathHelpers.GetRandomDoubleNumber(0.9, 1.11);
                     estimatedPassengerLevel = (paxLargest * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel; if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
             }

             if (dAirportSize.Equals("Very_large") && airportSize.Equals("Very_large"))
             {
                 if (airport.Profile.Pax == 0)
                 {
                     double paxVeryLarge = 20000 * 0.24 / Airports.VeryLargeAirports;
                     paxVeryLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.12);
                     estimatedPassengerLevel = paxVeryLarge * 1000 / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
                 else
                 {
                     double paxVeryLarge = airport.Profile.Pax * 0.24 / Airports.VeryLargeAirports;
                     paxVeryLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.12);
                     estimatedPassengerLevel = (paxVeryLarge * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
             }

             if (dAirportSize.Equals("Large") && airportSize.Equals("Very_large"))
             {
                 if (airport.Profile.Pax == 0)
                 {
                     double paxLarge = 20000 * 0.24 / Airports.LargeAirports;
                     paxLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.14);
                     estimatedPassengerLevel = (paxLarge * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
                 else
                 {
                     double paxLarge = airport.Profile.Pax * 0.24 / Airports.LargeAirports;
                     paxLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.14);
                     estimatedPassengerLevel = (paxLarge * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
             }

             if (dAirportSize.Equals("Medium") && airportSize.Equals("Very_large"))
             {
                 if (airport.Profile.Pax == 0)
                 {
                     double paxMedium = 20000 * 0.15 / Airports.MediumAirports;
                     paxMedium *= MathHelpers.GetRandomDoubleNumber(0.9, 1.16);
                     estimatedPassengerLevel = (paxMedium * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
                 else
                 {
                     double paxMedium = airport.Profile.Pax * 0.15 / Airports.MediumAirports;
                     paxMedium *= MathHelpers.GetRandomDoubleNumber(0.9, 1.16);
                     estimatedPassengerLevel = (paxMedium * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
             }

             if (dAirportSize.Equals("Small") && airportSize.Equals("Very_large"))
             {
                 if (airport.Profile.Pax == 0)
                 {
                     double paxSmall = 20000 * 0.10 / Airports.SmallAirports;
                     paxSmall *= MathHelpers.GetRandomDoubleNumber(0.95, 1.10);
                     estimatedPassengerLevel = (paxSmall * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.7;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.1;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.35;
                 }
                 else
                 {
                     double paxSmall = airport.Profile.Pax * 0.10 / Airports.SmallAirports;
                     paxSmall *= MathHelpers.GetRandomDoubleNumber(0.95, 1.10);
                     estimatedPassengerLevel = (paxSmall * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.7;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.35;
                 }
             }

             if (dAirportSize.Equals("Very_small") && airportSize.Equals("Very_large"))
             {
                 if (airport.Profile.Pax == 0)
                 {
                     double paxVery_small = 20000 * 0.04 / Airports.VerySmallAirports;
                     paxVery_small *= MathHelpers.GetRandomDoubleNumber(0.97, 1.06);
                     estimatedPassengerLevel = (paxVery_small * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.75;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.2;
                 }
                 else
                 {
                     double paxVery_small = airport.Profile.Pax * 0.04 / Airports.VerySmallAirports;
                     paxVery_small *= MathHelpers.GetRandomDoubleNumber(0.97, 1.06);
                     estimatedPassengerLevel = (paxVery_small * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.75;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.2;
                 }
             }

             if (dAirportSize.Equals("Smallest") && airportSize.Equals("Very_large"))
             {
                 if (dist > 800)
                 { estimatedPassengerLevel = 0; }

                 else if (airport.Profile.Pax == 0)
                 {
                     double paxSmallest = 20000 * 0.02 / Airports.SmallestAirports;
                     paxSmallest *= MathHelpers.GetRandomDoubleNumber(0.98, 1.04);
                     estimatedPassengerLevel = (paxSmallest * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.8;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 0.8;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.1;
                 }
                 else
                 {
                     double paxSmallest = 20000 * 0.02 / Airports.SmallestAirports;
                     paxSmallest *= MathHelpers.GetRandomDoubleNumber(0.98, 1.04);
                     estimatedPassengerLevel = (paxSmallest * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.8;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 0.8;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.1;
                 }
             }

             #endregion
             #region large airports
             if (dAirportSize.Equals("Largest") && airportSize.Equals("Large"))
             {
                 if (airport.Profile.Pax == 0)
                 {
                     double paxLargest = 10000 * 0.21 / Airports.LargestAirports;
                     paxLargest *= MathHelpers.GetRandomDoubleNumber(0.9, 1.11);
                     estimatedPassengerLevel = paxLargest * 1000 / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
                 else
                 {
                     double paxLargest = airport.Profile.Pax * 0.21 / Airports.LargestAirports;
                     paxLargest *= MathHelpers.GetRandomDoubleNumber(0.9, 1.11);
                     estimatedPassengerLevel = (paxLargest * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel; if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
             }

             if (dAirportSize.Equals("Very_large") && airportSize.Equals("Large"))
             {
                 if (airport.Profile.Pax == 0)
                 {
                     double paxVeryLarge = 10000 * 0.24 / Airports.VeryLargeAirports;
                     paxVeryLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.12);
                     estimatedPassengerLevel = paxVeryLarge * 1000 / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
                 else
                 {
                     double paxVeryLarge = airport.Profile.Pax * 0.24 / Airports.VeryLargeAirports;
                     paxVeryLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.12);
                     estimatedPassengerLevel = (paxVeryLarge * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
             }

             if (dAirportSize.Equals("Large") && airportSize.Equals("Large"))
             {
                 if (airport.Profile.Pax == 0)
                 {
                     double paxLarge = 10000 * 0.24 / Airports.LargeAirports;
                     paxLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.14);
                     estimatedPassengerLevel = (paxLarge * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
                 else
                 {
                     double paxLarge = airport.Profile.Pax * 0.24 / Airports.LargeAirports;
                     paxLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.14);
                     estimatedPassengerLevel = (paxLarge * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
             }

             if (dAirportSize.Equals("Medium") && airportSize.Equals("Large"))
             {
                 if (airport.Profile.Pax == 0)
                 {
                     double paxMedium = 10000 * 0.15 / Airports.MediumAirports;
                     paxMedium *= MathHelpers.GetRandomDoubleNumber(0.9, 1.16);
                     estimatedPassengerLevel = (paxMedium * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
                 else
                 {
                     double paxMedium = airport.Profile.Pax * 0.15 / Airports.MediumAirports;
                     paxMedium *= MathHelpers.GetRandomDoubleNumber(0.9, 1.16);
                     estimatedPassengerLevel = (paxMedium * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
             }

             if (dAirportSize.Equals("Small") && airportSize.Equals("Large"))
             {
                 if (airport.Profile.Pax == 0)
                 {
                     double paxSmall = 10000 * 0.10 / Airports.SmallAirports;
                     paxSmall *= MathHelpers.GetRandomDoubleNumber(0.95, 1.10);
                     estimatedPassengerLevel = (paxSmall * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.75;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.15;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.3;
                 }
                 else
                 {
                     double paxSmall = airport.Profile.Pax * 0.10 / Airports.SmallAirports;
                     paxSmall *= MathHelpers.GetRandomDoubleNumber(0.95, 1.10);
                     estimatedPassengerLevel = (paxSmall * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.75;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.15;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.3;
                 }
             }

             if (dAirportSize.Equals("Very_small") && airportSize.Equals("Large"))
             {
                 if (airport.Profile.Pax == 0)
                 {
                     double paxVery_small = 10000 * 0.04 / Airports.VerySmallAirports;
                     paxVery_small *= MathHelpers.GetRandomDoubleNumber(0.97, 1.06);
                     estimatedPassengerLevel = (paxVery_small * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.8;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 0.9;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.13;
                 }
                 else
                 {
                     double paxVery_small = airport.Profile.Pax * 0.04 / Airports.VerySmallAirports;
                     paxVery_small *= MathHelpers.GetRandomDoubleNumber(0.97, 1.06);
                     estimatedPassengerLevel = (paxVery_small * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.8;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 0.9;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.13;
                 }
             }

             if (dAirportSize.Equals("Smallest") && airportSize.Equals("Large"))
             {
                 if (dist > 800)
                 { estimatedPassengerLevel = 0; }

                 else if (airport.Profile.Pax == 0)
                 {
                     double paxSmallest = 10000 * 0.02 / Airports.SmallestAirports;
                     paxSmallest *= MathHelpers.GetRandomDoubleNumber(0.98, 1.04);
                     estimatedPassengerLevel = (paxSmallest * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.9;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 0.7;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.05;
                 }
                 else
                 {
                     double paxSmallest = 10000 * 0.02 / Airports.SmallestAirports;
                     paxSmallest *= MathHelpers.GetRandomDoubleNumber(0.98, 1.04);
                     estimatedPassengerLevel = (paxSmallest * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.9;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 0.7;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.05;
                 }
             }
             #endregion
             #region medium airports
             if (dAirportSize.Equals("Largest") && airportSize.Equals("Medium"))
             {
                 if (airport.Profile.Pax == 0)
                 {
                     double paxLargest = 6000 * 0.21 / Airports.LargestAirports;
                     paxLargest *= MathHelpers.GetRandomDoubleNumber(0.9, 1.11);
                     estimatedPassengerLevel = paxLargest * 1000 / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
                 else
                 {
                     double paxLargest = airport.Profile.Pax * 0.21 / Airports.LargestAirports;
                     paxLargest *= MathHelpers.GetRandomDoubleNumber(0.9, 1.11);
                     estimatedPassengerLevel = (paxLargest * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel; if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
             }

             if (dAirportSize.Equals("Very_large") && airportSize.Equals("Medium"))
             {
                 if (airport.Profile.Pax == 0)
                 {
                     double paxVeryLarge = 6000 * 0.24 / Airports.VeryLargeAirports;
                     paxVeryLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.12);
                     estimatedPassengerLevel = paxVeryLarge * 1000 / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
                 else
                 {
                     double paxVeryLarge = airport.Profile.Pax * 0.24 / Airports.VeryLargeAirports;
                     paxVeryLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.12);
                     estimatedPassengerLevel = (paxVeryLarge * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
             }

             if (dAirportSize.Equals("Large") && airportSize.Equals("Medium"))
             {
                 if (airport.Profile.Pax == 0)
                 {
                     double paxLarge = 6000 * 0.24 / Airports.LargeAirports;
                     paxLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.14);
                     estimatedPassengerLevel = (paxLarge * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
                 else
                 {
                     double paxLarge = airport.Profile.Pax * 0.24 / Airports.LargeAirports;
                     paxLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.14);
                     estimatedPassengerLevel = (paxLarge * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
             }

             if (dAirportSize.Equals("Medium") && airportSize.Equals("Medium"))
             {
                 if (airport.Profile.Pax == 0)
                 {
                     double paxMedium = 6000 * 0.15 / Airports.MediumAirports;
                     paxMedium *= MathHelpers.GetRandomDoubleNumber(0.9, 1.16);
                     estimatedPassengerLevel = (paxMedium * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
                 else
                 {
                     double paxMedium = airport.Profile.Pax * 0.15 / Airports.MediumAirports;
                     paxMedium *= MathHelpers.GetRandomDoubleNumber(0.9, 1.16);
                     estimatedPassengerLevel = (paxMedium * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
             }

             if (dAirportSize.Equals("Small") && airportSize.Equals("Medium"))
             {
                 if (airport.Profile.Pax == 0)
                 {
                     double paxSmall = 6000 * 0.10 / Airports.SmallAirports;
                     paxSmall *= MathHelpers.GetRandomDoubleNumber(0.95, 1.10);
                     estimatedPassengerLevel = (paxSmall * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.8;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.25;
                 }
                 else
                 {
                     double paxSmall = airport.Profile.Pax * 0.10 / Airports.SmallAirports;
                     paxSmall *= MathHelpers.GetRandomDoubleNumber(0.95, 1.10);
                     estimatedPassengerLevel = (paxSmall * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.8;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.25;
                 }
             }

             if (dAirportSize.Equals("Very_small") && airportSize.Equals("Medium"))
             {
                 if (dist > 800)
                 { estimatedPassengerLevel = 0; }
                
                 else if (airport.Profile.Pax == 0)
                 {
                     double paxVery_small = 6000 * 0.04 / Airports.VerySmallAirports;
                     paxVery_small *= MathHelpers.GetRandomDoubleNumber(0.97, 1.06);
                     estimatedPassengerLevel = (paxVery_small * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.95;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 0.75;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.1;
                 }
                 else
                 {
                     double paxVery_small = airport.Profile.Pax * 0.04 / Airports.VerySmallAirports;
                     paxVery_small *= MathHelpers.GetRandomDoubleNumber(0.97, 1.06);
                     estimatedPassengerLevel = (paxVery_small * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.95;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 0.75;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.1;
                 }
             }

             if (dAirportSize.Equals("Smallest") && airportSize.Equals("Medium"))
             {
                 if (dist > 1200 )
                 { estimatedPassengerLevel = 0; }

                 else if (airport.Profile.Pax == 0)
                 {
                     double paxSmallest = 6000 * 0.02 / Airports.SmallestAirports;
                     paxSmallest *= MathHelpers.GetRandomDoubleNumber(0.98, 1.04);
                     estimatedPassengerLevel = (paxSmallest * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 2;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 0.5;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.05;
                 }
                 else
                 {
                     double paxSmallest = 6000 * 0.02 / Airports.SmallestAirports;
                     paxSmallest *= MathHelpers.GetRandomDoubleNumber(0.98, 1.04);
                     estimatedPassengerLevel = (paxSmallest * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 2;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 0.5;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.05;
                 }
             }
             #endregion
             #region small airports
             if (dAirportSize.Equals("Largest") && airportSize.Equals("Small"))
             {
                 if (airport.Profile.Pax == 0)
                 {
                     double paxLargest = 1250 * 0.21 / Airports.LargestAirports;
                     paxLargest *= MathHelpers.GetRandomDoubleNumber(0.9, 1.11);
                     estimatedPassengerLevel = paxLargest * 1000 / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
                 else
                 {
                     double paxLargest = airport.Profile.Pax * 0.21 / Airports.LargestAirports;
                     paxLargest *= MathHelpers.GetRandomDoubleNumber(0.9, 1.11);
                     estimatedPassengerLevel = (paxLargest * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel; if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
             }

             if (dAirportSize.Equals("Very_large") && airportSize.Equals("Small"))
             {
                 if (airport.Profile.Pax == 0)
                 {
                     double paxVeryLarge = 1250 * 0.24 / Airports.VeryLargeAirports;
                     paxVeryLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.12);
                     estimatedPassengerLevel = paxVeryLarge * 1000 / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
                 else
                 {
                     double paxVeryLarge = airport.Profile.Pax * 0.24 / Airports.VeryLargeAirports;
                     paxVeryLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.12);
                     estimatedPassengerLevel = (paxVeryLarge * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
             }

             if (dAirportSize.Equals("Large") && airportSize.Equals("Small"))
             {
                 if (airport.Profile.Pax == 0)
                 {
                     double paxLarge = 1250 * 0.24 / Airports.LargeAirports;
                     paxLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.14);
                     estimatedPassengerLevel = (paxLarge * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
                 else
                 {
                     double paxLarge = airport.Profile.Pax * 0.24 / Airports.LargeAirports;
                     paxLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.14);
                     estimatedPassengerLevel = (paxLarge * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
             }

             if (dAirportSize.Equals("Medium") && airportSize.Equals("Small"))
             {
                 if (airport.Profile.Pax == 0)
                 {
                     double paxMedium = 1250 * 0.17 / Airports.MediumAirports;
                     paxMedium *= MathHelpers.GetRandomDoubleNumber(0.9, 1.16);
                     estimatedPassengerLevel = (paxMedium * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
                 else
                 {
                     double paxMedium = airport.Profile.Pax * 0.15 / Airports.MediumAirports;
                     paxMedium *= MathHelpers.GetRandomDoubleNumber(0.9, 1.16);
                     estimatedPassengerLevel = (paxMedium * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
             }

             if (dAirportSize.Equals("Small") && airportSize.Equals("Small"))
             {
                 if (dist > 1600)
                 { estimatedPassengerLevel = 0;}

                 else if (airport.Profile.Pax == 0)
                 {
                     double paxSmall = 1250 / Airports.SmallAirports;
                     paxSmall *= MathHelpers.GetRandomDoubleNumber(0.95, 1.10);
                     estimatedPassengerLevel = (paxSmall * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.9;
                     if (isSameContinent)
                         estimatedPassengerLevel *= .9;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.15;
                 }
                 else
                 {
                     double paxSmall = airport.Profile.Pax * 0.10 / Airports.SmallAirports;
                     paxSmall *= MathHelpers.GetRandomDoubleNumber(0.95, 1.10);
                     estimatedPassengerLevel = (paxSmall * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.9;
                     if (isSameContinent)
                         estimatedPassengerLevel *= .7;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.15;
                 }
             }

             if (dAirportSize.Equals("Very_small") && airportSize.Equals("Small"))
             {
                 if (dist > 1200)
                 {estimatedPassengerLevel = 0;}

                 else if (airport.Profile.Pax == 0)
                 {
                     double paxVery_small = 1250 / Airports.VerySmallAirports;
                     paxVery_small *= MathHelpers.GetRandomDoubleNumber(0.97, 1.06);
                     estimatedPassengerLevel = (paxVery_small * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 2.1;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 0.4;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.05;
                 }
                 else
                 {
                     double paxVery_small = airport.Profile.Pax * 0.04 / Airports.VerySmallAirports;
                     paxVery_small *= MathHelpers.GetRandomDoubleNumber(0.97, 1.06);
                     estimatedPassengerLevel = (paxVery_small * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 2.1;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 0.4;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.05;
                 }
             }

             if (dAirportSize.Equals("Smallest") && airportSize.Equals("Small"))
             {
                 if (dist > 800)
                 { estimatedPassengerLevel = 0;}

                 else if (airport.Profile.Pax == 0)
                 {
                     double paxSmallest = 0 * 0.02 / Airports.SmallestAirports;
                     paxSmallest *= MathHelpers.GetRandomDoubleNumber(0.98, 1.04);
                     estimatedPassengerLevel = (paxSmallest * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 2.25;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 0.25;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0;
                 }
                 else
                 {
                     double paxSmallest = airport.Profile.Pax * 0 / Airports.SmallestAirports;
                     paxSmallest *= MathHelpers.GetRandomDoubleNumber(0.98, 1.04);
                     estimatedPassengerLevel = (paxSmallest * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0;
                 }
             }
             #endregion
             #region very small airports
             if (dAirportSize.Equals("Largest") && airportSize.Equals("Very_small"))
             {
                 if (airport.Profile.Pax == 0)
                 {
                     double paxLargest = 350 * 0.21 / Airports.LargestAirports;
                     paxLargest *= MathHelpers.GetRandomDoubleNumber(0.9, 1.11);
                     estimatedPassengerLevel = paxLargest * 1000 / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
                 else
                 {
                     double paxLargest = airport.Profile.Pax * 0.21 / Airports.LargestAirports;
                     paxLargest *= MathHelpers.GetRandomDoubleNumber(0.9, 1.11);
                     estimatedPassengerLevel = (paxLargest * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel; if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
             }

             if (dAirportSize.Equals("Very_large") && airportSize.Equals("Very_small"))
             {
                 if (airport.Profile.Pax == 0)
                 {
                     double paxVeryLarge = 350 * 0.27 / Airports.VeryLargeAirports;
                     paxVeryLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.12);
                     estimatedPassengerLevel = paxVeryLarge * 1000 / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
                 else
                 {
                     double paxVeryLarge = airport.Profile.Pax * 0.24 / Airports.VeryLargeAirports;
                     paxVeryLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.12);
                     estimatedPassengerLevel = (paxVeryLarge * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
             }

             if (dAirportSize.Equals("Large") && airportSize.Equals("Very_small"))
             {
                 if (airport.Profile.Pax == 0)
                 {
                     double paxLarge = 350 * 0.27 / Airports.LargeAirports;
                     paxLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.14);
                     estimatedPassengerLevel = (paxLarge * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
                 else
                 {
                     double paxLarge = airport.Profile.Pax * 0.24 / Airports.LargeAirports;
                     paxLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.14);
                     estimatedPassengerLevel = (paxLarge * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 1.67;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.39;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
             }

             if (dAirportSize.Equals("Medium") && airportSize.Equals("Very_small"))
             {
                 if ( dist > 1600)
                 { estimatedPassengerLevel = 0;}

                 else if (airport.Profile.Pax == 0)
                 {
                     double paxMedium = 350 * 0.15 / Airports.MediumAirports;
                     paxMedium *= MathHelpers.GetRandomDoubleNumber(0.9, 1.16);
                     estimatedPassengerLevel = (paxMedium * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 2;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 0.5;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.15;
                 }
                 else
                 {
                     double paxMedium = airport.Profile.Pax * 0.15 / Airports.MediumAirports;
                     paxMedium *= MathHelpers.GetRandomDoubleNumber(0.9, 1.16);
                     estimatedPassengerLevel = (paxMedium * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 2;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 0.5;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.15;
                 }
             }

             if (dAirportSize.Equals("Small") && airportSize.Equals("Very_small"))
             {
                 if (dist > 1200 )
                 { estimatedPassengerLevel = 0; }

                 else if (airport.Profile.Pax == 0)
                 {
                     double paxSmall = 350 * 0.10 / Airports.SmallAirports;
                     paxSmall *= MathHelpers.GetRandomDoubleNumber(0.95, 1.10);
                     estimatedPassengerLevel = (paxSmall * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 2.25;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 0.35;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
                 else
                 {
                     double paxSmall = airport.Profile.Pax * 0.10 / Airports.SmallAirports;
                     paxSmall *= MathHelpers.GetRandomDoubleNumber(0.95, 1.10);
                     estimatedPassengerLevel = (paxSmall * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 2.25;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 0.35;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                 }
             }

             if (dAirportSize.Equals("Very_small") && airportSize.Equals("Very_small"))
             {
                 if (dist > 800)
                 { estimatedPassengerLevel = 0;}

                 else if (airport.Profile.Pax == 0)
                 {
                     double paxVery_small = 350 * 0 / Airports.VerySmallAirports;
                     paxVery_small *= MathHelpers.GetRandomDoubleNumber(0.97, 1.06);
                     estimatedPassengerLevel = (paxVery_small * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 2.35;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 0.25;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0;
                 }
                 else
                 {
                     double paxVery_small = airport.Profile.Pax * 0 / Airports.VerySmallAirports;
                     paxVery_small *= MathHelpers.GetRandomDoubleNumber(0.97, 1.06);
                     estimatedPassengerLevel = (paxVery_small * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 2.35;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 0.25;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0;
                 }
             }

             if (dAirportSize.Equals("Smallest") && airportSize.Equals("Very_small"))
             {
                 if ( dist > 800)
                 {estimatedPassengerLevel = 0;}

                 else if (airport.Profile.Pax == 0)
                 {
                     double paxSmallest = 350 * 0 / Airports.SmallestAirports;
                     paxSmallest *= MathHelpers.GetRandomDoubleNumber(0.98, 1.04);
                     estimatedPassengerLevel = (paxSmallest * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 2.5;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 0.25;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0;
                 }
                 else
                 {
                     double paxSmallest = 350 * 0 / Airports.SmallestAirports;
                     paxSmallest *= MathHelpers.GetRandomDoubleNumber(0.98, 1.04);
                     estimatedPassengerLevel = (paxSmallest * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 2.5;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 0.25;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0;
                 }
             }
             #endregion
             #region smallest airports
             if (dAirportSize.Equals("Largest") && airportSize.Equals("Smallest"))
             {
                 if (dist > 1600)
                 { estimatedPassengerLevel = 0; }
                 else if (airport.Profile.Pax == 0)
                 {
                     double paxLargest = 50 * 0.25 / Airports.LargestAirports;
                     paxLargest *= MathHelpers.GetRandomDoubleNumber(0.9, 1.11);
                     estimatedPassengerLevel = paxLargest * 1000 / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry && dist < 500)
                         estimatedPassengerLevel *= 5;
                     else if (isSameCountry && dist < 1000)
                         estimatedPassengerLevel *= 3;
                     else estimatedPassengerLevel *= 1.25;
                     if (isSameContinent && dist < 2000)
                         estimatedPassengerLevel *= 2;
                     else estimatedPassengerLevel *= 1.25;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                     if (estimatedPassengerLevel < 10)
                     { estimatedPassengerLevel *= 0; }
                     else { estimatedPassengerLevel *= 1; }
                 }
                 else
                 {
                     double paxLargest = airport.Profile.Pax * 0.25 / Airports.LargestAirports;
                     paxLargest *= MathHelpers.GetRandomDoubleNumber(0.9, 1.11);
                     estimatedPassengerLevel = (paxLargest * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry && dist < 500)
                         estimatedPassengerLevel *= 5;
                     else if (isSameCountry && dist < 1000)
                         estimatedPassengerLevel *= 3;
                     else estimatedPassengerLevel *= 1.25;
                     if (isSameContinent && dist < 2000)
                         estimatedPassengerLevel *= 2;
                     else estimatedPassengerLevel *= 1.25;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                     if (estimatedPassengerLevel < 10)
                     { estimatedPassengerLevel *= 0; }
                     else { estimatedPassengerLevel *= 1; }
                 }
                 if (dist < 1600)
                 { estimatedPassengerLevel *= 2; }
                 else if (dist < 2400)
                 { estimatedPassengerLevel *= 0.5; }
                 else
                 { estimatedPassengerLevel = 0; }
             }

             if (dAirportSize.Equals("Very_large") && airportSize.Equals("Smallest"))
             {
                 if (dist > 1600)
                 { estimatedPassengerLevel = 0; }
                 else if (airport.Profile.Pax == 0)
                 {
                     double paxVeryLarge = 50 * 0.32 / Airports.VeryLargeAirports;
                     paxVeryLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.12);
                     estimatedPassengerLevel = paxVeryLarge * 1000 / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry && dist < 500)
                         estimatedPassengerLevel *= 5;
                     else if (isSameCountry && dist < 1000)
                         estimatedPassengerLevel *= 3;
                     else estimatedPassengerLevel *= 1.25;
                     if (isSameContinent && dist < 2000)
                         estimatedPassengerLevel *= 2;
                     else estimatedPassengerLevel *= 1.25;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                     if (estimatedPassengerLevel < 10)
                     { estimatedPassengerLevel *= 0; }
                     else { estimatedPassengerLevel *= 1; }
                 }
                 else
                 {
                     double paxVeryLarge = airport.Profile.Pax * 0.32 / Airports.VeryLargeAirports;
                     paxVeryLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.12);
                     estimatedPassengerLevel = (paxVeryLarge * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry && dist < 500)
                         estimatedPassengerLevel *= 5;
                     else if (isSameCountry && dist < 1000)
                         estimatedPassengerLevel *= 3;
                     else estimatedPassengerLevel *= 1.25;
                     if (isSameContinent && dist < 2000)
                         estimatedPassengerLevel *= 2;
                     else estimatedPassengerLevel *= 1.25;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                     if (estimatedPassengerLevel < 10)
                     { estimatedPassengerLevel *= 0; }
                     else { estimatedPassengerLevel *= 1; }
                 }
                 if (dist < 1600)
                 { estimatedPassengerLevel *= 2; }
                 else if (dist < 2400)
                 { estimatedPassengerLevel *= 0.5; }
                 else
                 { estimatedPassengerLevel = 0; }
             }

             if (dAirportSize.Equals("Large") && airportSize.Equals("Smallest"))
             {
                 if (dist > 1600)
                 { estimatedPassengerLevel = 0; }
                 else if (airport.Profile.Pax == 0)
                 {
                     double paxLarge = 50 * 0.32 / Airports.LargeAirports;
                     paxLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.14);
                     estimatedPassengerLevel = (paxLarge * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry && dist < 500)
                         estimatedPassengerLevel *= 5;
                     else if (isSameCountry && dist < 1000)
                         estimatedPassengerLevel *= 3;
                     else estimatedPassengerLevel *= 1.25;
                     if (isSameContinent && dist < 2000)
                         estimatedPassengerLevel *= 2;
                     else estimatedPassengerLevel *= 1.25;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                     if (estimatedPassengerLevel < 10)
                     { estimatedPassengerLevel *= 0; }
                     else { estimatedPassengerLevel *= 1; }
                 }
                 else
                 {
                     double paxLarge = airport.Profile.Pax * 0.32 / Airports.LargeAirports;
                     paxLarge *= MathHelpers.GetRandomDoubleNumber(0.9, 1.14);
                     estimatedPassengerLevel = (paxLarge * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry && dist < 500)
                         estimatedPassengerLevel *= 5;
                     else if (isSameCountry && dist < 1000)
                         estimatedPassengerLevel *= 3;
                     else estimatedPassengerLevel *= 1.25;
                     if (isSameContinent && dist < 2000)
                         estimatedPassengerLevel *= 2;
                     else estimatedPassengerLevel *= 1.25;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                     if (estimatedPassengerLevel < 10)
                     { estimatedPassengerLevel *= 0; }
                     else { estimatedPassengerLevel *= 1; }
                 }
                 if (dist < 1600)
                 { estimatedPassengerLevel *= 2; }
                 else if (dist < 2400)
                 { estimatedPassengerLevel *= 0.5; }
                 else
                 { estimatedPassengerLevel = 0; }
             }

             if (dAirportSize.Equals("Medium") && airportSize.Equals("Smallest"))
             {
                 if (dist > 1200)
                 {estimatedPassengerLevel = 0;}

                 else if (airport.Profile.Pax == 0)
                 {
                     double paxMedium = 50 * 0.15 / Airports.MediumAirports;
                     paxMedium *= MathHelpers.GetRandomDoubleNumber(0.9, 1.16);
                     estimatedPassengerLevel = (paxMedium * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     estimatedPassengerLevel *= 2;
                     if (isSameCountry && dist < 500)
                         estimatedPassengerLevel *= 5;
                     else if (isSameCountry && dist < 1000)
                         estimatedPassengerLevel *= 3;
                     else estimatedPassengerLevel *= 1.25;
                     if (isSameContinent && dist < 2000)
                         estimatedPassengerLevel *= 2;
                     else estimatedPassengerLevel *= 1.25;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                     if (estimatedPassengerLevel < 10)
                     { estimatedPassengerLevel *= 0; }
                     else { estimatedPassengerLevel *= 1; }
                 }
                 else
                 {
                     double paxMedium = airport.Profile.Pax * 0.15 / Airports.MediumAirports;
                     paxMedium *= MathHelpers.GetRandomDoubleNumber(0.9, 1.16);
                     estimatedPassengerLevel = (paxMedium * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     estimatedPassengerLevel *= 2;
                     if (isSameCountry && dist < 500)
                         estimatedPassengerLevel *= 5;
                     else if (isSameCountry && dist < 1000)
                         estimatedPassengerLevel *= 3;
                     else estimatedPassengerLevel *= 1.25;
                     if (isSameContinent && dist < 2000)
                         estimatedPassengerLevel *= 2;
                     else estimatedPassengerLevel *= 1.25;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0.55;
                     if (estimatedPassengerLevel < 10)
                     { estimatedPassengerLevel *= 0; }
                     else { estimatedPassengerLevel *= 1; }
                 }
             }

             if (dAirportSize.Equals("Small") && airportSize.Equals("Smallest"))
             {
                 if (dist > 800)
                 {estimatedPassengerLevel = 0;}

                 else if (airport.Profile.Pax == 0)
                 {
                     double paxSmall = 50 * 0 / Airports.SmallAirports;
                     paxSmall *= MathHelpers.GetRandomDoubleNumber(0.95, 1.10);
                     estimatedPassengerLevel = (paxSmall * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     estimatedPassengerLevel *= 1.2;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 2.0;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0;
                     if (estimatedPassengerLevel < 10)
                     { estimatedPassengerLevel *= 0; }
                     else { estimatedPassengerLevel *= 1; }
                 }
                 else
                 {
                     double paxSmall = airport.Profile.Pax * 0 / Airports.SmallAirports;
                     paxSmall *= MathHelpers.GetRandomDoubleNumber(0.95, 1.10);
                     estimatedPassengerLevel = (paxSmall * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     estimatedPassengerLevel *= 1.2;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 2.0;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 1.00;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0;
                     if (estimatedPassengerLevel < 10)
                     { estimatedPassengerLevel *= 0; }
                     else { estimatedPassengerLevel *= 1; }
                 }
             }

             if (dAirportSize.Equals("Very_small") && airportSize.Equals("Smallest"))
             {
                 if (dist > 500)
                 {estimatedPassengerLevel = 0;}

                 else if (airport.Profile.Pax == 0)
                 {
                     double paxVery_small = 50 * 0 / Airports.VerySmallAirports;
                     paxVery_small *= MathHelpers.GetRandomDoubleNumber(0.97, 1.06);
                     estimatedPassengerLevel = (paxVery_small * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 2.35;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 0.75;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0;
                     if (estimatedPassengerLevel < 10)
                     { estimatedPassengerLevel *= 0; }
                     else { estimatedPassengerLevel *= 1; }
                 }
                 else
                 {
                     double paxVery_small = airport.Profile.Pax * 0 / Airports.VerySmallAirports;
                     paxVery_small *= MathHelpers.GetRandomDoubleNumber(0.97, 1.06);
                     estimatedPassengerLevel = (paxVery_small * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 2.35;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 0.75;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0;
                     if (estimatedPassengerLevel < 10)
                     { estimatedPassengerLevel *= 0; }
                     else { estimatedPassengerLevel *= 1; }
                 }
             }

             if (dAirportSize.Equals("Smallest") && airportSize.Equals("Smallest"))
             {
                 if (dist > 200)
                 {estimatedPassengerLevel = 0;}

                 else if (airport.Profile.Pax == 0)
                 {
                     double paxSmallest = 50 * 0 / Airports.SmallestAirports;
                     paxSmallest *= MathHelpers.GetRandomDoubleNumber(0.98, 1.04);
                     estimatedPassengerLevel = (paxSmallest * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 2.5;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 0;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel *= 0;
                     if (estimatedPassengerLevel < 10)
                     { estimatedPassengerLevel *= 0; }
                     else { estimatedPassengerLevel *= 1; }
                 }
                 else
                 {
                     double paxSmallest = 50 * 0 / Airports.SmallestAirports;
                     paxSmallest *= MathHelpers.GetRandomDoubleNumber(0.98, 1.04);
                     estimatedPassengerLevel = (paxSmallest * 1000) / 365;
                     estimatedPassengerLevel *= GameObject.GetInstance().Difficulty.PassengersLevel;
                     if (isSameCountry)
                         estimatedPassengerLevel *= 2.5;
                     if (isSameContinent)
                         estimatedPassengerLevel *= 0.5;
                     if (!isSameContinent && !isSameCountry)
                         estimatedPassengerLevel = 0;
                 }

                      

             }*/


             #endregion

            double value = estimatedPassengerLevel * GetDemandYearFactor(GameObject.GetInstance().GameTime.Year);


            double distance = MathHelpers.GetDistance(airport, dAirport);

            ushort rate = (ushort)value;

            if (rate > 0)
            {
                airport.addDestinationPassengersRate(new DestinationDemand(dAirport, rate));
                //DatabaseObject.GetInstance().addToTransaction(airport, dAirport, classType, rate);
            }




        }
        //returns the demand factor based on the year of playing
        private static double GetDemandYearFactor(int year)
        {
            double yearDiff = Convert.ToDouble(year - GameObject.StartYear) / 10;

            return 0.15 * (yearDiff + 1);


        }
        //changes the demand for all airports with a factor
        public static void ChangePaxDemand(double factor)
        {
            ChangePaxDemand(Airports.GetAllActiveAirports(), factor);
        }
        //changes the demand for a list of airports with a factor
        public static void ChangePaxDemand(List<Airport> airports, double factor)
        {
            //increases the passenger demand between airports with 5%
            Parallel.ForEach(airports, airport =>
            {
                ChangePaxDemand(airport, factor);
            });
        }

        //changes the demand for all airports belonging to an airline with a factor
        public static void ChangePaxDemand(Airline airline, double factor)
        {
            foreach (Airport a in airline.Airports)
            {
                ChangePaxDemand(a, factor);
            }
        }
        //changes the demand for an airport with a factor
        public static void ChangePaxDemand(Airport airport, double factor)
        {
            double value = (100 + factor) / 100;
            //factor 0.5 - 2;
            foreach (DestinationDemand destPax in airport.getDestinationsPassengers())
            {
                if (destPax.Rate > 0)
                {
                    ushort oRate = destPax.Rate;
                    if (oRate * value == destPax.Rate)
                        destPax.Rate = Convert.ToUInt16(destPax.Rate + Convert.ToUInt16(rnd.Next(0, Math.Max(1, (int)factor))));
                    else
                        destPax.Rate = (ushort)(destPax.Rate * value);
                }
                else
                    destPax.Rate = (ushort)rnd.Next(1, Math.Max(1, (int)factor));
            }
        }
    }
}