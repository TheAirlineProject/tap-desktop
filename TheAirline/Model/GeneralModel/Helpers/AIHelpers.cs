namespace TheAirline.Model.GeneralModel.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.AirlineModel;
    using TheAirline.Model.AirlineModel.SubsidiaryModel;
    using TheAirline.Model.AirlinerModel;
    using TheAirline.Model.AirlinerModel.RouteModel;
    using TheAirline.Model.AirportModel;
    using TheAirline.Model.PassengerModel;

    //the helpers class for the AI
    public class AIHelpers
    {
        #region Static Fields

        private static readonly Random rnd = new Random();

        #endregion

        #region Public Methods and Operators
        public static Boolean WillTakeSpecialContract(Airline airline, SpecialContractType contract)
        {
            return false;
        }

        public static Boolean CanJoinAlliance(Airline airline, Alliance alliance)
        {
            IEnumerable<Country> sameCountries =
                alliance.Members.SelectMany(m => m.Airline.Airports)
                    .Select(a => a.Profile.Country)
                    .Distinct()
                    .Intersect(airline.Airports.Select(a => a.Profile.Country).Distinct());
            IEnumerable<Airport> sameDestinations =
                alliance.Members.SelectMany(m => m.Airline.Airports).Distinct().Intersect(airline.Airports);

            double airlineDestinations = airline.Airports.Count;
            double airlineRoutes = airline.Routes.Count;
            double airlineCountries = airline.Airports.Select(a => a.Profile.Country).Distinct().Count();
            double airlineAlliances = airline.Alliances.Count;

            double allianceRoutes = alliance.Members.SelectMany(m => m.Airline.Routes).Count();

            //declines if airline is much smaller than alliance
            if (airlineRoutes * 5 < allianceRoutes)
            {
                return false;
            }

            //declines if there is a match for 75% of the airline and alliance destinations
            if (sameDestinations.Count() >= airlineDestinations * 0.75)
            {
                return false;
            }

            //declines if there is a match for 75% of the airline and alliance countries
            if (sameCountries.Count() >= airlineCountries * 0.75)
            {
                return false;
            }

            return true;
        }

        public static Boolean CanRemoveFromAlliance(Airline remover, Airline toremove, Alliance alliance)
        {
            return remover.getValue() > toremove.getValue() * 0.9;
        }

        public static RouteTimeTable CreateAirlinerRouteTimeTable(
            Route route,
            FleetAirliner airliner,
            int flightsPerDay,
            string flightCode1,
            string flightCode2)
        {
            int startHour = 6;
            int endHour = 22;

            TimeSpan routeFlightTime = route.getFlightTime(airliner.Airliner.Type);

            TimeSpan minFlightTime = routeFlightTime.Add(FleetAirlinerHelpers.GetMinTimeBetweenFlights(airliner));

            var minDelayMinutes = (int)FleetAirlinerHelpers.GetMinTimeBetweenFlights(airliner).TotalMinutes;

            int startMinutes =
                Convert.ToInt16(((endHour - startHour) * 60) - (minFlightTime.TotalMinutes * flightsPerDay * 2));

            if (startMinutes < 0)
            {
                startMinutes = 0;
            }

            TimeSpan flightTime = new TimeSpan(startHour, 0, 0).Add(new TimeSpan(0, startMinutes / 2, 0));

            return CreateAirlinerRouteTimeTable(
                route,
                airliner,
                flightsPerDay,
                true,
                minDelayMinutes,
                flightTime,
                flightCode1,
                flightCode2);
        }

        public static RouteTimeTable CreateAirlinerRouteTimeTable(
            Route route,
            FleetAirliner airliner,
            int numberOfFlights,
            Boolean isDaily,
            int delayMinutes,
            TimeSpan startTime,
            string flightCode1,
            string flightCode2)
        {
            var delayTime = new TimeSpan(0, delayMinutes, 0);
            var timeTable = new RouteTimeTable(route);

            TimeSpan routeFlightTime = route.getFlightTime(airliner.Airliner.Type);

            TimeSpan minFlightTime = routeFlightTime.Add(delayTime);

            if (minFlightTime.Hours < 12 && minFlightTime.Days < 1 && isDaily)
            {
                var flightTime = new TimeSpan(startTime.Hours, startTime.Minutes, startTime.Seconds);
                //new TimeSpan(startHour, 0, 0).Add(new TimeSpan(0, startMinutes / 2, 0));

                for (int i = 0; i < numberOfFlights; i++)
                {
                    timeTable.addDailyEntries(new RouteEntryDestination(route.Destination2, flightCode1), flightTime);

                    flightTime = flightTime.Add(minFlightTime);

                    timeTable.addDailyEntries(new RouteEntryDestination(route.Destination1, flightCode2), flightTime);

                    flightTime = flightTime.Add(minFlightTime);
                }
            }
            else
            {
                if (isDaily || minFlightTime.Hours >= 12 || minFlightTime.Days >= 1)
                {
                    DayOfWeek day = 0;

                    int outTime = 15 * rnd.Next(-12, 12);
                    int homeTime = 15 * rnd.Next(-12, 12);

                    for (int i = 0; i < 3; i++)
                    {
                        Gate outboundGate = GetFreeAirlineGate(
                            airliner.Airliner.Airline,
                            route.Destination1,
                            day,
                            new TimeSpan(12, 0, 0).Add(new TimeSpan(0, outTime, 0)));
                        timeTable.addEntry(
                            new RouteTimeTableEntry(
                                timeTable,
                                day,
                                new TimeSpan(12, 0, 0).Add(new TimeSpan(0, outTime, 0)),
                                new RouteEntryDestination(route.Destination2, flightCode1),
                                outboundGate));

                        day += 2;
                    }

                    day = (DayOfWeek)1;

                    for (int i = 0; i < 3; i++)
                    {
                        Gate outboundGate = GetFreeAirlineGate(
                            airliner.Airliner.Airline,
                            route.Destination2,
                            day,
                            new TimeSpan(12, 0, 0).Add(new TimeSpan(0, homeTime, 0)));

                        timeTable.addEntry(
                            new RouteTimeTableEntry(
                                timeTable,
                                day,
                                new TimeSpan(12, 0, 0).Add(new TimeSpan(0, homeTime, 0)),
                                new RouteEntryDestination(route.Destination1, flightCode2),
                                outboundGate));

                        day += 2;
                    }
                }
                else
                {
                    var day = (DayOfWeek)(7 - numberOfFlights / 2);

                    for (int i = 0; i < numberOfFlights; i++)
                    {
                        var flightTime = new TimeSpan(startTime.Hours, startTime.Minutes, startTime.Seconds);

                        timeTable.addEntry(
                            new RouteTimeTableEntry(
                                timeTable,
                                day,
                                flightTime,
                                new RouteEntryDestination(route.Destination2, flightCode1)));

                        flightTime = flightTime.Add(minFlightTime);

                        timeTable.addEntry(
                            new RouteTimeTableEntry(
                                timeTable,
                                day,
                                flightTime,
                                new RouteEntryDestination(route.Destination1, flightCode2)));

                        day++;

                        if (((int)day) > 6)
                        {
                            day = 0;
                        }
                    }
                }
            }

            foreach (RouteTimeTableEntry e in timeTable.Entries)
            {
                e.Airliner = airliner;

            }

        

            return timeTable;
        }

        public static RouteTimeTable CreateBusinessRouteTimeTable(
            Route route,
            FleetAirliner airliner,
            int flightsPerDay,
            string flightCode1,
            string flightCode2)
        {
            var timeTable = new RouteTimeTable(route);

            TimeSpan minFlightTime =
                MathHelpers.GetFlightTime(
                    route.Destination1.Profile.Coordinates.convertToGeoCoordinate(),
                    route.Destination2.Profile.Coordinates.convertToGeoCoordinate(),
                    airliner.Airliner.Type)
                    .Add(new TimeSpan(FleetAirlinerHelpers.GetMinTimeBetweenFlights(airliner).Ticks));

            int startHour = 6;
            int endHour = 10;

            int maxHours = endHour - startHour; //entries.Airliners == null

            int startMinutes = Convert.ToInt16((maxHours * 60) - (minFlightTime.TotalMinutes * flightsPerDay * 2));

            if (startMinutes < 0)
            {
                startMinutes = 0;
            }

            //morning
            TimeSpan flightTime = new TimeSpan(startHour, 0, 0).Add(new TimeSpan(0, startMinutes / 2, 0));

            for (int i = 0; i < flightsPerDay; i++)
            {
                timeTable.addWeekDailyEntries(new RouteEntryDestination(route.Destination2, flightCode1), flightTime);

                flightTime = flightTime.Add(minFlightTime);

                timeTable.addWeekDailyEntries(new RouteEntryDestination(route.Destination1, flightCode2), flightTime);

                flightTime = flightTime.Add(minFlightTime);
            }
            //evening
            startHour = 18;
            flightTime = new TimeSpan(startHour, 0, 0).Add(new TimeSpan(0, startMinutes / 2, 0));
            for (int i = 0; i < flightsPerDay; i++)
            {
                timeTable.addWeekDailyEntries(new RouteEntryDestination(route.Destination2, flightCode1), flightTime);

                flightTime = flightTime.Add(minFlightTime);

                timeTable.addWeekDailyEntries(new RouteEntryDestination(route.Destination1, flightCode2), flightTime);

                flightTime = flightTime.Add(minFlightTime);
            }

            if (timeTable.Entries.Count == 0)
            {
                flightCode1 = "TT";
            }

            foreach (RouteTimeTableEntry e in timeTable.Entries)
            {
                e.Airliner = airliner;
            }

            return timeTable;
        }

        public static void CreateCargoRouteTimeTable(Route route, FleetAirliner airliner)
        {
            TimeSpan routeFlightTime =
                MathHelpers.GetFlightTime(
                    route.Destination1.Profile.Coordinates.convertToGeoCoordinate(),
                    route.Destination2.Profile.Coordinates.convertToGeoCoordinate(),
                    airliner.Airliner.Type);
            TimeSpan minFlightTime =
                routeFlightTime.Add(new TimeSpan(FleetAirlinerHelpers.GetMinTimeBetweenFlights(airliner).Ticks));

            int maxHours = 20 - 8; //from 08.00 to 20.00

            int flightsPerDay = Convert.ToInt16(maxHours * 60 / (2 * minFlightTime.TotalMinutes));

            string flightCode1 = airliner.Airliner.Airline.getNextFlightCode(0);
            string flightCode2 = airliner.Airliner.Airline.getNextFlightCode(1);

            route.TimeTable = CreateAirlinerRouteTimeTable(route, airliner, flightsPerDay, flightCode1, flightCode2);
        }

        public static Boolean CreateRouteTimeTable(Route route, List<FleetAirliner> airliners)
        {
            var totalFlightTime = new TimeSpan(airliners.Sum(a => route.getFlightTime(a.Airliner.Type).Ticks));
            var maxFlightTime = new TimeSpan(airliners.Max(a => route.getFlightTime(a.Airliner.Type)).Ticks);

            int maxHours = 22 - 6 - (int)Math.Ceiling(maxFlightTime.TotalMinutes); //from 06.00 to 22.00

            if (totalFlightTime.TotalMinutes > maxHours)
            {
                return false;
            }

            var startTime = new TimeSpan(6, 0, 0);

            foreach (FleetAirliner airliner in airliners)
            {
                string flightCode1 = airliner.Airliner.Airline.getNextFlightCode(0);
                string flightCode2 = airliner.Airliner.Airline.getNextFlightCode(1);

                CreateAirlinerRouteTimeTable(
                    route,
                    airliner,
                    1,
                    true,
                    (int)FleetAirlinerHelpers.GetMinTimeBetweenFlights(airliner).TotalMinutes,
                    startTime,
                    flightCode1,
                    flightCode2);

                startTime = startTime.Add(route.getFlightTime(airliner.Airliner.Type));
            }

            return true;
        }

        public static void CreateRouteTimeTable(Route route, FleetAirliner airliner)
        {
            TimeSpan routeFlightTime =
                MathHelpers.GetFlightTime(
                    route.Destination1.Profile.Coordinates.convertToGeoCoordinate(),
                    route.Destination2.Profile.Coordinates.convertToGeoCoordinate(),
                    airliner.Airliner.Type);
            TimeSpan minFlightTime =
                routeFlightTime.Add(new TimeSpan(FleetAirlinerHelpers.GetMinTimeBetweenFlights(airliner).Ticks));

            int maxHours = 22 - 6; //from 06.00 to 22.00

            int flightsPerDay = Convert.ToInt16(maxHours * 60 / (2 * minFlightTime.TotalMinutes));

            string flightCode1 = airliner.Airliner.Airline.getNextFlightCode(0);
            string flightCode2 = airliner.Airliner.Airline.getNextFlightCode(1);

            route.TimeTable = CreateAirlinerRouteTimeTable(route, airliner, flightsPerDay, flightCode1, flightCode2);
        }

        public static Boolean DoAcceptAllianceInvitation(Airline airline, Alliance alliance)
        {
            //a subsidiary of a human airline will always accept an invitation to an alliance where the parent is a member
            if (alliance.IsHumanAlliance && airline.IsHuman)
            {
                return true;
            }

            IEnumerable<Country> sameCountries =
                alliance.Members.SelectMany(m => m.Airline.Airports)
                    .Select(a => a.Profile.Country)
                    .Distinct()
                    .Intersect(airline.Airports.Select(a => a.Profile.Country).Distinct());
            IEnumerable<Airport> sameDestinations =
                alliance.Members.SelectMany(m => m.Airline.Airports).Distinct().Intersect(airline.Airports);

            double airlineDestinations = airline.Airports.Count;
            double airlineRoutes = airline.Routes.Count;
            double airlineCountries = airline.Airports.Select(a => a.Profile.Country).Distinct().Count();
            double airlineAlliances = airline.Alliances.Count;

            double allianceRoutes = alliance.Members.SelectMany(m => m.Airline.Routes).Count();

            //declines if invited airline is much larger than alliance
            if (airlineRoutes > 2 * allianceRoutes)
            {
                return false;
            }

            //declines if there is a match for 50% of the airline and alliance destinations
            if (sameDestinations.Count() >= airlineDestinations * 0.50)
            {
                return false;
            }

            //declines if there is a match for 75% of the airline and alliance countries
            if (sameCountries.Count() >= airlineCountries * 0.75)
            {
                return false;
            }

            //declines if the airline already are in "many" alliances - many == 2
            if (airlineAlliances > 2)
            {
                return false;
            }

            return true;
        }
        private static int GetEstimatedDemandPerFlight(Airport destination1, Airport destination2, Route.RouteType focus)
        {
            //int maxWeeklyDepartures = (int)(7 * new TimeSpan(24, 0, 0).TotalHours / flightTime.TotalHours);

            //int weeklyDepartures = (int)this.EstimatedDemand * 7 / pax;

            TimeSpan flightTime = MathHelpers.GetFlightTime(destination1.Profile.Coordinates.convertToGeoCoordinate(), destination2.Profile.Coordinates.convertToGeoCoordinate(), 500);

            double demand = destination1.getDestinationPassengersRate(destination2, AirlinerClass.ClassType.Economy_Class);

            int estimatedFlightsPerDay = 0;

            if (flightTime.TotalHours < 2) //business route
                estimatedFlightsPerDay = 2;

            else
                estimatedFlightsPerDay = Math.Max(1, (int)Math.Floor(new TimeSpan(12, 0, 0).TotalHours / (flightTime.Add(new TimeSpan(1, 0, 0)).TotalHours)));

            return (int)demand / estimatedFlightsPerDay;

        }
        public static KeyValuePair<Airliner, Boolean>? GetAirlinerForRoute(
            Airline airline,
            Airport destination1,
            Airport destination2,
            Boolean doLeasing,
            Route.RouteType focus,
            Boolean forStartdata = false)
        {

            int demand = GetEstimatedDemandPerFlight(destination1, destination2, focus);

            List<AirlinerType> airlineAircrafts = airline.Profile.PreferedAircrafts;

            double maxLoanTotal = 100000000;
            double distance = MathHelpers.GetDistance(
                destination1.Profile.Coordinates.convertToGeoCoordinate(),
                destination2.Profile.Coordinates.convertToGeoCoordinate());

            AirlinerType.TypeRange rangeType = GeneralHelpers.ConvertDistanceToRangeType(distance);

            List<Airliner> airliners;

            if (airlineAircrafts.Count > 0)
            {
                if (focus == Route.RouteType.Cargo)
                {
                    if (doLeasing)
                    {
                        airliners = Airliners.GetAirlinersForLeasing().FindAll(a => a.Type is AirlinerCargoType
                            && a.LeasingPrice * 2 < airline.Money && a.getAge() < 10 && distance < a.Range
                                     && !FlightRestrictions.HasRestriction(airline, a.Type, GameObject.GetInstance().GameTime) && airlineAircrafts.Contains(a.Type));

                        if (airliners.Count == 0)
                        {
                            airliners =
                                Airliners.GetAirlinersForSale(a => a.Type is AirlinerCargoType)
                                    .FindAll(
                                        a =>
                                            a.LeasingPrice * 2 < airline.Money && a.getAge() < 10 && distance < a.Range
                                           && !FlightRestrictions.HasRestriction(airline, a.Type, GameObject.GetInstance().GameTime) && airlineAircrafts.Contains(a.Type));
                        }
                    }
                    else
                    {
                        airliners =
                            Airliners.GetAirlinersForSale(a => a.Type is AirlinerCargoType)
                                .FindAll(
                                    a =>
                                        a.getPrice() < airline.Money - 1000000 && a.getAge() < 10
                                        && !FlightRestrictions.HasRestriction(airline, a.Type, GameObject.GetInstance().GameTime) && distance < a.Range && airlineAircrafts.Contains(a.Type));
                    }
                }
                else if (focus == Route.RouteType.Helicopter_Cargo)
                {
                    if (doLeasing)
                    {
                        airliners =
                            Airliners.GetAirlinersForSale(a => a.Type is AirlinerPassengerType && a.Type.TypeAirliner == AirlinerType.TypeOfAirliner.Helicopter_Cargo)
                                .FindAll(
                                    a =>
                                        a.LeasingPrice * 2 < airline.Money && a.getAge() < 10 && distance < a.Range
                                       && !FlightRestrictions.HasRestriction(airline, a.Type, GameObject.GetInstance().GameTime) && airlineAircrafts.Contains(a.Type));
                    }
                    else
                    {
                        airliners =
                            Airliners.GetAirlinersForSale(a => a.Type is AirlinerPassengerType && a.Type.TypeAirliner == AirlinerType.TypeOfAirliner.Helicopter_Cargo)
                                .FindAll(
                                    a =>
                                        a.getPrice() < airline.Money - 1000000 && a.getAge() < 10
                                       && !FlightRestrictions.HasRestriction(airline, a.Type, GameObject.GetInstance().GameTime) && distance < a.Range && airlineAircrafts.Contains(a.Type));
                    }
                }
                else if (focus == Route.RouteType.Helicopter_Passenger)
                {
                    if (doLeasing)
                    {
                        airliners =
                            Airliners.GetAirlinersForSale(a => a.Type is AirlinerPassengerType && a.Type.TypeAirliner == AirlinerType.TypeOfAirliner.Helicopter)
                                .FindAll(
                                    a =>
                                        a.LeasingPrice * 2 < airline.Money && a.getAge() < 10 && distance < a.Range
                                       && !FlightRestrictions.HasRestriction(airline, a.Type, GameObject.GetInstance().GameTime) && airlineAircrafts.Contains(a.Type));
                    }
                    else
                    {
                        airliners =
                            Airliners.GetAirlinersForSale(a => a.Type is AirlinerPassengerType && a.Type.TypeAirliner == AirlinerType.TypeOfAirliner.Helicopter)
                                .FindAll(
                                    a =>
                                        a.getPrice() < airline.Money - 1000000 && a.getAge() < 10
                                       && !FlightRestrictions.HasRestriction(airline, a.Type, GameObject.GetInstance().GameTime) && distance < a.Range && airlineAircrafts.Contains(a.Type));
                    }
                }
                else
                {
                    if (doLeasing)
                    {
                        airliners =
                           Airliners.GetAirlinersForLeasing()
                               .FindAll(
                                   a => a.Type is AirlinerPassengerType
                                       && a.LeasingPrice * 2 < airline.Money && a.getAge() < 10 && distance < a.Range
                                      && !FlightRestrictions.HasRestriction(airline, a.Type, GameObject.GetInstance().GameTime) && airlineAircrafts.Contains(a.Type));

                        if (airliners.Count == 0)
                        {
                            airliners =
                                Airliners.GetAirlinersForSale(a => a.Type is AirlinerPassengerType)
                                    .FindAll(
                                        a =>
                                            a.LeasingPrice * 2 < airline.Money && a.getAge() < 10 && distance < a.Range
                                          && !FlightRestrictions.HasRestriction(airline, a.Type, GameObject.GetInstance().GameTime) && airlineAircrafts.Contains(a.Type));
                        }
                    }
                    else
                    {
                        airliners =
                            Airliners.GetAirlinersForSale(a => a.Type is AirlinerPassengerType)
                                .FindAll(
                                    a =>
                                        a.getPrice() < airline.Money - 1000000 && a.getAge() < 10
                                        && !FlightRestrictions.HasRestriction(airline, a.Type, GameObject.GetInstance().GameTime) && distance < a.Range && airlineAircrafts.Contains(a.Type));
                    }
                }
            }
            else
            {
                if (focus == Route.RouteType.Cargo)
                {
                    if (doLeasing)
                    {
                        airliners = Airliners.GetAirlinersForLeasing()
                                .FindAll(
                                    a => a.Type is AirlinerCargoType
                                       && !FlightRestrictions.HasRestriction(airline, a.Type, GameObject.GetInstance().GameTime) && a.LeasingPrice * 2 < airline.Money && a.getAge() < 10 && distance < a.Range);

                        if (airliners.Count == 0)
                        {
                            airliners =
                                Airliners.GetAirlinersForSale(a => a.Type is AirlinerCargoType)
                                    .FindAll(
                                        a =>
                                            a.LeasingPrice * 2 < airline.Money && a.getAge() < 10 && distance < a.Range && !FlightRestrictions.HasRestriction(airline, a.Type, GameObject.GetInstance().GameTime));
                        }
                    }
                    else
                    {
                        airliners =
                            Airliners.GetAirlinersForSale(a => a.Type is AirlinerCargoType)
                                .FindAll(
                                    a =>
                                        a.getPrice() < airline.Money - 1000000 && a.getAge() < 10
                                        && distance < a.Range && !FlightRestrictions.HasRestriction(airline, a.Type, GameObject.GetInstance().GameTime));
                    }
                }
                else if (focus == Route.RouteType.Helicopter_Passenger)
                {
                    if (doLeasing)
                    {
                        airliners =
                            Airliners.GetAirlinersForSale(a => a.Type is AirlinerPassengerType && a.Type.TypeAirliner == AirlinerType.TypeOfAirliner.Helicopter)
                                .FindAll(
                                    a =>
                                        a.LeasingPrice * 2 < airline.Money && a.getAge() < 10 && distance < a.Range && !FlightRestrictions.HasRestriction(airline, a.Type, GameObject.GetInstance().GameTime));
                    }
                    else
                    {
                        airliners =
                            Airliners.GetAirlinersForSale(a => a.Type is AirlinerPassengerType && a.Type.TypeAirliner == AirlinerType.TypeOfAirliner.Helicopter)
                                .FindAll(
                                    a =>
                                        a.getPrice() < airline.Money - 1000000 && a.getAge() < 10
                                        && distance < a.Range && !FlightRestrictions.HasRestriction(airline, a.Type, GameObject.GetInstance().GameTime));
                    }
                }
                else if (focus == Route.RouteType.Helicopter_Cargo)
                {
                    if (doLeasing)
                    {
                        airliners =
                            Airliners.GetAirlinersForSale(a => a.Type.TypeAirliner == AirlinerType.TypeOfAirliner.Helicopter_Cargo)
                                .FindAll(
                                    a =>
                                        a.LeasingPrice * 2 < airline.Money && a.getAge() < 10 && distance < a.Range && !FlightRestrictions.HasRestriction(airline, a.Type, GameObject.GetInstance().GameTime));
                    }
                    else
                    {
                        airliners =
                            Airliners.GetAirlinersForSale(a => a.Type.TypeAirliner == AirlinerType.TypeOfAirliner.Helicopter_Cargo)
                                .FindAll(
                                    a =>
                                        a.getPrice() < airline.Money - 1000000 && a.getAge() < 10
                                        && distance < a.Range && !FlightRestrictions.HasRestriction(airline, a.Type, GameObject.GetInstance().GameTime));
                    }
                }
                else
                {
                    if (doLeasing)
                    {
                        airliners =
                            Airliners.GetAirlinersForSale()
                                .FindAll(
                                    a => a.Type is AirlinerPassengerType
                                        && a.LeasingPrice * 2 < airline.Money && a.getAge() < 10 && distance < a.Range && !FlightRestrictions.HasRestriction(airline, a.Type, GameObject.GetInstance().GameTime));

                        if (airliners.Count == 0)
                        {
                            airliners =
                                Airliners.GetAirlinersForSale(a => a.Type is AirlinerPassengerType)
                                    .FindAll(
                                        a =>
                                            a.LeasingPrice * 2 < airline.Money && a.getAge() < 10 && distance < a.Range && !FlightRestrictions.HasRestriction(airline, a.Type, GameObject.GetInstance().GameTime));
                        }
                    }
                    else
                    {
                        airliners =
                             Airliners.GetAirlinersForSale(a => a.Type is AirlinerPassengerType)
                                 .FindAll(
                                     a =>
                                         a.getPrice() < airline.Money - 1000000 && a.getAge() < 10
                                         && distance < a.Range && !FlightRestrictions.HasRestriction(airline, a.Type, GameObject.GetInstance().GameTime));
                    }
                }
            }
            if (airliners.Count > 0)
            {
                if (airline.MarketFocus == Airline.AirlineFocus.Global && (airline.AirlineRouteFocus == Route.RouteType.Passenger || airline.AirlineRouteFocus == Route.RouteType.Mixed))
                {
                    var majorAirliners = airliners.FindAll(a => a.Type.Manufacturer.IsMajor && a.Type is AirlinerPassengerType);

                    if (majorAirliners.Count > 0)
                        return new KeyValuePair<Airliner, Boolean>(majorAirliners.OrderBy(a => ((AirlinerPassengerType)a.Type).MaxSeatingCapacity - demand).ThenBy(a => a.Price).First(), true);
                }


                return new KeyValuePair<Airliner, Boolean>(airliners.OrderBy(a => a.Price).First(), false);
            }
            if (airline.Mentality == Airline.AirlineMentality.Aggressive || airline.Fleet.Count == 0 || forStartdata)
            {
                double airlineLoanTotal = airline.Loans.Sum(l => l.PaymentLeft);

                if (airlineLoanTotal < maxLoanTotal)
                {
                    List<Airliner> loanAirliners = new List<Airliner>();

                    if (airlineAircrafts.Count > 0)
                    {
                        if (focus == Route.RouteType.Cargo)
                        {
                            loanAirliners =
                                Airliners.GetAirlinersForSale(a => a.Type is AirlinerCargoType)
                                    .FindAll(
                                        a =>
                                            a.getPrice() < airline.Money + maxLoanTotal - airlineLoanTotal
                                            && distance < a.Range && airlineAircrafts.Contains(a.Type) && !FlightRestrictions.HasRestriction(airline, a.Type, GameObject.GetInstance().GameTime));
                        }
                        else if (focus == Route.RouteType.Helicopter_Cargo)
                        {
                            loanAirliners =
                               Airliners.GetAirlinersForSale(a => a.Type.TypeAirliner == AirlinerType.TypeOfAirliner.Helicopter_Cargo)
                                   .FindAll(
                                       a =>
                                           a.getPrice() < airline.Money + maxLoanTotal - airlineLoanTotal
                                           && distance < a.Range && airlineAircrafts.Contains(a.Type) && !FlightRestrictions.HasRestriction(airline, a.Type, GameObject.GetInstance().GameTime));
                        }
                        else if (focus == Route.RouteType.Helicopter_Passenger)
                        {
                            loanAirliners =
                               Airliners.GetAirlinersForSale(a => a.Type.TypeAirliner == AirlinerType.TypeOfAirliner.Helicopter)
                                   .FindAll(
                                       a =>
                                           a.getPrice() < airline.Money + maxLoanTotal - airlineLoanTotal
                                           && distance < a.Range && airlineAircrafts.Contains(a.Type) && !FlightRestrictions.HasRestriction(airline, a.Type, GameObject.GetInstance().GameTime));
                        }
                        else
                        {
                            loanAirliners =
                                Airliners.GetAirlinersForSale(a => a.Type is AirlinerPassengerType)
                                    .FindAll(
                                        a =>
                                            a.getPrice() < airline.Money + maxLoanTotal - airlineLoanTotal
                                            && distance < a.Range && airlineAircrafts.Contains(a.Type) && !FlightRestrictions.HasRestriction(airline, a.Type, GameObject.GetInstance().GameTime));
                        }
                    }

                    if (loanAirliners.Count == 0)
                    {
                        if (focus == Route.RouteType.Cargo)
                        {
                            loanAirliners =
                                Airliners.GetAirlinersForSale(a => a.Type is AirlinerCargoType)
                                    .FindAll(
                                        a =>
                                            a.getPrice() < airline.Money + maxLoanTotal - airlineLoanTotal
                                            && distance < a.Range && !FlightRestrictions.HasRestriction(airline, a.Type, GameObject.GetInstance().GameTime));
                        }
                        else if (focus == Route.RouteType.Helicopter_Passenger)
                        {
                            loanAirliners =
                               Airliners.GetAirlinersForSale(a => a.Type is AirlinerPassengerType && a.Type.TypeAirliner == AirlinerType.TypeOfAirliner.Helicopter)
                                   .FindAll(
                                       a =>
                                           a.getPrice() < airline.Money + maxLoanTotal - airlineLoanTotal
                                           && distance < a.Range && !FlightRestrictions.HasRestriction(airline, a.Type, GameObject.GetInstance().GameTime));
                        }
                        else if (focus == Route.RouteType.Helicopter_Cargo)
                        {
                            loanAirliners =
                           Airliners.GetAirlinersForSale(a => a.Type is AirlinerPassengerType && a.Type.TypeAirliner == AirlinerType.TypeOfAirliner.Helicopter_Cargo)
                               .FindAll(
                                   a =>
                                       a.getPrice() < airline.Money + maxLoanTotal - airlineLoanTotal
                                       && distance < a.Range && !FlightRestrictions.HasRestriction(airline, a.Type, GameObject.GetInstance().GameTime));
                 
                        }
                        else
                        {
                            loanAirliners =
                                Airliners.GetAirlinersForSale(a => a.Type is AirlinerPassengerType)
                                    .FindAll(
                                        a =>
                                            a.getPrice() < airline.Money + maxLoanTotal - airlineLoanTotal
                                            && distance < a.Range && !FlightRestrictions.HasRestriction(airline, a.Type, GameObject.GetInstance().GameTime));
                        }
                    }
                    if (loanAirliners.Count > 0)
                    {
                        if (airline.MarketFocus == Airline.AirlineFocus.Global && (airline.AirlineRouteFocus == Route.RouteType.Passenger || airline.AirlineRouteFocus == Route.RouteType.Mixed))
                        {
                            var loanMajorAirliners = loanAirliners.FindAll(a => a.Type.Manufacturer.IsMajor && a.Type is AirlinerPassengerType);

                            if (loanMajorAirliners.Count > 0)
                                return new KeyValuePair<Airliner, Boolean>(loanMajorAirliners.OrderBy(a => ((AirlinerPassengerType)a.Type).MaxSeatingCapacity - demand).ThenBy(a => a.Price).First(), true);
                        }
                        Airliner airliner = loanAirliners.OrderBy(a => a.Price).First();

                        if (airliner == null)
                        {
                            return null;
                        }

                        return new KeyValuePair<Airliner, Boolean>(airliner, true);
                    }
                    return null;
                }
                return null;
            }
            return null;
        }

        public static Airport GetDestinationAirport(Airline airline, Airport airport)
        {
            List<Airport> airports = GetDestinationAirports(airline, airport);

            if (airports.Count == 0)
            {
                return null;
            }
            return airports[0];
        }

        public static List<Airport> GetDestinationAirports(Airline airline, Airport airport)
        {

            if (airline.Profile.FocusAirports.Count > 0)
            {
                var focusAirports = airline.Profile.FocusAirports.Where(f => f != airport && !airline.Airports.Contains(f));

                if (focusAirports.Count() > 0)
                {
                    focusAirports = focusAirports.OrderBy(f => MathHelpers.GetDistance(f, airport));

                    return focusAirports.ToList();
                }
            }

            IEnumerable<long> airliners = from a in Airliners.GetAirlinersForSale() select a.Range;
            double maxDistance = airliners.Count() == 0 ? 5000 : airliners.Max();

            double minDistance = (from a in Airports.GetAirports(a => a != airport)
                                  select
                                      MathHelpers.GetDistance(
                                          a.Profile.Coordinates.convertToGeoCoordinate(),
                                          airport.Profile.Coordinates.convertToGeoCoordinate())).Min();

            List<Airport> airports =
                Airports.GetAirports(
                    a =>
                        airline.Airports.Find(ar => ar.Profile.Town == a.Profile.Town) == null
                        && !airline.Routes.Exists(r=>(r.Destination1 == a && r.Destination2 == airport) || (r.Destination2 == a && r.Destination1 == airport))                        
                        && AirlineHelpers.HasAirlineLicens(airline, airport, a)
                        && FlightRestrictions.IsAllowed(
                        airport, a,
                        GameObject.GetInstance().GameTime)
                        && !FlightRestrictions.HasRestriction(
                            a.Profile.Country,
                            airport.Profile.Country,
                            GameObject.GetInstance().GameTime,
                            FlightRestriction.RestrictionType.Flights)
                        && !FlightRestrictions.HasRestriction(
                            airport.Profile.Country,
                            a.Profile.Country,
                            GameObject.GetInstance().GameTime,
                            FlightRestriction.RestrictionType.Flights)
                        && !FlightRestrictions.HasRestriction(
                            airline,
                            a.Profile.Country,
                            airport.Profile.Country,
                            GameObject.GetInstance().GameTime));

            if (airline.AirlineRouteFocus == Route.RouteType.Helicopter_Passenger || airline.AirlineRouteFocus == Route.RouteType.Helicopter_Mixed || airline.AirlineRouteFocus == Route.RouteType.Helicopter_Cargo)
                airports = airports.Where(a => a.Runways.Exists(r => r.Type == Runway.RunwayType.Helipad)).ToList();

            if (airline.AirlineRouteFocus == Route.RouteType.Cargo || airline.AirlineRouteFocus == Route.RouteType.Helicopter_Cargo)
                airports = airports.Where(a => a.Terminals.AirportTerminals.Exists(t => t.Type == Terminal.TerminalType.Cargo)).ToList();

            List<Route> routes = airline.Routes.FindAll(r => r.Destination1 == airport || r.Destination2 == airport);

            Airline.AirlineFocus marketFocus = airline.MarketFocus;

            Route.RouteType focus = airline.AirlineRouteFocus;
           
            if (focus == Route.RouteType.Mixed)
                focus = rnd.Next(3) == 0 ? Route.RouteType.Cargo : Route.RouteType.Passenger;

            if (focus == Route.RouteType.Helicopter_Mixed)
                focus = rnd.Next(3) == 0 ? Route.RouteType.Helicopter_Cargo : Route.RouteType.Helicopter_Passenger;

            Terminal.TerminalType terminaltype = (focus == Route.RouteType.Cargo || focus == Route.RouteType.Helicopter_Cargo) ? Terminal.TerminalType.Cargo : Terminal.TerminalType.Passenger;

            if (airline.Airports.Count < 4)
            {
                var focuses = new List<Airline.AirlineFocus>();
                focuses.Add(Airline.AirlineFocus.Local);
                focuses.Add(Airline.AirlineFocus.Local);
                focuses.Add(Airline.AirlineFocus.Local);
                focuses.Add(marketFocus);

                marketFocus = focuses[rnd.Next(focuses.Count)];
            }
            switch (marketFocus)
            {
                case Airline.AirlineFocus.Domestic:
                    airports = airports.FindAll(a => a.Profile.Country == airport.Profile.Country);
                    break;
                case Airline.AirlineFocus.Global:
                    airports =
                        airports.FindAll(
                            a =>
                                IsRouteInCorrectArea(airport, a)
                                && MathHelpers.GetDistance(
                                    a.Profile.Coordinates.convertToGeoCoordinate(),
                                    airport.Profile.Coordinates.convertToGeoCoordinate()) > 100
                                && airport.Profile.Town != a.Profile.Town
                                && MathHelpers.GetDistance(
                                    a.Profile.Coordinates.convertToGeoCoordinate(),
                                    airport.Profile.Coordinates.convertToGeoCoordinate()) < maxDistance
                                && MathHelpers.GetDistance(
                                    a.Profile.Coordinates.convertToGeoCoordinate(),
                                    airport.Profile.Coordinates.convertToGeoCoordinate()) > 100);
                    break;
                case Airline.AirlineFocus.Local:
                    airports =
                        airports.FindAll(
                            a =>
                                IsRouteInCorrectArea(airport, a)
                                && MathHelpers.GetDistance(
                                    a.Profile.Coordinates.convertToGeoCoordinate(),
                                    airport.Profile.Coordinates.convertToGeoCoordinate()) < Math.Max(minDistance, 1000)
                                && airport.Profile.Town != a.Profile.Town
                                && MathHelpers.GetDistance(
                                    a.Profile.Coordinates.convertToGeoCoordinate(),
                                    airport.Profile.Coordinates.convertToGeoCoordinate()) >= Route.MinRouteDistance);
                    break;
                case Airline.AirlineFocus.Regional:
                    airports =
                        airports.FindAll(
                            a =>
                                a.Profile.Country.Region == airport.Profile.Country.Region
                                && IsRouteInCorrectArea(airport, a)
                                && MathHelpers.GetDistance(
                                    a.Profile.Coordinates.convertToGeoCoordinate(),
                                    airport.Profile.Coordinates.convertToGeoCoordinate()) < maxDistance
                                && airport.Profile.Town != a.Profile.Town
                                && MathHelpers.GetDistance(
                                    a.Profile.Coordinates.convertToGeoCoordinate(),
                                    airport.Profile.Coordinates.convertToGeoCoordinate()) > 100);
                    break;
            }

            if (airports.Count == 0)
            {
                airports =
                    (from a in
                         Airports.GetAirports(
                             a =>
                                 IsRouteInCorrectArea(airport, a)
                                 && !airline.Routes.Exists(r => (r.Destination1 == a && r.Destination2 == airport) || (r.Destination2 == a && r.Destination1 == airport))                        
                                 && MathHelpers.GetDistance(
                                     a.Profile.Coordinates.convertToGeoCoordinate(),
                                     airport.Profile.Coordinates.convertToGeoCoordinate()) < 5000
                                 && MathHelpers.GetDistance(
                                     a.Profile.Coordinates.convertToGeoCoordinate(),
                                     airport.Profile.Coordinates.convertToGeoCoordinate()) >= Route.MinRouteDistance)
                     orderby a.Profile.Size descending
                     select a).ToList();
            }

            if (focus == Route.RouteType.Cargo)
                return (from a in airports
                        where
                            routes.Find(r => r.Destination1 == a || r.Destination2 == a) == null
                            && (a.Terminals.getFreeGates(terminaltype) > 0 || AirportHelpers.HasFreeGates(a, airline, terminaltype))
                        orderby
                            ((int)airport.getDestinationCargoRate(a))
                            + ((int)a.getDestinationCargoRate(airport)) descending
                        select a).ToList();
            else
                return (from a in airports
                        where
                            routes.Find(r => r.Destination1 == a || r.Destination2 == a) == null
                            && (a.Terminals.getFreeGates(terminaltype) > 0 || AirportHelpers.HasFreeGates(a, airline, terminaltype))
                        orderby
                            ((int)airport.getDestinationPassengersRate(a, AirlinerClass.ClassType.Economy_Class))
                            + ((int)a.getDestinationPassengersRate(airport, AirlinerClass.ClassType.Economy_Class)) descending
                        select a).ToList();
        }

        public static T GetRandomItem<T>(Dictionary<T, int> list)
        {
            var tList = new List<T>();

            foreach (T item in list.Keys)
            {
                for (int i = 0; i < list[item]; i++)
                {
                    tList.Add(item);
                }
            }

            return tList[rnd.Next(tList.Count)];
        }

        public static RouteClassesConfiguration GetRouteConfiguration(PassengerRoute route)
        {
            double distance = MathHelpers.GetDistance(route.Destination1, route.Destination2);

            if (distance < 500)
            {
                return (RouteClassesConfiguration)Configurations.GetStandardConfiguration("100");
            }
            if (distance < 2000)
            {
                return (RouteClassesConfiguration)Configurations.GetStandardConfiguration("101");
            }
            if (route.Destination1.Profile.Country == route.Destination2.Profile.Country)
            {
                return (RouteClassesConfiguration)Configurations.GetStandardConfiguration("102");
            }
            if (route.Destination1.Profile.Country != route.Destination2.Profile.Country)
            {
                return (RouteClassesConfiguration)Configurations.GetStandardConfiguration("103");
            }

            return null;
        }

        public static Boolean IsCargoRouteDestinationsCorrect(Airport dest1, Airport dest2, Airline airline)
        {

            return dest1.getAirportFacility(airline, AirportFacility.FacilityType.Cargo, true).TypeLevel > 0
                   && dest2.getAirportFacility(airline, AirportFacility.FacilityType.Cargo, true).TypeLevel > 0;
        }

        public static Boolean IsRouteInCorrectArea(Airport dest1, Airport dest2)
        {
            //   less than 3 hours is short haul Reminds me though, Hong Kong isnt in the same region as China in the game
            double distance = MathHelpers.GetDistance(
                dest1.Profile.Coordinates.convertToGeoCoordinate(),
                dest2.Profile.Coordinates.convertToGeoCoordinate());

            Boolean isOk = (dest1.Profile.Country == dest2.Profile.Country)
                || (dest1.Profile.Type == AirportProfile.AirportType.International && dest2.Profile.Type == AirportProfile.AirportType.International);


            return isOk;
        }

        public static void SetAirlinerHomebase(FleetAirliner airliner, Route.RouteType focus)
        {
            Airport homebase = GetServiceAirport(airliner.Airliner.Airline);

            if (homebase == null)
            {
                homebase = GetDestinationAirport(airliner.Airliner.Airline, airliner.Homebase);
            }

            if (homebase.Terminals.getNumberOfGates(airliner.Airliner.Airline) == 0)
            {
                AirportHelpers.RentGates(homebase, airliner.Airliner.Airline, AirportContract.ContractType.Full, focus == Route.RouteType.Cargo ? Terminal.TerminalType.Cargo : Terminal.TerminalType.Passenger);
                AirportFacility checkinFacility =
                    AirportFacilities.GetFacilities(AirportFacility.FacilityType.CheckIn).Find(f => f.TypeLevel == 1);

                if (
                    homebase.getAirportFacility(airliner.Airliner.Airline, AirportFacility.FacilityType.CheckIn)
                        .TypeLevel == 0)
                {
                    homebase.addAirportFacility(
                        airliner.Airliner.Airline,
                        checkinFacility,
                        GameObject.GetInstance().GameTime);
                    AirlineHelpers.AddAirlineInvoice(
                        airliner.Airliner.Airline,
                        GameObject.GetInstance().GameTime,
                        Invoice.InvoiceType.Purchases,
                        -checkinFacility.Price);
                }
            }

            airliner.Homebase = homebase;
        }

        //updates a cpu airline
        public static void UpdateCPUAirline(Airline airline)
        {
            /*
            try
            {
                CheckForNewRoute(airline);
                CheckForNewHub(airline); 
                CheckForUpdateRoute(airline); 
                CheckForAirlinersWithoutRoutes(airline); 
                CheckForOrderOfAirliners(airline); 
                CheckForAirlineAlliance(airline); 
                CheckForAirlineCodesharing(airline); 
                CheckForSubsidiaryAirline(airline);
                CheckForAirlineAirportFacilities(airline); 
                CheckForStocksHandling(airline);
                CheckForMaintenance(airline);
                CheckForOrderOfAirliners(airline); 
            }
            catch (Exception e)
            {
                TAPLogger.LogEvent(e.ToString(), string.Format("Exception in UpdateCPUAirline.cs for {0}",airline.Profile.Name));
            }
            */


            try
            {
                Parallel.Invoke(
                    () => { CheckForNewRoute(airline); },
                    () => { CheckForNewHub(airline); },
                    () => { CheckForUpdateRoute(airline); },
                    () => { CheckForAirlinersWithoutRoutes(airline); },
                    () => { CheckForOrderOfAirliners(airline); },
                    () => { CheckForAirlineAlliance(airline); },
                    () => { CheckForAirlineCodesharing(airline); },
                    () => { CheckForSubsidiaryAirline(airline); },
                    () => { CheckForAirlineAirportFacilities(airline); },
                    () => { CheckForStocksHandling(airline); },
                    () => { CheckForMaintenanceAndConvertable(airline); });
                //  () => { CheckForOrderOfAirliners(airline); }); //close parallel.invoke
            }
            catch (Exception e)
            {
                TAPLogger.LogEvent(e.ToString(), string.Format("Exception in UpdateCPUAirline.cs for {0}", airline.Profile.Name));
            }

        }

        #endregion

        #region Methods
        //checks the airline will buy or sell some stocks
        private static void CheckForStocksHandling(Airline airline)
        {
            double moneyPercent = (airline.Money / airline.StartMoney) * 100;

            //if lower than 10% back of the money then try to sell some shares
            if (moneyPercent < 10)
            {
                Airline shareAirline = null;

                shareAirline = Airlines.GetAirlines(a => a != airline).FirstOrDefault(a => a.Shares.FirstOrDefault(s => s.Airline == airline) != null);

                if (shareAirline == null)
                    shareAirline = airline.Shares.FirstOrDefault(s => s.Airline == airline) == null ? null : airline;

                if (shareAirline != null)
                    AirlineHelpers.SellShares(airline, shareAirline);
            }
            else
            {
                int buySharesInterval = 100000;
                Boolean buyShares = rnd.Next(buySharesInterval) == 0;

                if (buyShares)
                {
                    Airline shareAirline = null;

                    shareAirline = airline.Shares.FirstOrDefault(s => s.Airline == null) == null ? null : airline;

                    if (shareAirline == null)
                    {
                        shareAirline = Airlines.GetAirlines(a => a != airline).FirstOrDefault(a => a.Shares.FirstOrDefault(s => s.Airline == airline) != null);

                        if (shareAirline == null)
                            shareAirline = Airlines.GetAirlines(a => a != airline).OrderBy(a => AirlineHelpers.GetPricePerAirlineShare(a)).FirstOrDefault();
                    }

                    if (shareAirline != null && airline.Money > 300000 * 1.10 * AirlineHelpers.GetPricePerAirlineShare(shareAirline))
                        AirlineHelpers.BuyShares(airline, shareAirline);

                }
            }
        }
        private static void ChangeRouteServiceLevel(PassengerRoute route)
        {
            var oRoutes = new List<Route>(Airlines.GetAirlines(a => a != route.Airline).SelectMany(a => a.Routes));

            IEnumerable<Route> sameRoutes =
                oRoutes.Where(
                    r =>
                        (r.Type == Route.RouteType.Mixed || r.Type == Route.RouteType.Passenger)
                        && ((r.Destination1 == route.Destination1 && r.Destination2 == route.Destination2)
                        || (r.Destination2 == route.Destination1 && r.Destination1 == route.Destination2)));

            if (sameRoutes.Count() > 0)
            {
                double avgServiceLevel =
                    sameRoutes.Where(r => r is PassengerRoute)
                        .Average(r => ((PassengerRoute)r).getServiceLevel(AirlinerClass.ClassType.Economy_Class));

                RouteClassesConfiguration configuration = GetRouteConfiguration(route);

                Array types = Enum.GetValues(typeof(RouteFacility.FacilityType));

                int ct = 0;
                while (avgServiceLevel > route.getServiceLevel(AirlinerClass.ClassType.Economy_Class)
                       && ct < types.Length)
                {
                    var type = (RouteFacility.FacilityType)types.GetValue(ct);

                    RouteFacility currentFacility =
                        route.getRouteAirlinerClass(AirlinerClass.ClassType.Economy_Class).getFacility(type);

                    List<RouteFacility> facilities =
                        RouteFacilities.GetFacilities(type).OrderBy(f => f.ServiceLevel).ToList();

                    int index = facilities.IndexOf(currentFacility);

                    if (index + 1 < facilities.Count)
                    {
                        route.getRouteAirlinerClass(AirlinerClass.ClassType.Economy_Class)
                            .addFacility(facilities[index + 1]);
                    }

                    ct++;
                }
            }
            else
            {
                Array types = Enum.GetValues(typeof(RouteFacility.FacilityType));
                double currentServiceLevel = route.getServiceLevel(AirlinerClass.ClassType.Economy_Class);

                int ct = 0;
                while (currentServiceLevel + 50 > route.getServiceLevel(AirlinerClass.ClassType.Economy_Class)
                       && ct < types.Length)
                {
                    var type = (RouteFacility.FacilityType)types.GetValue(ct);

                    RouteFacility currentFacility =
                        route.getRouteAirlinerClass(AirlinerClass.ClassType.Economy_Class).getFacility(type);

                    List<RouteFacility> facilities =
                        RouteFacilities.GetFacilities(type).OrderBy(f => f.ServiceLevel).ToList();

                    int index = facilities.IndexOf(currentFacility);

                    if (index + 1 < facilities.Count)
                    {
                        route.getRouteAirlinerClass(AirlinerClass.ClassType.Economy_Class)
                            .addFacility(facilities[index + 1]);
                    }

                    ct++;
                }
            }
        }
        //checks for maintenance for the airline
        private static void CheckForMaintenanceAndConvertable(Airline airline)
        {
            int highestServiceLevel = AirlinerMaintenanceTypes.GetMaintenanceTypes().Max(m => m.Requirement.TypeLevel);
            var hasChecksForAll = airline.Airports.FirstOrDefault(a => a.getCurrentAirportFacility(airline, AirportFacility.FacilityType.Service).TypeLevel >= highestServiceLevel);

            if (hasChecksForAll == null && airline.MaintenanceCenters.Count() == 0)
            {
                MaintenanceCenter center = AirlineHelpers.GetAirlineMaintenanceCenter(airline);

                airline.MaintenanceCenters.Add(center);

            }

            foreach (FleetAirliner airliner in airline.Fleet)
            {
                if (!airliner.HasRoute)
                    CheckForConvertToCargo(airliner);

                if (airliner.Maintenance.Checks.Exists(c => !c.canPerformCheck()))
                {
                    foreach (AirlinerMaintenanceCheck check in airliner.Maintenance.Checks.Where(c => !c.canPerformCheck()))
                    {
                        if (hasChecksForAll != null)
                        {
                            var airport = airline.Airports.First(a => a.getCurrentAirportFacility(airline, AirportFacility.FacilityType.Service).TypeLevel >= check.Type.Requirement.TypeLevel);
                            check.CheckCenter = new AirlinerMaintenanceCenter(check.Type);
                            check.CheckCenter.Airport = airport;

                        }
                        else
                        {
                            check.CheckCenter = new AirlinerMaintenanceCenter(check.Type);
                            check.CheckCenter.Center = airline.MaintenanceCenters.First();
                        }
                    }
                }
            }


        }
        //checks for building airport facilities for the airline
        private static void CheckForAirlineAirportFacilities(Airline airline)
        {
            int minRoutesForTicketOffice = 3 + (int)airline.Mentality;
            List<Airport> airports =
                airline.Airports.FindAll(
                    a => AirlineHelpers.GetAirportOutboundRoutes(airline, a) >= minRoutesForTicketOffice);

            foreach (Airport airport in airports)
            {
                Boolean allianceHasTicketOffice = airline.Alliances == null
                    ? false
                    : airline.Alliances.SelectMany(a => a.Members)
                        .Any(
                            m =>
                                airport.getAirlineAirportFacility(m.Airline, AirportFacility.FacilityType.TicketOffice)
                                    .Facility.TypeLevel > 0);

                if (
                    airport.getAirlineAirportFacility(airline, AirportFacility.FacilityType.TicketOffice)
                        .Facility.TypeLevel == 0 && !allianceHasTicketOffice
                    && !airport.hasContractType(airline, AirportContract.ContractType.Full_Service)
                    && !airport.hasContractType(airline, AirportContract.ContractType.Medium_Service))
                {
                    AirportFacility facility =
                        AirportFacilities.GetFacilities(AirportFacility.FacilityType.TicketOffice)
                            .Find(f => f.TypeLevel == 1);
                    double price = facility.Price;

                    if (!airport.isBuildingFacility(airline, AirportFacility.FacilityType.TicketOffice))
                    {

                        if (airport.Profile.Country != airline.Profile.Country)
                        {
                            price = price * 1.25;
                        }

                        AirlineHelpers.AddAirlineInvoice(
                            airline,
                            GameObject.GetInstance().GameTime,
                            Invoice.InvoiceType.Purchases,
                            -price);

                        airport.addAirportFacility(
                            airline,
                            facility,
                            GameObject.GetInstance().GameTime.AddDays(facility.BuildingDays));
                    }
                }
            }
        }

        private static void CheckForAirlineAlliance(Airline airline)
        {
            int airlineAlliances = airline.Alliances.Count;

            if (airlineAlliances == 0)
            {
                int newAllianceInterval = 10000;
                Boolean newAlliance = rnd.Next(newAllianceInterval) == 0;

                if (newAlliance)
                {
                    Alliance alliance = GetAirlineAlliance(airline);

                    if (alliance == null)
                    {
                        //creates a new alliance for the airline
                        CreateNewAlliance(airline);
                    }
                    //joins an existing alliance
                    else
                    {
                        if (alliance.IsHumanAlliance)
                        {
                            alliance.addPendingMember(
                                new PendingAllianceMember(
                                    GameObject.GetInstance().GameTime,
                                    alliance,
                                    airline,
                                    PendingAllianceMember.AcceptType.Request));
                            GameObject.GetInstance()
                                .NewsBox.addNews(
                                    new News(
                                        News.NewsType.Alliance_News,
                                        GameObject.GetInstance().GameTime,
                                        "Request to join alliance",
                                        string.Format(
                                            "[LI airline={0}] has requested to joined {1}. The request can be accepted or declined on the alliance page",
                                            airline.Profile.IATACode,
                                            alliance.Name)));
                        }
                        else
                        {
                            if (CanJoinAlliance(airline, alliance))
                            {
                                alliance.addMember(new AllianceMember(airline, GameObject.GetInstance().GameTime));
                                GameObject.GetInstance()
                                    .NewsBox.addNews(
                                        new News(
                                            News.NewsType.Alliance_News,
                                            GameObject.GetInstance().GameTime,
                                            "Joined alliance",
                                            string.Format(
                                                "[LI airline={0}] has joined {1}",
                                                airline.Profile.IATACode,
                                                alliance.Name)));
                            }
                        }
                    }
                }
            }
            else
            {
                CheckForInviteToAlliance(airline);
            }
        }

        private static void CheckForAirlineCodesharing(Airline airline)
        {
            int airlineCodesharings = airline.Codeshares.Count;

            double newCodesharingInterval = 0;
            switch (airline.Mentality)
            {
                case Airline.AirlineMentality.Aggressive:
                    newCodesharingInterval = 85000;
                    break;
                case Airline.AirlineMentality.Moderate:
                    newCodesharingInterval = 850000;
                    break;
                case Airline.AirlineMentality.Safe:
                    newCodesharingInterval = 8500000;
                    break;
            }
            newCodesharingInterval *= GameObject.GetInstance().Difficulty.AILevel;

            Boolean newCodesharing = !airline.IsSubsidiary
                                     && rnd.Next(Convert.ToInt32(newCodesharingInterval) * (airlineCodesharings + 1))
                                     == 0;

            if (newCodesharing)
            {
                InviteToCodesharing(airline);
            }
        }

        //checks for any airliners without routes
        private static void CheckForAirlinersWithoutRoutes(Airline airline)
        {
            var fleet = new List<FleetAirliner>();

            lock (airline.Fleet)
            {
                fleet =
              new List<FleetAirliner>(
                  airline.Fleet.FindAll(
                      a => a.Airliner.BuiltDate <= GameObject.GetInstance().GameTime && !a.HasRoute));
            }
            //samle alle checked for eks airliners/airports
          
            int max = fleet.Count(f => f.Airliner.Status == Airliner.StatusTypes.Normal && f.Airliner.Airline == f.Airliner.Owner);

            Boolean outlease = max > 0 && rnd.Next(1000 / max) == 0;

            if (outlease)
            {
                var sFleet = fleet.OrderBy(f => f.Airliner.BuiltDate.Year);

                if (sFleet.Count() > 0)
                {
                    sFleet.ToList()[0].Airliner.Status = Airliner.StatusTypes.Leasing;

                    //Console.WriteLine("{0} has outleased {1}", airline.Profile.Name, sFleet.ToList()[0].Airliner.TailNumber);
                }
            }



            if (fleet.Count > 0)
                CreateNewRoute(airline);

        }
        //checks for if an airliner should be converted to cargo
        private static void CheckForConvertToCargo(FleetAirliner airliner)
        {
            Boolean isConvertable = airliner.Airliner.Type.IsConvertable;

            Boolean convertToCargo = rnd.Next(1000) == 0;

            if (isConvertable && convertToCargo)
            {
                FleetAirlinerHelpers.ConvertPassengerToCargoAirliner(airliner);
            }
        }
        private static void CheckForInviteToAlliance(Airline airline)
        {
            Alliance alliance = airline.Alliances[0];

            int members = alliance.Members.Count;
            int inviteToAllianceInterval = 100000;
            Boolean inviteToAlliance = rnd.Next(inviteToAllianceInterval * members) == 0;

            if (inviteToAlliance)
            {
                InviteToAlliance(airline, alliance);
            }
        }

        //checks for ordering new airliners

        //checks for etablishing a new hub
        private static void CheckForNewHub(Airline airline)
        {
            int hubs = airline.getHubs().Count;

            int newHubInterval = 0;
            switch (airline.Mentality)
            {
                case Airline.AirlineMentality.Aggressive:
                    newHubInterval = 100000;
                    break;
                case Airline.AirlineMentality.Moderate:
                    newHubInterval = 1000000;
                    break;
                case Airline.AirlineMentality.Safe:
                    newHubInterval = 10000000;
                    break;
            }

            Boolean newHub = rnd.Next(newHubInterval * hubs) == 0;

            if (newHub)
            {
                //creates a new hub for the airline
                CreateNewHub(airline);
            }
        }

        private static void CheckForNewRoute(Airline airline)
        {
            int airlinersInOrder;
            lock (airline.Fleet)
            {
                var fleet = new List<FleetAirliner>(airline.Fleet);
                airlinersInOrder = fleet.Count(a => a.Airliner.BuiltDate > GameObject.GetInstance().GameTime);
            }

            int newRouteInterval = 0;
            switch (airline.Mentality)
            {
                case Airline.AirlineMentality.Aggressive:
                    newRouteInterval = 10000;
                    break;
                case Airline.AirlineMentality.Moderate:
                    newRouteInterval = 100000;
                    break;
                case Airline.AirlineMentality.Safe:
                    newRouteInterval = 1000000;
                    break;
            }

            Boolean newRoute = rnd.Next(newRouteInterval * (airlinersInOrder + 1)) / 1100 == 0;

            if (newRoute)
            {
                //creates a new route for the airline
                CreateNewRoute(airline);
            }
        }

        private static void CheckForOrderOfAirliners(Airline airline)
        {
            int newAirlinersInterval = 0;

            var fleet = new List<FleetAirliner>(airline.Fleet);

            int airliners = fleet.Count + 1;

            int airlinersWithoutRoute = fleet.Count(a => !a.HasRoute) + 1;

            switch (airline.Mentality)
            {
                case Airline.AirlineMentality.Aggressive:
                    newAirlinersInterval = 10000;
                    break;
                case Airline.AirlineMentality.Moderate:
                    newAirlinersInterval = 100000;
                    break;
                case Airline.AirlineMentality.Safe:
                    newAirlinersInterval = 1000000;
                    break;
            }

            int coeff = newAirlinersInterval * (airliners / 2) * airlinersWithoutRoute;

            Boolean newAirliners = coeff > 0 && rnd.Next(newAirlinersInterval * (airliners / 2) * airlinersWithoutRoute) == 0;

            if (newAirliners && airline.Profile.PrimaryPurchasing != AirlineProfile.PreferedPurchasing.Leasing)
            {
                //order new airliners for the airline
                OrderAirliners(airline);
            }
        }

        //creates a new hub for an airline

        //checks for the creation of a subsidiary airline for an airline
        private static void CheckForSubsidiaryAirline(Airline airline)
        {
            int subAirlines = airline.Subsidiaries.Count;

            double newSubInterval = 0;
            switch (airline.Mentality)
            {
                case Airline.AirlineMentality.Aggressive:
                    newSubInterval = 100000;
                    break;
                case Airline.AirlineMentality.Moderate:
                    newSubInterval = 1000000;
                    break;
                case Airline.AirlineMentality.Safe:
                    newSubInterval = 10000000;
                    break;
            }
            newSubInterval *= GameObject.GetInstance().Difficulty.AILevel;

            var futureSubs = new List<FutureSubsidiaryAirline>(airline.FutureAirlines.Where(f => f.Date.ToShortDateString() == GameObject.GetInstance().GameTime.ToShortDateString()));

            foreach (FutureSubsidiaryAirline fAirline in futureSubs)
                CreateSubsidiaryAirline(airline, fAirline);

            //newSubInterval = 0;

            Boolean newSub = !airline.IsSubsidiary && rnd.Next(Convert.ToInt32(newSubInterval) * (subAirlines + 1)) == 0
                             && airline.FutureAirlines.Count(f => f.Date != new DateTime(1900, 1, 1)) > 0 && airline.Money > airline.StartMoney / 5;

            if (newSub)
            {
                //creates a new subsidiary airline for the airline
                CreateSubsidiaryAirline(airline);
            }
        }

        private static void CheckForUpdateRoute(Airline airline)
        {

            int totalHours = rnd.Next(24 * 7, 24 * 13);
            foreach (
                Route route in
                    airline.Routes.FindAll(
                        r => GameObject.GetInstance().GameTime.Subtract(r.LastUpdated).TotalHours > totalHours))
            {
                if (route.HasAirliner)
                {
                    double balance = route.getBalance(route.LastUpdated, GameObject.GetInstance().GameTime);

                    Route.RouteType routeType = route.Type;

                    if (routeType == Route.RouteType.Mixed || routeType == Route.RouteType.Passenger)
                    {
                        if (balance < -1000)
                        {
                            if (route.FillingDegree > 0.50 && (((PassengerRoute)route).IncomePerPassenger < 0.50))
                            {
                                foreach (RouteAirlinerClass rac in ((PassengerRoute)route).Classes)
                                {
                                    rac.FarePrice += 10;
                                }
                                route.LastUpdated = GameObject.GetInstance().GameTime;
                            }
                            if (route.FillingDegree >= 0.2 && route.FillingDegree <= 0.50)
                            {
                                ChangeRouteServiceLevel((PassengerRoute)route);
                            }
                            if (route.FillingDegree < 0.2)
                            {
                                airline.removeRoute(route);

                                if (route.HasAirliner)
                                {
                                    route.getAirliners().ForEach(a => a.removeRoute(route));
                                }

                                if (airline.Routes.Count == 0)
                                {
                                    CreateNewRoute(airline);
                                }

                                NewsFeeds.AddNewsFeed(
                                    new NewsFeed(
                                        GameObject.GetInstance().GameTime,
                                        string.Format(
                                            Translator.GetInstance().GetString("NewsFeed", "1002"),
                                            airline.Profile.Name,
                                            new AirportCodeConverter().Convert(route.Destination1),
                                            new AirportCodeConverter().Convert(route.Destination2))));
                            }
                        }
                    }
                    else
                    {
                        if (balance < -1000)
                        {
                            if (route.FillingDegree > 0.45)
                            {
                                ((CargoRoute)route).PricePerUnit += 10;
                            }
                            if (route.FillingDegree <= 0.45)
                            {
                                airline.removeRoute(route);

                                if (route.HasAirliner)
                                {
                                    route.getAirliners().ForEach(a => a.removeRoute(route));
                                }

                                if (airline.Routes.Count == 0)
                                {
                                    CreateNewRoute(airline);
                                }

                                NewsFeeds.AddNewsFeed(
                                    new NewsFeed(
                                        GameObject.GetInstance().GameTime,
                                        string.Format(
                                            Translator.GetInstance().GetString("NewsFeed", "1002"),
                                            airline.Profile.Name,
                                            new AirportCodeConverter().Convert(route.Destination1),
                                            new AirportCodeConverter().Convert(route.Destination2))));
                            }
                        }
                    }
                }
                if (route.Banned)
                {
                    airline.removeRoute(route);

                    if (route.HasAirliner)
                    {
                        route.getAirliners().ForEach(a => a.removeRoute(route));
                    }

                    if (airline.Routes.Count == 0)
                    {
                        CreateNewRoute(airline);
                    }

                    NewsFeeds.AddNewsFeed(
                        new NewsFeed(
                            GameObject.GetInstance().GameTime,
                            string.Format(
                                Translator.GetInstance().GetString("NewsFeed", "1002"),
                                airline.Profile.Name,
                                new AirportCodeConverter().Convert(route.Destination1),
                                new AirportCodeConverter().Convert(route.Destination2))));
                }
            }
        }
        private static void CreateCharterRouteTimeTable(Route route, FleetAirliner airliner)
        {
            Boolean twiceAWeek = rnd.Next(2) == 0;
            int tDay = rnd.Next(3);
            List<DayOfWeek> days = new List<DayOfWeek>();

            DayOfWeek firstDay = DayOfWeek.Friday + tDay;

            days.Add(firstDay);

            if (twiceAWeek)
                days.Add(firstDay + 3);

            string flightCode1 = airliner.Airliner.Airline.getNextFlightCode(0);
            string flightCode2 = airliner.Airliner.Airline.getNextFlightCode(1);

            route.TimeTable = CreateCharterRouteTimeTable(route, airliner, days, flightCode1, flightCode2);
        }
        public static RouteTimeTable CreateCharterRouteTimeTable(
          Route route,
          FleetAirliner airliner,
          List<DayOfWeek> days,
          string flightCode1,
          string flightCode2)
        {
            var timeTable = new RouteTimeTable(route);

            TimeSpan minFlightTime =
                MathHelpers.GetFlightTime(
                    route.Destination1.Profile.Coordinates.convertToGeoCoordinate(),
                    route.Destination2.Profile.Coordinates.convertToGeoCoordinate(),
                    airliner.Airliner.Type)
                    .Add(new TimeSpan(FleetAirlinerHelpers.GetMinTimeBetweenFlights(airliner).Ticks));

            foreach (DayOfWeek day in days)
            {

                TimeSpan startTime = new TimeSpan(11, 0, 0).Subtract(minFlightTime);

                timeTable.addEntry(new RouteTimeTableEntry(timeTable, day, startTime, new RouteEntryDestination(route.Destination2, flightCode1)));

                TimeSpan endTime = new TimeSpan(22, 0, 0).Subtract(minFlightTime);

                timeTable.addEntry(new RouteTimeTableEntry(timeTable, day, endTime, new RouteEntryDestination(route.Destination1, flightCode2)));
            }

            foreach (RouteTimeTableEntry e in timeTable.Entries)
            {
                e.Airliner = airliner;
            }

            return timeTable;
        }
        private static void CreateBusinessRouteTimeTable(Route route, FleetAirliner airliner)
        {
            TimeSpan minFlightTime =
                route.getFlightTime(airliner.Airliner.Type)
                    .Add(new TimeSpan(FleetAirlinerHelpers.GetMinTimeBetweenFlights(airliner).Ticks));

            int maxHours = 10 - 6; //from 06:00 to 10:00 and from 18:00 to 22:00

            int flightsPerDay = Convert.ToInt16(maxHours * 60 / (2 * minFlightTime.TotalMinutes));

            string flightCode1 = airliner.Airliner.Airline.getNextFlightCode(0);
            string flightCode2 = airliner.Airliner.Airline.getNextFlightCode(1);

            route.TimeTable = CreateBusinessRouteTimeTable(route, airliner, flightsPerDay, flightCode1, flightCode2);
        }

        private static void CreateNewAlliance(Airline airline)
        {
            string name = Alliance.GenerateAllianceName();
            Airport headquarter =
                airline.Airports.FirstOrDefault(
                    a => a.getCurrentAirportFacility(airline, AirportFacility.FacilityType.Service).TypeLevel > 0);


            if (headquarter != null)
            {
                var alliance = new Alliance(GameObject.GetInstance().GameTime, name, headquarter);
                alliance.addMember(new AllianceMember(airline, GameObject.GetInstance().GameTime));

                Alliances.AddAlliance(alliance);

                GameObject.GetInstance()
                    .NewsBox.addNews(
                        new News(
                            News.NewsType.Standard_News,
                            GameObject.GetInstance().GameTime,
                            "New alliance",
                            string.Format(
                                "A new alliance: {0} has been created by [LI airline={1}]",
                                name,
                                airline.Profile.IATACode)));

                InviteToAlliance(airline, alliance);
            }
        }

        private static void CreateNewHub(Airline airline)
        {
            var type = HubType.TypeOfHub.Focus_city;
            var airports = new List<Airport>();

            if (airline.MarketFocus == Airline.AirlineFocus.Domestic
                || airline.MarketFocus == Airline.AirlineFocus.Local)
            {
                type = HubType.TypeOfHub.Focus_city;
            }

            if (airline.MarketFocus == Airline.AirlineFocus.Global)
            {
                type = HubType.TypeOfHub.Hub;
            }

            if (airline.MarketFocus == Airline.AirlineFocus.Regional)
            {
                type = HubType.TypeOfHub.Regional_hub;
            }

            airports = airline.Airports.FindAll(a => AirlineHelpers.CanCreateHub(airline, a, HubTypes.GetHubType(type)));

            if (airports.Count > 0)
            {
                HubType hubtype = HubTypes.GetHubType(type);

                Airport airport = (from a in airports orderby a.Profile.Size descending select a).First();

                airport.addHub(new Hub(airline, hubtype));

                AirlineHelpers.AddAirlineInvoice(
                    airline,
                    GameObject.GetInstance().GameTime,
                    Invoice.InvoiceType.Purchases,
                    AirportHelpers.GetHubPrice(airport, hubtype));
                ;

                // NewsFeeds.AddNewsFeed(new NewsFeed(GameObject.GetInstance().GameTime, string.Format(Translator.GetInstance().GetString("NewsFeed", "1003"), airline.Profile.Name, new AirportCodeConverter().Convert(airport), airport.Profile.Town.Name, airport.Profile.Town.Country.ShortName)));
            }
        }

        private static void CreateNewRoute(Airline airline)
        {

            Route.RouteType focus = airline.AirlineRouteFocus;

            if (focus == Route.RouteType.Mixed)
                focus = rnd.Next(3) == 0 ? Route.RouteType.Cargo : Route.RouteType.Passenger;

            if (focus == Route.RouteType.Helicopter_Mixed)
                focus = rnd.Next(3) == 0 ? Route.RouteType.Helicopter_Cargo : Route.RouteType.Helicopter_Passenger;

            Airport airport = GetRouteStartDestination(airline, focus);

            if (airport != null)
            {
                Airport destination;

                destination = GetDestinationAirport(airline, airport);

                if (destination != null)
                {
                    Boolean doLeasing = airline.Profile.PrimaryPurchasing == AirlineProfile.PreferedPurchasing.Leasing
                                        || (airline.Profile.PrimaryPurchasing
                                            == AirlineProfile.PreferedPurchasing.Random
                                            && (rnd.Next(5) > 1 || airline.Money < 10000000));

                    FleetAirliner fAirliner;

                    KeyValuePair<Airliner, Boolean>? airliner = GetAirlinerForRoute(
                        airline,
                        airport,
                        destination,
                        doLeasing,
                        focus);

                    fAirliner = GetFleetAirliner(airline, airport, destination, focus);

                    if (airliner.HasValue || fAirliner != null)
                    {
                        if (!AirportHelpers.HasFreeGates(destination, airline))
                        {
                            AirportHelpers.RentGates(destination, airline, AirportContract.ContractType.Low_Service);
                        }

                        if (!airline.Airports.Contains(destination))
                        {
                            airline.addAirport(destination);
                        }


                        Guid id = Guid.NewGuid();

                        Route route = null;

                        if (focus == Route.RouteType.Passenger)
                        {
                            double price = PassengerHelpers.GetPassengerPrice(airport, destination);

                            route = new PassengerRoute(
                                id.ToString(),
                                airport,
                                destination,
                                GameObject.GetInstance().GameTime,
                                price);

                            RouteClassesConfiguration configuration = GetRouteConfiguration((PassengerRoute)route);

                            foreach (RouteClassConfiguration classConfiguration in configuration.getClasses())
                            {
                                ((PassengerRoute)route).getRouteAirlinerClass(classConfiguration.Type).FarePrice = price
                                                                                                                   * GeneralHelpers
                                                                                                                       .ClassToPriceFactor
                                                                                                                       (
                                                                                                                           classConfiguration
                                                                                                                               .Type);

                                foreach (RouteFacility facility in classConfiguration.getFacilities())
                                {
                                    ((PassengerRoute)route).getRouteAirlinerClass(classConfiguration.Type)
                                        .addFacility(facility);
                                }
                            }
                        }
                        if (focus == Route.RouteType.Helicopter_Passenger)
                        {
                            double price = PassengerHelpers.GetPassengerPrice(airport, destination);

                            route = new PassengerRoute(
                                id.ToString(),
                                airport,
                                destination,
                                GameObject.GetInstance().GameTime,
                                price);

                            RouteClassesConfiguration configuration = GetRouteConfiguration((PassengerRoute)route);

                            foreach (RouteClassConfiguration classConfiguration in configuration.getClasses())
                            {
                                ((PassengerRoute)route).getRouteAirlinerClass(classConfiguration.Type).FarePrice = price
                                                                                                                   * GeneralHelpers
                                                                                                                       .ClassToPriceFactor
                                                                                                                       (
                                                                                                                           classConfiguration
                                                                                                                               .Type);

                                foreach (RouteFacility facility in classConfiguration.getFacilities())
                                {
                                    ((PassengerRoute)route).getRouteAirlinerClass(classConfiguration.Type)
                                        .addFacility(facility);
                                }
                            }
                        }
                        if (focus == Route.RouteType.Helicopter_Cargo)
                        {
                            route = new CargoRoute(
                                id.ToString(),
                                airport,
                                destination,
                                GameObject.GetInstance().GameTime,
                                PassengerHelpers.GetCargoPrice(airport, destination));
                        }
                        if (focus == Route.RouteType.Cargo)
                        {
                            route = new CargoRoute(
                                id.ToString(),
                                airport,
                                destination,
                                GameObject.GetInstance().GameTime,
                                PassengerHelpers.GetCargoPrice(airport, destination));
                        }
                        Boolean isDeptOk = true;
                        Boolean isDestOk = true;

                      
                        if (!AirportHelpers.HasFreeGates(airport, airline))
                        {
                            isDeptOk = AirportHelpers.RentGates(
                                airport,
                                airline,
                                AirportContract.ContractType.Low_Service);
                        }

                        if (!AirportHelpers.HasFreeGates(destination, airline))
                        {
                            isDestOk = AirportHelpers.RentGates(
                                airport,
                                airline,
                                AirportContract.ContractType.Low_Service);
                        }

                        if (isDestOk && isDeptOk)
                        {
                            Boolean humanHasRoute =
                                Airlines.GetHumanAirlines()
                                    .SelectMany(a => a.Routes)
                                    .ToList()
                                    .Exists(
                                        r =>
                                            (r.Destination1 == route.Destination1
                                             && r.Destination2 == route.Destination2)
                                            || (r.Destination1 == route.Destination2
                                                && r.Destination2 == route.Destination1));

                            if (humanHasRoute && Settings.GetInstance().MailsOnAirlineRoutes)
                            {
                                GameObject.GetInstance()
                                    .NewsBox.addNews(
                                        new News(
                                            News.NewsType.Airline_News,
                                            GameObject.GetInstance().GameTime,
                                            Translator.GetInstance().GetString("News", "1013"),
                                            string.Format(
                                                Translator.GetInstance().GetString("News", "1013", "message"),
                                                airline.Profile.IATACode,
                                                route.Destination1.Profile.IATACode,
                                                route.Destination2.Profile.IATACode)));
                            }

                            Country newDestination =
                                airline.Routes.Count(
                                    r =>
                                        r.Destination1.Profile.Country == airport.Profile.Country
                                        || r.Destination2.Profile.Country == airport.Profile.Country) == 0
                                    ? airport.Profile.Country
                                    : null;

                            newDestination =
                                airline.Routes.Count(
                                    r =>
                                        r.Destination1.Profile.Country == destination.Profile.Country
                                        || r.Destination2.Profile.Country == destination.Profile.Country) == 0
                                    ? destination.Profile.Country
                                    : newDestination;

                            if (newDestination != null && Settings.GetInstance().MailsOnAirlineRoutes)
                            {
                                GameObject.GetInstance()
                                    .NewsBox.addNews(
                                        new News(
                                            News.NewsType.Airline_News,
                                            GameObject.GetInstance().GameTime,
                                            Translator.GetInstance().GetString("News", "1014"),
                                            string.Format(
                                                Translator.GetInstance().GetString("News", "1014", "message"),
                                                airline.Profile.IATACode,
                                                ((Country)new CountryCurrentCountryConverter().Convert(newDestination))
                                                    .Name)));
                            }

                            if (!AirportHelpers.HasFreeGates(airport, airline))
                            {
                                AirportHelpers.RentGates(airport, airline, AirportContract.ContractType.Low_Service);
                            }

                            if (!AirportHelpers.HasFreeGates(destination, airline))
                            {
                                AirportHelpers.RentGates(airport, airline, AirportContract.ContractType.Low_Service);
                            }

                            //Console.WriteLine("{3}: {0} has created a route between {1} and {2}", airline.Profile.Name, route.Destination1.Profile.Name, route.Destination2.Profile.Name,GameObject.GetInstance().GameTime.ToShortDateString());

                            if (fAirliner == null)
                            {
                                Country tailnumberCountry = Countries.GetCountryFromTailNumber(airliner.Value.Key.TailNumber);

                                if (tailnumberCountry != null && tailnumberCountry.Name
                                    != airline.Profile.Country.Name)
                                {
                                    airliner.Value.Key.TailNumber =
                                        airline.Profile.Country.TailNumbers.getNextTailNumber();
                                }

                                if (airliner.Value.Value) //loan
                                {
                                    double amount = airliner.Value.Key.getPrice() - airline.Money + 20000000;

                                    var loan = new Loan(
                                        GameObject.GetInstance().GameTime,
                                        amount,
                                        120,
                                        GeneralHelpers.GetAirlineLoanRate(airline));

                                    double payment = loan.getMonthlyPayment();

                                    airline.addLoan(loan);
                                    AirlineHelpers.AddAirlineInvoice(
                                        airline,
                                        loan.Date,
                                        Invoice.InvoiceType.Loans,
                                        loan.Amount);
                                }
                                else
                                {
                                    if (doLeasing)
                                    {
                                        AirlineHelpers.AddAirlineInvoice(
                                            airline,
                                            GameObject.GetInstance().GameTime,
                                            Invoice.InvoiceType.Rents,
                                            -airliner.Value.Key.LeasingPrice * 2);

                                        if (airliner.Value.Key.Owner != null && airliner.Value.Key.Owner.IsHuman)
                                            NewsFeeds.AddNewsFeed(new NewsFeed(GameObject.GetInstance().GameTime, string.Format(Translator.GetInstance().GetString("NewsFeed", "1004"), airline.Profile.Name, airliner.Value.Key.TailNumber)));

                                    }
                                    else
                                    {
                                        AirlineHelpers.AddAirlineInvoice(
                                            airline,
                                            GameObject.GetInstance().GameTime,
                                            Invoice.InvoiceType.Purchases,
                                            -airliner.Value.Key.getPrice());
                                    }
                                }

                                fAirliner =
                                    new FleetAirliner(
                                        doLeasing
                                            ? FleetAirliner.PurchasedType.Leased
                                            : FleetAirliner.PurchasedType.Bought,
                                        GameObject.GetInstance().GameTime,
                                        airline,
                                        airliner.Value.Key,
                                        airport);
                                airline.Fleet.Add(fAirliner);

                                AirlinerHelpers.CreateAirlinerClasses(fAirliner.Airliner);
                            }

                            //NewsFeeds.AddNewsFeed(new NewsFeed(GameObject.GetInstance().GameTime, string.Format(Translator.GetInstance().GetString("NewsFeed", "1001"), airline.Profile.Name, new AirportCodeConverter().Convert(route.Destination1), new AirportCodeConverter().Convert(route.Destination2))));

                            if (route.Type == Route.RouteType.Passenger || route.Type == Route.RouteType.Mixed)
                            {
                                if (airline.Schedule == Airline.AirlineRouteSchedule.Charter)
                                    CreateCharterRouteTimeTable(route, fAirliner);
                                else
                                {
                                    //creates a business route
                                    if (IsBusinessRoute(route, fAirliner) || airline.Schedule == Airline.AirlineRouteSchedule.Business)
                                    {
                                        CreateBusinessRouteTimeTable(route, fAirliner);
                                    }
                                    else
                                    {
                                        CreateRouteTimeTable(route, fAirliner);
                                    }
                                }
                            }
                            if (route.Type == Route.RouteType.Cargo)
                            {
                                CreateCargoRouteTimeTable(route, fAirliner);
                            }

                            fAirliner.Status = FleetAirliner.AirlinerStatus.To_route_start;
                            AirlineHelpers.HireAirlinerPilots(fAirliner);

                            route.LastUpdated = GameObject.GetInstance().GameTime;
                        }
                        if (route != null)
                        {
                            airline.addRoute(route);

                            if (fAirliner != null)
                            {
                                fAirliner.addRoute(route);
                            }

                        }
                        else
                        {
                            string emptyRoute = "Empty";
                        }

                        AirportFacility checkinFacility =
                            AirportFacilities.GetFacilities(AirportFacility.FacilityType.CheckIn)
                                .Find(f => f.TypeLevel == 1);
                        AirportFacility cargoTerminal =
                            AirportFacilities.GetFacilities(AirportFacility.FacilityType.Cargo)
                                .Find(f => f.TypeLevel > 0);

                        /*
                        if (destination.getAirportFacility(airline, AirportFacility.FacilityType.CheckIn).TypeLevel == 0)
                        {
                            destination.addAirportFacility(airline, checkinFacility, GameObject.GetInstance().GameTime);
                            AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -checkinFacility.Price);

                        }
                        if (airport.getAirportFacility(airline, AirportFacility.FacilityType.CheckIn).TypeLevel == 0)
                        {
                            airport.addAirportFacility(airline, checkinFacility, GameObject.GetInstance().GameTime);
                            AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -checkinFacility.Price);

                        }
                        */

                        if (destination.getAirportFacility(airline, AirportFacility.FacilityType.Cargo).TypeLevel == 0
                            && destination.getAirportFacility(null, AirportFacility.FacilityType.Cargo).TypeLevel == 0
                            && route.Type == Route.RouteType.Cargo)
                        {
                            destination.addAirportFacility(airline, cargoTerminal, GameObject.GetInstance().GameTime);
                            AirlineHelpers.AddAirlineInvoice(
                                airline,
                                GameObject.GetInstance().GameTime,
                                Invoice.InvoiceType.Purchases,
                                -cargoTerminal.Price);
                        }

                        if (airport.getAirportFacility(airline, AirportFacility.FacilityType.Cargo).TypeLevel == 0
                            && airport.getAirportFacility(null, AirportFacility.FacilityType.Cargo).TypeLevel == 0
                            && route.Type == Route.RouteType.Cargo)
                        {
                            airport.addAirportFacility(airline, cargoTerminal, GameObject.GetInstance().GameTime);
                            AirlineHelpers.AddAirlineInvoice(
                                airline,
                                GameObject.GetInstance().GameTime,
                                Invoice.InvoiceType.Purchases,
                                -cargoTerminal.Price);
                        }
                    }
                }
            }
        }
        //creates a new subsidiary airline for the airline
        public static void CreateSubsidiaryAirline(Airline airline, FutureSubsidiaryAirline futureAirline)
        {
            airline.FutureAirlines.Remove(futureAirline);

            SubsidiaryAirline sAirline = AirlineHelpers.CreateSubsidiaryAirline(
                airline,
                airline.Money / 5,
                futureAirline.Name,
                futureAirline.IATA,
                futureAirline.Mentality,
                futureAirline.Market,
                futureAirline.AirlineRouteFocus,
                futureAirline.PreferedAirport);

            sAirline.Profile.Logos.Clear();
            sAirline.Profile.AddLogo(new AirlineLogo(futureAirline.Logo));
            sAirline.Profile.Color = airline.Profile.Color;

            CreateNewRoute(sAirline);

            GameObject.GetInstance()
                .NewsBox.addNews(
                    new News(
                        News.NewsType.Airline_News,
                        GameObject.GetInstance().GameTime,
                        "Subsidiary Created",
                        string.Format(
                            "[LI airline={0}] has created a new subsidiary airline [LI airline={1}]",
                            airline.Profile.IATACode,
                            sAirline.Profile.IATACode)));
        }
        //creates a new subsidiary airline for the airline
        private static void CreateSubsidiaryAirline(Airline airline)
        {
            var randomFutureSubs = airline.FutureAirlines.Where(f => f.Date == new DateTime(1900, 1, 1)).ToList();

            FutureSubsidiaryAirline futureAirline = randomFutureSubs[rnd.Next(randomFutureSubs.Count)];

            CreateSubsidiaryAirline(airline, futureAirline);
        }

        //checks for the creation of code sharing for an airline

        //returns a "good" alliance for an airline to join
        private static Alliance GetAirlineAlliance(Airline airline)
        {
            Alliance bestAlliance = (from a in Alliances.GetAlliances()
                                     where !a.Members.ToList().Exists(m => m.Airline == airline)
                                     orderby GetAirlineAllianceScore(airline, a, true) descending
                                     select a).FirstOrDefault();

            if (bestAlliance != null && GetAirlineAllianceScore(airline, bestAlliance, true) > 50)
            {
                return bestAlliance;
            }
            return null;
        }

        //returns the "score" for an airline compared to an alliance
        private static double GetAirlineAllianceScore(Airline airline, Alliance alliance, Boolean forAlliance)
        {
            IEnumerable<Country> sameCountries =
                alliance.Members.SelectMany(m => m.Airline.Airports)
                    .Select(a => a.Profile.Country)
                    .Distinct()
                    .Intersect(airline.Airports.Select(a => a.Profile.Country).Distinct());
            IEnumerable<Airport> sameDestinations =
                alliance.Members.SelectMany(m => m.Airline.Airports).Distinct().Intersect(airline.Airports);

            double airlineRoutes = airline.Routes.Count;
            double allianceRoutes = alliance.Members.SelectMany(m => m.Airline.Routes).Count();

            double coeff = forAlliance ? allianceRoutes * 10 : airlineRoutes * 10;

            double score = coeff + (5 - sameCountries.Count()) * 5 + (5 - sameDestinations.Count()) * 5;

            return score;
        }

        //returns the best fit airline for an alliance
        private static Airline GetAllianceAirline(Alliance alliance)
        {
            Airline bestAirline = (from a in Airlines.GetAllAirlines()
                                   where !alliance.Members.ToList().Exists(m => m.Airline == a) && a.Alliances.Count == 0
                                   orderby GetAirlineAllianceScore(a, alliance, false) descending
                                   select a).FirstOrDefault();

            if (GetAirlineAllianceScore(bestAirline, alliance, false) > 50)
            {
                return bestAirline;
            }
            return null;
        }

        private static int GetCodesharingScore(Airline asker, Airline airline)
        {
            int diffCountries =
                asker.Airports.Select(a => a.Profile.Country)
                    .Intersect(airline.Airports.Select(a => a.Profile.Country))
                    .Distinct()
                    .Count();
            int diffRoutes = asker.Routes.Count - airline.Routes.Count;
            int coeff = asker.Airports.Select(a => a.Profile.Country).Distinct().Count()
                        > airline.Airports.Select(a => a.Profile.Country).Distinct().Count()
                ? 1
                : -1;
            int askerRoutes = airline.Routes.Count;

            return (diffRoutes * 7) + (diffCountries * coeff * 5) + (askerRoutes * 3);
        }

        //creates a new alliance for an airline

        //returns an airliner from the fleet which fits a route
        private static FleetAirliner GetFleetAirliner(Airline airline, Airport destination1, Airport destination2, Route.RouteType focus)
        {
            AirlinerType.TypeOfAirliner type = AirlinerType.TypeOfAirliner.Passenger;

            if (focus == Route.RouteType.Cargo)
                type = AirlinerType.TypeOfAirliner.Cargo;
            else if (focus == Route.RouteType.Helicopter_Passenger)
                type = AirlinerType.TypeOfAirliner.Helicopter;
            else if (focus == Route.RouteType.Helicopter_Cargo)
                type = AirlinerType.TypeOfAirliner.Helicopter_Cargo;
            else if (focus == Route.RouteType.Mixed)
                type = AirlinerType.TypeOfAirliner.Mixed;
            else if (focus == Route.RouteType.Passenger)
                type = AirlinerType.TypeOfAirliner.Passenger;

            //Order new airliner
            List<FleetAirliner> fleet =
                airline.Fleet.FindAll(
                    f =>
                        !f.HasRoute && f.Airliner.BuiltDate <= GameObject.GetInstance().GameTime
                        && f.Airliner.Range
                        > MathHelpers.GetDistance(
                            destination1.Profile.Coordinates.convertToGeoCoordinate(),
                            destination2.Profile.Coordinates.convertToGeoCoordinate())
                            && f.Airliner.Type.TypeAirliner == type);

            if (fleet.Count > 0)
            {
                return (from f in fleet orderby f.Airliner.Range select f).First();
            }
            return null;
        }

        //returns the best fit for an airliner for sale for a route true for loan

        //returns a free gate for an airline
        private static Gate GetFreeAirlineGate(Airline airline, Airport airport, DayOfWeek day, TimeSpan time)
        {
            List<Gate> airlineGates = airport.Terminals.getGates(airline);

            return airlineGates.FirstOrDefault();
        }

        private static Airport GetRouteStartDestination(Airline airline, Route.RouteType focus)
        {
            List<Airport> homeAirports;

            lock (airline.Airports)
            {
                homeAirports = AirlineHelpers.GetHomebases(airline);
                var focusAirports = airline.Airports.Where(a => airline.Profile.FocusAirports.Contains(a));

                homeAirports.AddRange(focusAirports);
            }
            homeAirports.AddRange(airline.getHubs());



            Terminal.TerminalType terminaltype = focus == Route.RouteType.Cargo ? Terminal.TerminalType.Cargo : Terminal.TerminalType.Passenger;

            Airport airport = homeAirports.Find(a => AirportHelpers.HasFreeGates(a, airline, terminaltype));

            if (airport == null)
            {
                airport = homeAirports.Find(a => a.Terminals.getFreeGates(terminaltype) > 0);
                if (airport != null)
                {
                    if (!AirportHelpers.HasFreeGates(airport, airline))
                        AirportHelpers.RentGates(airport, airline, AirportContract.ContractType.Low_Service, terminaltype);
                }
                else
                {
                    airport = GetServiceAirport(airline);
                    if (airport != null)
                    {
                        if (!AirportHelpers.HasFreeGates(airport, airline))
                            AirportHelpers.RentGates(airport, airline, AirportContract.ContractType.Low_Service, terminaltype);
                    }
                }
            }

            return airport;
        }

        private static Airport GetServiceAirport(Airline airline)
        {
            AirportFacility facility =
                AirportFacilities.GetFacilities(AirportFacility.FacilityType.Service).Find(f => f.TypeLevel == 1);

            IOrderedEnumerable<Airport> airports =
                from a in airline.Airports.FindAll(aa => aa.Terminals.getFreeGates(airline.AirlineRouteFocus == Route.RouteType.Cargo ? Terminal.TerminalType.Cargo : Terminal.TerminalType.Passenger) > 0)
                orderby a.Profile.Size descending
                select a;

            if (airports.Count() > 0)
            {
                Airport airport = airports.First();

                if (airport.getAirlineAirportFacility(airline, AirportFacility.FacilityType.Service).Facility.TypeLevel
                    == 0 && !airport.hasContractType(airline, AirportContract.ContractType.Full_Service))

                    if (!airport.isBuildingFacility(airline, AirportFacility.FacilityType.Service))
                    {
                        airport.addAirportFacility(
                            airline,
                            facility,
                            GameObject.GetInstance().GameTime.AddDays(facility.BuildingDays));

                        double price = facility.Price;

                        if (airport.Profile.Country != airline.Profile.Country)
                        {
                            price = price * 1.25;
                        }

                        AirlineHelpers.AddAirlineInvoice(
                            airline,
                            GameObject.GetInstance().GameTime,
                            Invoice.InvoiceType.Purchases,
                            -price);
                    }
                return airport;
            }


            return null;
        }

        private static void InviteToAlliance(Airline airline, Alliance alliance)
        {
            Airline bestFitAirline = GetAllianceAirline(alliance);

            if (bestFitAirline != null)
            {
                if (bestFitAirline == GameObject.GetInstance().HumanAirline)
                {
                    alliance.addPendingMember(
                        new PendingAllianceMember(
                            GameObject.GetInstance().GameTime,
                            alliance,
                            bestFitAirline,
                            PendingAllianceMember.AcceptType.Invitation));
                    GameObject.GetInstance()
                        .NewsBox.addNews(
                            new News(
                                News.NewsType.Alliance_News,
                                GameObject.GetInstance().GameTime,
                                "Invitation to join alliance",
                                string.Format(
                                    "[LI airline={0}] has invited you to join {1}. The invitation can be accepted or declined on the alliance page",
                                    airline.Profile.IATACode,
                                    alliance.Name)));
                }
                else
                {
                    if (DoAcceptAllianceInvitation(bestFitAirline, alliance))
                    {
                        GameObject.GetInstance()
                            .NewsBox.addNews(
                                new News(
                                    News.NewsType.Alliance_News,
                                    GameObject.GetInstance().GameTime,
                                    "Joined alliance",
                                    string.Format(
                                        "[LI airline={0}] has joined {1}",
                                        bestFitAirline.Profile.IATACode,
                                        alliance.Name)));
                        alliance.addMember(new AllianceMember(bestFitAirline, GameObject.GetInstance().GameTime));
                    }
                }
            }
        }

        private static void InviteToCodesharing(Airline airline)
        {
            //find the best airline for codesharing
            IEnumerable<Airline> airlines =
                Airlines.GetAllAirlines()
                    .Where(a => a != airline && (!a.IsSubsidiary || ((SubsidiaryAirline)a).Airline != airline));

            int bestscore = 0;
            Airline bestAirline = null;

            foreach (Airline tAirline in airlines)
            {
                int score = GetCodesharingScore(airline, tAirline);

                if (score > bestscore)
                {
                    bestAirline = tAirline;
                }
            }

            int minValue = 50;

            if (bestscore > minValue)
            {
                Boolean acceptInvitation = AirlineHelpers.AcceptCodesharing(
                    bestAirline,
                    airline,
                    CodeshareAgreement.CodeshareType.Both_Ways);

                if (acceptInvitation)
                {
                    if (bestAirline.IsHuman)
                    {
                        var agreement = new CodeshareAgreement(
                            bestAirline,
                            airline,
                            CodeshareAgreement.CodeshareType.Both_Ways);

                        var news = new News(
                            News.NewsType.Alliance_News,
                            GameObject.GetInstance().GameTime,
                            "Codeshare Agreement",
                            string.Format(
                                "[LI airline={0}] has asked you for a codeshare agreement. Do you accept it?",
                                airline.Profile.IATACode),
                            true);
                        news.ActionObject = agreement;
                        news.Action += news_Action;

                        GameObject.GetInstance().NewsBox.addNews(news);
                    }
                    else
                    {
                        var agreement = new CodeshareAgreement(
                            bestAirline,
                            airline,
                            CodeshareAgreement.CodeshareType.Both_Ways);
                        bestAirline.addCodeshareAgreement(agreement);
                        airline.addCodeshareAgreement(agreement);

                        GameObject.GetInstance()
                            .NewsBox.addNews(
                                new News(
                                    News.NewsType.Alliance_News,
                                    GameObject.GetInstance().GameTime,
                                    "Codeshare Agreement",
                                    string.Format(
                                        "[LI airline={0}] and [LI airline={1}] have made a codeshare agreement",
                                        airline.Profile.IATACode,
                                        bestAirline.Profile.IATACode)));
                    }
                }
            }
        }

        private static Boolean IsBusinessRoute(Route route, FleetAirliner airliner)
        {
            double maxBusinessRouteTime = new TimeSpan(2, 0, 0).TotalMinutes;

            TimeSpan minFlightTime =
                MathHelpers.GetFlightTime(
                    route.Destination1.Profile.Coordinates.convertToGeoCoordinate(),
                    route.Destination2.Profile.Coordinates.convertToGeoCoordinate(),
                    airliner.Airliner.Type)
                    .Add(new TimeSpan(FleetAirlinerHelpers.GetMinTimeBetweenFlights(airliner).Ticks));

            return minFlightTime.TotalMinutes <= maxBusinessRouteTime;
        }

        private static void OrderAirliners(Airline airline)
        {
            List<AirlinerType> airlineAircrafts = airline.Profile.PreferedAircrafts;

            if (airline.MarketFocus == Airline.AirlineFocus.Global && (airline.AirlineRouteFocus == Route.RouteType.Passenger || airline.AirlineRouteFocus == Route.RouteType.Mixed) && airlineAircrafts.Count == 0)
            {
                airlineAircrafts.AddRange(AirlinerTypes.GetTypes(t => t.Produced.From <= GameObject.GetInstance().GameTime
                            && t.Produced.To >= GameObject.GetInstance().GameTime && t.Manufacturer.IsMajor && t is AirlinerPassengerType));
            }

            int airliners = airline.Fleet.Count;
            int airlinersWithoutRoute = airline.Fleet.Count(a => !a.HasRoute);

            int numberToOrder = rnd.Next(1, 3 - (int)airline.Mentality);

            List<Airport> homeAirports = AirlineHelpers.GetHomebases(airline);

            var airportsList = new Dictionary<Airport, int>();
            //Parallel.ForEach(homeAirports, a => { airportsList.Add(a, (int)a.Profile.Size); });
            foreach (var a in homeAirports)
            {
                airportsList.Add(a, (int)a.Profile.Size);
            }

            if (airportsList.Count > 0)
            {
                Airport homeAirport = GetRandomItem(airportsList);

                List<AirlinerType> types =
                    AirlinerTypes.GetTypes(
                        t =>
                            t.Produced.From <= GameObject.GetInstance().GameTime
                            && t.Produced.To >= GameObject.GetInstance().GameTime && t.Price * numberToOrder < airline.Money);

                Route.RouteType focus = airline.AirlineRouteFocus;
                if (focus == Route.RouteType.Mixed)
                    focus = rnd.Next(3) == 0 ? Route.RouteType.Cargo : Route.RouteType.Passenger;

                if (focus == Route.RouteType.Helicopter_Mixed)
                    focus = rnd.Next(3) == 0 ? Route.RouteType.Helicopter_Cargo : Route.RouteType.Helicopter_Passenger;
                
                if (focus == Route.RouteType.Cargo)
                {
                    types.RemoveAll(a => a.TypeAirliner == AirlinerType.TypeOfAirliner.Passenger);
                    types.RemoveAll(a => a.TypeAirliner == AirlinerType.TypeOfAirliner.Helicopter);
                    types.RemoveAll(a => a.TypeAirliner == AirlinerType.TypeOfAirliner.Helicopter_Cargo);
                 }

                if (focus == Route.RouteType.Passenger)
                {
                    types.RemoveAll(a => a.TypeAirliner == AirlinerType.TypeOfAirliner.Cargo);
                    types.RemoveAll(a => a.TypeAirliner == AirlinerType.TypeOfAirliner.Helicopter);
                    types.RemoveAll(a => a.TypeAirliner == AirlinerType.TypeOfAirliner.Helicopter_Cargo);

                }

                if (focus == Route.RouteType.Helicopter_Passenger)
                {
                    types.RemoveAll(a => a.TypeAirliner == AirlinerType.TypeOfAirliner.Cargo);
                    types.RemoveAll(a => a.TypeAirliner == AirlinerType.TypeOfAirliner.Passenger);
                    types.RemoveAll(a => a.TypeAirliner == AirlinerType.TypeOfAirliner.Helicopter_Cargo);
                }
                if (focus == Route.RouteType.Helicopter_Cargo)
                {
                    types.RemoveAll(a => a.TypeAirliner == AirlinerType.TypeOfAirliner.Cargo);
                    types.RemoveAll(a => a.TypeAirliner == AirlinerType.TypeOfAirliner.Helicopter);
                    types.RemoveAll(a => a.TypeAirliner == AirlinerType.TypeOfAirliner.Passenger);
                }

                types = types.OrderBy(t => t.Price).ToList();

                var list = new Dictionary<AirlinerType, int>();

                foreach (AirlinerType type in types)
                {
                    list.Add(
                        type,
                        (int)((type.Range / (Convert.ToDouble(type.Price) / 100000)))
                        + (airlineAircrafts.Contains(type) ? 10 : 0));
                }
                /*
                Parallel.ForEach(types, t =>
                    {
                        list.Add(t, (int)((t.Range / (t.Price / 100000))));
                    });*/

                if (list.Keys.Count > 0)
                {
                    AirlinerType type = GetRandomItem(list);

                    var orders = new List<AirlinerOrder>();
                    orders.Add(new AirlinerOrder(type, AirlinerHelpers.GetAirlinerClasses(type), numberToOrder, false));

                    DateTime deliveryDate = AirlinerHelpers.GetOrderDeliveryDate(orders);
                    AirlineHelpers.OrderAirliners(airline, orders, homeAirport, deliveryDate);
                }
            }
        }
        private static void news_Action(object o)
        {
            var agreement = (CodeshareAgreement)o;

            agreement.Airline1.addCodeshareAgreement(agreement);
            agreement.Airline2.addCodeshareAgreement(agreement);
        }

        #endregion

        //check if an airline can join an alliance
    }
}