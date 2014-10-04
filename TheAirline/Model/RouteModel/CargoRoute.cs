using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.StatisticsModel;

namespace TheAirline.Model.RouteModel
{
    [Serializable]
    //the class for a cargo route
    public class CargoRoute : Route
    {
        #region Constructors and Destructors

        public CargoRoute(
            string id,
            Airport destination1,
            Airport destination2,
            DateTime startDate,
            double pricePerUnit)
            : base(RouteType.Cargo, id, destination1, destination2, startDate)
        {
            PricePerUnit = pricePerUnit;
        }

        private CargoRoute(SerializationInfo info, StreamingContext ctxt)
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

        public override double GetFillingDegree()
        {
            if (HasStopovers)
            {
                IEnumerable<Route> legs = Stopovers.SelectMany(s => s.Legs);
                Route[] enumerable = legs as Route[] ?? legs.ToArray();
                double fillingDegree = enumerable.Cast<CargoRoute>().Sum(leg => leg.GetFillingDegree());
                return fillingDegree/enumerable.Count();
            }
            double cargo = Convert.ToDouble(Statistics.GetTotalValue(StatisticsTypes.GetStatisticsType("Cargo")));

            double cargoCapacity =
                Convert.ToDouble(Statistics.GetTotalValue(StatisticsTypes.GetStatisticsType("Capacity")));

            if (cargo > cargoCapacity)
            {
                return 1;
            }

            return cargo/cargoCapacity;
        }

        #endregion
    }
}