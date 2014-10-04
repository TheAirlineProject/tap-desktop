using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.Model.PilotModel
{
    //the class for a flight school instructor
    [Serializable]
    public class Instructor : BaseModel
    {
        #region Constructors and Destructors

        public Instructor(PilotProfile profile, PilotRating rating)
        {
            Profile = profile;
            Rating = rating;
            Students = new List<PilotStudent>();
        }

        private Instructor(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
            if (Version == 1)
            {
                Rating = GeneralHelpers.GetPilotRating();
            }
        }

        #endregion

        #region Public Properties

        [Versioning("flightschool")]
        public FlightSchool FlightSchool { get; set; }

        [Versioning("profile")]
        public PilotProfile Profile { get; set; }

        [Versioning("rating")]
        public PilotRating Rating { get; set; }

        [Versioning("students")]
        public List<PilotStudent> Students { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 2);

            base.GetObjectData(info, context);
        }

        //adds a student to the instructor
        public void AddStudent(PilotStudent student)
        {
            Students.Add(student);
        }

        //removes a students from the instructor
        public void RemoveStudent(PilotStudent student)
        {
            Students.Remove(student);
        }

        #endregion
    }

    //the list of flight school instructors
    public class Instructors
    {
        #region Static Fields

        private static readonly List<Instructor> instructors = new List<Instructor>();

        #endregion

        #region Public Methods and Operators

        public static void AddInstructor(Instructor instructor)
        {
            instructors.Add(instructor);
        }

        public static void Clear()
        {
            instructors.Clear();
        }

        //returns all instructors
        public static List<Instructor> GetInstructors()
        {
            return instructors;
        }

        public static int GetNumberOfUnassignedInstructors()
        {
            return instructors.FindAll(i => i.FlightSchool == null).Count;
        }

        //returns all instructors not assigned to a flight school
        public static List<Instructor> GetUnassignedInstructors()
        {
            return instructors.FindAll(i => i.FlightSchool == null);
        }

        //clears the list of instructors

        //removes an instructor from the list
        public static void RemoveInstructor(Instructor instructor)
        {
            instructors.Remove(instructor);
        }

        #endregion

        //adds an instructor to the list

        //counts the number of unassigned instructors
    }
}