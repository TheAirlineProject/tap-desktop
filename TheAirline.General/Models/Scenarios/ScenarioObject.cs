using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TheAirline.General.Models;
using TheAirline.Infrastructure;

namespace TheAirline.Models.General.Scenarios
{
    //the actually scenario used in a game
    [Serializable]
    public class ScenarioObject : BaseModel
    {
        #region Fields

        [Versioning("failures")] private readonly List<ScenarioFailureObject> _failures;

        #endregion

        #region Constructors and Destructors

        public ScenarioObject(Scenario scenario)
        {
            Scenario = scenario;
            IsSuccess = false;
            _failures = new List<ScenarioFailureObject>();

            foreach (ScenarioFailure failure in Scenario.Failures)
            {
                _failures.Add(new ScenarioFailureObject(failure));
            }
        }

        //returns a scenario failure object

        private ScenarioObject(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("success")]
        public Boolean IsSuccess { get; set; }

        [Versioning("scenario")]
        public Scenario Scenario { get; set; }

        [Versioning("scenariofailed")]
        public ScenarioFailure ScenarioFailed { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        public ScenarioFailureObject GetScenarioFailure(ScenarioFailure failure)
        {
            return _failures.Find(f => f.Failure == failure);
        }

        //returns all scenario failure objects
        public List<ScenarioFailureObject> GetScenarioFailures()
        {
            return _failures;
        }

        #endregion
    }

    [Serializable]
    //the object for a scenario failure
    public class ScenarioFailureObject : BaseModel
    {
        #region Constructors and Destructors

        public ScenarioFailureObject(ScenarioFailure failure)
        {
            Failure = failure;
            Failures = 0;
        }

        private ScenarioFailureObject(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("failure")]
        public ScenarioFailure Failure { get; set; }

        [Versioning("failures")]
        public int Failures { get; set; }

        [Versioning("lastfailure")]
        public DateTime LastFailureTime { get; set; }

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