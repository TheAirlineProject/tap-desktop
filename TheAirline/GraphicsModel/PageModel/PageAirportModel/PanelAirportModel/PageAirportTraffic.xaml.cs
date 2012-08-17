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

            TextBlock txtHeader = new TextBlock();
            txtHeader.Uid = "1001";
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PageAirportTraffic", txtHeader.Uid);
            panelTraffic.Children.Add(txtHeader);

            ListBox lbDestinations = new ListBox();
            lbDestinations.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbDestinations.ItemTemplate = this.Resources["DestinationItem"] as DataTemplate;
            lbDestinations.MaxHeight = GraphicsHelpers.GetContentHeight() - 100;

            var destinations = from a in Airports.GetAllAirports() orderby this.Airport.getDestinationStatistics(a) descending select a;

            foreach (Airport a in destinations.Take(20))
                lbDestinations.Items.Add(new KeyValuePair<Airport,long>(a,this.Airport.getDestinationStatistics(a)));

            panelTraffic.Children.Add(lbDestinations);

            this.Content = panelTraffic;

  
        }
    }
}
