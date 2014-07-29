namespace TheAirline.GUIModel.PagesModel.RoutesPageModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;
    using TheAirline.Model.AirlinerModel;
    using TheAirline.Model.AirlinerModel.RouteModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.GeneralModel.CountryModel;
    using TheAirline.Model.GeneralModel.InvoicesModel;
    using TheAirline.Model.GeneralModel.StatisticsModel;

    //the mvvm object for a human route
    public class HumanRouteMVVM
    {
        #region Constructors and Destructors

        public HumanRouteMVVM(Route route)
        {
            this.Route = route;
            this.ShowCargoInformation = this.Route.Type == Route.RouteType.Cargo
                                        || this.Route.Type == Route.RouteType.Mixed;
            this.ShowPassengersInformation = this.Route.Type == Route.RouteType.Passenger
                                             || this.Route.Type == Route.RouteType.Mixed || this.Route.Type == Model.AirlinerModel.RouteModel.Route.RouteType.Helicopter;

            this.IsEditable = true;
                // !this.Route.getAirliners().Exists(a => a.Status != FleetAirliner.AirlinerStatus.Stopped);

            this.Invoices = new List<MonthlyInvoice>();

            foreach (Invoice.InvoiceType type in this.Route.getRouteInvoiceTypes())
            {
                this.Invoices.Add(new MonthlyInvoice(type, 1950, 1, this.Route.getRouteInvoiceAmount(type)));
            }

            this.Legs = new List<Route>();
            this.Legs.Add(this.Route);
            this.Legs.AddRange(this.Route.Stopovers.SelectMany(s => s.Legs));

            this.Distance = MathHelpers.GetDistance(this.Route.Destination1, this.Route.Destination2);
        }

        #endregion

        #region Public Properties

        public double Distance { get; set; }

        public List<MonthlyInvoice> Invoices { get; set; }

        public Boolean IsEditable { get; set; }

        public List<Route> Legs { get; set; }

        public Route Route { get; set; }

        public Boolean ShowCargoInformation { get; set; }

        public Boolean ShowPassengersInformation { get; set; }

        #endregion
    }

    //the mvvm object for a route
    public class RouteMVVM
    {
        #region Constructors and Destructors

        public RouteMVVM(Route route)
        {
            this.Route = route;
            this.FillingDegree = this.Route.FillingDegree;
            this.Balance = this.Route.Balance;
            this.Distance = MathHelpers.GetDistance(this.Route.Destination1, this.Route.Destination2);

            if (route.Type == Route.RouteType.Passenger || route.Type == Model.AirlinerModel.RouteModel.Route.RouteType.Helicopter)
            {
                RouteAirlinerClass raClass =
                    ((PassengerRoute)route).getRouteAirlinerClass(AirlinerClass.ClassType.Economy_Class);

                this.Total = route.Statistics.getStatisticsValue(
                    raClass,
                    StatisticsTypes.GetStatisticsType("Passengers"));
                this.Average = route.Statistics.getStatisticsValue(
                    raClass,
                    StatisticsTypes.GetStatisticsType("Passengers%"));
            }
            if (route.Type == Route.RouteType.Cargo)
            {
                this.Total = route.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Cargo"));
                this.Average = route.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Cargo%"));
            }

            if (this.Average < 0)
            {
                this.Average = 0;
            }
        }

        #endregion

        #region Public Properties

        public double Average { get; set; }

        public double Balance { get; set; }

        public double Distance { get; set; }

        public double FillingDegree { get; set; }

        public Route Route { get; set; }

        public double Total { get; set; }

        #endregion
    }

    //the mvvm object for an airliner
    public class FleetAirlinerMVVM : INotifyPropertyChanged
    {
        #region Fields

        private StatusMVVM _status;
        private Boolean _hasroute;

        #endregion

        #region Constructors and Destructors

        public FleetAirlinerMVVM(FleetAirliner airliner)
        {
            this.Airliner = airliner;
            this.HasRoute = this.Airliner.HasRoute;
            this.Status = this.Airliner.Status == FleetAirliner.AirlinerStatus.Stopped
                ? StatusMVVM.Stopped
                : StatusMVVM.Started;

            this.Routes = new ObservableCollection<Route>();

            foreach (Route route in this.Airliner.Routes)
                this.Routes.Add(route);
        }

        #endregion

        //sets the status of the airliner

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Enums

        public enum StatusMVVM
        {
            Stopped,

            Started
        }

        #endregion

        #region Public Properties

        public FleetAirliner Airliner { get; set; }

        public Boolean HasRoute 
        {
            get
            {
                return this._hasroute;
            }
            set
            {
               this._hasroute = value ;
               this.NotifyPropertyChanged("HasRoute");
            }
        }

        public StatusMVVM Status
        {
            get
            {
                return this._status;
            }
            set
            {
                this._status = value;
                this.NotifyPropertyChanged("Status");
            }
        }
        public ObservableCollection<Route> Routes { get; set; }

        #endregion

        #region Public Methods and Operators

        public void setStatus(FleetAirliner.AirlinerStatus status)
        {
            this.Airliner.Status = status;

            this.Status = this.Airliner.Status == FleetAirliner.AirlinerStatus.Stopped
                ? StatusMVVM.Stopped
                : StatusMVVM.Started;
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

    //the pax/income for a route
    public class RouteIncomePerPaxMVVM
    {
        #region Constructors and Destructors

        public RouteIncomePerPaxMVVM(Route route)
        {
            StatisticsType stat = StatisticsTypes.GetStatisticsType("Passengers");

            this.Route = route;

            double pax = this.Route.Statistics.getStatisticsValue(stat);

            this.IncomePerPax = this.Route.Balance / pax;
        }

        #endregion

        #region Public Properties

        public double IncomePerPax { get; set; }

        public Route Route { get; set; }

        #endregion
    }

    //the profit for a route
    public class RouteProfitMVVM
    {
        #region Constructors and Destructors

        public RouteProfitMVVM(Route route, double total)
        {
            this.Route = route;

            double balance = this.Route.Balance;

            this.Percent = balance / total * 100;
        }

        #endregion

        #region Public Properties

        public double Percent { get; set; }

        public Route Route { get; set; }

        #endregion
    }

    //the facilities for a route
    public class MVVMRouteFacility : INotifyPropertyChanged
    {
        #region Fields

        private RouteFacility _selectedFacility;

        #endregion

        #region Constructors and Destructors

        public MVVMRouteFacility(RouteFacility.FacilityType type)
        {
            this.Facilities = new ObservableCollection<RouteFacility>();

            this.Type = type;
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public ObservableCollection<RouteFacility> Facilities { get; set; }

        public RouteFacility SelectedFacility
        {
            get
            {
                return this._selectedFacility;
            }
            set
            {
                this._selectedFacility = value;
                this.NotifyPropertyChanged("SelectedFacility");
            }
        }

        public RouteFacility.FacilityType Type { get; set; }

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

    //the route class
    public class MVVMRouteClass : INotifyPropertyChanged
    {
        #region Fields

        private double _fareprice;

        #endregion

        //public int CabinCrew { get; set; }

        #region Constructors and Destructors

        public MVVMRouteClass(AirlinerClass.ClassType type, RouteAirlinerClass.SeatingType seating, double fareprice)
        {
            this.Type = type;
            this.Seating = seating;
            this.FarePrice = fareprice;

            this.Facilities = new ObservableCollection<MVVMRouteFacility>();

            foreach (RouteFacility.FacilityType facType in Enum.GetValues(typeof(RouteFacility.FacilityType)))
            {
                if (GameObject.GetInstance().GameTime.Year >= (int)facType)
                {
                    var facility = new MVVMRouteFacility(facType);

                    foreach (RouteFacility fac in RouteFacilities.GetFacilities(facType))
                    {
                        facility.Facilities.Add(fac);
                    }

                    this.Facilities.Add(facility);
                }
            }
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public ObservableCollection<MVVMRouteFacility> Facilities { get; set; }

        public double FarePrice
        {
            get
            {
                return this._fareprice;
            }
            set
            {
                this._fareprice = value;
                this.NotifyPropertyChanged("FarePrice");
            }
        }

        public RouteAirlinerClass.SeatingType Seating { get; set; }

        public AirlinerClass.ClassType Type { get; set; }

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

    //the converter for the statistics for a route
    public class RouteStatisticsConverter : IValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var route = (Route)value;
            var stats = new List<KeyValuePair<string, object>>();

            if (route.Type == Route.RouteType.Passenger || route.Type == Route.RouteType.Helicopter)
            {
                RouteAirlinerClass raClass =
                    ((PassengerRoute)route).getRouteAirlinerClass(AirlinerClass.ClassType.Economy_Class);

                double passengers = route.Statistics.getStatisticsValue(
                    raClass,
                    StatisticsTypes.GetStatisticsType("Passengers"));
                double avgPassengers = route.Statistics.getStatisticsValue(
                    raClass,
                    StatisticsTypes.GetStatisticsType("Passengers%"));

                stats.Add(new KeyValuePair<string, object>("Total Passengers", passengers));
                stats.Add(new KeyValuePair<string, object>("Average Passengers", Math.Max(0, avgPassengers)));
            }
            if (route.Type == Route.RouteType.Cargo)
            {
                double cargo = route.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Cargo"));
                double avgCargo = route.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Cargo%"));

                stats.Add(new KeyValuePair<string, object>("Total Cargo", cargo));
                stats.Add(new KeyValuePair<string, object>("Average Cargo", Math.Max(0, avgCargo)));
            }

            stats.Add(
                new KeyValuePair<string, object>(
                    "Filling Degree",
                    string.Format("{0:0.##} %", route.FillingDegree * 100)));

            return stats;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    //the converter for an unions member
    public class UnionMemberConverter : IValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var unit = (BaseUnit)value;

            if (unit is Union)
            {
                IEnumerable<Country> countries =
                    ((Union)unit).Members.Where(
                        m =>
                            GameObject.GetInstance().GameTime >= m.MemberFromDate
                            && GameObject.GetInstance().GameTime <= m.MemberToDate).Select(m => m.Country);

                return countries;
            }
            else
            {
                var countries = new List<Country>();

                countries.Add((Country)unit);

                return countries;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
   
}