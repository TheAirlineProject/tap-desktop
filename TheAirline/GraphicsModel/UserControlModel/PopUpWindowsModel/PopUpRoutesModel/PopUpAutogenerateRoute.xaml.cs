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
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.Converters;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpAutogenerateRoute.xaml
    /// </summary>
    public partial class PopUpAutogenerateRoute : PopUpWindow
    {
        private FleetAirliner Airliner;
        private ComboBox cbRoute, cbFlightsPerDay, cbFlightCode, cbRegion;
        private CheckBox cbBusinessRoute;
        private double maxBusinessRouteTime = new TimeSpan(2, 0, 0).TotalMinutes;

        public static object ShowPopUp(FleetAirliner airliner)
        {
            PopUpWindow window = new PopUpAutogenerateRoute(airliner);
            window.ShowDialog();

            return window.Selected;
        }
        public PopUpAutogenerateRoute(FleetAirliner airliner)
        {
            this.Airliner = airliner;

            InitializeComponent();

            this.Title = "Time table for " + this.Airliner.Name;

            this.Width = 600;

            this.Height = 125;

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            StackPanel mainPanel = new StackPanel();
            mainPanel.Margin = new Thickness(10, 10, 10, 10);

            mainPanel.Children.Add(createAutoGeneratePanel());
            mainPanel.Children.Add(createButtonsPanel());

            this.Content = mainPanel;
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
            
           
            return buttonsPanel;
        }

      
        //creates the panel for auto generation of time table from a route
        private WrapPanel createAutoGeneratePanel()
        {
            WrapPanel autogeneratePanel = new WrapPanel();

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
            cbRoute.ItemTemplate = this.Resources["RouteItem"] as DataTemplate;

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
                foreach (Route route in this.Airliner.Airliner.Airline.Routes.FindAll(r => this.Airliner.Airliner.Type.Range >r.getDistance() && !r.Banned && r.Destination1.getMaxRunwayLength() >= requiredRunway && r.Destination2.getMaxRunwayLength() >= requiredRunway).OrderBy(r => new AirportCodeConverter().Convert(r.Destination1)).ThenBy(r=>new AirportCodeConverter().Convert(r.Destination2)))
                {
                    cbRoute.Items.Add(route);
                }
            }
            else
            {
                var routes = this.Airliner.Airliner.Airline.Routes.FindAll(r => this.Airliner.Airliner.Type.Range >r.getDistance() && !r.Banned && r.Destination1.getMaxRunwayLength() >= requiredRunway && r.Destination2.getMaxRunwayLength() >= requiredRunway && ((r.Destination1.Profile.Country.Region == region && r.Destination2.Profile.Country.Region == GameObject.GetInstance().HumanAirline.Profile.Country.Region) || (r.Destination2.Profile.Country.Region == region && r.Destination1.Profile.Country.Region == GameObject.GetInstance().HumanAirline.Profile.Country.Region) || (r.Destination1.Profile.Country.Region == region && r.Destination2.Profile.Country.Region == region))).OrderBy(r => new AirportCodeConverter().Convert(r.Destination1)).ThenBy(r=>new AirportCodeConverter().Convert(r.Destination2));

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

                int maxHours = 22 - 6; //from 06.00 to 22.00

                int flightsPerDay = Convert.ToInt16(maxHours * 60 / (2 * minFlightTime.TotalMinutes));

                cbFlightsPerDay.Items.Clear();

                for (int i = 0; i < Math.Max(1,flightsPerDay); i++)
                    cbFlightsPerDay.Items.Add(i + 1);

                cbFlightsPerDay.SelectedIndex = 0;

            
                cbBusinessRoute.Visibility = minFlightTime.TotalMinutes <= maxBusinessRouteTime ? Visibility.Visible : System.Windows.Visibility.Collapsed;

                if (minFlightTime.TotalMinutes > maxBusinessRouteTime)
                    cbBusinessRoute.IsChecked = false;
                
            }


        }
       
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

       
        private void cbBusinessRoute_Checked(object sender, RoutedEventArgs e)
        {
            cbFlightsPerDay.IsEnabled = false;
        }

        private void cbBusinessRoute_Unchecked(object sender, RoutedEventArgs e)
        {
            cbFlightsPerDay.IsEnabled = true;
        }
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {

            Route route = (Route)cbRoute.SelectedItem;

            int flightsPerDay = (int)cbFlightsPerDay.SelectedItem;

            string flightcode1 = cbFlightCode.SelectedItem.ToString();
            string flightcode2 = this.Airliner.Airliner.Airline.getFlightCodes()[this.Airliner.Airliner.Airline.getFlightCodes().IndexOf(flightcode1) + 1];
            
            if (flightsPerDay > 0)
            {
                if (cbBusinessRoute.IsChecked.Value)
                {
                    flightsPerDay = (int)(route.getFlightTime(this.Airliner.Airliner.Type).Add(FleetAirlinerHelpers.GetMinTimeBetweenFlights(this.Airliner)).TotalMinutes / 2 / maxBusinessRouteTime);
                    this.Selected = AIHelpers.CreateBusinessRouteTimeTable(route, this.Airliner, Math.Max(1, flightsPerDay), flightcode1, flightcode2);
                }
                else
                    this.Selected = AIHelpers.CreateAirlinerRouteTimeTable(route, this.Airliner, flightsPerDay, flightcode1, flightcode2);
            }
            else
                this.Selected = null;
            this.Close();
        }

       
    }
    
}
