
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace TheAirline.Model.GeneralModel.CountryModel
{
    //the class for an union or organizational
    [Serializable]
    public class Union : BaseUnit
    {
        public static string Section { get; set; }
        public List<UnionMember> Members { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime ObsoleteDate { get; set; }
        public Union(string section, string uid,string shortname, DateTime creationDate, DateTime obsoleteDate) : base(uid,shortname)
        {
            Union.Section = section;
            this.CreationDate = creationDate;
            this.ObsoleteDate = obsoleteDate;

            this.Members = new List<UnionMember>();
        }
        //returns all members at a given date
        public List<UnionMember> getMembers(DateTime date)
        {
            return this.Members.FindAll(m => m.MemberFromDate < date && m.MemberToDate > date);
        }
        //returns if a union has a country as member
        public Boolean isMember(Country country, DateTime date)
        {
            return this.Members.Find(m => m.Country == country && m.MemberFromDate < date && m.MemberToDate > date) != null;
        }
        //adds a member to the union
        public void addMember(UnionMember member)
        {
            this.Members.Add(member);
        }
        public override string Name
        {
            get
            {
                return Translator.GetInstance().GetString(Union.Section, this.Uid);;
            }
        }
    }
    //the list of unions
    public class Unions
    {
        private static List<Union> unions = new List<Union>();
        //adds an union to the list
        public static void AddUnion(Union union)
        {
            unions.Add(union);
        }
        //returns the list of unions
        public static List<Union> GetUnions()
        {
            return unions;
        }
        //return all unions for a country
        public static List<Union> GetUnions(Country country, DateTime date)
        {
            return unions.FindAll(u => u.Members.Find(um=>um.Country == country)!=null && u.CreationDate < date && u.ObsoleteDate > date);
        }
        //returns an union
        public static Union GetUnion(string uid)
        {
            return unions.Find(u => u.Uid == uid);
        }
        //clears the list of unions
        public static void Clear()
        {
            unions.Clear();
        }
    }
   

}
