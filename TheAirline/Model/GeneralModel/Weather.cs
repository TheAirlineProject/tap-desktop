using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel
{
    //the class for the weather for a specific date
    public class Weather
    {
        public enum eWindSpeed { Calm = 0,Light_Air=4, Light_Breeze = 8,Gentle_Breeze=12, Moderate_Breeze = 15,Fresh_Breeze=27,Strong_Breeze=45,Near_Gale=52, Gale = 60, Strong_Gale=72, Storm = 90,Violent_Storm=102, Hurricane=114  }
        public eWindSpeed WindSpeed { get; set; }
        public enum WindDirection { Tail, Head }
        public WindDirection Direction { get; set; }
        public DateTime Date { get; set; }  
        public enum Season { All_Year, Winter, Summer }
        public Weather(DateTime date, eWindSpeed windspeed, WindDirection direction)
        {
            this.Date = date;
            this.WindSpeed = windspeed;
            this.Direction = direction;
        }
    }
    
}
