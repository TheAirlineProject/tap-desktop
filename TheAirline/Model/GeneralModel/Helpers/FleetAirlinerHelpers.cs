using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel.WeatherModel;

namespace TheAirline.Model.GeneralModel.Helpers
{
    //the helper class for fleet airliners
    public class FleetAirlinerHelpers
    {
        private static Random rnd = new Random();
        public enum DelayType { None, Airliner_problems, Bad_weather, Airport_Traffic }
        //returns the number of delay minutes (0 if not delayed) for an airliner
        public static KeyValuePair<DelayType, int> GetDelayedMinutes(FleetAirliner airliner)
        {
            //has already been delayed
            if (!airliner.CurrentFlight.IsOnTime)
                return new KeyValuePair<DelayType, int>(DelayType.None, 0);

            Dictionary<DelayType, int> delays = new Dictionary<DelayType, int>();

            delays.Add(DelayType.Airliner_problems, GetAirlinerAgeDelay(airliner));
            delays.Add(DelayType.Bad_weather, GetAirlinerWeatherDelay(airliner));

            KeyValuePair<DelayType, int> delay = new KeyValuePair<DelayType, int>(DelayType.None, 0);
            foreach (var d in delays)
            {
                if (d.Value > delay.Value)
                    delay = d;
            }

            return delay;
        }

        //returns the delay time because of the age of an airliner
        public static int GetAirlinerAgeDelay(FleetAirliner airliner)
        {
            int age = airliner.Airliner.Age;

            int tAge = 100 - (age * 3);

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

            int windFactor = 0;

            switch (departureAirport.Weather[0].WindSpeed)
            {
                case Weather.eWindSpeed.Strong_Breeze:
                    windFactor = 2;
                    break;
                case Weather.eWindSpeed.Near_Gale:
                    windFactor = 4;
                    break;
                case Weather.eWindSpeed.Gale:
                    windFactor = 6;
                    break;
                case Weather.eWindSpeed.Strong_Gale:
                    windFactor = 8;
                    break;
                case Weather.eWindSpeed.Storm:
                    windFactor = 10;
                    break;
                case Weather.eWindSpeed.Violent_Storm:
                    windFactor = 12;
                    break;
                case Weather.eWindSpeed.Hurricane:
                    windFactor = 14;
                    break;
            }

            if ((departureAirport.Weather[0].Temperatures[GameObject.GetInstance().GameTime.Hour].Precip == Weather.Precipitation.None || departureAirport.Weather[0].Temperatures[GameObject.GetInstance().GameTime.Hour].Precip == Weather.Precipitation.Light_rain) && windFactor == 0)
                return 0;

            int weatherFactor = 0;

            switch (departureAirport.Weather[0].Temperatures[GameObject.GetInstance().GameTime.Hour].Precip)
            {
                case WeatherModel.Weather.Precipitation.Light_snow:
                    weatherFactor = 4;
                    break;
                case WeatherModel.Weather.Precipitation.Heavy_snow:
                    weatherFactor = 8;
                    break;
                case WeatherModel.Weather.Precipitation.Fog:
                    weatherFactor = 4;
                    break;
                case WeatherModel.Weather.Precipitation.Freezing_rain:
                    weatherFactor = 6;
                    break;
                case WeatherModel.Weather.Precipitation.Hail:
                    weatherFactor = 6;
                    break;
                case WeatherModel.Weather.Precipitation.Light_rain:
                    weatherFactor = 1;
                    break;
                case WeatherModel.Weather.Precipitation.Heavy_rain:
                    weatherFactor = 4;
                    break;
                case WeatherModel.Weather.Precipitation.Sleet:
                    weatherFactor = 4;
                    break;
            }

            int delayTime = rnd.Next((weatherFactor + windFactor), (weatherFactor + windFactor) * 12);

            return delayTime;
        }
        //creates the stop over route based on the main route
        public static StopoverRoute CreateStopoverRoute(Airport dest1, Airport stopover, Airport dest2, Route mainroute, Boolean oneLegged)
        {
            StopoverRoute stopoverRoute = new StopoverRoute(stopover);

            Guid id = Guid.NewGuid();

            if (!oneLegged)
            {
                Route routeLegTwo = new Route(id.ToString(), dest1, stopover, 0);

                foreach (RouteAirlinerClass aClass in mainroute.Classes)
                {
                    //routeLegTwo.getRouteAirlinerClass(aClass.Type).FarePrice = aClass.FarePrice;
                    routeLegTwo.getRouteAirlinerClass(aClass.Type).FarePrice = PassengerHelpers.GetPassengerPrice(dest1, stopover) * GeneralHelpers.ClassToPriceFactor(aClass.Type);

                    foreach (RouteFacility facility in aClass.getFacilities())
                        routeLegTwo.getRouteAirlinerClass(aClass.Type).addFacility(facility);

                    routeLegTwo.getRouteAirlinerClass(aClass.Type).Seating = aClass.Seating;

                }


                stopoverRoute.addLeg(routeLegTwo);

            }
            id = Guid.NewGuid();
            
            Route routeLegOne = new Route(id.ToString(), stopover, dest2, 0);

            foreach (RouteAirlinerClass aClass in mainroute.Classes)
            {
                //routeLegOne.getRouteAirlinerClass(aClass.Type).FarePrice = aClass.FarePrice;

                routeLegOne.getRouteAirlinerClass(aClass.Type).FarePrice = PassengerHelpers.GetPassengerPrice(stopover, dest2) * GeneralHelpers.ClassToPriceFactor(aClass.Type);
      

                foreach (RouteFacility facility in aClass.getFacilities())
                    routeLegOne.getRouteAirlinerClass(aClass.Type).addFacility(facility);

                routeLegOne.getRouteAirlinerClass(aClass.Type).Seating = aClass.Seating;

            }

            stopoverRoute.addLeg(routeLegOne);

           

            return stopoverRoute;
        }
    }
}
