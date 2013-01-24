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
            double hAvgPPD = StatisticsHelpers.GetHumanAvgTicketPPD();
            double qAvgPPD = StatisticsHelpers.GetTotalTicketPPD();
            double ppdVMax = StatisticsHelpers.GetPPDdifference().Max() / StatisticsHelpers.GetPPDdifference().Max() * 100;
            double hPPD = (hAvgPPD - qAvgPPD) / StatisticsHelpers.GetPPDdifference().Max() * 100;

            double fillRateModifier = StatisticsHelpers.GetHumanFillAverage();
            fillRateModifier = Math.Pow((fillRateModifier - 100), 2);

            double otp = StatisticsHelpers.GetHumanOnTime();

            return (hPPD * 0.4) + (fillRateModifier * 0.2) + (otp * 0.4);
        }
        
    }
}