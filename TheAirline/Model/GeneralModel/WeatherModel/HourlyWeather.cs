
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

        public Boolean HasPrecip
        {
            private set { ;}
            get { return this.Cover == Weather.CloudCover.Overcast && this.Precip != Weather.Precipitation.None; }
        }
        public HourlyWeather(double temperature, Weather.CloudCover cover, Weather.Precipitation precip, Weather.eWindSpeed windspeed, Weather.WindDirection direction)
        {
            this.Temperature = temperature;
            this.Precip = precip;
            this.Cover = cover;
            this.WindSpeed = windspeed;
            this.Direction = direction;
        }
    }
}
