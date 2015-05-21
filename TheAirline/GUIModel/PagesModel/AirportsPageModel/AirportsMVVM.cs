using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Helpers;
using TheAirline.Models.Airports;
using TheAirline.Models.General;
using TheAirline.Models.General.Statistics;
using TheAirline.Models.Routes;

namespace TheAirline.GUIModel.PagesModel.AirportsPageModel
{
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
            Airport = airport;
            IsHuman = GameObject.GetInstance().HumanAirline.Airports.Contains(Airport);
            NumberOfFreeGates = Airport.Terminals.NumberOfFreeGates;
            NumberOfAirlines = Airport.AirlineContracts.Select(c => c.Airline).Distinct().Count();
            NumberOfRoutes = AirportHelpers.GetAirportRoutes(Airport).Count;
            LongestRunway = Airport.Runways.Count == 0 ? 0 : Airport.Runways.Max(r => r.Length);
            HasCargoTerminal = Airport.Terminals.AirportTerminals.Exists(t => t.Type == Terminal.TerminalType.Cargo);
            HasHelipad = Airport.Runways.Exists(r => r.Type == Runway.RunwayType.Helipad);
            HasFreeGates = Airport.Terminals.GetFreeGates(GameObject.GetInstance().HumanAirline.AirlineRouteFocus == Route.RouteType.Cargo ? Terminal.TerminalType.Cargo : Terminal.TerminalType.Passenger) > 0;
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
                return _distance;
            }
            set
            {
                _distance = value;
                NotifyPropertyChanged("Distance");
            }
        }

        public Boolean IsHuman
        {
            get
            {
                return _isHuman;
            }
            set
            {
                _isHuman = value;
                NotifyPropertyChanged("IsHuman");
            }
        }

        public long LongestRunway { get; set; }

        public int NumberOfAirlines
        {
            get
            {
                return _numberOfAirlines;
            }
            set
            {
                _numberOfAirlines = value;
                NotifyPropertyChanged("NumberOfAirlines");
            }
        }

        public int NumberOfFreeGates
        {
            get
            {
                return _numberOfFreeGates;
            }
            set
            {
                _numberOfFreeGates = value;
                NotifyPropertyChanged("NumberOfFreeGates");
            }
        }

        public int NumberOfRoutes
        {
            get
            {
                return _numberOfRoutes;
            }
            set
            {
                _numberOfRoutes = value;
                NotifyPropertyChanged("NumberOfRoutes");
            }
        }

        #endregion

        #region Public Methods and Operators

        public void addAirlineContract(AirportContract contract)
        {
            AirportHelpers.AddAirlineContract(contract);

            IsHuman = GameObject.GetInstance().HumanAirline.Airports.Contains(Airport);
            NumberOfFreeGates = Airport.Terminals.NumberOfFreeGates;
            NumberOfAirlines = Airport.AirlineContracts.Select(c => c.Airline).Distinct().Count();
            NumberOfRoutes = AirportHelpers.GetAirportRoutes(Airport).Count;
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