namespace TheAirline.GUIModel.PagesModel.AirlinesPageModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;

    using TheAirline.Model.AirlineModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.GeneralModel.Helpers;
    using TheAirline.Model.GeneralModel.StatisticsModel;

    //the mvvm object for an airline
    public class AirlinesMVVM : INotifyPropertyChanged
    {
        #region Fields

        private Boolean _isbuyable;

        private int _stocks;

        private int _stocksforsale;

        #endregion

        #region Constructors and Destructors

        public AirlinesMVVM(Airline airline)
        {
            this.Airline = airline;
            this.Profit = this.Airline.GetProfit();
            this.AvgFleetAge = this.Airline.GetAverageFleetAge();

            StatisticsType passengersType = StatisticsTypes.GetStatisticsType("Passengers");
            StatisticsType passengersAvgType = StatisticsTypes.GetStatisticsType("Passengers%");
            StatisticsType arrivalsType = StatisticsTypes.GetStatisticsType("Arrivals");
            StatisticsType cargoType = StatisticsTypes.GetStatisticsType("Cargo");
            StatisticsType cargoAvgType = StatisticsTypes.GetStatisticsType("Cargo%");

            this.Passengers = this.Airline.Statistics.GetStatisticsValue(
                GameObject.GetInstance().GameTime.Year,
                passengersType);
            this.PassengersPerFlight = this.Airline.Statistics.GetStatisticsValue(
                GameObject.GetInstance().GameTime.Year,
                passengersAvgType);
            this.Flights = this.Airline.Statistics.GetStatisticsValue(
                GameObject.GetInstance().GameTime.Year,
                arrivalsType);
            this.Cargo = this.Airline.Statistics.GetStatisticsValue(GameObject.GetInstance().GameTime.Year, cargoType);
            this.CargoPerFlight = this.Airline.Statistics.GetStatisticsValue(
                GameObject.GetInstance().GameTime.Year,
                cargoAvgType);

            this.Stocks = this.Airline.Shares.Count;
            this.StocksForSale = this.Airline.Shares.Count(s => s.Airline == null);
            this.StockPrice = AirlineHelpers.GetPricePerAirlineShare(this.Airline);

            this.setOwnershipValues();
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public Airline Airline { get; set; }

        public double AvgFleetAge { get; set; }

        public double Cargo { get; set; }

        public double CargoPerFlight { get; set; }

        public double Flights { get; set; }

        public Boolean IsBuyable
        {
            get
            {
                return this._isbuyable;
            }
            set
            {
                this._isbuyable = value;
                this.NotifyPropertyChanged("IsBuyable");
            }
        }

        public ObservableCollection<AirlineSharesMVVM> OwnershipAirlines { get; set; }

        public double Passengers { get; set; }

        public double PassengersPerFlight { get; set; }

        public double Profit { get; set; }

        public double StockPrice { get; set; }

        public int Stocks
        {
            get
            {
                return this._stocks;
            }
            set
            {
                this._stocks = value;
                this.NotifyPropertyChanged("Stocks");
            }
        }

        public int StocksForSale
        {
            get
            {
                return this._stocksforsale;
            }
            set
            {
                this._stocksforsale = value;
                this.NotifyPropertyChanged("StocksForSale");
            }
        }

        #endregion

        //adds ownership to the airline

        #region Public Methods and Operators

        public void addOwnership(Airline airline, int shares)
        {
            AirlineHelpers.SetAirlineShares(this.Airline, airline, shares);

            this.StocksForSale -= shares;

            if (this.OwnershipAirlines.Any(o => o.Airline == airline))
            {
                AirlineSharesMVVM share = this.OwnershipAirlines.First(o => o.Airline == airline);

                share.Shares += shares;
                share.Percent = Convert.ToDouble(share.Shares) / Convert.ToDouble(this.Stocks) * 100;
            }
            else
            {
                double percent = Convert.ToDouble(shares) / Convert.ToDouble(this.Stocks) * 100;

                this.OwnershipAirlines.Add(new AirlineSharesMVVM(airline, shares, percent));
            }

            int humanShares = this.Airline.Shares.Count(s => s.Airline == GameObject.GetInstance().HumanAirline);

            double humanSharesPercent = Convert.ToDouble(humanShares) / Convert.ToDouble(this.Stocks) * 100;

            this.IsBuyable = !this.Airline.IsHuman && humanSharesPercent > 50;
        }

        public void setOwnershipValues()
        {
            this.OwnershipAirlines = new ObservableCollection<AirlineSharesMVVM>();

            IEnumerable<Airline> airlines =
                this.Airline.Shares.Where(s => s.Airline != null).Select(s => s.Airline).Distinct();

            foreach (Airline shareAirline in airlines)
            {
                int shares = this.Airline.Shares.Count(s => s.Airline == shareAirline);
                double percent = Convert.ToDouble(shares) / Convert.ToDouble(this.Stocks) * 100;

                this.OwnershipAirlines.Add(new AirlineSharesMVVM(shareAirline, shares, percent));
            }

            int humanShares = this.Airline.Shares.Count(s => s.Airline == GameObject.GetInstance().HumanAirline);

            double humanSharesPercent = Convert.ToDouble(humanShares) / Convert.ToDouble(this.Stocks) * 100;

            this.IsBuyable = !this.Airline.IsHuman && humanSharesPercent > 50;
        }

        #endregion

        #region Methods

        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }

    //the mvvm class for the ownership percent in an airline
    public class AirlineSharesMVVM : INotifyPropertyChanged
    {
        #region Fields

        private double _percent;

        private int _shares;

        #endregion

        #region Constructors and Destructors

        public AirlineSharesMVVM(Airline airline, int shares, double percent)
        {
            this.Airline = airline;
            this.Shares = shares;
            this.Percent = percent;
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public Airline Airline { get; set; }

        public double Percent
        {
            get
            {
                return this._percent;
            }
            set
            {
                this._percent = value;
                this.NotifyPropertyChanged("Percent");
            }
        }

        public int Shares
        {
            get
            {
                return this._shares;
            }
            set
            {
                this._shares = value;
                this.NotifyPropertyChanged("Shares");
            }
        }

        #endregion

        #region Methods

        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}