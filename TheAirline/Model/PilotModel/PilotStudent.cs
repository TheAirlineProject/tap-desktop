
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.PilotModel
{
    //the class for a pilot student
    [Serializable]
    public class PilotStudent
    {
        
        public PilotProfile Profile { get; set; }
        
        public DateTime StartDate { get; set; }
        
        public DateTime EndDate { get; set; }
        
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
