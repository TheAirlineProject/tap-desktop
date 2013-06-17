using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheAirline.Model.AirlineModel.SubsidiaryModel
{
    [ProtoContract]
    //the class for a merger between two airlines either as a regular merger or where one of them gets subsidiary of the other
    public class AirlineMerger
    {
        public enum MergerType { Merger, Subsidiary }
        [ProtoMember(1)]
        public MergerType Type { get; set; }
        [ProtoMember(2)]
        public Airline Airline1 { get; set; }
        [ProtoMember(3)]
        public Airline Airline2 { get; set; }
        [ProtoMember(4)]
        public DateTime Date { get; set; }
        [ProtoMember(5)]
        public string NewName { get; set; }
        [ProtoMember(6)]
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
