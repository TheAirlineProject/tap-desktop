using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TheAirline.Infrastructure;

namespace TheAirline.Models.Airlines.Subsidiary
{
    [Serializable]
    //the class for a merger between two airlines either as a regular merger or where one of them gets subsidiary of the other
    public class AirlineMerger : BaseModel
    {
        #region Constructors and Destructors

        public AirlineMerger(string name, Airline airline1, Airline airline2, DateTime date, MergerType type)
        {
            Name = name;
            Airline1 = airline1;
            Airline2 = airline2;
            Date = date;
            Type = type;
        }

        private AirlineMerger(SerializationInfo info, StreamingContext ctxt) : base(info,ctxt)
        {
            
        }

        #endregion

        #region Enums

        public enum MergerType
        {
            Merger,

            Subsidiary
        }

        #endregion

        #region Public Properties

        public Airline Airline1 { get; set; }

        public Airline Airline2 { get; set; }

        public DateTime Date { get; set; }

        public string Name { get; set; }

        public string NewName { get; set; }

        public MergerType Type { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info,context);
        }

        #endregion
    }

    //the list of airline mergers 
    public class AirlineMergers
    {
        #region Static Fields

        private static readonly List<AirlineMerger> Mergers = new List<AirlineMerger>();

        #endregion

        #region Public Methods and Operators

        public static void AddAirlineMerger(AirlineMerger merger)
        {
            Mergers.Add(merger);
        }

        public static void Clear()
        {
            Mergers.Clear();
        }

        //returns all mergers
        public static List<AirlineMerger> GetAirlineMergers()
        {
            return Mergers;
        }

        //returns all mergers for a specific date
        public static List<AirlineMerger> GetAirlineMergers(DateTime date)
        {
            return Mergers.FindAll(m => m.Date.ToShortDateString() == date.ToShortDateString());
        }

        //removes a merger from the list
        public static void RemoveAirlineMerger(AirlineMerger merger)
        {
            Mergers.Remove(merger);
        }

        #endregion

        //adds a merger to the list of mergers

        //clears the list of mergers
    }
}