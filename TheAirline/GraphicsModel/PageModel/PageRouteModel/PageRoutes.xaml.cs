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

            this.Uid = "1000";
            this.Title = Translator.GetInstance().GetString("PageRoutes", this.Uid);

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
            lbFleet.MaxHeight = GraphicsHelpers.GetContentHeight() / 4;
            lbFleet.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbFleet.ItemTemplate = this.Resources["FleetItem"] as DataTemplate;
         
            panelFleet.Children.Add(lbFleet);

        



            return panelFleet;
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
        public void showRoutes()
        {
           
            ICollectionView dataView =
            CollectionViewSource.GetDefaultView(lbRoutes.ItemsSource);
            dataView.Refresh();
        }
        //shows the fleet
        private void showFleet()
        {
            lbFleet.Items.Clear();

            GameObject.GetInstance().HumanAirline.DeliveredFleet.ForEach(f => lbFleet.Items.Add(f));
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

        private void lnkAirline_Click(object sender, RoutedEventArgs e)
        {
            panelSideMenu.Children.Clear();

            FleetAirliner airliner = (FleetAirliner)((Hyperlink)sender).Tag;

            PopUpAirlinerRoutes.ShowPopUp(airliner,true);

            showFleet();
        }

       
    }
}
