
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheAirline.Model.AirlineModel
{
    //the class for the ratings for an airline
    [Serializable]
    public class AirlineRatings
    {
        
        public int CustomerHappinessRating { get; set; }
        
        public int SafetyRating { get; set; }
        
        public int SecurityRating { get; set; }
        
        public int EmployeeHappinessRating { get; set; }
        
        public int MaintenanceRating { get; set; }
        public AirlineRatings()
        {
            this.CustomerHappinessRating = 50;
            this.SafetyRating = 50;
            this.SecurityRating = 50;
            this.EmployeeHappinessRating = 50;
            this.MaintenanceRating = 50;
        }
    }
}
