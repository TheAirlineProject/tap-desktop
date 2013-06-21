using System;
using System.Collections.Generic;
using System.Globalization;
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
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel.PopUpRoutesModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;
using Xceed.Wpf.Toolkit;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpAirlinerAutoRoutes.xaml
    /// </summary>
    public partial class PopUpAirlinerAutoRoutes : PopUpWindow
    {

        private FleetAirliner Airliner;
        private ListBox lbFlights;

        public Dictionary<Route, List<RouteTimeTableEntry>> Entries { get; set; }
        private Dictionary<Route, List<RouteTimeTableEntry>> EntriesToDelete;
        public List<RouteTimeTableEntry> NewestEntries;

        private Button btnAdvanced, btnRegular;

        private Frame RouteFrame;

        private Route SelectedRoute;

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
            this.NewestEntries = new List<RouteTimeTableEntry>();

            this.Airliner = airliner;

            InitializeComponent();

            this.Title = this.Airliner.Name;

            this.Width = 1200;

            this.Height = 350;

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
            // mainPanel.Children.Add(grdFlights);

            lbFlights = new ListBox();
            lbFlights.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbFlights.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");
            lbFlights.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            //Grid.SetColumn(lbFlights, 0);
            //grdFlights.Children.Add(lbFlights);

            mainPanel.Children.Add(lbFlights);

            ScrollViewer panelRoutes = createRoutesPanel();

            Grid.SetColumn(panelRoutes, 1);
            grdFlights.Children.Add(panelRoutes);

            this.RouteFrame = new Frame();
            this.RouteFrame.NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden;

            mainPanel.Children.Add(this.RouteFrame);
            //mainPanel.Children.Add(createAutoGeneratePanel());
            mainPanel.Children.Add(createButtonsPanel());

            this.Content = scroller;

            showFlights();

            PageAirlinerAutoRoute pageRoute = new PageAirlinerAutoRoute(this.Airliner, this, PopUpAirlinerAutoRoutes_RouteChanged);

            this.RouteFrame.Navigate(pageRoute);

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


            if (this.SelectedRoute != null)
            {
                var occupiedSlots1 = AirportHelpers.GetOccupiedSlotTimes(this.SelectedRoute.Destination1, this.Airliner.Airliner.Airline).Where(s => s.Days == (int)day);
                var occupiedSlots2 = AirportHelpers.GetOccupiedSlotTimes(this.SelectedRoute.Destination2, this.Airliner.Airliner.Airline).Where(s => s.Days == (int)day);

                int slotLenght = 15;

                foreach (TimeSpan occupiedSlot in occupiedSlots1)
                {
                    ContentControl ccOccupied = new ContentControl();
                    ccOccupied.ContentTemplate = this.Resources["OccupiedItem"] as DataTemplate;
                    ccOccupied.Content = slotLenght / hourFactor;

                    Canvas.SetLeft(ccOccupied, 60 * occupiedSlot.Hours / hourFactor + occupiedSlot.Minutes / hourFactor);

                    cnvFlights.Children.Add(ccOccupied);
                }

                foreach (TimeSpan occupiedSlot in occupiedSlots2)
                {
                    ContentControl ccOccupied = new ContentControl();
                    ccOccupied.ContentTemplate = this.Resources["OccupiedItem"] as DataTemplate;
                    ccOccupied.Content = slotLenght / hourFactor;

                    Canvas.SetLeft(ccOccupied, 60 * occupiedSlot.Hours / hourFactor + occupiedSlot.Minutes / hourFactor);

                    cnvFlights.Children.Add(ccOccupied);

                }

            }

            return brdDay;
        }
        //creates the time indicator header
        private WrapPanel createTimeHeaderPanel()
        {
            int hourFactor = 1;

            WrapPanel panelHeader = new WrapPanel();
            for (int i = 0; i < 24; i++)
            {
                string fromHour = string.Format("{0:00}", Convert.ToInt16(new DateTime(2000, 1, 1, i, 0, 0).ToString("t", CultureInfo.CurrentCulture).Split(':')[0]));
                string toHour = string.Format("{0:00}", Convert.ToInt16(new DateTime(2000, 1, 1, i + 1 == 24 ? 0 : i + 1, 0, 0).ToString("t", CultureInfo.CurrentCulture).Split(':')[0]));

                Border brdHour = new Border();
                brdHour.BorderThickness = new Thickness(1, 0, 1, 0);
                brdHour.BorderBrush = Brushes.Black;

                TextBlock txtHour = new TextBlock();
                //txtHour.Text = string.Format("{0}-{1}", new DateTime(2000, 1, 1, i, 0, 0).ToString(format), new DateTime(2000, 1, 1, i + 1 == 24 ? 0 : i + 1, 0, 0).ToString(format));
                txtHour.Text = string.Format("{0}-{1}", fromHour, toHour);
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

            Button btnClear = new Button();
            btnClear.Uid = "108";
            btnClear.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnClear.Height = Double.NaN;
            btnClear.Width = Double.NaN;
            btnClear.Margin = new Thickness(5, 0, 0, 0);
            btnClear.Content = Translator.GetInstance().GetString("General", btnClear.Uid);
            btnClear.Click += new RoutedEventHandler(btnClear_Click);
            btnClear.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");

            buttonsPanel.Children.Add(btnClear);


            Button btnTransfer = new Button();
            btnTransfer.Uid = "202";
            btnTransfer.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnTransfer.Height = Double.NaN;
            btnTransfer.Width = Double.NaN;
            btnTransfer.Visibility = getTransferAirliners().Count > 0 && this.Airliner.Routes.Count == 0 ? Visibility.Visible : System.Windows.Visibility.Collapsed;
            btnTransfer.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnTransfer.Content = Translator.GetInstance().GetString("PopUpAirlinerAutoRoutes", btnTransfer.Uid);
            btnTransfer.Click += new RoutedEventHandler(btnTransfer_Click);
            btnTransfer.Margin = new Thickness(5, 0, 0, 0);

            buttonsPanel.Children.Add(btnTransfer);

            btnAdvanced = new Button();
            btnAdvanced.Uid = "201";
            btnAdvanced.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnAdvanced.Height = Double.NaN;
            btnAdvanced.Width = Double.NaN;
            btnAdvanced.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnAdvanced.Content = Translator.GetInstance().GetString("PopUpAirlinerAutoRoutes", btnAdvanced.Uid);
            btnAdvanced.Margin = new Thickness(5, 0, 0, 0);
            btnAdvanced.Click += btnAdvanced_Click;

            buttonsPanel.Children.Add(btnAdvanced);

            btnRegular = new Button();
            btnRegular.Uid = "203";
            btnRegular.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnRegular.Height = Double.NaN;
            btnRegular.Width = Double.NaN;
            btnRegular.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnRegular.Content = Translator.GetInstance().GetString("PopUpAirlinerAutoRoutes", btnRegular.Uid);
            btnRegular.Margin = new Thickness(5, 0, 0, 0);
            btnRegular.Visibility = System.Windows.Visibility.Collapsed;
            btnRegular.Click += btnRegular_Click;

            buttonsPanel.Children.Add(btnRegular);

            return buttonsPanel;
        }

        private void btnRegular_Click(object sender, RoutedEventArgs e)
        {
            PageAirlinerAutoRoute pageRoute = new PageAirlinerAutoRoute(this.Airliner, this, PopUpAirlinerAutoRoutes_RouteChanged);

            this.RouteFrame.Navigate(pageRoute);
            this.btnRegular.Visibility = System.Windows.Visibility.Collapsed;
            this.btnAdvanced.Visibility = System.Windows.Visibility.Visible;
        }

        private void PopUpAirlinerAutoRoutes_RouteChanged(Route route)
        {
            this.SelectedRoute = route;
            showFlights();
        }
        private void btnAdvanced_Click(object sender, RoutedEventArgs e)
        {
            PageAirlinerAdvancedRoute pageRoute = new PageAirlinerAdvancedRoute(this.Airliner, this, PopUpAirlinerAutoRoutes_RouteChanged);

            this.RouteFrame.Navigate(pageRoute);
            this.btnAdvanced.Visibility = System.Windows.Visibility.Collapsed;
            this.btnRegular.Visibility = System.Windows.Visibility.Visible;
        }


        private void btnTransfer_Click(object sender, RoutedEventArgs e)
        {
            ComboBox cbAirliners = new ComboBox();
            cbAirliners.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbAirliners.SelectedValuePath = "Name";
            cbAirliners.DisplayMemberPath = "Name";
            cbAirliners.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbAirliners.Width = 200;

            foreach (FleetAirliner airliner in getTransferAirliners())
                cbAirliners.Items.Add(airliner);

            cbAirliners.SelectedIndex = 0;

            if (PopUpSingleElement.ShowPopUp(Translator.GetInstance().GetString("PopUpAirlinerRoutes", "1001"), cbAirliners) == PopUpSingleElement.ButtonSelected.OK && cbAirliners.SelectedItem != null)
            {
                FleetAirliner transferAirliner = (FleetAirliner)cbAirliners.SelectedItem;

                foreach (Route route in transferAirliner.Routes)
                {
                    foreach (RouteTimeTableEntry entry in route.TimeTable.Entries.FindAll(en => en.Airliner == transferAirliner))
                    {
                        entry.Airliner = this.Airliner;
                    }
                    this.Airliner.addRoute(route);
                }
                transferAirliner.Routes.Clear();

                showFlights();
            }
        }

        //clears the time table
        public void clearTimeTable()
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
        public void showFlights()
        {
            lbFlights.Items.Clear();

            lbFlights.Items.Add(new QuickInfoValue("Day", createTimeHeaderPanel()));
            //CultureInfo ci = new CultureInfo(language.CultureInfo, true);

            DayOfWeek firstDay = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            for (int dayIndex = 0; dayIndex < 7; dayIndex++)
            {
                var currentDay = (DayOfWeek)(((int)firstDay + dayIndex) % 7);

                lbFlights.Items.Add(new QuickInfoValue(currentDay.ToString(), createRoutePanel(currentDay)));
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
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            clearTimeTable();

            showFlights();

        }
        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            

            foreach (RouteTimeTableEntry entry in this.NewestEntries)
                this.Entries[entry.TimeTable.Route].Remove(entry);

            this.NewestEntries.Clear();

            showFlights();

        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = null;
            this.Close();
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
        //returns the airliners from where the airliner can transfer schedule
        private List<FleetAirliner> getTransferAirliners()
        {
            long maxDistance = this.Airliner.Airliner.Type.Range;

            long requiredRunway = this.Airliner.Airliner.Type.MinRunwaylength;

            return GameObject.GetInstance().HumanAirline.Fleet.FindAll(a => a != this.Airliner && a.Routes.Count > 0 && a.Status == FleetAirliner.AirlinerStatus.Stopped && a.Routes.Max(r => r.getDistance()) <= maxDistance);
        }
    }
    public class RouteItemConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            if (value is Route)
            {
                Route route = (Route)value;

                string outboundRoute;
                if (route.HasStopovers)
                {
                    string stopovers = string.Join("-", from s in route.Stopovers select new AirportCodeConverter().Convert(s.Stopover));
                    outboundRoute = string.Format("{0}-{1}-{2}", new AirportCodeConverter().Convert(route.Destination1), stopovers, new AirportCodeConverter().Convert(route.Destination2));
                }
                else
                    outboundRoute = string.Format("{0}-{1}", new AirportCodeConverter().Convert(route.Destination1), new AirportCodeConverter().Convert(route.Destination2));

                return outboundRoute;
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
