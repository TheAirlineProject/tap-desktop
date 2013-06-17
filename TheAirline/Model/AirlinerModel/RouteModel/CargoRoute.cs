
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel.StatisticsModel;

namespace TheAirline.Model.AirlinerModel.RouteModel
{
   [Serializable]
    //the class for a cargo route
    public class CargoRoute : Route
    {
       public double PricePerUnit { get; set; }
        public CargoRoute(string id, Airport destination1, Airport destination2, double pricePerUnit)
            : base(RouteType.Cargo, id, destination1, destination2)
        {
            this.PricePerUnit = pricePerUnit;
        }
        public override double getFillingDegree()
        {
            if (this.HasStopovers)
            {
                double fillingDegree = 0;

                var legs = this.Stopovers.SelectMany(s => s.Legs);
                foreach (CargoRoute leg in legs)
                {
                    fillingDegree += leg.getFillingDegree();
                }
                return fillingDegree / legs.Count();

            }
            else
            {
                double cargo = Convert.ToDouble(this.Statistics.getTotalValue(StatisticsTypes.GetStatisticsType("Cargo")));

                double cargoCapacity = Convert.ToDouble(this.Statistics.getTotalValue(StatisticsTypes.GetStatisticsType("Capacity")));

                return cargo / cargoCapacity;
            }
        }
    }
}
