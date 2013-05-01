using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.GeneralModel.StatisticsModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.Model.StatisticsModel
{
    public class Ratings
    {
        //calculates customer happiness as a function of average ticket price, crowding on flights, and on-time % - for an airline
        public static double GetCustomerHappiness(Airline airline)
        {
            int negInt = -1;
            Dictionary<Airline, Double> fillAverages = StatisticsHelpers.GetFillAverages(); 
            Dictionary<Airline, Double> onTimePercent = StatisticsHelpers.GetTotalOnTime();
            Dictionary<Airline, Double> ticketPPD = StatisticsHelpers.GetTotalPPD();
            IDictionary<Airline, Double> scaleAvgFill = StatisticsHelpers.GetRatingScale(fillAverages);
            IDictionary<Airline, Double> scaleOnTimeP = StatisticsHelpers.GetRatingScale(onTimePercent);
            IDictionary<Airline, Double> scalePPD = StatisticsHelpers.GetRatingScale(ticketPPD);

            double airlineAvgFill = scaleAvgFill[airline]; 
            double airlineOTP = scaleOnTimeP[airline];
            double airlinePPD = scalePPD[airline];

            return ((airlinePPD * negInt + 100) * 0.4) + (airlineAvgFill * 0.2) + (airlineOTP * 0.4);
        }
 
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

            return (airlineWages * 0.7) + (airlineUnassignedPilots * 0.2) + (airlineDiscounts * 0.1);
        }
        
    }
}