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

            int totalRoutes1 = airportCurrent.Terminals.getRoutes().Count;
            int totalRoutes2 = airportDestination.Terminals.getRoutes().Count;

            if (totalRoutes1 == 0 || totalRoutes2 == 0)
            {
                string s = "";
                s = airportCurrent.Profile.Name;
            }

            int sameRoutes = 0;
      
            foreach (Route route in airportCurrent.Terminals.getRoutes())
                if (route.Destination1 == airportDestination || route.Destination2 == airportDestination)
                    sameRoutes++;

            int destPassengers = (int)airportCurrent.getDestinationPassengersRate(airportDestination, type);

            double size = (20000 * destPassengers * GetSeasonFactor(airportDestination));// + (750 * deptSize * GetSeasonFactor(airportCurrent));
            size = size / (sameRoutes + 1);
            size = size / totalRoutes1; 
            size = size / totalRoutes2;
        
            double happiness = GetPassengersHappiness(airliner.Airliner.Airline) > 0 ? GetPassengersHappiness(airliner.Airliner.Airline) : 35.0;

            size = Convert.ToDouble(size) * happiness / 100.0;

            double minValue = Math.Min(size, airliner.Airliner.getAirlinerClass(type).SeatingCapacity)*0.8;
            
            int value = rnd.Next((int)minValue, Math.Min(Math.Max(10,(int)size), airliner.Airliner.getAirlinerClass(type).SeatingCapacity));

            if (airportCurrent.IsHub)
            {
                double hubCoeff = 1.5;
                double dValue = Convert.ToDouble(value) * hubCoeff;
                value = Math.Min((int)dValue, airliner.Airliner.getAirlinerClass(type).SeatingCapacity);
            }

            double price = airliner.CurrentFlight.Entry.TimeTable.Route.getRouteAirlinerClass(type).FarePrice;
            double standardPrice = GetPassengerPrice(airliner.CurrentFlight.Entry.TimeTable.Route.Destination1, airliner.CurrentFlight.Entry.TimeTable.Route.Destination2);

            double priceDiff = (price / standardPrice) * 1.13;
     
            value = Math.Min((int)(Convert.ToDouble(value) / priceDiff),airliner.Airliner.getAirlinerClass(type).SeatingCapacity);

            if (value < 15)
                value = rnd.Next(value, 15);

            if (airportCurrent.getDestinationPassengersRate(airportDestination, type) == GeneralHelpers.Rate.None)
                return 0;
            else
             return value;

        


        }
         
        //returns the season factor for an airport
        private static double GetSeasonFactor(Airport airport)
        {
            Boolean isSummer = GameObject.GetInstance().GameTime.Month >= 3 && GameObject.GetInstance().GameTime.Month < 9;

            if (airport.Profile.Season == Weather.Season.All_Year)
                return 1;
            if (airport.Profile.Season == Weather.Season.Summer)
                if (isSummer) return 1.5;
                else return 0.5;
            if (airport.Profile.Season == Weather.Season.Winter)
                if (isSummer) return 0.5;
                else return 1.5;

            return 1;
        }
        //returns the suggested passenger price for a route - new
        public static double GetPassengerPrice(Airport dest1, Airport dest2)
        {
            double dist = MathHelpers.GetDistance(dest1, dest2);

            AirlinerType bestFitAirliner = (from at in AirlinerTypes.GetAllTypes() where at.Produced.From <= GameObject.GetInstance().GameTime && at.Produced.To > GameObject.GetInstance().GameTime && at.Range>=dist orderby at.Range select at).FirstOrDefault();

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
            if (dist < 500) ticketPrice = ticketPrice * 1.5;
            else
               if (dist < 5000) ticketPrice = ticketPrice * 0.8;
              else
                  if (dist < 5600) ticketPrice = ticketPrice * 0.90;
                  else
                    if (dist < 6500) ticketPrice = ticketPrice * 1;
                    else
                        if (dist < 7000) ticketPrice = ticketPrice * 1.50;
                        else
                            if (dist < 9000) ticketPrice = ticketPrice * 1.45;
                            else
                                if (dist < 11000) ticketPrice = ticketPrice * 1.40;
                                else
                                    if (dist < 13000) ticketPrice = ticketPrice * 1.35;
                                    else
                                        if (dist < 15000) ticketPrice = ticketPrice * 1.30;                
       
         

            return ticketPrice;
            
           
        }
        //returns the suggested passenger price for a route on a airliner
        public static double GetPassengerPriceOld(Airport dest1, Airport dest2)
        {
            
            double fuelConsumption = AirlinerTypes.GetAllTypes().Max(t => t.FuelConsumption);
            double groundTaxPerPassenger = GeneralHelpers.GetInflationPrice(5);

            double tax = groundTaxPerPassenger;

            if (dest1.Profile.Country.Name != dest2.Profile.Country.Name)
                tax *= 2;

            double dist = MathHelpers.GetDistance(dest1.Profile.Coordinates, dest2.Profile.Coordinates);

            double fuel = GameObject.GetInstance().FuelPrice * dist * fuelConsumption;

            double expenses = GameObject.GetInstance().FuelPrice * dist * fuelConsumption + (dest2.getLandingFee() + dest1.getLandingFee()) / (2 * 100) + tax;

            return expenses * 2.5;

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

            int value = coeff / 6;


            GeneralHelpers.Rate rate = (GeneralHelpers.Rate)Enum.ToObject(typeof(GeneralHelpers.Rate), value);


            airport.addDestinationPassengersRate(new DestinationPassengers(dAirport, rate));
        }
      
    }
}