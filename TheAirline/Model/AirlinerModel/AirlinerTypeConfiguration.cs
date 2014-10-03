using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirlinerModel
{
    //the configuration for a configuration for an airliner type
    [Serializable]
    public class AirlinerTypeConfiguration : Configuration
    {
        #region Constructors and Destructors

        public AirlinerTypeConfiguration(string name, AirlinerType type, Period<DateTime> period, Boolean standard)
            : base(ConfigurationType.AirlinerType, name, standard)
        {
            Airliner = type;
            Period = period;
            Classes = new List<AirlinerClassConfiguration>();
        }

        //returns the number of classes

        private AirlinerTypeConfiguration(SerializationInfo info, StreamingContext ctxt)
            : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("airliner")]
        public AirlinerType Airliner { get; set; }

        [Versioning("classes")]
        public List<AirlinerClassConfiguration> Classes { get; set; }

        [Versioning("period")]
        public Period<DateTime> Period { get; set; }

        #endregion

        #region Public Methods and Operators

        public new void GetObjectData(SerializationInfo info, StreamingContext context)
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
}