using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel.ScenarioModel
{
    //the actually scenario used in a game
       [ProtoContract]
     public class ScenarioObject
    {
           [ProtoMember(1)]
           public Scenario Scenario { get; set; }
           [ProtoMember(2)]
           public ScenarioFailure ScenarioFailed { get; set; }
           [ProtoMember(3)]
           public Boolean IsSuccess { get; set; }
           [ProtoMember(4)]
           private List<ScenarioFailureObject> Failures;
        public ScenarioObject(Scenario scenario)
        {
            this.Scenario = scenario;
            this.IsSuccess = false;
            this.Failures = new List<ScenarioFailureObject>();

            foreach (ScenarioFailure failure in this.Scenario.Failures)
                this.Failures.Add(new ScenarioFailureObject(failure));
        }
        //returns a scenario failure object
        public ScenarioFailureObject getScenarioFailure(ScenarioFailure failure)
        {
            return this.Failures.Find(f => f.Failure == failure);
        }
        //returns all scenario failure objects
        public List<ScenarioFailureObject> getScenarioFailures()
        {
            return this.Failures;
        }
    }
    [ProtoContract]
    //the object for a scenario failure
    public class ScenarioFailureObject
    {
        [ProtoMember(1)]
        public ScenarioFailure Failure { get; set; }
        [ProtoMember(2)]
        public int Failures { get; set; }
        [ProtoMember(3)]
        public DateTime LastFailureTime { get; set; }
        public ScenarioFailureObject(ScenarioFailure failure)
        {
            this.Failure = failure;
            this.Failures = 0;
        }
    }
}
