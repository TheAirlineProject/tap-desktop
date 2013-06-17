using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.AirlineModel
{
    [ProtoContract]
    //the member of an alliance
    public class AllianceMember
    {
        [ProtoMember(1,AsReference=true)]
        public Airline Airline { get; set; }
        [ProtoMember(2)]
        public DateTime JoinedDate { get; set; }
        [ProtoMember(3)]
        public DateTime ExitedDate { get; set; }
        public AllianceMember(Airline airline, DateTime joinedDate)
        {
            this.Airline = airline;
            this.JoinedDate = joinedDate;
            this.ExitedDate = new DateTime(2199, 1, 1);
        }
    }
}
