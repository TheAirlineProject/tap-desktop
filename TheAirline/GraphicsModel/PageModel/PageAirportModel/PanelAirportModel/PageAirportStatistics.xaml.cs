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
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.StatisticsModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.PageModel.PageAirlineModel;

namespace TheAirline.GraphicsModel.PageModel.PageAirportModel.PanelAirportModel
{
    /// <summary>
    /// Interaction logic for PageAirportStatistics.xaml
    /// </summary>
    public partial class PageAirportStatistics : Page
    {
        private Airport Airport;
        //private TextBlock txtPassengers, txtDepartures, txtPerFlight;
        private StackPanel panelStats;
        public PageAirportStatistics(Airport airport)
        {
            InitializeComponent();

            this.Airport = airport;

            StackPanel panelStatistics = new StackPanel();
            panelStatistics.Margin = new Thickness(0, 10, 50, 0);

            TextBlock txtHeader = new TextBlock();
            txtHeader.Uid = "1001";
            txtHeader.Margin = new Thickness(0, 0, 0, 0);
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PageAirportStatistics", txtHeader.Uid);

            panelStatistics.Children.Add(txtHeader);

            /*
            ListBox lbStatistics = new ListBox();
            lbStatistics.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            //lbStatistics.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");
            lbStatistics.ItemTemplate = this.Resources["AirlineStatItem"] as DataTemplate;

            Random rnd = new Random();

            foreach (Airline airline in Airlines.GetAirlines())
                lbStatistics.Items.Add(new AirlineStatisticsItem(airline, rnd.Next(100) + 1));
            /*

            txtDepartures = UICreator.CreateTextBlock(this.Airliner.Statistics.TotalDepartures.ToString());
            lbStatistics.Items.Add(new QuickInfoValue("Total departures", txtDepartures));

            txtPassengers = UICreator.CreateTextBlock(this.Airliner.Statistics.TotalPassengers.ToString());
            lbStatistics.Items.Add(new QuickInfoValue("Total passengers", txtPassengers));

            txtPerFlight = UICreator.CreateTextBlock(this.Airliner.Statistics.getPassengersPerFlight().ToString());
            lbStatistics.Items.Add(new QuickInfoValue("Passengers per flight", txtPerFlight));
            */
            //panelStatistics.Children.Add(lbStatistics);

            panelStats = new StackPanel();

            panelStatistics.Children.Add(panelStats);

            GameTimer.GetInstance().OnTimeChanged += new GameTimer.TimeChanged(PageAirportStatistics_OnTimeChanged);

            this.Content = panelStatistics;

            showStats();
        }

        //shows the stats
        private void showStats()
        {
            panelStats.Children.Clear();

            panelStats.Children.Add(createStatisticsPanel(StatisticsTypes.GetStatisticsType("Arrivals")));
            panelStats.Children.Add(createStatisticsPanel(StatisticsTypes.GetStatisticsType("Departures")));
            panelStats.Children.Add(createStatisticsPanel(StatisticsTypes.GetStatisticsType("Passengers")));
            panelStats.Children.Add(createStatisticsPanel(StatisticsTypes.GetStatisticsType("Passengers%")));
        }

        private void PageAirportStatistics_OnTimeChanged()
        {
            if (this.IsLoaded)
             showStats();
        }

        private void LnkAirline_Click(object sender, RoutedEventArgs e)
        {
            Airline airline = (Airline)((Hyperlink)sender).Tag;

            PageNavigator.NavigateTo(new PageAirline(airline));
        }

        //´creates the airport statistics
        private StackPanel createStatisticsPanel(StatisticsType type)
        {
            StackPanel panelStatistics = new StackPanel();
            panelStatistics.Margin = new Thickness(0, 0, 0, 5);

            TextBlock txtHeader = new TextBlock();
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = string.Format(Translator.GetInstance().GetString("PageAirportStatistics", "1002"), type.Name, this.Airport.Statistics.getTotalValue(type));

            panelStatistics.Children.Add(txtHeader);

            ListBox lbStatistics = new ListBox();
            lbStatistics.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            //lbStatistics.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");
            lbStatistics.ItemTemplate = this.Resources["AirlineStatItem"] as DataTemplate;

            double maxValue = getMaxValue(type);

            double coff = 100 / maxValue;

            List<Airline> airlines = Airlines.GetAirlines();
            airlines.Sort((delegate(Airline a1, Airline a2) { return a1.Profile.Name.CompareTo(a2.Profile.Name); }));

            foreach (Airline airline in airlines)
                lbStatistics.Items.Add(new AirlineStatisticsItem(airline,this.Airport.Statistics.getStatisticsValue(airline,type),Math.Max(1, (int)Convert.ToDouble(this.Airport.Statistics.getStatisticsValue(airline,type)*coff))));

            panelStatistics.Children.Add(lbStatistics);

            return panelStatistics;
        }

        //finds the maximum value for a statistics type
        private double getMaxValue(StatisticsType type)
        {
            double value = 1;
            foreach (Airline airline in Airlines.GetAirlines())
            {
                int aValue = this.Airport.Statistics.getStatisticsValue(airline, type);
                if (aValue > value)
                    value = aValue;
            }

            return value;
        }

        private class AirlineStatisticsItem
        {
            public Airline Airline { get; set; }
            public int Value { get; set; }
            public int Width { get; set; }
            public AirlineStatisticsItem(Airline airline, int value, int Width)
            {
                this.Airline = airline;
                this.Value = value;
                this.Width = Width;
            }
        }
    }
}
