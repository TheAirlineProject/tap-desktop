using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.GeneralModel.CountryModel.TownModel;

namespace TheAirline.Model.GeneralModel.WeatherModel
{
    //the class for the averages for a month for a region/state/country/town
    public class WeatherAverage
    {
        public Region Region { get; set; }
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
        public WeatherAverage(int month, double temperatureMin, double temperatureMax,int precipitation, Weather.eWindSpeed windspeedMin, Weather.eWindSpeed windspeedMax, Region region, Country country, Town town)
        {
            this.Month = month;
            this.Region = region;
            this.Country = country;
            this.Town = town;
            this.TemperatureMin = temperatureMin;
            this.TemperatureMax = temperatureMax;
            this.WindSpeedMax = windspeedMax;
            this.WindSpeedMin = windspeedMin;
            this.Precipitation = precipitation;
        }
        public WeatherAverage(int month, double temperatureMin, double temperatureMax, int precipitation, Weather.eWindSpeed windspeedMin, Weather.eWindSpeed windspeedMax, Town town)
            : this(month, temperatureMin, temperatureMax, precipitation, windspeedMin, windspeedMax, town.Country.Region, town.Country, town)
        {
        }
        public WeatherAverage(int month, double temperatureMin, double temperatureMax, int precipitation, Weather.eWindSpeed windspeedMin, Weather.eWindSpeed windspeedMax, Country country)
            : this(month, temperatureMin, temperatureMax, precipitation, windspeedMin, windspeedMax, country.Region, country, null)
        {
        }
        public WeatherAverage(int month, double temperatureMin, double temperatureMax, int precipitation, Weather.eWindSpeed windspeedMin, Weather.eWindSpeed windspeedMax, Region region)
            : this(month, temperatureMin, temperatureMax, precipitation, windspeedMin, windspeedMax, region, null, null)
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
        //returns the weather average for a specific town and specific month
        public static WeatherAverage GetWeatherAverage(int month, Town town)
        {
            if (averages.Exists(w=>town == w.Town && w.Month == month))
                return averages.Find(w=>w.Town == town && w.Month == month);

            if (averages.Exists(w => town.Country == w.Country && w.Month == month))
                return averages.Find(w => w.Country == town.Country && w.Month == month);

            if (averages.Exists(w => town.Country.Region == w.Region && w.Month == month))
                return averages.Find(w => w.Region == town.Country.Region && w.Month == month);

            return null;
        }
    }
}
