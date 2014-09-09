using TheAirline.Model.RouteModel;

namespace TheAirline.GUIModel.PagesModel.AirportsPageModel
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;
    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.AirportModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.GeneralModel.Helpers;
    using TheAirline.Model.GeneralModel.StatisticsModel;

    //the mvvm class for an airport
    public class AirportMVVM : INotifyPropertyChanged
    {
        #region Fields

        private Boolean _isHuman;

        private int _numberOfAirlines;

        private int _numberOfFreeGates;

        private int _numberOfRoutes;

        private double _distance;

        #endregion

        #region Constructors and Destructors

        public AirportMVVM(Airport airport)
        {
            this.Airport = airport;
            this.IsHuman = GameObject.GetInstance().HumanAirline.Airports.Contains(this.Airport);
            this.NumberOfFreeGates = this.Airport.Terminals.NumberOfFreeGates;
            this.NumberOfAirlines = this.Airport.AirlineContracts.Select(c => c.Airline).Distinct().Count();
            this.NumberOfRoutes = AirportHelpers.GetAirportRoutes(this.Airport).Count;
            this.LongestRunway = this.Airport.Runways.Count == 0 ? 0 : this.Airport.Runways.Max(r => r.Length);
            this.HasCargoTerminal = this.Airport.Terminals.AirportTerminals.Exists(t => t.Type == Terminal.TerminalType.Cargo);
            this.HasHelipad = this.Airport.Runways.Exists(r => r.Type == Runway.RunwayType.Helipad);
            this.HasFreeGates = this.Airport.Terminals.getFreeGates(GameObject.GetInstance().HumanAirline.AirlineRouteFocus == Route.RouteType.Cargo ? Terminal.TerminalType.Cargo : Terminal.TerminalType.Passenger) > 0;
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public Boolean HasCargoTerminal { get; set; }

        public Boolean HasHelipad { get; set; }

        public Airport Airport { get; set; }

        public Boolean HasFreeGates { get; set; }

        public double Distance 
        {
            get
            {
                return this._distance;
            }
            set
            {
                this._distance = value;
                this.NotifyPropertyChanged("Distance");
            }
        }

        public Boolean IsHuman
        {
            get
            {
                return this._isHuman;
            }
            set
            {
                this._isHuman = value;
                this.NotifyPropertyChanged("IsHuman");
            }
        }

        public long LongestRunway { get; set; }

        public int NumberOfAirlines
        {
            get
            {
                return this._numberOfAirlines;
            }
            set
            {
                this._numberOfAirlines = value;
                this.NotifyPropertyChanged("NumberOfAirlines");
            }
        }

        public int NumberOfFreeGates
        {
            get
            {
                return this._numberOfFreeGates;
            }
            set
            {
                this._numberOfFreeGates = value;
                this.NotifyPropertyChanged("NumberOfFreeGates");
            }
        }

        public int NumberOfRoutes
        {
            get
            {
                return this._numberOfRoutes;
            }
            set
            {
                this._numberOfRoutes = value;
                this.NotifyPropertyChanged("NumberOfRoutes");
            }
        }

        #endregion

        #region Public Methods and Operators

        public void addAirlineContract(AirportContract contract)
        {
            AirportHelpers.AddAirlineContract(contract);

            this.IsHuman = GameObject.GetInstance().HumanAirline.Airports.Contains(this.Airport);
            this.NumberOfFreeGates = this.Airport.Terminals.NumberOfFreeGates;
            this.NumberOfAirlines = this.Airport.AirlineContracts.Select(c => c.Airline).Distinct().Count();
            this.NumberOfRoutes = AirportHelpers.GetAirportRoutes(this.Airport).Count;
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
    //the converter for the airports statistics
    public class AirportStatisticsConverter : IValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var airport = (Airport)value;
            StatisticsType statType = StatisticsTypes.GetStatisticsType("Passengers");

            return airport.Statistics.GetTotalValue(GameObject.GetInstance().GameTime.Year, statType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
    //the converter for the airport distance converter
    public class AirportDistanceConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] != null && values[1] != null)
            {
                var humanAirport = (Airport)values[0];
                var airport = (AirportMVVM)values[1];

                if (humanAirport != null && airport != null)
                {
                    airport.Distance = MathHelpers.GetDistance(humanAirport, airport.Airport);

                    return new DistanceToUnitConverter().Convert(MathHelpers.GetDistance(humanAirport, airport.Airport));
                }
            }
            return 0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}