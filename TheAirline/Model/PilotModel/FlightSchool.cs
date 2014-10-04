using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.PilotModel
{
    //the class for a flight school
    [Serializable]
    public class FlightSchool : BaseModel
    {
        #region Constants

        public const int MaxNumberOfInstructors = 15;

        public const int MaxNumberOfStudentsPerInstructor = 2;

        #endregion

        #region Constructors and Destructors

        public FlightSchool(Airport airport)
        {
            Guid id = Guid.NewGuid();

            Airport = airport;
            Name = string.Format("Flight School {0}", Airport.Profile.Town.Name);
            Students = new List<PilotStudent>();
            Instructors = new List<Instructor>();
            TrainingAircrafts = new List<TrainingAircraft>();
            ID = id.ToString();
        }

        private FlightSchool(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
            Students.RemoveAll(s => s == null);
        }

        #endregion

        #region Public Properties

        [Versioning("airport")]
        public Airport Airport { get; set; }

        [Versioning("id")]
        public string ID { get; set; }

        [Versioning("instructors")]
        public List<Instructor> Instructors { get; set; }

        [Versioning("name")]
        public string Name { get; set; }

        public int NumberOfInstructors
        {
            get { return Instructors.Count; }
        }

        public int NumberOfStudents
        {
            get { return Students.Count; }
        }

        [Versioning("students")]
        public List<PilotStudent> Students { get; set; }

        [Versioning("aircrafts")]
        public List<TrainingAircraft> TrainingAircrafts { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        //adds an instructor to the flight school
        public void AddInstructor(Instructor instructor)
        {
            Instructors.Add(instructor);
        }

        //removes an instructor from the flight school

        //adds a student to the flight school
        public void AddStudent(PilotStudent student)
        {
            Students.Add(student);
        }

        public void AddTrainingAircraft(TrainingAircraft aircraft)
        {
            TrainingAircrafts.Add(aircraft);
        }

        public void RemoveInstructor(Instructor instructor)
        {
            Instructors.Remove(instructor);
        }

        //removes a student from the flight school
        public void RemoveStudent(PilotStudent student)
        {
            Students.Remove(student);
        }

        public void RemoveTrainingAircraft(TrainingAircraft aircraft)
        {
            TrainingAircrafts.Remove(aircraft);
        }

        #endregion
    }
}