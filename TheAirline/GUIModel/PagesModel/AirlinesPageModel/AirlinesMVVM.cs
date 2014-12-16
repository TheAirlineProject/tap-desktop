namespace TheAirline.GUIModel.PagesModel.AirlinesPageModel
{
    using De.TorstenMandelkow.MetroChart;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Media;
    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.AirlineModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.GeneralModel.Helpers;
    using TheAirline.Model.GeneralModel.StatisticsModel;

    //the mvvm object for an airline
    public class AirlinesMVVM : INotifyPropertyChanged
    {
        #region Fields

        private Boolean _isbuyable;

        private Boolean _isstocksbuyable;

        private Boolean _isstockssellable;

        private List<KeyValuePair<string, int>> _stocks;


        #endregion

        #region Constructors and Destructors

        public AirlinesMVVM(Airline airline)
        {
            this.Airline = airline;
            this.Profit = this.Airline.getProfit();
            this.AvgFleetAge = this.Airline.getAverageFleetAge();

            StatisticsType passengersType = StatisticsTypes.GetStatisticsType("Passengers");
            StatisticsType passengersAvgType = StatisticsTypes.GetStatisticsType("Passengers%");
            StatisticsType arrivalsType = StatisticsTypes.GetStatisticsType("Arrivals");
            StatisticsType cargoType = StatisticsTypes.GetStatisticsType("Cargo");
            StatisticsType cargoAvgType = StatisticsTypes.GetStatisticsType("Cargo%");

            this.Passengers = this.Airline.Statistics.getStatisticsValue(
                GameObject.GetInstance().GameTime.Year,
                passengersType);
            this.PassengersPerFlight = this.Airline.Statistics.getStatisticsValue(
                GameObject.GetInstance().GameTime.Year,
                passengersAvgType);
            this.Flights = this.Airline.Statistics.getStatisticsValue(
                GameObject.GetInstance().GameTime.Year,
                arrivalsType);
            this.Cargo = this.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, cargoType);
            this.CargoPerFlight = this.Airline.Statistics.getStatisticsValue(
                GameObject.GetInstance().GameTime.Year,
                cargoAvgType);

        

            this.StockPrice = AirlineHelpers.GetPricePerAirlineShare(this.Airline);

            setBuyNSellable(); 
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

       
        public double Passengers { get; set; }

        public double PassengersPerFlight { get; set; }

        public double Profit { get; set; }

        public double StockPrice { get; set; }

        public List<KeyValuePair<string, int>> Stocks
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
        public Boolean IsStocksBuyable
        {
            get
            {
                return this._isstocksbuyable;
            }
            set
            {
                this._isstocksbuyable = value;
                this.NotifyPropertyChanged("IsStocksBuyable");
            }
        }
        public Boolean IsStocksSellable
        {
            get
            {
                return this._isstockssellable;
            }
            set
            {
                this._isstockssellable = value;
                this.NotifyPropertyChanged("IsStocksSellable");
            }
        }

        #endregion

        //adds ownership to the airline

        #region Public Methods and Operators


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
        private void setBuyNSellable()
        {
            
            this.IsStocksSellable = this.Airline.Shares.FirstOrDefault(s => s.Airline == GameObject.GetInstance().HumanAirline) != null;
        
            var shareForSale = this.Airline.Shares.FirstOrDefault(s=>s.ForSale);

            double stocksPrice = 300000 * this.StockPrice;
            double money = GameObject.GetInstance().HumanAirline.Money;

            this.IsStocksBuyable = shareForSale != null && GameObject.GetInstance().HumanAirline.Money > 300000 * this.StockPrice;

            List<KeyValuePair<string, int>> stocks = new List<KeyValuePair<string, int>>();

            var airlines = this.Airline.Shares.Where(s => s.Airline != null).Select(s => s.Airline).Distinct();

            foreach (Airline sAirline in airlines)
            {
                int shares = this.Airline.Shares.Count(s => s.Airline == sAirline);

                stocks.Add(new KeyValuePair<string, int>(sAirline.Profile.Name, shares));
            }

            int nulls = this.Airline.Shares.Count(s => s.Airline == null);

            stocks.Add(new KeyValuePair<string, int>("Free", nulls));

            this.Stocks = stocks;

            int humanStocks = this.Airline.Shares.Count(s => s.Airline == GameObject.GetInstance().HumanAirline);

            double humanStocksPercent = Convert.ToDouble(humanStocks) / Convert.ToDouble(this.Airline.Shares.Count());

            this.IsBuyable = humanStocksPercent >= 0.50  && !this.Airline.IsHuman;
        }
        public void purchaseShares()
        {
            var share = this.Airline.Shares.First(s => s.Airline == null);
            share.Airline = GameObject.GetInstance().HumanAirline;
            share.ForSale = false;
       
            setBuyNSellable();

        }
        public void sellShares()
        {
            var share = this.Airline.Shares.First(s => s.Airline == GameObject.GetInstance().HumanAirline);
            share.Airline = null;
            share.ForSale = true;

            setBuyNSellable();
        }
        #endregion
    }

   
    //the converter for the colors
    public class PieChartColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            List<KeyValuePair<string, int>> airlines = (List<KeyValuePair<string, int>>)value;

            ResourceDictionaryCollection collection = new ResourceDictionaryCollection();

            int i = 1;
            foreach (KeyValuePair<string, int> airline in airlines)
            {
                if (airline.Key != "Free")
                {
                    Airline sAirline = Airlines.GetAirlines(a => a.Profile.Name == airline.Key).FirstOrDefault();
                    ResourceDictionary rd = new ResourceDictionary();

                    rd.Add("Brush" + i, new StringToBrushConverter().Convert(sAirline.Profile.Color, null, null, null) as SolidColorBrush);
                    collection.Add(rd);

                    i++;
                }

            }

            ResourceDictionary rdFree = new ResourceDictionary();
      
            rdFree.Add("Brush" + i, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5B9BD5")));

            collection.Add(rdFree);

            return collection;

      

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}