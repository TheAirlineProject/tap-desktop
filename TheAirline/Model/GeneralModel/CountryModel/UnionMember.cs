
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel.CountryModel
{ 
    //the class for a member of an union
    [Serializable]
    public class UnionMember
    {
        
        public Country Country { get; set; }
        
        public DateTime MemberFromDate { get; set; }
        
        public DateTime MemberToDate { get; set; }
        public UnionMember(Country country, DateTime memberFromDate, DateTime memberToDate)
        {
            this.Country = country;
            this.MemberFromDate = memberFromDate;
            this.MemberToDate = memberToDate;
        }
    }
}
