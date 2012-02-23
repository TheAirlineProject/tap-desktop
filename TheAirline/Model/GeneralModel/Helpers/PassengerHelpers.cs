using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel.StatisticsModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.AirlinerModel;

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
            double passengers = Convert.ToDouble(airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Passengers")));
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
        public static void SetPassengerHappiness(Airline airline,double value)
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
        //returns the number of passengers for a flight
        public static int GetFlightPassengers(RouteAirliner airliner, AirlinerClass.ClassType type)
        {
            Airport airportCurrent = Airports.GetAirport(airliner.CurrentPosition);
            Airport airportDestination = airliner.CurrentFlight.Entry.Destination.Airport;

            int totalRoutes1 = airportCurrent.Terminals.getRoutes().Count;
            int totalRoutes2 = airportDestination.Terminals.getRoutes().Count;

            int sameRoutes = 0;
      
            foreach (Route route in airportCurrent.Terminals.getRoutes())
                if (route.Destination1 == airportDestination || route.Destination2 == airportDestination)
                    sameRoutes++;

            int destSize = (int)airportDestination.Profile.Size;
            int deptSize = (int)airportCurrent.Profile.Size;

            int size = (1000 * destSize) + (750 * deptSize);
            size = size / (sameRoutes + 1);
            size = size / totalRoutes1; 
            size = size / totalRoutes2;


            double happiness = GetPassengersHappiness(airliner.Airliner.Airline) > 0 ? GetPassengersHappiness(airliner.Airliner.Airline) : 35.0;

            size = (int)(Convert.ToDouble(size) * happiness / 100.0);

            double minValue = Math.Min(size, airliner.Airliner.Airliner.getAirlinerClass(type).SeatingCapacity)*0.8;

            int value = rnd.Next((int)minValue, Math.Min(Math.Max(10,size), airliner.Airliner.Airliner.getAirlinerClass(type).SeatingCapacity));

            if (airportCurrent.IsHub)
            {
                double hubCoeff = 1.2;
                double dValue = Convert.ToDouble(value) * hubCoeff;
                value = Math.Min((int)dValue, airliner.Airliner.Airliner.getAirlinerClass(type).SeatingCapacity);
            }

            return value;

        }
        //returns the suggested passenger price for a route on a airliner
        public static double GetPassengerPrice(Airport dest1, Airport dest2)
        {

            double fuelConsumption = 0.040;
            double groundTaxPerPassenger = 5;

            double tax = groundTaxPerPassenger; //* airliner.getTotalSeatCapacity() / 2;

            if (dest1.Profile.Country.Name != dest2.Profile.Country.Name)
                tax *= 2;

            double dist = MathHelpers.GetDistance(dest1.Profile.Coordinates, dest2.Profile.Coordinates);

            double fuel = GameObject.GetInstance().FuelPrice * dist * fuelConsumption;

            double expenses = GameObject.GetInstance().FuelPrice * dist * fuelConsumption + (dest2.getLandingFee() + dest1.getLandingFee())/(2*100) + tax;

            return expenses * 2.5;

        }
    }
}