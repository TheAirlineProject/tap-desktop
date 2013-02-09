using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.AirlineModel
{
    //the member of an alliance
    public class AllianceMember
    {
        public Airline Airline { get; set; }
        public DateTime JoinedDate { get; set; }
        public DateTime ExitedDate { get; set; }
        public AllianceMember(Airline airline, DateTime joinedDate)
        {
            this.Airline = airline;
            this.JoinedDate = joinedDate;
        }
    }
}
