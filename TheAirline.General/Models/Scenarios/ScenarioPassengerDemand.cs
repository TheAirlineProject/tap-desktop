using System;
using System.Runtime.Serialization;
using TheAirline.General.Models;
using TheAirline.Infrastructure;
using TheAirline.Models.Airports;
using TheAirline.Models.General.Countries;

namespace TheAirline.Models.General.Scenarios
{
    //the class for passenger demand at a scenario
    [Serializable]
    public class ScenarioPassengerDemand : BaseModel
    {
        #region Constructors and Destructors

        public ScenarioPassengerDemand(double factor, DateTime enddate, Country country, Airport airport)
        {
            Country = country;
            Factor = factor;
            EndDate = enddate;
            Airport = airport;
        }

        private ScenarioPassengerDemand(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("airport")]
        public Airport Airport { get; set; }

        [Versioning("country")]
        public Country Country { get; set; }

        [Versioning("enddate")]
        public DateTime EndDate { get; set; }

        [Versioning("factor")]
        public double Factor { get; set; }

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