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
using System.Reflection;
using TheAirline.Model.AirlineModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.PageModel.PageAirlineModel;

namespace TheAirline.GraphicsModel.PageModel.PageAirlinesModel.PanelAirlinesModel
{
    // chs, 2011-18-10 added for the extended statistics view of the airliners
    /// <summary>
    /// Interaction logic for PageAirlinesExtendedStatistics.xaml
    /// </summary>
    public partial class PageAirlinesExtendedStatistics : Page
    {
        public enum ViewType { Fleet, Financial }
        public ViewType View { get; set; }
        private StackPanel panelStats;
        private int StatWidth = 200;
      
        public PageAirlinesExtendedStatistics(ViewType view)
        {
            this.View = view;

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
            txtHeader.Text = string.Format("{0} Statistics", this.View);

            panelStatistics.Children.Add(txtHeader);

            panelStats = new StackPanel();


            panelStatistics.Children.Add(panelStats);

            GameTimer.GetInstance().OnTimeChanged += new GameTimer.TimeChanged(PageAirlinesStatistics_OnTimeChanged);

            scroller.Content = panelStatistics;

            this.Content = scroller;

            showStats();
        }
        //shows the stats
        private void showStats()
        {
            panelStats.Children.Clear();

            if (this.View == ViewType.Fleet)
            {
                panelStats.Children.Add(createStatisticsPanel("getFleetSize", "Airline Fleet Size"));
                panelStats.Children.Add(createStatisticsPanel("getAverageFleetAge", "Airline Fleet Age"));
            }
            else if (this.View == ViewType.Financial)
            {
                panelStats.Children.Add(createStatisticsPanel("getProfit", "Airline Profit"));
                panelStats.Children.Add(createStatisticsPanel("getValue", "Airline Value"));
            }

        }
        //creates the airlines statistics
        private StackPanel createStatisticsPanel(string methodName, string name)
        {
            StackPanel panelStatistics = new StackPanel();
            panelStatistics.Margin = new Thickness(0, 0, 0, 5);

            TextBlock txtHeader = new TextBlock();
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = name;

            panelStatistics.Children.Add(txtHeader);

            ListBox lbStatistics = new ListBox();
            lbStatistics.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbStatistics.ItemTemplate = this.Resources["AirlineStatItem"] as DataTemplate;


            double maxValue = getMaxValue(methodName);

            double coff = this.StatWidth / maxValue;

            List<Airline> airlines = Airlines.GetAirlines();
            airlines.Sort((delegate(Airline a1, Airline a2) { return a1.Profile.Name.CompareTo(a2.Profile.Name); }));



            foreach (Airline airline in airlines)
            {

                MethodInfo method = typeof(Airline).GetMethod(methodName);
                double value = (double)method.Invoke(airline, null);

                lbStatistics.Items.Add(new AirlineStatisticsItem(airline, (int)value, Math.Max(1, (int)Convert.ToDouble(value * coff))));

            }

            panelStatistics.Children.Add(lbStatistics);

            return panelStatistics;

        }
        //finds the maximum value for a type
        private double getMaxValue(string methodName)
        {
            double value = 1;
            foreach (Airline airline in Airlines.GetAirlines())
            {

                MethodInfo method = typeof(Airline).GetMethod(methodName);
                double aValue = (double)method.Invoke(airline, null);

                if (aValue > value)
                    value = aValue;
            }

            return value;
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
