using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.PilotModel
{
    //the class for a flight school instructor
    public class Instructor
    {
        public PilotProfile Profile { get; set; }
        public Pilot.PilotRating Rating { get; set; }
        public FlightSchool FlightSchool { get; set; }
        public Instructor(PilotProfile profile, Pilot.PilotRating rating)
        {
            this.Profile = profile;
            this.Rating = rating;
        }
    }
    //the list of flight school instructors
    public class Instructors
    {
        private static List<Instructor> instructors = new List<Instructor>();
        //adds an instructor to the list
        public static void AddInstructor(Instructor instructor)
        {
            instructors.Add(instructor);
        }
        //returns all instructors
        public static List<Instructor> GetInstructors()
        {
            return instructors;
        }
        //returns all instructors not assigned to a flight school
        public static List<Instructor> GetUnassignedInstructors()
        {
            return instructors.FindAll(i => i.FlightSchool == null);
        }
        //clears the list of instructors
        public static void Clear()
        {
            instructors.Clear();
        }
    }
}
