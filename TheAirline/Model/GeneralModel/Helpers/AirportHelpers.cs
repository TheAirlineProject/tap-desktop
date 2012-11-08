using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirportModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel.WeatherModel;

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

                    airport.Weather[i] = CreateDayWeather(airport, GameObject.GetInstance().GameTime.AddDays(i),i>0 ? airport.Weather[i-1] : null);
                }
            }
            else
            {
                for (int i = 1; i < maxDays; i++)
                    airport.Weather[i - 1] = airport.Weather[i];

                airport.Weather[maxDays - 1] = CreateDayWeather(airport, GameObject.GetInstance().GameTime.AddDays(maxDays - 1),airport.Weather[maxDays-2]);
            }
   
        }
       //creates a new weather object for a specific date based on the weather for another day
        private static Weather CreateDayWeather(Airport airport,DateTime date, Weather previousWeather)
        {
            WeatherAverage average = WeatherAverages.GetWeatherAverage(date.Month, airport);

            if (average != null)
                return CreateDayWeather(date, previousWeather, average);

            Weather.Precipitation[] precipitationValues = (Weather.Precipitation[])Enum.GetValues(typeof(Weather.Precipitation));
            Weather.CloudCover[] coverValues = (Weather.CloudCover[])Enum.GetValues(typeof(Weather.CloudCover));
            Weather.WindDirection[] windDirectionValues = (Weather.WindDirection[])Enum.GetValues(typeof(Weather.WindDirection));
            Weather.eWindSpeed[] windSpeedValues = (Weather.eWindSpeed[])Enum.GetValues(typeof(Weather.eWindSpeed));
            Weather.WindDirection windDirection;
            Weather.eWindSpeed windSpeed;
            double temperature, temperatureLow, temperatureHigh;

            windDirection = windDirectionValues[rnd.Next(windDirectionValues.Length)];
      
            if (previousWeather == null)
            {
                windSpeed = windSpeedValues[rnd.Next(windSpeedValues.Length)];

                double maxTemp = 40;
                double minTemp = -20;

                temperature = rnd.NextDouble() * (maxTemp - minTemp) + minTemp;
             }
            else
            {
                int windIndex = windSpeedValues.ToList().IndexOf(previousWeather.WindSpeed);
                windSpeed = windSpeedValues[rnd.Next(Math.Max(0, windIndex - 2), Math.Min(windIndex + 2, windSpeedValues.Length))];

                double maxTemp = Math.Min(40, previousWeather.Temperature + 5);
                double minTemp = Math.Max(-20, previousWeather.Temperature - 5);

                temperature = rnd.NextDouble() * (maxTemp - minTemp) + minTemp;
            }

            Weather.CloudCover cover = coverValues[rnd.Next(coverValues.Length)];
            Weather.Precipitation precip = Weather.Precipitation.None;
            if (cover == Weather.CloudCover.Overcast)
                precip = precipitationValues[rnd.Next(precipitationValues.Length)];

            temperatureLow = temperature - rnd.Next(1, 10);
            temperatureHigh = temperature + rnd.Next(1, 10);
        
            Weather weather = new Weather(date, windSpeed, windDirection, cover,precip,temperature,temperatureLow,temperatureHigh);

       
            return weather;
   
          }
        //creates the weather from an average
        private static Weather CreateDayWeather(DateTime date, Weather previousWeather, WeatherAverage average)
        {
            Weather.WindDirection[] windDirectionValues = (Weather.WindDirection[])Enum.GetValues(typeof(Weather.WindDirection));
            Weather.eWindSpeed[] windSpeedValues = (Weather.eWindSpeed[])Enum.GetValues(typeof(Weather.eWindSpeed));

            Weather.WindDirection windDirection = windDirectionValues[rnd.Next(windDirectionValues.Length)];
            Weather.CloudCover cover;
            Weather.Precipitation precip = Weather.Precipitation.None;
            Weather.eWindSpeed windSpeed;
            double temperature, temperatureLow, temperatureHigh;

          
            int windIndexMin =  windSpeedValues.ToList().IndexOf(average.WindSpeedMin);
            int windIndexMax = windSpeedValues.ToList().IndexOf(average.WindSpeedMax);

            if (previousWeather == null)
            {
                windSpeed = windSpeedValues[rnd.Next(windIndexMin,windIndexMax)];
      
                temperatureLow = rnd.NextDouble()*((average.TemperatureMin+5)- (average.TemperatureMin - 5))+ (average.TemperatureMin - 5);
                temperatureHigh = rnd.NextDouble() * ((average.TemperatureMax + 5) - Math.Max(average.TemperatureMax - 5, temperatureLow + 1)) + Math.Max(average.TemperatureMax - 5, temperatureLow + 1);
     
            }
            else
            {
                int windIndex = windSpeedValues.ToList().IndexOf(previousWeather.WindSpeed);
                windSpeed = windSpeedValues[rnd.Next(Math.Max(windIndexMin, windIndex - 2), Math.Min(windIndex + 2, windIndexMax))];

                double minTemp = Math.Max(average.TemperatureMin, previousWeather.Temperature - 5);

                temperatureLow = rnd.NextDouble() * ((minTemp + 5) - (minTemp - 5)) + (minTemp - 5);

                double maxTemp = Math.Min(average.TemperatureMax, previousWeather.Temperature + 5);
                temperatureHigh = rnd.NextDouble() * ((maxTemp + 5) - Math.Max(maxTemp - 5,temperatureLow+1)) + Math.Max(maxTemp - 5,temperatureLow+1);
     
      
            }
            temperature = (temperatureLow + temperatureHigh)/2;

            Boolean isOvercast = rnd.Next(100) < average.Precipitation;
            if (isOvercast)
            {
                cover = Weather.CloudCover.Overcast;
                precip = GetPrecipitation(average, temperature);

            }
            else
                cover = rnd.Next(2) == 1 ? Weather.CloudCover.Clear : Weather.CloudCover.Broken;

        
            //weather som weather[24] for temperate hver time, gæt weather Local time
            Weather weather = new Weather(date, windSpeed, windDirection, cover,precip,temperature,temperatureLow,temperatureHigh);

         
            return weather;
        }
        //returns the precipitation for an average at a temperature
        private static Weather.Precipitation GetPrecipitation(WeatherAverage average, double temperature)
        {
            if (temperature > 5)
            {
                Weather.Precipitation[] values = { Weather.Precipitation.Heavy_rain,Weather.Precipitation.Light_rain };
                return values[rnd.Next(values.Length)];

            }
            if (temperature <= 5 && temperature >= -3)
            {
                Weather.Precipitation[] values = {Weather.Precipitation.Freezing_rain,Weather.Precipitation.Hail,Weather.Precipitation.Sleet,Weather.Precipitation.Snow};
                return values[rnd.Next(values.Length)];

            }
            if (temperature < -3)
            {
                return Weather.Precipitation.Snow;
            }
            return Weather.Precipitation.Light_rain;
        }
        //returns if there is bad weather at an airport
        public static Boolean HasBadWeather(Airport airport)
        {
            return false;//airport.Weather[0].WindSpeed == Weather.eWindSpeed.Hurricane || airport.Weather[0].WindSpeed == Weather.eWindSpeed.Violent_Storm;
        }
    }
   
}
