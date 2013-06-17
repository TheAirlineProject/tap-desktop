using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheAirline.Model.AirlineModel
{
    //the class for the ratings for an airline
    [ProtoContract]
    public class AirlineRatings
    {
        [ProtoMember(1)]
        public int CustomerHappinessRating { get; set; }
        [ProtoMember(2)]
        public int SafetyRating { get; set; }
        [ProtoMember(3)]
        public int SecurityRating { get; set; }
        [ProtoMember(4)]
        public int EmployeeHappinessRating { get; set; }
        [ProtoMember(5)]
        public int MaintenanceRating { get; set; }
        public AirlineRatings()
        {
            this.CustomerHappinessRating = 50;
            this.SafetyRating = 50;
            this.SecurityRating = 50;
            this.EmployeeHappinessRating = 50;
            this.MaintenanceRating = 50;
        }
    }
}
