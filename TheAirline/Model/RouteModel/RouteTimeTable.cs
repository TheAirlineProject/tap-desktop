using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.Model.GeneralModel.WeatherModel;

namespace TheAirline.Model.RouteModel
{
    /*! RouteTimeTable
* This class is used for the time table for a route
* The class needs parameters for the route
*/

    [Serializable]
    public class RouteTimeTable : BaseModel
    {
        #region Constructors and Destructors

        public RouteTimeTable(Route route)
        {
            Entries = new List<RouteTimeTableEntry>();
            Route = route;
        }

        private RouteTimeTable(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("entries")]
        public List<RouteTimeTableEntry> Entries { get; set; }

        [Versioning("route")]
        public Route Route { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        //adds entries for a specific destination and time for each day of the week assigned to an airliner
        public void AddDailyEntries(RouteEntryDestination destination, TimeSpan time, FleetAirliner airliner)
        {
            foreach (DayOfWeek day in Enum.GetValues(typeof (DayOfWeek)))
            {
                var entry = new RouteTimeTableEntry(this, day, time, destination) {Airliner = airliner};

                Entries.Add(entry);
            }
        }

        //adds entries for a specific destination and time for each day of the week
        public void AddDailyEntries(RouteEntryDestination destination, TimeSpan time)
        {
            AddDailyEntries(destination, time, null);
        }

        public void AddEntry(RouteTimeTableEntry entry)
        {
            Entries.Add(entry);
        }

        //adds entries for a specific destination and for each weekday of the week assinged to an airliner
        public void AddWeekDailyEntries(RouteEntryDestination destination, TimeSpan time)
        {
            foreach (DayOfWeek day in Enum.GetValues(typeof (DayOfWeek)))
            {
                if (day != DayOfWeek.Saturday && day != DayOfWeek.Sunday)
                {
                    var entry = new RouteTimeTableEntry(this, day, time, destination) {Airliner = null};

                    Entries.Add(entry);
                }
            }
        }

        /*
        //returns all entries for a specific airliner
        public List<RouteTimeTableEntry> getEntries(FleetAirliner airliner)
        {
            return this.Entries.FindAll(e => e.Airliner == airliner);
        }*/
        //returns all entries for a specific destination
        public List<RouteTimeTableEntry> GetEntries(Airport destination)
        {
            return Entries.FindAll(e => e.Destination.Airport == destination);
        }

        //returns all entries for a specific day
        public List<RouteTimeTableEntry> GetEntries(DayOfWeek day, Boolean useSeason = true)
        {
            Weather.Season season = GeneralHelpers.GetSeason(GameObject.GetInstance().GameTime);

            if (useSeason)
            {
                return
                    Entries.FindAll(
                        e =>
                        e.Day == day
                        && (e.TimeTable.Route.Season == Weather.Season.AllYear
                            || e.TimeTable.Route.Season == season));
            }
            return Entries.FindAll(e => e.Day == day);
        }

        //returns a entry if possible in a specific timespan on a specific day of the week
        public RouteTimeTableEntry GetEntry(DayOfWeek day, TimeSpan startTime, TimeSpan endTime)
        {
            return GetEntries(day).Find(e => e.Time >= startTime && e.Time <= endTime);
        }

        //returns the next entry after a specific date and with a specific airliner
        public RouteTimeTableEntry GetNextEntry(DateTime time, FleetAirliner airliner)
        {
            var dt = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second);

            int counter = 0;
            while (counter < 8)
            {
                List<RouteTimeTableEntry> entries = GetEntries(dt.DayOfWeek).FindAll(e => airliner == e.Airliner);

                foreach (RouteTimeTableEntry dEntry in (from e in entries orderby e.Time select e))
                {
                    if (!(dEntry.Time <= dt.TimeOfDay && dt.Day == time.Day))
                    {
                        return dEntry;
                    }
                }
                dt = dt.AddDays(1);

                counter++;
            }

            RouteTimeTableEntry entry = Entries.Find(e => e.Airliner == airliner);
            return entry;
        }

        //returns the next entry after a specific date and not to a specific coordinates (airport)
        public RouteTimeTableEntry GetNextEntry(DateTime time, Airport airport)
        {
            DayOfWeek day = time.DayOfWeek;

            int counter = 0;

            while (counter < 8)
            {
                List<RouteTimeTableEntry> entries = GetEntries(day);

                foreach (RouteTimeTableEntry dEntry in (from e in entries orderby e.Time select e))
                {
                    if (!((dEntry.Day == time.DayOfWeek && dEntry.Time <= time.TimeOfDay))
                        && dEntry.Destination.Airport != airport)
                    {
                        return dEntry;
                    }
                }
                day++;

                if (day == (DayOfWeek) 7)
                {
                    day = 0;
                }

                counter++;
            }

            return null;
        }

        //returns the next entry from a specific time
        public RouteTimeTableEntry GetNextEntry(DateTime time)
        {
            DayOfWeek day = time.DayOfWeek;

            int counter = 0;

            while (counter < 7)
            {
                List<RouteTimeTableEntry> entries = GetEntries(day);

                foreach (RouteTimeTableEntry dEntry in (from e in entries orderby e.Time select e))
                {
                    if (!((dEntry.Day == time.DayOfWeek && dEntry.Time <= time.TimeOfDay)))
                    {
                        return dEntry;
                    }
                }
                day++;

                if (day == (DayOfWeek) 7)
                {
                    day = 0;
                }

                counter++;
            }

            return null;
        }

        //returns the next entry after a specific entry
        public RouteTimeTableEntry GetNextEntry(RouteTimeTableEntry entry)
        {
            DayOfWeek eDay = entry.Day;

            int counter = 0;

            while (counter < 7)
            {
                List<RouteTimeTableEntry> entries = GetEntries(eDay);

                foreach (RouteTimeTableEntry dEntry in (from e in entries orderby e.Time select e))
                {
                    if (!((dEntry.Day == entry.Day && dEntry.Time <= entry.Time))) // && dEntry.Destination != entry.Destination)
                    {
                        return dEntry;
                    }
                }
                counter++;

                eDay++;

                if (eDay == (DayOfWeek) 7)
                {
                    eDay = 0;
                }
            }

            return entry;
        }

        public List<RouteEntryDestination> GetRouteEntryDestinations()
        {
            var destinations = new List<RouteEntryDestination>();
            var codes = new List<string>();

            foreach (RouteTimeTableEntry entry in Entries)
            {
                if (!codes.Contains(entry.Destination.FlightCode))
                {
                    destinations.Add(entry.Destination);
                    codes.Add(entry.Destination.FlightCode);
                }
            }

            return destinations;
        }

        public void RemoveEntry(RouteTimeTableEntry entry)
        {
            Entries.Remove(entry);
        }

        #endregion
    }
}