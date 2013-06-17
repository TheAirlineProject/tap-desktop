
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel.HistoricEventModel
{
    //the class for a historic event
    [Serializable]
    public class HistoricEvent
    {
          
        public string Name { get; set; }
          
          public DateTime Date { get; set; }
          
          public string Text { get; set; }
          
          public List<HistoricEventInfluence> Influences { get; set; }
        public HistoricEvent(string name,string text,DateTime date)
        {
            this.Name = name;
            this.Text = text;
            this.Date = date;
            this.Influences = new List<HistoricEventInfluence>();
        }
        //adds an influence to the event
        public void addInfluence(HistoricEventInfluence influence)
        {
            this.Influences.Add(influence);
        }
    }
    //the list of historic events
    public class HistoricEvents
    {
        private static List<HistoricEvent> events = new List<HistoricEvent>();
        //adds an event to the list
        public static void AddHistoricEvent(HistoricEvent e)
        {
            events.Add(e);
        }
        //returns all historic events 
        public static List<HistoricEvent> GetHistoricEvents()
        {
            return events;
        }
        //returns all historic events for a date
        public static List<HistoricEvent> GetHistoricEvents(DateTime date)
        {
            return events.FindAll(e => e.Date.ToShortDateString() == date.ToShortDateString());
        }
        //returns all historic events with influences ending at a date
        public static List<HistoricEventInfluence> GetHistoricEventInfluences(DateTime date)
        {
            return events.Where(e=>e.Date>=GameObject.GetInstance().StartDate).SelectMany(e => e.Influences.FindAll(i => i.EndDate.ToShortDateString() == date.ToShortDateString())).ToList();
         }
        //clears the list of historic events
        public static void Clear()
        {
            events.Clear();
        }
    }
}
