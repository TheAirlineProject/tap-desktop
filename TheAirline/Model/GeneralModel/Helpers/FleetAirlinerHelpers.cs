using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel.InvoicesModel;
using TheAirline.Model.GeneralModel.StatisticsModel;
using TheAirline.Model.GeneralModel.WeatherModel;
using TheAirline.Model.RouteModel;

namespace TheAirline.Model.GeneralModel.Helpers
{
    //the helper class for fleet airliners
    public class FleetAirlinerHelpers
    {
        public enum DelayType
        {
            None,
            AirlinerProblems,
            BadWeather,
            AirportTraffic
        }

        private static readonly Random Rnd = new Random();

        //returns the number of delay minutes (0 if not delayed) for an airliner
        public static KeyValuePair<DelayType, int> GetDelayedMinutes(FleetAirliner airliner)
        {
            //has already been delayed
            if (!airliner.CurrentFlight.IsOnTime)
                return new KeyValuePair<DelayType, int>(DelayType.None, 0);

            var delays = new Dictionary<DelayType, int> {{DelayType.AirlinerProblems, GetAirlinerAgeDelay(airliner)}, {DelayType.BadWeather, 0}};

            //delays.Add(DelayType.Bad_weather, GetAirlinerWeatherDelay(airliner));

            var delay = new KeyValuePair<DelayType, int>(DelayType.None, 0);
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

            int tAge = 100 - (age*2);

            Boolean delayed = Rnd.Next(100) > tAge;

            if (delayed)
                return Rnd.Next(0, age)*2;
            return 0;
        }

        //returns the delay time because of the weather for an airliner
        public static int GetAirlinerWeatherDelay(FleetAirliner airliner)
        {
            Airport departureAirport = airliner.CurrentFlight.GetDepartureAirport();

            int windFactor = 0;

            if (departureAirport.Weather[0] == null)
                return 0;

            switch (departureAirport.Weather[0].WindSpeed)
            {
                case Weather.eWindSpeed.StrongBreeze:
                    windFactor = 2;
                    break;
                case Weather.eWindSpeed.NearGale:
                    windFactor = 4;
                    break;
                case Weather.eWindSpeed.Gale:
                    windFactor = 6;
                    break;
                case Weather.eWindSpeed.StrongGale:
                    windFactor = 8;
                    break;
                case Weather.eWindSpeed.Storm:
                    windFactor = 10;
                    break;
                case Weather.eWindSpeed.ViolentStorm:
                    windFactor = 12;
                    break;
                case Weather.eWindSpeed.Hurricane:
                    windFactor = 14;
                    break;
            }

            if ((departureAirport.Weather[0].Temperatures[GameObject.GetInstance().GameTime.Hour].Precip == Weather.Precipitation.None ||
                 departureAirport.Weather[0].Temperatures[GameObject.GetInstance().GameTime.Hour].Precip == Weather.Precipitation.LightRain) && windFactor == 0)
                return 0;

            int weatherFactor = 0;

            switch (departureAirport.Weather[0].Temperatures[GameObject.GetInstance().GameTime.Hour].Precip)
            {
                case Weather.Precipitation.IsolatedSnow:
                    weatherFactor = 2;
                    break;
                case Weather.Precipitation.LightSnow:
                    weatherFactor = 4;
                    break;
                case Weather.Precipitation.HeavySnow:
                    weatherFactor = 8;
                    break;
                case Weather.Precipitation.Fog:
                    weatherFactor = 4;
                    break;
                case Weather.Precipitation.Sleet:
                    weatherFactor = 4;
                    break;
                case Weather.Precipitation.FreezingRain:
                    weatherFactor = 6;
                    break;
                case Weather.Precipitation.MixedRainAndSnow:
                    weatherFactor = 6;
                    break;
                case Weather.Precipitation.IsolatedRain:
                    weatherFactor = 1;
                    break;
                case Weather.Precipitation.LightRain:
                    weatherFactor = 2;
                    break;
                case Weather.Precipitation.HeavyRain:
                    weatherFactor = 4;
                    break;
                case Weather.Precipitation.IsolatedThunderstorms:
                    weatherFactor = 5;
                    break;
                case Weather.Precipitation.Thunderstorms:
                    weatherFactor = 8;
                    break;
            }

            int delayTime = Rnd.Next((weatherFactor + windFactor), (weatherFactor + windFactor)*12);

            return delayTime;
        }

        /* clears the statistics for all fleet airliners
        */

        public static void ClearAirlinerStatistics()
        {
            foreach (Airline airline in Airlines.GetAllAirlines())
                foreach (FleetAirliner airliner in airline.Fleet)
                    airliner.Statistics.Clear();
        }

        //creates the stop over route based on the main route
        public static StopoverRoute CreateStopoverRoute(Airport dest1, Airport stopover, Airport dest2, Route mainroute, Boolean oneLegged, Route.RouteType type)
        {
            var stopoverRoute = new StopoverRoute(stopover);

            Guid id = Guid.NewGuid();

            if (!oneLegged)
            {
                if (mainroute.Type == Route.RouteType.Passenger || mainroute.Type == Route.RouteType.Mixed)
                {
                    var routeLegTwo = new PassengerRoute(id.ToString(), dest1, stopover, GameObject.GetInstance().GameTime, 0);

                    foreach (RouteAirlinerClass aClass in ((PassengerRoute) mainroute).Classes)
                    {
                        //routeLegTwo.getRouteAirlinerClass(aClass.Type).FarePrice = aClass.FarePrice;
                        routeLegTwo.GetRouteAirlinerClass(aClass.Type).FarePrice = PassengerHelpers.GetPassengerPrice(dest1, stopover)*GeneralHelpers.ClassToPriceFactor(aClass.Type);

                        foreach (RouteFacility facility in aClass.GetFacilities())
                            routeLegTwo.GetRouteAirlinerClass(aClass.Type).AddFacility(facility);

                        routeLegTwo.GetRouteAirlinerClass(aClass.Type).Seating = aClass.Seating;
                    }


                    stopoverRoute.AddLeg(routeLegTwo);
                }
                if (mainroute.Type == Route.RouteType.Cargo || mainroute.Type == Route.RouteType.Mixed)
                {
                    var routeLegTwo = new CargoRoute(id.ToString(), dest1, stopover, GameObject.GetInstance().GameTime, ((CargoRoute) mainroute).PricePerUnit);

                    stopoverRoute.AddLeg(routeLegTwo);
                }
            }

            if (mainroute.Type == Route.RouteType.Mixed || mainroute.Type == Route.RouteType.Passenger)
            {
                id = Guid.NewGuid();

                var routeLegOne = new PassengerRoute(id.ToString(), stopover, dest2, GameObject.GetInstance().GameTime, 0);

                foreach (RouteAirlinerClass aClass in ((PassengerRoute) mainroute).Classes)
                {
                    //routeLegOne.getRouteAirlinerClass(aClass.Type).FarePrice = aClass.FarePrice;

                    routeLegOne.GetRouteAirlinerClass(aClass.Type).FarePrice = PassengerHelpers.GetPassengerPrice(stopover, dest2)*GeneralHelpers.ClassToPriceFactor(aClass.Type);


                    foreach (RouteFacility facility in aClass.GetFacilities())
                        routeLegOne.GetRouteAirlinerClass(aClass.Type).AddFacility(facility);

                    routeLegOne.GetRouteAirlinerClass(aClass.Type).Seating = aClass.Seating;
                }

                stopoverRoute.AddLeg(routeLegOne);
            }
            if (mainroute.Type == Route.RouteType.Cargo || mainroute.Type == Route.RouteType.Mixed)
            {
                var routeLegOne = new CargoRoute(id.ToString(), stopover, dest2, GameObject.GetInstance().GameTime, ((CargoRoute) mainroute).PricePerUnit);

                stopoverRoute.AddLeg(routeLegOne);
            }


            return stopoverRoute;
        }

        public static void CreateStopoverRoute(Route route, Airport stopover1, Airport stopover2 = null)
        {
            if (stopover1 != null)
            {
                if (stopover2 != null)
                {
                    route.AddStopover(CreateStopoverRoute(route.Destination1, stopover1, stopover2, route, false, route.Type));
                    route.AddStopover(CreateStopoverRoute(stopover1, stopover2, route.Destination2, route, true, route.Type));
                }
                else
                    route.AddStopover(CreateStopoverRoute(route.Destination1, stopover1, route.Destination2, route, false, route.Type));
            }

            /*
            if (stopover1 != null)
            {
                if (stopover2 != null)
                    route.addStopover(FleetAirlinerHelpers.CreateStopoverRoute(route.Destination1, stopover1, stopover2, route, false, route.Type));
                else
                    route.addStopover(FleetAirlinerHelpers.CreateStopoverRoute(route.Destination1, stopover1, route.Destination2, route, false, route.Type));
            }

            if (stopover2 != null && stopover1!=null)
            {
               route.addStopover(FleetAirlinerHelpers.CreateStopoverRoute(stopover1, stopover2, route.Destination2, route, true, route.Type));
              
            }
             * */
        }

        //returns the minimum time between flights for an airliner
        public static TimeSpan GetMinTimeBetweenFlights(FleetAirliner airliner)
        {
            return GetMinTimeBetweenFlights(airliner.Airliner.GetTotalSeatCapacity());
        }

        public static TimeSpan GetMinTimeBetweenFlights(AirlinerType type)
        {
            if (type == null || type is AirlinerCargoType)
                return GetMinTimeBetweenFlights(0);

            return GetMinTimeBetweenFlights(((AirlinerPassengerType) type).MaxSeatingCapacity);
        }

        private static TimeSpan GetMinTimeBetweenFlights(int passengers)
        {
            var minTime = new TimeSpan(0, 30, 0);

            if (passengers > 200)
                return minTime.Add(minTime);
            return minTime;
        }

        //sets the flights stats for an airliner
        public static void SetFlightStats(FleetAirliner airliner)
        {
            TimeSpan flightTime = MathHelpers.GetFlightTime(airliner.CurrentFlight.Entry.DepartureAirport.Profile.Coordinates.ConvertToGeoCoordinate(),
                                                            airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates.ConvertToGeoCoordinate(), airliner.Airliner.Type);
            DateTime landingTime = airliner.CurrentFlight.FlightTime.Add(flightTime);

            airliner.Airliner.FlownHours = airliner.Airliner.FlownHours.Add(flightTime);

            Airport dest = airliner.CurrentFlight.Entry.Destination.Airport;
            Airport dept = airliner.CurrentFlight.Entry.DepartureAirport;

            //canellation and ontime-percent
            double cancellationPercent = airliner.Airliner.Airline.Statistics.GetStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Cancellations"))/
                                         (airliner.Airliner.Airline.Statistics.GetStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Arrivals")) +
                                          airliner.Airliner.Airline.Statistics.GetStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Cancellations")));
            airliner.Airliner.Airline.Statistics.SetStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Cancellation%"), cancellationPercent*100);

            Boolean isOnTime = landingTime.Subtract(airliner.CurrentFlight.GetScheduledLandingTime()).Minutes < 15;

            if (isOnTime)
                airliner.Airliner.Airline.Statistics.AddStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("On-Time"), 1);

            airliner.Airliner.Airline.Statistics.AddStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Arrivals"), 1);
            dest.Statistics.AddStatisticsValue(GameObject.GetInstance().GameTime.Year, airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Arrivals"), 1);
            airliner.CurrentFlight.Entry.TimeTable.Route.Statistics.AddStatisticsValue(StatisticsTypes.GetStatisticsType("Cargo"), 1);
            airliner.Statistics.AddStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Arrivals"), 1);

            double onTimePercent = airliner.Airliner.Airline.Statistics.GetStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("On-Time"))/
                                   airliner.Airliner.Airline.Statistics.GetStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Arrivals"));
            airliner.Airliner.Airline.Statistics.SetStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("On-Time%"), onTimePercent*100);


            if (airliner.CurrentFlight.IsCargoFlight())
            {
                airliner.CurrentFlight.Entry.TimeTable.Route.Statistics.AddStatisticsValue(StatisticsTypes.GetStatisticsType("Cargo"), (int) airliner.CurrentFlight.Cargo);
                double routePassengers = airliner.CurrentFlight.Entry.TimeTable.Route.Statistics.GetStatisticsValue(StatisticsTypes.GetStatisticsType("Cargo"));
                double routeDepartures = airliner.CurrentFlight.Entry.TimeTable.Route.Statistics.GetStatisticsValue(StatisticsTypes.GetStatisticsType("Arrivals"));
                airliner.CurrentFlight.Entry.TimeTable.Route.Statistics.SetStatisticsValue(StatisticsTypes.GetStatisticsType("Cargo%"), (int) (routePassengers/routeDepartures));

                double cargocapacity = 0;

                var type = airliner.Airliner.Type as AirlinerCargoType;
                if (type != null)
                    cargocapacity = type.CargoSize;

                var combiType = airliner.Airliner.Type as AirlinerCombiType;
                if (combiType != null)
                    cargocapacity = combiType.CargoSize;

                airliner.CurrentFlight.Entry.TimeTable.Route.Statistics.AddStatisticsValue(StatisticsTypes.GetStatisticsType("Capacity"), (int) cargocapacity);

                double airlinerCargo = airliner.Statistics.GetStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Cargo"));
                double airlinerDepartures = airliner.Statistics.GetStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Arrivals"));
                airliner.Statistics.SetStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Cargo%"), (int) (airlinerCargo/airlinerDepartures));

                airliner.Airliner.Airline.Statistics.AddStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Cargo"), airliner.CurrentFlight.Cargo);
                airliner.Statistics.AddStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Cargo"), airliner.CurrentFlight.Cargo);
                dest.Statistics.AddStatisticsValue(GameObject.GetInstance().GameTime.Year, airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Cargo"), (int) airliner.CurrentFlight.Cargo);

                double destCargo = dest.Statistics.GetStatisticsValue(GameObject.GetInstance().GameTime.Year, airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Cargo"));
                double destDepartures = dest.Statistics.GetStatisticsValue(GameObject.GetInstance().GameTime.Year, airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Arrivals"));
                dest.Statistics.SetStatisticsValue(GameObject.GetInstance().GameTime.Year, airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Cargo%"), (int) (destCargo/destDepartures));

                double airlineCargo = airliner.Airliner.Airline.Statistics.GetStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Cargo"));
                double airlineDepartures = airliner.Airliner.Airline.Statistics.GetStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Arrivals"));
                airliner.Airliner.Airline.Statistics.SetStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Cargo%"), (int) (airlineCargo/airlineDepartures));

                dept.AddCargoDestinationStatistics(dest, airliner.CurrentFlight.Cargo);
            }
            if (airliner.CurrentFlight.IsPassengerFlight())
            {
                foreach (FlightAirlinerClass fac in airliner.CurrentFlight.Classes)
                {
                    RouteAirlinerClass raClass = ((PassengerRoute) airliner.CurrentFlight.Entry.TimeTable.Route).GetRouteAirlinerClass(fac.AirlinerClass.Type);

                    airliner.CurrentFlight.Entry.TimeTable.Route.Statistics.AddStatisticsValue(raClass, StatisticsTypes.GetStatisticsType("Passengers"),
                                                                                               airliner.CurrentFlight.GetFlightAirlinerClass(raClass.Type).Passengers);
                    double routePassengers = airliner.CurrentFlight.Entry.TimeTable.Route.Statistics.GetStatisticsValue(raClass, StatisticsTypes.GetStatisticsType("Passengers"));
                    double routeDepartures = airliner.CurrentFlight.Entry.TimeTable.Route.Statistics.GetStatisticsValue(raClass, StatisticsTypes.GetStatisticsType("Arrivals"));
                    airliner.CurrentFlight.Entry.TimeTable.Route.Statistics.SetStatisticsValue(raClass, StatisticsTypes.GetStatisticsType("Passengers%"), (int) (routePassengers/routeDepartures));

                    airliner.CurrentFlight.Entry.TimeTable.Route.Statistics.AddStatisticsValue(raClass, StatisticsTypes.GetStatisticsType("Capacity"),
                                                                                               airliner.Airliner.GetAirlinerClass(raClass.Type).SeatingCapacity);
                }

                double airlinerPassengers = airliner.Statistics.GetStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Passengers"));
                double airlinerDepartures = airliner.Statistics.GetStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Arrivals"));
                airliner.Statistics.SetStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Passengers%"), (int) (airlinerPassengers/airlinerDepartures));


                airliner.Airliner.Airline.Statistics.AddStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Passengers"),
                                                                        airliner.CurrentFlight.GetTotalPassengers());
                airliner.Statistics.AddStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Passengers"), airliner.CurrentFlight.GetTotalPassengers());
                dest.Statistics.AddStatisticsValue(GameObject.GetInstance().GameTime.Year, airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Passengers"),
                                                   airliner.CurrentFlight.GetTotalPassengers());


                double destPassengers = dest.Statistics.GetStatisticsValue(GameObject.GetInstance().GameTime.Year, airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Passengers"));
                double destDepartures = dest.Statistics.GetStatisticsValue(GameObject.GetInstance().GameTime.Year, airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Arrivals"));
                dest.Statistics.SetStatisticsValue(GameObject.GetInstance().GameTime.Year, airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Passengers%"),
                                                   (int) (destPassengers/destDepartures));

                double airlinePassengers = airliner.Airliner.Airline.Statistics.GetStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Passengers"));
                double airlineDepartures = airliner.Airliner.Airline.Statistics.GetStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Arrivals"));
                airliner.Airliner.Airline.Statistics.SetStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Passengers%"),
                                                                        (int) (airlinePassengers/airlineDepartures));

                //the statistics for destination airport
                dept.AddPassengerDestinationStatistics(dest, airliner.CurrentFlight.GetTotalPassengers());
            }
        }

        //returns the flight crusing speed based on the wind
        public static int GetCruisingSpeed(FleetAirliner airliner)
        {
            Airport dest = airliner.CurrentFlight.Entry.Destination.Airport;
            Airport dept = airliner.CurrentFlight.GetDepartureAirport();

            double windFirstHalf = ((int) dept.Weather[0].WindSpeed)*GetWindInfluence(airliner, dept.Weather[0]);

            double windSecondHalf = ((int) dest.Weather[0].WindSpeed)*GetWindInfluence(airliner, dest.Weather[0]);

            int speed = Convert.ToInt32(((airliner.Airliner.CruisingSpeed + windFirstHalf) + (airliner.Airliner.CruisingSpeed + windSecondHalf))/2);

            return speed;
        }

        //returns if the wind is tail (1), head (-1), or from side (0)
        private static int GetWindInfluence(FleetAirliner airliner, Weather currentWeather)
        {
            double direction = MathHelpers.GetDirection(airliner.CurrentFlight.GetDepartureAirport().Profile.Coordinates.ConvertToGeoCoordinate(),
                                                        airliner.CurrentFlight.GetNextDestination().Profile.Coordinates.ConvertToGeoCoordinate());

            Weather.WindDirection windDirection = MathHelpers.GetWindDirectionFromDirection(direction);

            int windDirectionLenght = Enum.GetValues(typeof (Weather.WindDirection)).Length;
            int indexCurrentPosition = Array.IndexOf(Enum.GetValues(typeof (Weather.WindDirection)), windDirection);
            //int indexWeather = Array.IndexOf(Enum.GetValues(typeof(Weather.WindDirection)),currentWeather.WindSpeed);

            //check for tail wind
            Weather.WindDirection windTailLeft = indexCurrentPosition > 0 ? (Weather.WindDirection) indexCurrentPosition - 1 : (Weather.WindDirection) windDirectionLenght - 1;
            Weather.WindDirection windTailRight = indexCurrentPosition < windDirectionLenght - 1 ? (Weather.WindDirection) indexCurrentPosition + 1 : 0;

            if (windTailLeft == currentWeather.Direction || windTailRight == currentWeather.Direction || windDirection == currentWeather.Direction)
                return 1;

            Weather.WindDirection windOpposite = indexCurrentPosition - (windDirectionLenght/2) > 0
                                                     ? (Weather.WindDirection) indexCurrentPosition - (windDirectionLenght/2)
                                                     : (Weather.WindDirection) windDirectionLenght - 1 - indexCurrentPosition - (windDirectionLenght/2);
            int indexOpposite = Array.IndexOf(Enum.GetValues(typeof (Weather.WindDirection)), windOpposite);

            Weather.WindDirection windHeadLeft = indexOpposite > 0 ? (Weather.WindDirection) indexOpposite - 1 : (Weather.WindDirection) windDirectionLenght - 1;
            Weather.WindDirection windHeadRight = indexOpposite < windDirectionLenght - 1 ? (Weather.WindDirection) indexOpposite + 1 : 0;

            if (windHeadLeft == currentWeather.Direction || windHeadRight == currentWeather.Direction || windOpposite == currentWeather.Direction)
                return -1;

            return 0;
        }

        //returns the fuel expenses for an airliner
        public static double GetFuelExpenses(FleetAirliner airliner, double distance)
        {
            Airport departureAirport = airliner.CurrentFlight.GetDepartureAirport();

            double fuelPrice = AirportHelpers.GetFuelPrice(departureAirport);

            if (airliner.CurrentFlight.IsPassengerFlight())
            {
                int pax = airliner.CurrentFlight.GetTotalPassengers();

                double basePrice = fuelPrice*distance*((AirlinerPassengerType) airliner.Airliner.Type).MaxSeatingCapacity*airliner.Airliner.FuelConsumption*0.55;
                double paxPrice = fuelPrice*distance*airliner.Airliner.FuelConsumption*airliner.CurrentFlight.GetTotalPassengers()*0.45;
                double seatsPrice =
                    airliner.CurrentFlight.Classes.Sum(c => (airliner.Airliner.GetAirlinerClass(c.AirlinerClass.Type).GetFacility(AirlinerFacility.FacilityType.Seat).SeatUses - 1)*c.Passengers);
                return basePrice + paxPrice + seatsPrice;
            }
            else
            {
                double basePrice = fuelPrice*distance*((AirlinerCargoType) airliner.Airliner.Type).CargoSize*airliner.Airliner.FuelConsumption*0.55;
                double cargoPrice = fuelPrice*airliner.Airliner.FuelConsumption*airliner.CurrentFlight.Cargo*distance*0.45;

                return basePrice + cargoPrice;
            }
        }

        //converts a passenger airliner to a cargo airliner
        public static void ConvertPassengerToCargoAirliner(FleetAirliner airliner)
        {
            var oldType = (AirlinerPassengerType) airliner.Airliner.Type;

            double cargoSize = AirlinerHelpers.GetPassengerCargoSize(oldType);
            DateTime builtDate = GameObject.GetInstance().GameTime.AddDays(AirlinerHelpers.GetCargoConvertingDays(oldType));

            var newType = new AirlinerCargoType(oldType.Manufacturer, oldType.Name + "F", oldType.AirlinerFamily, oldType.CockpitCrew, cargoSize, oldType.CruisingSpeed, oldType.Range, oldType.Wingspan,
                                                oldType.Length, oldType.Weight, oldType.FuelConsumption, oldType.Price, oldType.MinRunwaylength, oldType.FuelCapacity, oldType.Body, oldType.RangeType,
                                                oldType.Engine, oldType.Produced, oldType.ProductionRate, false, false);

            airliner.Airliner.Type = newType;
            airliner.Airliner.BuiltDate = builtDate;
        }

        //does the maintenance of a given type, sends the invoice, updates the last/next maintenance, and improves the aircraft's damage
        //make sure you pass this function a string value of either "A" "B" "C" or "D" or it will throw an error!
        public static void DoMaintenance(FleetAirliner airliner)
        {
            if (airliner.SchedAMaintenance == GameObject.GetInstance().GameTime.Date)
            {
                double expense = (airliner.Airliner.GetValue()*0.01) + 2000;
                GameObject.GetInstance().AddHumanMoney((long) -expense);
                var maintCheck = new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Maintenances, -expense);
                AirlineHelpers.AddAirlineInvoice(airliner.Airliner.Airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Maintenances, -expense);
                airliner.Airliner.Condition += Rnd.Next(3, 10);
                if (airliner.Airliner.Condition > 100) airliner.Airliner.Condition = 100;
                airliner.LastAMaintenance = GameObject.GetInstance().GameTime;
                airliner.SchedAMaintenance = airliner.SchedAMaintenance.AddDays(airliner.AMaintenanceInterval);
                airliner.MaintenanceHistory.Add(maintCheck, "A");
            }

            if (airliner.SchedBMaintenance == GameObject.GetInstance().GameTime.Date)
            {
                double expense = (airliner.Airliner.GetValue()*0.02) + 4500;
                GameObject.GetInstance().AddHumanMoney((long) -expense);
                var maintCheck = new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Maintenances, -expense);
                AirlineHelpers.AddAirlineInvoice(airliner.Airliner.Airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Maintenances, -expense);
                airliner.Airliner.Condition += Rnd.Next(12, 20);
                if (airliner.Airliner.Condition > 100) airliner.Airliner.Condition = 100;
                airliner.LastBMaintenance = GameObject.GetInstance().GameTime;
                airliner.SchedBMaintenance = airliner.SchedBMaintenance.AddDays(airliner.BMaintenanceInterval);
                airliner.MaintenanceHistory.Add(maintCheck, "B");
            }

            if (airliner.SchedCMaintenance == GameObject.GetInstance().GameTime.Date)
            {
                double expense = (airliner.Airliner.GetValue()*0.025) + 156000;
                airliner.OOSDate = airliner.SchedCMaintenance.AddDays(airliner.Airliner.Condition + 20);
                GameObject.GetInstance().AddHumanMoney((long) -expense);
                var maintCheck = new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Maintenances, -expense);
                AirlineHelpers.AddAirlineInvoice(airliner.Airliner.Airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Maintenances, -expense);
                airliner.Airliner.Condition += Rnd.Next(20, 30);
                if (airliner.Airliner.Condition > 100) airliner.Airliner.Condition = 100;
                airliner.LastCMaintenance = GameObject.GetInstance().GameTime;
                airliner.SchedCMaintenance = airliner.CMaintenanceInterval > -1
                                                 ? airliner.SchedCMaintenance.AddMonths(airliner.CMaintenanceInterval)
                                                 : airliner.DueCMaintenance = GameObject.GetInstance().GameTime.AddMonths(18);
                airliner.MaintenanceHistory.Add(maintCheck, "C");
                foreach (Route r in airliner.Routes.ToList())
                {
                    airliner.MaintRoutes.Add(r);
                    airliner.Routes.Remove(r);
                }
            }

            if (airliner.SchedDMaintenance == GameObject.GetInstance().GameTime.Date)
            {
                double expense = (airliner.Airliner.GetValue()*0.03) + 1200000;
                airliner.OOSDate = airliner.SchedDMaintenance.AddDays(airliner.Airliner.Condition + 50);
                GameObject.GetInstance().AddHumanMoney((long) -expense);
                var maintCheck = new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Maintenances, -expense);
                AirlineHelpers.AddAirlineInvoice(airliner.Airliner.Airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Maintenances, -expense);
                airliner.Airliner.Condition += Rnd.Next(35, 50);
                if (airliner.Airliner.Condition > 100) airliner.Airliner.Condition = 100;
                airliner.LastDMaintenance = GameObject.GetInstance().GameTime;
                airliner.SchedDMaintenance = airliner.DMaintenanceInterval > -1
                                                 ? airliner.SchedDMaintenance.AddMonths(airliner.DMaintenanceInterval)
                                                 : airliner.DueDMaintenance = GameObject.GetInstance().GameTime.AddMonths(60);
                airliner.DueDMaintenance = GameObject.GetInstance().GameTime.AddMonths(60);
                airliner.MaintenanceHistory.Add(maintCheck, "D");
                foreach (Route r in airliner.Routes.ToList())
                {
                    airliner.MaintRoutes.Add(r);
                    airliner.Routes.Remove(r);
                }
            }
        }

        //restores routes removed for maintenance
        public static void RestoreMaintRoutes(FleetAirliner airliner)
        {
            if (airliner.OOSDate <= GameObject.GetInstance().GameTime)
            {
                foreach (Route r in airliner.MaintRoutes)
                {
                    airliner.Routes.Add(r);
                }

                airliner.MaintRoutes.Clear();
            }
        }

        //sets A and B check intervals
        public static void SetMaintenanceIntervals(FleetAirliner airliner, int a, int b)
        {
            airliner.AMaintenanceInterval = a;
            airliner.BMaintenanceInterval = b;
            airliner.CMaintenanceInterval = -1;
            airliner.DMaintenanceInterval = -1;
        }

        public static void SetMaintenanceIntervals(FleetAirliner airliner, int a, int b, int c)
        {
            if (airliner.CMaintenanceInterval == -1)
            {
                airliner.AMaintenanceInterval = a;
                airliner.BMaintenanceInterval = b;
                airliner.DMaintenanceInterval = c;
            }

            else if (airliner.DMaintenanceInterval == -1)
            {
                airliner.AMaintenanceInterval = a;
                airliner.BMaintenanceInterval = b;
                airliner.CMaintenanceInterval = c;
            }
        }

        public static void SetMaintenanceIntervals(FleetAirliner airliner, int a, int b, int c, int d)
        {
            airliner.AMaintenanceInterval = a;
            airliner.BMaintenanceInterval = b;
            airliner.CMaintenanceInterval = c;
            airliner.DMaintenanceInterval = d;
        }
    }

    //the helpers class for fleet airliner insurance
    public class FleetAirlinerInsurancesHelpers
    {
        //add insurance policy
        public static void CreatePolicy(Airline airline, FleetAirliner airliner, AirlinerInsurance.InsuranceType type, AirlinerInsurance.InsuranceScope scope, AirlinerInsurance.PaymentTerms terms,
                                        double length, int amount)
        {
            #region Method Setup

            var rnd = new Random();
            double hub = airline.GetHubs().Count()*0.1;
            var policy = new AirlinerInsurance(type, scope, terms, amount)
                {
                    InsuranceEffective = GameObject.GetInstance().GameTime,
                    InsuranceExpires = GameObject.GetInstance().GameTime.AddYears((int) length),
                    PolicyIndex = GameObject.GetInstance().GameTime.ToString(CultureInfo.InvariantCulture) + airline
                };
            switch (policy.InsTerms)
            {
                case AirlinerInsurance.PaymentTerms.Monthly:
                    policy.RemainingPayments = length*12;
                    break;
                case AirlinerInsurance.PaymentTerms.Quarterly:
                    policy.RemainingPayments = length*4;
                    break;
                case AirlinerInsurance.PaymentTerms.Biannual:
                    policy.RemainingPayments = length*2;
                    break;
                case AirlinerInsurance.PaymentTerms.Annual:
                    policy.RemainingPayments = length;
                    break;
            }
            //sets up multipliers based on the type and scope of insurance policy
            var typeMultipliers = new Dictionary<AirlinerInsurance.InsuranceType, double>();
            var scopeMultipliers = new Dictionary<AirlinerInsurance.InsuranceScope, double>();
            const double typeMLiability = 1;
            const double typeMGroundParked = 1.2;
            const double typeMGroundTaxi = 1.5;
            const double typeMGroundCombined = 1.8;
            const double typeMInFlight = 2.2;
            const double typeMFullCoverage = 2.7;

            const double scMAirport = 1;
            const double scMDomestic = 1.5;
            double scMHub = 1.5 + hub;
            double scMGlobal = 2.0 + hub;

            #endregion

            #region Domestic/Int'l Airport Counter

            int i = 0;
            int j = 0;
            foreach (Airport airport in GameObject.GetInstance().HumanAirline.Airports)
            {
                if (airport.Profile.Country != GameObject.GetInstance().HumanAirline.Profile.Country)
                {
                    i++;
                }
                else j++;
            }

            #endregion

            // all the decision making for monthly payment amounts and deductibles
            switch (type)
            {
                    #region Liability

                case AirlinerInsurance.InsuranceType.Liability:
                    switch (scope)
                    {
                        case AirlinerInsurance.InsuranceScope.Airport:
                            policy.Deductible = amount*0.005;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMLiability*scMAirport;
                            if (terms == AirlinerInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlinerInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlinerInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlinerInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;

                            break;

                        case AirlinerInsurance.InsuranceScope.Domestic:
                            policy.Deductible = amount*0.001;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMLiability*scMDomestic;
                            if (terms == AirlinerInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlinerInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlinerInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlinerInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;

                        case AirlinerInsurance.InsuranceScope.Hub:
                            policy.Deductible = amount*0.001;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMLiability*scMHub;
                            if (terms == AirlinerInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlinerInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlinerInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlinerInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;

                        case AirlinerInsurance.InsuranceScope.Global:
                            policy.Deductible = amount*0.001;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMLiability*scMGlobal;
                            if (terms == AirlinerInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlinerInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlinerInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlinerInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;
                    }
                    break;

                    #endregion

                    #region Ground Parked

                case AirlinerInsurance.InsuranceType.GroundParked:
                    switch (scope)
                    {
                        case AirlinerInsurance.InsuranceScope.Airport:
                            policy.Deductible = amount*0.005;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMGroundParked*scMAirport;
                            if (terms == AirlinerInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlinerInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlinerInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlinerInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;

                        case AirlinerInsurance.InsuranceScope.Domestic:
                            policy.Deductible = amount*0.001;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMGroundParked*scMDomestic;
                            if (terms == AirlinerInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlinerInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlinerInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlinerInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;

                        case AirlinerInsurance.InsuranceScope.Hub:
                            policy.Deductible = amount*0.001;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMGroundParked*scMHub;
                            if (terms == AirlinerInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlinerInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlinerInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlinerInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;

                        case AirlinerInsurance.InsuranceScope.Global:
                            policy.Deductible = amount*0.001;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMGroundParked*scMGlobal;
                            if (terms == AirlinerInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlinerInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlinerInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlinerInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;
                    }
                    break;

                    #endregion

                    #region Ground Taxi

                case AirlinerInsurance.InsuranceType.GroundTaxi:
                    switch (scope)
                    {
                        case AirlinerInsurance.InsuranceScope.Airport:
                            policy.Deductible = amount*0.005;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMGroundTaxi*scMAirport;
                            if (terms == AirlinerInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlinerInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlinerInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlinerInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;

                        case AirlinerInsurance.InsuranceScope.Domestic:
                            policy.Deductible = amount*0.001;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMGroundTaxi*scMDomestic;
                            if (terms == AirlinerInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlinerInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlinerInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlinerInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;

                        case AirlinerInsurance.InsuranceScope.Hub:
                            policy.Deductible = amount*0.001;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMGroundTaxi*scMHub;
                            if (terms == AirlinerInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlinerInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlinerInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlinerInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;

                        case AirlinerInsurance.InsuranceScope.Global:
                            policy.Deductible = amount*0.001;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMGroundTaxi*scMGlobal;
                            if (terms == AirlinerInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlinerInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlinerInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlinerInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;
                    }
                    break;

                    #endregion

                    #region Ground Combined

                case AirlinerInsurance.InsuranceType.CombinedGround:
                    switch (scope)
                    {
                        case AirlinerInsurance.InsuranceScope.Airport:
                            policy.Deductible = amount*0.005;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMGroundCombined*scMAirport;
                            if (terms == AirlinerInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlinerInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlinerInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlinerInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;

                        case AirlinerInsurance.InsuranceScope.Domestic:
                            policy.Deductible = amount*0.001;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMGroundCombined*scMDomestic;
                            if (terms == AirlinerInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlinerInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlinerInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlinerInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;

                        case AirlinerInsurance.InsuranceScope.Hub:
                            policy.Deductible = amount*0.001;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMGroundCombined*scMHub;
                            if (terms == AirlinerInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlinerInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlinerInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlinerInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;

                        case AirlinerInsurance.InsuranceScope.Global:
                            policy.Deductible = amount*0.001;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMGroundCombined*scMGlobal;
                            if (terms == AirlinerInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlinerInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlinerInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlinerInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;
                    }
                    break;

                    #endregion

                    #region In Flight

                case AirlinerInsurance.InsuranceType.InFlight:
                    switch (scope)
                    {
                        case AirlinerInsurance.InsuranceScope.Airport:
                            policy.Deductible = amount*0.005;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMInFlight*scMAirport;
                            if (terms == AirlinerInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlinerInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlinerInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlinerInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;

                        case AirlinerInsurance.InsuranceScope.Domestic:
                            policy.Deductible = amount*0.001;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMInFlight*scMDomestic;
                            if (terms == AirlinerInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlinerInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlinerInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlinerInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;

                        case AirlinerInsurance.InsuranceScope.Hub:
                            policy.Deductible = amount*0.001;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMInFlight*scMHub;
                            if (terms == AirlinerInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlinerInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlinerInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlinerInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;

                        case AirlinerInsurance.InsuranceScope.Global:
                            policy.Deductible = amount*0.001;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMInFlight*scMGlobal;
                            if (terms == AirlinerInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlinerInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlinerInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlinerInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;
                    }

                    break;

                    #endregion

                    #region Full Coverage

                case AirlinerInsurance.InsuranceType.FullCoverage:
                    switch (scope)
                    {
                        case AirlinerInsurance.InsuranceScope.Airport:
                            policy.Deductible = amount*0.005;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMFullCoverage*scMAirport;
                            if (terms == AirlinerInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlinerInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlinerInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlinerInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;

                        case AirlinerInsurance.InsuranceScope.Domestic:
                            policy.Deductible = amount*0.001;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMFullCoverage*scMDomestic;
                            if (terms == AirlinerInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlinerInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlinerInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlinerInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;

                        case AirlinerInsurance.InsuranceScope.Hub:
                            policy.Deductible = amount*0.001;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMFullCoverage*scMHub;
                            if (terms == AirlinerInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlinerInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlinerInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlinerInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;

                        case AirlinerInsurance.InsuranceScope.Global:
                            policy.Deductible = amount*0.001;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMFullCoverage*scMGlobal;
                            if (terms == AirlinerInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlinerInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlinerInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlinerInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;
                    }
                    break;

                    #endregion
            }
        }

        public static void AddPolicy(FleetAirliner airliner, AirlinerInsurance insurance)
        {
            airliner.InsurancePolicies.Add(insurance);
        }


        //extend or modify policy
        public static void ModifyPolicy(FleetAirliner airliner, string index, AirlinerInsurance newPolicy)
        {
            //AirlinerInsurance oldPolicy = airliner.InsurancePolicies[index];
            //use the index to compare the new policy passed in to the existing one and make changes
        }

        public static void CheckExpiredInsurance(Airline airline)
        {
            DateTime date = GameObject.GetInstance().GameTime;
            foreach (FleetAirliner airliner in airline.Fleet)
            {
                foreach (AirlinerInsurance policy in airliner.InsurancePolicies)
                {
                    if (policy.InsuranceExpires < GameObject.GetInstance().GameTime)
                        airliner.InsurancePolicies.Remove(policy);
                }
            }
        }

        public static void MakeInsurancePayment(FleetAirliner airliner, Airline airline)
        {
            foreach (AirlinerInsurance policy in airliner.InsurancePolicies)
            {
                if (policy.RemainingPayments > 0)
                {
                    if (policy.NextPaymentDue.Month == GameObject.GetInstance().GameTime.Month)
                    {
                        airline.Money -= policy.PaymentAmount;
                        //Invoice payment = new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Maintenances, policy.PaymentAmount);
                        AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Maintenances, -policy.PaymentAmount);
                        policy.RemainingPayments--;
                        switch (policy.InsTerms)
                        {
                            case AirlinerInsurance.PaymentTerms.Monthly:
                                policy.NextPaymentDue = GameObject.GetInstance().GameTime.AddMonths(1);
                                break;
                            case AirlinerInsurance.PaymentTerms.Quarterly:
                                policy.NextPaymentDue = GameObject.GetInstance().GameTime.AddMonths(3);
                                break;
                            case AirlinerInsurance.PaymentTerms.Biannual:
                                policy.NextPaymentDue = GameObject.GetInstance().GameTime.AddMonths(6);
                                break;
                            case AirlinerInsurance.PaymentTerms.Annual:
                                policy.NextPaymentDue = GameObject.GetInstance().GameTime.AddMonths(12);
                                break;
                        }
                    }
                }
            }
        }

        public static void FileInsuranceClaim(Airline airline, Airport airport, AirportFacilities facility)
        {
        }

        public static void ReceiveInsurancePayout(Airline airline, Airport airport)
        {
        }
    }
}