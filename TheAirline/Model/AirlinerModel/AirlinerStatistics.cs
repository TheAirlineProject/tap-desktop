
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TheAirline.Model.GeneralModel.StatisticsModel;

namespace TheAirline.Model.AirlinerModel
{
    //the class for the statistics for an airliner
  [DataContract]
    public class AirlinerStatistics : GeneralStatistics
    {
      [DataMember]
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
            double avgPassengers = getStatisticsValue(StatisticsTypes.GetStatisticsType("Passengers")) / getStatisticsValue(StatisticsTypes.GetStatisticsType("Arrivals"));

            double totalPassengers = Convert.ToDouble(this.Airliner.Airliner.getTotalSeatCapacity());

            double fillingDegree = avgPassengers / totalPassengers;

            if (totalPassengers == 0)
                return 0;
            else
                return avgPassengers / totalPassengers;
        }
          //gets the income per passenger
        private double getIncomePerPassenger()
        {
            double totalPassengers = Convert.ToDouble(getStatisticsValue(StatisticsTypes.GetStatisticsType("Passengers")));

            if (totalPassengers == 0)
                return 0;
            else
                return getBalance() / totalPassengers;
        }
        //gets the balance
        private double getBalance()
        {
            return getStatisticsValue(StatisticsTypes.GetStatisticsType("Airliner_Income"));
        }
       
    }
}
