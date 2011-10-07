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
using TheAirlineV2.Model.GeneralModel;
using TheAirlineV2.Model.AirlinerModel;
using TheAirlineV2.GraphicsModel.PageModel.GeneralModel;
using TheAirlineV2.Model.GeneralModel.StatisticsModel;

namespace TheAirlineV2.GraphicsModel.PageModel.PageFleetAirlinerModel.PanelFleetAirlinerModel
{
    /// <summary>
    /// Interaction logic for PageFleetStatistics.xaml
    /// </summary>
    public partial class PageFleetStatistics : Page
    {
        private TextBlock txtPassengers, txtDepartures, txtPerFlight;
        private FleetAirliner Airliner;
        public PageFleetStatistics(FleetAirliner airliner)
        {
            this.Airliner = airliner;

            InitializeComponent();

          
            StackPanel panelStatistics = new StackPanel();
            panelStatistics.Margin = new Thickness(0, 10, 50, 0);

            TextBlock txtHeader = new TextBlock();
            txtHeader.Margin = new Thickness(0, 0, 0, 0);
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = "Airliner Statistics";

            panelStatistics.Children.Add(txtHeader);

            ListBox lbStatistics = new ListBox();
            lbStatistics.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbStatistics.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            txtDepartures = UICreator.CreateTextBlock(this.Airliner.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Departures")).ToString());
            lbStatistics.Items.Add(new QuickInfoValue("Total departures", txtDepartures));

            txtPassengers = UICreator.CreateTextBlock(this.Airliner.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Passengers")).ToString());
            lbStatistics.Items.Add(new QuickInfoValue("Total passengers", txtPassengers));

            txtPerFlight = UICreator.CreateTextBlock(this.Airliner.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Passengers%")).ToString());
            lbStatistics.Items.Add(new QuickInfoValue("Passengers per flight", txtPerFlight));


            panelStatistics.Children.Add(lbStatistics);


            GameTimer.GetInstance().OnTimeChanged += new GameTimer.TimeChanged(PageFleetStatistics_OnTimeChanged);

            this.Content = panelStatistics;
        }

        private void PageFleetStatistics_OnTimeChanged()
        {
            if (this.IsLoaded)
            {
                txtPassengers.Text = this.Airliner.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Passengers")).ToString();
                txtPerFlight.Text = this.Airliner.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Passengers%")).ToString();
                txtDepartures.Text = this.Airliner.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Departures")).ToString();
            }
        }
    }
}
