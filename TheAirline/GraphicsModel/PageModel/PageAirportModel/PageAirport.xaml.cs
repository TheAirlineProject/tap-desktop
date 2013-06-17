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
using TheAirline.Model.GeneralModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.GraphicsModel.Converters;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.PageModel.PageAirportModel.PanelAirportModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.PassengerModel;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.Model.GeneralModel.StatisticsModel;
using TheAirline.GraphicsModel.UserControlModel;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;

namespace TheAirline.GraphicsModel.PageModel.PageAirportModel
{
    /// <summary>
    /// Interaction logic for PageAirport.xaml
    /// </summary>
    public partial class PageAirport : StandardPage
    {
        public Airport Airport { get; set; }
        private TextBlock txtLocalTime;
        private ListBox lbArrivals, lbDepartures, lbPassengers, lbFlights;
        private Boolean domesticDemand;
        public PageAirport(Airport airport)
        {
            InitializeComponent();

            this.Uid = "1000";
            this.Title = Translator.GetInstance().GetString("PageAirport", this.Uid);

            this.Airport = airport;

            StackPanel airportPanel = new StackPanel();
            airportPanel.Margin = new Thickness(10, 0, 10, 0);

            airportPanel.Children.Add(createQuickInfoPanel());
            airportPanel.Children.Add(createPassengersPanel());
            //airportPanel.Children.Add(createFlightsPanel());
            //airportPanel.Children.Add(createArrivalsPanel());
            //airportPanel.Children.Add(createDeparturesPanel());

            StandardContentPanel panelContent = new StandardContentPanel();

            panelContent.setContentPage(airportPanel, StandardContentPanel.ContentLocation.Left);

            StackPanel panelSideMenu = new PanelAirport(this.Airport);

            panelContent.setContentPage(panelSideMenu, StandardContentPanel.ContentLocation.Right);

            base.setContent(panelContent);

            base.setHeaderContent(this.Title + " - " + this.Airport.Profile.Name);

            showPage(this);

          
            GameTimer.GetInstance().OnTimeChanged += new GameTimer.TimeChanged(PageAirport_OnTimeChanged);

            this.Unloaded += new RoutedEventHandler(PageAirport_Unloaded);
        }

        private void PageAirport_Unloaded(object sender, RoutedEventArgs e)
        {
            GameTimer.GetInstance().OnTimeChanged -= new GameTimer.TimeChanged(PageAirport_OnTimeChanged);
        }

        private void PageAirport_OnTimeChanged()
        {
            if (this.IsLoaded)
            {

                GameTimeZone tz = this.Airport.Profile.TimeZone;

                txtLocalTime.Text = string.Format("{0} {1}", MathHelpers.ConvertDateTimeToLoalTime(GameObject.GetInstance().GameTime, tz).ToShortTimeString(), tz.ShortName);


            }
        }

        //creates the panel for flights
        private StackPanel createFlightsPanel()
        {
            StackPanel panelFlights = new StackPanel();
            panelFlights.Margin = new Thickness(0, 10, 0, 0);

            ContentControl ccHeader = new ContentControl();
            ccHeader.ContentTemplate = this.Resources["FlightHeader"] as DataTemplate;
            panelFlights.Children.Add(ccHeader);

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

            lbFlights = new ListBox();
            lbFlights.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbFlights.ItemTemplate = this.Resources["FlightItem"] as DataTemplate;
            lbFlights.MaxHeight = (GraphicsHelpers.GetContentHeight() - 100) / 4;
            panelFlights.Children.Add(lbFlights);

            showFlights(true);


            return panelFlights;

        }
        //creates the panel for arrivals
        private ScrollViewer createArrivalsPanel()
        {
            ScrollViewer svArrivals = new ScrollViewer();
            svArrivals.Margin = new Thickness(0, 10, 0, 0);
            svArrivals.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            svArrivals.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            svArrivals.MaxHeight = GraphicsHelpers.GetContentHeight() / 3;

            StackPanel panelArrivals = new StackPanel();
            panelArrivals.Margin = new Thickness(0, 10, 0, 0);

            Grid grdType = UICreator.CreateGrid(2);
            grdType.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            panelArrivals.Children.Add(grdType);

            Image imgLogo = new Image();
            imgLogo.Source = new BitmapImage(new Uri(@"/Data/images/Arrivals.png", UriKind.RelativeOrAbsolute));
            imgLogo.Height = 20;
            RenderOptions.SetBitmapScalingMode(imgLogo, BitmapScalingMode.HighQuality);

            Grid.SetColumn(imgLogo, 0);
            grdType.Children.Add(imgLogo);

            TextBlock txtType = UICreator.CreateTextBlock(Translator.GetInstance().GetString("PageAirport", "1002"));
            txtType.FontStyle = FontStyles.Oblique;
            txtType.FontSize = 16;

            Grid.SetColumn(txtType, 1);
            grdType.Children.Add(txtType);

            ContentControl txtHeader = new ContentControl();
            txtHeader.Uid = "1003";
            txtHeader.ContentTemplate = this.Resources["FlightHeader"] as DataTemplate;
            txtHeader.Content = Translator.GetInstance().GetString("PageAirport", txtHeader.Uid);
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

            panelArrivals.Children.Add(txtHeader);

            lbArrivals = new ListBox();
            lbArrivals.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbArrivals.ItemTemplate = this.Resources["FlightItem"] as DataTemplate;

            panelArrivals.Children.Add(lbArrivals);

            svArrivals.Content = panelArrivals;

            return svArrivals;
        }
        //creates the panel for the passengers
        private StackPanel createPassengersPanel()
        {
            StackPanel panelPassengers = new StackPanel();
            panelPassengers.Margin = new Thickness(0, 10, 0, 0);

            TextBlock txtHeader = new TextBlock();
            txtHeader.Uid = "1020";
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush");
            txtHeader.TextAlignment = TextAlignment.Left;
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PageAirport", txtHeader.Uid);
            panelPassengers.Children.Add(txtHeader);

            WrapPanel panelButtons = new WrapPanel();

            ucSelectButton sbDomestic = new ucSelectButton();
            sbDomestic.Uid = "1025";
            sbDomestic.Content = Translator.GetInstance().GetString("PageAirport", sbDomestic.Uid);
            sbDomestic.IsSelected = true;
            sbDomestic.Click += sbDomestic_Click;
            panelButtons.Children.Add(sbDomestic);

            ucSelectButton sbInternational = new ucSelectButton();
            sbInternational.Uid = "1026";
            sbInternational.Content = Translator.GetInstance().GetString("PageAirport", sbInternational.Uid);
            sbInternational.Click += sbInternational_Click;
            panelButtons.Children.Add(sbInternational);

            panelPassengers.Children.Add(panelButtons);

            lbPassengers = new ListBox();
            lbPassengers.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbPassengers.ItemTemplate = this.Resources["DemandItem"] as DataTemplate;
            lbPassengers.MaxHeight = (GraphicsHelpers.GetContentHeight() - 100) / 3;
            panelPassengers.Children.Add(lbPassengers);

            domesticDemand = true;

            showDemand();

            return panelPassengers;


        }
        //shows the passengers
        private void showDemand()
        {
            lbPassengers.Items.Clear();
            List<Airport> airports;
            if (domesticDemand)
                airports = Airports.GetAirports(a => a != this.Airport && a.Profile.Country == this.Airport.Profile.Country).OrderByDescending(a => this.Airport.getDestinationPassengersRate(a, AirlinerClass.ClassType.Economy_Class)).ToList();
            else
                airports = Airports.GetAirports(a => a != this.Airport && a.Profile.Country != this.Airport.Profile.Country).OrderByDescending(a => this.Airport.getDestinationPassengersRate(a, AirlinerClass.ClassType.Economy_Class)).ToList();
            
            foreach (Airport airport in airports)
            {
                int passengers = this.Airport.getDestinationPassengersRate(airport,AirlinerClass.ClassType.Economy_Class);
                int cargo = this.Airport.getDestinationCargoRate(airport);

                if (passengers > 0 || cargo > 0)
                {
                    DestinationDemand passengerDemand = new DestinationDemand(airport, (ushort)passengers);
                    DestinationDemand cargoDemand = new DestinationDemand(airport, (ushort)cargo);

                     
                    int demand = passengers;
                    int covered = 0;
                    
                    List<Route> routes = AirportHelpers.GetAirportRoutes(airport, this.Airport).Where(r => r.Type == Route.RouteType.Mixed || r.Type == Route.RouteType.Passenger).ToList();
                   
                    foreach (Route route in routes)
                    {
                        RouteAirlinerClass raClass = ((PassengerRoute)route).getRouteAirlinerClass(AirlinerClass.ClassType.Economy_Class);
                        double avgPassengers = route.Statistics.getStatisticsValue(raClass, StatisticsTypes.GetStatisticsType("Passengers%"));
                        double flights = route.TimeTable.Entries.Count;
                        double routeCovered = avgPassengers / (7.0 / flights);
                        covered += (int)routeCovered;
                    }
                    KeyValuePair<DestinationDemand, int> paxDemand = new KeyValuePair<DestinationDemand, int>(passengerDemand, Math.Max(0, demand - covered));
                    KeyValuePair<DestinationDemand, int> cDemand = new KeyValuePair<DestinationDemand, int>(cargoDemand, 0);
                    KeyValuePair<KeyValuePair<DestinationDemand, int>, KeyValuePair<DestinationDemand, int>> totalDemand = new KeyValuePair<KeyValuePair<DestinationDemand, int>, KeyValuePair<DestinationDemand, int>>(paxDemand, cDemand);
                    lbPassengers.Items.Add(totalDemand);
                }
            } 


        }
        //creates the panel for departures
        private ScrollViewer createDeparturesPanel()
        {
            ScrollViewer svDepartures = new ScrollViewer();
            svDepartures.Margin = new Thickness(0, 10, 0, 0);
            svDepartures.MaxHeight = GraphicsHelpers.GetContentHeight() / 6;
            svDepartures.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            svDepartures.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;

            StackPanel panelDepartures = new StackPanel();

            Grid grdType = UICreator.CreateGrid(2);
            grdType.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            panelDepartures.Children.Add(grdType);

            Image imgLogo = new Image();
            imgLogo.Source = new BitmapImage(new Uri(@"/Data/images/Departures.png", UriKind.RelativeOrAbsolute));
            imgLogo.Height = 20;
            RenderOptions.SetBitmapScalingMode(imgLogo, BitmapScalingMode.HighQuality);

            Grid.SetColumn(imgLogo, 0);
            grdType.Children.Add(imgLogo);

            TextBlock txtType = UICreator.CreateTextBlock(Translator.GetInstance().GetString("PageAirport", "1004"));
            txtType.FontSize = 16;
            txtType.FontStyle = FontStyles.Oblique;

            Grid.SetColumn(txtType, 1);
            grdType.Children.Add(txtType);

            ContentControl txtHeader = new ContentControl();
            txtHeader.Uid = "1005";
            txtHeader.ContentTemplate = this.Resources["FlightHeader"] as DataTemplate;
            txtHeader.Content = Translator.GetInstance().GetString("PageAirport", txtHeader.Uid);
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

            panelDepartures.Children.Add(txtHeader);

            lbDepartures = new ListBox();
            lbDepartures.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbDepartures.ItemTemplate = this.Resources["FlightItem"] as DataTemplate;

            panelDepartures.Children.Add(lbDepartures);

            svDepartures.Content = panelDepartures;

            return svDepartures;


        }

        //shows the departures or arrivals
        private void showFlights(Boolean arrivals)
        {
            lbFlights.Items.Clear();

            var entries = arrivals ?   GeneralHelpers.GetAirportArrivals(this.Airport, GameObject.GetInstance().GameTime.DayOfWeek) : GeneralHelpers.GetAirportDepartures(this.Airport,GameObject.GetInstance().GameTime.DayOfWeek);
           
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
            /*
                        GameTimeZone tz = this.Airport.Profile.TimeZone;

                        lbArrivals.Items.Clear();
                        lbDepartures.Items.Clear();
         
            
            
                        foreach (RouteTimeTableEntry entry in GeneralHelpers.GetAirportDepartures(this.Airport, 2))
                        {
                            if (entry.Airliner.CurrentFlight != null && entry.Airliner.CurrentFlight.Entry == entry)
                                lbDepartures.Items.Add(new AirportFlightItem(MathHelpers.ConvertDateTimeToLoalTime(MathHelpers.ConvertEntryToDate(entry), tz), entry.Airliner.Airliner.Airline, entry.Destination.Airport, entry.Destination.FlightCode, "Planned"));
                            else
                                lbDepartures.Items.Add(new AirportFlightItem(MathHelpers.ConvertDateTimeToLoalTime(MathHelpers.ConvertEntryToDate(entry), tz), entry.Airliner.Airliner.Airline, entry.Destination.Airport, entry.Destination.FlightCode, "Planned"));
                        }

                        foreach (RouteTimeTableEntry entry in GeneralHelpers.GetAirportArrivals(this.Airport, 2))
                        {
                            TimeSpan flightTime = MathHelpers.GetFlightTime(entry.getDepartureAirport().Profile.Coordinates, entry.Destination.Airport.Profile.Coordinates, entry.Airliner.Airliner.Type);

                            if (entry.Airliner.CurrentFlight != null && entry == entry.Airliner.CurrentFlight.Entry && entry.Airliner.Status == FleetAirliner.AirlinerStatus.On_route)
                                lbArrivals.Items.Add(new AirportFlightItem(MathHelpers.ConvertDateTimeToLoalTime(MathHelpers.ConvertEntryToDate(entry).Add(flightTime), tz), entry.Airliner.Airliner.Airline, entry.Destination.Airport == entry.TimeTable.Route.Destination1 ? entry.TimeTable.Route.Destination2 : entry.TimeTable.Route.Destination1, entry.Destination.FlightCode, string.Format("{0:HH:mm}", entry.Airliner.CurrentFlight.getExpectedLandingTime())));
                            else
                                lbArrivals.Items.Add(new AirportFlightItem(MathHelpers.ConvertDateTimeToLoalTime(MathHelpers.ConvertEntryToDate(entry).Add(flightTime), tz), entry.Airliner.Airliner.Airline, entry.Destination.Airport == entry.TimeTable.Route.Destination1 ? entry.TimeTable.Route.Destination2 : entry.TimeTable.Route.Destination1, entry.Destination.FlightCode, string.Format("{0:HH:mm}", "Planned")));
                        }   */
        }

        //creates the quick info panel for the airport
        private ScrollViewer createQuickInfoPanel()
        {
            ScrollViewer scroller = new ScrollViewer();
            scroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            scroller.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scroller.MaxHeight = GraphicsHelpers.GetContentHeight() / 2;

            StackPanel panelInfo = new StackPanel();
            scroller.Content = panelInfo;

            TextBlock txtHeader = new TextBlock();
            txtHeader.Uid = "1006";
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush");
            txtHeader.TextAlignment = TextAlignment.Left;
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PageAirport", txtHeader.Uid);

            panelInfo.Children.Add(txtHeader);

            DockPanel grdQuickInfo = new DockPanel();
            grdQuickInfo.Margin = new Thickness(0, 5, 0, 0);

            panelInfo.Children.Add(grdQuickInfo);

            Image imgAirport = new Image();
            imgAirport.Source = this.Airport.Profile.Logo.Length > 0 ? new BitmapImage(new Uri(this.Airport.Profile.Logo, UriKind.RelativeOrAbsolute)) : new BitmapImage(new Uri(@"/Data/images/airport.png", UriKind.Relative));
            imgAirport.Width = 110;
            imgAirport.Margin = new Thickness(0, 0, 5, 0);
            imgAirport.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            RenderOptions.SetBitmapScalingMode(imgAirport, BitmapScalingMode.HighQuality);
            grdQuickInfo.Children.Add(imgAirport);

            StackPanel panelQuickInfo = new StackPanel();

            grdQuickInfo.Children.Add(panelQuickInfo);

            ListBox lbQuickInfo = new ListBox();
            lbQuickInfo.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbQuickInfo.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            panelQuickInfo.Children.Add(lbQuickInfo);

            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirport", "1007"), UICreator.CreateTextBlock(this.Airport.Profile.Name)));

            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirport", "1008"), UICreator.CreateTextBlock(new AirportCodeConverter().Convert(this.Airport).ToString())));
            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirport", "1009"), UICreator.CreateTextBlock(new TextUnderscoreConverter().Convert(this.Airport.Profile.Type).ToString())));


            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirport", "1010"), UICreator.CreateTownPanel(this.Airport.Profile.Town)));



            ContentControl lblFlag = new ContentControl();
            lblFlag.SetResourceReference(ContentControl.ContentTemplateProperty, "CountryFlagLongItem");
            lblFlag.Content = new CountryCurrentCountryConverter().Convert(this.Airport.Profile.Country);//this.Airport.Profile.Country is TemporaryCountry ? ((TemporaryCountry)this.Airport.Profile.Country).getCurrentCountry(GameObject.GetInstance().GameTime) : this.Airport.Profile.Country;

            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirport", "1011"), lblFlag));

            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirport", "1022"), UICreator.CreateTextBlock(this.Airport.Profile.Period.From.ToShortDateString())));

            if (GameObject.GetInstance().GameTime.AddDays(14) > this.Airport.Profile.Period.To)
            {

                TextBlock txtClosingDate = UICreator.CreateTextBlock(this.Airport.Profile.Period.To.ToShortDateString());
                txtClosingDate.Foreground = Brushes.DarkRed;

                lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirport", "1024"), txtClosingDate));
            }

            GameTimeZone tz = this.Airport.Profile.TimeZone;

            TextBlock txtTimeZone = UICreator.CreateTextBlock(tz.DisplayName);
            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirport", "1012"), txtTimeZone));

            txtLocalTime = UICreator.CreateTextBlock(string.Format("{0} {1}", MathHelpers.ConvertDateTimeToLoalTime(GameObject.GetInstance().GameTime, tz).ToShortTimeString(), tz.ShortName));

            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirport", "1013"), txtLocalTime));
            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirport", "1019"), UICreator.CreateTextBlock(new TextUnderscoreConverter().Convert(this.Airport.Profile.Season).ToString())));

            WrapPanel panelCoordinates = new WrapPanel();

            Image imgMap = new Image();
            imgMap.Source = new BitmapImage(new Uri(@"/Data/images/map.png", UriKind.RelativeOrAbsolute));
            imgMap.Height = 16;
            imgMap.MouseDown += new MouseButtonEventHandler(imgMap_MouseDown);
            RenderOptions.SetBitmapScalingMode(imgMap, BitmapScalingMode.HighQuality);

            imgMap.Margin = new Thickness(0, 0, 5, 0);
            panelCoordinates.Children.Add(imgMap);

            TextBlock txtCoordinates = UICreator.CreateLink(this.Airport.Profile.Coordinates.ToString());
            ((Hyperlink)txtCoordinates.Inlines.FirstInline).Click += new RoutedEventHandler(PageAirport_Click);
            txtCoordinates.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            panelCoordinates.Children.Add(txtCoordinates);

            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirport", "1014"), panelCoordinates));
            //  lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirport", "1015"), UICreator.CreateTextBlock(new TextUnderscoreConverter().Convert(this.Airport.Profile.Size).ToString())));
            //   lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirport", "1021"), UICreator.CreateTextBlock(new TextUnderscoreConverter().Convert(this.Airport.Profile.Cargo).ToString())));

            WrapPanel panelSize = new WrapPanel();

            Image imgPassenger = new Image();
            imgPassenger.Source = new BitmapImage(new Uri(@"/Data/images/passenger.png", UriKind.RelativeOrAbsolute));
            imgPassenger.Height = 16;
            RenderOptions.SetBitmapScalingMode(imgPassenger, BitmapScalingMode.HighQuality);

            panelSize.Children.Add(imgPassenger);

            TextBlock txtPassengerSize = UICreator.CreateTextBlock(new TextUnderscoreConverter().Convert(this.Airport.Profile.Size).ToString());
            txtPassengerSize.Margin = new Thickness(2, 0, 0, 0);
            txtPassengerSize.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

            panelSize.Children.Add(txtPassengerSize);

            Image imgCargo = new Image();
            imgCargo.Margin = new Thickness(10, 0, 0, 0);
            imgCargo.Source = new BitmapImage(new Uri(@"/Data/images/cargo.png", UriKind.RelativeOrAbsolute));
            imgCargo.Height = 16;
            RenderOptions.SetBitmapScalingMode(imgCargo, BitmapScalingMode.HighQuality);

            panelSize.Children.Add(imgCargo);

            TextBlock txtCargoSize = UICreator.CreateTextBlock(new TextUnderscoreConverter().Convert(this.Airport.Profile.Cargo).ToString());
            txtCargoSize.Margin = new Thickness(2, 0, 0, 0);
            txtCargoSize.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

            panelSize.Children.Add(txtCargoSize);

            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirport", "1015"), panelSize));

            TextBlock txtAirportIncome = UICreator.CreateTextBlock(new ValueCurrencyConverter().Convert(this.Airport.Income).ToString());
            txtAirportIncome.Foreground = new ValueIsMinusConverter().Convert(this.Airport.Income) as Brush;

            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirport", "1023"), txtAirportIncome));

            WrapPanel panelTerminals = new WrapPanel();

            Image imgMapOverview = new Image();
            imgMapOverview.Source = new BitmapImage(new Uri(@"/Data/images/info.png", UriKind.RelativeOrAbsolute));
            imgMapOverview.Height = 16;
            imgMapOverview.MouseDown += new MouseButtonEventHandler(imgMapOverview_MouseDown);
            RenderOptions.SetBitmapScalingMode(imgMapOverview, BitmapScalingMode.HighQuality);

            imgMapOverview.Margin = new Thickness(5, 0, 0, 0);
            imgMapOverview.Visibility = this.Airport.Profile.Map != null ? Visibility.Visible : System.Windows.Visibility.Collapsed;

            panelTerminals.Children.Add(UICreator.CreateTextBlock(string.Format("{0} ({1} {2})", this.Airport.Terminals.getNumberOfGates(), this.Airport.Terminals.getNumberOfAirportTerminals(), Translator.GetInstance().GetString("PageAirport", "1018"))));

            panelTerminals.Children.Add(imgMapOverview);

            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirport", "1016"), panelTerminals));

            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirport", "1017"), UICreator.CreateTextBlock(this.Airport.Runways.Count.ToString())));

            return scroller;
        }
        private void sbDepartures_Click(object sender, RoutedEventArgs e)
        {
            showFlights(false);
        }
        private void sbArrivals_Click(object sender, RoutedEventArgs e)
        {
            showFlights(true);
        }
        private void sbInternational_Click(object sender, RoutedEventArgs e)
        {
            domesticDemand = false;

            showDemand();
        }

        private void sbDomestic_Click(object sender, RoutedEventArgs e)
        {
            domesticDemand = true;

            showDemand();
        }
        private void imgMapOverview_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PopUpAirportMap.ShowPopUp(this.Airport);
        }
       

        private void PageAirport_Click(object sender, RoutedEventArgs e)
        {
            PopUpMap.ShowPopUp(this.Airport);
        }

        private void imgMap_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PopUpMap.ShowPopUp(this.Airport);
        }
        private void btnRent_Click(object sender, RoutedEventArgs e)
        {
            KeyValuePair<DestinationDemand, int> v = (KeyValuePair<DestinationDemand, int>)((Button)sender).Tag;
            Airport airport = v.Key.Destination;

            Boolean hasCheckin = airport.getAirportFacility(GameObject.GetInstance().HumanAirline, AirportFacility.FacilityType.CheckIn).TypeLevel > 0;

            object o = PopUpAirportContract.ShowPopUp(airport);

            //WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2222"), string.Format(Translator.GetInstance().GetString("MessageBox", "2222", "message"), airport.Profile.Name), WPFMessageBoxButtons.YesNo);

            if (o != null)
            {
                if (!hasCheckin)
                {
                    AirportFacility checkinFacility = AirportFacilities.GetFacilities(AirportFacility.FacilityType.CheckIn).Find(f => f.TypeLevel == 1);

                    airport.addAirportFacility(GameObject.GetInstance().HumanAirline, checkinFacility, GameObject.GetInstance().GameTime);
                    AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -checkinFacility.Price);

                }

                airport.addAirlineContract((AirportContract)o);

                showDemand();
            }
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


    }
    //the converter for renting a gate    
    public class RentingGateVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility rv = Visibility.Collapsed;
            try
            {
                KeyValuePair<DestinationDemand, int> v = (KeyValuePair<DestinationDemand, int>)value;
                Airport airport = v.Key.Destination;

                Boolean isEnabled = airport.Terminals.getFreeGates() > 0;// && airport.AirlineContract == null;


                rv = (Visibility)new BooleanToVisibilityConverter().Convert(isEnabled, null, null, null);

            }
            catch (Exception)
            {

            }
            return rv;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsHumanAirportConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DestinationDemand destination = (DestinationDemand)value;


            if (GameObject.GetInstance().HumanAirline.Airports.Contains(destination.Destination))
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


}
