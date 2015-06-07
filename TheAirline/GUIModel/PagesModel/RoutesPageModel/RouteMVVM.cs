using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using TheAirline.General.Models.Countries;
using TheAirline.Helpers;
using TheAirline.Models.Airliners;
using TheAirline.Models.General;
using TheAirline.Models.General.Countries;
using TheAirline.Models.General.Finances;
using TheAirline.Models.General.Statistics;
using TheAirline.Models.Routes;

namespace TheAirline.GUIModel.PagesModel.RoutesPageModel
{
    //the mvvm object for a human route
    public class HumanRouteMVVM
    {
        #region Constructors and Destructors

        public HumanRouteMVVM(Route route)
        {
            Route = route;
            ShowCargoInformation = Route.Type == Route.RouteType.Cargo
                                        || Route.Type == Route.RouteType.Mixed;
            ShowPassengersInformation = Route.Type == Route.RouteType.Passenger
                                             || Route.Type == Route.RouteType.Mixed || Route.Type == Route.RouteType.Helicopter;

            IsEditable = true;
            // !this.Route.getAirliners().Exists(a => a.Status != FleetAirliner.AirlinerStatus.Stopped);

            Invoices = new List<MonthlyInvoice>();

            foreach (Invoice.InvoiceType type in Route.GetRouteInvoiceTypes())
            {
                Invoices.Add(new MonthlyInvoice(type, 1950, 1,1, Route.GetRouteInvoiceAmount(type)));
            }

            Legs = new List<Route>();
            Legs.Add(Route);
            Legs.AddRange(Route.Stopovers.SelectMany(s => s.Legs));

            Distance = MathHelpers.GetDistance(Route.Destination1, Route.Destination2);

            setFeedback();
        }
        //sets the feedback values
        public void setFeedback()
        {
            FeedbackTypes = new Dictionary<string, List<string>>();

            FeedbackTypes.Add("plane", new List<string> { "1000", "1001", "1002" });
            FeedbackTypes.Add("age", new List<string> { "1003", "1004", "1005" });
            FeedbackTypes.Add("food", new List<string> { "1006", "1005", "1008" });
            FeedbackTypes.Add("seats", new List<string> { "1009", "1010", "1011" });
            FeedbackTypes.Add("inflight", new List<string> { "1012", "1013", "1014" });
            FeedbackTypes.Add("wifi", new List<string> { "1015", "1016", "1017" });
            FeedbackTypes.Add("price", new List<string> { "1018", "1019", "1020" });
            FeedbackTypes.Add("score", new List<string> { "1021", "1022", "1023" });
            FeedbackTypes.Add("luggage",new List<string> {"1024","1025","1026"});
            
            Feedbacks = new List<RouteFeedbackMVVM>();

            if (Route.HasAirliner && Route.Type == Route.RouteType.Passenger)
            {
                double routeScore = RouteHelpers.GetRouteTotalScore(Route);
                double priceScore = RouteHelpers.GetRoutePriceScore(Route);
                double planeTypeScore = RouteHelpers.GetRoutePlaneTypeScore(Route);
                double ageScore = RouteHelpers.GetPlaneAgeScore(Route);
                double foodScore = RouteHelpers.GetRouteMealScore(Route);
                double seatsScore = RouteHelpers.GetRouteSeatsScore(Route);
                double inflightScore = RouteHelpers.GetRouteInflightScore(Route);
                double luggageScore = RouteHelpers.GetRouteLuggageScore(Route);

                string priceText = priceScore < 4 ? "High" : ((priceScore >= 4 && priceScore < 7) ? "Medium" : "Low");
                string bagText = luggageScore >= 7 ? "Free Checked Bag" : "Checked Bag Fee";

                RouteFacility wifiFacility = ((PassengerRoute)Route).GetRouteAirlinerClass(AirlinerClass.ClassType.EconomyClass).GetFacility(RouteFacility.FacilityType.WiFi);
                RouteFacility foodFacility = ((PassengerRoute)Route).GetRouteAirlinerClass(AirlinerClass.ClassType.EconomyClass).GetFacility(RouteFacility.FacilityType.Food);
                AirlinerFacility seats = Route.GetAirliners()[0].Airliner.GetAirlinerClass(AirlinerClass.ClassType.EconomyClass).GetFacility(AirlinerFacility.FacilityType.Seat);
                AirlinerFacility videoFacility = Route.GetAirliners()[0].Airliner.GetAirlinerClass(AirlinerClass.ClassType.EconomyClass).GetFacility(AirlinerFacility.FacilityType.Video);

                Feedbacks.Add(new RouteFeedbackMVVM(Route.GetAirliners()[0].Airliner.Type.Name, "plane-feedback.png", planeTypeScore, getFeedbackText("plane", planeTypeScore)));
                Feedbacks.Add(new RouteFeedbackMVVM(string.Format("{0} year(s) old", Route.GetAirliners()[0].Airliner.Age), "age.png", ageScore, getFeedbackText("age", ageScore)));
                Feedbacks.Add(new RouteFeedbackMVVM(foodFacility.Name, "food.png", foodScore, getFeedbackText("food", foodScore)));
                Feedbacks.Add(new RouteFeedbackMVVM(seats.Name, "seats.png", seatsScore, getFeedbackText("seats", seatsScore)));
                Feedbacks.Add(new RouteFeedbackMVVM(videoFacility.Name, "tv.png", inflightScore, getFeedbackText("inflight", inflightScore)));
                Feedbacks.Add(new RouteFeedbackMVVM(bagText,"luggage.png",luggageScore,getFeedbackText("luggage",luggageScore)));

                if ((int)RouteFacility.FacilityType.WiFi <= GameObject.GetInstance().GameTime.Year)
                {
                    double wifiScore = RouteHelpers.GetRouteWifiScore(Route);
                    Feedbacks.Add(new RouteFeedbackMVVM(wifiFacility.Name, "wifi.png", wifiScore, getFeedbackText("wifi", wifiScore)));
                }


                Feedbacks.Add(new RouteFeedbackMVVM(priceText, "price.png", priceScore, getFeedbackText("price", priceScore)));
                Feedbacks.Add(new RouteFeedbackMVVM(String.Format("{0:0.0}", routeScore), "number.png", routeScore, getFeedbackText("score", routeScore)));
            }
            else
            {
                string nodataString = Translator.GetInstance().GetString("RouteFeedback", "1100");
                Feedbacks.Add(new RouteFeedbackMVVM("-", "plane-feedback.png", 5, nodataString));
                Feedbacks.Add(new RouteFeedbackMVVM("-", "age.png", 5, nodataString));
                Feedbacks.Add(new RouteFeedbackMVVM("-", "food.png", 5, nodataString));
                Feedbacks.Add(new RouteFeedbackMVVM("-", "seats.png", 5, nodataString));
                Feedbacks.Add(new RouteFeedbackMVVM("-", "tv.png", 5, nodataString));
                Feedbacks.Add(new RouteFeedbackMVVM("-", "luggage.png", 5, nodataString));


                if ((int)RouteFacility.FacilityType.WiFi <= GameObject.GetInstance().GameTime.Year)
                {
                    Feedbacks.Add(new RouteFeedbackMVVM("-", "wifi.png", 5, nodataString));
                }

                Feedbacks.Add(new RouteFeedbackMVVM("-", "price.png", 5, nodataString));
                Feedbacks.Add(new RouteFeedbackMVVM("-", "number.png", 5, nodataString));
            }


        }
        private string getFeedbackText(string type, double score)
        {
            int index = 0;

            if (score < 4)
                index = 0;
            if (score >= 4 && score < 7)
                index = 1;
            if (score >= 7)
                index = 2;

            string uid = FeedbackTypes[type][index];

            return Translator.GetInstance().GetString("RouteFeedback", uid);

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

        public List<RouteFeedbackMVVM> Feedbacks { get; set; }

        public Dictionary<string, List<string>> FeedbackTypes;

        #endregion
    }

    //the mvvm object for a route
    public class RouteMVVM
    {
        #region Constructors and Destructors

        public RouteMVVM(Route route)
        {
            Route = route;
            FillingDegree = Route.FillingDegree;
            Balance = Route.Balance;
            Distance = MathHelpers.GetDistance(Route.Destination1, Route.Destination2);

            if (route.Type == Route.RouteType.Passenger || route.Type == Route.RouteType.Helicopter)
            {
                RouteAirlinerClass raClass =
                    ((PassengerRoute)route).GetRouteAirlinerClass(AirlinerClass.ClassType.EconomyClass);

                Total = route.Statistics.GetStatisticsValue(
                    raClass,
                    StatisticsTypes.GetStatisticsType("Passengers"));
                Average = route.Statistics.GetStatisticsValue(
                    raClass,
                    StatisticsTypes.GetStatisticsType("Passengers%"));
            }
            if (route.Type == Route.RouteType.Cargo)
            {
                Total = route.Statistics.GetStatisticsValue(StatisticsTypes.GetStatisticsType("Cargo"));
                Average = route.Statistics.GetStatisticsValue(StatisticsTypes.GetStatisticsType("Cargo%"));
            }

            if (Average < 0)
            {
                Average = 0;
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
    //the mvvm object for a route feedback
    public class RouteFeedbackMVVM
    {
        public string Text { get; set; }
        public string Image { get; set; }
        public double Value { get; set; }
        public string Feedback { get; set; }
        public RouteFeedbackMVVM(string text, string image, double value, string feedback)
        {
            Text = text;
            Image = "/Data/images/" + image;
            Value = value;
            Feedback = feedback;
        }
    }
    //the mvvm object for a special contract
    public class SpecialContractMVVM
    {
        public SpecialContract Contract { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public SpecialContractMVVM(SpecialContract contract, DateTime startdate, DateTime enddate)
        {
            Contract = contract;
            StartDate = startdate;
            EndDate = enddate;
        }
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
            Airliner = airliner;
            HasRoute = Airliner.HasRoute;
            Status = Airliner.Status == FleetAirliner.AirlinerStatus.Stopped
                ? StatusMVVM.Stopped
                : StatusMVVM.Started;

            if (Airliner.Status == FleetAirliner.AirlinerStatus.OnCharter)
                Status = StatusMVVM.Charter;

            Routes = new ObservableCollection<Route>();

            foreach (Route route in Airliner.Routes)
                Routes.Add(route);
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

            Started,

            Charter
        }

        #endregion

        #region Public Properties

        public FleetAirliner Airliner { get; set; }

        public Boolean HasRoute
        {
            get
            {
                return _hasroute;
            }
            set
            {
                _hasroute = value;
                NotifyPropertyChanged("HasRoute");
            }
        }

        public StatusMVVM Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                NotifyPropertyChanged("Status");
            }
        }
        public ObservableCollection<Route> Routes { get; set; }

        #endregion

        #region Public Methods and Operators

        public void setStatus(FleetAirliner.AirlinerStatus status)
        {
            Airliner.Status = status;

            Status = Airliner.Status == FleetAirliner.AirlinerStatus.Stopped
                ? StatusMVVM.Stopped
                : StatusMVVM.Started;
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

    //the pax/income for a route
    public class RouteIncomePerPaxMVVM
    {
        #region Constructors and Destructors

        public RouteIncomePerPaxMVVM(Route route)
        {
            StatisticsType stat = StatisticsTypes.GetStatisticsType("Passengers");

            Route = route;

            double pax = Route.Statistics.GetStatisticsValue(stat);

            IncomePerPax = Route.Balance / pax;
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
            Route = route;

            double balance = Route.Balance;

            Percent = balance / total * 100;
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

        public MVVMRouteFacility(RouteFacility.FacilityType type, List<RouteFacility> facilities)
        {
            Facilities = new ObservableCollection<RouteFacility>();

            Type = type;

            foreach (RouteFacility facility in facilities)
                Facilities.Add(facility);

            SelectedFacility = Facilities.OrderBy(f => f.ServiceLevel).First();
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
                return _selectedFacility;
            }
            set
            {
                _selectedFacility = value;
                NotifyPropertyChanged("SelectedFacility");
            }
        }

        public RouteFacility.FacilityType Type { get; set; }

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

    //the route class
    public class MVVMRouteClass : INotifyPropertyChanged
    {
        #region Fields

        private double _fareprice;

        private Boolean _isuseable;

        #endregion

        //public int CabinCrew { get; set; }

        #region Constructors and Destructors

        public MVVMRouteClass(AirlinerClass.ClassType type, RouteAirlinerClass.SeatingType seating, double fareprice)
        {
            Type = type;
            Seating = seating;
            FarePrice = fareprice;

            Facilities = new ObservableCollection<MVVMRouteFacility>();

            foreach (RouteFacility.FacilityType facType in Enum.GetValues(typeof(RouteFacility.FacilityType)))
            {

                if (GameObject.GetInstance().GameTime.Year >= (int)facType)
                {
                    var facs = new List<RouteFacility>();
                    foreach (RouteFacility fac in RouteFacilities.GetFacilities(facType))
                    {
                        facs.Add(fac);
                    }

                    var facility = new MVVMRouteFacility(facType, facs);

                    Facilities.Add(facility);
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
                return _fareprice;
            }
            set
            {
                _fareprice = value;
                NotifyPropertyChanged("FarePrice");
            }
        }

        public Boolean IsUseable
        {
            get
            {
                return _isuseable;
            }
            set
            {
                _isuseable = value;
                NotifyPropertyChanged("IsUseable");
            }
        }

        public RouteAirlinerClass.SeatingType Seating { get; set; }

        public AirlinerClass.ClassType Type { get; set; }

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
                    ((PassengerRoute)route).GetRouteAirlinerClass(AirlinerClass.ClassType.EconomyClass);

                double passengers = route.Statistics.GetStatisticsValue(
                    raClass,
                    StatisticsTypes.GetStatisticsType("Passengers"));
                double avgPassengers = route.Statistics.GetStatisticsValue(
                    raClass,
                    StatisticsTypes.GetStatisticsType("Passengers%"));

                stats.Add(new KeyValuePair<string, object>("Total Passengers", passengers));
                stats.Add(new KeyValuePair<string, object>("Average Passengers", Math.Max(0, avgPassengers)));
            }
            if (route.Type == Route.RouteType.Cargo)
            {
                double cargo = route.Statistics.GetStatisticsValue(StatisticsTypes.GetStatisticsType("Cargo"));
                double avgCargo = route.Statistics.GetStatisticsValue(StatisticsTypes.GetStatisticsType("Cargo%"));

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
    //the conver for the feedback color
    public class FeedbackColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double score = Double.Parse(value.ToString());

            if (score < 4)
                return Brushes.DarkRed;
            if (score >= 4 && score < 7)
                return new SolidColorBrush(Color.FromRgb(58, 66, 89));
            if (score >= 7)
                return new SolidColorBrush(Color.FromRgb(0, 74, 127));

            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}