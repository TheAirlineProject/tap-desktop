using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.PilotModel
{
    //the class for a flight school
    public class FlightSchool
    {
        public string Name { get; set; }
        public int NumberOfInstructors { get { return this.Instructors.Count; } set { ;} }
        public int NumberOfStudents { get { return this.Students.Count; } set { ;} }
        public List<PilotStudent> Students { get; set; }
        public List<Instructor> Instructors { get; set; }
        public FlightSchool(string name)
        {
            this.Name = name;
            this.Students = new List<PilotStudent>();
            this.Instructors = new List<Instructor>();
        }
        //adds an instructor to the flight school
        public void addInstructor(Instructor instructor)
        {
            this.Instructors.Add(instructor);
        }
        //adds a student to the flight school
        public void addStudent(PilotStudent student)
        {
            this.Students.Add(student);
        }
    }
}
