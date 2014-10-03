using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirlineModel
{
    //the class the for score for an airline
    [Serializable]
    public class AirlineScores : BaseModel
    {
        #region Constructors and Destructors

        public AirlineScores()
        {
            CHR = new List<int>();
            EHR = new List<int>();
            Maintenance = new List<int>();
            Safety = new List<int>();
            Security = new List<int>();
        }

        private AirlineScores(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("chr")]
        public List<int> CHR { get; set; }

        [Versioning("ehr")]
        public List<int> EHR { get; set; }

        [Versioning("maintenance")]
        public List<int> Maintenance { get; set; }

        [Versioning("safety")]
        public List<int> Safety { get; set; }

        [Versioning("security")]
        public List<int> Security { get; set; }

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