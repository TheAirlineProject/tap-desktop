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
        //returns the longest distance between airports in a match
        public static double GetLongestDistance(Predicate<Airport> match)
        {
            var airports1 = Airports.GetAllAirports(match);
            var airports2 = Airports.GetAllAirports(match);

            double maxDistance = Double.MinValue;
           
            foreach (Airport airport1 in airports1)
            {
                double distance = airports2.Where(a=>a!=airport1).Max(a=>MathHelpers.GetDistance(a,airport1));

                if (distance >= maxDistance)
                    maxDistance = distance;
            }
               
            return maxDistance;
        }
        //returns the shortest distance between airports in a match
        public static double GetShortestDistance(Predicate<Airport> match)
        {
            var airports1 = Airports.GetAllAirports(match);
            var airports2 = Airports.GetAllAirports(match);

            double minDistance = Double.MaxValue;

            foreach (Airport airport1 in airports1)
            {
                double distance = airports2.Where(a => a != airport1).Max(a => MathHelpers.GetDistance(a, airport1));

                if (distance <= minDistance)
                    minDistance = distance;
            }

            return minDistance;
        }
        //returns the price for a runway at an airport
        public static double GetAirportRunwayPrice(Airport airport, long lenght)
        {
            double pricePerMeter = 0;
            if (airport.Profile.Size == GeneralHelpers.Size.Very_large || airport.Profile.Size == GeneralHelpers.Size.Largest)
                pricePerMeter = 30000;
            if (airport.Profile.Size == GeneralHelpers.Size.Large || airport.Profile.Size == GeneralHelpers.Size.Medium)
                pricePerMeter = 24000;
            if (airport.Profile.Size == GeneralHelpers.Size.Small)
                pricePerMeter = 18000;
            if (airport.Profile.Size == GeneralHelpers.Size.Smallest || airport.Profile.Size == GeneralHelpers.Size.Very_small)
                pricePerMeter = 12000;

            return pricePerMeter * lenght;
        }
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
        //returns all routes between two airports
        public static List<Route> GetAirportRoutes(Airport airport1, Airport airport2)
        {
            var routes = Airlines.GetAllAirlines().SelectMany(a => a.Routes).Where(r => (r.Destination1 == airport1 && r.Destination2 == airport2) || (r.Destination1 == airport2 && r.Destination2 == airport1));

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
        //creates the weather (5 days) for a number of airport with an average
        public static void CreateAirportsWeather(List<Airport> airports, WeatherAverage average)
        {
            if (airports.Count > 0)
            {
                int maxDays = 5;
                Weather[] weathers = new Weather[maxDays];

                if (airports[0].Weather[0] == null)
                {
                    for (int i = 0; i < maxDays; i++)
                        weathers[i] = CreateDayWeather(GameObject.GetInstance().GameTime.AddDays(i), i > 0 ? weathers[i - 1] : null, average);

                }
                else
                {
                    for (int i = 1; i < maxDays; i++)
                        weathers[i - 1] = airports[0].Weather[i];

                    weathers[maxDays - 1] = CreateDayWeather(GameObject.GetInstance().GameTime.AddDays(maxDays - 1), weathers[maxDays - 2], average);

                }

                foreach (var airport in airports)
                    airport.Weather = weathers;
            }
        }
        //creates the weather (5 days) for an airport
        public static void CreateAirportWeather(Airport airport)
        {
            int maxDays = 5;
            if (airport.Weather[0] == null)
            {
                for (int i = 0; i < maxDays; i++)
                {

                    airport.Weather[i] = CreateDayWeather(airport, GameObject.GetInstance().GameTime.AddDays(i), i > 0 ? airport.Weather[i - 1] : null);
                }
            }
            else
            {
                for (int i = 1; i < maxDays; i++)
                    airport.Weather[i - 1] = airport.Weather[i];

                airport.Weather[maxDays - 1] = CreateDayWeather(airport, GameObject.GetInstance().GameTime.AddDays(maxDays - 1), airport.Weather[maxDays - 2]);
            }

        }
        
        //creates a new weather object for a specific date based on the weather for another day
        private static Weather CreateDayWeather(Airport airport, DateTime date, Weather previousWeather)
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
            double temperature, temperatureLow, temperatureHigh, temperatureSunrise, temperatureSunset, temperatureDayend;

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

                double previousTemperature = (previousWeather.TemperatureHigh + previousWeather.TemperatureLow) / 2;

                double maxTemp = Math.Min(40, previousTemperature + 5);
                double minTemp = Math.Max(-20, previousTemperature - 5);

                temperature = rnd.NextDouble() * (maxTemp - minTemp) + minTemp;
            }

            temperatureLow = temperature - rnd.Next(1, 10);
            temperatureHigh = temperature + rnd.Next(1, 10);

            double tempDiff = temperatureHigh - temperatureLow;
            temperatureSunrise = temperatureLow + MathHelpers.GetRandomDoubleNumber(-2, Math.Min(tempDiff, 2));
            temperatureSunset = temperatureHigh - MathHelpers.GetRandomDoubleNumber(-2, Math.Min(tempDiff, 2));
            temperatureDayend = temperatureLow + rnd.Next(-2, 2);

            Weather.CloudCover cover = coverValues[rnd.Next(coverValues.Length)];
            Weather.Precipitation precip = Weather.Precipitation.None;
            if (cover == Weather.CloudCover.Overcast)
                precip = precipitationValues[rnd.Next(precipitationValues.Length)];

           
            HourlyWeather[] hourlyTemperature = new HourlyWeather[24];

            if (previousWeather == null)
                hourlyTemperature[0] = new HourlyWeather(temperatureLow, cover, cover == Weather.CloudCover.Overcast ? GetPrecipitation(temperatureLow) : Weather.Precipitation.None, windSpeed, windDirection);
            else
                hourlyTemperature[0] = previousWeather.Temperatures[previousWeather.Temperatures.Length - 1];

            double morningSteps = (temperatureSunrise - hourlyTemperature[0].Temperature) / (Weather.Sunrise - 1);

            for (int i = 1; i <= Weather.Sunrise; i++)
            {
                double temp = hourlyTemperature[i - 1].Temperature + morningSteps;
                Weather.CloudCover hourlyCover = rnd.Next(3) == 0 ? coverValues[rnd.Next(coverValues.Length)] : cover;

                int windspeedIndex = windSpeedValues.ToList().IndexOf(windSpeed);
                Weather.eWindSpeed[] hourlyWindspeedValues = new Weather.eWindSpeed[] { windSpeed, windSpeed, windSpeed, hourlyTemperature[i - 1].WindSpeed, windspeedIndex > 0 ? (Weather.eWindSpeed)windspeedIndex - 1 : (Weather.eWindSpeed)windspeedIndex + 1, windspeedIndex < windSpeedValues.Length - 1 ? (Weather.eWindSpeed)windspeedIndex + 1 : (Weather.eWindSpeed)windspeedIndex - 1 };
                Weather.eWindSpeed hourlyWindspeed = hourlyWindspeedValues[rnd.Next(hourlyWindspeedValues.Length)];

                hourlyTemperature[i] = new HourlyWeather(temp, hourlyCover, hourlyCover == Weather.CloudCover.Overcast ? GetPrecipitation(temp) : Weather.Precipitation.None, hourlyWindspeed, windDirection);
            }

            double daySteps = (temperatureSunset - temperatureSunrise) / (Weather.Sunset - Weather.Sunrise - 1);

            for (int i = Weather.Sunrise + 1; i < Weather.Sunset; i++)
            {
                 Weather.CloudCover hourlyCover = rnd.Next(3) == 0 ? coverValues[rnd.Next(coverValues.Length)] : cover;

                double temp = hourlyTemperature[i - 1].Temperature + daySteps;
                if (hourlyCover != hourlyTemperature[i - 1].Cover && hourlyCover == Weather.CloudCover.Overcast)
                    temp -= MathHelpers.GetRandomDoubleNumber(1, 4);
                 if (hourlyCover != hourlyTemperature[i-1].Cover && hourlyTemperature[i-1].Cover == Weather.CloudCover.Overcast)
                     temp += MathHelpers.GetRandomDoubleNumber(1, 4);
            

                int windspeedIndex = windSpeedValues.ToList().IndexOf(windSpeed);
                Weather.eWindSpeed[] hourlyWindspeedValues = new Weather.eWindSpeed[] { windSpeed, windSpeed, windSpeed, hourlyTemperature[i - 1].WindSpeed, windspeedIndex > 0 ? (Weather.eWindSpeed)windspeedIndex - 1 : (Weather.eWindSpeed)windspeedIndex + 1, windspeedIndex < windSpeedValues.Length - 1 ? (Weather.eWindSpeed)windspeedIndex + 1 : (Weather.eWindSpeed)windspeedIndex - 1 };
                Weather.eWindSpeed hourlyWindspeed = hourlyWindspeedValues[rnd.Next(hourlyWindspeedValues.Length)];

                hourlyTemperature[i] = new HourlyWeather(temp, hourlyCover, hourlyCover == Weather.CloudCover.Overcast ? GetPrecipitation(temp) : Weather.Precipitation.None, hourlyWindspeed, windDirection);

            }

            double eveningSteps = (temperatureDayend - temperatureSunset) / (hourlyTemperature.Length - Weather.Sunset);

            for (int i = Weather.Sunset; i < hourlyTemperature.Length; i++)
            {
                double temp = hourlyTemperature[i - 1].Temperature + eveningSteps;
                Weather.CloudCover hourlyCover = rnd.Next(3) == 0 ? coverValues[rnd.Next(coverValues.Length)] : cover;

                int windspeedIndex = windSpeedValues.ToList().IndexOf(windSpeed);
                Weather.eWindSpeed[] hourlyWindspeedValues = new Weather.eWindSpeed[] { windSpeed, windSpeed, windSpeed, hourlyTemperature[i - 1].WindSpeed, windspeedIndex > 0 ? (Weather.eWindSpeed)windspeedIndex - 1 : (Weather.eWindSpeed)windspeedIndex + 1, windspeedIndex < windSpeedValues.Length - 1 ? (Weather.eWindSpeed)windspeedIndex + 1 : (Weather.eWindSpeed)windspeedIndex - 1 };
                Weather.eWindSpeed hourlyWindspeed = hourlyWindspeedValues[rnd.Next(hourlyWindspeedValues.Length)];

                hourlyTemperature[i] = new HourlyWeather(temp, hourlyCover, hourlyCover == Weather.CloudCover.Overcast ? GetPrecipitation(temp) : Weather.Precipitation.None, hourlyWindspeed, windDirection);

            }

            temperatureLow = hourlyTemperature.Min(t => t.Temperature);
            temperatureHigh = hourlyTemperature.Max(t => t.Temperature);
            cover = (from c in hourlyTemperature group c by c.Cover into g select new { Cover = g.Key, Qty = g.Count() }).OrderByDescending(g => g.Qty).First().Cover;
            precip = (from c in hourlyTemperature group c by c.Precip into g select new { Precip = g.Key, Qty = g.Count() }).OrderByDescending(g => g.Qty).First().Precip;
          

            Weather weather = new Weather(date, windSpeed, windDirection, cover, precip, hourlyTemperature, temperatureLow, temperatureHigh);


            return weather;

        }
         
        //creates the weather from an average
        private static Weather CreateDayWeather(DateTime date, Weather previousWeather, WeatherAverage average)
        {
            
            Weather.WindDirection[] windDirectionValues = (Weather.WindDirection[])Enum.GetValues(typeof(Weather.WindDirection));
            Weather.eWindSpeed[] windSpeedValues = (Weather.eWindSpeed[])Enum.GetValues(typeof(Weather.eWindSpeed));
            Weather.CloudCover[] coverValues = (Weather.CloudCover[])Enum.GetValues(typeof(Weather.CloudCover));

            Weather.WindDirection windDirection = windDirectionValues[rnd.Next(windDirectionValues.Length)];
            Weather.CloudCover cover;
            Weather.Precipitation precip = Weather.Precipitation.None;
            Weather.eWindSpeed windSpeed;
            double temperature, temperatureLow, temperatureHigh, temperatureSunrise, temperatureSunset, temperatureDayend;

            int windIndexMin = windSpeedValues.ToList().IndexOf(average.WindSpeedMin);
            int windIndexMax = windSpeedValues.ToList().IndexOf(average.WindSpeedMax);

            if (previousWeather == null)
            {
                windSpeed = windSpeedValues[rnd.Next(windIndexMin, windIndexMax)];
                
                temperatureLow = rnd.NextDouble() * ((average.TemperatureMin + 5) - (average.TemperatureMin - 5)) + (average.TemperatureMin - 5);
                temperatureHigh = rnd.NextDouble() * ((average.TemperatureMax + 5) - Math.Max(average.TemperatureMax - 5, temperatureLow + 1)) + Math.Max(average.TemperatureMax - 5, temperatureLow + 1);

            }
            else
            {
                double previousTemperature = (previousWeather.TemperatureHigh + previousWeather.TemperatureLow) / 2;
                int windIndex = windSpeedValues.ToList().IndexOf(previousWeather.WindSpeed);
                windSpeed = windSpeedValues[rnd.Next(Math.Max(windIndexMin, windIndex - 2), Math.Min(windIndex + 2, windIndexMax))];

                double minTemp = Math.Max(average.TemperatureMin, previousTemperature - 5);
                temperatureLow = rnd.NextDouble() * ((minTemp + 5) - (minTemp - 5)) + (minTemp - 5);

                double maxTemp = Math.Min(average.TemperatureMax, previousTemperature + 5);

                temperatureHigh = MathHelpers.GetRandomDoubleNumber(Math.Max(maxTemp - 5, temperatureLow), maxTemp + 5);//rnd.NextDouble() * ((maxTemp + 5) - Math.Max(maxTemp - 5, temperatureLow + 2)) + Math.Max(maxTemp - 5, temperatureLow + 2);



            }

            double tempDiff = temperatureHigh - temperatureLow;
            temperatureSunrise = temperatureLow + MathHelpers.GetRandomDoubleNumber(-2, Math.Min(tempDiff, 2));
            temperatureSunset = temperatureHigh - MathHelpers.GetRandomDoubleNumber(-2, Math.Min(tempDiff, 2));
            temperatureDayend = temperatureLow + rnd.Next(-2, 2);

            temperature = (temperatureLow + temperatureHigh) / 2;

            Boolean isOvercast = rnd.Next(100) < average.Precipitation;
            if (isOvercast)
            {
                cover = Weather.CloudCover.Overcast;
                precip = GetPrecipitation(temperature);

            }
            else
                cover = rnd.Next(2) == 1 ? Weather.CloudCover.Clear : Weather.CloudCover.Broken;

            HourlyWeather[] hourlyTemperature = new HourlyWeather[24];

            if (previousWeather == null)
                hourlyTemperature[0] = new HourlyWeather(temperatureLow, cover, cover == Weather.CloudCover.Overcast ? GetPrecipitation(temperatureLow) : Weather.Precipitation.None, windSpeed, windDirection);
            else
                hourlyTemperature[0] = previousWeather.Temperatures[previousWeather.Temperatures.Length - 1];

            double morningSteps = (temperatureSunrise - hourlyTemperature[0].Temperature) / (Weather.Sunrise - 1);

            for (int i = 1; i <= Weather.Sunrise; i++)
            {
                double temp = hourlyTemperature[i - 1].Temperature + morningSteps;
                Weather.CloudCover hourlyCover = rnd.Next(3) == 0 ? coverValues[rnd.Next(coverValues.Length)] : cover;

                int windspeedIndex = windSpeedValues.ToList().IndexOf(windSpeed);
                Weather.eWindSpeed[] hourlyWindspeedValues = new Weather.eWindSpeed[] { windSpeed, windSpeed, windSpeed, hourlyTemperature[i - 1].WindSpeed, windspeedIndex > 0 ? (Weather.eWindSpeed)windspeedIndex - 1 : (Weather.eWindSpeed)windspeedIndex + 1, windspeedIndex < windSpeedValues.Length - 1 ? (Weather.eWindSpeed)windspeedIndex + 1 : (Weather.eWindSpeed)windspeedIndex - 1 };
                Weather.eWindSpeed hourlyWindspeed = hourlyWindspeedValues[rnd.Next(hourlyWindspeedValues.Length)];

                hourlyTemperature[i] = new HourlyWeather(temp, hourlyCover, hourlyCover == Weather.CloudCover.Overcast ? GetPrecipitation(temp) : Weather.Precipitation.None, hourlyWindspeed, windDirection);
            }

            double daySteps = (temperatureSunset - temperatureSunrise) / (Weather.Sunset - Weather.Sunrise - 1);

            for (int i = Weather.Sunrise + 1; i < Weather.Sunset; i++)
            {
                Weather.CloudCover hourlyCover = rnd.Next(3) == 0 ? coverValues[rnd.Next(coverValues.Length)] : cover;

                double temp = hourlyTemperature[i - 1].Temperature + daySteps;
                if (hourlyCover != hourlyTemperature[i - 1].Cover && hourlyCover == Weather.CloudCover.Overcast)
                    temp -= MathHelpers.GetRandomDoubleNumber(1,4);
                if (hourlyCover != hourlyTemperature[i - 1].Cover && hourlyTemperature[i - 1].Cover == Weather.CloudCover.Overcast)
                    temp += MathHelpers.GetRandomDoubleNumber(1, 4);

                int windspeedIndex = windSpeedValues.ToList().IndexOf(windSpeed);
                Weather.eWindSpeed[] hourlyWindspeedValues = new Weather.eWindSpeed[] { windSpeed, windSpeed, windSpeed, hourlyTemperature[i - 1].WindSpeed, windspeedIndex > 0 ? (Weather.eWindSpeed)windspeedIndex - 1 : (Weather.eWindSpeed)windspeedIndex + 1, windspeedIndex < windSpeedValues.Length - 1 ? (Weather.eWindSpeed)windspeedIndex + 1 : (Weather.eWindSpeed)windspeedIndex - 1 };
                Weather.eWindSpeed hourlyWindspeed = hourlyWindspeedValues[rnd.Next(hourlyWindspeedValues.Length)];

                hourlyTemperature[i] = new HourlyWeather(temp, hourlyCover, hourlyCover == Weather.CloudCover.Overcast ? GetPrecipitation(temp) : Weather.Precipitation.None, hourlyWindspeed, windDirection);

            }

            double eveningSteps = (temperatureDayend - temperatureSunset) / (hourlyTemperature.Length - Weather.Sunset);

            for (int i = Weather.Sunset; i < hourlyTemperature.Length; i++)
            {
                double temp = hourlyTemperature[i - 1].Temperature + eveningSteps;
                Weather.CloudCover hourlyCover = rnd.Next(3) == 0 ? coverValues[rnd.Next(coverValues.Length)] : cover;

                int windspeedIndex = windSpeedValues.ToList().IndexOf(windSpeed);
                Weather.eWindSpeed[] hourlyWindspeedValues = new Weather.eWindSpeed[] { windSpeed, windSpeed, windSpeed, hourlyTemperature[i - 1].WindSpeed, windspeedIndex > 0 ? (Weather.eWindSpeed)windspeedIndex - 1 : (Weather.eWindSpeed)windspeedIndex + 1, windspeedIndex < windSpeedValues.Length - 1 ? (Weather.eWindSpeed)windspeedIndex + 1 : (Weather.eWindSpeed)windspeedIndex - 1 };
                Weather.eWindSpeed hourlyWindspeed = hourlyWindspeedValues[rnd.Next(hourlyWindspeedValues.Length)];

                hourlyTemperature[i] = new HourlyWeather(temp, hourlyCover, hourlyCover == Weather.CloudCover.Overcast ? GetPrecipitation(temp) : Weather.Precipitation.None, hourlyWindspeed, windDirection);

            }
            temperatureLow = hourlyTemperature.Min(t => t.Temperature);
            temperatureHigh = hourlyTemperature.Max(t => t.Temperature);

            cover = (from c in hourlyTemperature group c by c.Cover into g select new { Cover = g.Key, Qty = g.Count() }).OrderByDescending(g=>g.Qty).First().Cover;
            precip = (from c in hourlyTemperature group c by c.Precip into g select new { Precip = g.Key, Qty = g.Count() }).OrderByDescending(g => g.Qty).First().Precip;
          
            Weather weather = new Weather(date, windSpeed, windDirection, cover, precip, hourlyTemperature, temperatureLow, temperatureHigh);


            return weather;
        }
        //returns the precipitation for a temperature
        private static Weather.Precipitation GetPrecipitation(double temperature)
        {
            if (temperature > 5)
            {
                Weather.Precipitation[] values = { Weather.Precipitation.Heavy_rain, Weather.Precipitation.Light_rain };
                return values[rnd.Next(values.Length)];

            }
            if (temperature <= 5 && temperature >= -3)
            {
                Weather.Precipitation[] values = { Weather.Precipitation.Freezing_rain, Weather.Precipitation.Hail, Weather.Precipitation.Sleet, Weather.Precipitation.Light_snow };
                return values[rnd.Next(values.Length)];

            }
            if (temperature < -3)
            {
                Weather.Precipitation[] values = { Weather.Precipitation.Heavy_snow, Weather.Precipitation.Light_snow };
                return values[rnd.Next(values.Length)];

            }
            return Weather.Precipitation.Light_rain;
        }
        //returns if there is bad weather at an airport
        public static Boolean HasBadWeather(Airport airport)
        {
            return false;//airport.Weather[0].WindSpeed == Weather.eWindSpeed.Hurricane || airport.Weather[0].WindSpeed == Weather.eWindSpeed.Violent_Storm;
        }
        //checks an airport for new gates
        public static void CheckForExtendAirport(Airport airport)
        {
           
            if (airport.Terminals.getOrdereredGates() == 0)
            {
                Boolean newTerminal = true;
                int numberOfGates = airport.Terminals.getTerminals()[0].Gates.NumberOfDeliveredGates;

                int daysToBuild = numberOfGates * 10 + (newTerminal ? 60 : 0);

                long price = numberOfGates * airport.getTerminalGatePrice() + (newTerminal ? airport.getTerminalPrice() : 0);

                if (airport.Income > price)
                {

                    Terminal terminal = new Terminal(airport, null, "Terminal", numberOfGates, GameObject.GetInstance().GameTime.Add(new TimeSpan(daysToBuild, 0, 0, 0)));

                    airport.addTerminal(terminal);
                    airport.Income -= price;
                }
            }
           
         
        }
    }

}
