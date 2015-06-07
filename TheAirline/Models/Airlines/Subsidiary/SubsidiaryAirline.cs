using System;
using System.Runtime.Serialization;
using TheAirline.Infrastructure;
using TheAirline.Models.Routes;

namespace TheAirline.Models.Airlines.Subsidiary
{
    //the class for a subsidiary airline for an airline

    // TODO - Replace with a proper reference to an airline object with a subsidiary flag instead of a class. Improper OO design.
    [Serializable]
    public class SubsidiaryAirline : Airline
    {
        #region Constructors and Destructors

        public SubsidiaryAirline(
            Airline airline,
            AirlineProfile profile,
            AirlineMentality mentality,
            AirlineFocus market,
            AirlineLicense license,
            Route.RouteType routefocus)
            : base(profile, mentality, market, license, routefocus)
        {
            Airline = airline;

            foreach (AirlineLogo logo in Airline.Profile.Logos)
            {
                Profile.AddLogo(logo);
            }
        }

        private SubsidiaryAirline(SerializationInfo info, StreamingContext ctxt)
            : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("airline")]
        public Airline Airline { get; set; }

        #endregion

        #region Public Methods and Operators

        public new void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        public override bool isHuman()
        {
            return Airline != null && Airline.isHuman();
        }

        public override bool IsSubsidiaryAirline()
        {
            return Airline != null;
        }

        #endregion
    }
}