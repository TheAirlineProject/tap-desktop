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
        //calculates customer happiness as a function of average ticket price, crowding on flights, and on-time %
        public static double GetCustomerHappiness()
        {

            Dictionary<Airline, Double> fillAverages = StatisticsHelpers.GetFillAverages();
            Dictionary<Airline, Double> onTimePercent = StatisticsHelpers.GetTotalOnTime();
            Dictionary<Airline, Double> ticketPPD = StatisticsHelpers.GetTotalPPD();
            IDictionary<Airline, Double> scaleAvgFill = StatisticsHelpers.GetRatingScale(fillAverages);
            IDictionary<Airline, Double> scaleOnTimeP = StatisticsHelpers.GetRatingScale(onTimePercent);
            IDictionary<Airline, Double> scalePPD = StatisticsHelpers.GetRatingScale(ticketPPD);

            double humanAvgFill = scaleAvgFill[GameObject.GetInstance().HumanAirline];
            double humanOTP = scaleOnTimeP[GameObject.GetInstance().HumanAirline];
            double humanPPD = scalePPD[GameObject.GetInstance().HumanAirline];

            return (humanPPD * 0.4) + (humanAvgFill * 0.2) + (humanOTP * 0.4);
        }
 
        //calculates employee happiness as a function of wages, discounts, and free pilots (relative to workload)
        public static double GetEmployeeHappiness()
        {
            Dictionary<Airline, Double> wages = StatisticsHelpers.GetEmployeeWages();
            Dictionary<Airline, Double> discounts = StatisticsHelpers.GetEmployeeDiscounts();
            Dictionary<Airline, Double> unassignedPilots = StatisticsHelpers.GetUnassignedPilots();
            IDictionary<Airline, Double> scaleWages = StatisticsHelpers.GetRatingScale(wages);
            IDictionary<Airline, Double> scaleDiscounts = StatisticsHelpers.GetRatingScale(discounts);
            IDictionary<Airline, Double> scaleUPilots = StatisticsHelpers.GetRatingScale(unassignedPilots);

            double humanWages = scaleWages[GameObject.GetInstance().HumanAirline];
            double humanDiscounts = scaleDiscounts[GameObject.GetInstance().HumanAirline];
            double humanUnassignedPilots = scaleUPilots[GameObject.GetInstance().HumanAirline];

            return (humanWages * 0.7) + (humanUnassignedPilots * 0.2) + (humanDiscounts * 0.1);
        }
        
    }
}