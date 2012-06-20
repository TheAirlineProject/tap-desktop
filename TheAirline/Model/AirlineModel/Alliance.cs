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
        public string Name { get; set; }
        public List<Airline> Members { get; set; }
        public Airport Headquarter { get; set; }
        public Alliance(string name, Airport headquarter)
        {
            this.Name = name;
            this.Members = new List<Airline>();
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
}
