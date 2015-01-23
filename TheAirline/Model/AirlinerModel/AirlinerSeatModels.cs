using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheAirline.Model.AirlinerModel
{
    //the list of seat models
    public class AirlinerSeatModels
    {
        private static List<Seatplanner.SeatPlannerModel> models = new List<Seatplanner.SeatPlannerModel>();
        //adds a new seat model to the list
        public static void AddSeatModel(Seatplanner.SeatPlannerModel model)
        {
            models.Add(model);
        }
        //returns the seat model for an airliner type
        public static Seatplanner.SeatPlannerModel GetSeatModel(AirlinerType type)
        {
            return models.FirstOrDefault(m => m.Type == type);
        }

    }
}
