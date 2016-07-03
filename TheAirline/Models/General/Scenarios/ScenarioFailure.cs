using System;
using System.Runtime.Serialization;
using TheAirline.Infrastructure;

namespace TheAirline.Models.General.Scenarios
{
    //the class for a failure at a scenario
    [Serializable]
    public class ScenarioFailure : BaseModel
    {
        #region Constructors and Destructors

        public ScenarioFailure(
            string id,
            FailureType type,
            int checkMonths,
            object value,
            string failureText,
            int monthsOfFailure)
        {
            ID = id;
            Type = type;
            CheckMonths = checkMonths;
            Value = value;
            FailureText = failureText;
            MonthsOfFailure = monthsOfFailure;
        }

        private ScenarioFailure(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Enums

        public enum FailureType
        {
            Cash,

            Safety,

            Debt,

            Security,

            Fleet,

            Domestic,

            Intl,

            PaxGrowth,

            Crime,

            FleetAge,

            Pax,

            Bases,

            JetRation
        }

        #endregion

        #region Public Properties

        [Versioning("checksmonths")]
        public int CheckMonths { get; set; }

        [Versioning("failuretext")]
        public string FailureText { get; set; }

        [Versioning("id")]
        public string ID { get; set; }

        [Versioning("monthsoffailure")]
        public int MonthsOfFailure { get; set; }

        [Versioning("type")]
        public FailureType Type { get; set; }

        [Versioning("value")]
        public object Value { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion

        //1 - means check each month
    }
}