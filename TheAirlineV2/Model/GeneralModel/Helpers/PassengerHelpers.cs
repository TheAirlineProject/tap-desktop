using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirlineV2.Model.AirlineModel;
using TheAirlineV2.Model.GeneralModel.StatisticsModel;
using TheAirlineV2.Model.AirlinerModel.RouteModel;
using TheAirlineV2.Model.AirportModel;
using TheAirlineV2.Model.AirlinerModel;

namespace TheAirlineV2.Model.GeneralModel
{
    //the helper class for the passengers
    public class PassengerHelpers
    {
        private static Dictionary<Airline, double> HappinessPercent = new Dictionary<Airline, double>();
        private static Random rnd = new Random();
        //returns the passengers happiness for an airline
        public static double GetPassengersHappiness(Airline airline)
        {
            double passengers = Convert.ToDouble(airline.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Passengers")));

            if (HappinessPercent.ContainsKey(airline))
                return HappinessPercent[airline] / passengers * 100.0;
            else
                return 0;
        }
        //adds happiness to an airline
        public static void AddPassengerHappiness(Airline airline)
        {
            if (HappinessPercent.ContainsKey(airline))
                HappinessPercent[airline] += 1;
            else
                HappinessPercent.Add(airline, 1);
        }
        //returns the number of passengers for a flight
        public static int GetFlightPassengers(RouteAirliner airliner, AirlinerClass.ClassType type)
        {
            Airport airportCurrent = Airports.GetAirport(airliner.CurrentPosition);
            Airport airportDestination = airliner.CurrentFlight.Entry.Destination.Airport;

            int totalRoutes1 = airportCurrent.Gates.getRoutes().Count;
            int totalRoutes2 = airportDestination.Gates.getRoutes().Count;

            int sameRoutes = 0;
      
            foreach (Route route in airportCurrent.Gates.getRoutes())
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