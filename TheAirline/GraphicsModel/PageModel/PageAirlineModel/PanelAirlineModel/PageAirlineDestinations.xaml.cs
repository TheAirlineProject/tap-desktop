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
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.PageModel.PageAirportModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.Converters;
using TheAirline.Model.GeneralModel.StatisticsModel;

namespace TheAirline.GraphicsModel.PageModel.PageAirlineModel.PanelAirlineModel
{
    /// <summary>
    /// Interaction logic for PageAirlineDestinations.xaml
    /// </summary>
    public partial class PageAirlineDestinations : Page
    {
        private Airline Airline;
        public PageAirlineDestinations(Airline airline)
        {
            InitializeComponent();

            this.Airline = airline;

            StackPanel panelDestinations = new StackPanel();
            panelDestinations.Margin = new Thickness(0, 10, 50, 0);

            TextBlock txtDestinationsHeader = new TextBlock();
            txtDestinationsHeader.Uid = "1001";
            txtDestinationsHeader.Margin = new Thickness(0, 0, 0, 0);
            txtDestinationsHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtDestinationsHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtDestinationsHeader.FontWeight = FontWeights.Bold;
            txtDestinationsHeader.Text = Translator.GetInstance().GetString("PageAirlineDestinations", txtDestinationsHeader.Uid);

            panelDestinations.Children.Add(txtDestinationsHeader);

            ListBox lbDestinations = new ListBox();
            lbDestinations.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbDestinations.ItemTemplate = this.Resources["AirportItem"] as DataTemplate;
            // chs, 2011-10-11 changed the max height so all elements are visible
            lbDestinations.MaxHeight = (GraphicsHelpers.GetContentHeight()-100)/2;

            this.Airline.Airports.ForEach(a => lbDestinations.Items.Add(a));
        
            panelDestinations.Children.Add(lbDestinations);

            WrapPanel panelButtons = new WrapPanel();
            panelButtons.Margin = new Thickness(0, 5, 0, 0);
            panelButtons.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            panelDestinations.Children.Add(panelButtons);

            Button btnDestinations = new Button();
            btnDestinations.Uid = "1002";
            btnDestinations.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnDestinations.Width = Double.NaN;
            btnDestinations.Height = Double.NaN;
            btnDestinations.Content = Translator.GetInstance().GetString("PageAirlineDestinations", btnDestinations.Uid);
            btnDestinations.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnDestinations.Click += new RoutedEventHandler(btnDestinations_Click);

            panelButtons.Children.Add(btnDestinations);

            Button btnMap = new Button();
            btnMap.Uid = "201";
            btnMap.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnMap.Width = Double.NaN;
            btnMap.Height = Double.NaN;
            btnMap.Content = Translator.GetInstance().GetString("PageAirlineDestinations", btnMap.Uid);
            btnMap.Margin = new Thickness(5, 0, 0, 0);
            btnMap.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnMap.Click += new RoutedEventHandler(btnMap_Click);

            panelButtons.Children.Add(btnMap);

            panelDestinations.Children.Add(createRoutesInformationPanel());

            this.Content = panelDestinations;

        }

        private void btnDestinations_Click(object sender, RoutedEventArgs e)
        {
            PopUpMap.ShowPopUp(this.Airline.Airports);
        }

        private void btnMap_Click(object sender, RoutedEventArgs e)
        {
            PopUpMap.ShowPopUp(this.Airline.Routes);
        }

        private void LnkAirport_Click(object sender, RoutedEventArgs e)
        {
            Airport airport = (Airport)((Hyperlink)sender).Tag;

            PageNavigator.NavigateTo(new PageAirport(airport));
        }
        //creates the panel for some routes information
        private StackPanel createRoutesInformationPanel()
        {
            StackPanel informationPanel = new StackPanel();
            informationPanel.Margin = new Thickness(0, 10, 0, 0);

            TextBlock txtHeader = new TextBlock();
            txtHeader.Uid = "1001";
            txtHeader.Margin = new Thickness(0, 0, 0, 0);
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PageAirlineDestinations", txtHeader.Uid);

            informationPanel.Children.Add(txtHeader);

            ListBox lbInfo = new ListBox();
            lbInfo.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbInfo.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");
            lbInfo.MaxHeight = (GraphicsHelpers.GetContentHeight()-100)/2;

            informationPanel.Children.Add(lbInfo);

            lbInfo.Items.Add(new QuickInfoValue("Total number of routes", UICreator.CreateTextBlock(this.Airline.Routes.Count.ToString())));

            double maxDistance = this.Airline.Routes.Count == 0 ? 0 : this.Airline.Routes.Max(r=>MathHelpers.GetDistance(r.Destination1,r.Destination2));
            lbInfo.Items.Add(new QuickInfoValue("Longest route",UICreator.CreateTextBlock(string.Format("{0:0} {1}", new NumberToUnitConverter().Convert(maxDistance),new StringToLanguageConverter().Convert("km.")))));

            double avgBalance = this.Airline.Routes.Count == 0 ? 0 : this.Airline.Routes.Average(r => r.Balance);
            lbInfo.Items.Add(new QuickInfoValue("Average route balance", UICreator.CreateTextBlock(string.Format("{0:C}", avgBalance))));

            double avgFillingPercent = this.Airline.Routes.Count == 0 ? 0 : this.Airline.Routes.Average(r => r.FillingDegree);
            lbInfo.Items.Add(new QuickInfoValue("Average filling percent",UICreator.CreateTextBlock(string.Format("{0:0} %",avgFillingPercent*100))));

            int totalFlights = this.Airline.Routes.Count == 0 ? 0 : this.Airline.Routes.Sum(r => r.TimeTable.Entries.Count);
            lbInfo.Items.Add(new QuickInfoValue("Total weekly flights", UICreator.CreateTextBlock(totalFlights.ToString())));

            long totalPassengers = this.Airline.Routes.Count == 0 ? 0 : this.Airline.Routes.Sum(r=>r.Statistics.getTotalValue(StatisticsTypes.GetStatisticsType("Passengers")));
            lbInfo.Items.Add(new QuickInfoValue("Total passengers", UICreator.CreateTextBlock(totalPassengers.ToString())));



            return informationPanel;
        }
    }
}
