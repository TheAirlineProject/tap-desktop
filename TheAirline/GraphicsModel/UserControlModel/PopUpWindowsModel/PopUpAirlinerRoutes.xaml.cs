using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TheAirline.Model.AirlinerModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.AirportModel;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpAirlinerRoutes.xaml
    /// </summary>
    public partial class PopUpAirlinerRoutes : PopUpWindow
    {
        private FleetAirliner Airliner;
        private ComboBox cbHour, cbMinute, cbRoute, cbDay;
        private TextBlock txtFlightTime;
        private ListBox lbFlights;
        private Dictionary<Route, List<RouteTimeTableEntry>> Entries;
        private Dictionary<Route, List<RouteTimeTableEntry>> EntriesToDelete;
        public static object ShowPopUp(FleetAirliner airliner,Boolean editable)
        {
            PopUpWindow window = new PopUpAirlinerRoutes(airliner,editable);
            window.ShowDialog();

            return window.Selected;
        }
        public PopUpAirlinerRoutes(FleetAirliner airliner, Boolean editable)
        {
            this.Entries = new Dictionary<Route, List<RouteTimeTableEntry>>();
            this.EntriesToDelete = new Dictionary<Route, List<RouteTimeTableEntry>>();

            this.Airliner = airliner;

            InitializeComponent();

            this.Title = this.Airliner.Name;

            this.Width = 1200;

            this.Height = editable ? 325 : 250;

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            StackPanel mainPanel = new StackPanel();
            mainPanel.Margin = new Thickness(10, 10, 10, 10);

            lbFlights = new ListBox();
            lbFlights.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbFlights.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");
            lbFlights.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            mainPanel.Children.Add(lbFlights);

            if (editable)
            {
                mainPanel.Children.Add(createNewEntryPanel());
                mainPanel.Children.Add(createButtonsPanel());
            }

            this.Content = mainPanel;

            showFlights();
        }
        //creates the buttons panel
        private WrapPanel createButtonsPanel()
        {
            WrapPanel buttonsPanel = new WrapPanel();
            buttonsPanel.Margin = new Thickness(0, 10, 0, 0);

            Button btnOk = new Button();
            btnOk.Uid = "100";
            btnOk.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnOk.Height = Double.NaN;
            btnOk.Width = Double.NaN;
            btnOk.Content = Translator.GetInstance().GetString("General", btnOk.Uid);
            btnOk.Click += new RoutedEventHandler(btnOk_Click);
            btnOk.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");

            buttonsPanel.Children.Add(btnOk);

            Button btnCancel = new Button();
            btnCancel.Uid = "101";
            btnCancel.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnCancel.Height = Double.NaN;
            btnCancel.Margin = new Thickness(5, 0, 0, 0);
            btnCancel.Width = Double.NaN;
            btnCancel.Click += new RoutedEventHandler(btnCancel_Click);
            btnCancel.Content = Translator.GetInstance().GetString("General", btnCancel.Uid);
            btnCancel.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");

            buttonsPanel.Children.Add(btnCancel);

            Button btnUndo = new Button();
            btnUndo.Uid = "103";
            btnUndo.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnUndo.Height = Double.NaN;
            btnUndo.Width = Double.NaN;
            btnUndo.Margin = new Thickness(5, 0, 0, 0);
            btnUndo.Content = Translator.GetInstance().GetString("General", btnUndo.Uid);
            btnUndo.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnUndo.Click += new RoutedEventHandler(btnUndo_Click);

            buttonsPanel.Children.Add(btnUndo);

            return buttonsPanel;
        }
        //creates the panel for adding a new entry
        private StackPanel createNewEntryPanel()
        {
            StackPanel newEntryPanel = new StackPanel();
            newEntryPanel.Margin = new Thickness(0, 10, 0, 0);

            WrapPanel entryPanel = new WrapPanel();

            newEntryPanel.Children.Add(entryPanel);

            cbRoute = new ComboBox();

            cbRoute.ItemTemplate = this.Resources["RouteItem"] as DataTemplate;
            cbRoute.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");

            cbRoute.SelectionChanged += new SelectionChangedEventHandler(cbRoute_SelectionChanged);
            foreach (Route route in this.Airliner.Airliner.Airline.Routes)
            {
                ComboBoxItem item1 = new ComboBoxItem();
                item1.Tag = new KeyValuePair<Route, Airport>(route, route.Destination2);
                item1.Content = string.Format("{0}-{1}", route.Destination1.Profile.IATACode, route.Destination2.Profile.IATACode);
                cbRoute.Items.Add(item1);

                ComboBoxItem item2 = new ComboBoxItem();
                item2.Tag = new KeyValuePair<Route, Airport>(route, route.Destination1);
                item2.Content = string.Format("{0}-{1}", route.Destination2.Profile.IATACode, route.Destination1.Profile.IATACode);
                cbRoute.Items.Add(item2);
            }

          
            entryPanel.Children.Add(cbRoute);

            cbDay = new ComboBox();
            cbDay.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbDay.Width = 100;
            cbDay.Margin = new Thickness(10, 0, 0, 0);
            cbDay.Items.Add("Daily");

            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                cbDay.Items.Add(day);

            cbDay.SelectedIndex = 0;

            entryPanel.Children.Add(cbDay);

            cbHour = new ComboBox();
            cbHour.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbHour.ItemStringFormat = "{0:D2}";
            cbHour.Margin = new Thickness(5, 0, 0, 0);

            for (int i = 0; i < 24; i++)
                cbHour.Items.Add(i);

            cbHour.SelectedIndex = 0;

            entryPanel.Children.Add(cbHour);

            entryPanel.Children.Add(UICreator.CreateTextBlock(":"));

            cbMinute = new ComboBox();
            cbMinute.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbMinute.ItemStringFormat = "{0:D2}";


            for (int i = 0; i < 60; i += 15)
                cbMinute.Items.Add(i);

            cbMinute.SelectedIndex = 0;

            entryPanel.Children.Add(cbMinute);

            Button btnAdd = new Button();
            btnAdd.Uid = "104";
            btnAdd.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnAdd.Height = Double.NaN;
            btnAdd.Width = Double.NaN;
            btnAdd.Click += new RoutedEventHandler(btnAdd_Click);
            btnAdd.Margin = new Thickness(5, 0, 0, 0);
            btnAdd.Content = Translator.GetInstance().GetString("General", btnAdd.Uid);
            btnAdd.IsEnabled = cbRoute.Items.Count > 0;
            btnAdd.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            btnAdd.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");


            entryPanel.Children.Add(btnAdd);

            txtFlightTime = new TextBlock();
            txtFlightTime.Text = "Flight time: ";

            newEntryPanel.Children.Add(txtFlightTime);
          
            cbRoute.SelectedIndex = 0;



            return newEntryPanel;
        }


        //creates the time indicator header
        private WrapPanel createTimeHeaderPanel()
        {
            int hourFactor = 1;

            WrapPanel panelHeader = new WrapPanel();
            for (int i = 0; i < 24; i++)
            {
                Border brdHour = new Border();
                brdHour.BorderThickness = new Thickness(1, 0, 1, 0);
                brdHour.BorderBrush = Brushes.Black;

                TextBlock txtHour = new TextBlock();
                txtHour.Text = string.Format("{0:00}-{1:00}", i, i + 1);
                txtHour.FontWeight = FontWeights.Bold;
                txtHour.Width = (60 / hourFactor) - 2;
                txtHour.TextAlignment = TextAlignment.Center;

                brdHour.Child = txtHour;
                panelHeader.Children.Add(brdHour);
            }

            return panelHeader;
        }
        //creates the panel for a route for a day
        private Border createRoutePanel(DayOfWeek day)
        {
            int hourFactor = 1;

            Border brdDay = new Border();
            brdDay.BorderThickness = new Thickness(1, 1, 1, 1);
            brdDay.BorderBrush = Brushes.Black;

            Canvas cnvFlights = new Canvas();
            cnvFlights.Width = 24 * 60 / hourFactor;
            cnvFlights.Height = 20;
            brdDay.Child = cnvFlights;

            List<RouteTimeTableEntry> entries = this.Airliner.Routes.SelectMany(r => r.TimeTable.Entries.FindAll(e => e.Airliner == this.Airliner && e.Day == day)).ToList(); //&& e.Time.Hours<12)).ToList();
            entries.AddRange(this.Entries.Keys.SelectMany(r => this.Entries[r].FindAll(e => e.Day == day)));
            entries.RemoveAll(e => this.EntriesToDelete.Keys.SelectMany(r => this.EntriesToDelete[r]).ToList().Find(te => te == e) == e);

            foreach (RouteTimeTableEntry e in entries)
            {
                double maxTime = new TimeSpan(24, 0, 0).Subtract(e.Time).TotalMinutes;

                TimeSpan flightTime = MathHelpers.GetFlightTime(e.TimeTable.Route.Destination1.Profile.Coordinates, e.TimeTable.Route.Destination2.Profile.Coordinates, this.Airliner.Airliner.Type);

                ContentControl ccFlight = new ContentControl();
                ccFlight.ContentTemplate = this.Resources["FlightItem"] as DataTemplate;
                ccFlight.Content = new KeyValuePair<double, RouteTimeTableEntry>(Math.Min(flightTime.TotalMinutes / hourFactor, maxTime / hourFactor), e);
                int hours = e.Time.Hours;

                Canvas.SetLeft(ccFlight, 60 * hours / hourFactor + e.Time.Minutes / hourFactor);

                cnvFlights.Children.Add(ccFlight);
            }

            List<RouteTimeTableEntry> dayBeforeEntries = this.Airliner.Routes.SelectMany(r => r.TimeTable.Entries.FindAll(e => e.Airliner == this.Airliner && (e.Day == day - 1 || ((int)day) == 0 && ((int)e.Day) == 6))).ToList();
            dayBeforeEntries.AddRange(this.Entries.Keys.SelectMany(r => this.Entries[r].FindAll(e => e.Day == day - 1 || ((int)day) == 0 && ((int)e.Day) == 6)));
            dayBeforeEntries.RemoveAll(e => this.EntriesToDelete.Keys.SelectMany(r => this.EntriesToDelete[r]).ToList().Find(te => te == e) == e);

            foreach (RouteTimeTableEntry e in dayBeforeEntries)
            {
                TimeSpan flightTime = MathHelpers.GetFlightTime(e.TimeTable.Route.Destination1.Profile.Coordinates, e.TimeTable.Route.Destination2.Profile.Coordinates, this.Airliner.Airliner.Type);

                TimeSpan endTime = e.Time.Add(flightTime);
                if (endTime.Days == 1)
                {
                    ContentControl ccFlight = new ContentControl();
                    ccFlight.ContentTemplate = this.Resources["FlightItem"] as DataTemplate;
                    ccFlight.Content = new KeyValuePair<double, RouteTimeTableEntry>(endTime.Subtract(new TimeSpan(1, 0, 0, 0)).TotalMinutes / 2, e);

                    Canvas.SetLeft(ccFlight, 0);

                    cnvFlights.Children.Add(ccFlight);
                }
            }

            return brdDay;
        }
        //checks if an entry is valid
        private Boolean isRouteEntryValid(RouteTimeTableEntry entry)
        {
            double dist = MathHelpers.GetDistance(entry.DepartureAirport.Profile.Coordinates, entry.Destination.Airport.Profile.Coordinates);

            if (this.Airliner.Airliner.Type.Range < dist)
                return false;

            TimeSpan flightTime = MathHelpers.GetFlightTime(entry.DepartureAirport.Profile.Coordinates, entry.Destination.Airport.Profile.Coordinates, this.Airliner.Airliner.Type.CruisingSpeed).Add(RouteTimeTable.MinTimeBetweenFlights);

            TimeSpan startTime = new TimeSpan((int)entry.Day, entry.Time.Hours, entry.Time.Minutes, entry.Time.Seconds);

            TimeSpan endTime = startTime.Add(flightTime);
            if (endTime.Days == 7)
                endTime = new TimeSpan(0, endTime.Hours, endTime.Minutes, endTime.Seconds);

            List<RouteTimeTableEntry> airlinerEntries = this.Airliner.Routes.SelectMany(r => r.TimeTable.Entries).ToList();
            airlinerEntries.AddRange(this.Entries.Keys.SelectMany(r => this.Entries[r]));
            airlinerEntries.AddRange(entry.TimeTable.Entries);

            foreach (RouteTimeTableEntry e in airlinerEntries)
            {
                TimeSpan eStartTime = new TimeSpan((int)e.Day, e.Time.Hours, e.Time.Minutes, e.Time.Seconds);

                TimeSpan eFlightTime = MathHelpers.GetFlightTime(e.DepartureAirport.Profile.Coordinates, e.Destination.Airport.Profile.Coordinates, this.Airliner.Airliner.Type.CruisingSpeed).Add(RouteTimeTable.MinTimeBetweenFlights);

                TimeSpan eEndTime = eStartTime.Add(eFlightTime);

                if (eEndTime.Days == 7)
                    eEndTime = new TimeSpan(0, endTime.Hours, endTime.Minutes, endTime.Seconds);

                if ((eStartTime >= startTime && endTime >= eStartTime) || (eEndTime >= startTime && endTime >= eEndTime) || (endTime >= eStartTime && eEndTime >= endTime) || (startTime >= eStartTime && eEndTime >= startTime))
                    return false;
            }
            double minutesPerWeek = 7 * 24*60;

            RouteTimeTableEntry nextEntry = getNextEntry(entry);

            RouteTimeTableEntry previousEntry = getPreviousEntry(entry);

            if (nextEntry != null && previousEntry != null)
            {
             
                TimeSpan flightTimeNext = MathHelpers.GetFlightTime(entry.Destination.Airport.Profile.Coordinates, nextEntry.DepartureAirport.Profile.Coordinates, this.Airliner.Airliner.Type.CruisingSpeed).Add(RouteTimeTable.MinTimeBetweenFlights);
                TimeSpan flightTimePrevious = MathHelpers.GetFlightTime(entry.DepartureAirport.Profile.Coordinates, previousEntry.Destination.Airport.Profile.Coordinates, this.Airliner.Airliner.Type.CruisingSpeed).Add(RouteTimeTable.MinTimeBetweenFlights);
        

                TimeSpan prevDate = new TimeSpan((int)previousEntry.Day,previousEntry.Time.Hours,previousEntry.Time.Minutes,previousEntry.Time.Seconds);
                TimeSpan nextDate = new TimeSpan((int)nextEntry.Day,nextEntry.Time.Hours,nextEntry.Time.Minutes,nextEntry.Time.Seconds);
                TimeSpan currentDate = new TimeSpan((int)entry.Day, entry.Time.Hours, entry.Time.Minutes, entry.Time.Seconds);

                        
                double timeToNext = currentDate.Subtract(nextDate).TotalMinutes > 0 ? minutesPerWeek - currentDate.Subtract(nextDate).TotalMinutes : Math.Abs(currentDate.Subtract(nextDate).TotalMinutes);
                double timeFromPrev = prevDate.Subtract(currentDate).TotalMinutes > 0 ? minutesPerWeek - prevDate.Subtract(currentDate).TotalMinutes : Math.Abs(prevDate.Subtract(currentDate).TotalMinutes);
    
                if (timeFromPrev> previousEntry.getFlightTime().TotalMinutes + flightTimePrevious.TotalMinutes && timeToNext>entry.getFlightTime().TotalMinutes + flightTimeNext.TotalMinutes) 
                    return true;
                else
                    return false;
            }
            else
                return true;

        }
        //returns the previous entry before a specific entry
        private RouteTimeTableEntry getPreviousEntry(RouteTimeTableEntry entry)
        {
            TimeSpan tsEntry = new TimeSpan((int)entry.Day, entry.Time.Hours, entry.Time.Minutes, entry.Time.Seconds);
            DayOfWeek eDay = entry.Day;

            int counter = 0;

            while (counter < 7)
            {

                List<RouteTimeTableEntry> entries = this.Airliner.Routes.SelectMany(r => r.TimeTable.Entries.FindAll(e => e.Airliner == this.Airliner && e.Day == eDay)).ToList();
                entries.AddRange(this.Entries.Keys.SelectMany(r => this.Entries[r].FindAll(e => e.Day == eDay)));
                entries.RemoveAll(e => this.EntriesToDelete.Keys.SelectMany(r => this.EntriesToDelete[r]).ToList().Find(te => te == e) == e);

                entries = (from e in entries orderby e.Time descending select e).ToList();

                foreach (RouteTimeTableEntry dEntry in entries)
                {
                    TimeSpan ts = new TimeSpan((int)eDay, dEntry.Time.Hours, dEntry.Time.Minutes, dEntry.Time.Seconds);
                    if (ts < tsEntry)
                        return dEntry;

                }
                counter++;

                eDay--;

                if (((int)eDay) == -1)
                {
                    eDay = (DayOfWeek)6;
                    tsEntry = new TimeSpan(7, tsEntry.Hours, tsEntry.Minutes, tsEntry.Seconds);
                }

            }

            return null;

        }
        //returns the next entry after a specific entry
        private RouteTimeTableEntry getNextEntry(RouteTimeTableEntry entry)
        {

            DayOfWeek day = entry.Day;

            int counter = 0;

            while (counter < 7)
            {

                List<RouteTimeTableEntry> entries = this.Airliner.Routes.SelectMany(r => r.TimeTable.Entries.FindAll(e => e.Airliner == this.Airliner && e.Day == day)).ToList();
                entries.AddRange(this.Entries.Keys.SelectMany(r => this.Entries[r].FindAll(e => e.Day == day)));
                entries.RemoveAll(e=>this.EntriesToDelete.Keys.SelectMany(r=>this.EntriesToDelete[r]).ToList().Find(te=>te == e)==e);

                foreach (RouteTimeTableEntry dEntry in entries)
                {
                    if (!((dEntry.Day == entry.Day && dEntry.Time <= entry.Time)))
                        return dEntry;
                }
                day++;

                if (day == (DayOfWeek)7)
                    day = (DayOfWeek)0;

                counter++;

            }

            return null;

        }

        //shows the flights for the airliner
        private void showFlights()
        {
            lbFlights.Items.Clear();

            lbFlights.Items.Add(new QuickInfoValue("Day", createTimeHeaderPanel()));

            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                lbFlights.Items.Add(new QuickInfoValue(day.ToString(), createRoutePanel(day)));

            }
        }
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxItem item = (ComboBoxItem)cbRoute.SelectedItem;

            Route route = ((KeyValuePair<Route, Airport>)item.Tag).Key;

            Airport airport = ((KeyValuePair<Route, Airport>)item.Tag).Value;

            TimeSpan time = new TimeSpan((int)cbHour.SelectedItem, (int)cbMinute.SelectedItem, 0);

            string day = cbDay.SelectedItem.ToString();

            if (!this.Entries.ContainsKey(route))
                this.Entries.Add(route, new List<RouteTimeTableEntry>());

            if (day == "Daily")
            {

                foreach (DayOfWeek dayOfWeek in Enum.GetValues(typeof(DayOfWeek)))
                {
                    RouteTimeTableEntry entry = new RouteTimeTableEntry(route.TimeTable, dayOfWeek, time, new RouteEntryDestination(airport, "FLAA"));
                    entry.Airliner = this.Airliner;
                    if (isRouteEntryValid(entry))
                        this.Entries[route].Add(entry);
                }

            }
            else
            {
                RouteTimeTableEntry entry = new RouteTimeTableEntry(route.TimeTable, (DayOfWeek)cbDay.SelectedItem, time, new RouteEntryDestination(airport, "FLAA"));
                entry.Airliner = this.Airliner;
                if (isRouteEntryValid(entry))
                    this.Entries[route].Add(entry);

            }



            showFlights();
        }
        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            this.Entries.Clear();

            this.EntriesToDelete.Clear();

            showFlights();
        }
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            foreach (Route route in this.Entries.Keys)
            {
                foreach (RouteTimeTableEntry entry in this.Entries[route])
                    route.TimeTable.addEntry(entry);

                if (!this.Airliner.Routes.Contains(route))
                    this.Airliner.addRoute(route);
            }
            foreach (Route route in this.EntriesToDelete.Keys)
            {
                foreach (RouteTimeTableEntry entry in this.Entries[route])
                    route.TimeTable.removeEntry(entry);

                if (route.TimeTable.getEntries(this.Airliner).Count == 0)
                    this.Airliner.removeRoute(route);
           
            }

            this.Close();
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void cbRoute_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = (ComboBoxItem)cbRoute.SelectedItem;

            Route route = ((KeyValuePair<Route, Airport>)item.Tag).Key;

            TimeSpan flightTime = MathHelpers.GetFlightTime(route.Destination1.Profile.Coordinates, route.Destination2.Profile.Coordinates, this.Airliner.Airliner.Type);

            txtFlightTime.Text = string.Format("Flight time: {0:hh\\:mm}", flightTime);
        }

        private void txtFlightEntry_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                RouteTimeTableEntry entry = (RouteTimeTableEntry)((TextBlock)sender).Tag;

                if (this.Entries.ContainsKey(entry.TimeTable.Route) && this.Entries[entry.TimeTable.Route].Find(te => te == entry) != null)
                {
                    this.Entries[entry.TimeTable.Route].Remove(entry);
                }
                else
                {
                    if (!this.EntriesToDelete.ContainsKey(entry.TimeTable.Route))
                        this.EntriesToDelete.Add(entry.TimeTable.Route, new List<RouteTimeTableEntry>());

                    this.EntriesToDelete[entry.TimeTable.Route].Add(entry);
                }

                showFlights();
            }
        }

    }

    //the converter for getting the end time for a route entry
    public class RouteEntryEndTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            RouteTimeTableEntry e = (RouteTimeTableEntry)value;
            TimeSpan flightTime = MathHelpers.GetFlightTime(e.TimeTable.Route.Destination1.Profile.Coordinates, e.TimeTable.Route.Destination2.Profile.Coordinates, e.Airliner.Airliner.Type);

            return e.Time.Add(flightTime);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

}
