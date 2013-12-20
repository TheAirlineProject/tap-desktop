using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GUIModel.CustomControlsModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GUIModel.PagesModel.RoutesPageModel
{
    /// <summary>
    /// Interaction logic for PageRoutePlanner.xaml
    /// </summary>
    public partial class PageRoutePlanner : Page, INotifyPropertyChanged
    {
        public FleetAirliner Airliner { get; set; }
        public List<RoutePlannerItemMVVM> AllRoutes { get; set; }
        public List<Region> AllRegions { get; set; }
        public List<Route> Routes { get; set; }
        public List<Airport> OutboundAirports { get; set; }
        public ObservableCollection<RouteTimeTableEntry> Entries { get; set; }
        public ObservableCollection<TimeSpan> StartTimes { get; set; }
        public List<int> StopoverMinutes { get; set; }
        public ObservableCollection<int> Intervals { get; set; }
        private Point startPoint;
        private Boolean _islongroute;
        public Boolean IsLongRoute
        {
            get { return _islongroute; }
            set { this._islongroute = value; NotifyPropertyChanged("IsLongRoute"); }
        }
        public List<IntervalType> IntervalTypes
        {
            get { return Enum.GetValues(typeof(IntervalType)).Cast<IntervalType>().ToList(); }
            private set { ;}
        }
        public List<OpsType> OpsTypes
        {
            get { return Enum.GetValues(typeof(OpsType)).Cast<OpsType>().ToList(); }
            private set { ;}

        }
       
        public PageRoutePlanner(FleetAirliner airliner)
        {
            this.Airliner = airliner;
            this.Entries = new ObservableCollection<RouteTimeTableEntry>();
            this.Entries.CollectionChanged += Entries_CollectionChanged;

            this.IsLongRoute = false;

            this.AllRoutes = new List<RoutePlannerItemMVVM>();
            this.Intervals = new ObservableCollection<int>() { 1, 2, 3, 4, 5, 6 };

            this.Routes = this.Airliner.Airliner.Airline.Routes.Where(r => r.getDistance() <= this.Airliner.Airliner.Type.Range).ToList();

            this.AllRegions = new List<Region>();
            this.AllRegions.Add(Regions.GetRegion("100"));

            var routeRegions = this.Routes.Select(r => r.Destination1.Profile.Country.Region).ToList();
            routeRegions.AddRange(this.Routes.Select(r => r.Destination2.Profile.Country.Region));

            foreach (Region region in routeRegions.Distinct())
                this.AllRegions.Add(region);

            foreach (Route route in this.Airliner.Airliner.Airline.Routes.Where(r=>r.getDistance()<= this.Airliner.Airliner.Type.Range))
                this.AllRoutes.Add(new RoutePlannerItemMVVM(route, this.Airliner.Airliner.Type));

            this.OutboundAirports = new List<Airport>();
            var routeAirports = this.Routes.Select(r => r.Destination1).ToList();
            routeAirports.AddRange(this.Routes.Select(r => r.Destination2));

            foreach (Airport airport in routeAirports.Distinct())
                this.OutboundAirports.Add(airport);

            this.StartTimes = new ObservableCollection<TimeSpan>();

            for (int i = 0; i < 20; i++)
                this.StartTimes.Add(new TimeSpan(6, i * 15, 0));

            this.StopoverMinutes = new List<int>() { 45, 60, 75, 90, 105, 120 };

            this.Loaded += PageRoutePlanner_Loaded;

            InitializeComponent();
        }
        //clears the time table
        private void clearTimeTable()
        {
             var entries = new ObservableCollection<RouteTimeTableEntry>(this.Entries);

            foreach (RouteTimeTableEntry entry in entries)
                this.Entries.Remove(entry);
        }
        

        private void PageRoutePlanner_Loaded(object sender, RoutedEventArgs e)
        {
            TabControl tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                var matchingItem =
     tab_main.Items.Cast<TabItem>()
       .Where(item => item.Tag.ToString() == "Route")
       .FirstOrDefault();

                matchingItem.Visibility = System.Windows.Visibility.Collapsed;
            }

            foreach (RouteTimeTableEntry entry in this.Airliner.Routes.SelectMany(r => r.TimeTable.Entries.Where(en => en.Airliner == this.Airliner)))
                this.Entries.Add(entry);

            cbRoute.SelectedIndex = 0;
            cbOutbound.SelectedIndex = 0;

        }

        private void cbRegion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Region region = (Region)((ComboBox)sender).SelectedItem;

            if (cbRoute != null)
            {
                var source = cbRoute.Items as ICollectionView;
                source.Filter = o =>
                {
                    Route r = o as Route;
                    return r.Destination1.Profile.Country.Region == region || r.Destination2.Profile.Country.Region == region || region.Uid == "100";
                };
            }



        }
        private void cbRoute_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Route route = (Route)((ComboBox)sender).SelectedItem;

            if (route != null && cbDelayMinutes != null && cbSchedule != null && cbIntervalType != null)
            {
                TimeSpan routeFlightTime = route.getFlightTime(this.Airliner.Airliner.Type);

                int delayMinutes = (int)cbDelayMinutes.SelectedItem;

                TimeSpan minFlightTime = routeFlightTime.Add(new TimeSpan(0, delayMinutes, 0));

                int maxHours = 22 - 06; //from 06.00 to 22.00

                OpsType opsType = (OpsType)cbSchedule.SelectedItem;
                IntervalType intervalType = (IntervalType)cbIntervalType.SelectedItem;

                if (opsType == OpsType.Whole_Day)
                    maxHours = 24;

                int flightsPerDay = (int)Math.Floor((maxHours * 60) / (2 * minFlightTime.TotalMinutes));//Convert.ToInt16(maxHours * 60 / (2 * minFlightTime.TotalMinutes));
          
                if (intervalType == IntervalType.Week)
                    flightsPerDay = 7;

                this.Intervals.Clear();

                for (int i = 0; i < Math.Max(1, flightsPerDay); i++)
                    this.Intervals.Add(i + 1);


            }
        }
        private void cbDelayMinutes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Route route = (Route)cbRoute.SelectedItem;

            if (route != null && cbDelayMinutes != null && cbSchedule != null && cbIntervalType != null)
            {
                TimeSpan routeFlightTime = route.getFlightTime(this.Airliner.Airliner.Type);

                int delayMinutes = (int)cbDelayMinutes.SelectedItem;

                TimeSpan minFlightTime = routeFlightTime.Add(new TimeSpan(0, delayMinutes, 0));

                int maxHours = 22 - 06; //from 06.00 to 22.00

                OpsType opsType = (OpsType)cbSchedule.SelectedItem;
                IntervalType intervalType = (IntervalType)cbIntervalType.SelectedItem;

                if (opsType == OpsType.Whole_Day)
                    maxHours = 24;

                int flightsPerDay = (int)Math.Floor((maxHours * 60) / (2 * minFlightTime.TotalMinutes));//Convert.ToInt16(maxHours * 60 / (2 * minFlightTime.TotalMinutes));
                
                if (intervalType == IntervalType.Week)
                    flightsPerDay = 6;

                this.Intervals.Clear();

                for (int i = 0; i < Math.Max(1, flightsPerDay); i++)
                    this.Intervals.Add(i + 1);

                if (cbInterval != null)
                    cbInterval.SelectedIndex = 0;
            }

        }

        private void cbIntervalType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Route route = (Route)cbRoute.SelectedItem;

            if (route != null)
            {
                OpsType opsType = cbSchedule == null ? OpsType.Regular : (OpsType)cbSchedule.SelectedItem;
                IntervalType intervalType = (IntervalType)cbIntervalType.SelectedItem;

                TimeSpan routeFlightTime = route.getFlightTime(this.Airliner.Airliner.Type);

                int delayMinutes = (int)cbDelayMinutes.SelectedItem;

                TimeSpan minFlightTime = routeFlightTime.Add(new TimeSpan(0, delayMinutes, 0));

                int maxHours = 22 - 06;

                if (opsType == OpsType.Whole_Day)
                    maxHours = 24;

                int flightsPerDay = (int)Math.Floor((maxHours * 60) / (2 * minFlightTime.TotalMinutes));//Convert.ToInt16(maxHours * 60 / (2 * minFlightTime.TotalMinutes));
          
                if (intervalType == IntervalType.Week)
                    flightsPerDay = 7;

                this.Intervals.Clear();

                for (int i = 0; i < Math.Max(1, flightsPerDay); i++)
                    this.Intervals.Add(i + 1);

                if (cbInterval != null)
                    cbInterval.SelectedIndex = 0;
            }

        }
        private void cbInterval_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Route route = (Route)cbRoute.SelectedItem;

            if (cbSchedule != null && cbIntervalType != null)
            {
                OpsType opsType = (OpsType)cbSchedule.SelectedItem;
                IntervalType intervalType = (IntervalType)cbIntervalType.SelectedItem;

                if (route != null && intervalType == IntervalType.Day && ((ComboBox)sender).SelectedItem != null)
                {
                    int interval = (int)((ComboBox)sender).SelectedItem;

                    int latestStartTime = 22;

                    TimeSpan routeFlightTime = route.getFlightTime(this.Airliner.Airliner.Type);

                    int delayMinutes = (int)cbDelayMinutes.SelectedItem;

                    TimeSpan minFlightTime = routeFlightTime.Add(new TimeSpan(0, delayMinutes, 0));

                    int lastDepartureHour = (int)(latestStartTime - (minFlightTime.TotalHours * interval * 2));

                    this.StartTimes.Clear();

                    int startTime = 6;

                    if (opsType == OpsType.Whole_Day)
                        startTime = 0;

                    for (int i = startTime; i <= Math.Max(startTime, lastDepartureHour); i++)
                        for (int j = 0; j < 60; j += 15)
                            this.StartTimes.Add(new TimeSpan(i, j, 0));

                    if (cbStartTime != null)
                        cbStartTime.SelectedIndex = 0;
                }
            }
        }
        private void cbSchedule_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           Route route = (Route)cbRoute.SelectedItem;
           
           if (cbSchedule != null && cbIntervalType != null)
           {
               OpsType opsType = (OpsType)cbSchedule.SelectedItem;
               IntervalType intervalType = (IntervalType)cbIntervalType.SelectedItem;

               if (route != null && intervalType == IntervalType.Day && ((ComboBox)sender).SelectedItem != null)
               {
                   int interval = (int)((ComboBox)sender).SelectedItem;

                   int latestStartTime = 22;

                   TimeSpan routeFlightTime = route.getFlightTime(this.Airliner.Airliner.Type);

                   int delayMinutes = (int)cbDelayMinutes.SelectedItem;

                   TimeSpan minFlightTime = routeFlightTime.Add(new TimeSpan(0, delayMinutes, 0));

                   int lastDepartureHour = (int)(latestStartTime - (minFlightTime.TotalHours * interval * 2));

                   this.StartTimes.Clear();

                   int startTime = 6;

                   if (opsType == OpsType.Whole_Day)
                       startTime = 0;

                   for (int i = startTime; i <= Math.Max(startTime, lastDepartureHour); i++)
                       for (int j = 0; j < 60; j += 15)
                           this.StartTimes.Add(new TimeSpan(i, j, 0));

                   if (cbStartTime != null)
                       cbStartTime.SelectedIndex = 0;
               }
           }

        }

        private void cbOutbound_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Airport outbuound = (Airport)((ComboBox)sender).SelectedItem;

            var routes = this.Airliner.Airliner.Airline.Routes.Where(r => r.Destination2 == outbuound || r.Destination1 == outbuound);

            if (cbHomebound != null)
            {
                cbHomebound.Items.Clear();

                foreach (Route route in routes)
                    cbHomebound.Items.Add(route);

                cbHomebound.SelectedIndex = 0;
            }

        }

        private void btnSwap_Click(object sender, RoutedEventArgs e)
        {
            Airport outbound = (Airport)cbOutbound.SelectedItem;
            Route route = (Route)cbHomebound.SelectedItem;

            cbOutbound.SelectedItem = route.Destination1 == outbound ? route.Destination2 : route.Destination1;

            cbHomebound.SelectedItem = route;
        }
        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            clearTimeTable();

            foreach (RouteTimeTableEntry entry in this.Airliner.Routes.SelectMany(r => r.TimeTable.Entries.Where(en => en.Airliner == this.Airliner)))
                this.Entries.Add(entry);
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
           clearTimeTable();
        }
        private void btnSave_Click(Object sender, RoutedEventArgs e)
        {
            var oldEntries = new List<RouteTimeTableEntry>(this.Airliner.Routes.SelectMany(r => r.TimeTable.Entries.Where(en => en.Airliner == this.Airliner)));

            var deleteEntries = oldEntries.Except(this.Entries);
            var newEntries = this.Entries.Except(oldEntries);

            foreach (RouteTimeTableEntry entry in newEntries)
            {
                entry.TimeTable.Route.TimeTable.addEntry(entry);

                if (!this.Airliner.Routes.Contains(entry.TimeTable.Route))
                    this.Airliner.addRoute(entry.TimeTable.Route);
            }

            foreach (RouteTimeTableEntry entry in deleteEntries)
            {
                entry.TimeTable.Route.TimeTable.removeEntry(entry);

                if (entry.TimeTable.Entries.Count(en => en.Airliner == this.Airliner) == 0)
                    this.Airliner.removeRoute(entry.TimeTable.Route);
            }
        }
        private void btnAddGenerator_Click(object sender, RoutedEventArgs e)
        {
            Route route = (Route)cbRoute.SelectedItem;

            RouteTimeTable rt = null;

            IntervalType intervalType = (IntervalType)cbIntervalType.SelectedItem;
            int interval = Convert.ToInt16(cbInterval.SelectedItem);
            OpsType opsType = (OpsType)cbSchedule.SelectedItem;
            int delayMinutes = (int)cbDelayMinutes.SelectedItem;

            double maxBusinessRouteTime = new TimeSpan(2, 0, 0).TotalMinutes;
            TimeSpan startTime = (TimeSpan)cbStartTime.SelectedItem;

            string flightcode1 = this.Airliner.Airliner.Airline.Profile.IATACode + txtFlightNumber.Text;
            string flightcode2 = this.Airliner.Airliner.Airline.getFlightCodes()[this.Airliner.Airliner.Airline.getFlightCodes().IndexOf(flightcode1) + 1];

            if (opsType == OpsType.Business)
            {
                int flightsPerDay = (int)(route.getFlightTime(this.Airliner.Airliner.Type).Add(FleetAirlinerHelpers.GetMinTimeBetweenFlights(this.Airliner)).TotalMinutes / 2 / maxBusinessRouteTime);
                rt = AIHelpers.CreateBusinessRouteTimeTable(route, this.Airliner, Math.Max(1, flightsPerDay), flightcode1, flightcode2);

            }
            if (intervalType == IntervalType.Day && opsType != OpsType.Business)
            {
                rt = AIHelpers.CreateAirlinerRouteTimeTable(route, this.Airliner, interval, true, delayMinutes, startTime, flightcode1, flightcode2);

            }
            if (intervalType == IntervalType.Week && opsType != OpsType.Business)
            {
                 rt = AIHelpers.CreateAirlinerRouteTimeTable(route, this.Airliner, interval, false, delayMinutes, startTime, flightcode1, flightcode2);

            }
            if (!TimeTableHelpers.IsTimeTableValid(rt, this.Airliner, this.Entries.ToList(), false))
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2705"), Translator.GetInstance().GetString("MessageBox", "2705", "message"), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    clearTimeTable();

                    foreach (RouteTimeTableEntry entry in rt.Entries)
                        if (!TimeTableHelpers.IsRouteEntryInOccupied(entry, this.Airliner))
                        {
                            this.Entries.Add(entry);

                        }
                }
            }
            else
            {

                foreach (RouteTimeTableEntry entry in rt.Entries)
                    if (!TimeTableHelpers.IsRouteEntryInOccupied(entry, this.Airliner))
                    {
                        this.Entries.Add(entry);


                    }
            }
           
           

        }
        private void EntryDeleted_Event(object sender, System.Windows.RoutedEventArgs e)
        {
            TimelineEntry entry = (TimelineEntry)e.OriginalSource;

            RouteTimeTableEntry rEntry = (RouteTimeTableEntry)entry.Source;

            this.Entries.Remove(rEntry);
         
        }
        private void EntryAdded_Event(object sender, System.Windows.RoutedEventArgs e)
        {
           
            TimelineDropItem item = e.OriginalSource as TimelineDropItem;
            /*
             RouteTimeTable rt = new RouteTimeTable(item.Object);

            string flightCode = this.Airliner.Airliner.Airline.Profile.IATACode + txtSchedulerFlightNumber.Text;

            RouteTimeTableEntry rEntry = new RouteTimeTableEntry(rt,item.Day,item.Time,new RouteEntryDestination(,flightCode));
            
            this.Entries.Add(rEntry);
            */

        }
        private void EntryChanged_Event(object sender, System.Windows.RoutedEventArgs e)
        {
            object[] entries =  e.OriginalSource as object[];
            
            TimelineEntry oldEntry = entries[0] as TimelineEntry;
            TimelineEntry newEntry = entries[1] as TimelineEntry;

            RouteTimeTableEntry oEntry = (RouteTimeTableEntry)oldEntry.Source;

            RouteTimeTable rt = new RouteTimeTable(oEntry.TimeTable.Route);
            RouteTimeTableEntry nEntry = new RouteTimeTableEntry(rt,(DayOfWeek)newEntry.StartTime.Days,newEntry.StartTime,oEntry.Destination);
            nEntry.Airliner = this.Airliner;
            rt.addEntry(nEntry);
            
            List<RouteTimeTableEntry> tEntries = new List<RouteTimeTableEntry>(this.Entries);
            tEntries.Remove(oEntry);

            if (!TimeTableHelpers.IsTimeTableValid(rt, this.Airliner, tEntries))
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2706"), Translator.GetInstance().GetString("MessageBox", "2706", "message"), WPFMessageBoxButtons.Ok);
            else
            {
                this.Entries.Remove(oEntry);

                this.Entries.Add(nEntry);
            }

        }
        private void Entries_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
             ucTimeTable uctimetable = UIHelpers.FindChild<ucTimeTable>(this, "uctimetable");

             if (e.NewItems != null)
             {
                 foreach (RouteTimeTableEntry entry in e.NewItems)
                 {
                     TimeSpan sTime = new TimeSpan((int)entry.Day, entry.Time.Hours, entry.Time.Minutes, 0);
                     TimeSpan eTime = sTime.Add(entry.TimeTable.Route.getFlightTime(this.Airliner.Airliner.Type));

                     Guid g2 = new Guid(entry.TimeTable.Route.Id);

                     byte[] bytes = g2.ToByteArray();

                     byte red = bytes[0];
                     byte green = bytes[1];
                     byte blue = bytes[2];

                     SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(red, green, blue));
                     brush.Opacity = 0.60;

                     TimeSpan localTimeDept = MathHelpers.ConvertTimeSpanToLocalTime(sTime, entry.DepartureAirport.Profile.TimeZone);
                     TimeSpan localTimeDest = MathHelpers.ConvertTimeSpanToLocalTime(eTime, entry.Destination.Airport.Profile.TimeZone);

                   //  string text = string.Format("{0}-{1}\n{2}-{3}", new AirportCodeConverter().Convert(entry.DepartureAirport), new AirportCodeConverter().Convert(entry.Destination.Airport),string.Format("{0:hh\\:mm}", entry.Time),string.Format("{0:hh\\:mm}",localTimeDept));
                     string text = string.Format("{0}-{1}", new AirportCodeConverter().Convert(entry.DepartureAirport), new AirportCodeConverter().Convert(entry.Destination.Airport));
                   
                     string tooltip = string.Format("{0}-{3}\n({1} {2})-({4} {5})", string.Format("{0:hh\\:mm}", entry.Time),string.Format("{0:hh\\:mm}",localTimeDept), entry.DepartureAirport.Profile.TimeZone.ShortName, string.Format("{0:hh\\:mm}", eTime),string.Format("{0:hh\\:mm}",localTimeDest),entry.Destination.Airport.Profile.TimeZone.ShortName);
                
                     uctimetable.addTimelineEntry(entry, sTime, eTime, text, brush, tooltip);

                   }
             }

             if (e.OldItems != null)
             {
                 foreach (RouteTimeTableEntry entry in e.OldItems)
                 {
                     TimeSpan sTime = new TimeSpan((int)entry.Day, entry.Time.Hours, entry.Time.Minutes, 0);
                     TimeSpan eTime = sTime.Add(entry.TimeTable.Route.getFlightTime(this.Airliner.Airliner.Type));

                     string text = string.Format("{0}-{1}", new AirportCodeConverter().Convert(entry.DepartureAirport), new AirportCodeConverter().Convert(entry.Destination.Airport));

                     uctimetable.removeTimelineEntry(sTime, eTime, text);
                 }
             }
        }
        private void btnAddScheduler_Click(object sender, RoutedEventArgs e)
        {
            var radioButtons = UIHelpers.FindRBChildren(this, "Interval");

            string interval = "";
            foreach (RadioButton rbInterval in radioButtons)
            {
                if (rbInterval.IsChecked.Value)
                    interval = rbInterval.Tag.ToString();
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
                    addEntries(new List<DayOfWeek>() { DayOfWeek.Thursday, DayOfWeek.Tuesday, DayOfWeek.Saturday });
                    break;
                case "EveryOther":
                    addEntries(new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday, DayOfWeek.Sunday });
                    break;
                case "Weekend":
                    addEntries(new List<DayOfWeek>() { DayOfWeek.Sunday, DayOfWeek.Saturday });
                    break;
                case "MWF":
                    addEntries(new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday });
                    break;
                case "Weekdays":
                    addEntries(new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday });
                    break;

            }

        }
        //returns the selected days
        private List<DayOfWeek> getSelectedDays()
        {
            List<DayOfWeek> days = new List<DayOfWeek>();

            if (cbMonday.IsChecked.Value) days.Add(DayOfWeek.Monday);
            if (cbTuesday.IsChecked.Value) days.Add(DayOfWeek.Tuesday);
            if (cbWednesday.IsChecked.Value) days.Add(DayOfWeek.Wednesday);
            if (cbThursday.IsChecked.Value) days.Add(DayOfWeek.Thursday);
            if (cbFriday.IsChecked.Value) days.Add(DayOfWeek.Friday);
            if (cbSaturday.IsChecked.Value) days.Add(DayOfWeek.Saturday);
            if (cbSunday.IsChecked.Value) days.Add(DayOfWeek.Sunday);

            return days;

        }
        //adds entries to the planner
        private void addEntries(List<DayOfWeek> days)
        {
            Route route = (Route)cbHomebound.SelectedItem;

            Airport origin = (Airport)cbOutbound.SelectedItem;
            Airport airport = route.Destination1 == origin ? route.Destination2 : route.Destination1;

            TimeSpan time = new TimeSpan(tpTime.Value.Value.Hour, tpTime.Value.Value.Minute, 0);

            string flightCode = this.Airliner.Airliner.Airline.Profile.IATACode + txtSchedulerFlightNumber.Text;

            RouteTimeTable rt = new RouteTimeTable(route);

            foreach (DayOfWeek dayOfWeek in days)
            {
                RouteTimeTableEntry entry = new RouteTimeTableEntry(route.TimeTable, dayOfWeek, time, new RouteEntryDestination(airport, flightCode));
                entry.Airliner = this.Airliner;

                rt.addEntry(entry);
            }

            if (!TimeTableHelpers.IsRoutePlannerTimeTableValid(rt, this.Airliner, this.Entries.ToList()))
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2706"), Translator.GetInstance().GetString("MessageBox", "2706", "message"), WPFMessageBoxButtons.Ok);
            else
            {

                foreach (RouteTimeTableEntry entry in rt.Entries)
                {
                    this.Entries.Add(entry);

                }

            }


        }

        private void Rectangle_MouseMove(object sender, MouseEventArgs e)
        {
            // Get the current mouse position
            Point mousePos = e.GetPosition(null);
            Vector diff = startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                Rectangle rect = (Rectangle)sender;

                // Initialize the drag & drop operation
                DataObject dragData = new DataObject("Route", GameObject.GetInstance().HumanAirline.Airports[0]);
                DragDrop.DoDragDrop(rect, dragData, DragDropEffects.Move);
            } 
        }
        private void cbHomebound_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Route route = (Route)cbHomebound.SelectedItem;

            if (route != null)
                this.IsLongRoute = MathHelpers.GetFlightTime(route.Destination1, route.Destination2, this.Airliner.Airliner.Type).Add(FleetAirlinerHelpers.GetMinTimeBetweenFlights(this.Airliner.Airliner.Type)).TotalHours > 12;
            else
                this.IsLongRoute = false;

        }
        private void Rectangle_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(null);
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
