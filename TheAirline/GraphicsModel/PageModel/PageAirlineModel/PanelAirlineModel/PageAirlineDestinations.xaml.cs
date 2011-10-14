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
using TheAirline.Model.AirlineModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.Model.AirportModel;
using TheAirline.GraphicsModel.PageModel.PageAirportModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;

namespace TheAirline.GraphicsModel.PageModel.PageAirlineModel.PanelAirlineModel
{
    /// <summary>
    /// Interaction logic for PageAirlineDestinations.xaml
    /// </summary>
    public partial class PageAirlineDestinations : Page
    {
        private Airline Airline;
        public PageAirlineDestinations(Airline airline)
        {
            this.Airline = airline;

            InitializeComponent();

            StackPanel panelDestinations = new StackPanel();
            panelDestinations.Margin = new Thickness(0, 10, 50, 0);

            TextBlock txtDestinationsHeader = new TextBlock();
            txtDestinationsHeader.Margin = new Thickness(0, 0, 0, 0);
            txtDestinationsHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtDestinationsHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtDestinationsHeader.FontWeight = FontWeights.Bold;
            txtDestinationsHeader.Text = "Destinations";

            panelDestinations.Children.Add(txtDestinationsHeader);

            ListBox lbDestinations = new ListBox();
            lbDestinations.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbDestinations.ItemTemplate = this.Resources["AirportItem"] as DataTemplate;
            // chs, 2011-10-11 changed the max height so all elements are visible
            lbDestinations.MaxHeight = GraphicsHelpers.GetContentHeight()-100;

            foreach (Airport airport in this.Airline.Airports)
                 lbDestinations.Items.Add(airport);

            panelDestinations.Children.Add(lbDestinations);

            WrapPanel panelButtons = new WrapPanel();
            panelButtons.Margin = new Thickness(0, 5, 0, 0);
            panelButtons.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            panelDestinations.Children.Add(panelButtons);

            Button btnDestinations = new Button();
            btnDestinations.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnDestinations.Width = 100;
            btnDestinations.Height = 20;
            btnDestinations.Content = "Destinations";
            btnDestinations.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnDestinations.Click += new RoutedEventHandler(btnDestinations_Click);

            panelButtons.Children.Add(btnDestinations);

            Button btnMap = new Button();
            btnMap.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnMap.Width = 100;
            btnMap.Height = 20;
            btnMap.Content = "Route map";
            btnMap.Margin = new Thickness(5, 0, 0, 0);
           // btnMap.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnMap.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnMap.Click += new RoutedEventHandler(btnMap_Click);

            panelButtons.Children.Add(btnMap);

          
            /*
            TextBlock txtRoutesHeader = new TextBlock();
            txtRoutesHeader.Margin = new Thickness(0, 10, 0, 0);
            txtRoutesHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtRoutesHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtRoutesHeader.FontWeight = FontWeights.Bold;
            txtRoutesHeader.Text = "Airline Routes";

            panelDestinations.Children.Add(txtRoutesHeader);

            ListBox lbRoutes = new ListBox();
            lbRoutes.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbRoutes.ItemTemplate = this.Resources["RouteItem"] as DataTemplate;

            foreach (Route route in this.Airline.Routes)
                    lbRoutes.Items.Add(route);
            
            panelDestinations.Children.Add(lbRoutes);
            */

            this.Content = panelDestinations;
        }

        private void btnDestinations_Click(object sender, RoutedEventArgs e)
        {
            PopUpMap.ShowPopUp(this.Airline.Airports);
        }

        private void btnMap_Click(object sender, RoutedEventArgs e)
        {
            PopUpMap.ShowPopUp(this.Airline.Routes);
        }
        private void LnkAirport_Click(object sender, RoutedEventArgs e)
        {
            Airport airport = (Airport)((Hyperlink)sender).Tag;

            PageNavigator.NavigateTo(new PageAirport(airport));

      
        }
    }
}
