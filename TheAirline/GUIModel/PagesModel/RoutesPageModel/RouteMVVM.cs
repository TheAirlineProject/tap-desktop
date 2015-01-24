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
using System.Windows.Media;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.CountryModel;
using TheAirline.Model.GeneralModel.Helpers;
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
                                             || this.Route.Type == Route.RouteType.Mixed;

            this.IsEditable = true;
            // !this.Route.getAirliners().Exists(a => a.Status != FleetAirliner.AirlinerStatus.Stopped);

            this.Invoices = new ObservableCollection<MonthlyInvoice>();

            foreach (Invoice.InvoiceType type in this.Route.getRouteInvoiceTypes())
            {
                this.Invoices.Add(new MonthlyInvoice(type, 1950, 1,1, this.Route.getRouteInvoiceAmount(type)));
            }

            this.Legs = new ObservableCollection<Route>();
            this.Legs.Add(this.Route);

            foreach (Route sRoute in this.Route.Stopovers.SelectMany(s => s.Legs))
                this.Legs.Add(sRoute);

            this.Distance = MathHelpers.GetDistance(this.Route.Destination1, this.Route.Destination2);

            this.ForHelicopters = this.Route.Destination1.Runways.Exists(r=>r.Type == Model.AirportModel.Runway.RunwayType.Helipad) && this.Route.Destination2.Runways.Exists(r=>r.Type == Model.AirportModel.Runway.RunwayType.Helipad) && !this.Route.Stopovers.Exists(s => !s.Stopover.Runways.Exists(r => r.Type == Model.AirportModel.Runway.RunwayType.Helipad));

            setFeedback();
        }
        //sets the feedback values
        public void setFeedback()
        {
            this.FeedbackTypes = new Dictionary<string, List<string>>();

            this.FeedbackTypes.Add("plane", new List<string>() { "1000", "1001", "1002" });
            this.FeedbackTypes.Add("age", new List<string>() { "1003", "1004", "1005" });
            this.FeedbackTypes.Add("food", new List<string>() { "1006", "1007", "1008" });
            this.FeedbackTypes.Add("seats", new List<string>() { "1009", "1010", "1011" });
            this.FeedbackTypes.Add("inflight", new List<string>() { "1012", "1013", "1014" });
            this.FeedbackTypes.Add("wifi", new List<string>() { "1015", "1016", "1017" });
            this.FeedbackTypes.Add("price", new List<string>() { "1018", "1019", "1020" });
            this.FeedbackTypes.Add("score", new List<string>() { "1021", "1022", "1023" });
            this.FeedbackTypes.Add("luggage",new List<string>() {"1024","1025","1026"});

            this.Feedbacks = new ObservableCollection<RouteFeedbackMVVM>();

            if (this.Route.HasAirliner && this.Route.Type == Route.RouteType.Passenger)
            {
                double routeScore = RouteHelpers.GetRouteTotalScore(this.Route);
                double priceScore = RouteHelpers.GetRoutePriceScore(this.Route);
                double planeTypeScore = RouteHelpers.GetRoutePlaneTypeScore(this.Route);
                double ageScore = RouteHelpers.GetPlaneAgeScore(this.Route);
                double foodScore = RouteHelpers.GetRouteMealScore(this.Route);
                double seatsScore = RouteHelpers.GetRouteSeatsScore(this.Route);
                double inflightScore = RouteHelpers.GetRouteInflightScore(this.Route);
                double luggageScore = RouteHelpers.GetRouteLuggageScore(this.Route);

                string priceText = priceScore < 4 ? "High" : ((priceScore >= 4 && priceScore < 7) ? "Medium" : "Low");
                string bagText = luggageScore >= 7 ? "Free Checked Bag" : "Checked Bag Fee";

                RouteFacility wifiFacility = ((PassengerRoute)this.Route).getRouteAirlinerClass(AirlinerClass.ClassType.Economy_Class).getFacility(RouteFacility.FacilityType.WiFi);
                RouteFacility foodFacility = ((PassengerRoute)this.Route).getRouteAirlinerClass(AirlinerClass.ClassType.Economy_Class).getFacility(RouteFacility.FacilityType.Food);
                AirlinerFacility seats = this.Route.getAirliners()[0].Airliner.getAirlinerClass(AirlinerClass.ClassType.Economy_Class).getFacility(AirlinerFacility.FacilityType.Seat);
                AirlinerFacility videoFacility = this.Route.getAirliners()[0].Airliner.getAirlinerClass(AirlinerClass.ClassType.Economy_Class).getFacility(AirlinerFacility.FacilityType.Video);

                this.Feedbacks.Add(new RouteFeedbackMVVM(this.Route.getAirliners()[0].Airliner.Type.Name, "plane-feedback.png", planeTypeScore, getFeedbackText("plane", planeTypeScore)));
                this.Feedbacks.Add(new RouteFeedbackMVVM(string.Format("{0} year(s) old", this.Route.getAirliners()[0].Airliner.Age), "age.png", ageScore, getFeedbackText("age", ageScore)));
                this.Feedbacks.Add(new RouteFeedbackMVVM(foodFacility.Name, "food.png", foodScore, getFeedbackText("food", foodScore)));
                this.Feedbacks.Add(new RouteFeedbackMVVM(seats.Name, "seats.png", seatsScore, getFeedbackText("seats", seatsScore)));
                this.Feedbacks.Add(new RouteFeedbackMVVM(videoFacility.Name, "tv.png", inflightScore, getFeedbackText("inflight", inflightScore)));
                this.Feedbacks.Add(new RouteFeedbackMVVM(bagText,"luggage.png",luggageScore,getFeedbackText("luggage",luggageScore)));

                if ((int)RouteFacility.FacilityType.WiFi <= GameObject.GetInstance().GameTime.Year)
                {
                    double wifiScore = RouteHelpers.GetRouteWifiScore(this.Route);
                    this.Feedbacks.Add(new RouteFeedbackMVVM(wifiFacility.Name, "wifi.png", wifiScore, getFeedbackText("wifi", wifiScore)));
                }


                this.Feedbacks.Add(new RouteFeedbackMVVM(priceText, "price.png", priceScore, getFeedbackText("price", priceScore)));
                this.Feedbacks.Add(new RouteFeedbackMVVM(String.Format("{0:0.0}", routeScore), "number.png", routeScore, getFeedbackText("score", routeScore))); 
            }
            else
            {
                string nodataString = Translator.GetInstance().GetString("RouteFeedback", "1100");
                this.Feedbacks.Add(new RouteFeedbackMVVM("-", "plane-feedback.png", 5, nodataString));
                this.Feedbacks.Add(new RouteFeedbackMVVM("-", "age.png", 5, nodataString));
                this.Feedbacks.Add(new RouteFeedbackMVVM("-", "food.png", 5, nodataString));
                this.Feedbacks.Add(new RouteFeedbackMVVM("-", "seats.png", 5, nodataString));
                this.Feedbacks.Add(new RouteFeedbackMVVM("-", "tv.png", 5, nodataString));
                this.Feedbacks.Add(new RouteFeedbackMVVM("-", "luggage.png", 5, nodataString));


                if ((int)RouteFacility.FacilityType.WiFi <= GameObject.GetInstance().GameTime.Year)
                {
                    this.Feedbacks.Add(new RouteFeedbackMVVM("-", "wifi.png", 5, nodataString));
                }

                this.Feedbacks.Add(new RouteFeedbackMVVM("-", "price.png", 5, nodataString));
                this.Feedbacks.Add(new RouteFeedbackMVVM("-", "number.png", 5, nodataString));
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

            string uid = this.FeedbackTypes[type][index];

            return Translator.GetInstance().GetString("RouteFeedback", uid);

        }
        #endregion

        #region Public Properties

        public double Distance { get; set; }

        public ObservableCollection<MonthlyInvoice> Invoices { get; set; }

        public Boolean IsEditable { get; set; }
        public Boolean ForHelicopters { get; set; }

        public ObservableCollection<Route> Legs { get; set; }

        public Route Route { get; set; }

        public Boolean ShowCargoInformation { get; set; }

        public Boolean ShowPassengersInformation { get; set; }

        public ObservableCollection<RouteFeedbackMVVM> Feedbacks { get; set; }

        public Dictionary<string, List<string>> FeedbackTypes;

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

            if (route.Type == Route.RouteType.Passenger)
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
    //the mvvm object for a route feedback
    public class RouteFeedbackMVVM
    {
        public string Text { get; set; }
        public string Image { get; set; }
        public double Value { get; set; }
        public string Feedback { get; set; }
        public RouteFeedbackMVVM(string text, string image, double value, string feedback)
        {
            this.Text = text;
            this.Image = "/Data/images/" + image;
            this.Value = value;
            this.Feedback = feedback;
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
            this.Contract = contract;
            this.StartDate = startdate;
            this.EndDate = enddate;
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
            this.Airliner = airliner;
            this.HasRoute = this.Airliner.HasRoute;
            this.Status = this.Airliner.Status == FleetAirliner.AirlinerStatus.Stopped
                ? StatusMVVM.Stopped
                : StatusMVVM.Started;

            if (this.Airliner.Status == FleetAirliner.AirlinerStatus.On_charter)
                this.Status = StatusMVVM.Charter;

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
                return this._hasroute;
            }
            set
            {
                this._hasroute = value;
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

        public MVVMRouteFacility(RouteFacility.FacilityType type, List<RouteFacility> facilities)
        {
            this.Facilities = new ObservableCollection<RouteFacility>();

            this.Type = type;

            foreach (RouteFacility facility in facilities)
                this.Facilities.Add(facility);

            this.SelectedFacility = this.Facilities.OrderBy(f => f.ServiceLevel).First();
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

        private Boolean _isuseable;

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
                    var facs = new List<RouteFacility>();
                    foreach (RouteFacility fac in RouteFacilities.GetFacilities(facType))
                    {
                        facs.Add(fac);
                    }

                    var facility = new MVVMRouteFacility(facType, facs);

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

        public Boolean IsUseable
        {
            get
            {
                return this._isuseable;
            }
            set
            {
                this._isuseable = value;
                this.NotifyPropertyChanged("IsUseable");
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
     //the mvvm object for the banned airlines from one country to another
    public class BannedAirlinesMVVM
    {
        public BaseUnit ToCountry { get; set; }
        public Country FromCountry { get; set; }
        public ObservableCollection<Airline> Airlines { get; set; }
        public BannedAirlinesMVVM(Country from, BaseUnit to, List<Airline> airlines)
        {
            this.ToCountry = to;
            this.FromCountry = from;
            this.Airlines = new ObservableCollection<Airline>(airlines);
        }
    }
    //the converter for the statistics for a route
    public class RouteStatisticsConverter : IValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var route = (Route)value;
            var stats = new List<KeyValuePair<string, object>>();

            if (route.Type == Route.RouteType.Passenger)
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