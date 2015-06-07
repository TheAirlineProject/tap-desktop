using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using TheAirline.Infrastructure;
using TheAirline.Models.Airports;
using TheAirline.Models.General.Statistics;

namespace TheAirline.Models.Routes
{
    //the class for a combi route
    // TODO - Derive from route directly
    [Serializable]
    public class CombiRoute : PassengerRoute
    {
        #region Constructors and Destructors

        public CombiRoute(
            string id,
            Airport destination1,
            Airport destination2,
            DateTime startDate,
            double farePrice,
            double pricePerUnit)
            : base(id, destination1, destination2, startDate, farePrice)
        {
            Type = RouteType.Mixed;

            PricePerUnit = pricePerUnit;
        }

        private CombiRoute(SerializationInfo info, StreamingContext ctxt)
            : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("unitprice")]
        public double PricePerUnit { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        //returns the filling degree for the passengers

        public override double GetFillingDegree()
        {
            return (GetCargoFillingDegree() + GetPassengerFillingDegree())/2;
        }

        #endregion

        #region Methods

        private double GetCargoFillingDegree()
        {
            if (HasStopovers)
            {
                IEnumerable<Route> legs = Stopovers.SelectMany(s => s.Legs);
                Route[] enumerable = legs as Route[] ?? legs.ToArray();
                double fillingDegree = enumerable.Cast<CombiRoute>().Sum(leg => leg.GetFillingDegree());
                return fillingDegree/enumerable.Count();
            }
            double cargo = Convert.ToDouble(Statistics.GetTotalValue(StatisticsTypes.GetStatisticsType("Cargo")));

            double cargoCapacity =
                Convert.ToDouble(Statistics.GetTotalValue(StatisticsTypes.GetStatisticsType("Capacity")));

            return cargo/cargoCapacity;
        }

        private double GetPassengerFillingDegree()
        {
            if (HasStopovers)
            {
                IEnumerable<Route> legs = Stopovers.SelectMany(s => s.Legs);
                Route[] enumerable = legs as Route[] ?? legs.ToArray();
                double fillingDegree = enumerable.Cast<PassengerRoute>().Sum(leg => leg.GetFillingDegree());
                return fillingDegree/enumerable.Count();
            }
            double passengers =
                Convert.ToDouble(Statistics.GetTotalValue(StatisticsTypes.GetStatisticsType("Passengers")));

            double passengerCapacity =
                Convert.ToDouble(Statistics.GetTotalValue(StatisticsTypes.GetStatisticsType("Capacity")));

            return passengers/passengerCapacity;
        }

        #endregion

        //På passenger route check box til "Tag cargo med hvis muligt". Så kan combi airliners bruges herpå./Skal Combi route være en passenger route men med cargo price?

        //adds a route airliner class to the route
    }
}