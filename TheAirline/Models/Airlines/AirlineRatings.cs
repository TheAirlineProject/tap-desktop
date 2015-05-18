using System;
using System.Runtime.Serialization;
using TheAirline.Infrastructure;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Models.Airlines
{
    //the class for the ratings for an airline
    [Serializable]
    public class AirlineRatings : BaseModel
    {
        #region Constructors and Destructors

        public AirlineRatings()
        {
            CustomerHappinessRating = 50;
            SafetyRating = 50;
            SecurityRating = 50;
            EmployeeHappinessRating = 50;
            MaintenanceRating = 50;
        }

        private AirlineRatings(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("customer")]
        public int CustomerHappinessRating { get; set; }

        [Versioning("employee")]
        public int EmployeeHappinessRating { get; set; }

        [Versioning("maintenance")]
        public int MaintenanceRating { get; set; }

        [Versioning("safety")]
        public int SafetyRating { get; set; }

        [Versioning("security")]
        public int SecurityRating { get; set; }

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