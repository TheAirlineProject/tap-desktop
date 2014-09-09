namespace TheAirline.GUIModel.PagesModel.RoutesPageModel
{
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
    using TheAirline.Model.AirlineModel;
    using TheAirline.Model.AirlinerModel;
    using TheAirline.Model.AirlinerModel.RouteModel;
    using TheAirline.Model.AirportModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.GeneralModel.Helpers;
    using TheAirline.Model.GeneralModel.WeatherModel;

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
            this.Airports = new ObservableCollection<Airport>();
            this.RouteInformationText = "";
            this.ConnectingRoutes = new ObservableCollection<Route>();
            this.Classes = new ObservableCollection<MVVMRouteClass>();
            this.Contracts = new ObservableCollection<SpecialContract>();

            foreach (AirlinerClass.ClassType type in AirlinerClass.GetAirlinerTypes())
            {
                if ((int)type <= GameObject.GetInstance().GameTime.Year)
                {
                    var rClass = new MVVMRouteClass(type, RouteAirlinerClass.SeatingType.Reserved_Seating, 1);

                    this.Classes.Add(rClass);
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

            this.HumanAircrafts = new List<AirlinerType>();

            this.HumanAircrafts.Add(dummyAircraft);

            foreach (
                AirlinerType type in GameObject.GetInstance().HumanAirline.Fleet.Select(f => f.Airliner.Type).Distinct()
                )
            {
                this.HumanAircrafts.Add(type);
            }

            this.Loaded += this.PageCreateRoute_Loaded;

            this.InitializeComponent();
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
                return this._routeinformationtext;
            }
            set
            {
                this._routeinformationtext = value;
                this.NotifyPropertyChanged("RouteInformationText");
            }
        }

        public Route.RouteType RouteType
        {
            get
            {
                return this._routetype;
            }
            set
            {
                this._routetype = value;
                this.NotifyPropertyChanged("RouteType");
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

        private void PageCreateRoute_Loaded(object sender, RoutedEventArgs e)
        {
            //hide the tab for a route
            var tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

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
            foreach (MVVMRouteClass rClass in this.Classes)
            {
                foreach (MVVMRouteFacility rFacility in rClass.Facilities)
                {
                    rFacility.SelectedFacility = rFacility.Facilities.OrderBy(f => f.ServiceLevel).FirstOrDefault();
                }
            }

            var daysRange = new CalendarDateRange(new DateTime(2000, 1, 1), GameObject.GetInstance().GameTime);

            this.dpStartDate.BlackoutDates.Add(daysRange);
            this.dpStartDate.DisplayDateStart = GameObject.GetInstance().GameTime;
            this.dpStartDate.DisplayDate = GameObject.GetInstance().GameTime;

            this.createGrouping(this.cbDestination1);
            this.createGrouping(this.cbDestination2);
            this.createGrouping(this.cbStopover1);
            this.createGrouping(this.cbStopover2);
        }

        //creates the grouping for a combobox

        private void btnCreateNew_Click(object sender, RoutedEventArgs e)
        {
            if (this.createRoute())
            {
                var tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

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
            if (this.createRoute())
            {
                var tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

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

                foreach (RouteClassConfiguration classConfiguration in configuration.getClasses())
                {
                    MVVMRouteClass rClass = this.Classes.FirstOrDefault(c => c.Type == classConfiguration.Type);

                    if (rClass != null)
                    {
                        foreach (RouteFacility facility in classConfiguration.getFacilities())
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
            var destination1 = (Airport)this.cbDestination1.SelectedItem;
            var destination2 = (Airport)this.cbDestination2.SelectedItem;

            PopUpShowConnectingRoutes.ShowPopUp(destination1, destination2);
        }

        private void btnStopover1_Click(object sender, RoutedEventArgs e)
        {
            this.cbStopover1.SelectedItem = null;
        }

        private void btnStopover2_Click(object sender, RoutedEventArgs e)
        {
            this.cbStopover2.SelectedItem = null;
        }
        private void btnContract_Click(object sender, RoutedEventArgs e)
        {
            this.cbContract.SelectedItem = null;
        }

        private void cbAircraft_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var type = (AirlinerType)((ComboBox)sender).SelectedItem;

            if (this.cbDestination2.ItemsSource != null && this.cbDestination1.SelectedItem != null)
            {
                var source = this.cbDestination2.Items as ICollectionView;
                source.Filter = o =>
                {
                    var a = o as Airport;
                    return a != null && a.getMaxRunwayLength() >= type.MinRunwaylength
                           && MathHelpers.GetDistance((Airport)this.cbDestination1.SelectedItem, a) <= type.Range
                           || type.Manufacturer.Name == "Dummy";
                };
            }
        }
        private void cbStopover1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            while (this.Contracts.Count > 0)
                this.Contracts.Remove(this.Contracts[0]);

            if (cbStopover1.SelectedItem == null)
            {
                if (cbDestination1.SelectedItem != null && cbDestination2.SelectedItem != null)
                {
                    var destination1 = (Airport)this.cbDestination1.SelectedItem;
                    var destination2 = (Airport)this.cbDestination2.SelectedItem;

                    var humanContracts = GameObject.GetInstance().HumanAirline.SpecialContracts.Where(s => s.Type.Routes.Exists(r => (r.Departure == destination1 && r.Destination == destination2) || (r.Departure == destination2 && r.Destination == destination1 && r.BothWays)));

                    foreach (SpecialContract sc in humanContracts)
                    {
                        int routesCount = sc.Routes.Count(r => r.Destination1 == destination1 && r.Destination2 == destination2 && r.Type == this.RouteType);
                        int contractRoutesCount = sc.Type.Routes.Count(r => r.RouteType == this.RouteType && ((r.Departure == destination1 && r.Destination == destination2) || (r.Departure == destination2 && r.Destination == destination1 && r.BothWays)));

                        if (contractRoutesCount > routesCount)
                            this.Contracts.Add(sc);
                    }
                }
            }
        }
        private void cbDestination_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.cbDestination1 != null && this.cbDestination2 != null && this.txtDistance != null)
            {
                while (this.Contracts.Count > 0)
                    this.Contracts.Remove(this.Contracts[0]);

                var destination1 = (Airport)this.cbDestination1.SelectedItem;
                var destination2 = (Airport)this.cbDestination2.SelectedItem;

                if (destination1 != null && destination2 != null)
                {
                    foreach (MVVMRouteClass rClass in this.Classes)
                    {
                        rClass.FarePrice = PassengerHelpers.GetPassengerPrice(destination1, destination2)
                                           * GeneralHelpers.ClassToPriceFactor(rClass.Type);
                    }

                    this.txtDistance.Text =
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

                    this.ConnectingRoutes.Clear();

                    foreach (Route route in humanConnectingRoutes)
                    {
                        this.ConnectingRoutes.Add(route);
                    }

                    foreach (Route route in codesharingConnectingRoutes)
                    {
                        this.ConnectingRoutes.Add(route);
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
                        this.RouteInformationText = "";
                    }
                    else
                    {
                        IEnumerable<Airline> airlines = opponentRoutes.Select(r => r.Airline).Distinct();

                        if (airlines.Count() == 1)
                        {
                            this.RouteInformationText = string.Format(
                                "{0} also operates a route between {1} and {2}",
                                airlines.ElementAt(0).Profile.Name,
                                destination1.Profile.Name,
                                destination2.Profile.Name);
                        }
                        else
                        {
                            this.RouteInformationText =
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
                            int routesCount = sc.Routes.Count(r => r.Destination1 == destination1 && r.Destination2 == destination2 && r.Type == this.RouteType);
                            int contractRoutesCount = sc.Type.Routes.Count(r => r.RouteType == this.RouteType && ((r.Departure == destination1 && r.Destination == destination2) || (r.Departure == destination2 && r.Destination == destination1 && r.BothWays)));

                            if (contractRoutesCount > routesCount)
                                this.Contracts.Add(sc);
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

            if (this.Contracts.Count > 0 && this.cbContract.SelectedItem != null)
            {
                contract = (SpecialContract)this.cbContract.SelectedItem;
            }
            Route route = null;
            var destination1 = (Airport)this.cbDestination1.SelectedItem;
            var destination2 = (Airport)this.cbDestination2.SelectedItem;
            var stopover1 = (Airport)this.cbStopover1.SelectedItem;
            Airport stopover2 = this.cbStopover2.Visibility == Visibility.Visible
                ? (Airport)this.cbStopover2.SelectedItem
                : null;
            DateTime startDate = this.dpStartDate.IsEnabled && this.dpStartDate.SelectedDate.HasValue
                ? this.dpStartDate.SelectedDate.Value
                : GameObject.GetInstance().GameTime;

            Weather.Season season = this.rbSeasonAll.IsChecked.Value ? Weather.Season.All_Year : Weather.Season.Winter;
            season = this.rbSeasonSummer.IsChecked.Value ? Weather.Season.Summer : season;
            season = this.rbSeasonWinter.IsChecked.Value ? Weather.Season.Winter : season;

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
                    this.RouteType,
                    stopover1,
                    stopover2))
                {
                    Guid id = Guid.NewGuid();

                    //passenger route
                    if (this.RouteType == Route.RouteType.Passenger)
                    {
                        route = new PassengerRoute(id.ToString(), destination1, destination2, startDate, 0);

                        foreach (MVVMRouteClass rac in this.Classes.Where(c => c.IsUseable))
                        {
                            ((PassengerRoute)route).getRouteAirlinerClass(rac.Type).FarePrice = rac.FarePrice;

                            foreach (MVVMRouteFacility facility in rac.Facilities)
                            {
                                ((PassengerRoute)route).getRouteAirlinerClass(rac.Type)
                                    .addFacility(facility.SelectedFacility);
                            }
                        }
                    }
                    //cargo route
                    else if (this.RouteType == Route.RouteType.Cargo)
                    {
                        double cargoPrice = Convert.ToDouble(this.txtCargoPrice.Text);
                        route = new CargoRoute(id.ToString(), destination1, destination2, startDate, cargoPrice);
                    }
                    else if (this.RouteType == Route.RouteType.Helicopter)
                    {
                        route = new HelicopterRoute(id.ToString(), destination1, destination2, startDate, 0);

                        foreach (MVVMRouteClass rac in this.Classes.Where(c => c.IsUseable))
                        {
                            ((HelicopterRoute)route).getRouteAirlinerClass(rac.Type).FarePrice = rac.FarePrice;

                            foreach (MVVMRouteFacility facility in rac.Facilities)
                            {
                                ((HelicopterRoute)route).getRouteAirlinerClass(rac.Type)
                                    .addFacility(facility.SelectedFacility);
                            }
                        }


                    }
                    else if (this.RouteType == Route.RouteType.Mixed)
                    {
                        double cargoPrice = Convert.ToDouble(this.txtCargoPrice.Text);

                        route = new CombiRoute(id.ToString(), destination1, destination2, startDate, 0, cargoPrice);

                        foreach (MVVMRouteClass rac in this.Classes.Where(c => c.IsUseable))
                        {
                            ((PassengerRoute)route).getRouteAirlinerClass(rac.Type).FarePrice = rac.FarePrice;

                            foreach (MVVMRouteFacility facility in rac.Facilities)
                            {
                                ((PassengerRoute)route).getRouteAirlinerClass(rac.Type)
                                    .addFacility(facility.SelectedFacility);
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
            this.RouteType = (Route.RouteType)Enum.Parse(typeof(Route.RouteType), type, true);

            while (this.Airports.Count > 0)
            {
                this.Airports.RemoveAt(this.Airports.Count - 1);
            }

            if (this.RouteType == Route.RouteType.Cargo)
            {
                var airports = GameObject.GetInstance()
                    .HumanAirline.Airports.Where(a => a.getAirlineContracts(GameObject.GetInstance().HumanAirline).Exists(c => c.TerminalType == Terminal.TerminalType.Cargo)).OrderByDescending(
                        a => a == GameObject.GetInstance().HumanAirline.Airports[0])
                    .ThenBy(a => a.Profile.Country.Name)
                    .ThenBy(a => a.Profile.Name);

                foreach (Airport airport in airports)
                {
                    this.Airports.Add(airport);
                }
            }
            else if (this.RouteType == Route.RouteType.Helicopter)
            {
                var airports = GameObject.GetInstance()
                   .HumanAirline.Airports.Where(a => a.Runways.Exists(r => r.Type == Runway.RunwayType.Helipad) && a.getAirlineContracts(GameObject.GetInstance().HumanAirline).Exists(c => c.TerminalType == Terminal.TerminalType.Passenger)).OrderByDescending(
                       a => a == GameObject.GetInstance().HumanAirline.Airports[0])
                   .ThenBy(a => a.Profile.Country.Name)
                   .ThenBy(a => a.Profile.Name);

                foreach (Airport airport in airports)
                {
                    this.Airports.Add(airport);
                }

                foreach (MVVMRouteClass rc in this.Classes)
                    rc.IsUseable = rc.Type == AirlinerClass.ClassType.EconomyClass;


            }
            else
            {
                var airports = GameObject.GetInstance()
                      .HumanAirline.Airports.Where(a => a.Runways.Exists(r => r.Type == Runway.RunwayType.Regular) && a.getAirlineContracts(GameObject.GetInstance().HumanAirline).Exists(c => c.TerminalType == Terminal.TerminalType.Passenger)).OrderByDescending(
                          a => a == GameObject.GetInstance().HumanAirline.Airports[0])
                      .ThenBy(a => a.Profile.Country.Name)
                      .ThenBy(a => a.Profile.Name);

                foreach (Airport airport in airports)
                {
                    this.Airports.Add(airport);
                }

                foreach (MVVMRouteClass rc in this.Classes)
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