namespace TheAirline.Model.GeneralModel.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using TheAirline.Model.AirlinerModel;
    using TheAirline.Model.AirlinerModel.RouteModel;
    using TheAirline.Model.GeneralModel.WeatherModel;

    //the helpers class for time tables
    public class TimeTableHelpers
    {
        //checks if a time table is valid

        //checks if an entry is in occupied slot

        #region Public Methods and Operators

        public static Boolean IsRouteEntryInOccupied(RouteTimeTableEntry entry, FleetAirliner airliner)
        {
            List<TimeSpan> occupiedSlots1 = AirportHelpers.GetOccupiedSlotTimes(
                entry.DepartureAirport,
                airliner.Airliner.Airline,
                GeneralHelpers.GetSeason(GameObject.GetInstance().GameTime));
            List<TimeSpan> occupiedSlots2 = AirportHelpers.GetOccupiedSlotTimes(
                entry.Destination.Airport,
                airliner.Airliner.Airline,
                GeneralHelpers.GetSeason(GameObject.GetInstance().GameTime));

            var gateTimeBefore = new TimeSpan(0, 15, 0);
            var gateTimeAfter = new TimeSpan(0, 15, 0);

            var entryTakeoffTime = new TimeSpan(
                (int)entry.Day,
                entry.Time.Hours,
                entry.Time.Minutes,
                entry.Time.Seconds);
            TimeSpan entryLandingTime =
                entryTakeoffTime.Add(entry.TimeTable.Route.getFlightTime(entry.Airliner.Airliner.Type));

            if (entryLandingTime.Days > 6)
            {
                entryLandingTime = new TimeSpan(
                    0,
                    entryLandingTime.Hours,
                    entryLandingTime.Minutes,
                    entryLandingTime.Seconds);
            }

            TimeSpan entryStartTakeoffTime = entryTakeoffTime.Subtract(gateTimeBefore);
            TimeSpan entryEndTakeoffTime = entryTakeoffTime.Add(gateTimeAfter);

            var tTakeoffTime = new TimeSpan(
                entryStartTakeoffTime.Days,
                entryStartTakeoffTime.Hours,
                (entryStartTakeoffTime.Minutes / 15) * 15,
                0);

            while (tTakeoffTime < entryEndTakeoffTime)
            {
                if (occupiedSlots1.Contains(tTakeoffTime))
                {
                    return true;
                }

                tTakeoffTime = tTakeoffTime.Add(new TimeSpan(0, 15, 0));
            }

            TimeSpan entryStartLandingTime = entryLandingTime.Subtract(gateTimeBefore);
            TimeSpan entryEndLandingTime = entryLandingTime.Add(gateTimeAfter);

            var tLandingTime = new TimeSpan(
                entryStartLandingTime.Days,
                entryStartLandingTime.Hours,
                (entryStartLandingTime.Minutes / 15) * 15,
                0);

            while (tLandingTime < entryEndLandingTime)
            {
                if (occupiedSlots2.Contains(tLandingTime))
                {
                    return true;
                }

                tLandingTime = tLandingTime.Add(new TimeSpan(0, 15, 0));
            }

            return false;
        }

        public static Boolean IsRoutePlannerTimeTableValid(
            RouteTimeTable timeTable,
            FleetAirliner airliner,
            List<RouteTimeTableEntry> entries,
            Boolean withSlots = true)
        {
            var tEntries = new List<RouteTimeTableEntry>();
            tEntries.AddRange(entries);
            tEntries.AddRange(timeTable.Entries);

            foreach (RouteTimeTableEntry e in timeTable.Entries)
            {
                var cEntries = new List<RouteTimeTableEntry>(tEntries);
                cEntries.Remove(e);

                if (!IsRouteEntryValid(e, airliner, cEntries, withSlots))
                {
                    return false;
                }
            }
            return true;
        }

        public static Boolean IsTimeTableValid(
            RouteTimeTable timeTable,
            FleetAirliner airliner,
            List<RouteTimeTableEntry> entries,
            Boolean withSlots = true)
        {
            foreach (RouteTimeTableEntry e in timeTable.Entries)
            {
                if (!IsRouteEntryValid(e, airliner, entries, withSlots))
                {
                    return false;
                }
            }
            return true;
        }

        #endregion

        //checks if an entry is valid

        #region Methods

        private static RouteTimeTableEntry GetNextEntry(
            RouteTimeTableEntry entry,
            FleetAirliner airliner,
            List<RouteTimeTableEntry> entries)
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
                    {
                        return dEntry;
                    }
                }
                day++;

                if (day == (DayOfWeek)7)
                {
                    day = 0;
                }

                counter++;
            }

            return null;
        }

        private static RouteTimeTableEntry GetPreviousEntry(
            RouteTimeTableEntry entry,
            FleetAirliner airliner,
            List<RouteTimeTableEntry> entries)
        {
            var tsEntry = new TimeSpan((int)entry.Day, entry.Time.Hours, entry.Time.Minutes, entry.Time.Seconds);
            DayOfWeek eDay = entry.Day;

            int counter = 0;

            while (counter < 7)
            {
                List<RouteTimeTableEntry> tEntries = entries;

                tEntries = (from e in tEntries orderby e.Time descending select e).ToList();

                foreach (RouteTimeTableEntry dEntry in tEntries)
                {
                    var ts = new TimeSpan((int)eDay, dEntry.Time.Hours, dEntry.Time.Minutes, dEntry.Time.Seconds);
                    if (ts < tsEntry)
                    {
                        return dEntry;
                    }
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

        private static Boolean IsRouteEntryValid(
            RouteTimeTableEntry entry,
            FleetAirliner airliner,
            List<RouteTimeTableEntry> entries,
            Boolean withSlots)
        {
            TimeSpan flightTime =
                entry.TimeTable.Route.getFlightTime(airliner.Airliner.Type)
                    .Add(FleetAirlinerHelpers.GetMinTimeBetweenFlights(airliner));

            var startTime = new TimeSpan((int)entry.Day, entry.Time.Hours, entry.Time.Minutes, entry.Time.Seconds);

            TimeSpan endTime = startTime.Add(flightTime);
            if (endTime.Days == 7)
            {
                endTime = new TimeSpan(0, endTime.Hours, endTime.Minutes, endTime.Seconds);
            }

            var airlinerEntries = new List<RouteTimeTableEntry>(entries);

            if (entry.TimeTable.Route.Season == Weather.Season.Winter)
            {
                airlinerEntries.RemoveAll(e => e.TimeTable.Route.Season == Weather.Season.Summer);
            }

            if (entry.TimeTable.Route.Season == Weather.Season.Summer)
            {
                airlinerEntries.RemoveAll(e => e.TimeTable.Route.Season == Weather.Season.Winter);
            }

            if (withSlots)
            {
                if (IsRouteEntryInOccupied(entry, airliner))
                {
                    return false;
                }
            }

            // airlinerEntries.AddRange(entry.TimeTable.Entries.FindAll(e => e.Destination.Airport == entry.Destination.Airport));

            foreach (RouteTimeTableEntry e in airlinerEntries)
            {
                var eStartTime = new TimeSpan((int)e.Day, e.Time.Hours, e.Time.Minutes, e.Time.Seconds);

                TimeSpan eFlightTime =
                    e.TimeTable.Route.getFlightTime(airliner.Airliner.Type)
                        .Add(FleetAirlinerHelpers.GetMinTimeBetweenFlights(airliner));

                TimeSpan eEndTime = eStartTime.Add(eFlightTime);

                double diffStartTime = Math.Abs(eStartTime.Subtract(startTime).TotalMinutes);
                double diffEndTime = Math.Abs(eEndTime.Subtract(endTime).TotalMinutes);

                if (eEndTime.Days == 7)
                {
                    eEndTime = new TimeSpan(0, eEndTime.Hours, eEndTime.Minutes, eEndTime.Seconds);
                }

                if ((eStartTime >= startTime && endTime >= eStartTime) || (eEndTime >= startTime && endTime >= eEndTime)
                    || (endTime >= eStartTime && eEndTime >= endTime)
                    || (startTime >= eStartTime && eEndTime >= startTime))
                {
                    if (e.Airliner == airliner || diffEndTime < 15 || diffStartTime < 15)
                    {
                        return false;
                    }
                }
            }
            double minutesPerWeek = 7 * 24 * 60;

            RouteTimeTableEntry nextEntry = GetNextEntry(entry, airliner, airlinerEntries);

            RouteTimeTableEntry previousEntry = GetPreviousEntry(entry, airliner, airlinerEntries);

            if (nextEntry != null && previousEntry != null)
            {
                TimeSpan flightTimeNext =
                    MathHelpers.GetFlightTime(
                        entry.Destination.Airport.Profile.Coordinates.convertToGeoCoordinate(),
                        nextEntry.DepartureAirport.Profile.Coordinates.convertToGeoCoordinate(),
                        airliner.Airliner.Type.CruisingSpeed)
                        .Add(FleetAirlinerHelpers.GetMinTimeBetweenFlights(airliner));
                TimeSpan flightTimePrevious =
                    MathHelpers.GetFlightTime(
                        entry.DepartureAirport.Profile.Coordinates.convertToGeoCoordinate(),
                        previousEntry.Destination.Airport.Profile.Coordinates.convertToGeoCoordinate(),
                        airliner.Airliner.Type.CruisingSpeed)
                        .Add(FleetAirlinerHelpers.GetMinTimeBetweenFlights(airliner));

                var prevDate = new TimeSpan(
                    (int)previousEntry.Day,
                    previousEntry.Time.Hours,
                    previousEntry.Time.Minutes,
                    previousEntry.Time.Seconds);
                var nextDate = new TimeSpan(
                    (int)nextEntry.Day,
                    nextEntry.Time.Hours,
                    nextEntry.Time.Minutes,
                    nextEntry.Time.Seconds);
                var currentDate = new TimeSpan((int)entry.Day, entry.Time.Hours, entry.Time.Minutes, entry.Time.Seconds);

                double timeToNext = currentDate.Subtract(nextDate).TotalMinutes > 0
                    ? minutesPerWeek - currentDate.Subtract(nextDate).TotalMinutes
                    : Math.Abs(currentDate.Subtract(nextDate).TotalMinutes);
                double timeFromPrev = prevDate.Subtract(currentDate).TotalMinutes > 0
                    ? minutesPerWeek - prevDate.Subtract(currentDate).TotalMinutes
                    : Math.Abs(prevDate.Subtract(currentDate).TotalMinutes);

                if (timeFromPrev
                    > previousEntry.TimeTable.Route.getFlightTime(airliner.Airliner.Type).TotalMinutes
                    + flightTimePrevious.TotalMinutes
                    && timeToNext
                    > entry.TimeTable.Route.getFlightTime(airliner.Airliner.Type).TotalMinutes
                    + flightTimeNext.TotalMinutes)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        #endregion
    }
}