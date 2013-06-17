
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirlinerModel.RouteModel
{
    /*! RouteTimeTableEntry.
 * This class is used for an entry in a time table
 * The class needs parameters for the time table, the day of flight, the time of flight and the destination
 */
    [Serializable]
    public class RouteTimeTableEntry : IComparable<RouteTimeTableEntry>
    {
        
        public DayOfWeek Day { get; set; }
        
        public TimeSpan Time { get; set; }
        
        public RouteTimeTable TimeTable { get; set; }
        
        public RouteEntryDestination Destination { get; set; }
        
        public FleetAirliner Airliner { get; set; }
        public Airport DepartureAirport { get { return getDepartureAirport(); } set { ;} }
        
        public RouteTimeTableEntry MainEntry { get; set; }
        
        public string ID { get; set; }
        public RouteTimeTableEntry(RouteTimeTable timeTable, DayOfWeek day, TimeSpan time, RouteEntryDestination destination)
        {
             Guid id = Guid.NewGuid();

            this.Day = day;
            this.Time = time;
            this.TimeTable = timeTable;
            this.Destination = destination;
            this.ID = id.ToString();
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
