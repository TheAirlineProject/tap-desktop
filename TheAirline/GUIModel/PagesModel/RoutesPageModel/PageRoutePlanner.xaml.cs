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
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Helpers;
using TheAirline.Infrastructure;
using TheAirline.Models.Airliners;
using TheAirline.Models.Airports;
using TheAirline.Models.General;
using TheAirline.Models.General.Countries;
using TheAirline.Models.General.Environment;
using TheAirline.Models.Routes;

namespace TheAirline.GUIModel.PagesModel.RoutesPageModel
{
    /// <summary>
    ///     Interaction logic for PageRoutePlanner.xaml
    /// </summary>
    public partial class PageRoutePlanner : Page, INotifyPropertyChanged
    {
        #region Fields

        private Weather.Season ShowSeason;

        private Boolean _cantransferschedule;

        private Boolean _islongroute;

        private Point startPoint;

        #endregion

        #region Constructors and Destructors

        public PageRoutePlanner(FleetAirliner airliner)
        {
            ShowSeason = Weather.Season.AllYear;

            Airliner = airliner;
            Entries = new ObservableCollection<RouteTimeTableEntry>();
            Entries.CollectionChanged += Entries_CollectionChanged;

            ViewEntries = new ObservableCollection<RouteTimeTableEntry>();
            ViewEntries.CollectionChanged += ViewEntries_CollectionChanged;

            IsLongRoute = false;

            AllRoutes = new List<RoutePlannerItemMVVM>();
            Intervals = new ObservableCollection<int> { 1, 2, 3, 4, 5, 6 };

            Routes = new ObservableCollection<Route>();

            var routeType =
                (Route.RouteType)
                    Enum.Parse(typeof(Route.RouteType), Airliner.Airliner.Type.TypeAirliner.ToString(), true);
            ;
            
            foreach (
                Route route in
                    Airliner.Airliner.Airline.Routes.Where(
                        r => r.GetDistance() <= Airliner.Airliner.Range && r.Type == routeType))
            {
                Routes.Add(route);
            }

            AllRegions = new List<Region>();
            AllRegions.Add(Regions.GetRegion("100"));

            List<Region> routeRegions = Routes.Select(r => r.Destination1.Profile.Country.Region).ToList();
            routeRegions.AddRange(Routes.Select(r => r.Destination2.Profile.Country.Region));

            foreach (Region region in routeRegions.Distinct())
            {
                AllRegions.Add(region);
            }

            foreach (
                Route route in
                    Airliner.Airliner.Airline.Routes.Where(
                        r => r.GetDistance() <= Airliner.Airliner.Range && r.Type == routeType))
            {
                AllRoutes.Add(new RoutePlannerItemMVVM(route, Airliner.Airliner.Type));
            }

            OutboundAirports = new List<Airport>();
            List<Airport> routeAirports = Routes.Select(r => r.Destination1).ToList();
            routeAirports.AddRange(Routes.Select(r => r.Destination2));

            foreach (Airport airport in routeAirports.Distinct())
            {
                OutboundAirports.Add(airport);
            }

            StartTimes = new ObservableCollection<TimeSpan>();

            for (int i = 0; i < 20; i++)
            {
                StartTimes.Add(new TimeSpan(6, i * 15, 0));
            }

            StopoverMinutes = new List<int> { 45, 60, 75, 90, 105, 120 };

            setCanTransferSchedule();

            SpecialContractRoutes = new List<Route>();

            var humanSpecialRoutes = GameObject.GetInstance().HumanAirline.SpecialContracts.Where(s => !s.Type.IsFixedDate).SelectMany(s => s.Routes.Where(r=>!r.HasAirliner));

            foreach (Route sRoute in humanSpecialRoutes)
                SpecialContractRoutes.Add(sRoute);

            Loaded += PageRoutePlanner_Loaded;

            InitializeComponent();
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public FleetAirliner Airliner { get; set; }

        public List<Region> AllRegions { get; set; }

        public List<RoutePlannerItemMVVM> AllRoutes { get; set; }

        public Boolean CanTransferSchedule
        {
            get
            {
                return _cantransferschedule;
            }
            set
            {
                _cantransferschedule = value;
                NotifyPropertyChanged("CanTransferSchedule");
            }
        }
        public List<Route> SpecialContractRoutes { get; set; }

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
                return _islongroute;
            }
            set
            {
                _islongroute = value;
                NotifyPropertyChanged("IsLongRoute");
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

        public List<Airport> OutboundAirports { get; set; }

        public ObservableCollection<Route> Routes { get; set; }

        public ObservableCollection<TimeSpan> StartTimes { get; set; }

        public List<int> StopoverMinutes { get; set; }

        public ObservableCollection<RouteTimeTableEntry> ViewEntries { get; set; }

        #endregion

        #region Methods

        private void Entries_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            setViewEntries();
        }

        private void EntryAdded_Event(object sender, RoutedEventArgs e)
        {
            var item = e.OriginalSource as TimelineDropItem;
            /*
             RouteTimeTable rt = new RouteTimeTable(item.Object);

            string flightCode = this.Airliner.Airliner.Airline.Profile.IATACode + txtSchedulerFlightNumber.Text;

            RouteTimeTableEntry rEntry = new RouteTimeTableEntry(rt,item.Day,item.Time,new RouteEntryDestination(,flightCode));
            
            this.Entries.Add(rEntry);
            */
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
            nEntry.Airliner = Airliner;
            rt.AddEntry(nEntry);

            var tEntries = new List<RouteTimeTableEntry>(Entries);
            tEntries.Remove(oEntry);

            if (!TimeTableHelpers.IsTimeTableValid(rt, Airliner, tEntries))
            {
                WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2706"),
                    Translator.GetInstance().GetString("MessageBox", "2706", "message"),
                    WPFMessageBoxButtons.Ok);
            }
            else
            {
                Entries.Remove(oEntry);

                Entries.Add(nEntry);
            }
        }

        private void EntryDeleted_Event(object sender, RoutedEventArgs e)
        {
            var entry = (TimelineEntry)e.OriginalSource;

            var rEntry = (RouteTimeTableEntry)entry.Source;

            Entries.Remove(rEntry);
        }

        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void PageRoutePlanner_Loaded(object sender, RoutedEventArgs e)
        {
            var tab_main = UIHelpers.FindChild<TabControl>(Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                TabItem matchingItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Route").FirstOrDefault();

                matchingItem.Visibility = Visibility.Collapsed;
            }

            foreach (
                RouteTimeTableEntry entry in
                    Airliner.Routes.SelectMany(r => r.TimeTable.Entries.Where(en => en.Airliner == Airliner)))
            {
                Entries.Add(entry);
            }

            setViewEntries();
            setFlightNumbers();

            cbRoute.SelectedIndex = 0;
            cbOutbound.SelectedIndex = 0;
        }

        private void Rectangle_MouseMove(object sender, MouseEventArgs e)
        {
            // Get the current mouse position
            Point mousePos = e.GetPosition(null);
            Vector diff = startPoint - mousePos;

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
            startPoint = e.GetPosition(null);
        }

        private void ViewEntries_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var uctimetable = UIHelpers.FindChild<ucTimeTable>(this, "uctimetable");

            if (e.NewItems != null)
            {
                foreach (RouteTimeTableEntry entry in e.NewItems)
                {
                    var sTime = new TimeSpan((int)entry.Day, entry.Time.Hours, entry.Time.Minutes, 0);
                    TimeSpan eTime = sTime.Add(entry.TimeTable.Route.GetFlightTime(Airliner.Airliner.Type));

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
                        "{0}-{3}\n({1} {2})-({4} {5})",
                        string.Format("{0:hh\\:mm}", entry.Time),
                        string.Format("{0:hh\\:mm}", localTimeDept),
                        entry.DepartureAirport.Profile.TimeZone.ShortName,
                        string.Format("{0:hh\\:mm}", eTime),
                        string.Format("{0:hh\\:mm}", localTimeDest),
                        entry.Destination.Airport.Profile.TimeZone.ShortName);

                    uctimetable.addTimelineEntry(entry, sTime, eTime, text, brush, tooltip);
                }
            }

            if (e.OldItems != null)
            {
                foreach (RouteTimeTableEntry entry in e.OldItems)
                {
                    var sTime = new TimeSpan((int)entry.Day, entry.Time.Hours, entry.Time.Minutes, 0);
                    TimeSpan eTime = sTime.Add(entry.TimeTable.Route.GetFlightTime(Airliner.Airliner.Type));

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
            var route = (Route)cbHomebound.SelectedItem;

            var origin = (Airport)cbOutbound.SelectedItem;
            Airport airport = route.Destination1 == origin ? route.Destination2 : route.Destination1;

            var time = new TimeSpan(tpTime.Value.Value.Hour, tpTime.Value.Value.Minute, 0);

            string flightCode = Airliner.Airliner.Airline.Profile.IATACode + txtSchedulerFlightNumber.Text;

            var rt = new RouteTimeTable(route);

            foreach (DayOfWeek dayOfWeek in days)
            {
                var entry = new RouteTimeTableEntry(
                    route.TimeTable,
                    dayOfWeek,
                    time,
                    new RouteEntryDestination(airport, flightCode));
                entry.Airliner = Airliner;

                rt.AddEntry(entry);
            }

            if (!TimeTableHelpers.IsRoutePlannerTimeTableValid(rt, Airliner, Entries.ToList()))
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
                    Entries.Add(entry);
                }
            }

            setFlightNumbers();
        }

        private void btnAddGenerator_Click(object sender, RoutedEventArgs e)
        {
            var route = (Route)cbRoute.SelectedItem;

            RouteTimeTable rt = null;

            var intervalType = (IntervalType)cbIntervalType.SelectedItem;
            int interval = Convert.ToInt16(cbInterval.SelectedItem);
            var opsType = (OpsType)cbSchedule.SelectedItem;
            var delayMinutes = (int)cbDelayMinutes.SelectedItem;

            double maxBusinessRouteTime = new TimeSpan(2, 0, 0).TotalMinutes;
            var startTime = (TimeSpan)cbStartTime.SelectedItem;

            string flightcode1 = Airliner.Airliner.Airline.Profile.IATACode + txtFlightNumber.Text;

            int indexOf = Airliner.Airliner.Airline.GetFlightCodes().IndexOf(flightcode1);

            string flightcode2;

            if (indexOf == Airliner.Airliner.Airline.GetFlightCodes().Count)
            {
                flightcode2 = "";
            }
            else
            {
                flightcode2 = Airliner.Airliner.Airline.GetFlightCodes()[indexOf + 1];
            }

            if (opsType == OpsType.Business)
            {
                var flightsPerDay =
                    (int)
                        (route.GetFlightTime(Airliner.Airliner.Type)
                            .Add(FleetAirlinerHelpers.GetMinTimeBetweenFlights(Airliner))
                            .TotalMinutes / 2 / maxBusinessRouteTime);
                rt = AIHelpers.CreateBusinessRouteTimeTable(
                    route,
                    Airliner,
                    Math.Max(1, flightsPerDay),
                    flightcode1,
                    flightcode2);
            }
            if (intervalType == IntervalType.Day && opsType != OpsType.Business)
            {
                rt = AIHelpers.CreateAirlinerRouteTimeTable(
                    route,
                    Airliner,
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
                    Airliner,
                    interval,
                    false,
                    delayMinutes,
                    startTime,
                    flightcode1,
                    flightcode2);
            }
            if (!TimeTableHelpers.IsTimeTableValid(rt, Airliner, Entries.ToList(), false))
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2705"),
                    Translator.GetInstance().GetString("MessageBox", "2705", "message"),
                    WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    clearTimeTable();

                    foreach (RouteTimeTableEntry entry in rt.Entries)
                    {
                        if (!TimeTableHelpers.IsRouteEntryInOccupied(entry, Airliner))
                        {
                            Entries.Add(entry);
                        }
                    }
                }
            }
            else
            {
                foreach (RouteTimeTableEntry entry in rt.Entries)
                {
                    if (!TimeTableHelpers.IsRouteEntryInOccupied(entry, Airliner))
                    {
                        Entries.Add(entry);
                    }
                }
            }

            setFlightNumbers();
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
                    addEntries(getSelectedDays());
                    break;
                case "Daily":
                    addEntries(Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>().ToList());
                    break;
                case "TTS":
                    addEntries(new List<DayOfWeek> { DayOfWeek.Thursday, DayOfWeek.Tuesday, DayOfWeek.Saturday });
                    break;
                case "EveryOther":
                    addEntries(
                        new List<DayOfWeek>
                        {
                            DayOfWeek.Monday,
                            DayOfWeek.Wednesday,
                            DayOfWeek.Friday,
                            DayOfWeek.Sunday
                        });
                    break;
                case "Weekend":
                    addEntries(new List<DayOfWeek> { DayOfWeek.Sunday, DayOfWeek.Saturday });
                    break;
                case "MWF":
                    addEntries(new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday });
                    break;
                case "Weekdays":
                    addEntries(
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
            clearTimeTable();
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
                AirlinerType destType = Airliner.Airliner.Type;

                Boolean canFlyRoute = originType.TypeAirliner == destType.TypeAirliner && TimeTableHelpers.IsTimeTableValid(Airliner,configuration.Entries);

                if (canFlyRoute)
                {
                    clearTimeTable();

                    foreach (RouteTimeTableEntry entry in configuration.Entries)
                        Entries.Add(entry);
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

            var entries = Entries.Count;
            var aircraft = Airliner.Airliner.Type.Name;

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
                var configuration = new AirlinerScheduleConfiguration(name, Airliner.Airliner.Type);

                foreach (RouteTimeTableEntry entry in Entries)
                    configuration.AddEntry(entry);

                Configurations.AddConfiguration(configuration);
            }
        }
        private void btnSaveContract_Click(Object sender, RoutedEventArgs e)
        {
            Route route = (Route)cbSpecialRoute.SelectedItem;

            RouteTimeTable rtt = new RouteTimeTable(route);

            RouteTimeTableEntry entry = new RouteTimeTableEntry(rtt, DayOfWeek.Wednesday, new TimeSpan(12, 0, 0), new RouteEntryDestination(route.Destination2, "Charter"));
            entry.Airliner = Airliner;
            rtt.AddEntry(entry);

            route.TimeTable = rtt;

            Airliner.AddRoute(route);
            Airliner.Status = FleetAirliner.AirlinerStatus.OnCharter;

            

       
        }
        private void btnSave_Click(Object sender, RoutedEventArgs e)
        {
            var oldEntries =
                new List<RouteTimeTableEntry>(
                    Airliner.Routes.SelectMany(r => r.TimeTable.Entries.Where(en => en.Airliner == Airliner)));

            IEnumerable<RouteTimeTableEntry> deleteEntries = oldEntries.Except(Entries);
            IEnumerable<RouteTimeTableEntry> newEntries = Entries.Except(oldEntries);

            foreach (RouteTimeTableEntry entry in newEntries)
            {
                entry.TimeTable.Route.TimeTable.AddEntry(entry);

                if (!Airliner.Routes.Contains(entry.TimeTable.Route))
                {
                    Airliner.AddRoute(entry.TimeTable.Route);
                }
            }

            foreach (RouteTimeTableEntry entry in deleteEntries)
            {
                entry.TimeTable.RemoveEntry(entry);
          
                if (entry.TimeTable.Entries.Count(en => en.Airliner == Airliner) == 0 || entry.TimeTable.Entries.Count == 0)
                {
                    Airliner.RemoveRoute(entry.TimeTable.Route);
                }
            }

            Airliner.Status = FleetAirliner.AirlinerStatus.Stopped;
 
        }

        private void btnSwap_Click(object sender, RoutedEventArgs e)
        {
            var outbound = (Airport)cbOutbound.SelectedItem;
            var route = (Route)cbHomebound.SelectedItem;

            cbOutbound.SelectedItem = route.Destination1 == outbound ? route.Destination2 : route.Destination1;

            cbHomebound.SelectedItem = route;
        }

        private void btnTransfer_Click(object sender, RoutedEventArgs e)
        {
            var cbAirliners = new ComboBox();
            cbAirliners.SetResourceReference(StyleProperty, "ComboBoxTransparentStyle");
            cbAirliners.SelectedValuePath = "Name";
            cbAirliners.DisplayMemberPath = "Name";
            cbAirliners.HorizontalAlignment = HorizontalAlignment.Left;
            cbAirliners.Width = 200;

            long maxDistance = Airliner.Airliner.Range;

            long requiredRunway = Airliner.Airliner.MinRunwaylength;

            List<FleetAirliner> airliners =
                GameObject.GetInstance()
                    .HumanAirline.Fleet.FindAll(
                        a =>
                            a != Airliner && a.Routes.Count > 0 && a.Status == FleetAirliner.AirlinerStatus.Stopped
                            && a.Routes.Max(r => r.GetDistance()) <= maxDistance);

            foreach (FleetAirliner airliner in airliners)
            {
                cbAirliners.Items.Add(airliner);
            }

            cbAirliners.SelectedIndex = 0;

            if (PopUpSingleElement.ShowPopUp(
                Translator.GetInstance().GetString("PopUpAirlinerRoutes", "1001"),
                cbAirliners) == PopUpSingleElement.ButtonSelected.OK && cbAirliners.SelectedItem != null)
            {
                clearTimeTable();

                var transferAirliner = (FleetAirliner)cbAirliners.SelectedItem;

                foreach (Route route in transferAirliner.Routes)
                {
                    foreach (
                        RouteTimeTableEntry entry in
                            route.TimeTable.Entries.FindAll(en => en.Airliner == transferAirliner))
                    {
                        entry.Airliner = Airliner;

                        Entries.Add(entry);
                    }

                    if (!Airliner.Routes.Contains(route))
                    {
                        Airliner.AddRoute(route);
                        Routes.Add(route);
                    }
                }
                transferAirliner.Routes.Clear();
            }
        }

        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            clearTimeTable();

            foreach (
                RouteTimeTableEntry entry in
                    Airliner.Routes.SelectMany(r => r.TimeTable.Entries.Where(en => en.Airliner == Airliner)))
            {
                Entries.Add(entry);
            }
        }

        private void cbDelayMinutes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var route = (Route)cbRoute.SelectedItem;

            if (route != null && cbDelayMinutes != null && cbSchedule != null && cbIntervalType != null)
            {
                TimeSpan routeFlightTime = route.GetFlightTime(Airliner.Airliner.Type);

                var delayMinutes = (int)cbDelayMinutes.SelectedItem;

                TimeSpan minFlightTime = routeFlightTime.Add(new TimeSpan(0, delayMinutes, 0));

                int maxHours = 22 - 06; //from 06.00 to 22.00

                var opsType = (OpsType)cbSchedule.SelectedItem;
                var intervalType = (IntervalType)cbIntervalType.SelectedItem;

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

                Intervals.Clear();

                for (int i = 0; i < Math.Max(1, flightsPerDay); i++)
                {
                    Intervals.Add(i + 1);
                }

                if (cbInterval != null)
                {
                    cbInterval.SelectedIndex = 0;
                }
            }
        }

        private void cbHomebound_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var route = (Route)cbHomebound.SelectedItem;

            if (route != null)
            {
                IsLongRoute =
                    MathHelpers.GetFlightTime(route.Destination1, route.Destination2, Airliner.Airliner.Type)
                        .Add(FleetAirlinerHelpers.GetMinTimeBetweenFlights(Airliner.Airliner.Type))
                        .TotalHours > 12;
            }
            else
            {
                IsLongRoute = false;
            }
        }

        private void cbIntervalType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var route = (Route)cbRoute.SelectedItem;

            if (route != null)
            {
                OpsType opsType = cbSchedule == null ? OpsType.Regular : (OpsType)cbSchedule.SelectedItem;
                var intervalType = (IntervalType)cbIntervalType.SelectedItem;

                TimeSpan routeFlightTime = route.GetFlightTime(Airliner.Airliner.Type);

                var delayMinutes = (int)cbDelayMinutes.SelectedItem;

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

                Intervals.Clear();

                for (int i = 0; i < Math.Max(1, flightsPerDay); i++)
                {
                    Intervals.Add(i + 1);
                }

                if (cbInterval != null)
                {
                    cbInterval.SelectedIndex = 0;
                }
            }
        }

        private void cbInterval_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var route = (Route)cbRoute.SelectedItem;

            if (cbSchedule != null && cbIntervalType != null)
            {
                var opsType = (OpsType)cbSchedule.SelectedItem;
                var intervalType = (IntervalType)cbIntervalType.SelectedItem;

                if (route != null && intervalType == IntervalType.Day && ((ComboBox)sender).SelectedItem != null)
                {
                    var interval = (int)((ComboBox)sender).SelectedItem;

                    int latestStartTime = 22;

                    TimeSpan routeFlightTime = route.GetFlightTime(Airliner.Airliner.Type);

                    var delayMinutes = (int)cbDelayMinutes.SelectedItem;

                    TimeSpan minFlightTime = routeFlightTime.Add(new TimeSpan(0, delayMinutes, 0));

                    var lastDepartureHour = (int)(latestStartTime - (minFlightTime.TotalHours * interval * 2));

                    StartTimes.Clear();

                    int startTime = 6;

                    if (opsType == OpsType.Whole_Day)
                    {
                        startTime = 0;
                    }

                    for (int i = startTime; i <= Math.Max(startTime, lastDepartureHour); i++)
                    {
                        for (int j = 0; j < 60; j += 15)
                        {
                            StartTimes.Add(new TimeSpan(i, j, 0));
                        }
                    }

                    if (cbStartTime != null)
                    {
                        cbStartTime.SelectedIndex = 0;
                    }
                }
            }
        }

        private void cbOutbound_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var outbuound = (Airport)((ComboBox)sender).SelectedItem;

            IEnumerable<Route> routes =
                Airliner.Airliner.Airline.Routes.Where(
                    r => r.Destination2 == outbuound || r.Destination1 == outbuound);

            if (cbHomebound != null)
            {
                cbHomebound.Items.Clear();

                foreach (Route route in routes)
                {
                    cbHomebound.Items.Add(route);
                }

                cbHomebound.SelectedIndex = 0;
            }
        }

        private void cbRegion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var region = (Region)((ComboBox)sender).SelectedItem;

            if (cbRoute != null)
            {
                var source = cbRoute.Items as ICollectionView;
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

            if (route != null && cbDelayMinutes != null && cbSchedule != null && cbIntervalType != null)
            {
                TimeSpan routeFlightTime = route.GetFlightTime(Airliner.Airliner.Type);

                var delayMinutes = (int)cbDelayMinutes.SelectedItem;

                TimeSpan minFlightTime = routeFlightTime.Add(new TimeSpan(0, delayMinutes, 0));

                int maxHours = 22 - 06; //from 06.00 to 22.00

                var opsType = (OpsType)cbSchedule.SelectedItem;
                var intervalType = (IntervalType)cbIntervalType.SelectedItem;

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

                Intervals.Clear();

                for (int i = 0; i < Math.Max(1, flightsPerDay); i++)
                {
                    Intervals.Add(i + 1);
                }
            }
        }

        private void cbSchedule_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var route = (Route)cbRoute.SelectedItem;

            if (cbSchedule != null && cbIntervalType != null)
            {
                var opsType = (OpsType)cbSchedule.SelectedItem;
                var intervalType = (IntervalType)cbIntervalType.SelectedItem;

                if (route != null && intervalType == IntervalType.Day && ((ComboBox)sender).SelectedItem != null)
                {
                    var interval = (int)((ComboBox)sender).SelectedItem;

                    int latestStartTime = 22;

                    TimeSpan routeFlightTime = route.GetFlightTime(Airliner.Airliner.Type);

                    var delayMinutes = (int)cbDelayMinutes.SelectedItem;

                    TimeSpan minFlightTime = routeFlightTime.Add(new TimeSpan(0, delayMinutes, 0));

                    var lastDepartureHour = (int)(latestStartTime - (minFlightTime.TotalHours * interval * 2));

                    StartTimes.Clear();

                    int startTime = 6;

                    if (opsType == OpsType.Whole_Day)
                    {
                        startTime = 0;
                    }

                    for (int i = startTime; i <= Math.Max(startTime, lastDepartureHour); i++)
                    {
                        for (int j = 0; j < 60; j += 15)
                        {
                            StartTimes.Add(new TimeSpan(i, j, 0));
                        }
                    }

                    if (cbStartTime != null)
                    {
                        cbStartTime.SelectedIndex = 0;
                    }
                }
            }
        }

        private void clearTimeTable()
        {
            var entries = new ObservableCollection<RouteTimeTableEntry>(Entries);
            
            foreach (RouteTimeTableEntry entry in entries)
            {
                Entries.Remove(entry);
            }
        }

        //returns the selected days
        private List<DayOfWeek> getSelectedDays()
        {
            var days = new List<DayOfWeek>();

            if (cbMonday.IsChecked.Value)
            {
                days.Add(DayOfWeek.Monday);
            }
            if (cbTuesday.IsChecked.Value)
            {
                days.Add(DayOfWeek.Tuesday);
            }
            if (cbWednesday.IsChecked.Value)
            {
                days.Add(DayOfWeek.Wednesday);
            }
            if (cbThursday.IsChecked.Value)
            {
                days.Add(DayOfWeek.Thursday);
            }
            if (cbFriday.IsChecked.Value)
            {
                days.Add(DayOfWeek.Friday);
            }
            if (cbSaturday.IsChecked.Value)
            {
                days.Add(DayOfWeek.Saturday);
            }
            if (cbSunday.IsChecked.Value)
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
                ShowSeason = Weather.Season.Winter;
            }

            if (season == "Summer")
            {
                ShowSeason = Weather.Season.Summer;
            }

            if (season == "AllYear")
            {
                ShowSeason = Weather.Season.AllYear;
            }

            setViewEntries();
        }

        private void setCanTransferSchedule()
        {
            long maxDistance = Airliner.Airliner.Range;

            long requiredRunway = Airliner.Airliner.MinRunwaylength;

            CanTransferSchedule =
                GameObject.GetInstance()
                    .HumanAirline.Fleet.FindAll(
                        a =>
                            a != Airliner && a.Routes.Count > 0 && a.Status == FleetAirliner.AirlinerStatus.Stopped
                            && a.Routes.Max(r => r.GetDistance()) <= maxDistance)
                    .Count > 0;
        }

        private void setFlightNumbers()
        {
            string number = "";

            int i = 0;

            while (number == "" || Entries.FirstOrDefault(e => e.Destination.FlightCode == number) != null)
            {
                number = GameObject.GetInstance().HumanAirline.GetNextFlightCode(i);

                i++;
            }

            number = number.Substring(2);

            txtFlightNumber.Text = String.Format("{0:0000}", number);
            txtSchedulerFlightNumber.Text = String.Format("{0:0000}", number);
        }

        //sets the view entries
        private void setViewEntries()
        {
            var entries = new ObservableCollection<RouteTimeTableEntry>(ViewEntries);

            foreach (RouteTimeTableEntry entry in entries)
            {
                ViewEntries.Remove(entry);
            }

            foreach (RouteTimeTableEntry entry in Entries.Where(e => e.TimeTable.Route.Season == ShowSeason))
            {
                ViewEntries.Add(entry);
            }
        }

        #endregion
    }
}