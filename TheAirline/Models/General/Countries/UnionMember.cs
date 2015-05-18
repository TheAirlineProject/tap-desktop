using System;
using System.Runtime.Serialization;
using TheAirline.Infrastructure;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Models.General.Countries
{
    //the class for a member of an union
    [Serializable]
    public class UnionMember : BaseModel
    {
        #region Constructors and Destructors

        public UnionMember(Country country, DateTime memberFromDate, DateTime memberToDate)
        {
            Country = country;
            MemberFromDate = memberFromDate;
            MemberToDate = memberToDate;
        }

        private UnionMember(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("country")]
        public Country Country { get; set; }

        [Versioning("memberfrom")]
        public DateTime MemberFromDate { get; set; }

        [Versioning("memberto")]
        public DateTime MemberToDate { get; set; }

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