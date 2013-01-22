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
using TheAirline.Model.AirlineModel.SubsidiaryModel;
using TheAirline.Model.GeneralModel.CountryModel;

namespace TheAirline.GraphicsModel.PageModel.PageAirlinesModel.PanelAirlinesModel
{
    // chs, 2011-18-10 added for the extended statistics view of the airliners
    /// <summary>
    /// Interaction logic for PageAirlinesExtendedStatistics.xaml
    /// </summary>
    public partial class PageAirlinesExtendedStatistics : Page
    {
        public enum ViewType { Fleet = 1001, Financial = 1002 }
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
            txtHeader.Text = string.Format(Translator.GetInstance().GetString("PanelAirlinesExtendedStatistics", ((int)this.View).ToString()));

            panelStatistics.Children.Add(txtHeader);

            panelStats = new StackPanel();


            panelStatistics.Children.Add(panelStats);

            // GameTimer.GetInstance().OnTimeChanged += new GameTimer.TimeChanged(PageAirlinesStatistics_OnTimeChanged);

            // this.Unloaded += new RoutedEventHandler(PageAirlinesExtendedStatistics_Unloaded);

            scroller.Content = panelStatistics;

            this.Content = scroller;

            showStats();
        }

        private void PageAirlinesExtendedStatistics_Unloaded(object sender, RoutedEventArgs e)
        {
            GameTimer.GetInstance().OnTimeChanged -= new GameTimer.TimeChanged(PageAirlinesStatistics_OnTimeChanged);

        }
        //shows the stats
        private void showStats()
        {
            panelStats.Children.Clear();

            if (this.View == ViewType.Fleet)
            {
                panelStats.Children.Add(createStatisticsPanel("getFleetSize", Translator.GetInstance().GetString("PanelAirlinesExtendedStatistics", "1003"), false,false));
                panelStats.Children.Add(createStatisticsPanel("getAverageFleetAge", Translator.GetInstance().GetString("PanelAirlinesExtendedStatistics", "1004"), false,false));
            }
            else if (this.View == ViewType.Financial)
            {
                panelStats.Children.Add(createStatisticsPanel("getProfit", Translator.GetInstance().GetString("PanelAirlinesExtendedStatistics", "1005"), true,false));
                panelStats.Children.Add(createStatisticsPanel("getValue", Translator.GetInstance().GetString("PanelAirlinesExtendedStatistics", "1006"), true,true));
            }

        }
        //creates the airlines statistics
        private StackPanel createStatisticsPanel(string methodName, string name, Boolean financial, Boolean thousands)
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


            // chs, 2011-22-10 changed so financial statistics are shown in currency
            if (financial && !thousands)
                lbStatistics.ItemTemplate = this.Resources["AirlineFinancialStatItem"] as DataTemplate;
            else if (financial && thousands)
                lbStatistics.ItemTemplate = this.Resources["AirlineThousandsStatItem"] as DataTemplate;
            else
                lbStatistics.ItemTemplate = this.Resources["AirlineStatItem"] as DataTemplate;


            double maxValue = getMaxValue(methodName);

     
            double coff = this.StatWidth / maxValue;

            List<Airline> airlines = Airlines.GetAllAirlines().FindAll(a => !a.IsSubsidiary);
            airlines.Sort((delegate(Airline a1, Airline a2) { return a1.Profile.Name.CompareTo(a2.Profile.Name); }));

            foreach (Airline airline in airlines)
            {

                MethodInfo method = typeof(Airline).GetMethod(methodName);
                long value = Convert.ToInt64(method.Invoke(airline, null));

                lbStatistics.Items.Add(new AirlineStatisticsItem(airline, (int)value, Math.Max(1, (int)Convert.ToDouble(value * coff))));
               
                foreach (SubsidiaryAirline sAirline in airline.Subsidiaries)
                {
                    long sValue = Convert.ToInt64(method.Invoke(sAirline, null));

                    lbStatistics.Items.Add(new AirlineStatisticsItem(sAirline, (int)sValue, Math.Max(1, (int)Convert.ToDouble(sValue * coff))));
                }

            }

            panelStatistics.Children.Add(lbStatistics);

            return panelStatistics;

        }
        //finds the maximum value for a type
        private long getMaxValue(string methodName)
        {
            long value = 1;
            foreach (Airline airline in Airlines.GetAllAirlines())
            {

                MethodInfo method = typeof(Airline).GetMethod(methodName);
                object v = method.Invoke(airline, null);

                if (Convert.ToInt64(v) > value)
                    value = Convert.ToInt64(v);


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
    public class FinancialThousandConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int amount = (int)value;


            if (GameObject.GetInstance().CurrencyCountry == null)
                return string.Format("{0:C}", value);

            CountryCurrency currency = GameObject.GetInstance().CurrencyCountry.getCurrency(GameObject.GetInstance().GameTime);

            if (currency == null)
            {
                return string.Format("{0:C}", value);
            }

            else
            {
                string sFormat = Translator.GetInstance().GetString("General", "2000");
                
                double v = amount * currency.Rate;

                if (currency.Position == CountryCurrency.CurrencyPosition.Right)
                    return string.Format("{0:#,0.##} {2} {1}", v, currency.CurrencySymbol, sFormat);
                else
                    return string.Format("{1}{0:#,0.##} {2}", v, currency.CurrencySymbol, sFormat);

            }
              
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
