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
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.StatisticsModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.Model.StatisticsModel;

namespace TheAirline.GraphicsModel.PageModel.PageAirlineModel.PanelAirlineModel
{
    /// <summary>
    /// Interaction logic for PageAirlineStatistics.xaml
    /// </summary>
    public partial class PageAirlineStatistics : Page
    {
        private Airline Airline;
        private ListBox lbStats, lbRatings;
        public PageAirlineStatistics(Airline airline)
        {
            InitializeComponent();

            this.Airline = airline;

            StackPanel panelStatistics = new StackPanel();
            panelStatistics.Margin = new Thickness(0, 10, 50, 0);

            TextBlock txtHeader = new TextBlock();
            txtHeader.Uid = "1001";
            txtHeader.Margin = new Thickness(0, 0, 0, 0);
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PageAirlineStatistics", txtHeader.Uid);

            panelStatistics.Children.Add(txtHeader);

          
            ContentControl ccHeader = new ContentControl();
            ccHeader.ContentTemplate = this.Resources["StatHeader"] as DataTemplate;
            ccHeader.Content = new KeyValuePair<int, int>(GameObject.GetInstance().GameTime.Year - 1, GameObject.GetInstance().GameTime.Year);

            panelStatistics.Children.Add(ccHeader);

            lbStats = new ListBox();
            lbStats.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbStats.ItemTemplate = this.Resources["StatItem"] as DataTemplate;

            panelStatistics.Children.Add(lbStats);

            if (this.Airline.IsHuman)
                panelStatistics.Children.Add(createHumanStatisticsPanel());

            TextBlock txtRatingsHeader = new TextBlock();
            txtRatingsHeader.Uid ="1018";
            txtRatingsHeader.Margin = new Thickness(0,10,0,0);
            txtRatingsHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtRatingsHeader.SetResourceReference(TextBlock.BackgroundProperty,"HeaderBackgroundBrush2");
            txtRatingsHeader.FontWeight = FontWeights.Bold;
            txtRatingsHeader.Text = Translator.GetInstance().GetString("PageAirlineStatistics",txtRatingsHeader.Uid);

            panelStatistics.Children.Add(txtRatingsHeader);

            lbRatings = new ListBox();
            lbRatings.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbRatings.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            panelStatistics.Children.Add(lbRatings);
          
            showStats();
            showRatings();

            this.Content = panelStatistics;
        }
        //creates the panel for the human statistics
        private StackPanel createHumanStatisticsPanel()
        {
            StackPanel panelHumanStatistics = new StackPanel();
            panelHumanStatistics.Margin = new Thickness(0, 10, 0, 0);

            TextBlock txtHeader = new TextBlock();
            txtHeader.Uid = "1005";
            txtHeader.Margin = new Thickness(0, 0, 0, 0);
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PageAirlineStatistics", txtHeader.Uid);

            panelHumanStatistics.Children.Add(txtHeader);

            ListBox lbHumanStats = new ListBox();
            lbHumanStats.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbHumanStats.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            panelHumanStatistics.Children.Add(lbHumanStats);

            lbHumanStats.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirlineStatistics", "1006"), UICreator.CreateTextBlock(string.Format("{0:0.00} %", StatisticsHelpers.GetHumanOnTime()))));
            lbHumanStats.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirlineStatistics", "1007"), UICreator.CreateTextBlock(string.Format("{0:0.00} %", StatisticsHelpers.GetHumanFillAverage()*100))));
            lbHumanStats.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirlineStatistics", "1008"), UICreator.CreateTextBlock(string.Format("{0:0.00} %", Ratings.GetCustomerHappiness(GameObject.GetInstance().HumanAirline)))));
            lbHumanStats.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirlineStatistics", "1009"), UICreator.CreateTextBlock(string.Format("{0:0.00}", StatisticsHelpers.GetHumanAvgTicketPPD()))));
            lbHumanStats.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirlineStatistics", "1012"), UICreator.CreateTextBlock(string.Format("{0:0.00} %", Ratings.GetEmployeeHappiness(GameObject.GetInstance().HumanAirline)))));
            return panelHumanStatistics;
          


        }
       
        //shows the stats
        private void showStats()
        {
            lbStats.Items.Clear();

            lbStats.Items.Add(new KeyValuePair<Airline, StatisticsType>(this.Airline, StatisticsTypes.GetStatisticsType("Passengers")));
            lbStats.Items.Add(new KeyValuePair<Airline, StatisticsType>(this.Airline, StatisticsTypes.GetStatisticsType("Passengers%")));
            lbStats.Items.Add(new KeyValuePair<Airline, StatisticsType>(this.Airline, StatisticsTypes.GetStatisticsType("Arrivals")));

        }
        //shows the ratings
        private void showRatings()
        {
            lbRatings.Items.Clear();
         
            lbRatings.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirlineStatistics","1013"),UICreator.CreateTextBlock(this.Airline.Ratings.CustomerHappinessRating.ToString())));
            lbRatings.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirlineStatistics", "1014"), UICreator.CreateTextBlock(this.Airline.Ratings.EmployeeHappinessRating.ToString())));
            lbRatings.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirlineStatistics", "1015"), UICreator.CreateTextBlock(this.Airline.Ratings.MaintenanceRating.ToString())));
            lbRatings.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirlineStatistics", "1016"), UICreator.CreateTextBlock(this.Airline.Ratings.SafetyRating.ToString())));
            lbRatings.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirlineStatistics", "1017"), UICreator.CreateTextBlock(this.Airline.Ratings.SecurityRating.ToString())));
     
        }
       
    }
    //the converter for a statistics type
    public class AirlineStatConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            KeyValuePair<Airline, StatisticsType> sa = (KeyValuePair<Airline, StatisticsType>)value;

            int year = Int16.Parse(parameter.ToString());

            if (year == 0 || year == -1)
            {
                int currentYear = GameObject.GetInstance().GameTime.Year + year;
                return string.Format("{0:#,0}", sa.Key.Statistics.getStatisticsValue(currentYear, sa.Value));
            }
            else
            {
                double currentYearValue = sa.Key.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, sa.Value);
                double lastYearValue = sa.Key.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year - 1, sa.Value);

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
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
