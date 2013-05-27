using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirlinerModel
{
    //the configuration for a configuration for an airliner type
    [DataContract]
    [KnownType(typeof(AirlinerTypeConfiguration))]
     public class AirlinerTypeConfiguration : Configuration
    {
        [DataMember]
        public AirlinerType Airliner { get; set; }
        [DataMember]
        public Period<DateTime> Period { get; set; }
        [DataMember]
        public List<AirlinerClassConfiguration> Classes { get; set; }
        public AirlinerTypeConfiguration(string name, AirlinerType type, Period<DateTime> period, Boolean standard)
            : base(Configuration.ConfigurationType.AirlinerType, name, standard)
        {
            this.Airliner = type;
            this.Period = period;
            this.Classes = new List<AirlinerClassConfiguration>();
        }
        //returns the number of classes
        public int getNumberOfClasses()
        {
            return this.Classes.Count;
        }
        //adds an airliner class configuration to the configuration
        public void addClassConfiguration(AirlinerClassConfiguration conf)
        {
            this.Classes.Add(conf);
        }

    }
}
