using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.PageModel.PageAirportModel;
using TheAirline.GraphicsModel.PageModel.PageRouteModel.PanelRoutesModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.PassengerModel;
using TheAirline.Model.GeneralModel.CountryModel;
using TheAirline.Model.PilotModel;
using TheAirline.GraphicsModel.Converters;

namespace TheAirline.GraphicsModel.PageModel.PageRouteModel
{
    /// <summary>
    /// Interaction logic for PageRoute.xaml
    /// </summary>
    public partial class PageRoutes : StandardPage
    {
        private StackPanel panelSideMenu;
        private ListBox lbRoutes, lbFleet;
        private ListSortDirection sortDirection = ListSortDirection.Ascending;
        public PageRoutes()
        {


            InitializeComponent();

            this.Uid = "1003";
            this.Title = string.Format(Translator.GetInstance().GetString("PageRoutes", this.Uid), GameObject.GetInstance().HumanAirline.Profile.Name);

            StackPanel routesPanel = new StackPanel();
            routesPanel.Margin = new Thickness(10, 0, 10, 0);


            TextBlock txtHeader = new TextBlock();
            txtHeader.Uid = "1001";
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PageRoutes", txtHeader.Uid);
            routesPanel.Children.Add(txtHeader);

            ContentControl txtRoutesHeader = new ContentControl();
            txtRoutesHeader.ContentTemplate = this.Resources["RoutesHeader"] as DataTemplate;
            txtRoutesHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            routesPanel.Children.Add(txtRoutesHeader);

            routesPanel.Children.Add(createRoutesPanel());
            routesPanel.Children.Add(createButtonsPanel());
            routesPanel.Children.Add(createFleetPanel());
            routesPanel.Children.Add(createRestrictionsPanel());

            StandardContentPanel panelContent = new StandardContentPanel();

            panelContent.setContentPage(routesPanel, StandardContentPanel.ContentLocation.Left);


            panelSideMenu = new StackPanel();

            panelContent.setContentPage(panelSideMenu, StandardContentPanel.ContentLocation.Right);



            base.setContent(panelContent);

            base.setHeaderContent(this.Title);

            showPage(this);

            showFleet();
        }
        //creates the fleet panel
        private StackPanel createFleetPanel()
        {
            StackPanel panelFleet = new StackPanel();
            panelFleet.Margin = new Thickness(0, 10, 0, 0);

            ContentControl txtFleetHeader = new ContentControl();
            txtFleetHeader.ContentTemplate = this.Resources["FleetHeader"] as DataTemplate;
            txtFleetHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            panelFleet.Children.Add(txtFleetHeader);

            lbFleet = new ListBox();
            lbFleet.MaxHeight = GraphicsHelpers.GetContentHeight() / 5;
            lbFleet.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbFleet.ItemTemplate = this.Resources["FleetItem"] as DataTemplate;

            panelFleet.Children.Add(lbFleet);

            return panelFleet;
        }
        //creates the panel for restrictions
        private StackPanel createRestrictionsPanel()
        {
            StackPanel panelRestrictions = new StackPanel();
            panelRestrictions.Margin = new Thickness(0, 10, 0, 0);

            TextBlock txtHeader = new TextBlock();
            txtHeader.Uid = "1002";
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PageRoutes", txtHeader.Uid);
            panelRestrictions.Children.Add(txtHeader);

            ListBox lbRestrictions = new ListBox();
            lbRestrictions.MaxHeight = GraphicsHelpers.GetContentHeight() / 5;
            lbRestrictions.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbRestrictions.ItemTemplate = this.Resources["RestrictionItem"] as DataTemplate;

            lbRestrictions.ItemsSource = FlightRestrictions.GetRestrictions().FindAll(r => r.StartDate < GameObject.GetInstance().GameTime && r.EndDate > GameObject.GetInstance().GameTime);

            panelRestrictions.Children.Add(lbRestrictions);

            return panelRestrictions;

        }
        //creates the panel for the routes
        private StackPanel createRoutesPanel()
        {
            StackPanel routesPanel = new StackPanel();

            lbRoutes = new ListBox();
            lbRoutes.MaxHeight = GraphicsHelpers.GetContentHeight() / 2;
            lbRoutes.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbRoutes.ItemTemplate = this.Resources["RouteItem"] as DataTemplate;
            lbRoutes.ItemsSource = GameObject.GetInstance().HumanAirline.Routes;

            routesPanel.Children.Add(lbRoutes);



            return routesPanel;
        }
        //creates the buttons panel
        private WrapPanel createButtonsPanel()
        {
            WrapPanel buttonsPanel = new WrapPanel();
            buttonsPanel.Margin = new Thickness(0, 5, 0, 0);

            Button btnCreate = new Button();
            btnCreate.Uid = "200";
            btnCreate.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnCreate.Height = Double.NaN;
            btnCreate.Width = Double.NaN;
            btnCreate.Content = Translator.GetInstance().GetString("PageRoutes", btnCreate.Uid);
            btnCreate.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnCreate.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnCreate.Click += new RoutedEventHandler(btnCreate_Click);

            buttonsPanel.Children.Add(btnCreate);

            Button btnMap = new Button();
            btnMap.Uid = "201";
            btnMap.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnMap.Width = Double.NaN;
            btnMap.Height = Double.NaN;
            btnMap.Content = Translator.GetInstance().GetString("PageRoutes", btnMap.Uid);
            btnMap.Margin = new Thickness(2, 0, 0, 0);
            btnMap.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnMap.Click += new RoutedEventHandler(btnMap_Click);

            buttonsPanel.Children.Add(btnMap);
            return buttonsPanel;
        }

        private void btnMap_Click(object sender, RoutedEventArgs e)
        {
            PopUpMap.ShowPopUp(GameObject.GetInstance().HumanAirline.Routes);
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {

            panelSideMenu.Children.Clear();

            panelSideMenu.Children.Add(new PanelNewRoute(this));
        }
        //shows the routes for the human airline
        private void showRoutes()
        {

            ICollectionView dataView =  CollectionViewSource.GetDefaultView(lbRoutes.ItemsSource);
            dataView.Refresh();
        }
        //shows the fleet
        private void showFleet()
        {
            lbFleet.Items.Clear();

            GameObject.GetInstance().HumanAirline.DeliveredFleet.ForEach(f => lbFleet.Items.Add(f));
        }

        private void Header_Click(object sender, RoutedEventArgs e)
        {
            string type = (string)((Hyperlink)sender).Tag;

            string name = "Destination1.Profile.IATACode";

            switch (type)
            {
                case "Dest1":
                    name = "Destination1.Profile.IATACode";
                    break;
                case "Dest2":
                    name = "Destination2.Profile.IATACode";
                    break;
                case "FlightCodes":
                    name = "FlightCodes";
                    break;
                case "Balance":
                    name = "Balance";
                    break;

            }

            sortDirection = sortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;

            ICollectionView dataView =
              CollectionViewSource.GetDefaultView(lbRoutes.ItemsSource);

            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(name, sortDirection);
            dataView.SortDescriptions.Add(sd);

        }
        private void LnkAirport_Click(object sender, RoutedEventArgs e)
        {
            Airport airport = (Airport)((Hyperlink)sender).Tag;

            PageNavigator.NavigateTo(new PageAirport(airport));


        }
        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            Route route = (Route)((Button)sender).Tag;

            panelSideMenu.Children.Clear();

            panelSideMenu.Children.Add(new PanelRoute(this, route));

        }
        private void ButtonMap_Click(object sender, RoutedEventArgs e)
        {
            Route route = (Route)((Button)sender).Tag;

            PopUpMap.ShowPopUp(route);
        }
        private void ButtonTimeTable_Click(object sender, RoutedEventArgs e)
        {
            Route route = (Route)((Button)sender).Tag;

            PopUpTimeTable.ShowPopUp(route);
        }
        private void btnStopRoute_Click(object sender, RoutedEventArgs e)
        {
             Route route = (Route)((Button)sender).Tag;

             foreach (FleetAirliner airliner in route.getAirliners())
                 airliner.Status = FleetAirliner.AirlinerStatus.Stopped;

             showFleet();

             showRoutes();
        }
        private void lnkAirline_Click(object sender, RoutedEventArgs e)
        {
            FleetAirliner airliner = (FleetAirliner)((Hyperlink)sender).Tag;

            panelSideMenu.Children.Clear();

        
            if (airliner.NumberOfPilots == airliner.Airliner.Type.CockpitCrew)
            {
             
                PopUpAirlinerAutoRoutes.ShowPopUp(airliner);
                showFleet();
            }
            else
            {
                int missingPilots = airliner.Airliner.Type.CockpitCrew - airliner.NumberOfPilots;
                if (GameObject.GetInstance().HumanAirline.Pilots.FindAll(p => p.Airliner == null).Count >= missingPilots)
                {
                    WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2505"), string.Format(Translator.GetInstance().GetString("MessageBox", "2505", "message")), WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                        var unassignedPilots = GameObject.GetInstance().HumanAirline.Pilots.FindAll(p => p.Airliner == null).ToList();

                        for (int i = 0; i < missingPilots; i++)
                        {
                            unassignedPilots[i].Airliner = airliner;
                            airliner.addPilot(unassignedPilots[i]);
                        }

                       // PopUpAirlinerRoutes.ShowPopUp(airliner, true);
                        PopUpAirlinerAutoRoutes.ShowPopUp(airliner);

                        showFleet();
                    }
                }
                else
                {
                    Random rnd = new Random();
                    WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2506"), string.Format(Translator.GetInstance().GetString("MessageBox", "2506", "message"), missingPilots), WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                        while (airliner.Airliner.Type.CockpitCrew > airliner.NumberOfPilots)
                        {
                            var pilots = Pilots.GetUnassignedPilots(p => p.Profile.Town.Country == airliner.Airliner.Airline.Profile.Country);

                            if (pilots.Count == 0)
                                pilots = Pilots.GetUnassignedPilots(p => p.Profile.Town.Country.Region == airliner.Airliner.Airline.Profile.Country.Region);

                            if (pilots.Count == 0)
                                pilots = Pilots.GetUnassignedPilots();

                            Pilot pilot = pilots.First();

                            airliner.Airliner.Airline.addPilot(pilot);
                            pilot.Airliner = airliner;
                            airliner.addPilot(pilot);
                        }

                        //PopUpAirlinerRoutes.ShowPopUp(airliner, true);
                        PopUpAirlinerAutoRoutes.ShowPopUp(airliner);

                        showFleet();
            

                    }
                }
            }
        }
        private void btnStopFlight_Click(object sender, RoutedEventArgs e)
        {
            FleetAirliner airliner = (FleetAirliner)((Button)sender).Tag;

            airliner.Status = FleetAirliner.AirlinerStatus.Stopped;

            showFleet();

            showRoutes();
           
        }
        private void btnStartFlight_Click(object sender, RoutedEventArgs e)
        {
            FleetAirliner airliner = (FleetAirliner)((Button)sender).Tag;

            if (airliner.Pilots.Count == airliner.Airliner.Type.CockpitCrew)
            {
                if (GameObject.GetInstance().DayRoundEnabled)
                    airliner.Status = FleetAirliner.AirlinerStatus.On_route;
                else
                    airliner.Status = FleetAirliner.AirlinerStatus.To_route_start;

                showFleet();

                showRoutes();
            }
            else
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2507"), string.Format(Translator.GetInstance().GetString("MessageBox", "2507", "message")), WPFMessageBoxButtons.Ok);


        }


    }
    //the converter for the possibility of stopping a route
    public class StopRouteVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Route route = (Route)value;

            if (route.getAirliners().Find(a=>a.Status != FleetAirliner.AirlinerStatus.Stopped) != null) 
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //the converter for the stop overs
    public class StopoverItemConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Route route = (Route)value;

            WrapPanel panelStopovers = new WrapPanel();

            foreach (StopoverRoute sRoute in route.Stopovers)
            {
                ContentControl ccStopover = new ContentControl();
                ccStopover.SetResourceReference(ContentControl.ContentTemplateProperty, "AirportIATACountryItem");
                ccStopover.Content = sRoute.Stopover;
                ccStopover.Margin = new Thickness(0, 0, 10, 0);

                panelStopovers.Children.Add(ccStopover);
            }

            return panelStopovers;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //the converter for an unions member
    public class UnionMemberConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            BaseUnit unit = (BaseUnit)value;

            if (unit is Union)
            {
                return string.Join("\r\n", from m in ((Union)unit).Members where GameObject.GetInstance().GameTime >= m.MemberFromDate && GameObject.GetInstance().GameTime <= m.MemberToDate select m.Country.Name);
            }
            else
                return unit.Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
   
    //the converter for possibility of starting a flight
    public class StartFlightBooleanToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            FleetAirliner airliner = (FleetAirliner)value;

            if (parameter.ToString() == "start")
            {
                if (airliner.HasRoute && airliner.Status == FleetAirliner.AirlinerStatus.Stopped)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }

            if (parameter.ToString() == "stop")
            {
                if (airliner.HasRoute && airliner.Status != FleetAirliner.AirlinerStatus.Stopped)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }

            return Visibility.Collapsed;

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
