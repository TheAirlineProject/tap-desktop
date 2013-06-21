using System;
using System.Collections.Generic;
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
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;
using Xceed.Wpf.Toolkit;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel.PopUpRoutesModel
{
    /// <summary>
    /// Interaction logic for PageAilinerAdvancedRoute.xaml
    /// </summary>
    public partial class PageAirlinerAdvancedRoute : Page
    {
        private FleetAirliner Airliner;
        private PopUpAirlinerAutoRoutes ParentPage;

        private ComboBox cbOrigin, cbDestination, cbDay, cbFlightCode;
        private TimePicker tpTime;

        private TextBlock txtStopovers, txtFlightTime;

        public delegate void OnRouteChanged(Route route);
        public event OnRouteChanged RouteChanged;

        public PageAirlinerAdvancedRoute(FleetAirliner airliner, PopUpAirlinerAutoRoutes parent, OnRouteChanged routeChanged)
        {
            this.ParentPage = parent;
            this.Airliner = airliner;
            this.RouteChanged += routeChanged;

            InitializeComponent();

            StackPanel panelMain = new StackPanel();

            panelMain.Children.Add(createNewEntryPanel());

            WrapPanel panelFlightTime = new WrapPanel();

            txtStopovers = UICreator.CreateTextBlock("");
            txtStopovers.Visibility = System.Windows.Visibility.Collapsed;
            txtStopovers.Margin = new Thickness(0, 0, 10, 0);
            panelFlightTime.Children.Add(txtStopovers);

            txtFlightTime = UICreator.CreateTextBlock("Flight time:");
            panelFlightTime.Children.Add(txtFlightTime);

            panelMain.Children.Add(panelFlightTime);

            this.Content = panelMain;

            cbOrigin.SelectedIndex = 0;


        }
        //creates the panel for adding a new entry
        private StackPanel createNewEntryPanel()
        {
            StackPanel newEntryPanel = new StackPanel();
            newEntryPanel.Margin = new Thickness(0, 10, 0, 0);

            WrapPanel entryPanel = new WrapPanel();

            newEntryPanel.Children.Add(entryPanel);

            Route.RouteType type = this.Airliner.Airliner.Type.TypeAirliner == AirlinerType.TypeOfAirliner.Cargo ? Route.RouteType.Cargo : Route.RouteType.Passenger;

            var origins = this.Airliner.Airliner.Airline.Routes.Where(r=>r.Type == type).SelectMany(r => r.getDestinations()).Distinct();
            origins.OrderBy(a => a.Profile.Name);

            cbOrigin = new ComboBox();
            cbOrigin.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbOrigin.SetResourceReference(ComboBox.ItemTemplateProperty, "AirportIATACountryItem");
            cbOrigin.Width = 100;
            cbOrigin.SelectionChanged += cbOrigin_SelectionChanged;

            foreach (Airport origin in origins)
                cbOrigin.Items.Add(origin);

            entryPanel.Children.Add(cbOrigin);


            TextBlock txtTo = UICreator.CreateTextBlock("->");
            txtTo.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            txtTo.Margin = new Thickness(5, 0, 5, 0);
            entryPanel.Children.Add(txtTo);

            cbDestination = new ComboBox();
            cbDestination.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbDestination.SelectionChanged += cbDestination_SelectionChanged;
            cbDestination.Width = 100;

            entryPanel.Children.Add(cbDestination);

            cbDay = new ComboBox();
            cbDay.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbDay.Width = 100;
            cbDay.Margin = new Thickness(10, 0, 0, 0);
            cbDay.Items.Add("Daily");

            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                cbDay.Items.Add(day);

            cbDay.SelectedIndex = 0;

            entryPanel.Children.Add(cbDay);

            tpTime = new TimePicker();
            tpTime.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            tpTime.EndTime = new TimeSpan(22, 0, 0);
            tpTime.StartTime = new TimeSpan(6, 0, 0);
            tpTime.Value = new DateTime(2011, 1, 1, 13, 0, 0);
            tpTime.Format = TimeFormat.ShortTime;
            tpTime.Background = Brushes.Transparent;
            tpTime.SetResourceReference(TimePicker.ForegroundProperty, "TextColor");
            tpTime.BorderBrush = Brushes.Black;

            entryPanel.Children.Add(tpTime);

            cbFlightCode = new ComboBox();
            cbFlightCode.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");

            foreach (string flightCode in this.Airliner.Airliner.Airline.getFlightCodes())
                cbFlightCode.Items.Add(flightCode);

            cbFlightCode.SelectedIndex = 0;

            entryPanel.Children.Add(cbFlightCode);

            Button btnAdd = new Button();
            btnAdd.Uid = "104";
            btnAdd.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnAdd.Height = Double.NaN;
            btnAdd.Width = Double.NaN;
            btnAdd.Click += new RoutedEventHandler(btnAdd_Click);
            btnAdd.Margin = new Thickness(5, 0, 0, 0);
            btnAdd.Content = Translator.GetInstance().GetString("General", btnAdd.Uid);
            btnAdd.IsEnabled = cbOrigin.Items.Count > 0;
            btnAdd.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            btnAdd.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");

            entryPanel.Children.Add(btnAdd);

      
            return newEntryPanel;

        }

        private void cbDestination_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbDestination.SelectedItem != null)
            {
                ComboBoxItem item = (ComboBoxItem)cbDestination.SelectedItem;
           
                Route route = (Route)item.Tag;

                if (route.HasStopovers)
                {
                    txtStopovers.Text = "Stopovers: " + string.Join(",", from s in route.Stopovers select new AirportCodeConverter().Convert(s.Stopover));
                    txtStopovers.Visibility = System.Windows.Visibility.Visible;
                }
                else
                    txtStopovers.Visibility = System.Windows.Visibility.Collapsed;

                txtFlightTime.Text = string.Format("Flight time: {0}", route.getFlightTime(this.Airliner.Airliner.Type).ToString("hh\\:mm"));

                if (this.RouteChanged != null)
                    this.RouteChanged(route);
         
            }
        }

        private void cbOrigin_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbOrigin.SelectedItem != null)
            {
                cbDestination.Items.Clear();

                Airport origin = (Airport)cbOrigin.SelectedItem;

                Route.RouteType type = this.Airliner.Airliner.Type.TypeAirliner == AirlinerType.TypeOfAirliner.Cargo ? Route.RouteType.Cargo : Route.RouteType.Passenger;

                var routes = this.Airliner.Airliner.Airline.Routes.Where(r => r.Type == type && (r.Destination1 == origin || r.Destination2 == origin));//.Select(r => r.Destination1 == origin ? r.Destination2 : r.Destination1);

                foreach (Route route in routes)
                {
                    Airport destination = route.Destination1 == origin ? route.Destination2 : route.Destination1;

                    ComboBoxItem itemRoute = new ComboBoxItem();
                    itemRoute.SetResourceReference(ComboBoxItem.ContentTemplateProperty, "AirportIATACountryItem");
                    itemRoute.Tag = route;
                    itemRoute.Content = destination;
                    cbDestination.Items.Add(itemRoute);
                }


                cbDestination.SelectedIndex = 0;
            }
        }
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxItem item = (ComboBoxItem)cbDestination.SelectedItem;

            Route route = (Route)item.Tag;

            Airport airport = (Airport)item.Content;

            TimeSpan time = new TimeSpan(tpTime.Value.Value.Hour, tpTime.Value.Value.Minute, 0);

            string day = cbDay.SelectedItem.ToString();

            if (!this.ParentPage.Entries.ContainsKey(route))
                this.ParentPage.Entries.Add(route, new List<RouteTimeTableEntry>());

            string flightCode = cbFlightCode.SelectedItem.ToString();

            RouteTimeTable rt = new RouteTimeTable(route);

            if (day == "Daily")
            {

                foreach (DayOfWeek dayOfWeek in Enum.GetValues(typeof(DayOfWeek)))
                {
                    RouteTimeTableEntry entry = new RouteTimeTableEntry(route.TimeTable, dayOfWeek, time, new RouteEntryDestination(airport, flightCode));
                    entry.Airliner = this.Airliner;

                    rt.addEntry(entry);

                }

            }
            else
            {
                RouteTimeTableEntry entry = new RouteTimeTableEntry(route.TimeTable, (DayOfWeek)cbDay.SelectedItem, time, new RouteEntryDestination(airport, flightCode));
                entry.Airliner = this.Airliner;

                rt.addEntry(entry);

            }

            if (!TimeTableHelpers.IsTimeTableValid(rt, this.Airliner, this.ParentPage.Entries))
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2706"), Translator.GetInstance().GetString("MessageBox", "2706", "message"), WPFMessageBoxButtons.Ok);

            else
            {
                this.ParentPage.NewestEntries.Clear();
                foreach (RouteTimeTableEntry entry in rt.Entries)
                {
                    this.ParentPage.Entries[route].Add(entry);

                    this.ParentPage.NewestEntries.Add(entry);
                }

            }

            this.ParentPage.showFlights();
        }
    }
}
