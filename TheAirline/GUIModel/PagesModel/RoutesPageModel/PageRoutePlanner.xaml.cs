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
    public partial class PageRoutePlanner : Page
    {
        public FleetAirliner Airliner { get; set; }
        public List<RoutePlannerItemMVVM> AllRoutes { get; set; }
        public List<Region> AllRegions { get; set; }
        public List<Route> Routes { get; set; }
        public List<Airport> OutboundAirports { get; set; }
        public ObservableCollection<RouteTimeTableEntry> Entries { get; set; }
        public ObservableCollection<TimeSpan> StartTimes { get; set; }
        public List<int> StopoverMinutes { get; set; }
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
        public List<int> Intervals { get; set; }
        public PageRoutePlanner(FleetAirliner airliner)
        {
            this.Airliner = airliner;
            this.Entries = new ObservableCollection<RouteTimeTableEntry>();

            foreach (RouteTimeTableEntry entry in this.Airliner.Routes.SelectMany(r => r.TimeTable.Entries.Where(en => en.Airliner == this.Airliner)))
                this.Entries.Add(entry);

            this.AllRoutes = new List<RoutePlannerItemMVVM>();
            this.Intervals = new List<int>() { 1, 2, 3, 4, 5, 6 };

            this.Routes = this.Airliner.Airliner.Airline.Routes;

            this.AllRegions = new List<Region>();
            this.AllRegions.Add(Regions.GetRegion("100"));

            var routeRegions = this.Routes.Select(r => r.Destination1.Profile.Country.Region).ToList();
            routeRegions.AddRange(this.Routes.Select(r => r.Destination2.Profile.Country.Region));

            foreach (Region region in routeRegions.Distinct())
                this.AllRegions.Add(region);

            foreach (Route route in this.Airliner.Airliner.Airline.Routes)
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
            this.Entries.Clear();

            foreach (RouteTimeTableEntry entry in this.Airliner.Routes.SelectMany(r => r.TimeTable.Entries.Where(en => en.Airliner == this.Airliner)))
                this.Entries.Add(entry);
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            this.Entries.Clear();
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
                if (opsType == OpsType.Regular)
                    rt = AIHelpers.CreateAirlinerRouteTimeTable(route, this.Airliner, interval, true, delayMinutes, startTime, flightcode1, flightcode2);

   
            }
            if (intervalType == IntervalType.Week && opsType != OpsType.Business)
            {
                if (opsType == OpsType.Regular)
                    rt = AIHelpers.CreateAirlinerRouteTimeTable(route, this.Airliner, interval, false, delayMinutes, startTime, flightcode1, flightcode2);

            }
            if (intervalType == IntervalType.Biweek && opsType != OpsType.Business)
            {
            }
  
            if (!TimeTableHelpers.IsTimeTableValid(rt, this.Airliner, this.Entries.ToList(), false))
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2705"), Translator.GetInstance().GetString("MessageBox", "2705", "message"), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    this.Entries.Clear();

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

            if (!TimeTableHelpers.IsTimeTableValid(rt, this.Airliner, this.Entries.ToList()))
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2706"), Translator.GetInstance().GetString("MessageBox", "2706", "message"), WPFMessageBoxButtons.Ok);
            else
            {

                foreach (RouteTimeTableEntry entry in rt.Entries)
                {
                    this.Entries.Add(entry);

                }

            }


        }



    }
}
