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
        
    }
}