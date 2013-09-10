
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel.WeatherModel
{
    [Serializable]
    //the class for the weather for a specific date
    public class Weather
    {
        public const int Sunset = 20;
        public const int Sunrise = 6;
        //public enum eWindSpeed { Calm = 0,Light_Air=4, Light_Breeze = 8,Gentle_Breeze=12, Moderate_Breeze = 15,Fresh_Breeze=27,Strong_Breeze=45,Near_Gale=52, Gale = 60, Strong_Gale=72, Storm = 90,Violent_Storm=102, Hurricane=114  }
        public enum eWindSpeed { Calm, Light_Air, Light_Breeze, Gentle_Breeze, Moderate_Breeze, Fresh_Breeze, Strong_Breeze, Near_Gale, Gale, Strong_Gale, Storm, Violent_Storm, Hurricane }
        
        public eWindSpeed WindSpeed { get; set; }
        public enum WindDirection { E, NE, N, NW, W, SW, S, SE }
        
        public WindDirection Direction { get; set; }
        public enum Precipitation { None, Fog, Isolated_rain, Light_rain, Heavy_rain, Thunderstorms, Isolated_thunderstorms, Isolated_snow, Light_snow, Heavy_snow, Mixed_rain_and_snow, Sleet, Freezing_rain }
        
        public Precipitation Precip { get; set; }
        public enum CloudCover {Clear, Partly_Cloudy, Mostly_Cloudy, Overcast}
        
        public CloudCover Cover { get; set; }
        
        public DateTime Date { get; set; }  
        public enum Season { All_Year, Winter, Summer }
        
        public HourlyWeather[] Temperatures { get; set; }
        
        public double TemperatureHigh { get; set; }
        
        public double TemperatureLow { get; set; }
        public Weather(DateTime date, eWindSpeed windspeed, WindDirection direction, CloudCover cover,Precipitation precip, HourlyWeather[] temperatures,double temperatureLow, double temperatureHigh)
        {
            this.Date = date;
            this.WindSpeed = windspeed;
            this.Direction = direction;
            this.Cover = cover;
            this.Precip = precip;
            this.Temperatures = temperatures;
            this.TemperatureLow = temperatureLow;
            this.TemperatureHigh = temperatureHigh;
        }
    }
    
}
