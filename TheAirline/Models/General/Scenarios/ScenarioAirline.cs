using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TheAirline.General.Models;
using TheAirline.Infrastructure;
using TheAirline.Models.Airliners;
using TheAirline.Models.Airlines;
using TheAirline.Models.Airports;

namespace TheAirline.Models.General.Scenarios
{
    //the class for an airline (opponent) in a scenario
    [Serializable]
    public class ScenarioAirline : BaseModel
    {
        #region Constructors and Destructors

        public ScenarioAirline(Airline airline, Airport homebase)
        {
            Airline = airline;
            Homebase = homebase;
            Routes = new List<ScenarioAirlineRoute>();
        }

        private ScenarioAirline(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("airline")]
        public Airline Airline { get; set; }

        [Versioning("homebase")]
        public Airport Homebase { get; set; }

        [Versioning("routes")]
        public List<ScenarioAirlineRoute> Routes { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        public void AddRoute(ScenarioAirlineRoute route)
        {
            Routes.Add(route);
        }

        #endregion
    }

    [Serializable]
    //a route for an scenario airline
    public class ScenarioAirlineRoute : BaseModel
    {
        #region Constructors and Destructors

        public ScenarioAirlineRoute(Airport destination1, Airport destination2, AirlinerType airlinertype, int quantity)
        {
            Destination1 = destination1;
            Destination2 = destination2;
            AirlinerType = airlinertype;
            Quantity = quantity;
        }

        private ScenarioAirlineRoute(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("airlinertype")]
        public AirlinerType AirlinerType { get; set; }

        [Versioning("destination1")]
        public Airport Destination1 { get; set; }

        [Versioning("destination2")]
        public Airport Destination2 { get; set; }

        [Versioning("quantity")]
        public int Quantity { get; set; }

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