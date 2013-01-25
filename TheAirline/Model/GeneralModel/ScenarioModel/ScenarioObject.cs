﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel.ScenarioModel
{
    //the actually scenario used in a game
    public class ScenarioObject
    {
        public Scenario Scenario { get; set; }
        private List<ScenarioFailureObject> Failures;
        public ScenarioObject(Scenario scenario)
        {
            this.Scenario = scenario;
            this.Failures = new List<ScenarioFailureObject>();

            foreach (ScenarioFailure failure in this.Scenario.Failures)
                this.Failures.Add(new ScenarioFailureObject(failure));
        }
        //returns a scenario failure object
        public ScenarioFailureObject getScenarioFailure(ScenarioFailure failure)
        {
            return this.Failures.Find(f => f.Failure == failure);
        }
    }
    //the object for a scenario failure
    public class ScenarioFailureObject
    {
        public ScenarioFailure Failure { get; set; }
        public int Failures { get; set; }
        public DateTime LastFailureTime { get; set; }
        public ScenarioFailureObject(ScenarioFailure failure)
        {
            this.Failure = failure;
            this.Failures = 0;
        }
    }
}
