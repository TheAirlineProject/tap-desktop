using System;
using System.Runtime.Serialization;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirlineModel
{
    [Serializable]
    //the member of an alliance
    public class AllianceMember : BaseModel
    {
        #region Constructors and Destructors

        public AllianceMember(Airline airline, DateTime joinedDate)
        {
            Airline = airline;
            JoinedDate = joinedDate;
            ExitedDate = new DateTime(2199, 1, 1);
        }

        private AllianceMember(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("airline")]
        public Airline Airline { get; set; }

        [Versioning("existed")]
        public DateTime ExitedDate { get; set; }

        [Versioning("joined")]
        public DateTime JoinedDate { get; set; }

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