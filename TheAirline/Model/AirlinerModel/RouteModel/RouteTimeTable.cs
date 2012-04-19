using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirlinerModel.RouteModel
{
    /*! RouteTimeTable
* This class is used for the time table for a route
* The class needs parameters for the route
*/
    public class RouteTimeTable
    {
        public static TimeSpan MinTimeBetweenFlights = new TimeSpan(0, 45, 0);
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
            var entries = from e in this.Entries where e.Destination.Airport.Profile.Coordinates.CompareTo(coordinates)!=0 orderby MathHelpers.ConvertEntryToDate(e).Subtract(time) select e;

      
            return entries.FirstOrDefault();          
  /*

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
             * */
        }

        //returns the next entry from a specific time
        public RouteTimeTableEntry getNextEntry(DateTime time)
        {
            /*
            DayOfWeek day = time.DayOfWeek;

            int counter = 0;

            while (counter < 7)
            {



                List<RouteTimeTableEntry> entries = getEntries(day);

                foreach (RouteTimeTableEntry dEntry in entries)
                {
                    if (!((dEntry.Day == time.DayOfWeek && dEntry.Time <= time.TimeOfDay)))
                        return dEntry;
                }
                day++;

                if (day == (DayOfWeek)7)
                    day = (DayOfWeek)0;

                counter++;
            }

            return null;*/

            var entries = from e in this.Entries orderby MathHelpers.ConvertEntryToDate(e).Subtract(time) select e;

            return entries.FirstOrDefault() ;          
      
        }
        //returns the next entry after a specific specific entry
        public RouteTimeTableEntry getNextEntry(RouteTimeTableEntry entry)
        {
            DateTime time = MathHelpers.ConvertEntryToDate(entry);
            var entries = from e in this.Entries orderby MathHelpers.ConvertEntryToDate(e).Subtract(time) where MathHelpers.ConvertEntryToDate(e).Subtract(time).TotalMinutes>0 && e.Destination!=entry.Destination select e;

            return entries.FirstOrDefault();
            /*
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
            */
        }

    }
   
}
