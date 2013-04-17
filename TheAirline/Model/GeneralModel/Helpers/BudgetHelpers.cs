using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlineModel;

namespace TheAirline.Model.GeneralModel.Helpers
{
    public class BudgetHelpers
    {           
        long money = (long) GameObject.GetInstance().HumanAirline.Money;

        //gets the total value of all aircraft in fleet combined
        public static long GetFleetValue()
        {
            List<Int64> values = new List<Int64>();
            foreach(FleetAirliner airliner in GameObject.GetInstance().HumanAirline.Fleet)
            {
                values.Add(airliner.Airliner.Price);
            }
            return values.Sum();
        }

        //gets the average value of an aircraft in the fleet
        public static double GetAvgFleetValue()
        {
            if (GameObject.GetInstance().HumanAirline.Fleet.Count() == 0)
            { return 0; }
            else
            {
                return GetFleetValue() / GameObject.GetInstance().HumanAirline.Fleet.Count();
            }
        }

        //returns the remaining budget amount based on the current month
        public static long GetRemainingBudget()
        {
            return ( 1 - GameObject.GetInstance().GameTime.Month / 12) * GameObject.GetInstance().HumanAirline.Budget.TotalBudget;
        }

        //returns the estimated end of year cash
        public static long GetEndYearCash(long budget, long money)
        {
            Airline humanAirline = GameObject.GetInstance().HumanAirline;

            long endYearCash = (long)(money - BudgetHelpers.GetRemainingBudget() - (humanAirline.Budget.TotalBudget * 15 / 100));
            return endYearCash;
        }

        public static long GetTotalSubValues(Airline airline)
        {
            long value = 0;
            foreach (Airline subsidiary in airline.Subsidiaries)
            {
                int airportValue = 10000 * (int)StatisticsHelpers.getWorldAirportsServed();
                value += (GetFleetValue() + airportValue);
            }

            return value;
        }

        public static long GetAvgSubValue(Airline airline)
        {
            if (airline.Subsidiaries.Count() == 0)
            { return 0; }

            else
            {
                return GetTotalSubValues(GameObject.GetInstance().HumanAirline) / GameObject.GetInstance().HumanAirline.Subsidiaries.Count();
            }
        }
  
        //sets default values and max values
        public static long SetDefaults(Airline humanAirline)
        {
           
            humanAirline.Budget.MarketingBudget = humanAirline.Budget.CSBudget = humanAirline.Budget.SecurityBudget = (long)(humanAirline.Money * 0.05);
            humanAirline.Budget.MaintenanceBudget = (long)(humanAirline.Money * 0.1);
            humanAirline.Budget.PrintBudget = humanAirline.Budget.TelevisionBudget = humanAirline.Budget.RadioBudget = humanAirline.Budget.InternetBudget = (humanAirline.Budget.MarketingBudget / 4);
            humanAirline.Budget.OverhaulBudget = humanAirline.Budget.PartsBudget = humanAirline.Budget.RemoteBudget = humanAirline.Budget.EnginesBudget = humanAirline.Budget.MaintenanceBudget / 4;
            humanAirline.Budget.ITBudget = humanAirline.Budget.EquipmentBudget = humanAirline.Budget.AirportBudget = humanAirline.Budget.InFlightBudget = humanAirline.Budget.SecurityBudget / 4;
            humanAirline.Budget.PRBudget = humanAirline.Budget.PromoBudget = humanAirline.Budget.CompBudget = humanAirline.Budget.ServCenterBudget = humanAirline.Budget.CSBudget / 4;
            return humanAirline.Budget.MarketingBudget + humanAirline.Budget.MaintenanceBudget + humanAirline.Budget.SecurityBudget + humanAirline.Budget.CSBudget;
        }

        //verifies to make sure sub-budgets don't exceed master budget
        public static void VerifyValues(AirlineBudget budget)
        {
            long userMarketingBudget = budget.RadioBudget + budget.TelevisionBudget + budget.PrintBudget + budget.InternetBudget;
            if (userMarketingBudget > budget.MarketingBudget)
            {
                budget.RadioBudget = budget.TelevisionBudget = budget.PrintBudget = budget.InternetBudget = budget.MarketingBudget / 4;
            }

            long userMaintBudget = budget.PartsBudget + budget.EnginesBudget + budget.OverhaulBudget + budget.RemoteBudget;
            if (userMaintBudget > budget.MaintenanceBudget)
            {
                budget.PartsBudget = budget.EnginesBudget = budget.OverhaulBudget = budget.RemoteBudget = budget.MaintenanceBudget / 4;
            }

            long userCSBudget = budget.ServCenterBudget + budget.CompBudget + budget.PromoBudget + budget.PRBudget;
            if (userCSBudget > budget.CSBudget)
            {
                budget.ServCenterBudget = budget.CompBudget = budget.PromoBudget = budget.PRBudget = budget.CSBudget / 4;
            }

            long userSecurityBudget = budget.AirportBudget + budget.EquipmentBudget + budget.InFlightBudget + budget.ITBudget;
            if (userSecurityBudget > budget.SecurityBudget)
            {
                budget.AirportBudget = budget.EquipmentBudget = budget.InFlightBudget = budget.ITBudget = budget.SecurityBudget / 4;
            }

        }

        //returns the budget from 1 year ago
        public static AirlineBudget GetOneYearBudget(DateTime date, int n)
        {            Airline humanAirline = GameObject.GetInstance().HumanAirline;
            if (GameObject.GetInstance().StartDate > GameObject.GetInstance().GameTime.AddMonths(-12))
            {
                AirlineBudget oldBudget = (from d in humanAirline.TestBudget where (d.Key.Month == date.Month) && (d.Key.Year == (date.Year - n)) select d.Value).FirstOrDefault();
                return oldBudget;
            }
            else
            {
                return humanAirline.Budget;
            }
        }

        //returns a bogus budget for testing
        public static IDictionary<DateTime, AirlineBudget> GetTestBudget(Airline airline)
        {
            AirlineBudget budget = GameObject.GetInstance().HumanAirline.Budget;
            Random random = new Random();
            for (int i = 1; i < 61; i++)
            {
                budget.BudgetActive = GameObject.GetInstance().GameTime.AddMonths(-1);
                budget.BudgetExpires = GameObject.GetInstance().GameTime.AddMonths(-1 + 12);
                budget.Cash = random.Next(1, 100000000);
                budget.AirportBudget = random.Next(1, 500000);
                budget.CompBudget = random.Next(1, 500000);
                budget.CSBudget = random.Next(1, 2000000);
                budget.EndYearCash = random.Next(1, 100000000);
                budget.EnginesBudget = random.Next(1, 1000000);
                budget.EquipmentBudget = random.Next(1, 500000);
                budget.FleetSize = random.Next(1, 250);
                budget.FleetValue = random.Next(10, 10000);
                budget.InFlightBudget = random.Next(1, 500000);
                budget.InternetBudget = random.Next(1, 500000);
                budget.ITBudget = random.Next(1, 500000);
                budget.MaintenanceBudget = random.Next(1, 4000000);
                budget.MarketingBudget = random.Next(1, 2000000);
                budget.OverhaulBudget = random.Next(1, 1000000);
                budget.PartsBudget = random.Next(1, 1000000);
                budget.PRBudget = random.Next(1, 500000);
                budget.PrintBudget = random.Next(1, 500000);
                budget.PromoBudget = random.Next(1, 500000);
                budget.RadioBudget = random.Next(1, 500000);
                budget.RemainingBudget = random.Next(1, 10000000);
                budget.RemoteBudget = random.Next(1, 1000000);
                budget.SecurityBudget = random.Next(1, 2000000);
                budget.ServCenterBudget = random.Next(1, 500000);
                budget.Subsidiaries = random.Next(0, 4);
                budget.TelevisionBudget = random.Next(1, 500000);
                budget.TotalBudget = random.Next(4, 10000000);
                budget.TotalEmployees = random.Next(1, 10000);
                budget.TotalPayroll = random.Next(500000, 10000000);
                budget.TotalSubValue = random.Next(200000000, 1000000000);
                if (airline.TestBudget.ContainsKey(budget.BudgetActive.AddMonths(-1)))
                {}
                else {
                airline.TestBudget.Add(budget.BudgetActive.AddMonths(-1), budget);
                }
            }
            return airline.TestBudget;
        }
    }
}
