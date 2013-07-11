
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;

namespace TheAirline.Model.PilotModel
{
    //the class for a pilot
    [DataContract]
    public class Pilot
    {
        
        public enum PilotRating { 
             [EnumMember(Value="A")]A=3, 
             [EnumMember(Value="B")]B=4,
             [EnumMember(Value="C")]C=5, 
             [EnumMember(Value="D")]D=7, 
             [EnumMember(Value="E")]E=10 }
        
        [DataMember]
        public PilotRating Rating { get; set; }
        
        [DataMember]
        public PilotProfile Profile { get; set; }
        [DataMember]
        public Airline Airline { get; set; }
        [DataMember]
        public DateTime AirlineSignedDate { get; set; }
        [DataMember]
        public DateTime EducationTime { get; set; }
        [DataMember]
        public FleetAirliner Airliner { get; set; }
        public const int RetirementAge = 55;
         public Pilot(PilotProfile profile, DateTime educationTime, PilotRating rating)
        {
            this.Profile = profile;
            this.EducationTime = educationTime;
            this.Rating = rating;
    
        }
        //sets the airline for a pilot
        public void setAirline(Airline airline, DateTime signDate)
        {
            this.Airline = airline;
            this.AirlineSignedDate = signDate;
        }
    }
    //the list of pilots
    public class Pilots
    {
        private static List<Pilot> pilots = new List<Pilot>();
        //adds a pilot to the list
        public static void AddPilot(Pilot pilot)
        {
            pilots.Add(pilot);
        }
        //clears the list of pilots
        public static void Clear()
        {
            pilots.Clear();
        }
        //returns all pilots
        public static List<Pilot> GetPilots()
        {
            return pilots;
        }
        //returns all unassigned pilots
        public static List<Pilot> GetUnassignedPilots()
        {
            return pilots.FindAll(p => p.Airline == null);
        }
        public static List<Pilot> GetUnassignedPilots(Predicate<Pilot> match)
        {
            return GetUnassignedPilots().FindAll(match);
        }
        //removes a pilot from the list
        public static void RemovePilot(Pilot pilot)
        {
            pilots.Remove(pilot);
        }
        //counts the number of unassigned pilots
        public static int GetNumberOfUnassignedPilots()
        {
            return pilots.FindAll(p => p.Airline == null).Count;
        }
        //counts the number of pilots
        public static int GetNumberOfPilots() 
        {
            return pilots.Count;
        }
    }
}
