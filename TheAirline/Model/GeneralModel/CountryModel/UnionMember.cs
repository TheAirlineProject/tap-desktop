using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel.CountryModel
{ 
    //the class for a member of an union
    [ProtoContract]
    public class UnionMember
    {
        [ProtoMember(1)]
        public Country Country { get; set; }
        [ProtoMember(2)]
        public DateTime MemberFromDate { get; set; }
        [ProtoMember(3)]
        public DateTime MemberToDate { get; set; }
        public UnionMember(Country country, DateTime memberFromDate, DateTime memberToDate)
        {
            this.Country = country;
            this.MemberFromDate = memberFromDate;
            this.MemberToDate = memberToDate;
        }
    }
}
