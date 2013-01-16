using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirlinerModel.RouteModel
{
    //the class for a stopover flight
    public class StopoverFlight : Flight
    {
        private int currentFlight;
        public Dictionary<RouteTimeTableEntry, List<FlightAirlinerClass>> AllClasses { get; set; }
        public Boolean IsLastTrip { get { return isLastTrip(); } set { ;} }
        public StopoverFlight(RouteTimeTableEntry entry)
            : base(entry)
        {

            this.currentFlight = 0;
            this.AllClasses = new Dictionary<RouteTimeTableEntry, List<FlightAirlinerClass>>();

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

                    time = time.Add(entry.TimeTable.Route.getFlightTime(mainEntry.Airliner.Airliner.Type)).Add(RouteTimeTable.MinTimeBetweenFlights);
                    entry.Airliner = mainEntry.Airliner;
                    entry.MainEntry = mainEntry;

                }
                //outbound
                else
                {
                    entry = new RouteTimeTableEntry(timetable, mainEntry.Day, time, new RouteEntryDestination(route.Destination2, mainEntry.Destination.FlightCode));
                    entry.Airliner = mainEntry.Airliner;
                    entry.MainEntry = mainEntry;

                    time = time.Add(entry.TimeTable.Route.getFlightTime(mainEntry.Airliner.Airliner.Type)).Add(RouteTimeTable.MinTimeBetweenFlights);

                }

                List<FlightAirlinerClass> classes = new List<FlightAirlinerClass>();
                foreach (AirlinerClass aClass in this.Airliner.Airliner.Classes)
                {
                    FlightAirlinerClass faClass;
                    if (isInbound)
                        faClass = new FlightAirlinerClass(route.getRouteAirlinerClass(aClass.Type), PassengerHelpers.GetStopoverFlightPassengers(this.Airliner, aClass.Type, route.Destination2, route.Destination1, routes, isInbound));
                    else
                        faClass = new FlightAirlinerClass(route.getRouteAirlinerClass(aClass.Type), PassengerHelpers.GetStopoverFlightPassengers(this.Airliner, aClass.Type, route.Destination1, route.Destination2, routes, isInbound));

                    classes.Add(faClass);
                }

                this.AllClasses.Add(entry, classes);
            }
        }
        //sets the next entry
        public void setNextEntry()
        {

            RouteTimeTableEntry entry = this.AllClasses.Keys.ElementAt(currentFlight);

            this.Entry = entry;
            this.Classes = this.AllClasses[entry];

            this.Airliner = this.Entry.Airliner;

            if (currentFlight == 0)
                this.FlightTime = MathHelpers.ConvertEntryToDate(this.Entry, 0);
            else
                this.FlightTime = GameObject.GetInstance().GameTime.Add(RouteTimeTable.MinTimeBetweenFlights);
            
            this.IsOnTime = true;

            currentFlight++;

        }
        //returns if the entry is the last of the trip
        private Boolean isLastTrip()
        {
            return currentFlight == this.AllClasses.Keys.Count;//this.AllClasses.Keys.ToList().IndexOf(this.Entry.TimeTable.Route) == this.AllClasses.Keys.Count -1;

        }
        public override void addDelayMinutes(int minutes)
        {
            base.addDelayMinutes(minutes);

            foreach (RouteTimeTableEntry e in this.AllClasses.Keys)
                e.Time = e.Time.Add(new TimeSpan(0, minutes, 0));

        }
    }
}
