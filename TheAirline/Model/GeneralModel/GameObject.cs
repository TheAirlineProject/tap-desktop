using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel.ScenarioModel;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Reflection;


//locked for verison 0.3.6t2 (this serves no purpose whatsoever)

namespace TheAirline.Model.GeneralModel
{
    [Serializable]
    //the class for the game object
    public class GameObject : INotifyPropertyChanged, ISerializable
    {
        private static GameObject GameInstance;
        [Versioning("currencycountry")]
        public Country CurrencyCountry { get; set; }
        public Boolean PagePerformanceCounterEnabled { get; set; }
        public Boolean FinancePageEnabled { get; set; }
        [Versioning("dayroundenabled")]
        public Boolean DayRoundEnabled { get; set; }
        
        //public DateTime GameTime { get; set; }
        [Versioning("startdate")]
        public DateTime StartDate { get; set; }
        [Versioning("humanairline")]
        public Airline HumanAirline { get; private set; }
        [Versioning("mainairline")]
        public Airline MainAirline { get; set; }
        [Versioning("newsbox")]
        public NewsBox NewsBox { get; set; }
        [Versioning("fuelprice")]
        private double _fuelprice;
        public double FuelPrice 
        {
            get { return _fuelprice; }
            set { _fuelprice = value; NotifyPropertyChanged("FuelPrice"); } 
        }
        
        public long StartMoney { get { return getStartMoney(); } set { ;} }
        [Versioning("timezone")]
        public GameTimeZone TimeZone { get; set; }
        [Versioning("name")]
        public string Name { get; set; }
        [Versioning("scenario")]
        public ScenarioObject Scenario { get; set; }
       // public enum DifficultyLevel { Easy, Normal, Hard } 
        [Versioning("difficulty")]
        public DifficultyLevel Difficulty { get; set; }
        //public double PassengerDemandFactor { get; set; }
        public const int StartYear = 1960;
        [Versioning("gametime")]
        private DateTime _gameTime;
        public DateTime GameTime
        {
            get { return _gameTime; }
            set { _gameTime = value; NotifyPropertyChanged("GameTime"); }
        }
        [Versioning("humanmoney")]
        private double _humanMoney;
        public double HumanMoney
        {
            get { return _humanMoney; }
            set { _humanMoney = value; NotifyPropertyChanged("HumanMoney"); }
        }
       
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
         //add money to the human airline
        public void addHumanMoney(double value)
        {
            this.HumanMoney += value;
            this.HumanAirline.Money += value;
        }
        //returns the start money based on year of start
        private long getStartMoney()
        {
            
            double baseStartMoney = 12500000;

            baseStartMoney *= this.Difficulty.MoneyLevel;
          
            return Convert.ToInt64(GeneralHelpers.GetInflationPrice(baseStartMoney));
        }
        //sets the human airline
        public void setHumanAirline(Airline airline)
        {
            this.HumanAirline = airline;
            this.HumanMoney = airline.Money;
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
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
          private GameObject(SerializationInfo info, StreamingContext ctxt)
        {
            int version = info.GetInt16("version");

            IList<PropertyInfo> props = new List<PropertyInfo>(this.GetType().GetProperties().Where(p => p.GetCustomAttribute(typeof(Versioning)) != null && ((Versioning)p.GetCustomAttribute(typeof(Versioning))).AutoGenerated));

            foreach (SerializationEntry entry in info)
            {
                PropertyInfo prop = props.FirstOrDefault(p => ((Versioning)p.GetCustomAttribute(typeof(Versioning))).Name == entry.Name);


                if (prop != null)
                    prop.SetValue(this, entry.Value);
            }

            var notSetProps = props.Where(p => ((Versioning)p.GetCustomAttribute(typeof(Versioning))).Version > version);

            foreach (PropertyInfo prop in notSetProps)
            {
                Versioning ver = (Versioning)prop.GetCustomAttribute(typeof(Versioning));

                if (ver.AutoGenerated)
                    prop.SetValue(this, ver.DefaultValue);

            }




        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            Type myType = this.GetType();
            IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties().Where(p => p.GetCustomAttribute(typeof(Versioning)) != null));

            foreach (PropertyInfo prop in props)
            {
                object propValue = prop.GetValue(this, null);

                Versioning att = (Versioning)prop.GetCustomAttribute(typeof(Versioning));

                info.AddValue(att.Name, propValue);
            }

        }
        

    }
}
