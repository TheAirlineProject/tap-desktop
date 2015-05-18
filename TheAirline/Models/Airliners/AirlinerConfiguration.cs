using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TheAirline.Infrastructure;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Models.Airliners
{
    //the configuration of an airliner 
    [Serializable]
    public class AirlinerConfiguration : Configuration
    {
        #region Constructors and Destructors

        public AirlinerConfiguration(string name, int minimumSeats, Boolean standard)
            : base(ConfigurationType.Airliner, name, standard)
        {
            MinimumSeats = minimumSeats;
            Classes = new List<AirlinerClassConfiguration>();
        }

        //returns the number of classes

        private AirlinerConfiguration(SerializationInfo info, StreamingContext ctxt)
            : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("classes")]
        public List<AirlinerClassConfiguration> Classes { get; set; }

        [Versioning("minseats")]
        public int MinimumSeats { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        public void AddClassConfiguration(AirlinerClassConfiguration conf)
        {
            Classes.Add(conf);
        }

        public int GetNumberOfClasses()
        {
            return Classes.Count;
        }

        #endregion
    }

    //the configuration of an airliner class
    [Serializable]
    public class AirlinerClassConfiguration : BaseModel
    {
        #region Constructors and Destructors

        public AirlinerClassConfiguration(AirlinerClass.ClassType type, int seating, int regularseating)
        {
            SeatingCapacity = seating;
            RegularSeatingCapacity = regularseating;
            Type = type;
            Facilities = new List<AirlinerFacility>();
        }

        private AirlinerClassConfiguration(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("facilities")]
        public List<AirlinerFacility> Facilities { get; set; }

        [Versioning("regularseating")]
        public int RegularSeatingCapacity { get; set; }

        [Versioning("seatingcapacity")]
        public int SeatingCapacity { get; set; }

        [Versioning("type")]
        public AirlinerClass.ClassType Type { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        //adds a facility to the configuration
        public void AddFacility(AirlinerFacility facility)
        {
            Facilities.Add(facility);
        }

        //returns all facilities
        public List<AirlinerFacility> GetFacilities()
        {
            return Facilities;
        }

        //returns the facility of a specific type
        public AirlinerFacility GetFacility(AirlinerFacility.FacilityType type)
        {
            return Facilities.Find(f => f.Type == type);
        }

        #endregion
    }
}