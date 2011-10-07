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

namespace TheAirline.GraphicsModel.PageModel.PageAirlineModel.PanelAirlineModel
{
    /// <summary>
    /// Interaction logic for PageAirlineStatistics.xaml
    /// </summary>
    public partial class PageAirlineStatistics : Page
    {
        private Airline Airline;
        private TextBlock txtPassengers, txtDepartures, txtPerFlight;
        public PageAirlineStatistics(Airline airline)
        {
            this.Airline = airline;

            InitializeComponent();

            StackPanel panelStatistics = new StackPanel();
            panelStatistics.Margin = new Thickness(0, 10, 50, 0);

            TextBlock txtHeader = new TextBlock();
            txtHeader.Margin = new Thickness(0, 0, 0, 0);
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = "Airline Statistics";

            panelStatistics.Children.Add(txtHeader);

            ListBox lbStatistics = new ListBox();
            lbStatistics.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbStatistics.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            txtDepartures = UICreator.CreateTextBlock(this.Airline.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Departures")).ToString());
            lbStatistics.Items.Add(new QuickInfoValue("Total departures",txtDepartures));

            txtPassengers = UICreator.CreateTextBlock(this.Airline.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Passengers")).ToString());
            lbStatistics.Items.Add(new QuickInfoValue("Total passengers", txtPassengers));

            txtPerFlight = UICreator.CreateTextBlock(this.Airline.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Passengers%")).ToString());
            lbStatistics.Items.Add(new QuickInfoValue("Passengers per flight",txtPerFlight));

           
            panelStatistics.Children.Add(lbStatistics);


            GameTimer.GetInstance().OnTimeChanged += new GameTimer.TimeChanged(PageAirlineStatistics_OnTimeChanged);

            this.Content = panelStatistics;
        }

        private void PageAirlineStatistics_OnTimeChanged()
        {
            if (this.IsLoaded)
            {
                txtPassengers.Text = this.Airline.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Passengers")).ToString();
                txtPerFlight.Text = this.Airline.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Passengers%")).ToString();
                txtDepartures.Text = this.Airline.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Departures")).ToString();
            }
        }
    }
}
