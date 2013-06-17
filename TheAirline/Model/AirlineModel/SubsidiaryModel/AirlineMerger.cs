
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheAirline.Model.AirlineModel.SubsidiaryModel
{
    [Serializable]
    //the class for a merger between two airlines either as a regular merger or where one of them gets subsidiary of the other
    public class AirlineMerger
    {
        public enum MergerType { Merger, Subsidiary }
        
        public MergerType Type { get; set; }
        
        public Airline Airline1 { get; set; }
        
        public Airline Airline2 { get; set; }
        
        public DateTime Date { get; set; }
        
        public string NewName { get; set; }
        
        public string Name { get; set; }
        public AirlineMerger(string name, Airline airline1, Airline airline2, DateTime date, MergerType type)
        {
            this.Name = name;
            this.Airline1 = airline1;
            this.Airline2 = airline2;
            this.Date = date;
            this.Type = type;
        }
    }
    //the list of airline mergers 
    public class AirlineMergers
    {
        private static List<AirlineMerger> mergers = new List<AirlineMerger>();
        //adds a merger to the list of mergers
        public static void AddAirlineMerger(AirlineMerger merger)
        {
            mergers.Add(merger);
        }
        //returns all mergers
        public static List<AirlineMerger> GetAirlineMergers()
        {
            return mergers;
        }
        //returns all mergers for a specific date
        public static List<AirlineMerger> GetAirlineMergers(DateTime date)
        {
            return mergers.FindAll(m => m.Date.ToShortDateString() == date.ToShortDateString());
        }
        //removes a merger from the list
        public static void RemoveAirlineMerger(AirlineMerger merger)
        {
            mergers.Remove(merger);
        }
        //clears the list of mergers
        public static void Clear()
        {
            mergers.Clear();
        }
    }
}
