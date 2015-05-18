using System;
using System.Runtime.Serialization;
using TheAirline.Infrastructure;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Models.Airlines
{
    //the class for a share in an airline
    [Serializable]
    public class AirlineShare : BaseModel
    {
        #region Constructors and Destructors

        public AirlineShare(Airline airline, double price)
        {
            Price = price;
            Airline = airline;
        }

        private AirlineShare(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("airline")]
        public Airline Airline { get; set; }

        [Versioning("price")]
        public double Price { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion

        //ændres til antal, percent per airline
    }
}