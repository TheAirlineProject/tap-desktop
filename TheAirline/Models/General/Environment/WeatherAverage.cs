using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TheAirline.General.Models;
using TheAirline.General.Models.Countries.Towns;
using TheAirline.Infrastructure;
using TheAirline.Models.Airports;
using TheAirline.Models.General.Countries;

namespace TheAirline.Models.General.Environment
{
    //the class for the averages for a month for a region/state/country/town
    [Serializable]
    public class WeatherAverage : BaseModel
    {
        #region Constructors and Destructors

        public WeatherAverage(
            int month,
            double temperatureMin,
            double temperatureMax,
            int precipitation,
            Weather.eWindSpeed windspeedMin,
            Weather.eWindSpeed windspeedMax,
            Country country,
            Town town,
            Airport airport)
        {
            Month = month;
            Airport = airport;
            Country = country;
            Town = town;
            TemperatureMin = temperatureMin;
            TemperatureMax = temperatureMax;
            WindSpeedMax = windspeedMax;
            WindSpeedMin = windspeedMin;
            Precipitation = precipitation;
        }

        public WeatherAverage(
            int month,
            double temperatureMin,
            double temperatureMax,
            int precipitation,
            Weather.eWindSpeed windspeedMin,
            Weather.eWindSpeed windspeedMax,
            Town town)
            : this(
                month,
                temperatureMin,
                temperatureMax,
                precipitation,
                windspeedMin,
                windspeedMax,
                town.Country,
                town,
                null)
        {
        }

        public WeatherAverage(
            int month,
            double temperatureMin,
            double temperatureMax,
            int precipitation,
            Weather.eWindSpeed windspeedMin,
            Weather.eWindSpeed windspeedMax,
            Country country)
            : this(month, temperatureMin, temperatureMax, precipitation, windspeedMin, windspeedMax, country, null, null
                )
        {
        }

        public WeatherAverage(
            int month,
            double temperatureMin,
            double temperatureMax,
            int precipitation,
            Weather.eWindSpeed windspeedMin,
            Weather.eWindSpeed windspeedMax,
            Airport airport)
            : this(
                month,
                temperatureMin,
                temperatureMax,
                precipitation,
                windspeedMin,
                windspeedMax,
                airport.Profile.Town.Country,
                airport.Profile.Town,
                airport)
        {
        }

        private WeatherAverage(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("airport")]
        public Airport Airport { get; set; }

        [Versioning("country")]
        public Country Country { get; set; }

        [Versioning("month")]
        public int Month { get; set; }

        //in mm
        [Versioning("precip")]
        public int Precipitation { get; set; }

        //in celcius
        [Versioning("max")]
        public double TemperatureMax { get; set; }

        [Versioning("min")]
        public double TemperatureMin { get; set; }

        [Versioning("town")]
        public Town Town { get; set; }

        [Versioning("windmax")]
        public Weather.eWindSpeed WindSpeedMax { get; set; }

        [Versioning("windmin")]
        public Weather.eWindSpeed WindSpeedMin { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion
    }

    //the list of weather averages
    public class WeatherAverages
    {
        #region Static Fields

        private static readonly List<WeatherAverage> Averages = new List<WeatherAverage>();

        #endregion

        #region Public Methods and Operators

        public static void AddWeatherAverage(WeatherAverage average)
        {
            Averages.Add(average);
        }

        public static void Clear()
        {
            Averages.Clear();
        }

        //returns the weather average for a specific airport and specific month
        public static WeatherAverage GetWeatherAverage(int month, Airport airport)
        {
            WeatherAverage airportAverage = Averages.Find(w => w.Airport == airport && w.Month == month);
            WeatherAverage townAverage = Averages.Find(w => w.Town == airport.Profile.Town && w.Month == month);
            WeatherAverage countryAverage =
                Averages.Find(w => w.Country == airport.Profile.Town.Country && w.Month == month);

            if (airportAverage != null)
            {
                return airportAverage;
            }

            if (townAverage != null)
            {
                return townAverage;
            }

            return countryAverage;
        }

        //returns all weather averages with a specific match
        public static List<WeatherAverage> GetWeatherAverages(Predicate<WeatherAverage> match)
        {
            return Averages.FindAll(match);
        }

        #endregion

        //adds a weather average to the list

        //clears the list of weather averages
    }
}