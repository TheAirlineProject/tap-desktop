using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.Model.PilotModel
{
    //the class for the aircraft for training for students
    [Serializable]
    public class TrainingAircraftType : BaseModel
    {
        #region Constructors and Destructors

        public TrainingAircraftType(string name, double price, int maxnumberofstudents, int typelevel)
        {
            Name = name;
            APrice = price;
            TypeLevel = typelevel;
            MaxNumberOfStudents = maxnumberofstudents;
        }

        private TrainingAircraftType(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("price")]
        public double APrice { get; set; }

        [Versioning("maxstudents")]
        public int MaxNumberOfStudents { get; set; }

        [Versioning("name")]
        public string Name { get; set; }

        public double Price
        {
            get { return GeneralHelpers.GetInflationPrice(APrice); }
        }

        [Versioning("level")]
        public int TypeLevel { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion
    }

    //the list of training aircrafts types
    public class TrainingAircraftTypes
    {
        #region Static Fields

        private static readonly List<TrainingAircraftType> Types = new List<TrainingAircraftType>();

        #endregion

        #region Public Methods and Operators

        public static void AddAircraftType(TrainingAircraftType type)
        {
            Types.Add(type);
        }

        //returns all aircraft types

        //clears the list of aircrafts 
        public static void Clear()
        {
            Types.Clear();
        }

        public static TrainingAircraftType GetAircraftType(string name)
        {
            return Types.Find(t => t.Name == name);
        }

        public static List<TrainingAircraftType> GetAircraftTypes()
        {
            return Types;
        }

        #endregion

        //adds a type to the list
    }
}