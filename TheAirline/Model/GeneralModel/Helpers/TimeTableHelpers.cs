using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;

namespace TheAirline.Model.GeneralModel.Helpers
{
    //the helpers class for time tables
    public class TimeTableHelpers
    {
        //checks if a time table is valid
        public static Boolean IsTimeTableValid(RouteTimeTable timeTable, FleetAirliner airliner, List<RouteTimeTableEntry> entries, Boolean withSlots = true)
        {
            foreach (RouteTimeTableEntry e in timeTable.Entries)
            {
                if (!IsRouteEntryValid(e, airliner, entries, withSlots))
                    return false;
            }
            return true;
        }
        public static Boolean IsTimeTableValid(RouteTimeTable timeTable, FleetAirliner airliner, Dictionary<Route, List<RouteTimeTableEntry>> entries, Boolean withSlots = true)
        {
            foreach (RouteTimeTableEntry e in timeTable.Entries)
            {
                if (!IsRouteEntryValid(e, airliner, entries, new Dictionary<Route, List<RouteTimeTableEntry>>(),withSlots))
                    return false;
            }
            return true;
        }
        public static Boolean IsTimeTableValid(RouteTimeTableEntry entry, FleetAirliner airliner, Dictionary<Route, List<RouteTimeTableEntry>> entries, Boolean withSlots = true)
        {
            return IsRouteEntryValid(entry, airliner, entries, new Dictionary<Route, List<RouteTimeTableEntry>>(), withSlots);

        }

        //checks if an entry is in occupied slot
        public static Boolean IsRouteEntryInOccupied(RouteTimeTableEntry entry, FleetAirliner airliner)
        {
            var occupiedSlots1 = AirportHelpers.GetOccupiedSlotTimes(entry.DepartureAirport, airliner.Airliner.Airline);
            var occupiedSlots2 = AirportHelpers.GetOccupiedSlotTimes(entry.Destination.Airport, airliner.Airliner.Airline);

            TimeSpan gateTimeBefore = new TimeSpan(0, 15, 0);
            TimeSpan gateTimeAfter = new TimeSpan(0, 15, 0);

            TimeSpan entryTakeoffTime = new TimeSpan((int)entry.Day, entry.Time.Hours, entry.Time.Minutes, entry.Time.Seconds);
            TimeSpan entryLandingTime = entryTakeoffTime.Add(entry.TimeTable.Route.getFlightTime(entry.Airliner.Airliner.Type));

            if (entryLandingTime.Days > 6)
                entryLandingTime = new TimeSpan(0, entryLandingTime.Hours, entryLandingTime.Minutes, entryLandingTime.Seconds);

            TimeSpan entryStartTakeoffTime = entryTakeoffTime.Subtract(gateTimeBefore);
            TimeSpan entryEndTakeoffTime = entryTakeoffTime.Add(gateTimeAfter);

            TimeSpan tTakeoffTime = new TimeSpan(entryStartTakeoffTime.Days, entryStartTakeoffTime.Hours, (entryStartTakeoffTime.Minutes / 15) * 15, 0);

            while (tTakeoffTime < entryEndTakeoffTime)
            {
                if (occupiedSlots1.Contains(tTakeoffTime))
                    return true;

                tTakeoffTime = tTakeoffTime.Add(new TimeSpan(0, 15, 0));
            }

            TimeSpan entryStartLandingTime = entryLandingTime.Subtract(gateTimeBefore);
            TimeSpan entryEndLandingTime = entryLandingTime.Add(gateTimeAfter);

            TimeSpan tLandingTime = new TimeSpan(entryStartLandingTime.Days, entryStartLandingTime.Hours, (entryStartLandingTime.Minutes / 15) * 15, 0);

            while (tLandingTime < entryEndLandingTime)
            {
                if (occupiedSlots2.Contains(tLandingTime))
                    return true;

                tLandingTime = tLandingTime.Add(new TimeSpan(0, 15, 0));
            }

            return false;
        }
        //checks if an entry is valid
        private static Boolean IsRouteEntryValid(RouteTimeTableEntry entry, FleetAirliner airliner, List<RouteTimeTableEntry> entries,Boolean withSlots)
        {
            TimeSpan flightTime = entry.TimeTable.Route.getFlightTime(airliner.Airliner.Type).Add(FleetAirlinerHelpers.GetMinTimeBetweenFlights(airliner));

            TimeSpan startTime = new TimeSpan((int)entry.Day, entry.Time.Hours, entry.Time.Minutes, entry.Time.Seconds);

            TimeSpan endTime = startTime.Add(flightTime);
            if (endTime.Days == 7)
                endTime = new TimeSpan(0, endTime.Hours, endTime.Minutes, endTime.Seconds);

            List<RouteTimeTableEntry> airlinerEntries = entries;

            if (withSlots)
                if (IsRouteEntryInOccupied(entry, airliner))
                    return false;


            // airlinerEntries.AddRange(entry.TimeTable.Entries.FindAll(e => e.Destination.Airport == entry.Destination.Airport));

            foreach (RouteTimeTableEntry e in airlinerEntries)
            {
                TimeSpan eStartTime = new TimeSpan((int)e.Day, e.Time.Hours, e.Time.Minutes, e.Time.Seconds);

                TimeSpan eFlightTime = e.TimeTable.Route.getFlightTime(airliner.Airliner.Type).Add(FleetAirlinerHelpers.GetMinTimeBetweenFlights(airliner));

                TimeSpan eEndTime = eStartTime.Add(eFlightTime);

                double diffStartTime = Math.Abs(eStartTime.Subtract(startTime).TotalMinutes);
                double diffEndTime = Math.Abs(eEndTime.Subtract(endTime).TotalMinutes);

                if (eEndTime.Days == 7)
                    eEndTime = new TimeSpan(0, eEndTime.Hours, eEndTime.Minutes, eEndTime.Seconds);

                if ((eStartTime >= startTime && endTime >= eStartTime) || (eEndTime >= startTime && endTime >= eEndTime) || (endTime >= eStartTime && eEndTime >= endTime) || (startTime >= eStartTime && eEndTime >= startTime))
                {
                    if (e.Airliner == airliner || diffEndTime < 15 || diffStartTime < 15)
                    {


                        return false;
                    }
                }
            }
            double minutesPerWeek = 7 * 24 * 60;

            RouteTimeTableEntry nextEntry = GetNextEntry(entry, airliner, entries);

            RouteTimeTableEntry previousEntry = GetPreviousEntry(entry, airliner, entries);

            if (nextEntry != null && previousEntry != null)
            {

                TimeSpan flightTimeNext = MathHelpers.GetFlightTime(entry.Destination.Airport.Profile.Coordinates, nextEntry.DepartureAirport.Profile.Coordinates, airliner.Airliner.Type.CruisingSpeed).Add(FleetAirlinerHelpers.GetMinTimeBetweenFlights(airliner));
                TimeSpan flightTimePrevious = MathHelpers.GetFlightTime(entry.DepartureAirport.Profile.Coordinates, previousEntry.Destination.Airport.Profile.Coordinates, airliner.Airliner.Type.CruisingSpeed).Add(FleetAirlinerHelpers.GetMinTimeBetweenFlights(airliner));


                TimeSpan prevDate = new TimeSpan((int)previousEntry.Day, previousEntry.Time.Hours, previousEntry.Time.Minutes, previousEntry.Time.Seconds);
                TimeSpan nextDate = new TimeSpan((int)nextEntry.Day, nextEntry.Time.Hours, nextEntry.Time.Minutes, nextEntry.Time.Seconds);
                TimeSpan currentDate = new TimeSpan((int)entry.Day, entry.Time.Hours, entry.Time.Minutes, entry.Time.Seconds);


                double timeToNext = currentDate.Subtract(nextDate).TotalMinutes > 0 ? minutesPerWeek - currentDate.Subtract(nextDate).TotalMinutes : Math.Abs(currentDate.Subtract(nextDate).TotalMinutes);
                double timeFromPrev = prevDate.Subtract(currentDate).TotalMinutes > 0 ? minutesPerWeek - prevDate.Subtract(currentDate).TotalMinutes : Math.Abs(prevDate.Subtract(currentDate).TotalMinutes);

                if (timeFromPrev > previousEntry.TimeTable.Route.getFlightTime(airliner.Airliner.Type).TotalMinutes + flightTimePrevious.TotalMinutes && timeToNext > entry.TimeTable.Route.getFlightTime(airliner.Airliner.Type).TotalMinutes + flightTimeNext.TotalMinutes)
                    return true;
                else
                {

                    return false;
                }
            }
            else
                return true;

        }
        public static Boolean IsRouteEntryValid(RouteTimeTableEntry entry, FleetAirliner airliner, Dictionary<Route, List<RouteTimeTableEntry>> entries, Dictionary<Route, List<RouteTimeTableEntry>> entriesToDelete, Boolean withSlots)
        {

            TimeSpan flightTime = entry.TimeTable.Route.getFlightTime(airliner.Airliner.Type).Add(FleetAirlinerHelpers.GetMinTimeBetweenFlights(airliner));

            TimeSpan startTime = new TimeSpan((int)entry.Day, entry.Time.Hours, entry.Time.Minutes, entry.Time.Seconds);

            TimeSpan endTime = startTime.Add(flightTime);
            if (endTime.Days == 7)
                endTime = new TimeSpan(0, endTime.Hours, endTime.Minutes, endTime.Seconds);

            List<RouteTimeTableEntry> airlinerEntries = airliner.Routes.SelectMany(r => r.TimeTable.Entries.FindAll(e => e.Airliner == airliner)).ToList();
            airlinerEntries.AddRange(entries.Keys.SelectMany(r => entries[r]));

            //var deletable = this.EntriesToDelete.Keys.SelectMany(r => this.Entries.ContainsKey(r) ? this.Entries[r] : null);
            List<RouteTimeTableEntry> deletable = new List<RouteTimeTableEntry>();
            deletable.AddRange(entriesToDelete.Keys.SelectMany(r => entriesToDelete[r]));

            foreach (Route route in entriesToDelete.Keys)
            {
                if (entries.ContainsKey(route))
                    deletable.AddRange(entries[route]);


            }

            foreach (RouteTimeTableEntry e in deletable)
                if (airlinerEntries.Contains(e))
                    airlinerEntries.Remove(e);

            if (withSlots)
                if (IsRouteEntryInOccupied(entry, airliner))
                    return false;


            // airlinerEntries.AddRange(entry.TimeTable.Entries.FindAll(e => e.Destination.Airport == entry.Destination.Airport));

            foreach (RouteTimeTableEntry e in airlinerEntries)
            {
                TimeSpan eStartTime = new TimeSpan((int)e.Day, e.Time.Hours, e.Time.Minutes, e.Time.Seconds);

                TimeSpan eFlightTime = e.TimeTable.Route.getFlightTime(airliner.Airliner.Type).Add(FleetAirlinerHelpers.GetMinTimeBetweenFlights(airliner));

                TimeSpan eEndTime = eStartTime.Add(eFlightTime);

                double diffStartTime = Math.Abs(eStartTime.Subtract(startTime).TotalMinutes);
                double diffEndTime = Math.Abs(eEndTime.Subtract(endTime).TotalMinutes);

                if (eEndTime.Days == 7)
                    eEndTime = new TimeSpan(0, eEndTime.Hours, eEndTime.Minutes, eEndTime.Seconds);

                if ((eStartTime >= startTime && endTime >= eStartTime) || (eEndTime >= startTime && endTime >= eEndTime) || (endTime >= eStartTime && eEndTime >= endTime) || (startTime >= eStartTime && eEndTime >= startTime))
                {
                    if (e.Airliner == airliner || diffEndTime < 15 || diffStartTime < 15)
                    {


                        return false;
                    }
                }
            }
            double minutesPerWeek = 7 * 24 * 60;

            RouteTimeTableEntry nextEntry = GetNextEntry(entry, airliner, entries, entriesToDelete);

            RouteTimeTableEntry previousEntry = GetPreviousEntry(entry, airliner, entries, entriesToDelete);

            if (nextEntry != null && previousEntry != null)
            {

                TimeSpan flightTimeNext = MathHelpers.GetFlightTime(entry.Destination.Airport.Profile.Coordinates, nextEntry.DepartureAirport.Profile.Coordinates, airliner.Airliner.Type.CruisingSpeed).Add(FleetAirlinerHelpers.GetMinTimeBetweenFlights(airliner));
                TimeSpan flightTimePrevious = MathHelpers.GetFlightTime(entry.DepartureAirport.Profile.Coordinates, previousEntry.Destination.Airport.Profile.Coordinates, airliner.Airliner.Type.CruisingSpeed).Add(FleetAirlinerHelpers.GetMinTimeBetweenFlights(airliner));


                TimeSpan prevDate = new TimeSpan((int)previousEntry.Day, previousEntry.Time.Hours, previousEntry.Time.Minutes, previousEntry.Time.Seconds);
                TimeSpan nextDate = new TimeSpan((int)nextEntry.Day, nextEntry.Time.Hours, nextEntry.Time.Minutes, nextEntry.Time.Seconds);
                TimeSpan currentDate = new TimeSpan((int)entry.Day, entry.Time.Hours, entry.Time.Minutes, entry.Time.Seconds);


                double timeToNext = currentDate.Subtract(nextDate).TotalMinutes > 0 ? minutesPerWeek - currentDate.Subtract(nextDate).TotalMinutes : Math.Abs(currentDate.Subtract(nextDate).TotalMinutes);
                double timeFromPrev = prevDate.Subtract(currentDate).TotalMinutes > 0 ? minutesPerWeek - prevDate.Subtract(currentDate).TotalMinutes : Math.Abs(prevDate.Subtract(currentDate).TotalMinutes);

                if (timeFromPrev > previousEntry.TimeTable.Route.getFlightTime(airliner.Airliner.Type).TotalMinutes + flightTimePrevious.TotalMinutes && timeToNext > entry.TimeTable.Route.getFlightTime(airliner.Airliner.Type).TotalMinutes + flightTimeNext.TotalMinutes)
                    return true;
                else
                {

                    return false;
                }
            }
            else
                return true;

        }
        //returns the previous entry before a specific entry
        private static RouteTimeTableEntry GetPreviousEntry(RouteTimeTableEntry entry, FleetAirliner airliner, Dictionary<Route, List<RouteTimeTableEntry>> entries, Dictionary<Route, List<RouteTimeTableEntry>> entriesToDelete)
        {
            TimeSpan tsEntry = new TimeSpan((int)entry.Day, entry.Time.Hours, entry.Time.Minutes, entry.Time.Seconds);
            DayOfWeek eDay = entry.Day;

            int counter = 0;

            while (counter < 7)
            {

                List<RouteTimeTableEntry> tEntries = airliner.Routes.SelectMany(r => r.TimeTable.Entries.FindAll(e => e.Airliner == airliner && e.Day == eDay)).ToList();
                tEntries.AddRange(entries.Keys.SelectMany(r => entries[r].FindAll(e => e.Day == eDay)));
                tEntries.RemoveAll(e => entriesToDelete.Keys.SelectMany(r => entriesToDelete[r]).ToList().Find(te => te == e) == e);

                tEntries = (from e in tEntries orderby e.Time descending select e).ToList();

                foreach (RouteTimeTableEntry dEntry in tEntries)
                {
                    TimeSpan ts = new TimeSpan((int)eDay, dEntry.Time.Hours, dEntry.Time.Minutes, dEntry.Time.Seconds);
                    if (ts < tsEntry)
                        return dEntry;

                }
                counter++;

                eDay--;

                if (((int)eDay) == -1)
                {
                    eDay = (DayOfWeek)6;
                    tsEntry = new TimeSpan(7, tsEntry.Hours, tsEntry.Minutes, tsEntry.Seconds);
                }

            }

            return null;

        }
        private static RouteTimeTableEntry GetPreviousEntry(RouteTimeTableEntry entry, FleetAirliner airliner, List<RouteTimeTableEntry> entries)
        {
            TimeSpan tsEntry = new TimeSpan((int)entry.Day, entry.Time.Hours, entry.Time.Minutes, entry.Time.Seconds);
            DayOfWeek eDay = entry.Day;

            int counter = 0;

            while (counter < 7)
            {

                List<RouteTimeTableEntry> tEntries = entries;

                tEntries = (from e in tEntries orderby e.Time descending select e).ToList();

                foreach (RouteTimeTableEntry dEntry in tEntries)
                {
                    TimeSpan ts = new TimeSpan((int)eDay, dEntry.Time.Hours, dEntry.Time.Minutes, dEntry.Time.Seconds);
                    if (ts < tsEntry)
                        return dEntry;

                }
                counter++;

                eDay--;

                if (((int)eDay) == -1)
                {
                    eDay = (DayOfWeek)6;
                    tsEntry = new TimeSpan(7, tsEntry.Hours, tsEntry.Minutes, tsEntry.Seconds);
                }

            }

            return null;

        }
        //returns the next entry after a specific entry
        private static RouteTimeTableEntry GetNextEntry(RouteTimeTableEntry entry, FleetAirliner airliner, Dictionary<Route, List<RouteTimeTableEntry>> entries, Dictionary<Route, List<RouteTimeTableEntry>> entriesToDelete)
        {

            DayOfWeek day = entry.Day;

            int counter = 0;

            while (counter < 7)
            {

                List<RouteTimeTableEntry> tEntries = airliner.Routes.SelectMany(r => r.TimeTable.Entries.FindAll(e => e.Airliner == airliner && e.Day == day)).ToList();
                tEntries.AddRange(entries.Keys.SelectMany(r => entries[r].FindAll(e => e.Day == day)));
                tEntries.RemoveAll(e => entriesToDelete.Keys.SelectMany(r => entriesToDelete[r]).ToList().Find(te => te == e) == e);

                tEntries = (from e in tEntries orderby e.Time select e).ToList();


                foreach (RouteTimeTableEntry dEntry in tEntries)
                {
                    if (!((dEntry.Day == entry.Day && dEntry.Time <= entry.Time)))
                        return dEntry;
                }
                day++;

                if (day == (DayOfWeek)7)
                    day = (DayOfWeek)0;

                counter++;

            }

            return null;

        }
        private static RouteTimeTableEntry GetNextEntry(RouteTimeTableEntry entry, FleetAirliner airliner, List<RouteTimeTableEntry> entries)
        {

            DayOfWeek day = entry.Day;

            int counter = 0;

            while (counter < 7)
            {

                List<RouteTimeTableEntry> tEntries = entries;

                tEntries = (from e in tEntries orderby e.Time select e).ToList();

                foreach (RouteTimeTableEntry dEntry in tEntries)
                {
                    if (!((dEntry.Day == entry.Day && dEntry.Time <= entry.Time)))
                        return dEntry;
                }
                day++;

                if (day == (DayOfWeek)7)
                    day = (DayOfWeek)0;

                counter++;

            }

            return null;

        }
    }
}
