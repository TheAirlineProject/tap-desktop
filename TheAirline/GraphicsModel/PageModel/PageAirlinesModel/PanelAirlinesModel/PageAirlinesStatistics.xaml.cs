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
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.StatisticsModel;
using TheAirline.Model.AirlineModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.PageModel.PageAirlineModel;
using System.ComponentModel;
using System.Threading;

namespace TheAirline.GraphicsModel.PageModel.PageAirlinesModel.PanelAirlinesModel
{
    /// <summary>
    /// Interaction logic for PageAirlinesStatistics.xaml
    /// </summary>
    public partial class PageAirlinesStatistics : Page
    {
        private StackPanel panelStats;
        private List<ListBox> lbToUpdate;
        public PageAirlinesStatistics()
        {
            
            InitializeComponent();

            ScrollViewer scroller = new ScrollViewer();
            scroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            scroller.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scroller.MaxHeight = GraphicsHelpers.GetContentHeight() - 50;
            scroller.Margin = new Thickness(0, 10, 50, 0);

            StackPanel panelStatistics = new StackPanel();
            panelStatistics.Orientation = Orientation.Vertical;

            TextBlock txtHeader = new TextBlock();
            txtHeader.Margin = new Thickness(0, 0, 0, 0);
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PanelAirlinesStatistics", "1001");

            panelStatistics.Children.Add(txtHeader);

            panelStats = new StackPanel();


            panelStatistics.Children.Add(panelStats);

       
            scroller.Content = panelStatistics;

            this.Content = scroller;
            this.Unloaded+=new RoutedEventHandler(PageAirlinesStatistics_Unloaded);

            GameTimer.GetInstance().OnTimeChanged+=new GameTimer.TimeChanged(PageAirlinesStatistics_OnTimeChanged);

            createStats();
        }

        private void PageAirlinesStatistics_Unloaded(object sender, RoutedEventArgs e)
        {
            GameTimer.GetInstance().OnTimeChanged -= new GameTimer.TimeChanged(PageAirlinesStatistics_OnTimeChanged);

         
        }

      

        //creates the stats panel
        private void createStats()
        {
            lbToUpdate = new List<ListBox>();

            panelStats.Children.Clear();

            panelStats.Children.Add(createStatisticsPanel(StatisticsTypes.GetStatisticsType("Departures"), false));
            panelStats.Children.Add(createStatisticsPanel(StatisticsTypes.GetStatisticsType("Passengers"), false));
            panelStats.Children.Add(createStatisticsPanel(StatisticsTypes.GetStatisticsType("Passengers%"), false));
            panelStats.Children.Add(createStatisticsPanel(StatisticsTypes.GetStatisticsType("On-Time%"), true));


        }
        //shows the stats
        private void showStats()
        {
            foreach (ListBox lbStat in lbToUpdate)
            {
                refreshListBox(lbStat);
            }

        }
        private void refreshListBox(ListBox listBox)
        {
            Action act = () =>
            {
                ICollectionView dataView =
                     CollectionViewSource.GetDefaultView(listBox.ItemsSource);



                dataView.Refresh();
            };

            listBox.Dispatcher.BeginInvoke(act);
        }
        private void PageAirlinesStatistics_OnTimeChanged()
        {
            if (this.IsLoaded)
                showStats();

        }
        private void LnkAirline_Click(object sender, RoutedEventArgs e)
        {
            Airline airline = (Airline)((Hyperlink)sender).Tag;

            PageNavigator.NavigateTo(new PageAirline(airline));


        }

        //creates the airlines statistics
        private StackPanel createStatisticsPanel(StatisticsType type, Boolean inPercent)
        {
            StackPanel panelStatistics = new StackPanel();
            panelStatistics.Margin = new Thickness(0, 0, 0, 5);

            ContentControl ccHeader = new ContentControl();
            ccHeader.ContentTemplate = this.Resources["StatHeader"] as DataTemplate;
            ccHeader.Content = new KeyValuePair<StatisticsType, KeyValuePair<int, int>>(type, new KeyValuePair<int, int>(GameObject.GetInstance().GameTime.Year - 1, GameObject.GetInstance().GameTime.Year));
            panelStatistics.Children.Add(ccHeader);

            ListBox lbStats = new ListBox();
            lbStats.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbStats.ItemTemplate = inPercent ? this.Resources["StatPercentItem"] as DataTemplate : this.Resources["StatItem"] as DataTemplate;

            List<Airline> airlines = Airlines.GetAllAirlines();
            airlines.Sort((delegate(Airline a1, Airline a2) { return a1.Profile.Name.CompareTo(a2.Profile.Name); }));

            List<KeyValuePair<Airline, StatisticsType>> values = new List<KeyValuePair<Airline, StatisticsType>>();

            airlines.ForEach(a => values.Add(new KeyValuePair<Airline, StatisticsType>(a, type)));

            lbStats.ItemsSource = values;

            panelStatistics.Children.Add(lbStats);

            lbToUpdate.Add(lbStats);

            /*
            if (!inPercent)
            {
                ContentControl ccTotal = new ContentControl();
                ccTotal.Margin = new Thickness(5, 0, 0, 0);
                ccTotal.ContentTemplate = this.Resources["StatTotalItem"] as DataTemplate;
                ccTotal.Content = type;

                panelStatistics.Children.Add(ccTotal);
            }
             * */

            return panelStatistics;

        }

    }
    //the converter for a stat with the total value
    public class TotalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            StatisticsType stat = (StatisticsType)value;

            int year = Int16.Parse(parameter.ToString());



            double lastYearValue = Airlines.GetAllAirlines().Sum(a => a.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year - 1, stat));
            double currentYearValue = Airlines.GetAllAirlines().Sum(a => a.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, stat));


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
            DateTime timestart = DateTime.Now;
            KeyValuePair<Airline, StatisticsType> aa = (KeyValuePair<Airline, StatisticsType>)value;

            int year = Int16.Parse(parameter.ToString());
            
            if (year == 0 || year == -1)
            {
                int currentYear = GameObject.GetInstance().GameTime.Year + year;
                return string.Format("{0}", aa.Key.Statistics.getStatisticsValue(currentYear, aa.Value));
            }
            else
            {
                double currentYearValue = aa.Key.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, aa.Value);
                double totalValue = Airlines.GetAllAirlines().Sum(a => a.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, aa.Value));

                if (totalValue == 0)
                    return "-";

                double changePercent = System.Convert.ToDouble(currentYearValue) / System.Convert.ToDouble(totalValue);

                if (double.IsInfinity(changePercent))
                    return "100.00 %";
                if (double.IsNaN(changePercent))
                    return "-";

                Console.WriteLine(DateTime.Now.Subtract(timestart).TotalMilliseconds);

                return string.Format("{0:0.00} %", changePercent * 100);
            }


        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
