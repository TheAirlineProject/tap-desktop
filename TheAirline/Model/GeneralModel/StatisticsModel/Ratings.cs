using System;
using System.Collections.Generic;
using System.Linq;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.Model.GeneralModel.StatisticsModel
{
    public class Ratings
    {
        #region Public Methods and Operators

        public static double GetCustomerHappiness(Airline airline)
        {
            const int negInt = -1;
            Dictionary<Airline, Double> fillAverages = StatisticsHelpers.GetFillAverages();
            Dictionary<Airline, Double> onTimePercent = StatisticsHelpers.GetTotalOnTime();
            Dictionary<Airline, Double> ticketPPD = StatisticsHelpers.GetTotalPPD();
            IDictionary<Airline, Double> scaleAvgFill = StatisticsHelpers.GetRatingScale(fillAverages);
            IDictionary<Airline, Double> scaleOnTimeP = StatisticsHelpers.GetRatingScale(onTimePercent);
            IDictionary<Airline, Double> scalePPD = StatisticsHelpers.GetRatingScale(ticketPPD);
            IDictionary<Airline, Double> scaleLuxury = StatisticsHelpers.GetRatingScale(GetAirlinesLuxuryLevels());

            double airlineAvgFill = scaleAvgFill[airline];
            double airlineOTP = scaleOnTimeP[airline];
            double airlinePPD = scalePPD[airline];
            double airlineLuxury = scaleLuxury[airline];

            return ((airlinePPD*negInt + 100)*0.4) + (airlineAvgFill*0.2) + (airlineOTP*0.2)
                   + (airlineLuxury*0.2);
        }

        //returns all airline luxury levels

        //calculates employee happiness as a function of wages, discounts, and free pilots (relative to workload)
        public static double GetEmployeeHappiness(Airline airline)
        {
            Dictionary<Airline, Double> wages = StatisticsHelpers.GetEmployeeWages();
            Dictionary<Airline, Double> discounts = StatisticsHelpers.GetEmployeeDiscounts();
            Dictionary<Airline, Double> unassignedPilots = StatisticsHelpers.GetUnassignedPilots();
            IDictionary<Airline, Double> scaleWages = StatisticsHelpers.GetRatingScale(wages);
            IDictionary<Airline, Double> scaleDiscounts = StatisticsHelpers.GetRatingScale(discounts);
            IDictionary<Airline, Double> scaleUPilots = StatisticsHelpers.GetRatingScale(unassignedPilots);

            double airlineWages = scaleWages[airline];
            double airlineDiscounts = scaleDiscounts[airline];
            double airlineUnassignedPilots = scaleUPilots[airline];

            return (airlineWages*0.7) + (airlineUnassignedPilots*0.2) + (airlineDiscounts*0.1);
        }

        #endregion

        #region Methods

        private static double GetAirlineLuxuryLevel(Airline airline)
        {
            int luxuryLevel;

            if (airline.Alliances.Count > 0)
            {
                luxuryLevel =
                    airline.Alliances.SelectMany(a => a.Members)
                           .Select(m => m.Airline)
                           .Max(m => airline.Facilities.Sum(f => f.LuxuryLevel));
            }
            else
            {
                luxuryLevel = airline.Facilities.Sum(f => f.LuxuryLevel);
            }
            return luxuryLevel;
        }

        private static Dictionary<Airline, Double> GetAirlinesLuxuryLevels()
        {
            return Airlines.GetAllAirlines().ToDictionary(airline => airline, GetAirlineLuxuryLevel);
        }

        #endregion

        //calculates customer happiness as a function of average ticket price, crowding on flights, and on-time % - for an airline
    }
}