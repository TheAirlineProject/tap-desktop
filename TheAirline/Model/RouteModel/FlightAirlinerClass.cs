using System;
using System.Runtime.Serialization;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.RouteModel
{
    /*! Flight airliner class.
   * This class is used for an airliner class onboard of a flight
   * The class needs parameters for type of class and the number of passengers
   */

    [Serializable]
    public class FlightAirlinerClass : BaseModel
    {
        #region Constructors and Destructors

        public FlightAirlinerClass(RouteAirlinerClass aClass, int passengers)
        {
            AirlinerClass = aClass;
            Passengers = passengers;
        }

        private FlightAirlinerClass(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("class")]
        public RouteAirlinerClass AirlinerClass { get; set; }

        [Versioning("passengers")]
        public int Passengers { get; set; }

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