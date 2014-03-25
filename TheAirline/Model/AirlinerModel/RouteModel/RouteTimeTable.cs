
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Device.Location;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using System.Runtime.Serialization;
using System.Reflection;
using TheAirline.Model.GeneralModel.WeatherModel;

namespace TheAirline.Model.AirlinerModel.RouteModel
{
    /*! RouteTimeTable
* This class is used for the time table for a route
* The class needs parameters for the route
*/
    [Serializable]
    public class RouteTimeTable : ISerializable
    {
        [Versioning("entries")]
        public List<RouteTimeTableEntry> Entries { get; set; }
        [Versioning("route")]
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
        //adds entries for a specific destination and time for each day of the week assigned to an airliner
        public void addDailyEntries(RouteEntryDestination destination, TimeSpan time, FleetAirliner airliner)
        {
            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                RouteTimeTableEntry entry = new RouteTimeTableEntry(this, day, time, destination);
                entry.Airliner = airliner;

                this.Entries.Add(entry);
            }
        }
      
        //adds entries for a specific destination and time for each day of the week
        public void addDailyEntries(RouteEntryDestination destination, TimeSpan time)
        {
            addDailyEntries(destination, time, null);
        }
        //adds entries for a specific destination and for each weekday of the week assinged to an airliner
        public void addWeekDailyEntries(RouteEntryDestination destination, TimeSpan time)
        {
            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                if (day != DayOfWeek.Saturday && day != DayOfWeek.Sunday)
                {
                    RouteTimeTableEntry entry = new RouteTimeTableEntry(this, day, time, destination);
                    entry.Airliner = null;

                    this.Entries.Add(entry);
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
        public List<RouteTimeTableEntry> getEntries(Airport destination)
        {
        
            return this.Entries.FindAll(e=>e.Destination.Airport == destination);
        }
        //returns all entries for a specific day
        public List<RouteTimeTableEntry> getEntries(DayOfWeek day, Boolean useSeason = true)
        {
            Weather.Season season = GeneralHelpers.GetSeason(GameObject.GetInstance().GameTime);

            if (useSeason)
                return this.Entries.FindAll(e=>e.Day == day && (e.TimeTable.Route.Season == Weather.Season.All_Year || e.TimeTable.Route.Season == season));
            else
                return this.Entries.FindAll(e=>e.Day == day);
        }
        //returns a entry if possible in a specific timespan on a specific day of the week
        public RouteTimeTableEntry getEntry(DayOfWeek day, TimeSpan startTime, TimeSpan endTime)
        {
            return getEntries(day).Find(e=>e.Time >= startTime && e.Time <= endTime);
        }
        //returns the next entry after a specific date and with a specific airliner
        public RouteTimeTableEntry getNextEntry(DateTime time, FleetAirliner airliner) 
        {
           
            DateTime dt = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second);

            int counter = 0;
            while (counter < 8)
            {
                
                List<RouteTimeTableEntry> entries = getEntries(dt.DayOfWeek).FindAll(e=>airliner == e.Airliner);

                foreach (RouteTimeTableEntry dEntry in (from e in entries orderby e.Time select e))
                {
                    if (!(dEntry.Time <= dt.TimeOfDay && dt.Day == time.Day))
                        return dEntry;
                }
                dt = dt.AddDays(1);

                counter++;
            }

            RouteTimeTableEntry entry = this.Entries.Find(e => e.Airliner == airliner);
            return entry;

        }
    
        //returns the next entry after a specific date and not to a specific coordinates (airport)
        public RouteTimeTableEntry getNextEntry(DateTime time, Airport airport)
        {
          
            DayOfWeek day = time.DayOfWeek;

            int counter = 0;

            while (counter < 8)
            {

                List<RouteTimeTableEntry> entries = getEntries(day);

                foreach (RouteTimeTableEntry dEntry in (from e in entries orderby e.Time select e))
                {
                    if (!((dEntry.Day == time.DayOfWeek && dEntry.Time <= time.TimeOfDay)) && dEntry.Destination.Airport != airport)
                        return dEntry;
                }
                day++;

                if (day == (DayOfWeek)7)
                    day = (DayOfWeek)0;

                counter++;
            }

            return null;

        }

        //returns the next entry from a specific time
        public RouteTimeTableEntry getNextEntry(DateTime time)
        {
          
            DayOfWeek day = time.DayOfWeek;

            int counter = 0;

            while (counter < 7)
            {



                List<RouteTimeTableEntry> entries = getEntries(day);

                foreach (RouteTimeTableEntry dEntry in (from e in entries orderby e.Time select e))
                {
                    if (!((dEntry.Day == time.DayOfWeek && dEntry.Time <= time.TimeOfDay)))
                        return dEntry;
                }
                day++;

                if (day == (DayOfWeek)7)
                    day = (DayOfWeek)0;

                counter++;
            }

            return null;

        }
        //returns the next entry after a specific entry
        public RouteTimeTableEntry getNextEntry(RouteTimeTableEntry entry)
        {
         
            DayOfWeek eDay = entry.Day;

            int counter = 0;

            while (counter < 7)
            {


                List<RouteTimeTableEntry> entries = getEntries(eDay);

                foreach (RouteTimeTableEntry dEntry in (from e in entries orderby e.Time select e))
                {
                    if (!((dEntry.Day == entry.Day && dEntry.Time <= entry.Time)))// && dEntry.Destination != entry.Destination)
                        return dEntry;
                }
                counter++;

                eDay++;

                if (eDay == (DayOfWeek)7)
                    eDay = (DayOfWeek)0;

            }

            return entry;

        }
           private RouteTimeTable(SerializationInfo info, StreamingContext ctxt)
        {
            int version = info.GetInt16("version");

            var fields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null);

            IList<PropertyInfo> props = new List<PropertyInfo>(this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null));

            var propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

            foreach (SerializationEntry entry in info)
            {
                MemberInfo prop = propsAndFields.FirstOrDefault(p => ((Versioning)p.GetCustomAttribute(typeof(Versioning))).Name == entry.Name);


                if (prop != null)
                {
                    if (prop is FieldInfo)
                        ((FieldInfo)prop).SetValue(this, entry.Value);
                    else
                        ((PropertyInfo)prop).SetValue(this, entry.Value);
                }
            }

            var notSetProps = propsAndFields.Where(p => ((Versioning)p.GetCustomAttribute(typeof(Versioning))).Version > version);

            foreach (MemberInfo notSet in notSetProps)
            {
                Versioning ver = (Versioning)notSet.GetCustomAttribute(typeof(Versioning));

                if (ver.AutoGenerated)
                {
                    if (notSet is FieldInfo)
                        ((FieldInfo)notSet).SetValue(this, ver.DefaultValue);
                    else
                        ((PropertyInfo)notSet).SetValue(this, ver.DefaultValue);

                }

            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            Type myType = this.GetType();

            var fields = myType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null);

            IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null));

            var propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

            foreach (MemberInfo member in propsAndFields)
            {
                object propValue;

                if (member is FieldInfo)
                    propValue = ((FieldInfo)member).GetValue(this);
                else
                    propValue = ((PropertyInfo)member).GetValue(this, null);

                Versioning att = (Versioning)member.GetCustomAttribute(typeof(Versioning));

                info.AddValue(att.Name, propValue);
            }




        }

    }

}
