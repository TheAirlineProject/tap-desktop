using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.PilotModel
{
    //the class for a training aircraft
    [ProtoContract]
    public class TrainingAircraft
    {
        [ProtoMember(1)]
        public TrainingAircraftType Type { get; set; }
        [ProtoMember(2)]
        public FlightSchool FlightSchool { get; set; }
        [ProtoMember(3)]
        public DateTime BoughtDate { get; set; }
        public int Age { get { return MathHelpers.GetAge(this.BoughtDate);} private set { ;} }
        public TrainingAircraft(TrainingAircraftType type, DateTime boughtDate, FlightSchool flighschool)
        {
            this.Type = type;
            this.FlightSchool = flighschool;
            this.BoughtDate = boughtDate;
     
        }
    }
}
