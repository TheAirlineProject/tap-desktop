using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.Model.GeneralModel.StatisticsModel;

namespace TheAirline.GUIModel.PagesModel.AirlinesPageModel
{
    //the mvvm object for an airline
    public class AirlinesMVVM : INotifyPropertyChanged
    {
        public Airline Airline { get; set; }
        public double Profit { get; set; }
        public double AvgFleetAge { get; set; }
        public double Passengers { get; set; }
        public double PassengersPerFlight { get; set; }
        public double Flights { get; set; }
        public double Cargo { get; set; }
        public double CargoPerFlight { get; set; }
        private int _stocks;
        public int Stocks 
        {
            get { return _stocks; }
            set { _stocks = value; NotifyPropertyChanged("Stocks"); }
        }
        private int _stocksforsale;
        public int StocksForSale 
        {
            get { return _stocksforsale; }
            set { _stocksforsale = value; NotifyPropertyChanged("StocksForSale"); }
        }
        public double StockPrice { get; set; }
        public ObservableCollection<AirlineSharesMVVM> OwnershipAirlines { get; set; }
        private Boolean _isbuyable;
        public Boolean IsBuyable
        {
            get { return _isbuyable; }
            set { _isbuyable = value; NotifyPropertyChanged("IsBuyable"); }
        }
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

            this.Passengers = this.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, passengersType);
            this.PassengersPerFlight = this.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, passengersAvgType);
            this.Flights = this.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, arrivalsType);
            this.Cargo = this.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, cargoType);
            this.CargoPerFlight = this.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, cargoAvgType);

            this.Stocks = this.Airline.Shares.Count;
            this.StocksForSale = this.Airline.Shares.Count(s => s.Airline == null);
            this.StockPrice = AirlineHelpers.GetPricePerAirlineShare(this.Airline);

            setOwnershipValues();
        }
        //sets the ownership values
        public void setOwnershipValues()
        {
            this.OwnershipAirlines = new ObservableCollection<AirlineSharesMVVM>();

            var airlines = this.Airline.Shares.Where(s => s.Airline != null).Select(s => s.Airline).Distinct();

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
        //adds ownership to the airline
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

                this.OwnershipAirlines.Add(new AirlineSharesMVVM(airline,shares,percent));

            }

            int humanShares = this.Airline.Shares.Count(s => s.Airline == GameObject.GetInstance().HumanAirline);

            double humanSharesPercent = Convert.ToDouble(humanShares) / Convert.ToDouble(this.Stocks) * 100;

            this.IsBuyable = !this.Airline.IsHuman && humanSharesPercent > 50;
   
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    //the mvvm class for the ownership percent in an airline
    public class AirlineSharesMVVM : INotifyPropertyChanged
    {
        public Airline Airline { get; set; }
        private int _shares;
        public int Shares 
        {
            get { return _shares; }
            set { _shares = value; NotifyPropertyChanged("Shares"); } 
        }
        private double _percent;
        public double Percent 
        {
            get { return _percent; }
            set { _percent = value; NotifyPropertyChanged("Percent"); } 
        }
        public AirlineSharesMVVM(Airline airline, int shares, double percent)
        {
            this.Airline = airline;
            this.Shares = shares;
            this.Percent = percent;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
