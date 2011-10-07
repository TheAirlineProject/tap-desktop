using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirlinerModel.RouteModel
{
    //the class for a route time table for a route airliner
    public class RouteTimeTable
    {
        public static TimeSpan MinTimeBetweenFlights = new TimeSpan(0, 60, 0);
        public List<RouteTimeTableEntry> Entries { get; set; }
        public Route Route { get; set; }
        public RouteTimeTable(Route route)
        {
            this.Entries = new List<RouteTimeTableEntry>();
            this.Route = route;
        }
        //adds an entry to the timetable
        public void addEntry(RouteTimeTableEntry entry)
        {
            this.Entries.Add(entry);
        }
        //removes an entry from the time table
        public void removeEntry(RouteTimeTableEntry entry)
        {
            this.Entries.Remove(entry);
        }
        //returns all route entry destinations 
        public List<RouteEntryDestination> getRouteEntryDestinations()
        {
            List<RouteEntryDestination> destinations = new List<RouteEntryDestination>();
            List<string> codes = new List<string>();
            
            
            foreach (RouteTimeTableEntry entry in this.Entries)
                if (!codes.Contains(entry.Destination.FlightCode))
                {
                    destinations.Add(entry.Destination);
                    codes.Add(entry.Destination.FlightCode);
                }


            return destinations;
             

        }
        //adds entries for a specific destination and time for each day of the week
        public void addDailyEntries(RouteEntryDestination destination, TimeSpan time)
        {
            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                this.Entries.Add(new RouteTimeTableEntry(this,day, time, destination));
        }
        //returns all entries for a specific destination
        public List<RouteTimeTableEntry> getEntries(Airport destination)
        {
            return this.Entries.FindAll((delegate(RouteTimeTableEntry entry) { return entry.Destination.Airport == destination; }));
        }
        //returns all entries for a specific day
        public List<RouteTimeTableEntry> getEntries(DayOfWeek day)
        {
            return this.Entries.FindAll((delegate(RouteTimeTableEntry entry) { return entry.Day == day; }));
        }
        //returns a entry if possible in a specific timespan on a specific day of the week
        public RouteTimeTableEntry getEntry(DayOfWeek day, TimeSpan startTime, TimeSpan endTime)
        {
            return getEntries(day).Find((delegate(RouteTimeTableEntry entry) { return entry.Time >= startTime && entry.Time <= endTime; }));
        }
        //returns the next entry after a specific date and not to a specific coordinates (airport)
        public RouteTimeTableEntry getNextEntry(DateTime time, Coordinates coordinates)
        {
            DayOfWeek day = time.DayOfWeek;

             int counter = 0;

            while (counter < 7)
            {


              
                List<RouteTimeTableEntry> entries = getEntries(day);

                foreach (RouteTimeTableEntry dEntry in entries)
                {
                    if (!((dEntry.Day == time.DayOfWeek && dEntry.Time <= time.TimeOfDay)) && dEntry.Destination.Airport.Profile.Coordinates.CompareTo(coordinates) != 0)
                        return dEntry;
                }
                day++;

                if (day == (DayOfWeek)7)
                    day = (DayOfWeek)0;

                counter++;
            }

            return null;
        }

        
        //returns the next entry after a specific specific entry
        public RouteTimeTableEntry getNextEntry(RouteTimeTableEntry entry)
        {
            DayOfWeek eDay = entry.Day;
       
            int counter = 0;

            while (counter < 7)
            {


                  List<RouteTimeTableEntry> entries = getEntries(eDay);

                foreach (RouteTimeTableEntry dEntry in entries)
                {
                    if (!((dEntry.Day == entry.Day && dEntry.Time <= entry.Time)) && dEntry.Destination != entry.Destination)
                        return dEntry;
                }
                counter++;

                eDay++;

                if (eDay == (DayOfWeek)7)
                    eDay = (DayOfWeek)0;
             
            }

            return null;
        }

    }
    //the entry for a route time table
    public class RouteTimeTableEntry :  IComparable<RouteTimeTableEntry>
    {
        public DayOfWeek Day { get; set; }
        public TimeSpan Time { get; set; }
        public RouteTimeTable TimeTable { get; set; }
        public RouteEntryDestination Destination { get; set; }
        public RouteTimeTableEntry(RouteTimeTable timeTable, DayOfWeek day, TimeSpan time, RouteEntryDestination destination)
        {
            this.Day = day;
            this.Time = time;
            this.TimeTable = timeTable;
            this.Destination = destination;
        }
        //returns the departure airport
        public Airport getDepartureAirport()
        {
            return this.Destination.Airport == this.TimeTable.Route.Destination1 ? this.TimeTable.Route.Destination2 : this.TimeTable.Route.Destination1;
        }


        public int CompareTo(RouteTimeTableEntry entry)
        {
            int compare = entry.Day.CompareTo(this.Day);
            if (compare == 0)
                return entry.Time.CompareTo(this.Time);
            return compare;
        }
        //returns the timespan between two entries
        public TimeSpan getTimeDifference(RouteTimeTableEntry entry)
        {
            int daysBetween = Math.Abs(this.Day - entry.Day);

            TimeSpan time = entry.Time.Subtract(this.Time);

            return new TimeSpan(24 * daysBetween, 0, 0).Add(time);
        }
        
    }
    
}
