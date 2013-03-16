using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.Converters;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.PageModel.PageRouteModel.PanelRoutesModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.Model.GeneralModel.StatisticsModel;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;

namespace TheAirline.GraphicsModel.PageModel.PageFleetAirlinerModel.PanelFleetAirlinerModel
{
    /// <summary>
    /// Interaction logic for PageFleetRoute.xaml
    /// </summary>
    public partial class PageFleetRoute : Page
    {
        private FleetAirliner Airliner;
        //private TextBlock txtStatus, txtDestination, txtPosition, txtPassengers, txtFlightTime;
        private Button btnStopFlight, btnStartFlight;
        public PageFleetRoute(FleetAirliner airliner)
        {
            InitializeComponent();

            this.Airliner = airliner;

            InitializeComponent();

            StackPanel panelRoute = new StackPanel();
            panelRoute.Margin = new Thickness(0, 10, 50, 0);

            TextBlock txtHeader = new TextBlock();
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PageFleetRoute","1000");



            if (this.Airliner.HasRoute)
            {
               panelRoute.Children.Add(createRoutesInfo());
               panelRoute.Children.Add(createFlightInfo());

                if (this.Airliner.Airliner.Airline == GameObject.GetInstance().HumanAirline) panelRoute.Children.Add(createFlightButtons());
            }
            else panelRoute.Children.Add(txtHeader);
         
            this.Content = panelRoute;

       
   
        }
       
        
        //creates the buttons for the flight
        private WrapPanel createFlightButtons()
        {
            WrapPanel buttonsPanel = new WrapPanel();
            buttonsPanel.Margin = new Thickness(0, 5, 0, 0);
            buttonsPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            btnStartFlight = new Button();
            btnStartFlight.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnStartFlight.Height = Double.NaN;
            btnStartFlight.Width = Double.NaN;
            btnStartFlight.Content = Translator.GetInstance().GetString("PageFleetRoute","200");
            btnStartFlight.IsEnabled = this.Airliner.Status == FleetAirliner.AirlinerStatus.Stopped;
            btnStartFlight.Click += new RoutedEventHandler(btnStartFligth_Click);
            btnStartFlight.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");

            buttonsPanel.Children.Add(btnStartFlight);

            btnStopFlight = new Button();
            btnStopFlight.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnStopFlight.Width = Double.NaN;
            btnStopFlight.Height = Double.NaN;
            btnStopFlight.Content = Translator.GetInstance().GetString("PageFleetRoute", "201");
            btnStopFlight.IsEnabled = this.Airliner.Status != FleetAirliner.AirlinerStatus.Stopped;
            btnStopFlight.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnStopFlight.Margin = new Thickness(5, 0, 0, 0);
            btnStopFlight.Click += new RoutedEventHandler(btnStopFlight_Click);
            buttonsPanel.Children.Add(btnStopFlight);

            return buttonsPanel;


        }

        private void btnStopFlight_Click(object sender, RoutedEventArgs e)
        {
            if (GameObject.GetInstance().DayRoundEnabled)
                this.Airliner.Status = FleetAirliner.AirlinerStatus.Stopped;
            else
                this.Airliner.Status = FleetAirliner.AirlinerStatus.To_homebase;

            btnStartFlight.IsEnabled = true;

            btnStopFlight.IsEnabled = false;
        }

        private void btnStartFligth_Click(object sender, RoutedEventArgs e)
        {
            if (this.Airliner.Pilots.Count == this.Airliner.Airliner.Type.CockpitCrew)
            {

                if (GameObject.GetInstance().DayRoundEnabled)
                    this.Airliner.Status = FleetAirliner.AirlinerStatus.On_route;
                else
                    this.Airliner.Status = FleetAirliner.AirlinerStatus.To_route_start;

                btnStartFlight.IsEnabled = false;

                btnStopFlight.IsEnabled = true;
            }
            else
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2507"), string.Format(Translator.GetInstance().GetString("MessageBox", "2507", "message")), WPFMessageBoxButtons.Ok);
       
        }
        
        //creates the flight details
        private StackPanel createFlightInfo()
        {

            StackPanel panelFlight = new StackPanel();
            panelFlight.Margin = new Thickness(0, 5, 0, 0);

            TextBlock txtHeader = new TextBlock();
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PageFleetRoute", "1001");
            panelFlight.Children.Add(txtHeader);


            ListBox lbFlightInfo = new ListBox();
            lbFlightInfo.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbFlightInfo.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            string status = "Resting";
            
            if (this.Airliner.Status == FleetAirliner.AirlinerStatus.Stopped) status = "Stopped";
            if (this.Airliner.Status == FleetAirliner.AirlinerStatus.On_route || this.Airliner.Status == FleetAirliner.AirlinerStatus.To_route_start) status = "Active";

            TextBlock txtStatus = UICreator.CreateTextBlock(status);
            TextBlock txtFlightsPerDay = UICreator.CreateTextBlock(this.Airliner.Routes.Sum(r=>r.TimeTable.Entries.FindAll(e=>e.Day == GameObject.GetInstance().GameTime.DayOfWeek).Count).ToString());
            TextBlock txtPassengers = UICreator.CreateTextBlock(string.Format("{0:0.00}",this.Airliner.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Passengers")) / this.Airliner.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Arrivals"))));

            lbFlightInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageFleetRoute","1002"), txtStatus));
            lbFlightInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageFleetRoute", "1003"), txtFlightsPerDay));
            lbFlightInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageFleetRoute", "1004"), txtPassengers));

            panelFlight.Children.Add(lbFlightInfo);

            return panelFlight;
        }
      
        //creates the routes details
        private StackPanel createRoutesInfo()
        {

           
            StackPanel panelRoutesInfo = new StackPanel();

            TextBlock txtHeader = new TextBlock();
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = "Routes Information";
            panelRoutesInfo.Children.Add(txtHeader);

            ScrollViewer svRoutes = new ScrollViewer();
            svRoutes.MaxHeight = GraphicsHelpers.GetContentHeight()/2;
            svRoutes.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            svRoutes.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            panelRoutesInfo.Children.Add(svRoutes);

            StackPanel panelRoutes = new StackPanel();
            svRoutes.Content = panelRoutes;

            foreach (Route route in this.Airliner.Routes)
            {
                ListBox lbRouteInfo = new ListBox();
                lbRouteInfo.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
                lbRouteInfo.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");
                lbRouteInfo.Margin = new Thickness(0, 0, 0, 5);
                panelRoutes.Children.Add(lbRouteInfo);


                double distance = MathHelpers.GetDistance(route.Destination1.Profile.Coordinates, route.Destination2.Profile.Coordinates);
                
                lbRouteInfo.Items.Add(new QuickInfoValue("Route", UICreator.CreateTextBlock(string.Format("{0} <-> {1}",route.Destination1.Profile.Name,route.Destination2.Profile.Name))));
                // chs, 2011-10-10 added missing conversion
                lbRouteInfo.Items.Add(new QuickInfoValue("Distance", UICreator.CreateTextBlock(string.Format("{0:0} {1}", new NumberToUnitConverter().Convert(distance), new StringToLanguageConverter().Convert("km.")))));

                if (route.Type == Route.RouteType.Passenger || route.Type == Route.RouteType.Mixed)
                {
                    foreach (AirlinerClass aClass in this.Airliner.Airliner.Classes)
                    {
                        RouteAirlinerClass rClass = ((PassengerRoute)route).getRouteAirlinerClass(aClass.Type);

                        Image imgInfo = new Image();
                        imgInfo.Width = 16;
                        imgInfo.Source = new BitmapImage(new Uri(@"/Data/images/info.png", UriKind.RelativeOrAbsolute));
                        imgInfo.Margin = new Thickness(5, 0, 0, 0);
                        RenderOptions.SetBitmapScalingMode(imgInfo, BitmapScalingMode.HighQuality);

                        Border brdToolTip = new Border();
                        brdToolTip.Margin = new Thickness(-4, 0, -4, -3);
                        brdToolTip.Padding = new Thickness(5);
                        brdToolTip.SetResourceReference(Border.BackgroundProperty, "HeaderBackgroundBrush2");


                        ContentControl lblClass = new ContentControl();
                        lblClass.SetResourceReference(ContentControl.ContentTemplateProperty, "RouteAirlinerClassItem");
                        lblClass.Content = rClass;

                        brdToolTip.Child = lblClass;


                        imgInfo.ToolTip = brdToolTip;


                        lbRouteInfo.Items.Add(new QuickInfoValue(new TextUnderscoreConverter().Convert(aClass.Type, null, null, null).ToString(), imgInfo));
                    }
                }
            }
            WrapPanel buttonsPanel = new WrapPanel();
            buttonsPanel.Margin = new Thickness(0, 5, 0, 0);

            Button btnTimeTable = new Button();
            btnTimeTable.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnTimeTable.Height = Double.NaN;
            btnTimeTable.Width = Double.NaN;
            btnTimeTable.Content = "Timetable";
            btnTimeTable.Visibility = Visibility.Visible;// Visibility.Collapsed;//this.Airliner.Airliner.Airline.IsHuman ? Visibility.Collapsed : Visibility.Visible;
            btnTimeTable.Click += new RoutedEventHandler(btnTimeTable_Click);
            btnTimeTable.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnTimeTable.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");

            buttonsPanel.Children.Add(btnTimeTable);

            Button btnMap = new Button();
            btnMap.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnMap.Width = Double.NaN;
            btnMap.Height = Double.NaN;
            btnMap.Content = "Routes map";
            btnMap.Margin = new Thickness(2, 0, 0, 0);
            btnMap.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnMap.Click += new RoutedEventHandler(btnMap_Click);

            buttonsPanel.Children.Add(btnMap);

            panelRoutesInfo.Children.Add(buttonsPanel);

            return panelRoutesInfo;

        }

        private void btnMap_Click(object sender, RoutedEventArgs e)
        {
            PopUpMap.ShowPopUp(this.Airliner.Routes);
        }

        private void btnTimeTable_Click(object sender, RoutedEventArgs e)
        {
            PopUpAirlinerRoutes.ShowPopUp(this.Airliner,false);
        }
        
    }
}
