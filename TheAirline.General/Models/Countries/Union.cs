using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TheAirline.General.Models.Countries;
using TheAirline.Infrastructure;

namespace TheAirline.Models.General.Countries
{
    //the class for an union or organizational
    [Serializable]
    public class Union : BaseUnit
    {
        #region Constructors and Destructors

        public Union(string section, string uid, string shortname, DateTime creationDate, DateTime obsoleteDate)
            : base(uid, shortname)
        {
            Section = section;
            CreationDate = creationDate;
            ObsoleteDate = obsoleteDate;

            Members = new List<UnionMember>();
        }

        private Union(SerializationInfo info, StreamingContext ctxt)
            : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        public static string Section { get; set; }

        [Versioning("creation")]
        public DateTime CreationDate { get; set; }

        [Versioning("members")]
        public List<UnionMember> Members { get; set; }

        public override string Name => Translator.GetInstance().GetString(Section, Uid);

        [Versioning("obsolete")]
        public DateTime ObsoleteDate { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        public void AddMember(UnionMember member)
        {
            Members.Add(member);
        }

        //returns all members at a given date
        public List<UnionMember> GetMembers(DateTime date)
        {
            return Members.FindAll(m => m.MemberFromDate < date && m.MemberToDate > date);
        }

        //returns if a union has a country as member
        public bool IsMember(Country country, DateTime date)
        {
            return Members.Find(m => m.Country == country && m.MemberFromDate < date && m.MemberToDate > date)
                   != null;
        }

        #endregion

        //adds a member to the union
    }

    //the list of unions
    public class Unions
    {
        #region Static Fields

        private static readonly List<Union> unions = new List<Union>();

        #endregion

        #region Public Methods and Operators

        public static void AddUnion(Union union)
        {
            unions.Add(union);
        }

        public static void Clear()
        {
            unions.Clear();
        }

        public static Union GetUnion(string uid)
        {
            return unions.Find(u => u.Uid == uid);
        }

        //returns the list of unions
        public static List<Union> GetUnions()
        {
            return unions;
        }

        //return all unions for a country
        public static List<Union> GetUnions(Country country, DateTime date)
        {
            return
                unions.FindAll(
                    u =>
                    u.Members.Find(um => um.Country == country) != null && u.CreationDate < date
                    && u.ObsoleteDate > date);
        }

        #endregion

        //adds an union to the list

        //returns an union
    }
}