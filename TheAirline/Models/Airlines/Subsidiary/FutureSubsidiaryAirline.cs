using System;
using System.Runtime.Serialization;
using TheAirline.Infrastructure;
using TheAirline.Model.GeneralModel;
using TheAirline.Models.Routes;

namespace TheAirline.Models.Airlines.Subsidiary
{
    [Serializable]
    //the class for a future subsidiary airline for an airline
    public class FutureSubsidiaryAirline : BaseModel
    {
        #region Constructors and Destructors

        public FutureSubsidiaryAirline(
            string name,
            string iata,
            Airports.Airport airport,
            Airline.AirlineMentality mentality,
            Airline.AirlineFocus market,
            Route.RouteType airlineRouteFocus,
            string logo)
        {
            Name = name;
            IATA = iata;
            PreferedAirport = airport;
            Mentality = mentality;
            Market = market;
            Logo = logo;
            AirlineRouteFocus = airlineRouteFocus;
        }

        private FutureSubsidiaryAirline(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("focus")]
        public Route.RouteType AirlineRouteFocus { get; set; }

        [Versioning("IATA")]
        public string IATA { get; set; }

        [Versioning("logo")]
        public string Logo { get; set; }

        [Versioning("market")]
        public Airline.AirlineFocus Market { get; set; }

        [Versioning("mentality")]
        public Airline.AirlineMentality Mentality { get; set; }

        [Versioning("name")]
        public string Name { get; set; }

        [Versioning("airport")]
        public Airports.Airport PreferedAirport { get; set; }

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