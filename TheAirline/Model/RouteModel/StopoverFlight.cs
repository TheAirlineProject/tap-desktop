using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.Model.RouteModel
{
    //the class for a stopover flight
    [Serializable]
    public class StopoverFlight : Flight
    {
        #region Constructors and Destructors

        public StopoverFlight(RouteTimeTableEntry entry)
            : base(entry)
        {
            CurrentFlight = 0;
            AllClasses = new Dictionary<RouteTimeTableEntry, List<FlightAirlinerClass>>();
            AllCargo = new Dictionary<RouteTimeTableEntry, double>();

            List<Route> legs = entry.TimeTable.Route.Stopovers.SelectMany(s => s.Legs).ToList();

            Boolean isInbound = entry.DepartureAirport == entry.TimeTable.Route.Destination2;

            if (isInbound)
            {
                legs.Reverse();
            }

            CreateEntries(entry);
            /*
            foreach (Route route in legs)
            {
                List<FlightAirlinerClass> classes = new List<FlightAirlinerClass>();
                foreach (AirlinerClass aClass in this.Airliner.Airliner.Classes)
                {
                    FlightAirlinerClass faClass;
                     if (isInbound)
                         faClass = new FlightAirlinerClass(route.getRouteAirlinerClass(aClass.Type), PassengerHelpers.GetStopoverFlightPassengers(this.Airliner,aClass.Type,route.Destination2,route.Destination1));
                     else
                         faClass = new FlightAirlinerClass(route.getRouteAirlinerClass(aClass.Type), PassengerHelpers.GetStopoverFlightPassengers(this.Airliner,aClass.Type,route.Destination1,route.Destination2));
            
                     classes.Add(faClass);
                }
                this.AllClasses.Add(route, classes);
            }
             * */
        }

        private StopoverFlight(SerializationInfo info, StreamingContext ctxt)
            : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("allcargo")]
        public Dictionary<RouteTimeTableEntry, double> AllCargo { get; set; }

        [Versioning("allclasses")]
        public Dictionary<RouteTimeTableEntry, List<FlightAirlinerClass>> AllClasses { get; set; }

        [Versioning("currentflight")]
        public int CurrentFlight { get; set; }

        public Boolean IsLastTrip
        {
            get { return CurrentFlight == Math.Max(AllClasses.Keys.Count, AllCargo.Keys.Count); }
        }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        public override void AddDelayMinutes(int minutes)
        {
            base.AddDelayMinutes(minutes);

            if (IsPassengerFlight())
            {
                foreach (RouteTimeTableEntry e in AllClasses.Keys)
                {
                    e.Time = e.Time.Add(new TimeSpan(0, minutes, 0));
                }
            }

            if (IsCargoFlight())
            {
                foreach (RouteTimeTableEntry e in AllCargo.Keys)
                {
                    e.Time = e.Time.Add(new TimeSpan(0, minutes, 0));
                }
            }
        }

        //sets the next entry
        public void SetNextEntry()
        {
            RouteTimeTableEntry entry = null;

            if (IsPassengerFlight())
            {
                entry = AllClasses.Keys.ElementAt(CurrentFlight);
            }

            if (IsCargoFlight())
            {
                entry = AllCargo.Keys.ElementAt(CurrentFlight);
            }

            Entry = entry;

            if (entry != null && (entry.TimeTable.Route.Type == Route.RouteType.Mixed
                                  || entry.TimeTable.Route.Type == Route.RouteType.Passenger))
            {
                Classes = AllClasses[entry];
            }

            if (entry != null && (entry.TimeTable.Route.Type == Route.RouteType.Mixed
                                  || entry.TimeTable.Route.Type == Route.RouteType.Cargo))
            {
                Cargo = AllCargo[entry];
            }

            if (Entry != null)
            {
                Airliner = Entry.Airliner;

                FlightTime = CurrentFlight == 0 ? MathHelpers.ConvertEntryToDate(Entry, 0) : GameObject.GetInstance().GameTime.Add(FleetAirlinerHelpers.GetMinTimeBetweenFlights(Airliner));
            }

            IsOnTime = true;

            if (entry != null)
                DistanceToDestination =
                    MathHelpers.GetDistance(
                        entry.Destination.Airport.Profile.Coordinates.ConvertToGeoCoordinate(),
                        entry.DepartureAirport.Profile.Coordinates.ConvertToGeoCoordinate());

            CurrentFlight++;
        }

        #endregion

        #region Methods

        private void CreateEntries(RouteTimeTableEntry mainEntry)
        {
            List<Route> routes = mainEntry.TimeTable.Route.Stopovers.SelectMany(s => s.Legs).ToList();

            TimeSpan time = mainEntry.Time;

            Boolean isInbound = mainEntry.DepartureAirport == mainEntry.TimeTable.Route.Destination2;

            if (isInbound)
            {
                routes.Reverse();
            }

            foreach (Route route in routes)
            {
                var timetable = new RouteTimeTable(route);

                RouteTimeTableEntry entry;
                //inbound
                if (isInbound)
                {
                    entry = new RouteTimeTableEntry(
                        timetable,
                        mainEntry.Day,
                        time,
                        new RouteEntryDestination(route.Destination1, mainEntry.Destination.FlightCode));

                    time =
                        time.Add(entry.TimeTable.Route.GetFlightTime(mainEntry.Airliner.Airliner.Type))
                            .Add(FleetAirlinerHelpers.GetMinTimeBetweenFlights(mainEntry.Airliner));
                    entry.Airliner = mainEntry.Airliner;
                    entry.MainEntry = mainEntry;
                }
                    //outbound
                else
                {
                    entry = new RouteTimeTableEntry(
                        timetable,
                        mainEntry.Day,
                        time,
                        new RouteEntryDestination(route.Destination2, mainEntry.Destination.FlightCode)) {Airliner = mainEntry.Airliner, MainEntry = mainEntry};

                    time =
                        time.Add(entry.TimeTable.Route.GetFlightTime(mainEntry.Airliner.Airliner.Type))
                            .Add(FleetAirlinerHelpers.GetMinTimeBetweenFlights(mainEntry.Airliner));
                }

                if (route.Type == Route.RouteType.Passenger || route.Type == Route.RouteType.Mixed)
                {
                    var classes = new List<FlightAirlinerClass>();
                    foreach (AirlinerClass aClass in Airliner.Airliner.Classes)
                    {
                        FlightAirlinerClass faClass;
                        if (isInbound)
                        {
                            faClass = new FlightAirlinerClass(
                                ((PassengerRoute) route).GetRouteAirlinerClass(aClass.Type),
                                PassengerHelpers.GetStopoverFlightPassengers(
                                    Airliner,
                                    aClass.Type,
                                    route.Destination2,
                                    route.Destination1,
                                    routes,
                                    true));
                        }
                        else
                        {
                            faClass = new FlightAirlinerClass(
                                ((PassengerRoute) route).GetRouteAirlinerClass(aClass.Type),
                                PassengerHelpers.GetStopoverFlightPassengers(
                                    Airliner,
                                    aClass.Type,
                                    route.Destination1,
                                    route.Destination2,
                                    routes,
                                    false));
                        }

                        classes.Add(faClass);
                    }

                    AllClasses.Add(entry, classes);
                }
                if (route.Type == Route.RouteType.Cargo || route.Type == Route.RouteType.Mixed)
                {
                    if (isInbound)
                    {
                        AllCargo.Add(
                            entry,
                            PassengerHelpers.GetStopoverFlightCargo(
                                Airliner,
                                route.Destination2,
                                route.Destination1,
                                routes,
                                true));
                    }
                    else
                    {
                        AllCargo.Add(
                            entry,
                            PassengerHelpers.GetStopoverFlightCargo(
                                Airliner,
                                route.Destination1,
                                route.Destination2,
                                routes,
                                false));
                    }
                }
            }
        }

        #endregion
    }
}