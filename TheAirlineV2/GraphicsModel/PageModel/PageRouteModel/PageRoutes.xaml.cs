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
using TheAirlineV2.GraphicsModel.PageModel.GeneralModel;
using TheAirlineV2.Model.AirportModel;
using TheAirlineV2.GraphicsModel.PageModel.PageAirportModel;
using TheAirlineV2.Model.AirlinerModel.RouteModel;
using TheAirlineV2.Model.GeneralModel;
using TheAirlineV2.GraphicsModel.PageModel.PageRouteModel.PanelRoutesModel;
using TheAirlineV2.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirlineV2.GraphicsModel.UserControlModel.MessageBoxModel;
using System.ComponentModel;

namespace TheAirlineV2.GraphicsModel.PageModel.PageRouteModel
{
    /// <summary>
    /// Interaction logic for PageRoute.xaml
    /// </summary>
    public partial class PageRoutes : StandardPage
    {
        private StackPanel panelSideMenu;
        private ListBox lbRoutes;
        private ListSortDirection sortDirection = ListSortDirection.Ascending;
        public PageRoutes()
        {

            InitializeComponent();

            StackPanel routesPanel = new StackPanel();
            routesPanel.Margin = new Thickness(10, 0, 10, 0);


            TextBlock txtHeader = new TextBlock();
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = "Airline Routes";
            routesPanel.Children.Add(txtHeader);

            ContentControl txtRoutesHeader = new ContentControl();
            txtRoutesHeader.ContentTemplate = this.Resources["RoutesHeader"] as DataTemplate;
            txtRoutesHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            routesPanel.Children.Add(txtRoutesHeader);

            routesPanel.Children.Add(createRoutesPanel());
            routesPanel.Children.Add(createButtonsPanel());


            StandardContentPanel panelContent = new StandardContentPanel();

            panelContent.setContentPage(routesPanel, StandardContentPanel.ContentLocation.Left);


            panelSideMenu = new StackPanel();//new PanelNewRoute();

            panelContent.setContentPage(panelSideMenu, StandardContentPanel.ContentLocation.Right);



            base.setContent(panelContent);

            base.setHeaderContent("Routes");

            //showRoutes();

            showPage(this);
        }
        //creates the buttons panel
        private WrapPanel createButtonsPanel()
        {
            WrapPanel buttonsPanel = new WrapPanel();
            buttonsPanel.Margin = new Thickness(0, 5, 0, 0);

            Button btnCreate = new Button();
            btnCreate.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnCreate.Height = 20;
            btnCreate.Width = 150;
            btnCreate.Content = "Create new route";
            btnCreate.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnCreate.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnCreate.Click += new RoutedEventHandler(btnCreate_Click);

            buttonsPanel.Children.Add(btnCreate);

            Button btnMap = new Button();
            btnMap.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnMap.Width = 100;
            btnMap.Height = 20;
            btnMap.Content = "Route map";
            btnMap.Margin = new Thickness(2, 0, 0, 0);
          //  btnMap.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
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
            /*
            lbRoutes.Items.Clear();

            foreach (Route route in GameObject.GetInstance().HumanAirline.Routes)
               lbRoutes.Items.Add(route);
            */
            ICollectionView dataView =
            CollectionViewSource.GetDefaultView(lbRoutes.ItemsSource);
            dataView.Refresh();
        }
        //creates the panel for the routes
        private StackPanel createRoutesPanel()
        {
            StackPanel routesPanel = new StackPanel();

            lbRoutes = new ListBox();
            lbRoutes.MaxHeight = GraphicsHelpers.GetContentHeight() - 75;
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
        private void ButtonTimetable_Click(object sender, RoutedEventArgs e)
        {
            Route route = (Route)((Button)sender).Tag;

            RouteTimeTable timeTable = (RouteTimeTable)PopUpTimeTable.ShowPopUp(GameObject.GetInstance().HumanAirline, route);
            
            if (timeTable != null)
            {
                route.TimeTable = timeTable;
            }



        }


    }
}
