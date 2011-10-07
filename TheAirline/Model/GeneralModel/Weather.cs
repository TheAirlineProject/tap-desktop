using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel
{
    //the class for the weather
    public class Weather
    {
        public enum eWindSpeed { Calm = 0, Light_Breeze = 7, Moderate_Breeze = 15, Gale = 60, Storm = 100 }
        public eWindSpeed WindSpeed { get; set; }
        public enum WindDirection { Tail, Head }
        public WindDirection Direction { get; set; }
   
        public Weather()
        {
            this.WindSpeed = eWindSpeed.Light_Breeze;
            this.Direction = WindDirection.Head;
        }
    }
    
}
