using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace TheAirline.Model.GeneralModel.HistoricEventModel
{
    //the class for a historic event
    [Serializable]
    public class HistoricEvent : BaseModel
    {
        #region Constructors and Destructors

        public HistoricEvent(string name, string text, DateTime date)
        {
            Name = name;
            Text = text;
            Date = date;
            Influences = new List<HistoricEventInfluence>();
        }

        //adds an influence to the event

        private HistoricEvent(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("date")]
        public DateTime Date { get; set; }

        [Versioning("influences")]
        public List<HistoricEventInfluence> Influences { get; set; }

        [Versioning("name")]
        public string Name { get; set; }

        [Versioning("text")]
        public string Text { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        public void AddInfluence(HistoricEventInfluence influence)
        {
            Influences.Add(influence);
        }

        #endregion
    }

    //the list of historic events
    public class HistoricEvents
    {
        #region Static Fields

        private static readonly List<HistoricEvent> Events = new List<HistoricEvent>();

        #endregion

        #region Public Methods and Operators

        public static void AddHistoricEvent(HistoricEvent e)
        {
            Events.Add(e);
        }

        public static void Clear()
        {
            Events.Clear();
        }

        public static List<HistoricEventInfluence> GetHistoricEventInfluences(DateTime date)
        {
            return
                Events.Where(e => e.Date >= GameObject.GetInstance().StartDate)
                      .SelectMany(
                          e => e.Influences.FindAll(i => i.EndDate.ToShortDateString() == date.ToShortDateString()))
                      .ToList();
        }

        //returns all historic events 
        public static List<HistoricEvent> GetHistoricEvents()
        {
            return Events;
        }

        //returns all historic events for a date
        public static List<HistoricEvent> GetHistoricEvents(DateTime date)
        {
            return Events.FindAll(e => e.Date.ToShortDateString() == date.ToShortDateString());
        }

        #endregion

        //adds an event to the list

        //returns all historic events with influences ending at a date
    }
}