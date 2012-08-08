using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirportModel;

namespace TheAirline.Model.AirlineModel
{
    //the class for an alliance of airlines
    public class Alliance
    {
        public enum AllianceType { Codesharing, Full }
        public AllianceType Type { get; set; }
        public string Name { get; set; }
        public List<Airline> Members { get; set; }
        public Airport Headquarter { get; set; }
        public DateTime FormationDate { get; set; }
        public List<PendingAllianceMember> PendingMembers { get; set; }
        public Alliance(DateTime formationDate, AllianceType type, string name, Airport headquarter)
        {
            this.FormationDate = formationDate;
            this.Type = type;
            this.Name = name;
            this.Members = new List<Airline>();
            this.PendingMembers = new List<PendingAllianceMember>();
            this.Headquarter = headquarter;
        }
        //adds an airline to the alliance
        public void addMember(Airline airline)
        {
            this.Members.Add(airline);
            airline.addAlliance(this);
        }
        //removes an airline from the alliance
        public void removeMember(Airline airline)
        {
            this.Members.Remove(airline);
            airline.removeAlliance(this);
        }
        //adds a pending member to the alliance
        public void addPendingMember(PendingAllianceMember pending)
        {
            PendingAllianceMember member = this.PendingMembers.Find(p => p.Airline == pending.Airline);

            if (member != null)
                this.removePendingMember(member);

            this.PendingMembers.Add(pending);
        }
        //removes a pending member from the alliance
        public void removePendingMember(PendingAllianceMember pending)
        {
            this.PendingMembers.Remove(pending);
        }
        //the method for generating an alliance name
        public static string GenerateAllianceName()
        {
            Random rnd = new Random();

            string[] tNames = new string[] { "Star Alliance", "One World", "Sky Team", "Sky Alliance", "WOW Alliance", "Air Alliance", "Blue Sky", "Golden Circle Alliance", "Skywalkers", "One Air Alliance" };
            List<string> aNames = (from a in Alliances.GetAlliances() select a.Name).ToList();

            List<string> names = tNames.ToList().Except(aNames).ToList();

            return names[rnd.Next(names.Count)];
            
        }

    }
    //the list of alliances
    public class Alliances
    {
        private static List<Alliance> alliances = new List<Alliance>();
        //adds an alliance to the list
        public static void AddAlliance(Alliance alliance)
        {
            alliances.Add(alliance);
        }
        //removes an alliance from the list
        public static void RemoveAlliance(Alliance alliance)
        {
            alliances.Remove(alliance);
        }
        //returns all alliances
        public static List<Alliance> GetAlliances()
        {
            return alliances;
        }
        //clears the list of alliances
        public static void Clear()
        {
            alliances.Clear();
        }
    }
    //the class for pending acceptions to an alliance
    public class PendingAllianceMember
    {
        public Airline Airline { get; set; }
        public DateTime Date { get; set; }
        public enum AcceptType { Invitation, Request }
        public AcceptType Type { get; set; }
        public Alliance Alliance { get; set; }
        public PendingAllianceMember(DateTime date,Alliance alliance, Airline airline, AcceptType type)
        {
            this.Alliance = alliance;
            this.Airline = airline;
            this.Date = date;
            this.Type = type;
        }
    }
}
