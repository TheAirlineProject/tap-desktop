using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using TheAirline.Model.AirlineModel;
using TheAirline.GraphicsModel.PageModel.PageGameModel;
using TheAirline.Model.GeneralModel.ScenarioModel;
using System.Runtime.Serialization;
using ProtoBuf;

//locked for verison 0.3.6t2 (this serves no purpose whatsoever)

namespace TheAirline.Model.GeneralModel
{
    [ProtoContract]
    //the class for the game object
    public class GameObject
    {
        private static GameObject GameInstance;
        [ProtoMember(1)]
        public Country CurrencyCountry { get; set; }
        public Boolean PagePerformanceCounterEnabled { get; set; }
        public Boolean FinancePageEnabled { get; set; }
        [ProtoMember(2)]
        public Boolean DayRoundEnabled { get; set; }
        [ProtoMember(3)]
        public DateTime GameTime { get; set; }
        [ProtoMember(4)]
        public DateTime StartDate { get; set; }
        [ProtoMember(5,AsReference=true)]
        public Airline HumanAirline { get; set; }
        [ProtoMember(6,AsReference=true)]
        public Airline MainAirline { get; set; }
        [ProtoMember(7)]
        public NewsBox NewsBox { get; set; }
        [ProtoMember(8)]
        public double FuelPrice { get; set; }
        [ProtoMember(9)]
        public long StartMoney { get { return getStartMoney(); } set { ;} }
        [ProtoMember(10)]
        public GameTimeZone TimeZone { get; set; }
        [ProtoMember(11)]
        public string Name { get; set; }
        [ProtoMember(12)]
        public ScenarioObject Scenario { get; set; }
       // public enum DifficultyLevel { Easy, Normal, Hard } 
        [ProtoMember(13)]
        public DifficultyLevel Difficulty { get; set; }
        //public double PassengerDemandFactor { get; set; }
        public const int StartYear = 1960;
        
        private GameObject()
        {
            //this.PassengerDemandFactor = 100;
            this.GameTime = new DateTime(2007, 12, 31, 10, 0, 0);
            this.TimeZone = TimeZones.GetTimeZones().Find(delegate(GameTimeZone gtz) { return gtz.UTCOffset == new TimeSpan(0, 0, 0); });
            this.Difficulty = DifficultyLevels.GetDifficultyLevel("Easy");
            this.NewsBox = new NewsBox();
            this.PagePerformanceCounterEnabled = false;
            this.FinancePageEnabled = false;
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
        //sets the instance to an instance
        public static void SetInstance(GameObject instance)
        {
            GameInstance = instance;
        }
        //restarts the instance
        public static void RestartInstance()
        {
            GameInstance = new GameObject();
        }

    }
}
