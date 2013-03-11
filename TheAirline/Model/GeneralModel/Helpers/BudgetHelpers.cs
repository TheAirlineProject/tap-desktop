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

    }
}
