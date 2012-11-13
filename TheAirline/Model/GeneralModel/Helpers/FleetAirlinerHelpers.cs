using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirportModel;

namespace TheAirline.Model.GeneralModel.Helpers
{
    //the helper class for fleet airliners
    public class FleetAirlinerHelpers
    {
        private static Random rnd = new Random();
        //returns the delay time because of the age of an airliner
        public static int GetAirlinerAgeDelay(FleetAirliner airliner)
        {
            int age = airliner.Airliner.Age;

            int tAge = 100 - (age*3);

            Boolean delayed = rnd.Next(100) > tAge;

            if (delayed)
                return rnd.Next(0, age) * 5;
            else
                return 0;
        }
        //returns the delay time because of the weather for an airliner
        public static int GetAirlinerWeatherDelay(FleetAirliner airliner)
        {
           Airport departureAirport = airliner.CurrentFlight.getDepartureAirport();

           if (departureAirport.Weather[0].Temperatures[GameObject.GetInstance().GameTime.Hour].Precip == WeatherModel.Weather.Precipitation.None || departureAirport.Weather[0].Temperatures[GameObject.GetInstance().GameTime.Hour].Precip == WeatherModel.Weather.Precipitation.Light_rain)
               return 0;

           int weatherFactor = 0;

           switch (departureAirport.Weather[0].Temperatures[GameObject.GetInstance().GameTime.Hour].Precip)
           {
               case WeatherModel.Weather.Precipitation.Light_snow:
                   weatherFactor = 1;
                   break;
               case WeatherModel.Weather.Precipitation.Heavy_snow:
                   weatherFactor = 3;
                   break;
               case WeatherModel.Weather.Precipitation.Fog:
                   weatherFactor = 2;
                   break;
               case WeatherModel.Weather.Precipitation.Freezing_rain:
                   weatherFactor = 4;
                   break;
               case WeatherModel.Weather.Precipitation.Hail:
                   weatherFactor = 4;
                   break;
               case WeatherModel.Weather.Precipitation.Heavy_rain:
                   weatherFactor = 3;
                   break;
               case WeatherModel.Weather.Precipitation.Sleet:
                   weatherFactor = 1;
                   break;
           }

           int delayTime = rnd.Next(weatherFactor, weatherFactor * 20);

           return delayTime;
        }
    }
}
