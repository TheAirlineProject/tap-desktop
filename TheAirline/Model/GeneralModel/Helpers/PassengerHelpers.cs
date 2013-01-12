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
        //returns the number of passengers between two destinations
        public static int GetFlightPassengers(Airport airportCurrent, Airport airportDestination,FleetAirliner airliner, AirlinerClass.ClassType type)
        {
            double distance = MathHelpers.GetDistance(airportCurrent, airportDestination);

            var currentRoute = airliner.Routes.Find(r =>r.Stopovers.SelectMany(s=>s.Legs).ToList().Exists(l=>(l.Destination1 == airportCurrent || l.Destination1 == airportDestination) && (l.Destination2 == airportDestination || l.Destination2 == airportCurrent)) || (r.Destination1 == airportCurrent || r.Destination1 == airportDestination) && (r.Destination2 == airportDestination || r.Destination2 == airportCurrent));
            
            if (currentRoute == null)
                return 0;

            double basicPrice = GetPassengerPrice(currentRoute.Destination1, currentRoute.Destination2, type);
            double routePrice = currentRoute.getFarePrice(type);

            double priceDiff = basicPrice / routePrice;

            double demand = (double)airportCurrent.getDestinationPassengersRate(airportDestination, type);

            double passengerDemand = demand * GetSeasonFactor(airportDestination) * GetHolidayFactor(airportDestination) * GetHolidayFactor(airportCurrent);

            passengerDemand = passengerDemand * (GameObject.GetInstance().PassengerDemandFactor / 100.0);

            passengerDemand *= GameObject.GetInstance().Difficulty.PassengersLevel;

            if (airliner.Airliner.Airline.MarketFocus == Airline.AirlineFocus.Global && distance > 3000 && airportCurrent.Profile.Country != airportDestination.Profile.Country)
                passengerDemand = passengerDemand * (115 / 100);

            if (airliner.Airliner.Airline.MarketFocus == Airline.AirlineFocus.Regional && distance < 1500)
                passengerDemand = passengerDemand * (115 / 100);

            if (airliner.Airliner.Airline.MarketFocus == Airline.AirlineFocus.Domestic && distance < 1500 && airportDestination.Profile.Country == airportCurrent.Profile.Country)
                passengerDemand = passengerDemand * (115 / 100);

            if (airliner.Airliner.Airline.MarketFocus == Airline.AirlineFocus.Local && distance < 1000)
                passengerDemand = passengerDemand * (115 / 100);

            var routes = Airlines.GetAllAirlines().SelectMany(a => a.Routes.FindAll(r => (r.HasAirliner) && (r.Destination1 == airportCurrent || r.Destination1 == airportDestination) && (r.Destination2 == airportDestination || r.Destination2 == airportCurrent)));

            double flightsPerDay = Convert.ToDouble(routes.Sum(r => r.TimeTable.Entries.Count)) / 7;

            passengerDemand = passengerDemand / flightsPerDay;

            double totalCapacity = routes.Sum(r => r.getAirliners().Max(a => a.Airliner.getTotalSeatCapacity()));

            double capacityPercent = passengerDemand > totalCapacity ? 1 : passengerDemand / totalCapacity;

            Dictionary<Route, double> rations = new Dictionary<Route, double>();

            foreach (Route route in routes)
            {
                double level = route.getServiceLevel(type) / route.getFarePrice(type);

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

            return GetFlightPassengers(airportCurrent, airportDestination,airliner, type);
        }
        //returns the number of passengers for a flight on a stopover route
        public static int GetStopoverFlightPassengers(FleetAirliner airliner, AirlinerClass.ClassType type)
        {
            RouteTimeTableEntry mainEntry = airliner.CurrentFlight.Entry.MainEntry;
            RouteTimeTableEntry entry = airliner.CurrentFlight.Entry;
 
            List<Route> legs = mainEntry.TimeTable.Route.Stopovers.SelectMany(s=>s.Legs).ToList();
              
            Boolean isInbound = mainEntry.DepartureAirport == mainEntry.TimeTable.Route.Destination2;
            //inboound
            if (isInbound)
                legs.Reverse();

            int index = legs.IndexOf(entry.TimeTable.Route);
         
            int passengers = 0;
            for (int i = index; i < legs.Count; i++)
            {
                if (isInbound)
                    passengers += GetFlightPassengers(legs[i].Destination2, legs[i].Destination1, airliner, type);
                else
                    passengers += GetFlightPassengers(legs[i].Destination1, legs[i].Destination2, airliner, type);
               
            }

            return (int)Math.Min(airliner.Airliner.getAirlinerClass(type).SeatingCapacity,passengers);

       
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
        //creates the airport destination passengers a destination
        public static void CreateDestinationPassengers(Airport airport)
        {
            var airports = Airports.GetAirports(a => a != airport && a.Profile.Town != airport.Profile.Town && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) > 50);
            Parallel.ForEach(airports, dAirport =>
            {
                CreateDestinationPassengers(airport, dAirport);
            });
        }
        //creates the airport destinations passenger for all destinations
        public static void CreateDestinationPassengers()
        {
             Parallel.ForEach(Airports.GetAllActiveAirports(), airport =>
            {
              CreateDestinationPassengers(airport);
            });
  
        }
        //creates the airport destinations passengers between two destinations 
        public static void CreateDestinationPassengers(Airport airport, Airport dAirport)
        {
            Array values = Enum.GetValues(typeof(GeneralHelpers.Size));

            double estimatedPassengerLevel = 0;
            Boolean isSameContinent = airport.Profile.Country.Region == dAirport.Profile.Country.Region;
            Boolean isSameCountry = airport.Profile.Country == dAirport.Profile.Country;

            String dAirportSize = dAirport.Profile.Size.ToString();
            String airportSize = airport.Profile.Size.ToString();
            double dist = MathHelpers.GetDistance(dAirport, airport);



            {

                //PLEASE don't change the same country/continent/international values. Most of these were specifically calculated and are not yet calculated 
                //by the program itself! Based largely on US airport system values.
                #region largest airports
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

            #endregion

            }


            #region old else to CreateDestinationPassengers
            {
                //Smallest Airports
                /* if (airportSize.Equals("Smallest") && dAirportSize.Equals("Smallest"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 40;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 30;
                     else
                         estimatedPassengerLevel = 0;
                 }
                 else if (airportSize.Equals("Smallest") && dAirportSize.Equals("Very_small"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 50;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 40;
                     else
                         estimatedPassengerLevel = 0;
                 }
                 else if (airportSize.Equals("Smallest") && dAirportSize.Equals("Small"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 80;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 60;
                     else
                         estimatedPassengerLevel = 0;
                 }
                 else if (airportSize.Equals("Smallest") && dAirportSize.Equals("Medium"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 110;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 80;
                     else
                         estimatedPassengerLevel = 0;
                 }
                 else if (airportSize.Equals("Smallest") && dAirportSize.Equals("Large"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 145;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 110;
                     else
                         estimatedPassengerLevel = 0;
                 }
                 else if (airportSize.Equals("Smallest") && dAirportSize.Equals("Very_large"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 220;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 170;
                     else
                         estimatedPassengerLevel = 0;
                 }
                 else if (airportSize.Equals("Smallest") && dAirportSize.Equals("Largest"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 250;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 190;
                     else
                         estimatedPassengerLevel = 0;
                 }
                 //Very Small Airports
                 if (airportSize.Equals("Very_small") && dAirportSize.Equals("Smallest"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 50;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 40;
                     else
                         estimatedPassengerLevel = 0;
                 }
                 else if (airportSize.Equals("Very_small") && dAirportSize.Equals("Very_small"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 80;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 60;
                     else
                         estimatedPassengerLevel = 0;
                 }
                 else if (airportSize.Equals("Very_small") && dAirportSize.Equals("Small"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 120;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 90;
                     else
                         estimatedPassengerLevel = 0;
                 }
                 else if (airportSize.Equals("Very_small") && dAirportSize.Equals("Medium"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 160;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 120;
                     else
                         estimatedPassengerLevel = 0;
                 }
                 else if (airportSize.Equals("Very_small") && dAirportSize.Equals("Large"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 240;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 180;
                     else
                         estimatedPassengerLevel = 0;
                 }
                 else if (airportSize.Equals("Very_small") && dAirportSize.Equals("Very_large"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 370;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 280;
                     else
                         estimatedPassengerLevel = 0;
                 }
                 else if (airportSize.Equals("Very_small") && dAirportSize.Equals("Largest"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 850;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 640;
                     else
                         estimatedPassengerLevel = 0;
                 }
                 //Small Airports
                 if (airportSize.Equals("Small") && dAirportSize.Equals("Smallest"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 80;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 60;
                     else
                         estimatedPassengerLevel = 0;
                 }
                 else if (airportSize.Equals("Small") && dAirportSize.Equals("Very_small"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 120;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 90;
                     else
                         estimatedPassengerLevel = 0;
                 }
                 else if (airportSize.Equals("Small") && dAirportSize.Equals("Small"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 260;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 200;
                     else
                         estimatedPassengerLevel = 170;
                 }
                 else if (airportSize.Equals("Small") && dAirportSize.Equals("Medium"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 440;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 330;
                     else
                         estimatedPassengerLevel = 290;
                 }
                 else if (airportSize.Equals("Small") && dAirportSize.Equals("Large"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 550;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 410;
                     else
                         estimatedPassengerLevel = 360;
                 }
                 else if (airportSize.Equals("Small") && dAirportSize.Equals("Very_large"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 890;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 670;
                     else
                         estimatedPassengerLevel = 580;
                 }
                 else if (airportSize.Equals("Small") && dAirportSize.Equals("Largest"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 1250;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 940;
                     else
                         estimatedPassengerLevel = 810;
                 }
                 //Medium Airports
                 if (airportSize.Equals("Medium") && dAirportSize.Equals("Smallest"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 110;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 80;
                     else
                         estimatedPassengerLevel = 0;
                 }
                 else if (airportSize.Equals("Medium") && dAirportSize.Equals("Very_small"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 160;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 120;
                     else
                         estimatedPassengerLevel = 0;
                 }
                 else if (airportSize.Equals("Medium") && dAirportSize.Equals("Small"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 440;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 330;
                     else
                         estimatedPassengerLevel = 290;
                 }
                 else if (airportSize.Equals("Medium") && dAirportSize.Equals("Medium"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 660;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 500;
                     else
                         estimatedPassengerLevel = 430;
                 }
                 else if (airportSize.Equals("Medium") && dAirportSize.Equals("Large"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 1040;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 780;
                     else
                         estimatedPassengerLevel = 680;
                 }
                 else if (airportSize.Equals("Medium") && dAirportSize.Equals("Very_large"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 1540;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 1160;
                     else
                         estimatedPassengerLevel = 1000;
                 }
                 else if (airportSize.Equals("Medium") && dAirportSize.Equals("Largest"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 2200;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 1650;
                     else
                         estimatedPassengerLevel = 1430;
                 }
                 //Large Airports
                 if (airportSize.Equals("Large") && dAirportSize.Equals("Smallest"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 145;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 110;
                     else
                         estimatedPassengerLevel = 0;
                 }
                 else if (airportSize.Equals("Large") && dAirportSize.Equals("Very_small"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 240;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 180;
                     else
                         estimatedPassengerLevel = 0;
                 }
                 else if (airportSize.Equals("Large") && dAirportSize.Equals("Small"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 550;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 410;
                     else
                         estimatedPassengerLevel = 360;
                 }
                 else if (airportSize.Equals("Large") && dAirportSize.Equals("Medium"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 1040;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 780;
                     else
                         estimatedPassengerLevel = 680;
                 }
                 else if (airportSize.Equals("Large") && dAirportSize.Equals("Large"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 1900;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 1430;
                     else
                         estimatedPassengerLevel = 1240;
                 }
                 else if (airportSize.Equals("Large") && dAirportSize.Equals("Very_large"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 2790;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 2090;
                     else
                         estimatedPassengerLevel = 1810;
                 }
                 else if (airportSize.Equals("Large") && dAirportSize.Equals("Largest"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 3720;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 2790;
                     else
                         estimatedPassengerLevel = 2420;
                 }
                 //Very Large Airports
                 if (airportSize.Equals("Very_large") && dAirportSize.Equals("Smallest"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 220;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 170;
                     else
                         estimatedPassengerLevel = 0;
                 }
                 else if (airportSize.Equals("Very_large") && dAirportSize.Equals("Very_small"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 370;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 280;
                     else
                         estimatedPassengerLevel = 0;
                 }
                 else if (airportSize.Equals("Very_large") && dAirportSize.Equals("Small"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 890;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 670;
                     else
                         estimatedPassengerLevel = 580;
                 }
                 else if (airportSize.Equals("Very_large") && dAirportSize.Equals("Medium"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 1540;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 1160;
                     else
                         estimatedPassengerLevel = 1000;
                 }
                 else if (airportSize.Equals("Very_large") && dAirportSize.Equals("Large"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 2790;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 2090;
                     else
                         estimatedPassengerLevel = 1810;
                 }
                 else if (airportSize.Equals("Very_large") && dAirportSize.Equals("Very_large"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 3600;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 2700;
                     else
                         estimatedPassengerLevel = 2340;
                 }
                 else if (airportSize.Equals("Very_large") && dAirportSize.Equals("Largest"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 4360;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 3270;
                     else
                         estimatedPassengerLevel = 2830;
                 }
                 //Largest Airports
                 if (airportSize.Equals("Largest") && dAirportSize.Equals("Smallest"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 250;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 190;
                     else
                         estimatedPassengerLevel = 0;
                 }
                 else if (airportSize.Equals("Largest") && dAirportSize.Equals("Very_small"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 850;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 640;
                     else
                         estimatedPassengerLevel = 0;
                 }
                 else if (airportSize.Equals("Largest") && dAirportSize.Equals("Small"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 1250;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 940;
                     else
                         estimatedPassengerLevel = 810;
                 }
                 else if (airportSize.Equals("Largest") && dAirportSize.Equals("Medium"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 2200;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 1650;
                     else
                         estimatedPassengerLevel = 1430;
                 }
                 else if (airportSize.Equals("Largest") && dAirportSize.Equals("Large"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 3720;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 2790;
                     else
                         estimatedPassengerLevel = 2420;
                 }
                 else if (airportSize.Equals("Largest") && dAirportSize.Equals("Very_large"))
                 {
                     if (isSameCountry)
                         estimatedPassengerLevel = 4360;
                     else if (isSameContinent)
                         estimatedPassengerLevel = 3270;
                     else
                         estimatedPassengerLevel = 2830;
                 }
                  else if (airportSize.Equals("Largest") && dAirportSize.Equals("Largest"))
                  {
                      if (isSameCountry)
                          estimatedPassengerLevel = 5490;
                      else if (isSameContinent)
                          estimatedPassengerLevel = 4120;
                      else
                          estimatedPassengerLevel = 3570;
                  }*/
            }
            #endregion

            double value = estimatedPassengerLevel * GetDemandYearFactor(GameObject.GetInstance().GameTime.Year);

            foreach (AirlinerClass.ClassType classType in Enum.GetValues(typeof(AirlinerClass.ClassType)))
            {
                double distance = MathHelpers.GetDistance(airport, dAirport);

                if ((classType == AirlinerClass.ClassType.Economy_Class || classType == AirlinerClass.ClassType.Business_Class) && distance < 7500)
                    value = value / (int)classType;

                ushort rate = (ushort)value;

                if (rate > 0)
                    airport.addDestinationPassengersRate(new DestinationPassengers(classType, dAirport, rate));
            }
        }
        //returns the demand factor based on the year of playing
        private static double GetDemandYearFactor(int year)
        {
            double yearDiff = Convert.ToDouble(year - GameObject.StartYear) / 10;

            return 0.15 * (yearDiff + 1);

        }

    }
}