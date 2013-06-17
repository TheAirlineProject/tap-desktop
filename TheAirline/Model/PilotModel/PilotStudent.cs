using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.PilotModel
{
    //the class for a pilot student
    [ProtoContract]
    public class PilotStudent
    {
        [ProtoMember(1)]
        public PilotProfile Profile { get; set; }
        [ProtoMember(2)]
        public DateTime StartDate { get; set; }
        [ProtoMember(3)]
        public DateTime EndDate { get; set; }
        [ProtoMember(4)]
        public Instructor Instructor { get; set; }
        public const double StudentCost = 33381.69;
        public PilotStudent(PilotProfile profile, DateTime startDate, Instructor instructor)
        {
            this.Profile = profile;
            this.StartDate = startDate;
            this.EndDate = this.StartDate.AddDays(90);
            this.Instructor = instructor;
         }
    }
}
