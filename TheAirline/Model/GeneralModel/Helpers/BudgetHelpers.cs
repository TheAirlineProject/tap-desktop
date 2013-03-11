using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlineModel;

namespace TheAirline.Model.GeneralModel.Helpers
{
    class BudgetHelpers
    {
        public long GetFleetValue()
        {
            List<Int64> values = new List<Int64>();
            foreach(FleetAirliner airliner in GameObject.GetInstance().HumanAirline.Fleet)
            {
                values.Add(airliner.Airliner.Price);
            }
            return values.Sum();
        }

        public double GetAvgFleetValue()
        {
            long value = GetFleetValue();
            double size = GameObject.GetInstance().HumanAirline.Fleet.Count();
            return (value / size);
        }

    }
}
