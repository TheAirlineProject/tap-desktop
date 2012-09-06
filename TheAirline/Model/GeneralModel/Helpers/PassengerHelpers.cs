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
            Airports.GetAirports(a => currentAirport!=null && a != currentAirport && !FlightRestrictions.HasRestriction(currentAirport.Profile.Country, a.Profile.Country, GameObject.GetInstance().GameTime, FlightRestriction.RestrictionType.Flights)).ForEach(a => airportsList.Add(a, (int)a.Profile.Size * (a.Profile.Country == currentAirport.Profile.Country ? 7 : 3)));

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

            int passengerDemand = Convert.ToInt16(((int)airportCurrent.getDestinationPassengersRate(airportDestination, type)) * GetSeasonFactor(airportDestination) * GetHolidayFactor(airportDestination) * GetHolidayFactor(airportCurrent));
            
            if (airportCurrent.IsHub)
                passengerDemand = passengerDemand * (150 / 100);

            var routes = airportCurrent.Terminals.getRoutes();

            double passengerCapacity=0;

            if (routes.Count>0)
                passengerCapacity = routes.SelectMany(a => a.getAirliners()).Max(a=>a.Airliner.getTotalSeatCapacity());
      
            double size = passengerDemand - passengerCapacity;
            
            double happiness = GetPassengersHappiness(airliner.Airliner.Airline) > 0 ? GetPassengersHappiness(airliner.Airliner.Airline) : 35.0;

            size = Convert.ToDouble(size) * happiness / 100.0;
            
            int value = Math.Min(Math.Max(10,(int)size), airliner.Airliner.getAirlinerClass(type).SeatingCapacity);

            double price = airliner.CurrentFlight.Entry.TimeTable.Route.getRouteAirlinerClass(type).FarePrice;
            double standardPrice = GetPassengerPrice(airliner.CurrentFlight.Entry.TimeTable.Route.Destination1, airliner.CurrentFlight.Entry.TimeTable.Route.Destination2);

            double priceDiff = (price / standardPrice) * 1.13;
     
            value = Math.Min((int)(Convert.ToDouble(value) / priceDiff),airliner.Airliner.getAirlinerClass(type).SeatingCapacity);

            if (airportCurrent.getDestinationPassengersRate(airportDestination, type) == GeneralHelpers.Rate.None)
                return 0;
            else
             return value;
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

            AirlinerType bestFitAirliner = (from at in AirlinerTypes.GetAllTypes() where at is AirlinerPassengerType && at.Produced.From <= GameObject.GetInstance().GameTime && at.Produced.To > GameObject.GetInstance().GameTime && at.Range>=dist orderby at.Range select at).FirstOrDefault();

            TimeSpan estFlightTime = MathHelpers.GetFlightTime(dest1.Profile.Coordinates,dest2.Profile.Coordinates,bestFitAirliner);

            double cabinWage = FeeTypes.GetType("Cabin wage").DefaultValue;
            double cockpitWage = FeeTypes.GetType("Cockpit wage").DefaultValue;
            double fuelprice = GameObject.GetInstance().FuelPrice;

         
            double crewExpenses = (bestFitAirliner.CockpitCrew * cockpitWage * estFlightTime.TotalHours) + (((AirlinerPassengerType)bestFitAirliner).CabinCrew * cabinWage * estFlightTime.TotalHours);
            double tMax = bestFitAirliner.Range / bestFitAirliner.CruisingSpeed;
            double consumption = (bestFitAirliner.FuelCapacity / tMax / bestFitAirliner.CruisingSpeed)*0.9; //why tMax / speed when tMax is range / speed
            double fuelExpenses = fuelprice * dist * consumption; //why are passenger capacity isn't used here
            double otherCost = ((AirlinerPassengerType)bestFitAirliner).MaxSeatingCapacity * (2.50 + dist * 0.0005);
            Convert.ToDouble(((AirlinerPassengerType)bestFitAirliner).MaxSeatingCapacity);

            double totalExpenses = otherCost + fuelExpenses + crewExpenses;//minimum ticket price
        
            double paxIndex = 3 / Convert.ToDouble(((AirlinerPassengerType)bestFitAirliner).MaxSeatingCapacity);
         
            double ticketPrice = totalExpenses * paxIndex;
            //distance modifiers
            if (dist <= 500) ticketPrice = ticketPrice * 1.5;
            else if (dist > 500 && dist <= 5000) ticketPrice = ticketPrice * 0.8;
            else if (dist > 5000 && dist <= 5600) ticketPrice = ticketPrice * 0.90;
            else if (dist > 5600 && dist <= 6500) ticketPrice = ticketPrice * 1;
            else if (dist > 6500 && dist <= 7000) ticketPrice = ticketPrice * 1.50;
            else if (dist > 7000 && dist <= 9000) ticketPrice = ticketPrice * 1.45;
            else if (dist > 9000 && dist <= 11000) ticketPrice = ticketPrice * 1.40;
            else if (dist > 11000 && dist <= 13000) ticketPrice = ticketPrice * 1.35;
            else if (dist > 13000 && dist <= 15000) ticketPrice = ticketPrice * 1.30;
            else ticketPrice = ticketPrice * 1.25;                    

            return ticketPrice;
        }
        //creates the airport destination passengers a destination
        public static void CreateDestinationPassengers(Airport airport)
        {
            foreach (Airport dAirport in Airports.GetAirports(a => a != airport && a.Profile.Town != airport.Profile.Town && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) > 25))
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

            Boolean isSameContinent = airport.Profile.Country.Region == dAirport.Profile.Country.Region;
            Boolean isSameCountry = airport.Profile.Country == dAirport.Profile.Country;

            int sameContinentCoeff = isSameContinent ? values.Length * 1 : 0;
            int sameCountryCoeff = isSameCountry ? values.Length * 2 : 0;

            int destCoeff = ((int)dAirport.Profile.Size + 1) * 2;
            int deptCoeff = ((int)airport.Profile.Size + 1);

            int rndCoeff = rnd.Next(values.Length);

            int coeff = destCoeff + deptCoeff + sameContinentCoeff + sameCountryCoeff;
            
            int passengerModifier = rnd.Next(50, 125);
            
            double value = ((coeff / 6) * ((coeff / 6) * 101)) * Convert.ToDouble((passengerModifier) / 100);

            GeneralHelpers.Rate rate = (GeneralHelpers.Rate)Enum.ToObject(typeof(GeneralHelpers.Rate), (int)value);

            airport.addDestinationPassengersRate(new DestinationPassengers(dAirport, rate));
        }
      
    }
}