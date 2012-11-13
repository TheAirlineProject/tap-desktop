using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel.WeatherModel
{
    //the class for the weather for an hour
    public class HourlyWeather
    {
        public double Temperature { get; set; }
        public Weather.Precipitation Precip { get; set; }
        public Weather.CloudCover Cover { get; set; }
        public HourlyWeather(double Temperature, Weather.CloudCover cover, Weather.Precipitation precip)
        {
            this.Temperature = Temperature;
            this.Precip = precip;
            this.Cover = cover;
        }
    }
}
