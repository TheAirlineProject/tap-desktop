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
        private ComboBox cbRoute, cbFlightsPerDay, cbFlightCode;

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

            this.Width = 400;

            this.Height = 100;

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

            cbRoute = new ComboBox();
            cbRoute.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbRoute.SelectionChanged += new SelectionChangedEventHandler(cbAutoRoute_SelectionChanged);
            cbRoute.ItemTemplate = this.Resources["RouteItem"] as DataTemplate;

            foreach (Route route in this.Airliner.Airliner.Airline.Routes.FindAll(r => this.Airliner.Airliner.Type.Range > MathHelpers.GetDistance(r.Destination1.Profile.Coordinates, r.Destination2.Profile.Coordinates) && !r.Banned))
            {
                cbRoute.Items.Add(route);
            }

            autogeneratePanel.Children.Add(cbRoute);

            cbFlightCode = new ComboBox();
            cbFlightCode.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");

            foreach (string flightCode in this.Airliner.Airliner.Airline.getFlightCodes())
                cbFlightCode.Items.Add(flightCode);

            cbFlightCode.SelectedIndex = 0;

            autogeneratePanel.Children.Add(cbFlightCode);

            TextBlock txtFlightsPerDay = UICreator.CreateTextBlock("Flights per day");
            //txtFlightsPerDay.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            txtFlightsPerDay.Margin = new Thickness(10, 0, 5, 0);

            autogeneratePanel.Children.Add(txtFlightsPerDay);

            cbFlightsPerDay = new ComboBox();
            cbFlightsPerDay.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");

            cbRoute.SelectedIndex = 0;

            autogeneratePanel.Children.Add(cbFlightsPerDay);
            return autogeneratePanel;


        }
        private void cbAutoRoute_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {


            Route route = (Route)cbRoute.SelectedItem;

            if (route != null)
            {
                TimeSpan minFlightTime = MathHelpers.GetFlightTime(route.Destination1.Profile.Coordinates, route.Destination2.Profile.Coordinates, this.Airliner.Airliner.Type).Add(new TimeSpan(RouteTimeTable.MinTimeBetweenFlights.Ticks));

                int maxHours = 22 - 6; //from 06.00 to 22.00

                int flightsPerDay = Convert.ToInt16(maxHours * 60 / (2 * minFlightTime.TotalMinutes));

                cbFlightsPerDay.Items.Clear();

                for (int i = 0; i < flightsPerDay; i++)
                    cbFlightsPerDay.Items.Add(i + 1);

                cbFlightsPerDay.SelectedIndex = 0;

            }


        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = -1;
            this.Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {

            Route route = (Route)cbRoute.SelectedItem;

            int flightsPerDay = (int)cbFlightsPerDay.SelectedItem;

            string flightcode1 = cbFlightCode.SelectedItem.ToString();
            string flightcode2 = this.Airliner.Airliner.Airline.getFlightCodes()[this.Airliner.Airliner.Airline.getFlightCodes().IndexOf(flightcode1)+1];
            
            //WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2701"), string.Format(Translator.GetInstance().GetString("MessageBox", "2701", "message"), route.Destination1.Profile.Name, route.Destination2.Profile.Name), WPFMessageBoxButtons.YesNo);

            //if (result == WPFMessageBoxResult.Yes)
            {

              
                AIHelpers.CreateRouteTimeTable(route, this.Airliner, flightsPerDay,flightcode1,flightcode2);

                if (!this.Airliner.Routes.Contains(route))
                    this.Airliner.addRoute(route);

        
            }
             
            this.Selected = flightsPerDay;
            this.Close();
        }
       
    }
}
