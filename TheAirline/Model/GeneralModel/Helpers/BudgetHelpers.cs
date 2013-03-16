using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlineModel;

namespace TheAirline.Model.GeneralModel.Helpers
{
    public class BudgetHelpers
    {
        //gets the total value of all aircraft in fleet combined
        public long GetFleetValue()
        {
            List<Int64> values = new List<Int64>();
            foreach(FleetAirliner airliner in GameObject.GetInstance().HumanAirline.Fleet)
            {
                values.Add(airliner.Airliner.Price);
            }
            return values.Sum();
        }

        //gets the average value of an aircraft in the fleet
        public double GetAvgFleetValue()
        {
            long value = GetFleetValue();
            double size = GameObject.GetInstance().HumanAirline.Fleet.Count();
            return (value / size);
        }

        //returns the remaining budget amount based on the current month
        public static double GetRemainingBudget(double budget)
        {
            return 1 - (GameObject.GetInstance().GameTime.Month / 12) * budget;
        }

        //sets default values and max values
        public static void SetDefaults(Airline humanAirline)
        {
           
            humanAirline.Budget.MarketingBudget = humanAirline.Budget.CSBudget = humanAirline.Budget.SecurityBudget = humanAirline.Money * 0.05;
            humanAirline.Budget.MaintenanceBudget = humanAirline.Money * 0.1;
            humanAirline.Budget.PrintBudget = humanAirline.Budget.TelevisionBudget = humanAirline.Budget.RadioBudget = humanAirline.Budget.InternetBudget = (humanAirline.Budget.MarketingBudget / 4);
            humanAirline.Budget.OverhaulBudget = humanAirline.Budget.PartsBudget = humanAirline.Budget.RemoteBudget = humanAirline.Budget.EnginesBudget = humanAirline.Budget.MaintenanceBudget / 4;
            humanAirline.Budget.ITBudget = humanAirline.Budget.EquipmentBudget = humanAirline.Budget.AirportBudget = humanAirline.Budget.InFlightBudget = humanAirline.Budget.SecurityBudget / 4;
            humanAirline.Budget.PRBudget = humanAirline.Budget.PromoBudget = humanAirline.Budget.CompBudget = humanAirline.Budget.ServCenterBudget = humanAirline.Budget.CSBudget / 4;
        }

        //verifies to make sure sub-budgets don't exceed master budget
        public static void verifyValues(AirlineBudget budget)
        {
            double userMarketingBudget = budget.RadioBudget + budget.TelevisionBudget + budget.PrintBudget + budget.InternetBudget;
            if (userMarketingBudget > budget.MarketingBudget)
            {
                double sumD = userMarketingBudget - budget.MarketingBudget;
                budget.RadioBudget -= (sumD / 4);
                budget.TelevisionBudget -= (sumD / 4);
                budget.PrintBudget -= (sumD / 4);
                budget.InternetBudget -= (sumD / 4);
            }

            double userMaintBudget = budget.PartsBudget + budget.EnginesBudget + budget.OverhaulBudget + budget.RemoteBudget;
            if (userMaintBudget > budget.MaintenanceBudget)
            {
                double sumD = userMaintBudget = budget.MaintenanceBudget;
                budget.PartsBudget -= (sumD / 4);
                budget.OverhaulBudget -= (sumD / 4);
                budget.EnginesBudget -= (sumD / 4);
                budget.RemoteBudget -= (sumD / 4);
            }

            double userCSBudget = budget.ServCenterBudget + budget.CompBudget + budget.PromoBudget + budget.PRBudget;
            if (userCSBudget > budget.CSBudget)
            {
                double sumD = userCSBudget - budget.CSBudget;
                budget.ServCenterBudget -= (sumD / 4);
                budget.CompBudget -= (sumD / 4);
                budget.PromoBudget -= (sumD / 4);
                budget.PRBudget -= (sumD / 4);
            }

            double userSecurityBudget = budget.AirportBudget + budget.EquipmentBudget + budget.InFlightBudget + budget.ITBudget;
            if (userSecurityBudget > budget.SecurityBudget)
            {
                double sumD = userSecurityBudget - budget.SecurityBudget;
                budget.AirportBudget -= (sumD / 4);
                budget.EquipmentBudget -= (sumD / 4);
                budget.InFlightBudget -= (sumD / 4);
                budget.ITBudget -= (sumD / 4);
            }

        }

    }
}
