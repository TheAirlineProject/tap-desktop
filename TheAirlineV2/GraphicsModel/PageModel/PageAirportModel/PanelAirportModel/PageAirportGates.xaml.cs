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
using TheAirlineV2.Model.AirportModel;
using TheAirlineV2.GraphicsModel.PageModel.GeneralModel;
using TheAirlineV2.Model.AirlineModel;
using TheAirlineV2.Model.GeneralModel;
using TheAirlineV2.GraphicsModel.PageModel.PageAirlineModel;

namespace TheAirlineV2.GraphicsModel.PageModel.PageAirportModel.PanelAirportModel
{
    /// <summary>
    /// Interaction logic for PageAirportGates.xaml
    /// </summary>
    public partial class PageAirportGates : Page
    {
        private Airport Airport;
        private StackPanel panelGates;
        public PageAirportGates(Airport airport)
        {
            this.Airport = airport;

            InitializeComponent();

            panelGates = new StackPanel();
            panelGates.Margin = new Thickness(0, 10, 50, 0);

            showGatesInformation();

            this.Content = panelGates;
        }
        //shows the gates information
        private void showGatesInformation()
        {
            panelGates.Children.Clear();

            //GameObject.HumanAirline

            TextBlock txtGatesInfoHeader = new TextBlock();
            txtGatesInfoHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtGatesInfoHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtGatesInfoHeader.FontWeight = FontWeights.Bold;
            txtGatesInfoHeader.Text = "Gates Information";

            panelGates.Children.Add(txtGatesInfoHeader);

            ListBox lbAirlineInfo = new ListBox();
            lbAirlineInfo.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbAirlineInfo.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");
            lbAirlineInfo.Items.Add(new QuickInfoValue("Total number of gates", UICreator.CreateTextBlock(this.Airport.Profile.Gates.ToString())));
            lbAirlineInfo.Items.Add(new QuickInfoValue("Used gates", UICreator.CreateTextBlock((this.Airport.Profile.Gates - this.Airport.Gates.getFreeGates()).ToString())));
            lbAirlineInfo.Items.Add(new QuickInfoValue("Free gates", UICreator.CreateTextBlock(this.Airport.Gates.getFreeGates().ToString())));
            lbAirlineInfo.Items.Add(new QuickInfoValue("Monthly price per gate", UICreator.CreateTextBlock(string.Format("{0:c}",this.Airport.getGatePrice()))));
            
            panelGates.Children.Add(lbAirlineInfo);
            
            TextBlock txtGatesHeader = new TextBlock();
            txtGatesHeader.Margin = new Thickness(0, 10, 0, 0);
            txtGatesHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtGatesHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtGatesHeader.FontWeight = FontWeights.Bold;
            txtGatesHeader.Text = "Airline Gates (Total / Used)";

            panelGates.Children.Add(txtGatesHeader);

            ListBox lbAirlineGates = new ListBox();
            lbAirlineGates.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbAirlineGates.ItemTemplate = this.Resources["AirlineGatesItem"] as DataTemplate;


            panelGates.Children.Add(lbAirlineGates);

            List<Airline> airlines = Airlines.GetAirlines();
            airlines.Sort((delegate(Airline a1, Airline a2) { return a1.Profile.Name.CompareTo(a2.Profile.Name); }));

            foreach (Airline airline in airlines)
                lbAirlineGates.Items.Add(new AirlineGates(airline, this.Airport.Gates.getNumberOfGates(airline), this.Airport.Gates.getNumberOfGates(airline) - this.Airport.Gates.getFreeGates(airline)));


            WrapPanel buttonsPanel = new WrapPanel();
            buttonsPanel.Margin = new Thickness(0, 5, 0, 0);

            panelGates.Children.Add(buttonsPanel);

            Button btnRent = new Button();
            btnRent.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnRent.Height = 20;
            btnRent.Width = 80;
            btnRent.Content = "Rent";
            btnRent.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnRent.IsEnabled = this.Airport.Gates.getFreeGates() > 0;
            btnRent.Click += new RoutedEventHandler(btnRent_Click);
            buttonsPanel.Children.Add(btnRent);

            Button btnRelease = new Button();
            btnRelease.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnRelease.Height = 20;
            btnRelease.Width = 80;
            btnRelease.Content = "Release";
           // Boolean b = this.Airport.Gates.getNumberOfGates(GameObject.GetInstance().HumanAirline) > 0 && !this.Airport.hasFacilities(GameObject.GetInstance().HumanAirline);
            btnRelease.IsEnabled = this.Airport.Gates.getNumberOfGates(GameObject.GetInstance().HumanAirline) > 0 && !(this.Airport.hasFacilities(GameObject.GetInstance().HumanAirline) && this.Airport.Gates.getNumberOfGates(GameObject.GetInstance().HumanAirline)==1)  && !(this.Airport.Gates.getNumberOfGates(GameObject.GetInstance().HumanAirline) == 1 && this.Airport.hasAsHomebase(GameObject.GetInstance().HumanAirline)) && !(this.Airport.Gates.getNumberOfGates(GameObject.GetInstance().HumanAirline) == 1 && GameObject.GetInstance().HumanAirline.Airports.Count == 1) && !(this.Airport.Gates.getRoutes(GameObject.GetInstance().HumanAirline).Count>0 && this.Airport.Gates.getFreeGates(GameObject.GetInstance().HumanAirline) == 0);
            btnRelease.Click += new RoutedEventHandler(btnRelease_Click);
            btnRelease.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnRelease.Margin = new Thickness(2, 0, 0, 0);
            buttonsPanel.Children.Add(btnRelease);

        }

        private void btnRelease_Click(object sender, RoutedEventArgs e)
        {
            this.Airport.Gates.releaseGate(GameObject.GetInstance().HumanAirline);

            showGatesInformation();
        }

        private void btnRent_Click(object sender, RoutedEventArgs e)
        {
            this.Airport.Gates.rentGate(GameObject.GetInstance().HumanAirline);

            showGatesInformation();

        }
        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Airline airline = (Airline)((Hyperlink)sender).Tag;

            PageNavigator.NavigateTo(new PageAirline(airline));

           
        }
        //the class for the gates at an airport
        private class AirlineGates
        {
            public Airline Airline { get; set; }
            public int Gates { get; set; }
            public int Used { get; set; }
            public AirlineGates(Airline airline, int gates, int used)
            {
                this.Airline = airline;
                this.Gates = gates;
                this.Used = used;

            }
        }
    }
}
