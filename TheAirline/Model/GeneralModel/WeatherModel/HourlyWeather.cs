
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel.WeatherModel
{
    //the class for the weather for an hour
    [Serializable]
    public class HourlyWeather
    {
        
        public double Temperature { get; set; }
        
        public Weather.Precipitation Precip { get; set; }
        
        public Weather.CloudCover Cover { get; set; }
        
        public Weather.WindDirection Direction { get; set; }
        
        public Weather.eWindSpeed WindSpeed { get; set; }
        public HourlyWeather(double Temperature, Weather.CloudCover cover, Weather.Precipitation precip, Weather.eWindSpeed windspeed, Weather.WindDirection direction)
        {
            this.Temperature = Temperature;
            this.Precip = precip;
            this.Cover = cover;
            this.WindSpeed = windspeed;
            this.Direction = direction;
        }
    }
}
