using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using TheAirline.Model.AirlineModel;

namespace TheAirline.Model.GeneralModel
{
    //the class for the game object
    public class GameObject
    {
        private static GameObject GameInstance;
        public DateTime GameTime { get; set; }
        public Airline HumanAirline { get; set; }
        public NewsBox NewsBox { get; set; }
        public double FuelPrice { get; set; }
        public long StartMoney { get { return getStartMoney(); } set { ;} }
        public GameTimeZone TimeZone { get; set; }
        public string Name { get; set; }
        private GameObject()
        {
            this.GameTime = new DateTime(2007, 12, 31, 10, 0, 0);
            this.TimeZone = TimeZones.GetTimeZones().Find(delegate(GameTimeZone gtz) { return gtz.UTCOffset == new TimeSpan(0, 0, 0); });
            this.FuelPrice = 1.142;

            this.NewsBox = new NewsBox();
        }

        //returns the start money based on year of start
        private long getStartMoney()
        {
            long baseStartMoney = 100000000;

            return GeneralHelpers.GetInflationPrice(baseStartMoney);
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
