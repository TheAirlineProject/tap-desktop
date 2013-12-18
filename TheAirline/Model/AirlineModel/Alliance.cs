
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirlineModel
{
    [Serializable]
    //the class for an alliance of airlines
    public class Alliance : ISerializable
    {
       
        public enum AllianceType { Codesharing, Full }
       [Versioning("type")] 
        public AllianceType Type { get; set; }
        [Versioning("name")]
        public string Name { get; set; }
        [Versioning("members")]
        public List<AllianceMember> Members { get; set; }
        [Versioning("headquarter")]
        public Airport Headquarter { get; set; }
        [Versioning("date")]
        public DateTime FormationDate { get; set; }
        [Versioning("pending")]
        public List<PendingAllianceMember> PendingMembers { get; set; }
        [Versioning("logo")]
        public string Logo { get; set; }
        public Boolean IsHumanAlliance { get{ return this.Members.ToList().Exists(m=>m.Airline.IsHuman);} private set { ;} }
        public Alliance(DateTime formationDate, AllianceType type, string name, Airport headquarter)
        {
            this.FormationDate = formationDate;
            this.Type = type;
            this.Name = name;
            this.Members = new List<AllianceMember>();
            this.PendingMembers = new List<PendingAllianceMember>();
            this.Headquarter = headquarter;
        }
        //adds an airline to the alliance
        public void addMember(AllianceMember airline)
        {
            this.Members.Add(airline);
            airline.Airline.addAlliance(this);
        }
        //removes an airline from the alliance
        public void removeMember(AllianceMember airline)
        {
            this.Members.Remove(airline);
            airline.Airline.removeAlliance(this);
        }
        public void removeMember(Airline airline)
        {
            AllianceMember member = this.Members.FirstOrDefault(m => m.Airline == airline);
            this.Members.Remove(member);
            airline.removeAlliance(this);
        }
        //adds a pending member to the alliance
        public void addPendingMember(PendingAllianceMember pending)
        {
            PendingAllianceMember member = this.PendingMembers.FirstOrDefault(p => p.Airline == pending.Airline);

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
            
            string[] tNames = new string[] { "Wings Alliance", "Qualiflyer","Air Team", "Sky Alliance", "WOW Alliance", "Air Alliance", "Blue Sky", "Golden Circle Alliance", "Skywalkers", "One Air Alliance" };
            List<string> aNames = (from a in Alliances.GetAlliances() select a.Name).ToList();

            List<string> names = tNames.ToList().Except(aNames).ToList();

            return names[rnd.Next(names.Count)];
            
        }
        private Alliance(SerializationInfo info, StreamingContext ctxt)
        {
            int version = info.GetInt16("version");

            var fields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null);

            IList<PropertyInfo> props = new List<PropertyInfo>(this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null));

            var propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

            foreach (SerializationEntry entry in info)
            {
                MemberInfo prop = propsAndFields.FirstOrDefault(p => ((Versioning)p.GetCustomAttribute(typeof(Versioning))).Name == entry.Name);

                if (prop != null)
                {
                    if (prop is FieldInfo)
                        ((FieldInfo)prop).SetValue(this, entry.Value);
                    else
                        ((PropertyInfo)prop).SetValue(this, entry.Value);
                }
            }

            var notSetProps = propsAndFields.Where(p => ((Versioning)p.GetCustomAttribute(typeof(Versioning))).Version > version);

            foreach (MemberInfo notSet in notSetProps)
            {
                Versioning ver = (Versioning)notSet.GetCustomAttribute(typeof(Versioning));

                if (ver.AutoGenerated)
                {
                    if (notSet is FieldInfo)
                        ((FieldInfo)notSet).SetValue(this, ver.DefaultValue);
                    else
                        ((PropertyInfo)notSet).SetValue(this, ver.DefaultValue);

                }

            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            Type myType = this.GetType();

            var fields = myType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null);

            IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null));

            var propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

            foreach (MemberInfo member in propsAndFields)
            {
                object propValue;

                if (member is FieldInfo)
                    propValue = ((FieldInfo)member).GetValue(this);
                else
                    propValue = ((PropertyInfo)member).GetValue(this, null);

                Versioning att = (Versioning)member.GetCustomAttribute(typeof(Versioning));

                info.AddValue(att.Name, propValue);
            }


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
    [Serializable] 
    //the class for pending acceptions to an alliance
    public class PendingAllianceMember : ISerializable
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
           private PendingAllianceMember(SerializationInfo info, StreamingContext ctxt)
        {
            int version = info.GetInt16("version");

            var fields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null);

            IList<PropertyInfo> props = new List<PropertyInfo>(this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null));

            var propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

            foreach (SerializationEntry entry in info)
            {
                MemberInfo prop = propsAndFields.FirstOrDefault(p => ((Versioning)p.GetCustomAttribute(typeof(Versioning))).Name == entry.Name);

                if (prop != null)
                {
                    if (prop is FieldInfo)
                        ((FieldInfo)prop).SetValue(this, entry.Value);
                    else
                        ((PropertyInfo)prop).SetValue(this, entry.Value);
                }
            }

            var notSetProps = propsAndFields.Where(p => ((Versioning)p.GetCustomAttribute(typeof(Versioning))).Version > version);

            foreach (MemberInfo notSet in notSetProps)
            {
                Versioning ver = (Versioning)notSet.GetCustomAttribute(typeof(Versioning));

                if (ver.AutoGenerated)
                {
                    if (notSet is FieldInfo)
                        ((FieldInfo)notSet).SetValue(this, ver.DefaultValue);
                    else
                        ((PropertyInfo)notSet).SetValue(this, ver.DefaultValue);

                }

            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            Type myType = this.GetType();

            var fields = myType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null);

            IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null));

            var propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

            foreach (MemberInfo member in propsAndFields)
            {
                object propValue;

                if (member is FieldInfo)
                    propValue = ((FieldInfo)member).GetValue(this);
                else
                    propValue = ((PropertyInfo)member).GetValue(this, null);

                Versioning att = (Versioning)member.GetCustomAttribute(typeof(Versioning));

                info.AddValue(att.Name, propValue);
            }


        }
    }
}
