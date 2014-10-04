using System;
using System.Runtime.Serialization;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.RouteModel
{
    /*! RouteTimeTableEntry.
 * This class is used for an entry in a time table
 * The class needs parameters for the time table, the day of flight, the time of flight and the destination
 */

    [Serializable]
    public class RouteTimeTableEntry : BaseModel, IComparable<RouteTimeTableEntry>
    {
        #region Constructors and Destructors

        public RouteTimeTableEntry(
            RouteTimeTable timeTable,
            DayOfWeek day,
            TimeSpan time,
            RouteEntryDestination destination)
            : this(timeTable, day, time, destination, null)
        {
        }

        public RouteTimeTableEntry(
            RouteTimeTable timeTable,
            DayOfWeek day,
            TimeSpan time,
            RouteEntryDestination destination,
            Gate outboundgate)
        {
            Guid id = Guid.NewGuid();

            Day = day;
            Time = time;
            TimeTable = timeTable;
            Destination = destination;
            ID = id.ToString();
            Gate = outboundgate;
        }

        private RouteTimeTableEntry(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("airliner")]
        public FleetAirliner Airliner { get; set; }

        [Versioning("day")]
        public DayOfWeek Day { get; set; }

        public Airport DepartureAirport
        {
            get { return GetDepartureAirport(); }
        }

        [Versioning("destination")]
        public RouteEntryDestination Destination { get; set; }

        [Versioning("gate")]
        public Gate Gate { get; set; }

        [Versioning("id")]
        public string ID { get; set; }

        [Versioning("mainentry")]
        public RouteTimeTableEntry MainEntry { get; set; }

        [Versioning("time")]
        public TimeSpan Time { get; set; }

        [Versioning("timetable")]
        public RouteTimeTable TimeTable { get; set; }

        #endregion

        #region Public Methods and Operators

        public int CompareTo(RouteTimeTableEntry entry)
        {
            int compare = entry.Day.CompareTo(Day);
            if (compare == 0)
            {
                return entry.Time.CompareTo(Time);
            }
            return compare;
        }

        //returns the timespan between two entries

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        public Airport GetDepartureAirport()
        {
            return Destination.Airport == TimeTable.Route.Destination1
                       ? TimeTable.Route.Destination2
                       : TimeTable.Route.Destination1;
        }

        public TimeSpan GetTimeDifference(RouteTimeTableEntry entry)
        {
            int daysBetween = Math.Abs(Day - entry.Day);

            TimeSpan time = entry.Time.Subtract(Time);

            return new TimeSpan(24*daysBetween, 0, 0).Add(time);
        }

        #endregion
    }
}