//locked for verison 0.3.6t2 (this serves no purpose whatsoever)

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using TheAirline.Helpers;
using TheAirline.Infrastructure;
using TheAirline.Model.GeneralModel;
using TheAirline.Models.Airlines;
using TheAirline.Models.General.Countries;
using TheAirline.Models.General.Scenarios;

namespace TheAirline.Models.General
{
    [Serializable]
    //the class for the game object
    public class GameObject : BaseModel, INotifyPropertyChanged
    {
        #region Constants

        public const int StartYear = 1960;

        #endregion

        #region Static Fields

        private static GameObject _gameInstance;

        #endregion

        #region Fields

        [Versioning("gametime")] private DateTime _gameTime;

        [Versioning("humanmoney")] private double _humanMoney;

        #endregion

        #region Constructors and Destructors

        private GameObject()
        {
            //this.PassengerDemandFactor = 100;
            GameTime = new DateTime(2007, 12, 31, 10, 0, 0);
            TimeZone =
                TimeZones.GetTimeZones()
                         .Find(gtz => gtz.UTCOffset == new TimeSpan(0, 0, 0));
            Difficulty = DifficultyLevels.GetDifficultyLevel("Easy");
            NewsBox = new NewsBox();
            PagePerformanceCounterEnabled = false;
            FinancePageEnabled = false;
            DayRoundEnabled = true;
            Contracts = new ObservableCollection<SpecialContractType>();
        }

        private GameObject(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
            if (Version == 1)
                Contracts = new ObservableCollection<SpecialContractType>();
        }

        #endregion

        #region Public Events

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        [Versioning("currencycountry")]
        public Country CurrencyCountry { get; set; }

        [Versioning("dayroundenabled")]
        public Boolean DayRoundEnabled { get; set; }

        [Versioning("difficulty")]
        public DifficultyLevel Difficulty { get; set; }

        public Boolean FinancePageEnabled { get; set; }
        /*
        public double FuelPrice 
        {
            get
            {
                return this._fuelprice;
            }
            set
            {
                this._fuelprice = value;
                this.NotifyPropertyChanged("FuelPrice");
            }
        }
        */

        public DateTime GameTime
        {
            get { return _gameTime; }
            set
            {
                _gameTime = value;
                NotifyPropertyChanged("GameTime");
            }
        }

        [Versioning("contracts", Version = 2)]
        public ObservableCollection<SpecialContractType> Contracts { get; set; }

        //public DateTime GameTime { get; set; }

        [Versioning("humanairline")]
        public Airline HumanAirline { get; private set; }

        public double HumanMoney
        {
            get { return _humanMoney; }
            set
            {
                _humanMoney = value;
                NotifyPropertyChanged("HumanMoney");
            }
        }

        [Versioning("mainairline")]
        public Airline MainAirline { get; set; }

        [Versioning("name")]
        public string Name { get; set; }

        [Versioning("newsbox")]
        public NewsBox NewsBox { get; set; }

        public Boolean PagePerformanceCounterEnabled { get; set; }

        [Versioning("scenario")]
        public ScenarioObject Scenario { get; set; }

        [Versioning("startdate")]
        public DateTime StartDate { get; set; }

        public long StartMoney => GetStartMoney();

        [Versioning("timezone")]
        public GameTimeZone TimeZone { get; set; }

        [Versioning("fuelprice")]
        public double FuelPrice { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 2);

            base.GetObjectData(info, context);
        }

        public static GameObject GetInstance()
        {
            return _gameInstance ?? (_gameInstance = new GameObject());
        }

        //sets the instance to an instance

        //restarts the instance
        public static void RestartInstance()
        {
            _gameInstance = new GameObject();
        }

        public static void SetInstance(GameObject instance)
        {
            _gameInstance = instance;
        }

        public void AddHumanMoney(double value)
        {
            HumanMoney += value;
            HumanAirline.Money += value;
        }

        public void SetHumanAirline(Airline airline)
        {
            HumanAirline = airline;
            HumanMoney = airline.Money;
        }

        #endregion

        #region Methods

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private long GetStartMoney()
        {
            double baseStartMoney = 7500000;

            baseStartMoney *= Difficulty.MoneyLevel;

            return Convert.ToInt64(GeneralHelpers.GetInflationPrice(baseStartMoney));
        }

        #endregion

        //returns the game instance
    }
}