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

namespace TheAirline.GraphicsModel.PageModel.PageAirportModel
{
    /// <summary>
    /// Interaction logic for PageAirport.xaml
    /// </summary>
    public partial class PageAirport : StandardPage
    {
        public Airport Airport { get; set; }
        private TextBlock txtWind, txtLocalTime;
        private ListBox lbArrivals, lbDepartures, lbPassengers;
        public PageAirport(Airport airport)
        {
            InitializeComponent();

            this.Uid = "1000";
            this.Title = Translator.GetInstance().GetString("PageAirport", this.Uid);

            this.Airport = airport;

            StackPanel airportPanel = new StackPanel();
            airportPanel.Margin = new Thickness(10, 0, 10, 0);

            airportPanel.Children.Add(createQuickInfoPanel());
            airportPanel.Children.Add(createWeatherPanel());
            airportPanel.Children.Add(createPassengersPanel());
            //airportPanel.Children.Add(createArrivalsPanel());
            //airportPanel.Children.Add(createDeparturesPanel());

            //showFlights();

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
                txtWind.Text = string.Format("{0} ({1:0.##} {2} in {3} direction)", new Converters.TextUnderscoreConverter().Convert(this.Airport.Weather.WindSpeed, null, null, null), new NumberToUnitConverter().Convert((int)this.Airport.Weather.WindSpeed), new StringToLanguageConverter().Convert("km/t"), this.Airport.Weather.Direction);

       
                GameTimeZone tz = this.Airport.Profile.TimeZone;

                txtLocalTime.Text = string.Format("{0} {1}", MathHelpers.ConvertDateTimeToLoalTime(GameObject.GetInstance().GameTime, tz).ToShortTimeString(), tz.ShortName);

         
            }
        }

        //creates the panel for the weather
        private Panel createWeatherPanel()
        {
            StackPanel panelWeather = new StackPanel();
            panelWeather.Margin = new Thickness(0, 10, 0, 0);

            TextBlock txtHeader = new TextBlock();
            txtHeader.Uid = "1001";
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush");
            txtHeader.TextAlignment = TextAlignment.Left;
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PageAirport", txtHeader.Uid);

            panelWeather.Children.Add(txtHeader);

            WrapPanel panelWind = new WrapPanel();
            panelWeather.Children.Add(panelWind);

            Image imgWind = new Image();
            imgWind.Source = new BitmapImage(new Uri(@"/Data/images/wind.png", UriKind.RelativeOrAbsolute));
            imgWind.Height = 24;
            imgWind.Width = 24;
            RenderOptions.SetBitmapScalingMode(imgWind, BitmapScalingMode.HighQuality);

            panelWind.Children.Add(imgWind);

            txtWind = UICreator.CreateTextBlock(string.Format("{0} ({1:0.##} {2} in {3} direction)", new Converters.TextUnderscoreConverter().Convert(this.Airport.Weather.WindSpeed, null, null, null), new NumberToUnitConverter().Convert((int)this.Airport.Weather.WindSpeed), new StringToLanguageConverter().Convert("km/t"), this.Airport.Weather.Direction));//string.Format("{0} ({1} km/h) in {2} direction", new Converters.TextUnderscoreConverter().Convert(this.Airport.Weather.WindSpeed, null, null, null), (int)this.Airport.Weather.WindSpeed, this.Airport.Weather.Direction));
            txtWind.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            txtWind.Margin = new Thickness(10, 0, 0, 0);

            panelWind.Children.Add(txtWind);

            return panelWeather;
        }

        //creates the panel for arrivals
        private ScrollViewer createArrivalsPanel()
        {
            ScrollViewer svArrivals = new ScrollViewer();
            svArrivals.Margin = new Thickness(0, 10, 0, 0);
            svArrivals.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            svArrivals.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            svArrivals.MaxHeight = GraphicsHelpers.GetContentHeight() / 6;

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

            lbPassengers = new ListBox();
            lbPassengers.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbPassengers.ItemTemplate = this.Resources["PassengersItem"] as DataTemplate;
            lbPassengers.MaxHeight = GraphicsHelpers.GetContentHeight() / 4;
            panelPassengers.Children.Add(lbPassengers);

            showPassengers();

            return panelPassengers;





        }
        //shows the passengers
        private void showPassengers()
        {
            lbPassengers.Items.Clear();

            var airports = Airports.GetAirports(a => a != this.Airport).OrderByDescending(a=>this.Airport.getDestinationPassengersRate(a,AirlinerClass.ClassType.Economy_Class));
      
            foreach (Airport airport in airports)
            {
                DestinationPassengers passengers = this.Airport.getDestinationPassengersObject(airport);

                if (passengers == null)
                    lbPassengers.Items.Add(new DestinationPassengers(airport, GeneralHelpers.Rate.None));
                else
                    lbPassengers.Items.Add(passengers);
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

        //shows the departures and arrivals
        private void showFlights()
        {
            lbArrivals.Items.Clear();
            lbDepartures.Items.Clear();

            GameTimeZone tz = this.Airport.Profile.TimeZone;

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
            }
        }

        //creates the quick info panel for the airport
        private Panel createQuickInfoPanel()
        {
            StackPanel panelInfo = new StackPanel();

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
    

            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirport", "1010"),createTownPanel()));
    
            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirport", "1022"), UICreator.CreateTextBlock(this.Airport.Profile.Period.From.ToShortDateString())));

            if (GameObject.GetInstance().GameTime.AddDays(14) > this.Airport.Profile.Period.To)
            {
            
                TextBlock txtClosingDate = UICreator.CreateTextBlock(this.Airport.Profile.Period.To.ToShortDateString());
                txtClosingDate.Foreground = Brushes.DarkRed;

                lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirport", "1024"),txtClosingDate ));
            }

            ContentControl lblFlag = new ContentControl();
            lblFlag.SetResourceReference(ContentControl.ContentTemplateProperty, "CountryFlagLongItem");
            lblFlag.Content = new CountryCurrentCountryConverter().Convert(this.Airport.Profile.Country);//this.Airport.Profile.Country is TemporaryCountry ? ((TemporaryCountry)this.Airport.Profile.Country).getCurrentCountry(GameObject.GetInstance().GameTime) : this.Airport.Profile.Country;

            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirport", "1011"), lblFlag));

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
            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirport", "1015"), UICreator.CreateTextBlock(new TextUnderscoreConverter().Convert(this.Airport.Profile.Size).ToString())));
            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirport","1021"),UICreator.CreateTextBlock(new TextUnderscoreConverter().Convert(this.Airport.Profile.Cargo).ToString())));
            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirport","1023"),UICreator.CreateTextBlock(string.Format("{0:C}", this.Airport.Income))));

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
            return panelInfo;
        }
        //creates the town panel
        private WrapPanel createTownPanel()
        {
            WrapPanel townPanel = new WrapPanel();

            townPanel.Children.Add(UICreator.CreateTextBlock(this.Airport.Profile.Town.Name));

            if (this.Airport.Profile.Town.State != null)
            {
                townPanel.Children.Add(UICreator.CreateTextBlock(string.Format(", {0}", this.Airport.Profile.Town.State.ShortName)));

                if (this.Airport.Profile.Town.State.Flag != null)
                {
                    Image imgFlag = new Image();
                    imgFlag.Source = new BitmapImage(new Uri(this.Airport.Profile.Town.State.Flag, UriKind.RelativeOrAbsolute));
                    imgFlag.Height = 16;
                    imgFlag.MouseDown += new MouseButtonEventHandler(imgMap_MouseDown);
                    imgFlag.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
                    RenderOptions.SetBitmapScalingMode(imgFlag, BitmapScalingMode.HighQuality);

                    imgFlag.Margin = new Thickness(5, 0, 0, 0);

                    townPanel.Children.Add(imgFlag);
                }

            }
            return townPanel;
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

        //the class for a flight at the airport
        private class AirportFlightItem
        {
            public Airline Airline { get; set; }
            public string Flight { get; set; }
            public DateTime Time { get; set; }
            public Airport Airport { get; set; }
            public string Status { get; set; }
            public AirportFlightItem(DateTime time, Airline airline, Airport airport, string flight, string status)
            {
                this.Time = time;
                this.Airport = airport;
                this.Airline = airline;
                this.Flight = flight;
                this.Status = status;
            }
        }
      

    }
   
}
