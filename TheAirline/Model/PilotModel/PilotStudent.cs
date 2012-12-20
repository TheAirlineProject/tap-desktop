using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.PilotModel
{
    //the class for a pilot student
    public class PilotStudent
    {
        public PilotProfile Profile { get; set; }
        public DateTime StartDate { get; set; }
        public PilotStudent(PilotProfile profile, DateTime startDate)
        {
            this.Profile = profile;
            this.StartDate = startDate;
        }
    }
}
