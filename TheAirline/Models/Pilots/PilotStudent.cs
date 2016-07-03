using System;
using System.Runtime.Serialization;
using TheAirline.Helpers;
using TheAirline.Infrastructure;

namespace TheAirline.Models.Pilots
{
    //the class for a pilot student
    [Serializable]
    public class PilotStudent : BaseModel
    {
        #region Constants

        public const double StudentCost = 33381.69;

        #endregion

        #region Constructors and Destructors

        public PilotStudent(
            PilotProfile profile,
            DateTime startDate,
            Instructor instructor,
            PilotRating rating,
            string airlinerfamily)
        {
            Rating = rating;
            Profile = profile;
            StartDate = startDate;
            EndDate = StartDate.AddDays(Rating.TrainingDays);
            Instructor = instructor;
            AirlinerFamily = airlinerfamily;
        }

        private PilotStudent(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
            if (Version == 1)
            {
                Rating = GeneralHelpers.GetPilotStudentRating(this);
            }
            if (Version < 3)
            {
                Aircraft = null;
                AirlinerFamily = "";
            }
        }

        #endregion

        #region Public Properties

        [Versioning("aircraft", Version = 3)]
        public TrainingAircraft Aircraft { get; set; }

        [Versioning("airlinerfamily", Version = 3)]
        public string AirlinerFamily { get; set; }

        [Versioning("enddate")]
        public DateTime EndDate { get; set; }

        [Versioning("instructor")]
        public Instructor Instructor { get; set; }

        [Versioning("profile")]
        public PilotProfile Profile { get; set; }

        [Versioning("rating", Version = 2)]
        public PilotRating Rating { get; set; }

        [Versioning("startdate")]
        public DateTime StartDate { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 3);

            base.GetObjectData(info, context);
        }

        #endregion
    }
}