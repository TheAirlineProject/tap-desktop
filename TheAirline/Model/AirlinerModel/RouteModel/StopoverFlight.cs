
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.Model.AirlinerModel.RouteModel
{
    //the class for a stopover flight
    [Serializable]
    public class StopoverFlight : Flight
    {
        public int CurrentFlight { get; set; }
        public Dictionary<RouteTimeTableEntry, List<FlightAirlinerClass>> AllClasses { get; set; }
        public Dictionary<RouteTimeTableEntry, double> AllCargo { get; set; }
        public Boolean IsLastTrip { get { return isLastTrip(); } set { ;} }
        public StopoverFlight(RouteTimeTableEntry entry)
            : base(entry)
        {

            this.CurrentFlight = 0;
            this.AllClasses = new Dictionary<RouteTimeTableEntry, List<FlightAirlinerClass>>();
            this.AllCargo = new Dictionary<RouteTimeTableEntry, double>();

            List<Route> legs = entry.TimeTable.Route.Stopovers.SelectMany(s => s.Legs).ToList();

            Boolean isInbound = entry.DepartureAirport == entry.TimeTable.Route.Destination2;

            if (isInbound)
                legs.Reverse();

            createEntries(entry);
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
        //creates the entries for the stopoverflight
        private void createEntries(RouteTimeTableEntry mainEntry)
        {
            List<Route> routes = mainEntry.TimeTable.Route.Stopovers.SelectMany(s => s.Legs).ToList();

            TimeSpan time = mainEntry.Time;

            Boolean isInbound = mainEntry.DepartureAirport == mainEntry.TimeTable.Route.Destination2;

            if (isInbound)
                routes.Reverse();

            foreach (Route route in routes)
            {
                RouteTimeTable timetable = new RouteTimeTable(route);

                RouteTimeTableEntry entry;
                //inbound
                if (isInbound)
                {
                    entry = new RouteTimeTableEntry(timetable, mainEntry.Day, time, new RouteEntryDestination(route.Destination1, mainEntry.Destination.FlightCode));

                    time = time.Add(entry.TimeTable.Route.getFlightTime(mainEntry.Airliner.Airliner.Type)).Add(FleetAirlinerHelpers.GetMinTimeBetweenFlights(mainEntry.Airliner));
                    entry.Airliner = mainEntry.Airliner;
                    entry.MainEntry = mainEntry;

                }
                //outbound
                else
                {
                    entry = new RouteTimeTableEntry(timetable, mainEntry.Day, time, new RouteEntryDestination(route.Destination2, mainEntry.Destination.FlightCode));
                    entry.Airliner = mainEntry.Airliner;
                    entry.MainEntry = mainEntry;

                    time = time.Add(entry.TimeTable.Route.getFlightTime(mainEntry.Airliner.Airliner.Type)).Add(FleetAirlinerHelpers.GetMinTimeBetweenFlights(mainEntry.Airliner));

                }

                if (route.Type == Route.RouteType.Passenger || route.Type == Route.RouteType.Mixed)
                {
                    List<FlightAirlinerClass> classes = new List<FlightAirlinerClass>();
                    foreach (AirlinerClass aClass in this.Airliner.Airliner.Classes)
                    {
                        FlightAirlinerClass faClass;
                        if (isInbound)
                            faClass = new FlightAirlinerClass(((PassengerRoute)route).getRouteAirlinerClass(aClass.Type), PassengerHelpers.GetStopoverFlightPassengers(this.Airliner, aClass.Type, route.Destination2, route.Destination1, routes, isInbound));
                        else
                            faClass = new FlightAirlinerClass(((PassengerRoute)route).getRouteAirlinerClass(aClass.Type), PassengerHelpers.GetStopoverFlightPassengers(this.Airliner, aClass.Type, route.Destination1, route.Destination2, routes, isInbound));

                        classes.Add(faClass);
                    }

                    this.AllClasses.Add(entry, classes);
                }
                if (route.Type == Route.RouteType.Cargo || route.Type == Route.RouteType.Mixed)
                {
                    if (isInbound)
                        this.AllCargo.Add(entry, PassengerHelpers.GetStopoverFlightCargo(this.Airliner, route.Destination2, route.Destination1, routes, isInbound));
                    else
                        this.AllCargo.Add(entry, PassengerHelpers.GetStopoverFlightCargo(this.Airliner, route.Destination1, route.Destination2, routes,isInbound));

                }

            }
        }
        //sets the next entry
        public void setNextEntry()
        {
            RouteTimeTableEntry entry=null;

            if (isPassengerFlight())
                entry = this.AllClasses.Keys.ElementAt(CurrentFlight);

            if (isCargoFlight())
                entry = this.AllCargo.Keys.ElementAt(CurrentFlight);

            this.Entry = entry;

            if (entry.TimeTable.Route.Type == Route.RouteType.Mixed || entry.TimeTable.Route.Type == Route.RouteType.Passenger)
                this.Classes = this.AllClasses[entry];

            if (entry.TimeTable.Route.Type == Route.RouteType.Mixed || entry.TimeTable.Route.Type == Route.RouteType.Cargo)
                this.Cargo = this.AllCargo[entry];

            this.Airliner = this.Entry.Airliner;

            if (CurrentFlight == 0)
                this.FlightTime = MathHelpers.ConvertEntryToDate(this.Entry, 0);
            else
                this.FlightTime = GameObject.GetInstance().GameTime.Add(FleetAirlinerHelpers.GetMinTimeBetweenFlights(this.Airliner));

            this.IsOnTime = true;

            CurrentFlight++;

        }
        //returns if the entry is the last of the trip
        private Boolean isLastTrip()
        {

            return CurrentFlight == Math.Max(this.AllClasses.Keys.Count, this.AllCargo.Keys.Count);//this.AllClasses.Keys.ToList().IndexOf(this.Entry.TimeTable.Route) == this.AllClasses.Keys.Count -1;

        }
        public override void addDelayMinutes(int minutes)
        {
            base.addDelayMinutes(minutes);

            if (isPassengerFlight())
                foreach (RouteTimeTableEntry e in this.AllClasses.Keys)
                    e.Time = e.Time.Add(new TimeSpan(0, minutes, 0));

            if (isCargoFlight())
                foreach (RouteTimeTableEntry e in this.AllCargo.Keys)
                    e.Time = e.Time.Add(new TimeSpan(0, minutes, 0));

        }
    }
}
