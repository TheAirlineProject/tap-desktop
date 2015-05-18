using System;
using System.Runtime.Serialization;
using TheAirline.Infrastructure;

namespace TheAirline.Models.Pilots
{
    //the class for a pilot on training
    [Serializable]
    public class PilotTraining : BaseModel
    {
        #region Constructors and Destructors

        public PilotTraining(string airlinerfamily, DateTime enddate)
        {
            EndDate = enddate;
            AirlinerFamily = airlinerfamily;
        }

        private PilotTraining(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("airliner")]
        public string AirlinerFamily { get; set; }

        [Versioning("date")]
        public DateTime EndDate { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion
    }
}