using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirportModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel.WeatherModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.PassengerModel;

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
                double distance = airports2.Where(a => a != airport1).Max(a => MathHelpers.GetDistance(a, airport1));

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
        //finds all airports in a radius of x km from a airport
        public static List<Airport> GetAirportsNearAirport(Airport airport, double distance)
        {
            return airport.Statics.getAirportsWithin(distance);
        }
        //returns all routes from an airport for an airline
        public static List<Route> GetAirportRoutes(Airport airport, Airline airline)
        {
            return airline.Routes.FindAll(r => r.Destination2 == airport || r.Destination1 == airport);
        }
        //returns if there is a route between two airports
        public static Boolean HasRoute(Airport airport1, Airport airport2)
        {
            var airlines = new List<Airline>(Airlines.GetAllAirlines());

            var routes = new List<Route>();

            foreach (Airline airline in airlines)
            {
                routes.AddRange(airline.Routes);
            }

            return routes.Where(r => (r.Destination1 == airport1 && r.Destination2 == airport2) || (r.Destination1 == airport2 && r.Destination2 == airport1)).Count() > 0;

        }
        //returns all routes from an airport
        public static List<Route> GetAirportRoutes(Airport airport)
        {
            var routes = Airlines.GetAllAirlines().SelectMany(a => a.Routes).Where(r => r.Destination1 == airport || r.Destination2 == airport);

            return routes.ToList();
        }
        //returns the number of routes between two airports
        public static int GetNumberOfAirportsRoutes(Airport airport1, Airport airport2)
        {
            int count = 0;
            var routes = Airlines.GetAllAirlines().SelectMany(a => a.Routes);

            lock (routes)
            {
                count = routes.Count(r => (r.Destination1 == airport1 && r.Destination2 == airport2) || (r.Destination1 == airport2 && r.Destination2 == airport1));
            }
            
            return count;
        }
        //returns all routes between two airports
        public static List<Route> GetAirportRoutes(Airport airport1, Airport airport2)
        {
            var airlines = new List<Airline>(Airlines.GetAllAirlines());

            var routes = new List<Route>();

            foreach (Airline airline in airlines)
            {
                lock (airline.Routes)
                {
                    var aRoutes = new List<Route>(airline.Routes);

                    foreach (Route route in aRoutes)
                        routes.Add(route);
                }

            }
            return routes.Where(r => (r.Destination1 == airport1 && r.Destination2 == airport2) || (r.Destination1 == airport2 && r.Destination2 == airport1)).ToList();


        }

        public static void ClearAirportStatistics()
        {
            foreach (Airport airport in Airports.GetAllAirports())
                airport.Statistics.Stats.Clear();
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
        //creates the weather for an airport
        public static void CreateAirportWeather(Airport airport)
        {
            airport.Weather[0] = null;

            WeatherAverage average = WeatherAverages.GetWeatherAverages(w => w.Airport != null && w.Airport == airport && w.Month == GameObject.GetInstance().GameTime.Month).FirstOrDefault();

            if (average == null)
                average = WeatherAverages.GetWeatherAverages(w => w.Town != null && w.Town == airport.Profile.Town && w.Month == GameObject.GetInstance().GameTime.Month).FirstOrDefault();

            if (average == null)
                average = WeatherAverages.GetWeatherAverages(w => w.Country != null && w.Country == airport.Profile.Town.Country && w.Month == GameObject.GetInstance().GameTime.Month).FirstOrDefault();

            if (average == null)
                CreateFiveDaysAirportWeather(airport);
            else
            {
                var lAirport = new List<Airport>();
                lAirport.Add(airport);

                CreateAirportsWeather(lAirport, average);
            }

            
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
        public static void CreateFiveDaysAirportWeather(Airport airport)
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

                temperature = MathHelpers.GetRandomDoubleNumber(minTemp, maxTemp);
            }
            else
            {
                int windIndex = windSpeedValues.ToList().IndexOf(previousWeather.WindSpeed);
                windSpeed = windSpeedValues[rnd.Next(Math.Max(0, windIndex - 2), Math.Min(windIndex + 2, windSpeedValues.Length))];

                double previousTemperature = (previousWeather.TemperatureHigh + previousWeather.TemperatureLow) / 2;

                double maxTemp = Math.Min(40, previousTemperature + 5);
                double minTemp = Math.Max(-20, previousTemperature - 5);

                temperature = MathHelpers.GetRandomDoubleNumber(minTemp, maxTemp);
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

                temperatureLow = MathHelpers.GetRandomDoubleNumber(average.TemperatureMin - 5, average.TemperatureMin + 5);
                temperatureHigh = MathHelpers.GetRandomDoubleNumber(Math.Max(average.TemperatureMax - 5, temperatureLow + 1), average.TemperatureMax + 5);

            }
            else
            {
                double previousTemperature = (previousWeather.TemperatureHigh + previousWeather.TemperatureLow) / 2;
                int windIndex = windSpeedValues.ToList().IndexOf(previousWeather.WindSpeed);
                windSpeed = windSpeedValues[rnd.Next(Math.Max(windIndexMin, windIndex - 2), Math.Min(windIndex + 2, windIndexMax))];

                double minTemp = Math.Max(average.TemperatureMin, previousTemperature - 5);
                temperatureLow = MathHelpers.GetRandomDoubleNumber(minTemp - 5, minTemp + 5);

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
            {
                Weather.CloudCover[] notOvercastCovers = new Weather.CloudCover[] { Weather.CloudCover.Clear, Weather.CloudCover.Mostly_Cloudy, Weather.CloudCover.Partly_Cloudy };
                cover = notOvercastCovers[rnd.Next(notOvercastCovers.Length)];
            }
       
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

            cover = (from c in hourlyTemperature group c by c.Cover into g select new { Cover = g.Key, Qty = g.Count() }).OrderByDescending(g => g.Qty).First().Cover;
            precip = (from c in hourlyTemperature group c by c.Precip into g select new { Precip = g.Key, Qty = g.Count() }).OrderByDescending(g => g.Qty).First().Precip;

            Weather weather = new Weather(date, windSpeed, windDirection, cover, precip, hourlyTemperature, temperatureLow, temperatureHigh);


            return weather;
        }
        //returns the precipitation for a temperature
        private static Weather.Precipitation GetPrecipitation(double temperature)
        {
            if (temperature > 10)
            {
                Weather.Precipitation[] values = { Weather.Precipitation.Thunderstorms, Weather.Precipitation.Heavy_rain, Weather.Precipitation.Light_rain,Weather.Precipitation.Isolated_thunderstorms };
                return values[rnd.Next(values.Length)];
            }
            if (temperature <= 10 && temperature >= 5)
            {
                Weather.Precipitation[] values = { Weather.Precipitation.Heavy_rain, Weather.Precipitation.Light_rain,Weather.Precipitation.Isolated_rain,Weather.Precipitation.Isolated_thunderstorms };
                return values[rnd.Next(values.Length)];
            }
            if (temperature < 5 && temperature >= -3)
            {
                Weather.Precipitation[] values = { Weather.Precipitation.Freezing_rain, Weather.Precipitation.Mixed_rain_and_snow, Weather.Precipitation.Sleet, Weather.Precipitation.Light_snow,Weather.Precipitation.Isolated_snow };
                return values[rnd.Next(values.Length)];
            }
            if (temperature < -3)
            {
                Weather.Precipitation[] values = { Weather.Precipitation.Heavy_snow, Weather.Precipitation.Light_snow,Weather.Precipitation.Isolated_snow};
                return values[rnd.Next(values.Length)];
            }
            return Weather.Precipitation.Light_rain;
        }
        //returns if there is bad weather at an airport
        public static Boolean HasBadWeather(Airport airport)
        {
            return false;//airport.Weather[0].WindSpeed == Weather.eWindSpeed.Hurricane || airport.Weather[0].WindSpeed == Weather.eWindSpeed.Violent_Storm;
        }
        //returns the price for a contract at an airport
        public static double GetAirportContractPrice(Airport airport)
        {
            double paxDemand = airport.Profile.MajorDestionations.Sum(d => d.Value) + airport.Profile.Pax;

            double basePrice = 10000;

            return GeneralHelpers.GetInflationPrice(paxDemand * basePrice);

            //(initial amount, in the millions; and a montly amount probably $50,000 or so
        }
        //checks an airport for extending of runway
        public static void CheckForExtendRunway(Airport airport)
        {
            int minYearsBetweenExpansions = 5;

            long maxRunwayLenght = (from r in airport.Runways select r.Length).Max();
            long longestRequiredRunwayLenght = AirlinerTypes.GetTypes(a => a.Produced.From <= GameObject.GetInstance().GameTime && a.Produced.To >= GameObject.GetInstance().GameTime).Max(a => a.MinRunwaylength);

            var airportRoutes = AirportHelpers.GetAirportRoutes(airport);
            var routeAirliners = airportRoutes.SelectMany(r => r.getAirliners());

            long longestRunwayInUse = routeAirliners.Count() > 0 ? routeAirliners.Max(a => a.Airliner.Type.MinRunwaylength) : 0;

            if (maxRunwayLenght < longestRequiredRunwayLenght / 2 && maxRunwayLenght < longestRunwayInUse * 3 / 4 && GameObject.GetInstance().GameTime.AddYears(-minYearsBetweenExpansions) > airport.LastExpansionDate)
            {
                List<string> runwayNames = (from r in Airports.GetAllAirports().SelectMany(a => a.Runways) select r.Name).Distinct().ToList();

                foreach (Runway r in airport.Runways)
                    runwayNames.Remove(r.Name);

                Runway.SurfaceType surface = airport.Runways[0].Surface;
                long lenght = Math.Min(longestRequiredRunwayLenght * 3 / 4, longestRunwayInUse * 2);

                Runway runway = new Runway(runwayNames[rnd.Next(runwayNames.Count)], lenght, surface, GameObject.GetInstance().GameTime.AddDays(90), false);
                airport.Runways.Add(runway);

                airport.LastExpansionDate = GameObject.GetInstance().GameTime;
            }

        }
        //checks an airport for new gates
        public static void CheckForExtendGates(Airport airport)
        {
            int minYearsBetweenExpansions = 5;

            if (airport.Terminals.getOrdereredGates() == 0 && GameObject.GetInstance().GameTime.AddYears(-minYearsBetweenExpansions) > airport.LastExpansionDate)
            {
                Terminal minTerminal = airport.Terminals.AirportTerminals.OrderBy(t => t.Gates.NumberOfGates).First();

                Boolean newTerminal = minTerminal.Gates.NumberOfGates > 50;
                //extend existing
                if (!newTerminal)
                {
                    int numberOfGates = Math.Max(5, minTerminal.Gates.NumberOfGates);
                    int daysToBuild = numberOfGates * 10 + (newTerminal ? 60 : 0);

                    long price = numberOfGates * airport.getTerminalGatePrice() + (newTerminal ? airport.getTerminalPrice() : 0);
                    price = price / 3 * 4;

                    if (airport.Income > price)
                    {

                        for (int i = 0; i < numberOfGates; i++)
                        {
                            Gate gate = new Gate(GameObject.GetInstance().GameTime.AddDays(daysToBuild));
                            gate.Airline = minTerminal.Airline;

                            minTerminal.Gates.addGate(gate);
                        }

                        airport.Income -= price;
                        airport.LastExpansionDate = GameObject.GetInstance().GameTime;
                    }

                }
                //build new terminal
                else
                {

                    int numberOfGates = airport.Terminals.getTerminals()[0].Gates.NumberOfDeliveredGates;

                    int daysToBuild = numberOfGates * 10 + (newTerminal ? 60 : 0);

                    long price = numberOfGates * airport.getTerminalGatePrice() + (newTerminal ? airport.getTerminalPrice() : 0);
                    price = price / 3 * 4;

                    if (airport.Income > price)
                    {

                        Terminal terminal = new Terminal(airport, null, "Terminal", numberOfGates, GameObject.GetInstance().GameTime.AddDays(daysToBuild));

                        airport.addTerminal(terminal);
                        airport.Income -= price;
                        airport.LastExpansionDate = GameObject.GetInstance().GameTime;
                    }
                }
            }


        }
        //reallocate the pax demand from one airport to another
        public static void ReallocateAirport(Airport airportOld, Airport airportNew)
        {
            if (airportNew.getMajorDestinations().Count == 0)
            {
                foreach (DestinationDemand paxDemand in airportOld.getDestinationsPassengers())
                    airportNew.addDestinationPassengersRate(paxDemand);
            }
        }
        //checks if an airline has any free gates at an airport - more than 90 % free
        public static Boolean HasFreeGates(Airport airport, Airline airline)
        {
            List<AirportContract> contracts = airport.getAirlineContracts(airline);

            if (contracts.Count == 0)
                return false;

            return airport.Terminals.getFreeSlotsPercent(airline) > 90;
        }
        //rents a "standard" amount of gates at an airport for an airline
        public static Boolean RentGates(Airport airport, Airline airline)
        {
            int maxGates = airport.Terminals.getFreeGates();

            int gatesToRent = Math.Min(maxGates, (int)(airline.Mentality) + 2);

            if (gatesToRent == 0)
                return false;

            RentGates(airport, airline, gatesToRent);

            return true;

        }
        public static void RentGates(Airport airport, Airline airline, int gates)
        {
            int currentgates = airport.AirlineContracts.Where(a => a.Airline == airline).Sum(c => c.NumberOfGates);
            AirportContract contract = new AirportContract(airline, airport, GameObject.GetInstance().GameTime, gates, 20, GetYearlyContractPayment(airport, gates, 20));
            
            if (currentgates == 0)
            {
                airport.addAirlineContract(contract);
            }
            else
            {
                foreach (AirportContract c in airport.AirlineContracts.Where(a => a.Airline == airline))
                { 
                    c.NumberOfGates += gates;
                }
            }

            for (int i = 0; i < gates; i++)
            {
                Gate gate = airport.Terminals.getGates().Where(g => g.Airline == null).First();
                gate.Airline = airline;
            }

        }

        //returns all occupied slot times for an airline at an airport (15 minutes slots)
        public static List<TimeSpan> GetOccupiedSlotTimes(Airport airport, Airline airline, List<AirportContract> contracts)
        {
            List<KeyValuePair<Route, TimeSpan>> occupiedSlots = new List<KeyValuePair<Route, TimeSpan>>();

            TimeSpan gateTimeBefore = new TimeSpan(0, 15, 0);
            TimeSpan gateTimeAfter = new TimeSpan(0, 15, 0);

            int gates = contracts.Sum(c => c.NumberOfGates);

            var routes = new List<Route>(GetAirportRoutes(airport, airline));

            var entries = new List<RouteTimeTableEntry>(routes.SelectMany(r => r.TimeTable.Entries));

            foreach (var entry in entries)
            {

                TimeSpan entryTakeoffTime = new TimeSpan((int)entry.Day, entry.Time.Hours, entry.Time.Minutes, entry.Time.Seconds);
                TimeSpan entryLandingTime = entryTakeoffTime.Add(entry.TimeTable.Route.getFlightTime(entry.Airliner.Airliner.Type));

                if (entryLandingTime.Days > 6)
                    entryLandingTime = new TimeSpan(0, entryLandingTime.Hours, entryLandingTime.Minutes, entryLandingTime.Seconds);

                if (entry.DepartureAirport == airport)
                {
                    TimeSpan entryStartTakeoffTime = entryTakeoffTime.Subtract(gateTimeBefore);
                    TimeSpan entryEndTakeoffTime = entryTakeoffTime.Add(gateTimeAfter);

                    TimeSpan tTakeoffTime = new TimeSpan(entryStartTakeoffTime.Days, entryStartTakeoffTime.Hours, (entryStartTakeoffTime.Minutes / 15) * 15, 0);

                    while (tTakeoffTime < entryEndTakeoffTime)
                    {
                        if (!occupiedSlots.Exists(s=>s.Key==entry.TimeTable.Route && s.Value == tTakeoffTime))
                            occupiedSlots.Add(new KeyValuePair<Route,TimeSpan>(entry.TimeTable.Route,tTakeoffTime));
                        tTakeoffTime = tTakeoffTime.Add(new TimeSpan(0, 15, 0));
                    }
                }

                if (entry.DepartureAirport != airport)
                {
                    TimeSpan entryStartLandingTime = entryLandingTime.Subtract(gateTimeBefore);
                    TimeSpan entryEndLandingTime = entryLandingTime.Add(gateTimeAfter);

                    TimeSpan tLandingTime = new TimeSpan(entryStartLandingTime.Days, entryStartLandingTime.Hours, (entryStartLandingTime.Minutes / 15) * 15, 0);

                    while (tLandingTime < entryEndLandingTime)
                    {
                        if (!occupiedSlots.Exists(s => s.Key == entry.TimeTable.Route && s.Value == tLandingTime))
                            occupiedSlots.Add(new KeyValuePair<Route, TimeSpan>(entry.TimeTable.Route, tLandingTime));
                        tLandingTime = tLandingTime.Add(new TimeSpan(0, 15, 0));
                    }
                }
            }

            var slots = (from s in occupiedSlots
                         group s.Value by s.Value into g
                         select new { Time = g.Key, Slots = g });

            return slots.Where(s => s.Slots.Count() >= gates).SelectMany(s => s.Slots).ToList();

        }
        public static List<TimeSpan> GetOccupiedSlotTimes(Airport airport, Airline airline)
        {
            return GetOccupiedSlotTimes(airport, airline, airport.AirlineContracts.Where(c => c.Airline == airline).ToList());
        }
        //returns if an airline has enough free slots at an airport
        public static Boolean CanFillRoutesEntries(Airport airport, Airline airline, List<AirportContract> contracts)
        {
            int numberOfOccupiedSlots = GetOccupiedSlotTimes(airport, airline, contracts).GroupBy(s => s.Ticks).Where(x => x.Count() > 1).Count();
            return numberOfOccupiedSlots == 0;

        }
        //returns the yearly payment for a number of gates
        public static double GetYearlyContractPayment(Airport airport, int gates, int length)
        {
            double basePrice = airport.getGatePrice() * 12;

            double lengthFactor = 100 - length;

            return gates * (basePrice * (lengthFactor / 100));
        }
        //returns the price for a hub at an airport
        public static double GetHubPrice(Airport airport, HubType type)
        {
           double price = type.Price;
          
            price = price +25000 * ((int)airport.Profile.Size);
            return Convert.ToInt64(GeneralHelpers.GetInflationPrice(price));
  
        }
        //converts a pax value to airport size
        public static GeneralHelpers.Size ConvertAirportPaxToSize(double size)
        {
            Dictionary<int, double> yearCoeffs = new Dictionary<int, double>();
            yearCoeffs.Add(1960, 1.3);
            yearCoeffs.Add(1970, 1.2);
            yearCoeffs.Add(1980, 1.15);
            yearCoeffs.Add(1990, 1.10);
            yearCoeffs.Add(2000, 1.0658);
            yearCoeffs.Add(2010, 1);

            int decade = (GameObject.GetInstance().GameTime.Year - 1960) / 10 * 10 + 1960;

            double coeff = 1;

            if (yearCoeffs.ContainsKey(decade))
                coeff = yearCoeffs[decade];

            double coeffPax = coeff * size;

            if (coeffPax > 32000)
                return GeneralHelpers.Size.Largest;
            if (coeffPax > 16000)
                return GeneralHelpers.Size.Very_large;
            if (coeffPax > 9000)
                return GeneralHelpers.Size.Large;
            if (coeffPax > 3000)
                return GeneralHelpers.Size.Medium;
            if (coeffPax > 535)
                return GeneralHelpers.Size.Small;
            if (coeffPax > 160)
                return GeneralHelpers.Size.Very_small;

            return GeneralHelpers.Size.Smallest;


        }
    }

}
