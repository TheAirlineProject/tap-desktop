using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.GeneralModel.CountryModel.TownModel;
using TheAirline.Model.AirportModel;


namespace TheAirline.Model.GeneralModel.WeatherModel
{
    //the class for the averages for a month for a region/state/country/town
    [Serializable]
    public class WeatherAverage
    {
        
        public Airport Airport { get; set; }
        
        public Country Country { get; set; }
        
        public Town Town { get; set; }
        
        public int Month { get; set; }
        //in mm
        
        public int Precipitation { get; set; }
        //in celcius
        
        public double TemperatureMax { get; set; }
        
        public double TemperatureMin { get; set; }
        
        public Weather.eWindSpeed WindSpeedMax { get; set; }
        
        public Weather.eWindSpeed WindSpeedMin { get; set; }
        public WeatherAverage(int month, double temperatureMin, double temperatureMax,int precipitation, Weather.eWindSpeed windspeedMin, Weather.eWindSpeed windspeedMax, Country country, Town town,Airport airport)
        {
            this.Month = month;
            this.Airport = airport;
            this.Country = country;
            this.Town = town;
            this.TemperatureMin = temperatureMin;
            this.TemperatureMax = temperatureMax;
            this.WindSpeedMax = windspeedMax;
            this.WindSpeedMin = windspeedMin;
            this.Precipitation = precipitation;
        }
        public WeatherAverage(int month, double temperatureMin, double temperatureMax, int precipitation, Weather.eWindSpeed windspeedMin, Weather.eWindSpeed windspeedMax, Town town)
            : this(month, temperatureMin, temperatureMax, precipitation, windspeedMin, windspeedMax, town.Country, town,null)
        {
        }
        public WeatherAverage(int month, double temperatureMin, double temperatureMax, int precipitation, Weather.eWindSpeed windspeedMin, Weather.eWindSpeed windspeedMax, Country country)
            : this(month, temperatureMin, temperatureMax, precipitation, windspeedMin, windspeedMax, country, null,null)
        {
        }
        public WeatherAverage(int month, double temperatureMin, double temperatureMax, int precipitation, Weather.eWindSpeed windspeedMin, Weather.eWindSpeed windspeedMax, Airport airport)
            : this(month, temperatureMin, temperatureMax, precipitation, windspeedMin, windspeedMax,airport.Profile.Town.Country, airport.Profile.Town,airport)
        {
        }
     
       

    }
    //the list of weather averages
    public class WeatherAverages
    {
        private static List<WeatherAverage> averages = new List<WeatherAverage>();
        //adds a weather average to the list
        public static void AddWeatherAverage(WeatherAverage average)
        {
            averages.Add(average);
        }
        //returns the weather average for a specific airport and specific month
        public static WeatherAverage GetWeatherAverage(int month, Airport airport)
        {
            if (averages.Exists(w => airport == w.Airport && w.Month == month))
                return averages.Find(w => w.Airport == airport && w.Month == month);

            if (averages.Exists(w=>airport.Profile.Town == w.Town && w.Month == month))
                return averages.Find(w=>w.Town == airport.Profile.Town && w.Month == month);

            if (averages.Exists(w => airport.Profile.Town.Country == w.Country && w.Month == month))
                return averages.Find(w => w.Country == airport.Profile.Town.Country && w.Month == month);
        
            return null;
        }
        //returns all weather averages with a specific match
        public static List<WeatherAverage> GetWeatherAverages(Predicate<WeatherAverage> match)
        {
            return averages.FindAll(match);
        }
        //clears the list of weather averages
        public static void Clear()
        {
            averages.Clear();
        }
    }
}
