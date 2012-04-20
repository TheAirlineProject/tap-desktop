using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.GeneralModel.StatisticsModel;

namespace TheAirline.Model.AirlinerModel
{
    //the class for the statistics for an airliner
    public class AirlinerStatistics : GeneralStatistics
    {
        private FleetAirliner Airliner;
        public double FillingDegree { get { return getFillingDegree(); } set { ;} }
        public double Balance { get { return getBalance(); } set { ;} }
        public double IncomePerPassenger { get { return getIncomePerPassenger(); } set { ;} }
        public AirlinerStatistics(FleetAirliner airliner)
        {
            this.Airliner = airliner;
        }
        //get the degree of filling
        private double getFillingDegree()
        {
            double avgPassengers = getStatisticsValue(StatisticsTypes.GetStatisticsType("Passengers")) / getStatisticsValue(StatisticsTypes.GetStatisticsType("Departures"));

            double totalPassengers = Convert.ToDouble(this.Airliner.Airliner.getTotalSeatCapacity());

            double fillingDegree = avgPassengers / totalPassengers;


            return avgPassengers / totalPassengers;
        }
          //gets the income per passenger
        private double getIncomePerPassenger()
        {
            double totalPassengers = Convert.ToDouble(getStatisticsValue(StatisticsTypes.GetStatisticsType("Passengers")));

            return getBalance() / totalPassengers;
        }
        //gets the balance
        private double getBalance()
        {
            return getStatisticsValue(StatisticsTypes.GetStatisticsType("Airliner_Income"));
        }
       
    }
}
