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
using System.Globalization;


namespace TheAirline.GraphicsModel.PageModel.PageAirportModel.PanelAirportModel
{
    /// <summary>
    /// Interaction logic for PageAirportStatistics.xaml
    /// </summary>
    public partial class PageAirportStatistics : Page
    {
        private Airport Airport;
        private StackPanel panelStats;
        public PageAirportStatistics(Airport airport)
        {

            InitializeComponent();

            this.Airport = airport;

            ScrollViewer svStatistics = new ScrollViewer();
            svStatistics.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            svStatistics.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            svStatistics.MaxHeight = GraphicsHelpers.GetContentHeight() - 50;

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

            panelStats = new StackPanel();

            panelStatistics.Children.Add(panelStats);

            //GameTimer.GetInstance().OnTimeChanged+=new GameTimer.TimeChanged(PageAirportStatistics_OnTimeChanged);

            //this.Unloaded += new RoutedEventHandler(PageAirportStatistics_Unloaded);

            svStatistics.Content = panelStatistics;

            this.Content = svStatistics;

            showStats();
        }

        private void PageAirportStatistics_Unloaded(object sender, RoutedEventArgs e)
        {
            GameTimer.GetInstance().OnTimeChanged -= new GameTimer.TimeChanged(PageAirportStatistics_OnTimeChanged);

        }

        //shows the stats
        private void showStats()
        {
            panelStats.Children.Clear();

            panelStats.Children.Add(createStatisticsPanel(StatisticsTypes.GetStatisticsType("Arrivals")));
           // panelStats.Children.Add(createStatisticsPanel(StatisticsTypes.GetStatisticsType("Departures")));
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

            ContentControl ccHeader = new ContentControl();
            ccHeader.ContentTemplate = this.Resources["AirportStatHeader"] as DataTemplate;
            ccHeader.Content = new KeyValuePair<string, KeyValuePair<int, int>>(string.Format(Translator.GetInstance().GetString("PageAirportStatistics", "1002"), type.Name), new KeyValuePair<int, int>(GameObject.GetInstance().GameTime.Year - 1, GameObject.GetInstance().GameTime.Year));
            panelStatistics.Children.Add(ccHeader);

            ListBox lbStatistics = new ListBox();
            lbStatistics.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbStatistics.ItemTemplate = this.Resources["AirportStatItem"] as DataTemplate;


            List<Airline> airlines = Airlines.GetAllAirlines();
            airlines.Sort((delegate(Airline a1, Airline a2) { return a1.Profile.Name.CompareTo(a2.Profile.Name); }));

            foreach (Airline airline in airlines)
                lbStatistics.Items.Add(new KeyValuePair<Airline, KeyValuePair<Airport, StatisticsType>>(airline, new KeyValuePair<Airport, StatisticsType>(this.Airport, type)));

            panelStatistics.Children.Add(lbStatistics);

            ContentControl ccTotal = new ContentControl();
            ccTotal.Margin = new Thickness(5, 0, 0, 0);
            ccTotal.ContentTemplate = this.Resources["AirportStatTotalItem"] as DataTemplate;
            ccTotal.Content = new KeyValuePair<Airport, StatisticsType>(this.Airport, type);

            panelStatistics.Children.Add(ccTotal);

            return panelStatistics;
        }


    }
    //the converter for a stat for a airport with the total value
    public class AirportTotalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            KeyValuePair<Airport, StatisticsType> astat = (KeyValuePair<Airport, StatisticsType>)value;

            int year = Int16.Parse(parameter.ToString());

            double lastYearValue = astat.Key.Statistics.getTotalValue(GameObject.GetInstance().GameTime.Year - 1, astat.Value);
            double currentYearValue = astat.Key.Statistics.getTotalValue(GameObject.GetInstance().GameTime.Year, astat.Value);


            if (year == 0)
                return currentYearValue;
            else if (year == -1)
                return lastYearValue;
            else
            {
                if (lastYearValue == 0)
                    return "100.00 %";
                double changePercent = System.Convert.ToDouble(currentYearValue - lastYearValue) / lastYearValue;

                if (double.IsInfinity(changePercent))
                    return "100.00 %";
                if (double.IsNaN(changePercent))
                    return "-";

                return string.Format("{0:0.00} %", changePercent * 100);
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //the converter for the stats for an airline at an airport
    public class AirlineStatConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            KeyValuePair<Airline, KeyValuePair<Airport, StatisticsType>> aa = (KeyValuePair<Airline, KeyValuePair<Airport, StatisticsType>>)value;

            int year = Int16.Parse(parameter.ToString());

            if (year == 0 || year == -1)
            {
                int currentYear = GameObject.GetInstance().GameTime.Year + year;
                return string.Format("{0}", aa.Value.Key.Statistics.getStatisticsValue(currentYear, aa.Key, aa.Value.Value));
            }
            else
            {
                double currentYearValue = aa.Value.Key.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, aa.Key, aa.Value.Value);
                double totalValue = aa.Value.Key.Statistics.getTotalValue(GameObject.GetInstance().GameTime.Year, aa.Value.Value);

                if (totalValue == 0)
                    return "-";

                double changePercent = System.Convert.ToDouble(currentYearValue) / System.Convert.ToDouble(totalValue); 

                if (double.IsInfinity(changePercent))
                    return "100.00 %";
                if (double.IsNaN(changePercent))
                    return "-";

                return string.Format("{0:0.00} %", changePercent * 100);
            }


        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
