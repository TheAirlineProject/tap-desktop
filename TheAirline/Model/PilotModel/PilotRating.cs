using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.PilotModel
{
    //the class for the rating for a pilot
    [Serializable]
    public class PilotRating : BaseModel, IComparable<PilotRating>
    {
        #region Constructors and Destructors

        public PilotRating(string name, int trainingdays, int costindex)
        {
            Name = name;
            TrainingDays = trainingdays;
            CostIndex = costindex;
            Aircrafts = new List<TrainingAircraftType>();
        }

        //adds a training aircraft which the rating (pilot) uses in training

        private PilotRating(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("trainingaircrafts")]
        public List<TrainingAircraftType> Aircrafts { get; set; }

        [Versioning("costindex")]
        public int CostIndex { get; set; }

        [Versioning("name")]
        public string Name { get; set; }

        [Versioning("trainingdays")]
        public int TrainingDays { get; set; }

        #endregion

        #region Public Methods and Operators

        public int CompareTo(PilotRating other)
        {
            return CostIndex.CompareTo(other.CostIndex);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        public void AddAircraft(TrainingAircraftType aircraft)
        {
            Aircrafts.Add(aircraft);
        }

        #endregion
    }

    //the list of pilot ratings
    public class PilotRatings
    {
        #region Static Fields

        private static List<PilotRating> _ratings = new List<PilotRating>();

        #endregion

        #region Public Methods and Operators

        public static void AddRating(PilotRating rating)
        {
            _ratings.Add(rating);
        }

        public static void Clear()
        {
            _ratings = new List<PilotRating>();
        }

        //returns the rating with a name
        public static PilotRating GetRating(string name)
        {
            return _ratings.FirstOrDefault(r => r.Name == name);
        }

        //returns the list of ratings 
        public static List<PilotRating> GetRatings()
        {
            return _ratings;
        }

        #endregion

        //adds a rating to the list

        //clears the pilot ratings
    }
}