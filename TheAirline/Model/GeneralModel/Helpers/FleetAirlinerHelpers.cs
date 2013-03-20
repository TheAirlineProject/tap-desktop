using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel.StatisticsModel;
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
        public static StopoverRoute CreateStopoverRoute(Airport dest1, Airport stopover, Airport dest2, Route mainroute, Boolean oneLegged,Route.RouteType type)
        {
            StopoverRoute stopoverRoute = new StopoverRoute(stopover);

            Guid id = Guid.NewGuid();

            if (!oneLegged)
            {
                if (mainroute.Type == Route.RouteType.Passenger || mainroute.Type == Route.RouteType.Mixed)
                {
                    PassengerRoute routeLegTwo = new PassengerRoute(id.ToString(), dest1, stopover, 0);

                    foreach (RouteAirlinerClass aClass in ((PassengerRoute)mainroute).Classes)
                    {
                        //routeLegTwo.getRouteAirlinerClass(aClass.Type).FarePrice = aClass.FarePrice;
                        routeLegTwo.getRouteAirlinerClass(aClass.Type).FarePrice = PassengerHelpers.GetPassengerPrice(dest1, stopover) * GeneralHelpers.ClassToPriceFactor(aClass.Type);

                        foreach (RouteFacility facility in aClass.getFacilities())
                            routeLegTwo.getRouteAirlinerClass(aClass.Type).addFacility(facility);

                        routeLegTwo.getRouteAirlinerClass(aClass.Type).Seating = aClass.Seating;

                    }


                    stopoverRoute.addLeg(routeLegTwo);
                }
                if (mainroute.Type == Route.RouteType.Cargo || mainroute.Type == Route.RouteType.Mixed)
                {
                    CargoRoute routeLegTwo = new CargoRoute(id.ToString(), dest1, stopover, ((CargoRoute)mainroute).PricePerUnit);

                    stopoverRoute.addLeg(routeLegTwo);
                }

            }

            if (mainroute.Type == Route.RouteType.Mixed || mainroute.Type == Route.RouteType.Passenger)
            {
                id = Guid.NewGuid();

                PassengerRoute routeLegOne = new PassengerRoute(id.ToString(), stopover, dest2, 0);

                foreach (RouteAirlinerClass aClass in ((PassengerRoute)mainroute).Classes)
                {
                    //routeLegOne.getRouteAirlinerClass(aClass.Type).FarePrice = aClass.FarePrice;

                    routeLegOne.getRouteAirlinerClass(aClass.Type).FarePrice = PassengerHelpers.GetPassengerPrice(stopover, dest2) * GeneralHelpers.ClassToPriceFactor(aClass.Type);


                    foreach (RouteFacility facility in aClass.getFacilities())
                        routeLegOne.getRouteAirlinerClass(aClass.Type).addFacility(facility);

                    routeLegOne.getRouteAirlinerClass(aClass.Type).Seating = aClass.Seating;

                }

                stopoverRoute.addLeg(routeLegOne);
            }
            if (mainroute.Type == Route.RouteType.Cargo || mainroute.Type == Route.RouteType.Mixed)
            {
                CargoRoute routeLegOne = new CargoRoute(id.ToString(), stopover, dest2, ((CargoRoute)mainroute).PricePerUnit);

                stopoverRoute.addLeg(routeLegOne);
              
            }
           

            return stopoverRoute;
        }
        //returns the minimum time between flights for an airliner
        public static TimeSpan GetMinTimeBetweenFlights(FleetAirliner airliner)
        {
            return GetMinTimeBetweenFlights(airliner.Airliner.getTotalSeatCapacity());
        }
        public static TimeSpan GetMinTimeBetweenFlights(AirlinerType type)
        {
            if (type == null || type is AirlinerCargoType)
                return GetMinTimeBetweenFlights(0);

            return GetMinTimeBetweenFlights(((AirlinerPassengerType)type).MaxSeatingCapacity);
        }
        private static TimeSpan GetMinTimeBetweenFlights(int passengers)
        {
            TimeSpan minTime = new TimeSpan(0, 30, 0);

            if (passengers > 200)
                return minTime.Add(minTime);
            else
                return minTime;


            
        }
        //sets the flights stats for an airliner
        public static void SetFlightStats(FleetAirliner airliner)
        {
            DateTime landingTime = airliner.CurrentFlight.FlightTime.Add(MathHelpers.GetFlightTime(airliner.CurrentFlight.Entry.DepartureAirport.Profile.Coordinates, airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates, GetCruisingSpeed(airliner)));

            Airport dest = airliner.CurrentFlight.Entry.Destination.Airport;
            Airport dept = airliner.CurrentFlight.Entry.DepartureAirport;
 
            //canellation and ontime-percent
            double cancellationPercent = airliner.Airliner.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Cancellations")) / (airliner.Airliner.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Arrivals")) + airliner.Airliner.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Cancellations")));
            airliner.Airliner.Airline.Statistics.setStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Cancellation%"), cancellationPercent * 100);

            Boolean isOnTime = landingTime.Subtract(airliner.CurrentFlight.getScheduledLandingTime()).Minutes < 15;

            if (isOnTime)
                airliner.Airliner.Airline.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("On-Time"), 1);

            airliner.Airliner.Airline.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Arrivals"), 1);
            dest.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Arrivals"), 1);
            airliner.CurrentFlight.Entry.TimeTable.Route.Statistics.addStatisticsValue(StatisticsTypes.GetStatisticsType("Cargo"), 1);
            airliner.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Arrivals"), 1);

            double onTimePercent = airliner.Airliner.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("On-Time")) / airliner.Airliner.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Arrivals"));
            airliner.Airliner.Airline.Statistics.setStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("On-Time%"), onTimePercent * 100);

        
            if (airliner.CurrentFlight.isCargoFlight())
            {
                airliner.CurrentFlight.Entry.TimeTable.Route.Statistics.addStatisticsValue(StatisticsTypes.GetStatisticsType("Cargo"), (int)airliner.CurrentFlight.Cargo);
                double routePassengers = airliner.CurrentFlight.Entry.TimeTable.Route.Statistics.getStatisticsValue( StatisticsTypes.GetStatisticsType("Cargo"));
                double routeDepartures = airliner.CurrentFlight.Entry.TimeTable.Route.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Arrivals"));
                airliner.CurrentFlight.Entry.TimeTable.Route.Statistics.setStatisticsValue(StatisticsTypes.GetStatisticsType("Cargo%"), (int)(routePassengers / routeDepartures));

                airliner.CurrentFlight.Entry.TimeTable.Route.Statistics.addStatisticsValue(StatisticsTypes.GetStatisticsType("Capacity"),(int)((AirlinerCargoType)airliner.Airliner.Type).CargoSize);

                double airlinerCargo = airliner.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Cargo"));
                double airlinerDepartures = airliner.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Arrivals"));
                airliner.Statistics.setStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Cargo%"), (int)(airlinerCargo / airlinerDepartures));

                airliner.Airliner.Airline.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Cargo"), airliner.CurrentFlight.Cargo);
                airliner.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Cargo"), airliner.CurrentFlight.Cargo);
                dest.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Cargo"), (int)airliner.CurrentFlight.Cargo);

                double destCargo = dest.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Cargo"));
                double destDepartures = dest.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Arrivals"));
                dest.Statistics.setStatisticsValue(GameObject.GetInstance().GameTime.Year, airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Cargo%"), (int)(destCargo / destDepartures));

                double airlineCargo = airliner.Airliner.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Cargo"));
                double airlineDepartures = airliner.Airliner.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Arrivals"));
                airliner.Airliner.Airline.Statistics.setStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Cargo%"), (int)(airlineCargo / airlineDepartures));

            }
            if (airliner.CurrentFlight.isPassengerFlight())
            {
                foreach (AirlinerClass aClass in airliner.Airliner.Classes)
                {
                    RouteAirlinerClass raClass = ((PassengerRoute)airliner.CurrentFlight.Entry.TimeTable.Route).getRouteAirlinerClass(aClass.Type);

                    airliner.CurrentFlight.Entry.TimeTable.Route.Statistics.addStatisticsValue(raClass, StatisticsTypes.GetStatisticsType("Passengers"), airliner.CurrentFlight.getFlightAirlinerClass(raClass.Type).Passengers);
                    double routePassengers = airliner.CurrentFlight.Entry.TimeTable.Route.Statistics.getStatisticsValue(raClass, StatisticsTypes.GetStatisticsType("Passengers"));
                    double routeDepartures = airliner.CurrentFlight.Entry.TimeTable.Route.Statistics.getStatisticsValue(raClass, StatisticsTypes.GetStatisticsType("Arrivals"));
                    airliner.CurrentFlight.Entry.TimeTable.Route.Statistics.setStatisticsValue(raClass, StatisticsTypes.GetStatisticsType("Passengers%"), (int)(routePassengers / routeDepartures));

                    airliner.CurrentFlight.Entry.TimeTable.Route.Statistics.addStatisticsValue(raClass, StatisticsTypes.GetStatisticsType("Capacity"), airliner.Airliner.getAirlinerClass(raClass.Type).SeatingCapacity);
                }

                double airlinerPassengers = airliner.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Passengers"));
                double airlinerDepartures = airliner.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Arrivals"));
                airliner.Statistics.setStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Passengers%"), (int)(airlinerPassengers / airlinerDepartures));


                airliner.Airliner.Airline.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Passengers"), airliner.CurrentFlight.getTotalPassengers());
                airliner.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Passengers"), airliner.CurrentFlight.getTotalPassengers());
                dest.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Passengers"), airliner.CurrentFlight.getTotalPassengers());


                double destPassengers = dest.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Passengers"));
                double destDepartures = dest.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Arrivals"));
                dest.Statistics.setStatisticsValue(GameObject.GetInstance().GameTime.Year, airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Passengers%"), (int)(destPassengers / destDepartures));

                double airlinePassengers = airliner.Airliner.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Passengers"));
                double airlineDepartures = airliner.Airliner.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Arrivals"));
                airliner.Airliner.Airline.Statistics.setStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Passengers%"), (int)(airlinePassengers / airlineDepartures));

                //the statistics for destination airport
                dept.addDestinationStatistics(dest, airliner.CurrentFlight.getTotalPassengers());
            }
        }
        //returns the flight crusing speed based on the wind
        public static int GetCruisingSpeed(FleetAirliner airliner)
        {
            Airport dest = airliner.CurrentFlight.Entry.Destination.Airport;
            Airport dept = airliner.CurrentFlight.getDepartureAirport();

            double windFirstHalf = ((int)dept.Weather[0].WindSpeed) * GetWindInfluence(airliner, dept.Weather[0]);

            double windSecondHalf = ((int)dest.Weather[0].WindSpeed) * GetWindInfluence(airliner, dest.Weather[0]);

            int speed = Convert.ToInt32(((airliner.Airliner.Type.CruisingSpeed + windFirstHalf) + (airliner.Airliner.Type.CruisingSpeed + windSecondHalf)) / 2);

            return speed;

        }
        //returns if the wind is tail (1), head (-1), or from side (0)
        private static int GetWindInfluence(FleetAirliner airliner, Weather currentWeather)
        {
            double direction = MathHelpers.GetDirection(airliner.CurrentFlight.getDepartureAirport().Profile.Coordinates, airliner.CurrentFlight.getNextDestination().Profile.Coordinates);

            Weather.WindDirection windDirection = MathHelpers.GetWindDirectionFromDirection(direction);

            //W+E = 0+4= 5, N+S=2+6 - = Abs(Count/2) -> Head, Abs(0) -> Tail -> if ends/starts with same => tail, indexof +-1 -> tail, (4+(indexof))+-1 -> head 

            int windDirectionLenght = Enum.GetValues(typeof(Weather.WindDirection)).Length;
            int indexCurrentPosition = Array.IndexOf(Enum.GetValues(typeof(Weather.WindDirection)), windDirection);
            //int indexWeather = Array.IndexOf(Enum.GetValues(typeof(Weather.WindDirection)),currentWeather.WindSpeed);

            //check for tail wind
            Weather.WindDirection windTailLeft = indexCurrentPosition > 0 ? (Weather.WindDirection)indexCurrentPosition - 1 : (Weather.WindDirection)windDirectionLenght - 1;
            Weather.WindDirection windTailRight = indexCurrentPosition < windDirectionLenght - 1 ? (Weather.WindDirection)indexCurrentPosition + 1 : (Weather.WindDirection)0;

            if (windTailLeft == currentWeather.Direction || windTailRight == currentWeather.Direction || windDirection == currentWeather.Direction)
                return 1;

            Weather.WindDirection windOpposite = indexCurrentPosition - (windDirectionLenght / 2) > 0 ? (Weather.WindDirection)indexCurrentPosition - (windDirectionLenght / 2) : (Weather.WindDirection)windDirectionLenght - 1 - indexCurrentPosition - (windDirectionLenght / 2);
            int indexOpposite = Array.IndexOf(Enum.GetValues(typeof(Weather.WindDirection)), windOpposite);

            Weather.WindDirection windHeadLeft = indexOpposite > 0 ? (Weather.WindDirection)indexOpposite - 1 : (Weather.WindDirection)windDirectionLenght - 1;
            Weather.WindDirection windHeadRight = indexOpposite < windDirectionLenght - 1 ? (Weather.WindDirection)indexOpposite + 1 : (Weather.WindDirection)0;

            if (windHeadLeft == currentWeather.Direction || windHeadRight == currentWeather.Direction || windOpposite == currentWeather.Direction)
                return -1;

            return 0;
        }
    }
}
