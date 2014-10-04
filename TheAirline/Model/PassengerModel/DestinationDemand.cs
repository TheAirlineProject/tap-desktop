using System;
using System.Runtime.Serialization;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.PassengerModel
{
    [Serializable]
    //the class for the demand rate for an airport/destination
    public class DestinationDemand : BaseModel
    {
        #region Constructors and Destructors

        public DestinationDemand(string destination, ushort rate)
        {
            Rate = rate;
            Destination = destination;
        }

        private DestinationDemand(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("destination")]
        public string Destination { get; set; }

        [Versioning("rate")]
        public ushort Rate { get; set; }

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