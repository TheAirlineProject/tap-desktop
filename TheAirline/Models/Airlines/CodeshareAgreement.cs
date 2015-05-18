using System;
using System.Runtime.Serialization;
using TheAirline.Infrastructure;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Models.Airlines
{
    //the class for a codeshare agreement - if one way then it is from airline1 to airline2 (airline1 codeshares its routes with airline2)
    [Serializable]
    public class CodeshareAgreement : BaseModel
    {
        #region Constants

        public const double TicketSalePercent = 1;

        #endregion

        #region Constructors and Destructors

        public CodeshareAgreement(Airline airline1, Airline airline2, CodeshareType type)
        {
            Airline1 = airline1;
            Airline2 = airline2;
            Type = type;
        }

        private CodeshareAgreement(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Enums

        public enum CodeshareType
        {
            OneWay,

            BothWays
        }

        #endregion

        #region Public Properties

        [Versioning("airline1")]
        public Airline Airline1 { get; set; }

        [Versioning("airline2")]
        public Airline Airline2 { get; set; }

        [Versioning("type")]
        public CodeshareType Type { get; set; }

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