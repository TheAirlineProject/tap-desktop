using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.StatisticsModel;

namespace TheAirline.GUIModel.PagesModel.AirlinesPageModel
{
    //the mvvm object for an airline
    public class AirlinesMVVM
    {
        public Airline Airline { get; set; }
        public double Profit { get; set; }
        public double AvgFleetAge { get; set; }
        public double Passengers { get; set; }
        public double PassengersPerFlight { get; set; }
        public double Flights { get; set; }
        public double Cargo { get; set; }
        public double CargoPerFlight { get; set; }
        public AirlinesMVVM(Airline airline)
        {
            this.Airline = airline;
            this.Profit = this.Airline.getProfit();
            this.AvgFleetAge = this.Airline.getAverageFleetAge();
            
            StatisticsType passengersType = StatisticsTypes.GetStatisticsType("Passengers");
            StatisticsType passengersAvgType = StatisticsTypes.GetStatisticsType("Passengers%");
            StatisticsType arrivalsType = StatisticsTypes.GetStatisticsType("Arrivals");
            StatisticsType cargoType = StatisticsTypes.GetStatisticsType("Cargo");
            StatisticsType cargoAvgType = StatisticsTypes.GetStatisticsType("Cargo%");

            this.Passengers = this.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, passengersType);
            this.PassengersPerFlight = this.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, passengersAvgType);
            this.Flights = this.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, arrivalsType);
            this.Cargo = this.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, cargoType);
            this.CargoPerFlight = this.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, cargoAvgType);
        }
    }
}
