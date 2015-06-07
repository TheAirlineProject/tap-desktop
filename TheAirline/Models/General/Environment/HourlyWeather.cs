using System;
using System.Runtime.Serialization;
using TheAirline.General.Models;
using TheAirline.Infrastructure;

namespace TheAirline.Models.General.Environment
{
    //the class for the weather for an hour
    [Serializable]
    public class HourlyWeather : BaseModel
    {
        #region Constructors and Destructors

        public HourlyWeather(
            double temperature,
            Weather.CloudCover cover,
            Weather.Precipitation precip,
            Weather.eWindSpeed windspeed,
            Weather.WindDirection direction)
        {
            Temperature = temperature;
            Precip = precip;
            Cover = cover;
            WindSpeed = windspeed;
            Direction = direction;
        }

        private HourlyWeather(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("cover")]
        public Weather.CloudCover Cover { get; set; }

        [Versioning("direction")]
        public Weather.WindDirection Direction { get; set; }

        public bool HasPrecip => Cover == Weather.CloudCover.Overcast && Precip != Weather.Precipitation.None;

        [Versioning("precip")]
        public Weather.Precipitation Precip { get; set; }

        [Versioning("temperature")]
        public double Temperature { get; set; }

        [Versioning("windspeed")]
        public Weather.eWindSpeed WindSpeed { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion
    }
}