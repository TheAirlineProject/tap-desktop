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

namespace TheAirline.GraphicsModel.PageModel.PageAirportModel.PanelAirportModel
{
    /// <summary>
    /// Interaction logic for PageAirportFlights.xaml
    /// </summary>
    public partial class PageAirportFlights : Page
    {
        private Airport Airport;
        private StackPanel panelDestinationFlights;
        private ListBox lbDestinationArrivals, lbDestinationDepartures;
        public PageAirportFlights(Airport airport)
        {
            InitializeComponent();

            this.Airport = airport;

            StackPanel panelFlights = new StackPanel();
            panelFlights.Margin = new Thickness(0, 10, 50, 0);

            TextBlock txtHeader = new TextBlock();
            txtHeader.Uid = "1001";
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PageAirportFlights", txtHeader.Uid);

            panelFlights.Children.Add(txtHeader);

            ListBox lbStatistics = new ListBox();
            lbStatistics.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbStatistics.ItemTemplate = this.Resources["DestinationItem"] as DataTemplate;

            foreach (Airport a in getDestinations().Keys)
                lbStatistics.Items.Add(new DestinationFlights(a, getDestinations()[a]));

            panelFlights.Children.Add(lbStatistics);

            panelFlights.Children.Add(createDestinationFlightsPanel());

            Button btnSlotAllocation = new Button();
            btnSlotAllocation.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnSlotAllocation.Height = Double.NaN;
            btnSlotAllocation.Width = Double.NaN;
            btnSlotAllocation.Content = "Slot allocations";
            btnSlotAllocation.Click += new RoutedEventHandler(btnSlotAllocation_Click);
            btnSlotAllocation.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnSlotAllocation.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnSlotAllocation.Visibility = this.Airport.Terminals.getRoutes().Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            btnSlotAllocation.Margin = new Thickness(0, 5, 0, 0);

            panelFlights.Children.Add(btnSlotAllocation);
          
            this.Content = panelFlights;
           
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
            foreach (Route route in this.Airport.Terminals.getRoutes().FindAll(r=>r.getAirliners().Count>0))
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
        private void btnSlotAllocation_Click(object sender, RoutedEventArgs e)
        {
            PopUpAirportSlot.ShowPopUp(this.Airport);

        }
        private void lnkAirport_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Airport airport = (Airport)((ContentControl)sender).Tag;

            panelDestinationFlights.Visibility = System.Windows.Visibility.Visible;

            lbDestinationArrivals.Items.Clear();
            lbDestinationDepartures.Items.Clear();
            
            GameTimeZone tz = this.Airport.Profile.TimeZone;
             
            foreach (RouteTimeTableEntry entry in GeneralHelpers.GetAirportFlights(this.Airport,airport,false))
            {
                lbDestinationDepartures.Items.Add(new DestinationFlight(entry.Airliner.Airliner.Airline, entry,MathHelpers.ConvertDateTimeToLoalTime(MathHelpers.ConvertEntryToDate(entry), tz)));
            }

            foreach (RouteTimeTableEntry entry in GeneralHelpers.GetAirportFlights(this.Airport,airport,true))
            {
                TimeSpan flightTime = MathHelpers.GetFlightTime(entry.getDepartureAirport().Profile.Coordinates, entry.Destination.Airport.Profile.Coordinates, entry.Airliner.Airliner.Type);

                lbDestinationArrivals.Items.Add(new DestinationFlight(entry.Airliner.Airliner.Airline, entry,MathHelpers.ConvertDateTimeToLoalTime(MathHelpers.ConvertEntryToDate(entry).Add(flightTime), tz)));
                
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
