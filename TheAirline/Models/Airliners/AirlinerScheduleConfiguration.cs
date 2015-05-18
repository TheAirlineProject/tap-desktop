using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TheAirline.Infrastructure;
using TheAirline.Model.GeneralModel;
using TheAirline.Models.Routes;

namespace TheAirline.Models.Airliners
{
    //the class for saving an airliner schedule
    [Serializable]
    public class AirlinerScheduleConfiguration : Configuration
    {
        public AirlinerScheduleConfiguration(string name, AirlinerType type) : base(ConfigurationType.AirlinerSchedule, name, false)
        {
            AirlinerType = type;
            Entries = new List<RouteTimeTableEntry>();
        }

        private AirlinerScheduleConfiguration(SerializationInfo info, StreamingContext ctxt)
            : base(info, ctxt)
        {
        }

        [Versioning("airlinertype")]
        public AirlinerType AirlinerType { get; set; }

        [Versioning("entries")]
        public List<RouteTimeTableEntry> Entries { get; set; }

        //adds an entry to the configuration
        public void AddEntry(RouteTimeTableEntry entry)
        {
            Entries.Add(entry);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }
    }
}