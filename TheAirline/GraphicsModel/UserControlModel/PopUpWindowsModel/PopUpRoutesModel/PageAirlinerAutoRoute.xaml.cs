using System;
using System.Collections.Generic;
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
using TheAirline.GraphicsModel.Converters;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel.PopUpRoutesModel
{
    /// <summary>
    /// Interaction logic for PageAirlinerAutoRoute.xaml
    /// </summary>
    public partial class PageAirlinerAutoRoute : Page
    {
        private FleetAirliner Airliner;
        private PopUpAirlinerAutoRoutes ParentPage;

        private enum FlightInterval { Daily, Weekly }
        private FlightInterval Interval;

        private enum RouteOperationsType {[Description("Standard Operations")] Standard,
            [Description("Business Operations")] Business, 
            [Description("24-Hour Operations")] WholeDay
        }
        private RouteOperationsType RouteOperations;

        private ComboBox cbRoute, cbFlightsPerDay, cbFlightsPerWeek, cbFlightCode, cbRegion, cbDelayMinutes, cbStartTime;
 
        private double maxBusinessRouteTime = new TimeSpan(2, 0, 0).TotalMinutes;
      
        public delegate void OnRouteChanged(Route route);
        public event OnRouteChanged RouteChanged;
      
        public PageAirlinerAutoRoute(FleetAirliner airliner, PopUpAirlinerAutoRoutes parent, OnRouteChanged routeChanged)
        {
            this.ParentPage = parent;

            this.Airliner = airliner;

            this.RouteChanged += routeChanged;

            InitializeComponent();

            StackPanel mainPanel = new StackPanel();

            mainPanel.Children.Add(createAutoGeneratePanel());
       
            this.Content = mainPanel;
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
            cbRegion.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

            var routes = this.Airliner.Airliner.Airline.Routes.FindAll(r => this.Airliner.Airliner.Type.Range > r.getDistance() && !r.Banned);

            List<Region> regions = routes.Where(r => r.Destination1.Profile.Country.Region == r.Destination2.Profile.Country.Region).Select(r => r.Destination1.Profile.Country.Region).ToList();
            regions.AddRange(routes.Where(r => r.Destination1.Profile.Country == GameObject.GetInstance().HumanAirline.Profile.Country).Select(r => r.Destination2.Profile.Country.Region));
            regions.AddRange(routes.Where(r => r.Destination2.Profile.Country == GameObject.GetInstance().HumanAirline.Profile.Country).Select(r => r.Destination1.Profile.Country.Region));
            

            foreach (Region region in regions.Distinct())
                cbRegion.Items.Add(region);

            autogeneratePanel.Children.Add(cbRegion);

            cbRoute = new ComboBox();
            cbRoute.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbRoute.SelectionChanged += new SelectionChangedEventHandler(cbAutoRoute_SelectionChanged);
            cbRoute.ItemTemplate = this.Resources["SelectRouteItem"] as DataTemplate;
            cbRoute.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

            foreach (Route route in routes)
            {
                cbRoute.Items.Add(route);
            }

            autogeneratePanel.Children.Add(cbRoute);

            cbFlightCode = new ComboBox();
            cbFlightCode.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbFlightCode.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

            foreach (string flightCode in this.Airliner.Airliner.Airline.getFlightCodes())
                cbFlightCode.Items.Add(flightCode);

            cbFlightCode.Items.RemoveAt(cbFlightCode.Items.Count - 1);

            cbFlightCode.SelectedIndex = 0;

            autogeneratePanel.Children.Add(cbFlightCode);

            StackPanel panelFlightInterval = new StackPanel();
            panelFlightInterval.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            panelFlightInterval.Margin = new Thickness(10, 0, 0, 0);

            RadioButton rbDailyFlights = new RadioButton();
            rbDailyFlights.Content = Translator.GetInstance().GetString("PopUpAirlinerAutoRoutes", "1002");
            rbDailyFlights.GroupName = "Interval";
            rbDailyFlights.IsChecked = true;
            rbDailyFlights.Checked += rbDailyFlights_Checked;

            panelFlightInterval.Children.Add(rbDailyFlights);

            RadioButton rbWeeklyFlights = new RadioButton();
            rbWeeklyFlights.Content = Translator.GetInstance().GetString("PopUpAirlinerAutoRoutes", "1005");
            rbWeeklyFlights.GroupName = "Interval";
            rbWeeklyFlights.Checked += rbWeeklyFlights_Checked;

            panelFlightInterval.Children.Add(rbWeeklyFlights);

            autogeneratePanel.Children.Add(panelFlightInterval);

            cbFlightsPerDay = new ComboBox();
            cbFlightsPerDay.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbFlightsPerDay.SelectionChanged += cbFlightsPerDay_SelectionChanged;
            cbFlightsPerDay.Margin = new Thickness(5, 0, 0, 0);
            cbFlightsPerDay.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

            autogeneratePanel.Children.Add(cbFlightsPerDay);

            this.Interval = FlightInterval.Daily;

            cbFlightsPerWeek = new ComboBox();
            cbFlightsPerWeek.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbFlightsPerWeek.Visibility = System.Windows.Visibility.Collapsed;
            cbFlightsPerWeek.Margin = new Thickness(5, 0, 0, 0);
            cbFlightsPerWeek.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

            for (int i = 1; i < 7; i++)
                cbFlightsPerWeek.Items.Add(i);

            cbFlightsPerWeek.SelectedIndex = 0;

            autogeneratePanel.Children.Add(cbFlightsPerWeek);

            TextBlock txtDelayMinutes = UICreator.CreateTextBlock(Translator.GetInstance().GetString("PopUpAirlinerAutoRoutes", "1003"));
            txtDelayMinutes.Margin = new Thickness(10, 0, 0, 0);
            txtDelayMinutes.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

            autogeneratePanel.Children.Add(txtDelayMinutes);

            cbDelayMinutes = new ComboBox();
            cbDelayMinutes.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbDelayMinutes.SelectionChanged += cbDelayMinutes_SelectionChanged;
            cbDelayMinutes.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right;
            cbDelayMinutes.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;


            autogeneratePanel.Children.Add(cbDelayMinutes);

            TextBlock txtStartTime = UICreator.CreateTextBlock(Translator.GetInstance().GetString("PopUpAirlinerAutoRoutes", "1004"));
            txtStartTime.Margin = new Thickness(10, 0, 5, 0);
            txtStartTime.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

            autogeneratePanel.Children.Add(txtStartTime);

            cbStartTime = new ComboBox();
            cbStartTime.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbStartTime.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            cbStartTime.SetResourceReference(ComboBox.ItemTemplateProperty, "TimeSpanItem");

            autogeneratePanel.Children.Add(cbStartTime);

            StackPanel panelRouteType = new StackPanel();
            panelRouteType.Margin = new Thickness(5, 0, 0, 0);

            //string[] routeTypes = new string[] { "Standard Operations", "Business Operations", "24-Hour Operations" };

            foreach (RouteOperationsType type in Enum.GetValues(typeof(RouteOperationsType)))
            {
                RadioButton rbRouteType = new RadioButton();
                rbRouteType.Tag = type;
                rbRouteType.GroupName = "RouteType";
                rbRouteType.Content = type.GetAttributeOfType<DescriptionAttribute>().Description;
                rbRouteType.Checked += rbRouteType_Checked;

                if (type == RouteOperationsType.Standard)
                    rbRouteType.IsChecked = true;

                panelRouteType.Children.Add(rbRouteType);
            }

            this.RouteOperations = RouteOperationsType.Standard;

            autogeneratePanel.Children.Add(panelRouteType);

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

            autogeneratePanel.Children.Add(btnAdd);

            cbRegion.SelectedIndex = 0;

            return autogeneratePanel;


        }

        private void rbRouteType_Checked(object sender, RoutedEventArgs e)
        {
            this.RouteOperations = (RouteOperationsType)((RadioButton)sender).Tag;

            cbFlightsPerDay.IsEnabled = this.RouteOperations != RouteOperationsType.Business;
            cbFlightsPerWeek.IsEnabled = this.RouteOperations != RouteOperationsType.Business;
            cbStartTime.IsEnabled = this.RouteOperations != RouteOperationsType.Business;
            cbDelayMinutes.IsEnabled = this.RouteOperations != RouteOperationsType.Business;

            setValues();
        }
        
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {

            Route route = (Route)cbRoute.SelectedItem;

            RouteTimeTable rt;

            int flightsPerDay = (int)cbFlightsPerDay.SelectedItem;
            int flightsPerWeek = (int)cbFlightsPerWeek.SelectedItem;
            int delayMinutes = (int)cbDelayMinutes.SelectedItem;
            TimeSpan startTime = (TimeSpan)cbStartTime.SelectedItem;

            string flightcode1 = cbFlightCode.SelectedItem.ToString();
            string flightcode2 = this.Airliner.Airliner.Airline.getFlightCodes()[this.Airliner.Airliner.Airline.getFlightCodes().IndexOf(flightcode1) + 1];

            if (flightsPerDay > 0)
            {
                if (this.RouteOperations == RouteOperationsType.Business)
                {
                    flightsPerDay = (int)(route.getFlightTime(this.Airliner.Airliner.Type).Add(FleetAirlinerHelpers.GetMinTimeBetweenFlights(this.Airliner)).TotalMinutes / 2 / maxBusinessRouteTime);
                    rt = AIHelpers.CreateBusinessRouteTimeTable(route, this.Airliner, Math.Max(1, flightsPerDay), flightcode1, flightcode2);
                }
                else 
                {
                    if (this.Interval == FlightInterval.Daily)
                        rt = AIHelpers.CreateAirlinerRouteTimeTable(route, this.Airliner, flightsPerDay, true, delayMinutes, startTime, flightcode1, flightcode2);
                    else
                        rt = AIHelpers.CreateAirlinerRouteTimeTable(route, this.Airliner, flightsPerWeek, false, delayMinutes, startTime, flightcode1, flightcode2);
                }
            
            }
            else
                rt = null;

            if (!TimeTableHelpers.IsTimeTableValid(rt, this.Airliner, this.ParentPage.Entries,false))
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2705"), Translator.GetInstance().GetString("MessageBox", "2705", "message"), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    this.ParentPage.NewestEntries.Clear();
                    this.ParentPage.clearTimeTable();

                    if (!this.ParentPage.Entries.ContainsKey(route))
                        this.ParentPage.Entries.Add(route, new List<RouteTimeTableEntry>());


                    foreach (RouteTimeTableEntry entry in rt.Entries)
                        if (!TimeTableHelpers.IsRouteEntryInOccupied(entry,this.Airliner))
                        {
                            this.ParentPage.Entries[route].Add(entry);
                               this.ParentPage.NewestEntries.Add(entry);
                        }
                }
            }
            else
            {
                if (!this.ParentPage.Entries.ContainsKey(route))
                    this.ParentPage.Entries.Add(route, new List<RouteTimeTableEntry>());

                foreach (RouteTimeTableEntry entry in rt.Entries)
                    if (!TimeTableHelpers.IsRouteEntryInOccupied(entry,this.Airliner))
                    {
                        this.ParentPage.Entries[route].Add(entry);
                           this.ParentPage.NewestEntries.Add(entry);
                    }
            }

            
            this.ParentPage.showFlights();
        }
        private void rbWeeklyFlights_Checked(object sender, RoutedEventArgs e)
        {
            cbFlightsPerWeek.Visibility = System.Windows.Visibility.Visible;
            cbFlightsPerDay.Visibility = System.Windows.Visibility.Collapsed;

            cbFlightsPerDay.SelectedIndex = 0;

            this.Interval = FlightInterval.Weekly;
        }

        private void rbDailyFlights_Checked(object sender, RoutedEventArgs e)
        {
            cbFlightsPerDay.Visibility = System.Windows.Visibility.Visible;
            cbFlightsPerWeek.Visibility = System.Windows.Visibility.Collapsed;

            this.Interval = FlightInterval.Daily;
        }

       
       
        private void cbRegion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Route.RouteType type = this.Airliner.Airliner.Type.TypeAirliner == AirlinerType.TypeOfAirliner.Cargo ? Route.RouteType.Cargo : Route.RouteType.Passenger;

            long requiredRunway = this.Airliner.Airliner.Type.MinRunwaylength;

            Region region = (Region)cbRegion.SelectedItem;

            cbRoute.Items.Clear();

            if (region.Uid == "100")
            {
                foreach (Route route in this.Airliner.Airliner.Airline.Routes.FindAll(r => r.Type == type && this.Airliner.Airliner.Type.Range > r.getDistance() && !r.Banned && r.Destination1.getMaxRunwayLength() >= requiredRunway && r.Destination2.getMaxRunwayLength() >= requiredRunway).OrderBy(r => new AirportCodeConverter().Convert(r.Destination1)).ThenBy(r => new AirportCodeConverter().Convert(r.Destination2)))
                {
                    cbRoute.Items.Add(route);
                }
            }
            else
            {
                var routes = this.Airliner.Airliner.Airline.Routes.FindAll(r => r.Type == type && this.Airliner.Airliner.Type.Range > r.getDistance() && !r.Banned && r.Destination1.getMaxRunwayLength() >= requiredRunway && r.Destination2.getMaxRunwayLength() >= requiredRunway && ((r.Destination1.Profile.Country.Region == region && r.Destination2.Profile.Country.Region == GameObject.GetInstance().HumanAirline.Profile.Country.Region) || (r.Destination2.Profile.Country.Region == region && r.Destination1.Profile.Country.Region == GameObject.GetInstance().HumanAirline.Profile.Country.Region) || (r.Destination1.Profile.Country.Region == region && r.Destination2.Profile.Country.Region == region))).OrderBy(r => new AirportCodeConverter().Convert(r.Destination1)).ThenBy(r => new AirportCodeConverter().Convert(r.Destination2));

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

                TimeSpan minFlightTime = routeFlightTime.Add(FleetAirlinerHelpers.GetMinTimeBetweenFlights(this.Airliner));

                cbDelayMinutes.Items.Clear();

                int minDelayMinutes = (int)FleetAirlinerHelpers.GetMinTimeBetweenFlights(this.Airliner).TotalMinutes;

                for (int i = minDelayMinutes; i < minDelayMinutes + 120; i += 15)
                    cbDelayMinutes.Items.Add(i);

                cbDelayMinutes.SelectedIndex = 0;

                //cbBusinessRoute.Visibility = minFlightTime.TotalMinutes <= maxBusinessRouteTime ? Visibility.Visible : System.Windows.Visibility.Collapsed;

                //if (minFlightTime.TotalMinutes > maxBusinessRouteTime)
                  //  cbBusinessRoute.IsChecked = false;

                if (this.RouteChanged != null)
                    this.RouteChanged(route);
                
            }


        }
       //sets the values
        private void setValues()
        {
            Route route = (Route)cbRoute.SelectedItem;

            if (route != null && cbDelayMinutes.SelectedItem != null)
            {
                TimeSpan routeFlightTime = route.getFlightTime(this.Airliner.Airliner.Type);

                int delayMinutes = (int)cbDelayMinutes.SelectedItem;

                TimeSpan minFlightTime = routeFlightTime.Add(new TimeSpan(0, delayMinutes, 0));

                int maxHours = 22 - 06; //from 06.00 to 22.00

                if (this.RouteOperations == RouteOperationsType.WholeDay)
                    maxHours = 22;

                int flightsPerDay = Convert.ToInt16(maxHours * 60 / (2 * minFlightTime.TotalMinutes));

                cbFlightsPerDay.Items.Clear();

                for (int i = 0; i < Math.Max(1, flightsPerDay); i++)
                    cbFlightsPerDay.Items.Add(i + 1);

                cbFlightsPerDay.SelectedIndex = 0;


            }

        }
        private void cbDelayMinutes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            setValues();
        }
        private void cbFlightsPerDay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Route route = (Route)cbRoute.SelectedItem;

            if (route != null && cbFlightsPerDay.SelectedItem != null)
            {
                int latestStartTime = 22;

                TimeSpan routeFlightTime = route.getFlightTime(this.Airliner.Airliner.Type);

                int delayMinutes = (int)cbDelayMinutes.SelectedItem;

                TimeSpan minFlightTime = routeFlightTime.Add(new TimeSpan(0, delayMinutes, 0));

                int flightsPerDay = (int)cbFlightsPerDay.SelectedItem;

                int lastDepartureHour = (int)(latestStartTime - (minFlightTime.TotalHours * flightsPerDay * 2));

                cbStartTime.Items.Clear();

                int startTime = 6;

                if (this.RouteOperations == RouteOperationsType.WholeDay)
                    startTime = 0;

                for (int i = startTime; i <= Math.Max(startTime, lastDepartureHour); i++)
                    for (int j = 0; j < 60; j += 15)
                        cbStartTime.Items.Add(new TimeSpan(i, j, 0));

                cbStartTime.SelectedIndex = cbStartTime.Items.Count / 2;
            }
        }
       
    }
}
