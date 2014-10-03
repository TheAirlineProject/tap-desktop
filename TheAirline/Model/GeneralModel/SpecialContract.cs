using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.RouteModel;

namespace TheAirline.Model.GeneralModel
{
    /*the class for a speial contract*/

    public class SpecialContract : BaseModel
    {
        public SpecialContract(SpecialContractType type, DateTime date, Airline airline)
        {
            Airline = airline;
            Type = type;
            Date = date;
            IsOk = true;
            Routes = new List<Route>();
        }

        private SpecialContract(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion

        [Versioning("airline")]
        public Airline Airline { get; set; }

        [Versioning("date")]
        public DateTime Date { get; set; }

        [Versioning("type")]
        public SpecialContractType Type { get; set; }

        [Versioning("isok")]
        public Boolean IsOk { get; set; }

        [Versioning("routes")]
        public List<Route> Routes { get; set; }
    }
}