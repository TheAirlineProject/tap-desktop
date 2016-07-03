using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TheAirline.Infrastructure;
using TheAirline.Models.Airports;

namespace TheAirline.Models.Routes
{
    [Serializable]
    //the class for the stop over routes
    public class StopoverRoute : BaseModel
    {
        #region Constructors and Destructors

        public StopoverRoute(Airport stopover)
        {
            Legs = new List<Route>();
            Stopover = stopover;
        }

        //adds a leg to the stopover route

        private StopoverRoute(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("legs")]
        public List<Route> Legs { get; set; }

        [Versioning("stopover")]
        public Airport Stopover { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        public void AddLeg(Route leg)
        {
            Legs.Add(leg);
        }

        #endregion
    }
}