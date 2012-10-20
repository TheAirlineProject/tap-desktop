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
            if (HappinessPercent.ContainsKey(airline))
                HappinessPercent[airline] += 1;
            else
                HappinessPercent.Add(airline, 1);
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

        //returns the number of passengers for a flight
        public static int GetFlightPassengers(FleetAirliner airliner, AirlinerClass.ClassType type)
        {


            Airport airportCurrent = Airports.GetAirport(airliner.CurrentPosition);
            Airport airportDestination = airliner.CurrentFlight.Entry.Destination.Airport;

            var currentRoute = airliner.Routes.Find(r => (r.Destination1 == airportCurrent || r.Destination1 == airportDestination) && (r.Destination2 == airportDestination || r.Destination2 == airportCurrent));

            double basicPrice = GetPassengerPrice(currentRoute.Destination1, currentRoute.Destination2,type);
            double routePrice = currentRoute.getFarePrice(type);

            double priceDiff = basicPrice / routePrice;
            /*
             * If the capacity is less than the demand, fill the airliner and decrease airline happiness. 

If an airline wants to increase its market share on a route that is already at capacity, it would have two options.
             * It could either increase the level of service or more likely lower the price.
             * If the other airlines do not do the same thing, they would begin to loose passengers to the other airline. 
             * The AI would have to be programmed to understand this. 
             * The current prices charged by airlines on the route would to be displayed somewhere so player knows what to charge.*/
            double demand = (double)airportCurrent.getDestinationPassengersRate(airportDestination, type);

            double passengerDemand = demand * GetSeasonFactor(airportDestination) * GetHolidayFactor(airportDestination) * GetHolidayFactor(airportCurrent);

            passengerDemand = passengerDemand * (GameObject.GetInstance().PassengerDemandFactor / 100.0);

            if (airportCurrent.IsHub && GameObject.GetInstance().Difficulty == GameObject.DifficultyLevel.Hard)
            { passengerDemand = passengerDemand * (125 / 100); }

            else if (airportCurrent.IsHub && GameObject.GetInstance().Difficulty == GameObject.DifficultyLevel.Normal)
            { passengerDemand = passengerDemand * (150 / 100); }

            else if (airportCurrent.IsHub && GameObject.GetInstance().Difficulty == GameObject.DifficultyLevel.Easy)
            { passengerDemand = passengerDemand * (175 / 100); }

            var routes = Airlines.GetAllAirlines().SelectMany(a => a.Routes.FindAll(r => (r.HasAirliner) && (r.Destination1 == airportCurrent || r.Destination1 == airportDestination) && (r.Destination2 == airportDestination || r.Destination2 == airportCurrent)));

            double totalCapacity = routes.Sum(r => r.getAirliners().Max(a => a.Airliner.getTotalSeatCapacity()));

            double capacityPercent = passengerDemand > totalCapacity ? 1 : passengerDemand / totalCapacity;

            Dictionary<Route, double> rations = new Dictionary<Route, double>();

            foreach (Route route in routes)
                rations.Add(route, route.getServiceLevel(type) / route.getFarePrice(type));

            double totalRatio = rations.Values.Sum();

            double routeRatioPercent = rations[currentRoute] / totalRatio;

            double routePriceDiff = priceDiff < 0.5 ? priceDiff : 1;
            if (GameObject.GetInstance().Difficulty == GameObject.DifficultyLevel.Hard)
            { routePriceDiff *= 1.2; }
            else if (GameObject.GetInstance().Difficulty == GameObject.DifficultyLevel.Normal)
            { routePriceDiff *= 1.1; }
            else if (GameObject.GetInstance().Difficulty == GameObject.DifficultyLevel.Easy)
            { routePriceDiff *= 1.0; }
                     

            
            double randomPax = Convert.ToDouble(rnd.Next(97, 103))/100;


            return (int)Math.Min(airliner.Airliner.getAirlinerClass(type).SeatingCapacity,(airliner.Airliner.getAirlinerClass(type).SeatingCapacity * routeRatioPercent * capacityPercent * routePriceDiff * randomPax));
            //return (int)(airliner.Airliner.getAirlinerClass(type).SeatingCapacity);


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

            Boolean isSameContinent = dest1.Profile.Country.Region == dest2.Profile.Country.Region;
            Boolean isSameCountry = dest1.Profile.Country == dest2.Profile.Country;

            if (!isSameCountry && !isSameContinent)
                ticketPrice = ticketPrice * 1.9;
            if (!isSameCountry && isSameContinent)
                ticketPrice = ticketPrice * 1.35;

            if (ticketPrice < minimumTicketPrice)
                ticketPrice = minimumTicketPrice + (ticketPrice / 4);

            return ticketPrice;
        }
        public static double GetPassengerPrice(Airport dest1, Airport dest2,AirlinerClass.ClassType type)
        {
           

            return GetPassengerPrice(dest1, dest2) * GeneralHelpers.ClassToPriceFactor(type);
        }
        //creates the airport destination passengers a destination
        public static void CreateDestinationPassengers(Airport airport)
        {
            foreach (Airport dAirport in Airports.GetAirports(a => a != airport && a.Profile.Town != airport.Profile.Town && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) > 50))
            {
                CreateDestinationPassengers(airport, dAirport);
            }
        }
        //creates the airport destinations passenger for all destinations
        public static void CreateDestinationPassengers()
        {
            foreach (Airport airport in Airports.GetAllActiveAirports())
                CreateDestinationPassengers(airport);
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

            //Smallest Airports
            if (airportSize.Equals("Smallest") && dAirportSize.Equals("Smallest"))
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
            }

            if (GameObject.GetInstance().Difficulty == GameObject.DifficultyLevel.Hard)
            { estimatedPassengerLevel *= 1.0; }
            else if (GameObject.GetInstance().Difficulty == GameObject.DifficultyLevel.Normal)
            { estimatedPassengerLevel *= 1.2; }
            else if (GameObject.GetInstance().Difficulty == GameObject.DifficultyLevel.Easy)
            { estimatedPassengerLevel *= 1.5; }

            double value = estimatedPassengerLevel;

            foreach (AirlinerClass.ClassType classType in Enum.GetValues(typeof(AirlinerClass.ClassType)))
            {
                double distance = MathHelpers.GetDistance(airport,dAirport);

                if ((classType == AirlinerClass.ClassType.Economy_Class || classType == AirlinerClass.ClassType.Business_Class) && distance<7500)
                    value = value / (int)classType;


                GeneralHelpers.Rate rate = (GeneralHelpers.Rate)Enum.ToObject(typeof(GeneralHelpers.Rate), (int)value);

                airport.addDestinationPassengersRate(new DestinationPassengers(classType, dAirport, rate));
            }
        }

    }
}