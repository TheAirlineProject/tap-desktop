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
using TheAirline.GraphicsModel.Converters;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpAirlinerAutoRoutes.xaml
    /// </summary>
    public partial class PopUpAirlinerAutoRoutes : PopUpWindow
    {
        private FleetAirliner Airliner;
        private ListBox lbFlights;
        
        private Dictionary<Route, List<RouteTimeTableEntry>> Entries;
        private Dictionary<Route, List<RouteTimeTableEntry>> EntriesToDelete;

        private ComboBox cbRoute, cbFlightsPerDay, cbFlightCode, cbRegion;
        private CheckBox cbBusinessRoute;
        private double maxBusinessRouteTime = new TimeSpan(2, 0, 0).TotalMinutes;

        public static object ShowPopUp(FleetAirliner airliner)
        {
            PopUpWindow window = new PopUpAirlinerAutoRoutes(airliner);
            window.ShowDialog();

            return window.Selected;
        }
        public PopUpAirlinerAutoRoutes(FleetAirliner airliner)
        {
            this.Entries = new Dictionary<Route, List<RouteTimeTableEntry>>();
            this.EntriesToDelete = new Dictionary<Route, List<RouteTimeTableEntry>>();

            this.Airliner = airliner;
       
            InitializeComponent();

            this.Title = this.Airliner.Name;

            this.Width = 1200;

            this.Height = 325;
          
            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            ScrollViewer scroller = new ScrollViewer();
            //scroller.Margin = new Thickness(10, 10, 10, 10);
            scroller.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            scroller.MaxHeight = this.Height;

            StackPanel mainPanel = new StackPanel();
            mainPanel.Margin = new Thickness(10, 10, 10, 10);
            scroller.Content = mainPanel;

            Grid grdFlights = UICreator.CreateGrid(2);
            grdFlights.ColumnDefinitions[1].Width = new GridLength(200);
            mainPanel.Children.Add(grdFlights);

            lbFlights = new ListBox();
            lbFlights.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbFlights.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");
            lbFlights.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            Grid.SetColumn(lbFlights, 0);
            grdFlights.Children.Add(lbFlights);

            ScrollViewer panelRoutes = createRoutesPanel();

            Grid.SetColumn(panelRoutes, 1);
            grdFlights.Children.Add(panelRoutes);

            mainPanel.Children.Add(createAutoGeneratePanel());
            mainPanel.Children.Add(createButtonsPanel());

            this.Content = scroller;

            showFlights();
        }
        //creates the panel for the routes
        private ScrollViewer createRoutesPanel()
        {
            ScrollViewer scroller = new ScrollViewer();
            scroller.MaxHeight = 200;
            scroller.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;

            StackPanel panelRoutes = new StackPanel();
            panelRoutes.Margin = new Thickness(5, 0, 0, 0);

            long requiredRunway = this.Airliner.Airliner.Type.MinRunwaylength;

            var routes = this.Airliner.Airliner.Airline.Routes.FindAll(r => this.Airliner.Airliner.Type.Range > r.getDistance() && !r.Banned && r.Destination1.getMaxRunwayLength() >= requiredRunway && r.Destination2.getMaxRunwayLength() >= requiredRunway);
            foreach (Route route in routes)
            {
                Border brdRoute = new Border();
                brdRoute.BorderBrush = Brushes.Black;
                brdRoute.BorderThickness = new Thickness(1);
                brdRoute.Margin = new Thickness(0, 0, 0, 2);
                brdRoute.Width = 150;
                brdRoute.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

                ContentControl ccRoute = new ContentControl();
                ccRoute.ContentTemplate = this.Resources["RouteItem"] as DataTemplate;
                ccRoute.Content = route;

                brdRoute.Child = ccRoute;

                panelRoutes.Children.Add(brdRoute);
            }

            scroller.Content = panelRoutes;

            return scroller;

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

                TimeSpan flightTime = e.TimeTable.Route.getFlightTime(this.Airliner.Airliner.Type);

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
                TimeSpan flightTime = e.TimeTable.Route.getFlightTime(this.Airliner.Airliner.Type);

                TimeSpan endTime = e.Time.Add(flightTime);
                if (endTime.Days == 1)
                {
                    ContentControl ccFlight = new ContentControl();
                    ccFlight.ContentTemplate = this.Resources["FlightItem"] as DataTemplate;
                    ccFlight.Content = new KeyValuePair<double, RouteTimeTableEntry>(endTime.Subtract(new TimeSpan(1, 0, 0, 0)).TotalMinutes / hourFactor, e);

                    Canvas.SetLeft(ccFlight, 0);

                    cnvFlights.Children.Add(ccFlight);
                }
            }

            return brdDay;
        }
        //creates the time indicator header
        private WrapPanel createTimeHeaderPanel()
        {
            string format = System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.PMDesignator == "" ? "HH" : "hh";
            int hourFactor = 1;

            WrapPanel panelHeader = new WrapPanel();
            for (int i = 0; i < 24; i++)
            {
                Border brdHour = new Border();
                brdHour.BorderThickness = new Thickness(1, 0, 1, 0);
                brdHour.BorderBrush = Brushes.Black;

                TextBlock txtHour = new TextBlock();
                txtHour.Text = string.Format("{0}-{1}", new DateTime(2000, 1, 1, i, 0, 0).ToString(format), new DateTime(2000, 1, 1, i + 1 == 24 ? 0 : i + 1, 0, 0).ToString(format));
                txtHour.FontWeight = FontWeights.Bold;
                txtHour.Width = (60 / hourFactor) - 2;
                txtHour.TextAlignment = TextAlignment.Center;

                brdHour.Child = txtHour;
                panelHeader.Children.Add(brdHour);
            }

            return panelHeader;
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

            buttonsPanel.Children.Add(btnAdd);


            return buttonsPanel;
        }
        //creates the panel for auto generation of time table from a route
        private WrapPanel createAutoGeneratePanel()
        {
            WrapPanel autogeneratePanel = new WrapPanel();
            autogeneratePanel.Margin = new Thickness(0, 5, 0, 0);

            cbRegion = new ComboBox();
            cbRegion.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbRegion.Width = 150;
            cbRegion.DisplayMemberPath = "Name";
            cbRegion.SelectedValuePath = "Name";
            cbRegion.SelectionChanged += cbRegion_SelectionChanged;
            cbRegion.Items.Add(Regions.GetRegion("100"));

            List<Region> regions = GameObject.GetInstance().HumanAirline.Routes.Where(r => r.Destination1.Profile.Country.Region == r.Destination2.Profile.Country.Region).Select(r => r.Destination1.Profile.Country.Region).ToList();
            regions.AddRange(GameObject.GetInstance().HumanAirline.Routes.Where(r => r.Destination1.Profile.Country == GameObject.GetInstance().HumanAirline.Profile.Country).Select(r => r.Destination2.Profile.Country.Region));
            regions.AddRange(GameObject.GetInstance().HumanAirline.Routes.Where(r => r.Destination2.Profile.Country == GameObject.GetInstance().HumanAirline.Profile.Country).Select(r => r.Destination1.Profile.Country.Region));


            foreach (Region region in regions.Distinct())
                cbRegion.Items.Add(region);

            autogeneratePanel.Children.Add(cbRegion);

            cbRoute = new ComboBox();
            cbRoute.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbRoute.SelectionChanged += new SelectionChangedEventHandler(cbAutoRoute_SelectionChanged);
            cbRoute.ItemTemplate = this.Resources["SelectRouteItem"] as DataTemplate;

            foreach (Route route in this.Airliner.Airliner.Airline.Routes.FindAll(r => this.Airliner.Airliner.Type.Range > r.getDistance() && !r.Banned))
            {
                cbRoute.Items.Add(route);
            }

            autogeneratePanel.Children.Add(cbRoute);

            cbFlightCode = new ComboBox();
            cbFlightCode.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");

            foreach (string flightCode in this.Airliner.Airliner.Airline.getFlightCodes())
                cbFlightCode.Items.Add(flightCode);

            cbFlightCode.Items.RemoveAt(cbFlightCode.Items.Count - 1);

            cbFlightCode.SelectedIndex = 0;

            autogeneratePanel.Children.Add(cbFlightCode);

            TextBlock txtFlightsPerDay = UICreator.CreateTextBlock("Flights per day");
            txtFlightsPerDay.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            txtFlightsPerDay.Margin = new Thickness(10, 0, 5, 0);

            autogeneratePanel.Children.Add(txtFlightsPerDay);

            cbFlightsPerDay = new ComboBox();
            cbFlightsPerDay.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");

            //cbRoute.SelectedIndex = 0;

            autogeneratePanel.Children.Add(cbFlightsPerDay);

            cbBusinessRoute = new CheckBox();
            cbBusinessRoute.FlowDirection = System.Windows.FlowDirection.RightToLeft;
            cbBusinessRoute.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            cbBusinessRoute.Unchecked += cbBusinessRoute_Unchecked;
            cbBusinessRoute.Checked += cbBusinessRoute_Checked;
            cbBusinessRoute.Content = "Business route";
            cbBusinessRoute.Margin = new Thickness(5, 0, 0, 0);

            autogeneratePanel.Children.Add(cbBusinessRoute);

            cbRegion.SelectedIndex = 0;

            return autogeneratePanel;


        }


        private void cbRegion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            long requiredRunway = this.Airliner.Airliner.Type.MinRunwaylength;

            Region region = (Region)cbRegion.SelectedItem;

            cbRoute.Items.Clear();

            if (region.Uid == "100")
            {
                foreach (Route route in this.Airliner.Airliner.Airline.Routes.FindAll(r => this.Airliner.Airliner.Type.Range > r.getDistance() && !r.Banned && r.Destination1.getMaxRunwayLength() >= requiredRunway && r.Destination2.getMaxRunwayLength() >= requiredRunway).OrderBy(r => new AirportCodeConverter().Convert(r.Destination1)).ThenBy(r => new AirportCodeConverter().Convert(r.Destination2)))
                {
                    cbRoute.Items.Add(route);
                }
            }
            else
            {
                var routes = this.Airliner.Airliner.Airline.Routes.FindAll(r => this.Airliner.Airliner.Type.Range > r.getDistance() && !r.Banned && r.Destination1.getMaxRunwayLength() >= requiredRunway && r.Destination2.getMaxRunwayLength() >= requiredRunway && ((r.Destination1.Profile.Country.Region == region && r.Destination2.Profile.Country.Region == GameObject.GetInstance().HumanAirline.Profile.Country.Region) || (r.Destination2.Profile.Country.Region == region && r.Destination1.Profile.Country.Region == GameObject.GetInstance().HumanAirline.Profile.Country.Region) || (r.Destination1.Profile.Country.Region == region && r.Destination2.Profile.Country.Region == region))).OrderBy(r => new AirportCodeConverter().Convert(r.Destination1)).ThenBy(r => new AirportCodeConverter().Convert(r.Destination2));

                foreach (Route route in routes)
                    cbRoute.Items.Add(route);
            }
            cbRoute.SelectedIndex = 0;

        }
        private void cbAutoRoute_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            Route route = (Route)cbRoute.SelectedItem;

            if (route != null)
            {
                TimeSpan routeFlightTime = route.getFlightTime(this.Airliner.Airliner.Type);

                TimeSpan minFlightTime = routeFlightTime.Add(RouteTimeTable.MinTimeBetweenFlights);

                if (minFlightTime.TotalHours > 5)
                    minFlightTime = minFlightTime.Add(RouteTimeTable.MinTimeBetweenFlights);

                int maxHours = 22 - 6; //from 06.00 to 22.00

                int flightsPerDay = Convert.ToInt16(maxHours * 60 / (2 * minFlightTime.TotalMinutes));

                cbFlightsPerDay.Items.Clear();

                for (int i = 0; i < Math.Max(1, flightsPerDay); i++)
                    cbFlightsPerDay.Items.Add(i + 1);

                cbFlightsPerDay.SelectedIndex = 0;


                cbBusinessRoute.Visibility = minFlightTime.TotalMinutes <= maxBusinessRouteTime ? Visibility.Visible : System.Windows.Visibility.Collapsed;

                if (minFlightTime.TotalMinutes > maxBusinessRouteTime)
                    cbBusinessRoute.IsChecked = false;

            }


        }
        //clears the time table
        private void clearTimeTable()
        {
            this.Entries.Clear();

            foreach (Route r in this.Airliner.Routes)
            {
                foreach (RouteTimeTableEntry entry in r.TimeTable.Entries)
                {
                    if (!this.EntriesToDelete.ContainsKey(r))
                    {
                        this.EntriesToDelete.Add(r, new List<RouteTimeTableEntry>());
                        this.EntriesToDelete[r].Add(entry);
                    }
                    else
                    {
                        if (!this.EntriesToDelete[r].Contains(entry))
                            this.EntriesToDelete[r].Add(entry);
                    }
                }

            }
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
                foreach (RouteTimeTableEntry entry in this.EntriesToDelete[route])
                    route.TimeTable.removeEntry(entry);

                if (route.TimeTable.getEntries(this.Airliner).Count == 0)
                    this.Airliner.removeRoute(route);

            }

            this.Close();
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = null;
            this.Close();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {

            Route route = (Route)cbRoute.SelectedItem;

            RouteTimeTable rt;
            
            clearTimeTable();

            if (!this.Entries.ContainsKey(route))
                this.Entries.Add(route, new List<RouteTimeTableEntry>());

        
           int flightsPerDay = (int)cbFlightsPerDay.SelectedItem;

            string flightcode1 = cbFlightCode.SelectedItem.ToString();
            string flightcode2 = this.Airliner.Airliner.Airline.getFlightCodes()[this.Airliner.Airliner.Airline.getFlightCodes().IndexOf(flightcode1) + 1];
            
            if (flightsPerDay > 0)
            {
                if (cbBusinessRoute.IsChecked.Value)
                {
                    flightsPerDay = (int)(route.getFlightTime(this.Airliner.Airliner.Type).Add(RouteTimeTable.MinTimeBetweenFlights).TotalMinutes / 2 / maxBusinessRouteTime);
                    rt = AIHelpers.CreateBusinessRouteTimeTable(route, this.Airliner, Math.Max(1, flightsPerDay), flightcode1, flightcode2);
                }
                else
                    rt = AIHelpers.CreateAirlinerRouteTimeTable(route, this.Airliner, flightsPerDay, flightcode1, flightcode2);
            }
            else
                rt = null;

            foreach (RouteTimeTableEntry entry in rt.Entries)
                this.Entries[route].Add(entry);
            
            showFlights();
        }
        private void cbBusinessRoute_Checked(object sender, RoutedEventArgs e)
        {
            cbFlightsPerDay.IsEnabled = false;
        }

        private void cbBusinessRoute_Unchecked(object sender, RoutedEventArgs e)
        {
            cbFlightsPerDay.IsEnabled = true;
        }
        private void txtFlightEntry_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                /*
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

                showFlights();*/
            }
        }
    }
}
