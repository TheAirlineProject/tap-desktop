using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Helpers;
using TheAirline.Infrastructure;
using TheAirline.Models.Airliners;
using TheAirline.Models.Airlines;
using TheAirline.Models.Airports;
using TheAirline.Models.General;
using TheAirline.Models.General.Environment;
using TheAirline.Models.Routes;

namespace TheAirline.GUIModel.PagesModel.RoutesPageModel
{
    /// <summary>
    ///     Interaction logic for PageCreateRoute.xaml
    /// </summary>
    public partial class PageCreateRoute : Page, INotifyPropertyChanged
    {
        #region Fields

        private string _routeinformationtext;

        private Route.RouteType _routetype;

        #endregion

        #region Constructors and Destructors

        public PageCreateRoute()
        {
            Airports = new ObservableCollection<Airport>();
            RouteInformationText = "";
            ConnectingRoutes = new ObservableCollection<Route>();
            Classes = new ObservableCollection<MVVMRouteClass>();
            Contracts = new ObservableCollection<SpecialContract>();

            foreach (AirlinerClass.ClassType type in AirlinerClass.GetAirlinerTypes())
            {
                if ((int)type <= GameObject.GetInstance().GameTime.Year)
                {
                    var rClass = new MVVMRouteClass(type, RouteAirlinerClass.SeatingType.ReservedSeating, 1);

                    Classes.Add(rClass);
                }
            }
            /*
            this.Airports =
                GameObject.GetInstance()
                    .HumanAirline.Airports.OrderByDescending(
                        a => a == GameObject.GetInstance().HumanAirline.Airports[0])
                    .ThenBy(a => a.Profile.Country.Name)
                    .ThenBy(a => a.Profile.Name)
                    .ToList();*/

            AirlinerType dummyAircraft = new AirlinerCargoType(
                new Manufacturer("Dummy", "", null, false),
                "All Aircrafts",
                "",
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                AirlinerType.BodyType.SingleAisle,
                AirlinerType.TypeRange.Regional,
                AirlinerType.TypeOfEngine.Jet,
                new Period<DateTime>(DateTime.Now, DateTime.Now),
                0,
                false);

            HumanAircrafts = new List<AirlinerType>();

            HumanAircrafts.Add(dummyAircraft);

            foreach (
                AirlinerType type in GameObject.GetInstance().HumanAirline.Fleet.Select(f => f.Airliner.Type).Distinct()
                )
            {
                HumanAircrafts.Add(type);
            }

            Loaded += PageCreateRoute_Loaded;

            InitializeComponent();
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public ObservableCollection<Airport> Airports { get; set; }

        public ObservableCollection<MVVMRouteClass> Classes { get; set; }

        public ObservableCollection<Route> ConnectingRoutes { get; set; }

        public ObservableCollection<SpecialContract> Contracts { get; set; }

        public List<AirlinerType> HumanAircrafts { get; set; }

        public string RouteInformationText
        {
            get
            {
                return _routeinformationtext;
            }
            set
            {
                _routeinformationtext = value;
                NotifyPropertyChanged("RouteInformationText");
            }
        }

        public Route.RouteType RouteType
        {
            get
            {
                return _routetype;
            }
            set
            {
                _routetype = value;
                NotifyPropertyChanged("RouteType");
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

        private void PageCreateRoute_Loaded(object sender, RoutedEventArgs e)
        {
            //hide the tab for a route
            var tab_main = UIHelpers.FindChild<TabControl>(Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                TabItem matchingItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Route").FirstOrDefault();

                matchingItem.Visibility = Visibility.Collapsed;

                TabItem airlinerItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Airliner").FirstOrDefault();

                airlinerItem.Visibility = Visibility.Collapsed;
            }

            //sets the selected items for the facilities
            foreach (MVVMRouteClass rClass in Classes)
            {
                foreach (MVVMRouteFacility rFacility in rClass.Facilities)
                {
                    rFacility.SelectedFacility = rFacility.Facilities.OrderBy(f => f.ServiceLevel).FirstOrDefault();
                }
            }

            var daysRange = new CalendarDateRange(new DateTime(2000, 1, 1), GameObject.GetInstance().GameTime);

            dpStartDate.BlackoutDates.Add(daysRange);
            dpStartDate.DisplayDateStart = GameObject.GetInstance().GameTime;
            dpStartDate.DisplayDate = GameObject.GetInstance().GameTime;

            createGrouping(cbDestination1);
            createGrouping(cbDestination2);
            createGrouping(cbStopover1);
            createGrouping(cbStopover2);
        }

        //creates the grouping for a combobox

        private void btnCreateNew_Click(object sender, RoutedEventArgs e)
        {
            if (createRoute())
            {
                var tab_main = UIHelpers.FindChild<TabControl>(Tag as Page, "tabMenu");

                if (tab_main != null)
                {
                    TabItem matchingCreateItem =
                        tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Create").FirstOrDefault();

                    tab_main.SelectedItem = matchingCreateItem;
                }
            }
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (createRoute())
            {
                var tab_main = UIHelpers.FindChild<TabControl>(Tag as Page, "tabMenu");

                if (tab_main != null)
                {
                    TabItem matchingItem =
                        tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Routes").FirstOrDefault();

                    tab_main.SelectedItem = matchingItem;
                }
            }
        }

        //creates a route and returns if success

        private void btnLoadConfiguration_Click(object sender, RoutedEventArgs e)
        {
            var cbConfigurations = new ComboBox();
            cbConfigurations.SetResourceReference(StyleProperty, "ComboBoxTransparentStyle");
            cbConfigurations.SelectedValuePath = "Name";
            cbConfigurations.DisplayMemberPath = "Name";
            cbConfigurations.HorizontalAlignment = HorizontalAlignment.Left;
            cbConfigurations.Width = 200;

            foreach (
                RouteClassesConfiguration confItem in
                    Configurations.GetConfigurations(Configuration.ConfigurationType.Routeclasses))
            {
                cbConfigurations.Items.Add(confItem);
            }

            cbConfigurations.SelectedIndex = 0;

            if (PopUpSingleElement.ShowPopUp(
                Translator.GetInstance().GetString("PageCreateRoute", "1012"),
                cbConfigurations) == PopUpSingleElement.ButtonSelected.OK && cbConfigurations.SelectedItem != null)
            {
                var configuration = (RouteClassesConfiguration)cbConfigurations.SelectedItem;

                foreach (RouteClassConfiguration classConfiguration in configuration.GetClasses())
                {
                    MVVMRouteClass rClass = Classes.FirstOrDefault(c => c.Type == classConfiguration.Type);

                    if (rClass != null)
                    {
                        foreach (RouteFacility facility in classConfiguration.GetFacilities())
                        {
                            MVVMRouteFacility rFacility = rClass.Facilities.FirstOrDefault(f => f.Type == facility.Type);

                            if (rFacility != null)
                            {
                                rFacility.SelectedFacility = facility;
                            }
                        }
                    }
                }
            }
        }

        private void btnShowConnectingRoutes_Click(object sender, RoutedEventArgs e)
        {
            var destination1 = (Airport)cbDestination1.SelectedItem;
            var destination2 = (Airport)cbDestination2.SelectedItem;

            PopUpShowConnectingRoutes.ShowPopUp(destination1, destination2);
        }

        private void btnStopover1_Click(object sender, RoutedEventArgs e)
        {
            cbStopover1.SelectedItem = null;
        }

        private void btnStopover2_Click(object sender, RoutedEventArgs e)
        {
            cbStopover2.SelectedItem = null;
        }
        private void btnContract_Click(object sender, RoutedEventArgs e)
        {
            cbContract.SelectedItem = null;
        }

        private void cbAircraft_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var type = (AirlinerType)((ComboBox)sender).SelectedItem;

            if (cbDestination2.ItemsSource != null && cbDestination1.SelectedItem != null)
            {
                var source = cbDestination2.Items as ICollectionView;
                source.Filter = o =>
                {
                    var a = o as Airport;
                    return a != null && a.GetMaxRunwayLength() >= type.MinRunwaylength
                           && MathHelpers.GetDistance((Airport)cbDestination1.SelectedItem, a) <= type.Range
                           || type.Manufacturer.Name == "Dummy";
                };
            }
        }
        private void cbStopover1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            while (Contracts.Count > 0)
                Contracts.Remove(Contracts[0]);

            if (cbStopover1.SelectedItem == null)
            {
                if (cbDestination1.SelectedItem != null && cbDestination2.SelectedItem != null)
                {
                    var destination1 = (Airport)cbDestination1.SelectedItem;
                    var destination2 = (Airport)cbDestination2.SelectedItem;

                    var humanContracts = GameObject.GetInstance().HumanAirline.SpecialContracts.Where(s => s.Type.Routes.Exists(r => (r.Departure == destination1 && r.Destination == destination2) || (r.Departure == destination2 && r.Destination == destination1 && r.BothWays)));

                    foreach (SpecialContract sc in humanContracts)
                    {
                        int routesCount = sc.Routes.Count(r => r.Destination1 == destination1 && r.Destination2 == destination2 && r.Type == RouteType);
                        int contractRoutesCount = sc.Type.Routes.Count(r => r.RouteType == RouteType && ((r.Departure == destination1 && r.Destination == destination2) || (r.Departure == destination2 && r.Destination == destination1 && r.BothWays)));

                        if (contractRoutesCount > routesCount)
                            Contracts.Add(sc);
                    }
                }
            }
        }
        private void cbDestination_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbDestination1 != null && cbDestination2 != null && txtDistance != null)
            {
                while (Contracts.Count > 0)
                    Contracts.Remove(Contracts[0]);

                var destination1 = (Airport)cbDestination1.SelectedItem;
                var destination2 = (Airport)cbDestination2.SelectedItem;

                if (destination1 != null && destination2 != null)
                {
                    foreach (MVVMRouteClass rClass in Classes)
                    {
                        rClass.FarePrice = PassengerHelpers.GetPassengerPrice(destination1, destination2)
                                           * GeneralHelpers.ClassToPriceFactor(rClass.Type);
                    }

                    txtDistance.Text =
                        new DistanceToUnitConverter().Convert(MathHelpers.GetDistance(destination1, destination2))
                            .ToString();

                    IEnumerable<Route> codesharingRoutes =
                        GameObject.GetInstance()
                            .HumanAirline.Codeshares.Where(
                                c =>
                                    c.Airline2 == GameObject.GetInstance().HumanAirline
                                    || c.Type == CodeshareAgreement.CodeshareType.BothWays)
                            .Select(c => c.Airline1 == GameObject.GetInstance().HumanAirline ? c.Airline2 : c.Airline1)
                            .SelectMany(a => a.Routes);
                    IEnumerable<Route> humanConnectingRoutes =
                        GameObject.GetInstance()
                            .HumanAirline.Routes.Where(
                                r =>
                                    r.Destination1 == destination1 || r.Destination2 == destination1
                                    || r.Destination1 == destination2 || r.Destination2 == destination2);

                    IEnumerable<Route> codesharingConnectingRoutes =
                        codesharingRoutes.Where(
                            r =>
                                r.Destination1 == destination1 || r.Destination2 == destination1
                                || r.Destination1 == destination2 || r.Destination2 == destination2);

                    ConnectingRoutes.Clear();

                    foreach (Route route in humanConnectingRoutes)
                    {
                        ConnectingRoutes.Add(route);
                    }

                    foreach (Route route in codesharingConnectingRoutes)
                    {
                        ConnectingRoutes.Add(route);
                    }

                    IEnumerable<Route> opponentRoutes =
                        Airlines.GetAllAirlines()
                            .Where(a => !a.IsHuman)
                            .SelectMany(a => a.Routes)
                            .Where(
                                r =>
                                    (r.Destination1 == destination1 && r.Destination2 == destination2)
                                    || (r.Destination2 == destination1 && r.Destination1 == destination2));

                    if (opponentRoutes.Count() == 0)
                    {
                        RouteInformationText = "";
                    }
                    else
                    {
                        IEnumerable<Airline> airlines = opponentRoutes.Select(r => r.Airline).Distinct();

                        if (airlines.Count() == 1)
                        {
                            RouteInformationText = string.Format(
                                "{0} also operates a route between {1} and {2}",
                                airlines.ElementAt(0).Profile.Name,
                                destination1.Profile.Name,
                                destination2.Profile.Name);
                        }
                        else
                        {
                            RouteInformationText =
                                string.Format(
                                    "{0} other airlines do also operate a route between {1} and {2}",
                                    airlines.Count(),
                                    destination1.Profile.Name,
                                    destination2.Profile.Name);
                        }
                    }

                    if (cbStopover1.SelectedItem == null)
                    {
                        var humanContracts = GameObject.GetInstance().HumanAirline.SpecialContracts.Where(s => s.Type.Routes.Exists(r => (r.Departure == destination1 && r.Destination == destination2) || (r.Departure == destination2 && r.Destination == destination1 && r.BothWays)));

                        foreach (SpecialContract sc in humanContracts)
                        {
                            int routesCount = sc.Routes.Count(r => r.Destination1 == destination1 && r.Destination2 == destination2 && r.Type == RouteType);
                            int contractRoutesCount = sc.Type.Routes.Count(r => r.RouteType == RouteType && ((r.Departure == destination1 && r.Destination == destination2) || (r.Departure == destination2 && r.Destination == destination1 && r.BothWays)));

                            if (contractRoutesCount > routesCount)
                                Contracts.Add(sc);
                        }
                    }
                   


                }
            }
        }

        private void createGrouping(ComboBox cb)
        {
            if (cb != null && cb.ItemsSource != null)
            {
                var view = (ListCollectionView)CollectionViewSource.GetDefaultView(cb.ItemsSource);

                if (view != null)
                {
                    if (view.GroupDescriptions != null)
                    {
                        view.GroupDescriptions.Clear();

                        var groupDescription = new PropertyGroupDescription("Profile.Town.Country");
                        view.GroupDescriptions.Add(groupDescription);
                    }
                }
            }
        }

        private Boolean createRoute()
        {
            SpecialContract contract = null;

            if (Contracts.Count > 0 && cbContract.SelectedItem != null)
            {
                contract = (SpecialContract)cbContract.SelectedItem;
            }
            Route route = null;
            var destination1 = (Airport)cbDestination1.SelectedItem;
            var destination2 = (Airport)cbDestination2.SelectedItem;
            var stopover1 = (Airport)cbStopover1.SelectedItem;
            Airport stopover2 = cbStopover2.Visibility == Visibility.Visible
                ? (Airport)cbStopover2.SelectedItem
                : null;
            DateTime startDate = dpStartDate.IsEnabled && dpStartDate.SelectedDate.HasValue
                ? dpStartDate.SelectedDate.Value
                : GameObject.GetInstance().GameTime;

            Weather.Season season = rbSeasonAll.IsChecked.Value ? Weather.Season.AllYear : Weather.Season.Winter;
            season = rbSeasonSummer.IsChecked.Value ? Weather.Season.Summer : season;
            season = rbSeasonWinter.IsChecked.Value ? Weather.Season.Winter : season;

            Boolean routeExists = GameObject.GetInstance().HumanAirline.Routes.Exists(r => r.Destination1 == destination1 && r.Destination2 == destination2);

            if (routeExists)
            {
                WPFMessageBox.Show(
                                  Translator.GetInstance().GetString("MessageBox", "3006"),
                                  string.Format(Translator.GetInstance().GetString("MessageBox", "3006", "message"), destination1.Profile.Name, destination2.Profile.Name),
                                  WPFMessageBoxButtons.Ok);
                return false;
            }

            try
            {
                if (AirlineHelpers.IsRouteDestinationsOk(
                    GameObject.GetInstance().HumanAirline,
                    destination1,
                    destination2,
                    RouteType,
                    stopover1,
                    stopover2))
                {
                    Guid id = Guid.NewGuid();

                    //passenger route
                    if (RouteType == Route.RouteType.Passenger)
                    {
                        route = new PassengerRoute(id.ToString(), destination1, destination2, startDate, 0);

                        foreach (MVVMRouteClass rac in Classes.Where(c => c.IsUseable))
                        {
                            ((PassengerRoute)route).GetRouteAirlinerClass(rac.Type).FarePrice = rac.FarePrice;

                            foreach (MVVMRouteFacility facility in rac.Facilities)
                            {
                                ((PassengerRoute)route).GetRouteAirlinerClass(rac.Type)
                                    .AddFacility(facility.SelectedFacility);
                            }
                        }
                    }
                    //cargo route
                    else if (RouteType == Route.RouteType.Cargo)
                    {
                        double cargoPrice = Convert.ToDouble(txtCargoPrice.Text);
                        route = new CargoRoute(id.ToString(), destination1, destination2, startDate, cargoPrice);
                    }
                    else if (RouteType == Route.RouteType.Helicopter)
                    {
                        route = new HelicopterRoute(id.ToString(), destination1, destination2, startDate, 0);

                        foreach (MVVMRouteClass rac in Classes.Where(c => c.IsUseable))
                        {
                            ((HelicopterRoute)route).GetRouteAirlinerClass(rac.Type).FarePrice = rac.FarePrice;

                            foreach (MVVMRouteFacility facility in rac.Facilities)
                            {
                                ((HelicopterRoute)route).GetRouteAirlinerClass(rac.Type)
                                    .AddFacility(facility.SelectedFacility);
                            }
                        }


                    }
                    else if (RouteType == Route.RouteType.Mixed)
                    {
                        double cargoPrice = Convert.ToDouble(txtCargoPrice.Text);

                        route = new CombiRoute(id.ToString(), destination1, destination2, startDate, 0, cargoPrice);

                        foreach (MVVMRouteClass rac in Classes.Where(c => c.IsUseable))
                        {
                            ((PassengerRoute)route).GetRouteAirlinerClass(rac.Type).FarePrice = rac.FarePrice;

                            foreach (MVVMRouteFacility facility in rac.Facilities)
                            {
                                ((PassengerRoute)route).GetRouteAirlinerClass(rac.Type)
                                    .AddFacility(facility.SelectedFacility);
                            }
                        }
                    }

                    FleetAirlinerHelpers.CreateStopoverRoute(route, stopover1, stopover2);

                    route.Season = season;

                    GameObject.GetInstance().HumanAirline.AddRoute(route);

                    if (contract != null)
                        contract.Routes.Add(route);

                    return true;
                }
            }
            catch (Exception ex)
            {
                WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", ex.Message),
                    Translator.GetInstance().GetString("MessageBox", ex.Message, "message"),
                    WPFMessageBoxButtons.Ok);

                return false;
            }

            return false;
        }

        private void rbRouteType_Checked(object sender, RoutedEventArgs e)
        {
            string type = ((RadioButton)sender).Tag.ToString();
            RouteType = (Route.RouteType)Enum.Parse(typeof(Route.RouteType), type, true);

            while (Airports.Count > 0)
            {
                Airports.RemoveAt(Airports.Count - 1);
            }

            if (RouteType == Route.RouteType.Cargo)
            {
                var airports = GameObject.GetInstance()
                    .HumanAirline.Airports.Where(a => a.GetAirlineContracts(GameObject.GetInstance().HumanAirline).Exists(c => c.TerminalType == Terminal.TerminalType.Cargo)).OrderByDescending(
                        a => a == GameObject.GetInstance().HumanAirline.Airports[0])
                    .ThenBy(a => a.Profile.Country.Name)
                    .ThenBy(a => a.Profile.Name);

                foreach (Airport airport in airports)
                {
                    Airports.Add(airport);
                }
            }
            else if (RouteType == Route.RouteType.Helicopter)
            {
                var airports = GameObject.GetInstance()
                   .HumanAirline.Airports.Where(a => a.Runways.Exists(r => r.Type == Runway.RunwayType.Helipad) && a.GetAirlineContracts(GameObject.GetInstance().HumanAirline).Exists(c => c.TerminalType == Terminal.TerminalType.Passenger)).OrderByDescending(
                       a => a == GameObject.GetInstance().HumanAirline.Airports[0])
                   .ThenBy(a => a.Profile.Country.Name)
                   .ThenBy(a => a.Profile.Name);

                foreach (Airport airport in airports)
                {
                    Airports.Add(airport);
                }

                foreach (MVVMRouteClass rc in Classes)
                    rc.IsUseable = rc.Type == AirlinerClass.ClassType.EconomyClass;


            }
            else
            {
                var airports = GameObject.GetInstance()
                      .HumanAirline.Airports.Where(a => a.Runways.Exists(r => r.Type == Runway.RunwayType.Regular) && a.GetAirlineContracts(GameObject.GetInstance().HumanAirline).Exists(c => c.TerminalType == Terminal.TerminalType.Passenger)).OrderByDescending(
                          a => a == GameObject.GetInstance().HumanAirline.Airports[0])
                      .ThenBy(a => a.Profile.Country.Name)
                      .ThenBy(a => a.Profile.Name);

                foreach (Airport airport in airports)
                {
                    Airports.Add(airport);
                }

                foreach (MVVMRouteClass rc in Classes)
                    rc.IsUseable = true;


            }
            /*
            //sets the selected items for the facilities
            foreach (MVVMRouteClass rClass in this.Classes)
            {
                foreach (MVVMRouteFacility rFacility in rClass.Facilities)
                {
                    rFacility.SelectedFacility = rFacility.Facilities.OrderBy(f => f.ServiceLevel).FirstOrDefault();
                }
            }*/

        }

        #endregion

      
    }

}