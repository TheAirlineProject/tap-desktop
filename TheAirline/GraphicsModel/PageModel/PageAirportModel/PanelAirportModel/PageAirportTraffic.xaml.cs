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
using TheAirline.GraphicsModel.PageModel.GeneralModel;

namespace TheAirline.GraphicsModel.PageModel.PageAirportModel.PanelAirportModel
{
    /// <summary>
    /// Interaction logic for PageAirportTraffic.xaml
    /// </summary>
    public partial class PageAirportTraffic : Page
    {
        private Airport Airport;
        public PageAirportTraffic(Airport airport)
        {
            this.Airport = airport;

            InitializeComponent();

            StackPanel panelTraffic = new StackPanel();
            panelTraffic.Margin = new Thickness(0, 10, 50, 0);

            TextBlock txtPassengerHeader = new TextBlock();
            txtPassengerHeader.Uid = "1001";
            txtPassengerHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtPassengerHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtPassengerHeader.FontWeight = FontWeights.Bold;
            txtPassengerHeader.Text = Translator.GetInstance().GetString("PageAirportTraffic", txtPassengerHeader.Uid);
            panelTraffic.Children.Add(txtPassengerHeader);

            ListBox lbPassengerDestinations = new ListBox();
            lbPassengerDestinations.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbPassengerDestinations.ItemTemplate = this.Resources["DestinationItem"] as DataTemplate;
            lbPassengerDestinations.MaxHeight = (GraphicsHelpers.GetContentHeight() - 100)/2;

            var destinations = from a in Airports.GetAllActiveAirports() orderby this.Airport.getDestinationPassengerStatistics(a) descending select a;

            foreach (Airport a in destinations.Take(20))
                lbPassengerDestinations.Items.Add(new KeyValuePair<Airport,long>(a,this.Airport.getDestinationPassengerStatistics(a)));

            panelTraffic.Children.Add(lbPassengerDestinations);

            TextBlock txtCargoHeader = new TextBlock();
            txtCargoHeader.Margin = new Thickness(0, 10, 0, 0);
            txtCargoHeader.Uid = "1002";
            txtCargoHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtCargoHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtCargoHeader.FontWeight = FontWeights.Bold;
            txtCargoHeader.Text = Translator.GetInstance().GetString("PageAirportTraffic", txtCargoHeader.Uid);
            panelTraffic.Children.Add(txtCargoHeader);

            ListBox lbCargoDestinations = new ListBox();
            lbCargoDestinations.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbCargoDestinations.ItemTemplate = this.Resources["DestinationItem"] as DataTemplate;
            lbCargoDestinations.MaxHeight = (GraphicsHelpers.GetContentHeight() - 100) / 2;

            var cargoDestinations = from a in Airports.GetAllActiveAirports() orderby this.Airport.getDestinationCargoStatistics(a) descending select a;

            foreach (Airport a in destinations.Take(20))
                lbCargoDestinations.Items.Add(new KeyValuePair<Airport, double>(a, this.Airport.getDestinationCargoStatistics(a)));

            panelTraffic.Children.Add(lbCargoDestinations);

            this.Content = panelTraffic;

  
        }
    }
}
