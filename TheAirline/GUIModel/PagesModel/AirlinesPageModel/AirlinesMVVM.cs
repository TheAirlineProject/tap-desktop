using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TheAirline.Helpers;
using TheAirline.Models.Airlines;
using TheAirline.Models.General;
using TheAirline.Models.General.Statistics;

namespace TheAirline.GUIModel.PagesModel.AirlinesPageModel
{
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
            Airline = airline;
            Profit = Airline.GetProfit();
            AvgFleetAge = Airline.GetAverageFleetAge();

            StatisticsType passengersType = StatisticsTypes.GetStatisticsType("Passengers");
            StatisticsType passengersAvgType = StatisticsTypes.GetStatisticsType("Passengers%");
            StatisticsType arrivalsType = StatisticsTypes.GetStatisticsType("Arrivals");
            StatisticsType cargoType = StatisticsTypes.GetStatisticsType("Cargo");
            StatisticsType cargoAvgType = StatisticsTypes.GetStatisticsType("Cargo%");

            Passengers = Airline.Statistics.GetStatisticsValue(
                GameObject.GetInstance().GameTime.Year,
                passengersType);
            PassengersPerFlight = Airline.Statistics.GetStatisticsValue(
                GameObject.GetInstance().GameTime.Year,
                passengersAvgType);
            Flights = Airline.Statistics.GetStatisticsValue(
                GameObject.GetInstance().GameTime.Year,
                arrivalsType);
            Cargo = Airline.Statistics.GetStatisticsValue(GameObject.GetInstance().GameTime.Year, cargoType);
            CargoPerFlight = Airline.Statistics.GetStatisticsValue(
                GameObject.GetInstance().GameTime.Year,
                cargoAvgType);

            Stocks = Airline.Shares.Count;
            StocksForSale = Airline.Shares.Count(s => s.Airline == null);
            StockPrice = AirlineHelpers.GetPricePerAirlineShare(Airline);

            setOwnershipValues();
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
                return _isbuyable;
            }
            set
            {
                _isbuyable = value;
                NotifyPropertyChanged("IsBuyable");
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
                return _stocks;
            }
            set
            {
                _stocks = value;
                NotifyPropertyChanged("Stocks");
            }
        }

        public int StocksForSale
        {
            get
            {
                return _stocksforsale;
            }
            set
            {
                _stocksforsale = value;
                NotifyPropertyChanged("StocksForSale");
            }
        }

        #endregion

        //adds ownership to the airline

        #region Public Methods and Operators

        public void addOwnership(Airline airline, int shares)
        {
            AirlineHelpers.SetAirlineShares(Airline, airline, shares);

            StocksForSale -= shares;

            if (OwnershipAirlines.Any(o => o.Airline == airline))
            {
                AirlineSharesMVVM share = OwnershipAirlines.First(o => o.Airline == airline);

                share.Shares += shares;
                share.Percent = Convert.ToDouble(share.Shares) / Convert.ToDouble(Stocks) * 100;
            }
            else
            {
                double percent = Convert.ToDouble(shares) / Convert.ToDouble(Stocks) * 100;

                OwnershipAirlines.Add(new AirlineSharesMVVM(airline, shares, percent));
            }

            int humanShares = Airline.Shares.Count(s => s.Airline == GameObject.GetInstance().HumanAirline);

            double humanSharesPercent = Convert.ToDouble(humanShares) / Convert.ToDouble(Stocks) * 100;

            IsBuyable = !Airline.IsHuman && humanSharesPercent > 50;
        }

        public void setOwnershipValues()
        {
            OwnershipAirlines = new ObservableCollection<AirlineSharesMVVM>();

            IEnumerable<Airline> airlines =
                Airline.Shares.Where(s => s.Airline != null).Select(s => s.Airline).Distinct();

            foreach (Airline shareAirline in airlines)
            {
                int shares = Airline.Shares.Count(s => s.Airline == shareAirline);
                double percent = Convert.ToDouble(shares) / Convert.ToDouble(Stocks) * 100;

                OwnershipAirlines.Add(new AirlineSharesMVVM(shareAirline, shares, percent));
            }

            int humanShares = Airline.Shares.Count(s => s.Airline == GameObject.GetInstance().HumanAirline);

            double humanSharesPercent = Convert.ToDouble(humanShares) / Convert.ToDouble(Stocks) * 100;

            IsBuyable = !Airline.IsHuman && humanSharesPercent > 50;
        }

        #endregion

        #region Methods

        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
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
            Airline = airline;
            Shares = shares;
            Percent = percent;
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
                return _percent;
            }
            set
            {
                _percent = value;
                NotifyPropertyChanged("Percent");
            }
        }

        public int Shares
        {
            get
            {
                return _shares;
            }
            set
            {
                _shares = value;
                NotifyPropertyChanged("Shares");
            }
        }

        #endregion

        #region Methods

        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}