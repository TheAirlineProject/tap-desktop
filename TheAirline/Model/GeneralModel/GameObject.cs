using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using TheAirline.Model.AirlineModel;
using TheAirline.GraphicsModel.PageModel.PageGameModel;

//locked for verison 0.3.6 (this serves no purpose whatsoever)

namespace TheAirline.Model.GeneralModel
{
    //the class for the game object
    public class GameObject
    {
        private static GameObject GameInstance;
        public Country CurrencyCountry { get; set; }
        public Boolean PagePerformanceCounterEnabled { get; set; }
        public Boolean DayRoundEnabled { get; set; }
        public DateTime GameTime { get; set; }
        public DateTime StartDate { get; set; }
        public Airline HumanAirline { get; set; }
        public Airline MainAirline { get; set; }
        public NewsBox NewsBox { get; set; }
        public double FuelPrice { get; set; }
        public long StartMoney { get { return getStartMoney(); } set { ;} }
        public GameTimeZone TimeZone { get; set; }
        public string Name { get; set; }
       // public enum DifficultyLevel { Easy, Normal, Hard } 
        public DifficultyLevel Difficulty { get; set; }
        public double PassengerDemandFactor { get; set; }
        public const int StartYear = 1960;
        
        private GameObject()
        {
            this.PassengerDemandFactor = 100;
            this.GameTime = new DateTime(2007, 12, 31, 10, 0, 0);
            this.TimeZone = TimeZones.GetTimeZones().Find(delegate(GameTimeZone gtz) { return gtz.UTCOffset == new TimeSpan(0, 0, 0); });
            this.Difficulty = DifficultyLevels.GetDifficultyLevel("Easy");
            this.NewsBox = new NewsBox();
            this.PagePerformanceCounterEnabled = false;
            this.DayRoundEnabled = true;
        }

        //returns the start money based on year of start
        
        private long getStartMoney()
        {
            
            double baseStartMoney = 12500000;

            baseStartMoney *= this.Difficulty.MoneyLevel;
          
            return Convert.ToInt64(GeneralHelpers.GetInflationPrice(baseStartMoney));
        }

        //returns the game instance
        public static GameObject GetInstance()
        {
            if (GameInstance == null)
                GameInstance = new GameObject();
            return GameInstance;
        }

        //restarts the instance
        public static void RestartInstance()
        {
            GameInstance = new GameObject();
        }

    }
}
