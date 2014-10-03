using System;
using System.Runtime.Serialization;

namespace TheAirline.Model.GeneralModel.WeatherModel
{
    [Serializable]
    //the class for the weather for a specific date
    public class Weather : BaseModel
    {
        #region Constants

        public const int Sunrise = 6;

        public const int Sunset = 20;

        #endregion

        #region Constructors and Destructors

        public Weather(
            DateTime date,
            eWindSpeed windspeed,
            WindDirection direction,
            CloudCover cover,
            Precipitation precip,
            HourlyWeather[] temperatures,
            double temperatureLow,
            double temperatureHigh)
        {
            Date = date;
            WindSpeed = windspeed;
            Direction = direction;
            Cover = cover;
            Precip = precip;
            Temperatures = temperatures;
            TemperatureLow = temperatureLow;
            TemperatureHigh = temperatureHigh;
        }

        private Weather(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Enums

        public enum CloudCover
        {
            Clear,

            PartlyCloudy,

            MostlyCloudy,

            Overcast
        }

        public enum Precipitation
        {
            None,

            Fog,

            IsolatedRain,

            LightRain,

            HeavyRain,

            Thunderstorms,

            IsolatedThunderstorms,

            IsolatedSnow,

            LightSnow,

            HeavySnow,

            MixedRainAndSnow,

            Sleet,

            FreezingRain
        }

        public enum Season
        {
            AllYear,

            Winter,

            Summer
        }

        public enum WindDirection
        {
            E,

            NE,

            N,

            NW,

            W,

            SW,

            S,

            SE
        }

        public enum eWindSpeed
        {
            Calm,

            LightAir,

            LightBreeze,

            GentleBreeze,

            ModerateBreeze,

            FreshBreeze,

            StrongBreeze,

            NearGale,

            Gale,

            StrongGale,

            Storm,

            ViolentStorm,

            Hurricane
        }

        #endregion

        #region Public Properties

        [Versioning("cover")]
        public CloudCover Cover { get; set; }

        [Versioning("date")]
        public DateTime Date { get; set; }

        [Versioning("direction")]
        public WindDirection Direction { get; set; }

        [Versioning("precip")]
        public Precipitation Precip { get; set; }

        [Versioning("high")]
        public double TemperatureHigh { get; set; }

        [Versioning("low")]
        public double TemperatureLow { get; set; }

        [Versioning("temperatures")]
        public HourlyWeather[] Temperatures { get; set; }

        [Versioning("windspeed")]
        public eWindSpeed WindSpeed { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion

        //public enum eWindSpeed { Calm = 0,Light_Air=4, Light_Breeze = 8,Gentle_Breeze=12, Moderate_Breeze = 15,Fresh_Breeze=27,Strong_Breeze=45,Near_Gale=52, Gale = 60, Strong_Gale=72, Storm = 90,Violent_Storm=102, Hurricane=114  }
    }
}