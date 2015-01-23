namespace TheAirline.GUIModel.PagesModel.RoutesPageModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
    using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
    using TheAirline.GUIModel.CustomControlsModel;
    using TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel;
    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.AirlinerModel;
    using TheAirline.Model.AirlinerModel.RouteModel;
    using TheAirline.Model.AirportModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.GeneralModel.Helpers;
    using TheAirline.Model.GeneralModel.WeatherModel;

    /// <summary>
    ///     Interaction logic for PageRoutePlanner.xaml
    /// </summary>
    public partial class PageRoutePlanner : Page, INotifyPropertyChanged
    {
        #region Fields

        private Weather.Season ShowSeason;

        private Boolean _cantransferschedule;

        private Boolean _islongroute;

        private Route _selectedroute;

        private Point startPoint;

        #endregion

        #region Constructors and Destructors

        public PageRoutePlanner(FleetAirliner airliner)
        {
            this.ShowSeason = Weather.Season.All_Year;

            this.Airliner = airliner;
            this.Entries = new ObservableCollection<RouteTimeTableEntry>();
            this.Entries.CollectionChanged += this.Entries_CollectionChanged;

            this.ViewEntries = new ObservableCollection<RouteTimeTableEntry>();
            this.ViewEntries.CollectionChanged += this.ViewEntries_CollectionChanged;

            this.IsLongRoute = false;

            this.AllRoutes = new ObservableCollection<RoutePlannerItemMVVM>();
            this.Intervals = new ObservableCollection<int> { 1, 2, 3, 4, 5, 6 };

            this.Routes = new ObservableCollection<Route>();

            var routeType =
                (Route.RouteType)
                    Enum.Parse(typeof(Route.RouteType), this.Airliner.Airliner.Type.TypeAirliner.ToString(), true);

            if (this.Airliner.Airliner.Type.TypeAirliner == AirlinerType.TypeOfAirliner.Helicopter)
                routeType = Route.RouteType.Passenger;

            foreach (
                Route route in
                    this.Airliner.Airliner.Airline.Routes.Where(
                        r => r.getDistance() <= this.Airliner.Airliner.Range && r.Type == routeType || (routeType == Route.RouteType.Helicopter && r.Type == Route.RouteType.Helicopter && RouteHelpers.HasHelipads(r))))
            {
                this.Routes.Add(route);
            }

            this.AllRegions = new ObservableCollection<Region>();
            this.AllRegions.Add(Regions.GetRegion("100"));

            List<Region> routeRegions = this.Routes.Select(r => r.Destination1.Profile.Country.Region).ToList();
            routeRegions.AddRange(this.Routes.Select(r => r.Destination2.Profile.Country.Region));

            foreach (Region region in routeRegions.Distinct())
            {
                this.AllRegions.Add(region);
            }

            foreach (
                Route route in
                    this.Airliner.Airliner.Airline.Routes.Where(
                        r => r.getDistance() <= this.Airliner.Airliner.Range && r.Type == routeType || (routeType == Route.RouteType.Helicopter && r.Type == Route.RouteType.Helicopter && RouteHelpers.HasHelipads(r))))
            {
                this.AllRoutes.Add(new RoutePlannerItemMVVM(route, this.Airliner.Airliner.Type));
            }

            this.OutboundAirports = new ObservableCollection<Airport>();
            List<Airport> routeAirports = this.Routes.Select(r => r.Destination1).ToList();
            routeAirports.AddRange(this.Routes.Select(r => r.Destination2));

            foreach (Airport airport in routeAirports.Distinct())
            {
                this.OutboundAirports.Add(airport);
            }

            this.StartTimes = new ObservableCollection<TimeSpan>();

            for (int i = 0; i < 20; i++)
            {
                this.StartTimes.Add(new TimeSpan(6, i * 15, 0));
            }

            this.StopoverMinutes = new ObservableCollection<int> { 45, 60, 75, 90, 105, 120 };

            this.setCanTransferSchedule();

            this.SpecialContractRoutes = new ObservableCollection<Route>();

            var humanSpecialRoutes = GameObject.GetInstance().HumanAirline.SpecialContracts.Where(s => !s.Type.IsFixedDate).SelectMany(s => s.Routes.Where(r=>!r.HasAirliner));

            foreach (Route sRoute in humanSpecialRoutes)
                this.SpecialContractRoutes.Add(sRoute);

            this.Loaded += this.PageRoutePlanner_Loaded;

            this.InitializeComponent();
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public FleetAirliner Airliner { get; set; }

        public ObservableCollection<Region> AllRegions { get; set; }

        public ObservableCollection<RoutePlannerItemMVVM> AllRoutes { get; set; }

        public Boolean CanTransferSchedule
        {
            get
            {
                return this._cantransferschedule;
            }
            set
            {
                this._cantransferschedule = value;
                this.NotifyPropertyChanged("CanTransferSchedule");
            }
        }

        public Route SelectedRoute
        {
            get
            {
                return this._selectedroute;
            }
            set
            {
                this._selectedroute = value;
                this.NotifyPropertyChanged("SelectedRoute");
            }
        }

        public ObservableCollection<Route> SpecialContractRoutes { get; set; }

        public ObservableCollection<RouteTimeTableEntry> Entries { get; set; }

        public List<IntervalType> IntervalTypes
        {
            get
            {
                return Enum.GetValues(typeof(IntervalType)).Cast<IntervalType>().ToList();
            }
            private set
            {
                ;
            }
        }

        public ObservableCollection<int> Intervals { get; set; }

        public Boolean IsLongRoute
        {
            get
            {
                return this._islongroute;
            }
            set
            {
                this._islongroute = value;
                this.NotifyPropertyChanged("IsLongRoute");
            }
        }

        public List<OpsType> OpsTypes
        {
            get
            {
                return Enum.GetValues(typeof(OpsType)).Cast<OpsType>().ToList();
            }
            private set
            {
                ;
            }
        }

        public ObservableCollection<Airport> OutboundAirports { get; set; }

        public ObservableCollection<Route> Routes { get; set; }

        public ObservableCollection<TimeSpan> StartTimes { get; set; }

        public ObservableCollection<int> StopoverMinutes { get; set; }

        public ObservableCollection<RouteTimeTableEntry> ViewEntries { get; set; }

        #endregion

        #region Methods

        private void Entries_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.setViewEntries();
        }

        private void EntryAdded_Event(object sender, RoutedEventArgs e)
        {
            var item = e.OriginalSource as TimelineDropItem;
           
        }

        private void EntryChanged_Event(object sender, RoutedEventArgs e)
        {
            var entries = e.OriginalSource as object[];

            var oldEntry = entries[0] as TimelineEntry;
            var newEntry = entries[1] as TimelineEntry;

            var oEntry = (RouteTimeTableEntry)oldEntry.Source;

            var rt = new RouteTimeTable(oEntry.TimeTable.Route);
            var nEntry = new RouteTimeTableEntry(
                rt,
                (DayOfWeek)newEntry.StartTime.Days,
                newEntry.StartTime,
                oEntry.Destination);
            nEntry.Airliner = this.Airliner;
            rt.addEntry(nEntry);

            var tEntries = new List<RouteTimeTableEntry>(this.Entries);
            tEntries.Remove(oEntry);

            if (!TimeTableHelpers.IsTimeTableValid(rt, this.Airliner, tEntries))
            {
                WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2706"),
                    Translator.GetInstance().GetString("MessageBox", "2706", "message"),
                    WPFMessageBoxButtons.Ok);
            }
            else
            {
                this.Entries.Remove(oEntry);

                this.Entries.Add(nEntry);
            }
        }

        private void EntryDeleted_Event(object sender, RoutedEventArgs e)
        {
            var entry = (TimelineEntry)e.OriginalSource;

            var rEntry = (RouteTimeTableEntry)entry.Source;

            this.Entries.Remove(rEntry);
        }

        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void PageRoutePlanner_Loaded(object sender, RoutedEventArgs e)
        {
            var tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                TabItem matchingItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Route").FirstOrDefault();

                matchingItem.Visibility = Visibility.Collapsed;
            }

            foreach (
                RouteTimeTableEntry entry in
                    this.Airliner.Routes.SelectMany(r => r.TimeTable.Entries.Where(en => en.Airliner == this.Airliner)))
            {
                this.Entries.Add(entry);
            }

            this.setViewEntries();
            this.setFlightNumbers();

            this.cbRoute.SelectedIndex = 0;
            this.cbOutbound.SelectedIndex = 0;
        }

        private void Rectangle_MouseMove(object sender, MouseEventArgs e)
        {
            // Get the current mouse position
            Point mousePos = e.GetPosition(null);
            Vector diff = this.startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed
                && (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance
                    || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                var rect = (Rectangle)sender;

                // Initialize the drag & drop operation
                var dragData = new DataObject("Route", GameObject.GetInstance().HumanAirline.Airports[0]);
                DragDrop.DoDragDrop(rect, dragData, DragDropEffects.Move);
            }
        }

        private void Rectangle_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.startPoint = e.GetPosition(null);
        }

        private void ViewEntries_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var uctimetable = UIHelpers.FindChild<ucTimeTable>(this, "uctimetable");

            if (e.NewItems != null)
            {
                foreach (RouteTimeTableEntry entry in e.NewItems)
                {
                    var sTime = new TimeSpan((int)entry.Day, entry.Time.Hours, entry.Time.Minutes, 0);
                    TimeSpan eTime = sTime.Add(entry.TimeTable.Route.getFlightTime(this.Airliner.Airliner.Type));

                    var g2 = new Guid(entry.TimeTable.Route.Id);

                    byte[] bytes = g2.ToByteArray();

                    byte red = bytes[0];
                    byte green = bytes[1];
                    byte blue = bytes[2];

                    var brush = new SolidColorBrush(Color.FromRgb(red, green, blue));
                    brush.Opacity = 0.60;

                    TimeSpan localTimeDept = MathHelpers.ConvertTimeSpanToLocalTime(
                        sTime,
                        entry.DepartureAirport.Profile.TimeZone);
                    TimeSpan localTimeDest = MathHelpers.ConvertTimeSpanToLocalTime(
                        eTime,
                        entry.Destination.Airport.Profile.TimeZone);

                    //  string text = string.Format("{0}-{1}\n{2}-{3}", new AirportCodeConverter().Convert(entry.DepartureAirport), new AirportCodeConverter().Convert(entry.Destination.Airport),string.Format("{0:hh\\:mm}", entry.Time),string.Format("{0:hh\\:mm}",localTimeDept));
                    string text = string.Format(
                        "{0}-{1}",
                        new AirportCodeConverter().Convert(entry.DepartureAirport),
                        new AirportCodeConverter().Convert(entry.Destination.Airport));

                    string tooltip = string.Format(
                        "{6}\n{0}-{3}\n({1} {2})-({4} {5})",
                        string.Format("{0:hh\\:mm}", entry.Time),
                        string.Format("{0:hh\\:mm}", localTimeDept),
                        entry.DepartureAirport.Profile.TimeZone.ShortName,
                        string.Format("{0:hh\\:mm}", eTime),
                        string.Format("{0:hh\\:mm}", localTimeDest),
                        entry.Destination.Airport.Profile.TimeZone.ShortName,
                        entry.Destination.FlightCode);

                    uctimetable.addTimelineEntry(entry, sTime, eTime, text, brush, tooltip);
                }
            }

            if (e.OldItems != null)
            {
                foreach (RouteTimeTableEntry entry in e.OldItems)
                {
                    var sTime = new TimeSpan((int)entry.Day, entry.Time.Hours, entry.Time.Minutes, 0);
                    TimeSpan eTime = sTime.Add(entry.TimeTable.Route.getFlightTime(this.Airliner.Airliner.Type));

                    string text = string.Format(
                        "{0}-{1}",
                        new AirportCodeConverter().Convert(entry.DepartureAirport),
                        new AirportCodeConverter().Convert(entry.Destination.Airport));

                    uctimetable.removeTimelineEntry(sTime, eTime, text);
                }
            }
        }

        private void addEntries(List<DayOfWeek> days)
        {
            var route = (Route)this.cbHomebound.SelectedItem;

            var origin = (Airport)this.cbOutbound.SelectedItem;
            Airport airport = route.Destination1 == origin ? route.Destination2 : route.Destination1;

            var time = new TimeSpan(this.tpTime.Value.Value.Hour, this.tpTime.Value.Value.Minute, 0);

            string flightCode = this.Airliner.Airliner.Airline.Profile.IATACode + this.txtSchedulerFlightNumber.Text;

            var rt = new RouteTimeTable(route);

            foreach (DayOfWeek dayOfWeek in days)
            {
                var entry = new RouteTimeTableEntry(
                    route.TimeTable,
                    dayOfWeek,
                    time,
                    new RouteEntryDestination(airport, flightCode));
                entry.Airliner = this.Airliner;

                rt.addEntry(entry);
            }

            if (!TimeTableHelpers.IsRoutePlannerTimeTableValid(rt, this.Airliner, this.Entries.ToList()))
            {
                WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2706"),
                    Translator.GetInstance().GetString("MessageBox", "2706", "message"),
                    WPFMessageBoxButtons.Ok);
            }
            else
            {
                foreach (RouteTimeTableEntry entry in rt.Entries)
                {
                    this.Entries.Add(entry);
                }
            }

            this.setFlightNumbers();
        }

        private void btnAddGenerator_Click(object sender, RoutedEventArgs e)
        {
            var route = (Route)this.cbRoute.SelectedItem;

            RouteTimeTable rt = null;

            var intervalType = (IntervalType)this.cbIntervalType.SelectedItem;
            int interval = Convert.ToInt16(this.cbInterval.SelectedItem);
            var opsType = (OpsType)this.cbSchedule.SelectedItem;
            var delayMinutes = (int)this.cbDelayMinutes.SelectedItem;

            double maxBusinessRouteTime = new TimeSpan(2, 0, 0).TotalMinutes;
            var startTime = (TimeSpan)this.cbStartTime.SelectedItem;

            string flightcode1 = this.Airliner.Airliner.Airline.Profile.IATACode + this.txtFlightNumber.Text;

            int indexOf = this.Airliner.Airliner.Airline.getFlightCodes().IndexOf(flightcode1);

            string flightcode2;

            if (indexOf == this.Airliner.Airliner.Airline.getFlightCodes().Count)
            {
                flightcode2 = "";
            }
            else
            {
                flightcode2 = this.Airliner.Airliner.Airline.getFlightCodes()[indexOf + 1];
            }

            if (opsType == OpsType.Business)
            {
                var flightsPerDay =
                    (int)
                        (route.getFlightTime(this.Airliner.Airliner.Type)
                            .Add(FleetAirlinerHelpers.GetMinTimeBetweenFlights(this.Airliner))
                            .TotalMinutes / 2 / maxBusinessRouteTime);
                rt = AIHelpers.CreateBusinessRouteTimeTable(
                    route,
                    this.Airliner,
                    Math.Max(1, flightsPerDay),
                    flightcode1,
                    flightcode2);
            }
            if (intervalType == IntervalType.Day && opsType != OpsType.Business)
            {
                rt = AIHelpers.CreateAirlinerRouteTimeTable(
                    route,
                    this.Airliner,
                    interval,
                    true,
                    delayMinutes,
                    startTime,
                    flightcode1,
                    flightcode2);
            }
            if (intervalType == IntervalType.Week && opsType != OpsType.Business)
            {
                rt = AIHelpers.CreateAirlinerRouteTimeTable(
                    route,
                    this.Airliner,
                    interval,
                    false,
                    delayMinutes,
                    startTime,
                    flightcode1,
                    flightcode2);
            }
            if (!TimeTableHelpers.IsTimeTableValid(rt, this.Airliner, this.Entries.ToList(), false))
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2705"),
                    Translator.GetInstance().GetString("MessageBox", "2705", "message"),
                    WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    this.clearTimeTable();

                    foreach (RouteTimeTableEntry entry in rt.Entries)
                    {
                        if (!TimeTableHelpers.IsRouteEntryInOccupied(entry, this.Airliner))
                        {
                            this.Entries.Add(entry);
                        }
                    }
                }
            }
            else
            {
                foreach (RouteTimeTableEntry entry in rt.Entries)
                {
                    if (!TimeTableHelpers.IsRouteEntryInOccupied(entry, this.Airliner))
                    {
                        this.Entries.Add(entry);
                    }
                }
            }

            this.setFlightNumbers();
        }

        private void btnAddScheduler_Click(object sender, RoutedEventArgs e)
        {
            List<RadioButton> radioButtons = UIHelpers.FindRBChildren(this, "Interval");

            string interval = "";
            foreach (RadioButton rbInterval in radioButtons)
            {
                if (rbInterval.IsChecked.Value)
                {
                    interval = rbInterval.Tag.ToString();
                }
            }

            switch (interval)
            {
                    //SFO -> ATH: 767-200ER
                case "Manual":
                    this.addEntries(this.getSelectedDays());
                    break;
                case "Daily":
                    this.addEntries(Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>().ToList());
                    break;
                case "TTS":
                    this.addEntries(new List<DayOfWeek> { DayOfWeek.Thursday, DayOfWeek.Tuesday, DayOfWeek.Saturday });
                    break;
                case "EveryOther":
                    this.addEntries(
                        new List<DayOfWeek>
                        {
                            DayOfWeek.Monday,
                            DayOfWeek.Wednesday,
                            DayOfWeek.Friday,
                            DayOfWeek.Sunday
                        });
                    break;
                case "Weekend":
                    this.addEntries(new List<DayOfWeek> { DayOfWeek.Sunday, DayOfWeek.Saturday });
                    break;
                case "MWF":
                    this.addEntries(new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday });
                    break;
                case "Weekdays":
                    this.addEntries(
                        new List<DayOfWeek>
                        {
                            DayOfWeek.Monday,
                            DayOfWeek.Tuesday,
                            DayOfWeek.Wednesday,
                            DayOfWeek.Thursday,
                            DayOfWeek.Friday
                        });
                    break;
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            this.clearTimeTable();
        }
        private void btnLoadTimetable_Click(object sender, RoutedEventArgs e)
        {
             ComboBox cbConfigurations = new ComboBox();
            cbConfigurations.SetResourceReference(StyleProperty, "ComboBoxTransparentStyle");
            cbConfigurations.SelectedValuePath = "Name";
            cbConfigurations.DisplayMemberPath = "Name";
            cbConfigurations.HorizontalAlignment = HorizontalAlignment.Left;
            cbConfigurations.Width = 350;

            foreach (
                AirlinerScheduleConfiguration confItem in
                    Configurations.GetConfigurations(Configuration.ConfigurationType.AirlinerSchedule)
                    )
            {
                cbConfigurations.Items.Add(confItem);
            }

            cbConfigurations.SelectedIndex = 0;

            if (PopUpSingleElement.ShowPopUp(
                Translator.GetInstance().GetString("PageAirlineWages", "1013"),
                cbConfigurations) == PopUpSingleElement.ButtonSelected.OK && cbConfigurations.SelectedItem != null)
            {
                var configuration = (AirlinerScheduleConfiguration)cbConfigurations.SelectedItem;

                AirlinerType originType = configuration.AirlinerType;
                AirlinerType destType = this.Airliner.Airliner.Type;

                Boolean canFlyRoute = originType.TypeAirliner == destType.TypeAirliner && TimeTableHelpers.IsTimeTableValid(this.Airliner,configuration.Entries);

                if (canFlyRoute)
                {
                    this.clearTimeTable();

                    foreach (RouteTimeTableEntry entry in configuration.Entries)
                        this.Entries.Add(entry);
                }
                else
                {
                    WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2707"),
                    Translator.GetInstance().GetString("MessageBox", "2707", "message"),
                    WPFMessageBoxButtons.Ok);
                }
               
            }
        }
        private void btnCopyTimetable_Click(object sender, RoutedEventArgs e)
        {

            var entries = this.Entries.Count;
            var aircraft = this.Airliner.Airliner.Type.Name;

            var txtName = new TextBox();
            txtName.Width = 350;
            txtName.Background = Brushes.Transparent;
            txtName.Foreground = Brushes.White;
            txtName.Text = string.Format("Route schedule (Aircraft: {0} with {1} entries)",aircraft, entries);
            txtName.HorizontalAlignment = HorizontalAlignment.Left;

            if (PopUpSingleElement.ShowPopUp(
                Translator.GetInstance().GetString("PageAirlineEditAirliners", "1002"),
                txtName) == PopUpSingleElement.ButtonSelected.OK && txtName.Text.Trim().Length > 2)
            {
                string name = txtName.Text.Trim();
                var configuration = new AirlinerScheduleConfiguration(name, this.Airliner.Airliner.Type);

                foreach (RouteTimeTableEntry entry in this.Entries)
                    configuration.addEntry(entry);

                Configurations.AddConfiguration(configuration);
            }
        }
        private void btnSaveContract_Click(Object sender, RoutedEventArgs e)
        {
            Route route = (Route)cbSpecialRoute.SelectedItem;

            RouteTimeTable rtt = new RouteTimeTable(route);

            RouteTimeTableEntry entry = new RouteTimeTableEntry(rtt, DayOfWeek.Wednesday, new TimeSpan(12, 0, 0), new RouteEntryDestination(route.Destination2, "Charter"));
            entry.Airliner = this.Airliner;
            rtt.addEntry(entry);

            route.TimeTable = rtt;

            this.Airliner.addRoute(route);
            this.Airliner.Status = FleetAirliner.AirlinerStatus.On_charter;

            

       
        }
        private void btnSave_Click(Object sender, RoutedEventArgs e)
        {
            var oldEntries =
                new List<RouteTimeTableEntry>(
                    this.Airliner.Routes.SelectMany(r => r.TimeTable.Entries.Where(en => en.Airliner == this.Airliner)));

            IEnumerable<RouteTimeTableEntry> deleteEntries = oldEntries.Except(this.Entries);
            IEnumerable<RouteTimeTableEntry> newEntries = this.Entries.Except(oldEntries);

            foreach (RouteTimeTableEntry entry in newEntries)
            {
                entry.TimeTable.Route.TimeTable.addEntry(entry);

                if (!this.Airliner.Routes.Contains(entry.TimeTable.Route))
                {
                    this.Airliner.addRoute(entry.TimeTable.Route);
                }
            }

            foreach (RouteTimeTableEntry entry in deleteEntries)
            {
                entry.TimeTable.removeEntry(entry);
          
                if (entry.TimeTable.Entries.Count(en => en.Airliner == this.Airliner) == 0 || entry.TimeTable.Entries.Count == 0)
                {
                    this.Airliner.removeRoute(entry.TimeTable.Route);
                }
            }

            this.Airliner.Status = FleetAirliner.AirlinerStatus.Stopped;
 
        }

        private void btnSwap_Click(object sender, RoutedEventArgs e)
        {
            var outbound = (Airport)this.cbOutbound.SelectedItem;
            var route = (Route)this.cbHomebound.SelectedItem;

            this.cbOutbound.SelectedItem = route.Destination1 == outbound ? route.Destination2 : route.Destination1;

            this.cbHomebound.SelectedItem = route;
        }

        private void btnTransfer_Click(object sender, RoutedEventArgs e)
        {
            var cbAirliners = new ComboBox();
            cbAirliners.SetResourceReference(StyleProperty, "ComboBoxTransparentStyle");
            cbAirliners.SelectedValuePath = "Name";
            cbAirliners.DisplayMemberPath = "Name";
            cbAirliners.HorizontalAlignment = HorizontalAlignment.Left;
            cbAirliners.Width = 200;

            long maxDistance = this.Airliner.Airliner.Range;

            long requiredRunway = this.Airliner.Airliner.MinRunwaylength;

            List<FleetAirliner> airliners =
                GameObject.GetInstance()
                    .HumanAirline.Fleet.FindAll(
                        a =>
                            a != this.Airliner && a.Routes.Count > 0 && a.Status == FleetAirliner.AirlinerStatus.Stopped
                            && a.Routes.Max(r => r.getDistance()) <= maxDistance);

            foreach (FleetAirliner airliner in airliners)
            {
                cbAirliners.Items.Add(airliner);
            }

            cbAirliners.SelectedIndex = 0;

            if (PopUpSingleElement.ShowPopUp(
                Translator.GetInstance().GetString("PopUpAirlinerRoutes", "1001"),
                cbAirliners) == PopUpSingleElement.ButtonSelected.OK && cbAirliners.SelectedItem != null)
            {
                this.clearTimeTable();

                var transferAirliner = (FleetAirliner)cbAirliners.SelectedItem;

                foreach (Route route in transferAirliner.Routes)
                {
                    foreach (
                        RouteTimeTableEntry entry in
                            route.TimeTable.Entries.FindAll(en => en.Airliner == transferAirliner))
                    {
                        entry.Airliner = this.Airliner;

                        this.Entries.Add(entry);
                    }

                    if (!this.Airliner.Routes.Contains(route))
                    {
                        this.Airliner.addRoute(route);
                        this.Routes.Add(route);
                    }
                }
                transferAirliner.Routes.Clear();
            }
        }
        private void btnAllFlights_Click(object sender, RoutedEventArgs e)
        {
            PopUpRouteFlights.ShowPopUp(this.SelectedRoute);
        }
        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            this.clearTimeTable();

            foreach (
                RouteTimeTableEntry entry in
                    this.Airliner.Routes.SelectMany(r => r.TimeTable.Entries.Where(en => en.Airliner == this.Airliner)))
            {
                this.Entries.Add(entry);
            }
        }

        private void cbDelayMinutes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var route = (Route)this.cbRoute.SelectedItem;

            if (route != null && this.cbDelayMinutes != null && this.cbSchedule != null && this.cbIntervalType != null)
            {
                TimeSpan routeFlightTime = route.getFlightTime(this.Airliner.Airliner.Type);

                var delayMinutes = (int)this.cbDelayMinutes.SelectedItem;

                TimeSpan minFlightTime = routeFlightTime.Add(new TimeSpan(0, delayMinutes, 0));

                int maxHours = 22 - 06; //from 06.00 to 22.00

                var opsType = (OpsType)this.cbSchedule.SelectedItem;
                var intervalType = (IntervalType)this.cbIntervalType.SelectedItem;

                if (opsType == OpsType.Whole_Day)
                {
                    maxHours = 24;
                }

                var flightsPerDay = (int)Math.Floor((maxHours * 60) / (2 * minFlightTime.TotalMinutes));
                    //Convert.ToInt16(maxHours * 60 / (2 * minFlightTime.TotalMinutes));

                if (intervalType == IntervalType.Week)
                {
                    flightsPerDay = 6;
                }

                this.Intervals.Clear();

                for (int i = 0; i < Math.Max(1, flightsPerDay); i++)
                {
                    this.Intervals.Add(i + 1);
                }

                if (this.cbInterval != null)
                {
                    this.cbInterval.SelectedIndex = 0;
                }
            }
        }

        private void cbHomebound_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var route = (Route)this.cbHomebound.SelectedItem;

            if (route != null)
            {
                this.IsLongRoute =
                    MathHelpers.GetFlightTime(route.Destination1, route.Destination2, this.Airliner.Airliner.Type)
                        .Add(FleetAirlinerHelpers.GetMinTimeBetweenFlights(this.Airliner.Airliner.Type))
                        .TotalHours > 12;

                this.SelectedRoute = route;
            }
            else
            {
                this.IsLongRoute = false;
            }
        }

        private void cbIntervalType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var route = (Route)this.cbRoute.SelectedItem;

            if (route != null)
            {
                OpsType opsType = this.cbSchedule == null ? OpsType.Regular : (OpsType)this.cbSchedule.SelectedItem;
                var intervalType = (IntervalType)this.cbIntervalType.SelectedItem;

                TimeSpan routeFlightTime = route.getFlightTime(this.Airliner.Airliner.Type);

                var delayMinutes = (int)this.cbDelayMinutes.SelectedItem;

                TimeSpan minFlightTime = routeFlightTime.Add(new TimeSpan(0, delayMinutes, 0));

                int maxHours = 22 - 06;

                if (opsType == OpsType.Whole_Day)
                {
                    maxHours = 24;
                }

                var flightsPerDay = (int)Math.Floor((maxHours * 60) / (2 * minFlightTime.TotalMinutes));
                    //Convert.ToInt16(maxHours * 60 / (2 * minFlightTime.TotalMinutes));

                if (intervalType == IntervalType.Week)
                {
                    flightsPerDay = 7;
                }

                this.Intervals.Clear();

                for (int i = 0; i < Math.Max(1, flightsPerDay); i++)
                {
                    this.Intervals.Add(i + 1);
                }

                if (this.cbInterval != null)
                {
                    this.cbInterval.SelectedIndex = 0;
                }
            }
        }

        private void cbInterval_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var route = (Route)this.cbRoute.SelectedItem;

            if (this.cbSchedule != null && this.cbIntervalType != null)
            {
                var opsType = (OpsType)this.cbSchedule.SelectedItem;
                var intervalType = (IntervalType)this.cbIntervalType.SelectedItem;

                if (route != null && intervalType == IntervalType.Day && ((ComboBox)sender).SelectedItem != null)
                {
                    var interval = (int)((ComboBox)sender).SelectedItem;

                    int latestStartTime = 22;
                    
                    TimeSpan routeFlightTime = route.getFlightTime(this.Airliner.Airliner.Type);

                    var delayMinutes = (int)this.cbDelayMinutes.SelectedItem;

                    TimeSpan minFlightTime = routeFlightTime.Add(new TimeSpan(0, delayMinutes, 0));

                    
                    var lastDepartureHour = (int)(latestStartTime - (minFlightTime.TotalHours * interval * 2));

                    this.StartTimes.Clear();

                    int startTime = 6;

                    if (opsType == OpsType.Whole_Day)
                    {
                        startTime = 0;
                    }

                    for (int i = startTime; i <= Math.Max(startTime, lastDepartureHour); i++)
                    {
                        for (int j = 0; j < 60; j += 15)
                        {
                            this.StartTimes.Add(new TimeSpan(i, j, 0));
                        }
                    }

                    if (this.cbStartTime != null)
                    {
                        this.cbStartTime.SelectedIndex = 0;
                    }
                }
            }
        }

        private void cbOutbound_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var outbuound = (Airport)((ComboBox)sender).SelectedItem;

            IEnumerable<Route> routes =
                this.Airliner.Airliner.Airline.Routes.Where(
                    r => r.Destination2 == outbuound || r.Destination1 == outbuound);

            if (this.cbHomebound != null)
            {
                this.cbHomebound.Items.Clear();

                foreach (Route route in routes)
                {
                    this.cbHomebound.Items.Add(route);
                }

                this.cbHomebound.SelectedIndex = 0;
            }
        }

        private void cbRegion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var region = (Region)((ComboBox)sender).SelectedItem;

            if (this.cbRoute != null)
            {
                var source = this.cbRoute.Items as ICollectionView;
                source.Filter = o =>
                {
                    var r = o as Route;
                    return r.Destination1.Profile.Country.Region == region
                           || r.Destination2.Profile.Country.Region == region || region.Uid == "100";
                };
            }
        }

        private void cbRoute_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var route = (Route)((ComboBox)sender).SelectedItem;

            if (route != null && this.cbDelayMinutes != null && this.cbSchedule != null && this.cbIntervalType != null)
            {
                this.SelectedRoute = route;

                TimeSpan routeFlightTime = route.getFlightTime(this.Airliner.Airliner.Type);

                var delayMinutes = (int)this.cbDelayMinutes.SelectedItem;

                TimeSpan minFlightTime = routeFlightTime.Add(new TimeSpan(0, delayMinutes, 0));

                int maxHours = 22 - 06; //from 06.00 to 22.00<

                var opsType = (OpsType)this.cbSchedule.SelectedItem;
                var intervalType = (IntervalType)this.cbIntervalType.SelectedItem;

                if (opsType == OpsType.Whole_Day)
                {
                    maxHours = 24;
                }

                var flightsPerDay = (int)Math.Floor((maxHours * 60) / (2 * minFlightTime.TotalMinutes));
                    //Convert.ToInt16(maxHours * 60 / (2 * minFlightTime.TotalMinutes));

                if (intervalType == IntervalType.Week)
                {
                    flightsPerDay = 7;
                }

                this.Intervals.Clear();

                for (int i = 0; i < Math.Max(1, flightsPerDay); i++)
                {
                    this.Intervals.Add(i + 1);
                }
            }
        }

        private void cbSchedule_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var route = (Route)this.cbRoute.SelectedItem;

            if (this.cbSchedule != null && this.cbIntervalType != null)
            {
                var opsType = (OpsType)this.cbSchedule.SelectedItem;
                var intervalType = (IntervalType)this.cbIntervalType.SelectedItem;

                if (route != null && intervalType == IntervalType.Day && ((ComboBox)sender).SelectedItem != null)
                {
                    var interval = (int)((ComboBox)sender).SelectedItem;

                    int latestStartTime = 22; 

                    TimeSpan routeFlightTime = route.getFlightTime(this.Airliner.Airliner.Type);

                    var delayMinutes = (int)this.cbDelayMinutes.SelectedItem;

                    TimeSpan minFlightTime = routeFlightTime.Add(new TimeSpan(0, delayMinutes, 0));

                    var lastDepartureHour = (int)(latestStartTime - (minFlightTime.TotalHours * interval * 2));

                    this.StartTimes.Clear();

                    int startTime = 6;

                    if (opsType == OpsType.Whole_Day)
                    {
                        startTime = 0;
                    }

                    for (int i = startTime; i <= Math.Max(startTime, lastDepartureHour); i++)
                    {
                        for (int j = 0; j < 60; j += 15)
                        {
                            this.StartTimes.Add(new TimeSpan(i, j, 0));
                        }
                    }

                    if (this.cbStartTime != null)
                    {
                        this.cbStartTime.SelectedIndex = 0;
                    }

                    int maxHours = 22 - 06; //from 06.00 to 22.00<

                  
                    if (opsType == OpsType.Whole_Day)
                    {
                        maxHours = 24;
                    }

                    var flightsPerDay = (int)Math.Floor((maxHours * 60) / (2 * minFlightTime.TotalMinutes));
                  
                    if (intervalType == IntervalType.Week)
                    {
                        flightsPerDay = 7;
                    }


                    this.Intervals.Clear();

                     for (int i = 0; i < Math.Max(1, flightsPerDay); i++)
                {
                    this.Intervals.Add(i + 1);
                }

                    
                }
            }
        }

        private void clearTimeTable()
        {
            var entries = new ObservableCollection<RouteTimeTableEntry>(this.Entries);
            
            foreach (RouteTimeTableEntry entry in entries)
            {
                this.Entries.Remove(entry);
            }
        }

        //returns the selected days
        private List<DayOfWeek> getSelectedDays()
        {
            var days = new List<DayOfWeek>();

            if (this.cbMonday.IsChecked.Value)
            {
                days.Add(DayOfWeek.Monday);
            }
            if (this.cbTuesday.IsChecked.Value)
            {
                days.Add(DayOfWeek.Tuesday);
            }
            if (this.cbWednesday.IsChecked.Value)
            {
                days.Add(DayOfWeek.Wednesday);
            }
            if (this.cbThursday.IsChecked.Value)
            {
                days.Add(DayOfWeek.Thursday);
            }
            if (this.cbFriday.IsChecked.Value)
            {
                days.Add(DayOfWeek.Friday);
            }
            if (this.cbSaturday.IsChecked.Value)
            {
                days.Add(DayOfWeek.Saturday);
            }
            if (this.cbSunday.IsChecked.Value)
            {
                days.Add(DayOfWeek.Sunday);
            }

            return days;
        }

        //adds entries to the planner

        private void rbSeason_Checked(object sender, RoutedEventArgs e)
        {
            var season = (string)((RadioButton)sender).Tag;

            if (season == "Winter")
            {
                this.ShowSeason = Weather.Season.Winter;
            }

            if (season == "Summer")
            {
                this.ShowSeason = Weather.Season.Summer;
            }

            if (season == "AllYear")
            {
                this.ShowSeason = Weather.Season.All_Year;
            }

            this.setViewEntries();
        }

        private void setCanTransferSchedule()
        {
            long maxDistance = this.Airliner.Airliner.Range;

            long requiredRunway = this.Airliner.Airliner.MinRunwaylength;

            this.CanTransferSchedule =
                GameObject.GetInstance()
                    .HumanAirline.Fleet.FindAll(
                        a =>
                            a != this.Airliner && a.Routes.Count > 0 && a.Status == FleetAirliner.AirlinerStatus.Stopped
                            && a.Routes.Max(r => r.getDistance()) <= maxDistance)
                    .Count > 0;
        }

        private void setFlightNumbers()
        {
            string number = "";

            int i = 0;

            while (number == "" || this.Entries.FirstOrDefault(e => e.Destination.FlightCode == number) != null)
            {
                number = GameObject.GetInstance().HumanAirline.getNextFlightCode(i);

                i++;
            }

            number = number.Substring(2);

            this.txtFlightNumber.Text = String.Format("{0:0000}", number);
            this.txtSchedulerFlightNumber.Text = String.Format("{0:0000}", number);
        }

        //sets the view entries
        private void setViewEntries()
        {
            var entries = new ObservableCollection<RouteTimeTableEntry>(this.ViewEntries);

            foreach (RouteTimeTableEntry entry in entries)
            {
                this.ViewEntries.Remove(entry);
            }

            foreach (RouteTimeTableEntry entry in this.Entries.Where(e => e.TimeTable.Route.Season == this.ShowSeason))
            {
                this.ViewEntries.Add(entry);
            }
        }

        #endregion
    }
}