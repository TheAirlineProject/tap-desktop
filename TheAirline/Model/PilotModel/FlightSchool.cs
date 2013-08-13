
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirportModel;

namespace TheAirline.Model.PilotModel
{
    //the class for a flight school
    [Serializable]
    public class FlightSchool
    {
        public const int MaxNumberOfStudentsPerInstructor = 2;
        public const int MaxNumberOfInstructors = 15;
        
        public string Name { get; set; }
        public Airport Airport { get; set; }
        
        public string ID { get; set; }
        public int NumberOfInstructors { get { return this.Instructors.Count; } set { ;} }
        public int NumberOfStudents { get { return this.Students.Count; } set { ;} }
       
        public List<PilotStudent> Students { get; set; }
        
        public List<Instructor> Instructors { get; set; }
        
        public List<TrainingAircraft> TrainingAircrafts { get; set; }
        public FlightSchool(Airport airport)
        {
            Guid id = Guid.NewGuid();

            this.Airport = airport;
            this.Name = string.Format("Flight School {0}", this.Airport.Profile.Town.Name);
            this.Students = new List<PilotStudent>();
            this.Instructors = new List<Instructor>();
            this.TrainingAircrafts = new List<TrainingAircraft>();
            this.ID = id.ToString();
        }
        //adds a training aircraft to the flight school
        public void addTrainingAircraft(TrainingAircraft aircraft)
        {
            this.TrainingAircrafts.Add(aircraft);
        }
        //removes an aircraft from the flight school
        public void removeTrainingAircraft(TrainingAircraft aircraft)
        {
            this.TrainingAircrafts.Remove(aircraft);
        }
        //adds an instructor to the flight school
        public void addInstructor(Instructor instructor)
        {
            this.Instructors.Add(instructor);
        }
        //removes an instructor from the flight school
        public void removeInstructor(Instructor instructor)
        {
            this.Instructors.Remove(instructor);
        }
        //adds a student to the flight school
        public void addStudent(PilotStudent student)
        {
            this.Students.Add(student);
        }
        //removes a student from the flight school
        public void removeStudent(PilotStudent student)
        {
            this.Students.Remove(student);
        }
    }
}
