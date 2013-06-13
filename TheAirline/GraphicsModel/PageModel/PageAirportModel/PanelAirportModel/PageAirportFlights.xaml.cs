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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheAirline.Model.AirportModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.GraphicsModel.UserControlModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.Model.GeneralModel.Helpers;
using System.ComponentModel;

namespace TheAirline.GraphicsModel.PageModel.PageAirportModel.PanelAirportModel
{
    /// <summary>
    /// Interaction logic for PageAirportFlights.xaml
    /// </summary>
    public partial class PageAirportFlights : Page
    {
        private Airport Airport;
        private StackPanel panelDestinationFlights;
        private ListBox lbStatistics, lbDestinationArrivals, lbDestinationDepartures;
        private ListBox lbFlights;
        private ListSortDirection sortDirection = ListSortDirection.Ascending;
        public PageAirportFlights(Airport airport)
        {
            InitializeComponent();

            this.Airport = airport;

            StackPanel panelFlights = new StackPanel();
            panelFlights.Margin = new Thickness(0, 10, 50, 0);

            ContentControl ccHeader = new ContentControl();
            ccHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            ccHeader.ContentTemplate = this.Resources["DestinationsHeader"] as DataTemplate;
            panelFlights.Children.Add(ccHeader);

            /*
            TextBlock txtHeader = new TextBlock();
            txtHeader.Uid = "1001";
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PageAirportFlights", txtHeader.Uid);
            
            panelFlights.Children.Add(txtHeader);
            */

            lbStatistics = new ListBox();
            lbStatistics.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbStatistics.ItemTemplate = this.Resources["DestinationItem"] as DataTemplate;
            lbStatistics.MaxHeight = (GraphicsHelpers.GetContentHeight() - 100) / 2;

            var items = new List<DestinationFlights>();

            foreach (Airport a in getDestinations().Keys)
                items.Add(new DestinationFlights(a, getDestinations()[a]));

            items.OrderBy(i => i.Airport.Profile.Name);

            lbStatistics.ItemsSource = items;

            panelFlights.Children.Add(lbStatistics);

            //panelFlights.Children.Add(createDestinationFlightsPanel());

            WrapPanel panelButtons = new WrapPanel();
            panelButtons.Margin = new Thickness(0, 5, 0, 0);

            Button btnSlotAllocation = new Button();
            btnSlotAllocation.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnSlotAllocation.Height = Double.NaN;
            btnSlotAllocation.Width = Double.NaN;
            btnSlotAllocation.Uid = "200";
            btnSlotAllocation.Content = Translator.GetInstance().GetString("PageAirportFlights", btnSlotAllocation.Uid);
            btnSlotAllocation.Click += new RoutedEventHandler(btnSlotAllocation_Click);
            btnSlotAllocation.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnSlotAllocation.Visibility = AirportHelpers.GetAirportRoutes(this.Airport).Count > 0 ? Visibility.Visible : Visibility.Collapsed;

            panelButtons.Children.Add(btnSlotAllocation);

            Button btnFlightMap = new Button();
            btnFlightMap.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnFlightMap.Height = Double.NaN;
            btnFlightMap.Width = Double.NaN;
            btnFlightMap.Uid ="201";
            btnFlightMap.Content = Translator.GetInstance().GetString("PageAirportFlights", btnFlightMap.Uid);
            btnFlightMap.Click += btnFlightMap_Click;
            btnFlightMap.SetResourceReference(Button.BackgroundProperty,"ButtonBrush");
            btnFlightMap.Margin = new Thickness(5,0,0,0);
            btnFlightMap.Visibility = AirportHelpers.GetAirportRoutes(this.Airport).Count > 0 ? Visibility.Visible : System.Windows.Visibility.Collapsed;

            panelButtons.Children.Add(btnFlightMap);

            panelFlights.Children.Add(panelButtons);
            panelFlights.Children.Add(createFlightsPanel());

            this.Content = panelFlights;
           
        }
        //creates the panel for flights
        private StackPanel createFlightsPanel()
        {
            StackPanel panelFlights = new StackPanel();
            panelFlights.Margin = new Thickness(0, 20, 0, 0);

               WrapPanel panelButtons = new WrapPanel();

            ucSelectButton sbArrivals = new ucSelectButton();
            sbArrivals.Uid = "1029";
            sbArrivals.Content = Translator.GetInstance().GetString("PageAirport", sbArrivals.Uid);
            sbArrivals.IsSelected = true;
            sbArrivals.Click += sbArrivals_Click;
            panelButtons.Children.Add(sbArrivals);

            ucSelectButton sbDepartures = new ucSelectButton();
            sbDepartures.Uid = "1030";
            sbDepartures.Content = Translator.GetInstance().GetString("PageAirport", sbDepartures.Uid);
            sbDepartures.Click += sbDepartures_Click;
            panelButtons.Children.Add(sbDepartures);

            panelFlights.Children.Add(panelButtons);

            ContentControl ccHeader = new ContentControl();
            ccHeader.ContentTemplate = this.Resources["FlightHeader"] as DataTemplate;
            panelFlights.Children.Add(ccHeader);

             lbFlights = new ListBox();
            lbFlights.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbFlights.ItemTemplate = this.Resources["FlightItem"] as DataTemplate;
            lbFlights.MaxHeight = (GraphicsHelpers.GetContentHeight() - 100) / 2;
            panelFlights.Children.Add(lbFlights);

            showFlights(true);


            return panelFlights;

        }
      
        //shows the departures or arrivals
        private void showFlights(Boolean arrivals)
        {
            lbFlights.Items.Clear();

            var entries = arrivals ? GeneralHelpers.GetAirportArrivals(this.Airport, GameObject.GetInstance().GameTime.DayOfWeek) : GeneralHelpers.GetAirportDepartures(this.Airport, GameObject.GetInstance().GameTime.DayOfWeek);

            GameTimeZone tz = this.Airport.Profile.TimeZone;

            foreach (RouteTimeTableEntry entry in entries)
            {
                if (arrivals)
                {
                    TimeSpan flightTime = MathHelpers.GetFlightTime(entry.getDepartureAirport().Profile.Coordinates, entry.Destination.Airport.Profile.Coordinates, entry.Airliner.Airliner.Type);
                    lbFlights.Items.Add(new AirportFlightItem(MathHelpers.ConvertDateTimeToLoalTime(MathHelpers.ConvertEntryToDate(entry).Add(flightTime), tz), entry.Airliner.Airliner.Airline, entry.Destination.Airport == entry.TimeTable.Route.Destination1 ? entry.TimeTable.Route.Destination2 : entry.TimeTable.Route.Destination1, entry.Destination.FlightCode));
                }
                else
                {
                    lbFlights.Items.Add(new AirportFlightItem(MathHelpers.ConvertDateTimeToLoalTime(MathHelpers.ConvertEntryToDate(entry), tz), entry.Airliner.Airliner.Airline, entry.Destination.Airport, entry.Destination.FlightCode));
                }
            }
        }
       
       
        //creates the panel for destination flights
        private StackPanel createDestinationFlightsPanel()
        {
            panelDestinationFlights = new StackPanel();
            panelDestinationFlights.Margin = new Thickness(0, 10, 0, 0);
            panelDestinationFlights.Visibility = System.Windows.Visibility.Collapsed;

            ScrollViewer svFlights = new ScrollViewer();
            svFlights.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            svFlights.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            svFlights.MaxHeight = GraphicsHelpers.GetContentHeight() / 2;

            panelDestinationFlights.Children.Add(svFlights);

            StackPanel panelFlights = new StackPanel();
            svFlights.Content = panelFlights;

            TextBlock txtDestinationHeader = new TextBlock();
            txtDestinationHeader.Uid = "1004";
            txtDestinationHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtDestinationHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtDestinationHeader.FontWeight = FontWeights.Bold;
            txtDestinationHeader.Text = Translator.GetInstance().GetString("PageAirportFlights", txtDestinationHeader.Uid);

            panelFlights.Children.Add(txtDestinationHeader);

            Grid grdArrivalsHeader = UICreator.CreateGrid(2);
            grdArrivalsHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            panelFlights.Children.Add(grdArrivalsHeader);

            Image imgArrivalsHeader = new Image();
            imgArrivalsHeader.Source = new BitmapImage(new Uri(@"/Data/images/Arrivals.png", UriKind.RelativeOrAbsolute));
            imgArrivalsHeader.Height = 20;
            RenderOptions.SetBitmapScalingMode(imgArrivalsHeader, BitmapScalingMode.HighQuality);

            Grid.SetColumn(imgArrivalsHeader, 0);
            grdArrivalsHeader.Children.Add(imgArrivalsHeader);

            TextBlock txtArrivalsHeader = UICreator.CreateTextBlock(Translator.GetInstance().GetString("PageAirportFlights", "1002"));
            txtArrivalsHeader.FontStyle = FontStyles.Oblique;
            txtArrivalsHeader.FontSize = 16;

            Grid.SetColumn(txtArrivalsHeader, 1);
            grdArrivalsHeader.Children.Add(txtArrivalsHeader);

            lbDestinationArrivals = new ListBox();
            lbDestinationArrivals.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbDestinationArrivals.ItemTemplate = this.Resources["AirportDestinationItem"] as DataTemplate;
            panelFlights.Children.Add(lbDestinationArrivals);

            Grid grdDeparturesHeader = UICreator.CreateGrid(2);
            grdDeparturesHeader.Margin = new Thickness(0, 5, 0, 0);
            grdDeparturesHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            panelFlights.Children.Add(grdDeparturesHeader);

            Image imgDeparturesHeader = new Image();
            imgDeparturesHeader.Source = new BitmapImage(new Uri(@"/Data/images/Departures.png", UriKind.RelativeOrAbsolute));
            imgDeparturesHeader.Height = 20;
            RenderOptions.SetBitmapScalingMode(imgDeparturesHeader, BitmapScalingMode.HighQuality);

            Grid.SetColumn(imgDeparturesHeader, 0);
            grdDeparturesHeader.Children.Add(imgDeparturesHeader);

            TextBlock txtDeparturesHeader = UICreator.CreateTextBlock(Translator.GetInstance().GetString("PageAirportFlights", "1003"));
            txtDeparturesHeader.FontStyle = FontStyles.Oblique;
            txtDeparturesHeader.FontSize = 16;

            Grid.SetColumn(txtDeparturesHeader, 1);
            grdDeparturesHeader.Children.Add(txtDeparturesHeader);

            lbDestinationDepartures = new ListBox();
            lbDestinationDepartures.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbDestinationDepartures.ItemTemplate = this.Resources["AirportDestinationItem"] as DataTemplate;
            //lbDestinationDepartures.MaxHeight = GraphicsHelpers.GetContentHeight() / 2 - 100;
            panelFlights.Children.Add(lbDestinationDepartures);
            
            return panelDestinationFlights;
        }
        //returns the list of destinations for the airport
        private Dictionary<Airport, int> getDestinations()
        {
          
            Dictionary<Airport, int> destinations = new Dictionary<Airport, int>();
            foreach (Route route in AirportHelpers.GetAirportRoutes(this.Airport).FindAll(r=>r.getAirliners().Count>0))
            {
                if (route.Destination1 != this.Airport)
                {
                    if (!destinations.ContainsKey(route.Destination1))
                        destinations.Add(route.Destination1, 0);
                    destinations[route.Destination1] += route.TimeTable.getEntries(route.Destination1).Count;


                }
                if (route.Destination2 != this.Airport)
                {
                    if (!destinations.ContainsKey(route.Destination2))
                        destinations.Add(route.Destination2, 0);
                    destinations[route.Destination2] += route.TimeTable.getEntries(route.Destination2).Count;
                }
            }
            return destinations;

        }
        private void btnFlightMap_Click(object sender, RoutedEventArgs e)
        {

            PopUpMap.ShowPopUp(AirportHelpers.GetAirportRoutes(this.Airport));
        }

        private void btnSlotAllocation_Click(object sender, RoutedEventArgs e)
        {
            PopUpAirportSlot.ShowPopUp(this.Airport);

        }
        private void sbDepartures_Click(object sender, RoutedEventArgs e)
        {
            showFlights(false);
        }
        private void sbArrivals_Click(object sender, RoutedEventArgs e)
        {
            showFlights(true);
        }
        private void Header_Click(object sender, RoutedEventArgs e)
        {
            string type = (string)((Hyperlink)sender).Tag;

            string name = "Destination";

            switch (type)
            {
                case "Destination":
                    name = "Airport.Profile.IATACode";
                    break;
                case "Flights":
                    name = "Flights";
                    break;
               

            }

            sortDirection = sortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;

            ICollectionView dataView =
              CollectionViewSource.GetDefaultView(lbStatistics.ItemsSource);

            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(name, sortDirection);
            dataView.SortDescriptions.Add(sd);

        }
        //the class for a flight at the airport
        private class AirportFlightItem
        {
            public Airline Airline { get; set; }
            public string Flight { get; set; }
            public DateTime Time { get; set; }
            public Airport Airport { get; set; }
            public AirportFlightItem(DateTime time, Airline airline, Airport airport, string flight)
            {
                this.Time = time;
                this.Airport = airport;
                this.Airline = airline;
                this.Flight = flight;
            }
        }
        //the class for a destination flight
        public class DestinationFlight
        {
            public Airline Airline { get; set; }
            public RouteTimeTableEntry Entry { get; set; }
            public DateTime Time { get; set; }
            public AirlinerType AirlinerType { get; set; }
            public DestinationFlight(Airline airline, RouteTimeTableEntry entry, DateTime time)
            {
                this.Airline = airline;
                this.Entry = entry;
                this.Time = time;
                this.AirlinerType = this.Entry.Airliner.Airliner.Type;
            }
        }
        //the class for a destination with number of weekly flights
        public class DestinationFlights
        {
            public int Flights { get; set; }
            public Airport Airport { get; set; }
            public DestinationFlights(Airport airport, int flights)
            {
                this.Flights = flights;
                this.Airport = airport;
            }
        }


    }
}
