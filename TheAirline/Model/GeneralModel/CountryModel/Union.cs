using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace TheAirline.Model.GeneralModel.CountryModel
{
    //the class for an union or organizational
    public class Union : BaseUnit
    {
        public static string Section { get; set; }
        public List<UnionMember> Members { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime ObsoleteDate { get; set; }
        public Union(string uid,string shortname, DateTime creationDate, DateTime obsoleteDate) : base(uid,shortname)
        { 
            this.CreationDate = creationDate;
            this.ObsoleteDate = obsoleteDate;
        }
        //returns all members at a given date
        public List<UnionMember> getMembers(DateTime date)
        {
            return this.Members.FindAll(m => m.MemberFromDate < date && m.MemberToDate > date);
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
        //returns an union
        public static Union GetUnion(string uid)
        {
            return unions.Find(u => u.Uid == uid);
        }
    }
   

}
