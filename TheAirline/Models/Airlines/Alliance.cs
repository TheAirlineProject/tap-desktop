using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using TheAirline.Infrastructure;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Models.Airlines
{
    [Serializable]
    //the class for an alliance of airlines
    public class Alliance : BaseModel
    {
        #region Constructors and Destructors

        public Alliance(DateTime formationDate, string name, Airports.Airport headquarter)
        {
            FormationDate = formationDate;
            Name = name;
            Members = new List<AllianceMember>();
            PendingMembers = new List<PendingAllianceMember>();
            Headquarter = headquarter;
        }

        private Alliance(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("date")]
        public DateTime FormationDate { get; set; }

        [Versioning("headquarter")]
        public Airports.Airport Headquarter { get; set; }

        public bool IsHumanAlliance
        {
            get { return Members.ToList().Exists(m => m.Airline.IsHuman); }
        }

        [Versioning("logo")]
        public string Logo { get; set; }

        [Versioning("members")]
        public List<AllianceMember> Members { get; set; }

        [Versioning("name")]
        public string Name { get; set; }

        [Versioning("pending")]
        public List<PendingAllianceMember> PendingMembers { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        public static string GenerateAllianceName()
        {
            var rnd = new Random();

            string[] tNames =
                {
                    "Wings Alliance", "Qualiflyer", "Air Team", "Sky Alliance", "WOW Alliance",
                    "Air Alliance", "Blue Sky", "Golden Circle Alliance", "Skywalkers", "One Air Alliance"
                };
            List<string> aNames = (from a in Alliances.GetAlliances() select a.Name).ToList();

            List<string> names = tNames.ToList().Except(aNames).ToList();

            return names[rnd.Next(names.Count)];
        }

        //adds an airline to the alliance
        public void AddMember(AllianceMember airline)
        {
            Members.Add(airline);
            airline.Airline.AddAlliance(this);
        }

        public void AddPendingMember(PendingAllianceMember pending)
        {
            PendingAllianceMember member = PendingMembers.FirstOrDefault(p => p.Airline == pending.Airline);

            if (member != null)
            {
                RemovePendingMember(member);
            }

            PendingMembers.Add(pending);
        }

        //removes an airline from the alliance
        public void RemoveMember(AllianceMember airline)
        {
            Members.Remove(airline);
            airline.Airline.RemoveAlliance(this);
        }

        public void RemoveMember(Models.Airlines.Airline airline)
        {
            AllianceMember member = Members.FirstOrDefault(m => m.Airline == airline);
            Members.Remove(member);
            airline.RemoveAlliance(this);
        }

        //adds a pending member to the alliance

        //removes a pending member from the alliance
        public void RemovePendingMember(PendingAllianceMember pending)
        {
            PendingMembers.Remove(pending);
        }

        #endregion

        //the method for generating an alliance name
    }

    //the list of alliances
    public class Alliances
    {
        #region Static Fields

        private static readonly List<Alliance> alliances = new List<Alliance>();

        #endregion

        #region Public Methods and Operators

        public static void AddAlliance(Alliance alliance)
        {
            alliances.Add(alliance);
        }

        //removes an alliance from the list

        //clears the list of alliances
        public static void Clear()
        {
            alliances.Clear();
        }

        public static List<Alliance> GetAlliances()
        {
            return alliances;
        }

        public static void RemoveAlliance(Alliance alliance)
        {
            alliances.Remove(alliance);
        }

        #endregion

        //adds an alliance to the list
    }

    [Serializable]
    //the class for pending acceptions to an alliance
    public class PendingAllianceMember : BaseModel
    {
        #region Constructors and Destructors

        public PendingAllianceMember(DateTime date, Alliance alliance, Models.Airlines.Airline airline, AcceptType type)
        {
            Alliance = alliance;
            Airline = airline;
            Date = date;
            Type = type;
        }

        private PendingAllianceMember(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Enums

        public enum AcceptType
        {
            Invitation,

            Request
        }

        #endregion

        #region Public Properties

        public Models.Airlines.Airline Airline { get; set; }

        public Alliance Alliance { get; set; }

        public DateTime Date { get; set; }

        public AcceptType Type { get; set; }

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