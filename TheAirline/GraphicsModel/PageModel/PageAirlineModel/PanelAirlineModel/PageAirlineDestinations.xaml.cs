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
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.PageModel.PageAirportModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel;

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
            InitializeComponent();

            this.Airline = airline;

            StackPanel panelDestinations = new StackPanel();
            panelDestinations.Margin = new Thickness(0, 10, 50, 0);

            TextBlock txtDestinationsHeader = new TextBlock();
            txtDestinationsHeader.Uid = "1001";
            txtDestinationsHeader.Margin = new Thickness(0, 0, 0, 0);
            txtDestinationsHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtDestinationsHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtDestinationsHeader.FontWeight = FontWeights.Bold;
            txtDestinationsHeader.Text = Translator.GetInstance().GetString("PageAirlineDestinations", txtDestinationsHeader.Uid);

            panelDestinations.Children.Add(txtDestinationsHeader);

            ListBox lbDestinations = new ListBox();
            lbDestinations.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbDestinations.ItemTemplate = this.Resources["AirportItem"] as DataTemplate;
            // chs, 2011-10-11 changed the max height so all elements are visible
            lbDestinations.MaxHeight = GraphicsHelpers.GetContentHeight()-100;

            this.Airline.Airports.ForEach(a => lbDestinations.Items.Add(a));
        
            panelDestinations.Children.Add(lbDestinations);

            WrapPanel panelButtons = new WrapPanel();
            panelButtons.Margin = new Thickness(0, 5, 0, 0);
            panelButtons.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            panelDestinations.Children.Add(panelButtons);

            Button btnDestinations = new Button();
            btnDestinations.Uid = "1002";
            btnDestinations.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnDestinations.Width = Double.NaN;
            btnDestinations.Height = Double.NaN;
            btnDestinations.Content = Translator.GetInstance().GetString("PageAirlineDestinations", btnDestinations.Uid);
            btnDestinations.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnDestinations.Click += new RoutedEventHandler(btnDestinations_Click);

            panelButtons.Children.Add(btnDestinations);

            Button btnMap = new Button();
            btnMap.Uid = "201";
            btnMap.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnMap.Width = Double.NaN;
            btnMap.Height = Double.NaN;
            btnMap.Content = Translator.GetInstance().GetString("PageAirlineDestinations", btnMap.Uid);
            btnMap.Margin = new Thickness(5, 0, 0, 0);
            btnMap.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnMap.Click += new RoutedEventHandler(btnMap_Click);

            panelButtons.Children.Add(btnMap);

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
