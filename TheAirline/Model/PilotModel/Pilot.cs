using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.Model.PilotModel
{
    //the class for a pilot
    [Serializable]
    public class Pilot : BaseModel
    {
        #region Constants

        public const int RetirementAge = 55;

        #endregion

        #region Constructors and Destructors

        public Pilot(PilotProfile profile, DateTime educationTime, PilotRating rating)
        {
            Profile = profile;
            EducationTime = educationTime;
            Rating = rating;
            Aircrafts = new List<string>();
        }

        private Pilot(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
            if (Version == 1)
            {
                Rating = GeneralHelpers.GetPilotRating();
            }
            if (Version < 3)
            {
                Aircrafts = GeneralHelpers.GetPilotAircrafts(this);
                Training = null;
            }
        }

        #endregion

        #region Public Properties

        [Versioning("aircrafts", Version = 3)]
        public List<string> Aircrafts { get; set; }

        [Versioning("airline")]
        public Airline Airline { get; set; }

        [Versioning("signeddate")]
        public DateTime AirlineSignedDate { get; set; }

        [Versioning("airliner")]
        public FleetAirliner Airliner { get; set; }

        [Versioning("education")]
        public DateTime EducationTime { get; set; }

        public Boolean OnTraining
        {
            get { return Training != null; }
        }

        [Versioning("profile")]
        public PilotProfile Profile { get; set; }

        [Versioning("pilotrating", Version = 2)]
        public PilotRating Rating { get; set; }

        [Versioning("training", Version = 3)]
        public PilotTraining Training { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 3);

            base.GetObjectData(info, context);
        }

        //adds an airliner type family which the pilot expirence
        public void AddAirlinerFamily(string family)
        {
            Aircrafts.Add(family);
        }

        //sets the airline for a pilot
        public void SetAirline(Airline airline, DateTime signDate)
        {
            Airline = airline;
            AirlineSignedDate = signDate;
        }

        #endregion
    }

    //the list of pilots
    public class Pilots
    {
        #region Static Fields

        private static readonly List<Pilot> pilots = new List<Pilot>();

        #endregion

        #region Public Methods and Operators

        public static void AddPilot(Pilot pilot)
        {
            lock (pilots)
            {
                if (pilot != null)
                {
                    pilots.Add(pilot);
                }
            }
        }

        //clears the list of pilots
        public static void Clear()
        {
            pilots.Clear();
        }

        public static int GetNumberOfPilots()
        {
            return pilots.Count;
        }

        public static int GetNumberOfUnassignedPilots()
        {
            return GetUnassignedPilots().Count;
        }

        //returns all pilots
        public static List<Pilot> GetPilots()
        {
            return pilots;
        }

        //returns all unassigned pilots
        public static List<Pilot> GetUnassignedPilots()
        {
            List<Pilot> unassigned = pilots.FindAll(p => p.Airline == null);

            if (unassigned.Count < 5)
            {
                GeneralHelpers.CreatePilots(10);

                return GetUnassignedPilots();
            }

            return unassigned;
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

        #endregion

        //adds a pilot to the list

        //counts the number of unassigned pilots
    }
}