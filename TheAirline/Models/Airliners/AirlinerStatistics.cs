using System;
using System.Runtime.Serialization;
using TheAirline.Infrastructure;
using TheAirline.Models.General.Statistics;

namespace TheAirline.Models.Airliners
{
    //the class for the statistics for an airliner
    [Serializable]
    public class AirlinerStatistics : GeneralStatistics
    {
        #region Fields

        [Versioning("airliner")] private readonly FleetAirliner _airliner;

        #endregion

        #region Constructors and Destructors

        public AirlinerStatistics(FleetAirliner airliner)
        {
            _airliner = airliner;
        }

        private AirlinerStatistics(SerializationInfo info, StreamingContext ctxt)
        {
        }

        #endregion

        #region Public Properties

        public double Balance => GetBalance();

        public double FillingDegree => GetFillingDegree();

        public double IncomePerPassenger => GetIncomePerPassenger();

        #endregion

        #region Public Methods and Operators

        public new void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion

        #region Methods

        private double GetBalance()
        {
            return GetStatisticsValue(StatisticsTypes.GetStatisticsType("Airliner_Income"));
        }

        //get the degree of filling
        private double GetFillingDegree()
        {
            double avgPassengers = GetStatisticsValue(StatisticsTypes.GetStatisticsType("Passengers"))
                                   /GetStatisticsValue(StatisticsTypes.GetStatisticsType("Arrivals"));

            double totalPassengers = Convert.ToDouble(_airliner.Airliner.GetTotalSeatCapacity());

            double fillingDegree = avgPassengers/totalPassengers;

            if (totalPassengers.Equals(0))
            {
                return 0;
            }
            return avgPassengers/totalPassengers;
        }

        //gets the income per passenger
        private double GetIncomePerPassenger()
        {
            double totalPassengers =
                Convert.ToDouble(GetStatisticsValue(StatisticsTypes.GetStatisticsType("Passengers")));

            if (totalPassengers.Equals(0))
            {
                return 0;
            }
            return GetBalance()/totalPassengers;
        }

        #endregion

        //gets the balance
    }
}