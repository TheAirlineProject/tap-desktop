using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirportModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirlineModel;

namespace TheAirline.Model.GeneralModel.Helpers
{
    //the class for some general airport helpers
    public class AirportHelpers
    {
        private static Random rnd = new Random();
        //finds all airports in a radius of 1000 km from a airport
        public static List<Airport> GetAirportsNearAirport(Airport airport)
        {
            return Airports.GetAirports(a => MathHelpers.GetDistance(airport.Profile.Coordinates, a.Profile.Coordinates) < 1000 && airport != a);
        }
        //returns all routes from an airport for an airline
        public static List<Route> GetAirportRoutes(Airport airport, Airline airline)
        {
            return airline.Routes.FindAll(r => r.Destination2 == airport || r.Destination1 == airport);
        }
        //returns all routes from an airport
        public static List<Route> GetAirportRoutes(Airport airport)
        {
            var routes = Airlines.GetAllAirlines().SelectMany(a => a.Routes).Where(r => r.Destination1 == airport || r.Destination2 == airport);

            return routes.ToList();
        }
        //returns all entries for a specific airport with take off in a time span for a day
        public static List<RouteTimeTableEntry> GetAirportTakeoffs(Airport airport, DayOfWeek day, TimeSpan startTime, TimeSpan endTime)
        {
            return GetAirportRoutes(airport).SelectMany(r => r.TimeTable.Entries.FindAll(e => e.Airliner != null && e.DepartureAirport == airport && e.Time >= startTime && e.Time < endTime && e.Day == day)).ToList();
        }
        //returns all entries for a specific airport with landings in a time span for a day
        public static List<RouteTimeTableEntry> GetAirportLandings(Airport airport, DayOfWeek day, TimeSpan startTime, TimeSpan endTime)
        {
            return GetAirportRoutes(airport).SelectMany(r => r.TimeTable.Entries.FindAll(e => e.Airliner != null && e.Destination.Airport == airport && e.Time.Add(MathHelpers.GetFlightTime(e.Destination.Airport.Profile.Coordinates, e.DepartureAirport.Profile.Coordinates, e.Airliner.Airliner.Type)) >= startTime && e.Time.Add(MathHelpers.GetFlightTime(e.Destination.Airport.Profile.Coordinates, e.DepartureAirport.Profile.Coordinates, e.Airliner.Airliner.Type)) < endTime && e.Day == day)).ToList();
        }
        //creates the weather (5 days) for an airport
        public static void CreateAirportWeather(Airport airport)
        {
            int maxDays = 5;
            if (airport.Weather[0] == null)
            {
                for (int i = 0; i < maxDays; i++)
                {

                    airport.Weather[i] = CreateDayWeather(GameObject.GetInstance().GameTime.AddDays(i),i>0 ? airport.Weather[i-1] : null);
                }
            }
            else
            {
                for (int i = 1; i < maxDays; i++)
                    airport.Weather[i - 1] = airport.Weather[i];

                airport.Weather[maxDays - 1] = CreateDayWeather(GameObject.GetInstance().GameTime.AddDays(maxDays - 1),airport.Weather[maxDays-2]);
            }
   
        }
       //creates a new weather object for a specific date based on the weather for another day
        private static Weather CreateDayWeather(DateTime date, Weather previousWeather)
        {
            Weather.WindDirection[] windDirectionValues = (Weather.WindDirection[])Enum.GetValues(typeof(Weather.WindDirection));
            Weather.eWindSpeed[] windSpeedValues = (Weather.eWindSpeed[])Enum.GetValues(typeof(Weather.eWindSpeed));
            Weather.WindDirection windDirection;
            Weather.eWindSpeed windSpeed;

            windDirection = windDirectionValues[rnd.Next(windDirectionValues.Length)];
      
            if (previousWeather == null)
            {
                windSpeed = windSpeedValues[rnd.Next(windSpeedValues.Length)];
             }
            else
            {
                int windIndex = windSpeedValues.ToList().IndexOf(previousWeather.WindSpeed);
                windSpeed = windSpeedValues[rnd.Next(Math.Max(0, windIndex - 2), Math.Min(windIndex + 2, windSpeedValues.Length))];
            }
            return new Weather(date, windSpeed, windDirection);
   

        }
        //returns if there is bad weather at an airport
        public static Boolean HasBadWeather(Airport airport)
        {
            return airport.Weather[0].WindSpeed == Weather.eWindSpeed.Hurricane || airport.Weather[0].WindSpeed == Weather.eWindSpeed.Violent_Storm;
        }
    }
   
}
