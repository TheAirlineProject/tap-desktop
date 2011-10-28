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
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.PageModel.PageAirlineModel;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;

namespace TheAirline.GraphicsModel.PageModel.PageAirportModel.PanelAirportModel
{
    /// <summary>
    /// Interaction logic for PageAirportGates.xaml
    /// </summary>
    public partial class PageAirportGates : Page
    {
        private Airport Airport;
        private StackPanel panelGates;
        private ListBox lbTerminals;
        public PageAirportGates(Airport airport)
        {
            this.Airport = airport;

            //this.Airport.addTerminal(new Terminal(this.Airport, null, 20));
            //this.Airport.addTerminal(new Terminal(this.Airport,Airlines.GetAirlines()[4], 10));


            InitializeComponent();

            // chs, 2011-27-10 added for the possibility of purchasing a terminal
            StackPanel panelGatesTerminals = new StackPanel();
            panelGatesTerminals.Margin = new Thickness(0, 10, 50, 0);

            panelGates = new StackPanel();


            panelGatesTerminals.Children.Add(panelGates);


            TextBlock txtTerminalsInfoHeader = new TextBlock();
            txtTerminalsInfoHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtTerminalsInfoHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtTerminalsInfoHeader.FontWeight = FontWeights.Bold;
            txtTerminalsInfoHeader.Text = "Terminals Information (Owner/Gates)";
            txtTerminalsInfoHeader.Margin = new Thickness(0, 10, 0, 0);

            panelGatesTerminals.Children.Add(txtTerminalsInfoHeader);

            lbTerminals = new ListBox();
            lbTerminals.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbTerminals.ItemTemplate = this.Resources["TerminalItem"] as DataTemplate;

            panelGatesTerminals.Children.Add(lbTerminals);

            Button btnTerminal = new Button();
            btnTerminal.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnTerminal.Height = Double.NaN;
            btnTerminal.Width = Double.NaN;
            btnTerminal.Margin = new Thickness(0, 5, 0, 0);
            btnTerminal.Content = "Build Terminal";
            btnTerminal.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnTerminal.Click += new RoutedEventHandler(btnTerminal_Click);
            btnTerminal.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            panelGatesTerminals.Children.Add(btnTerminal);

            this.Content = panelGatesTerminals;

            showGatesInformation();
            showTerminals();
        }

        private void btnTerminal_Click(object sender, RoutedEventArgs e)
        {
            this.Airport.addTerminal(new Terminal(this.Airport, GameObject.GetInstance().HumanAirline, 20));
            showGatesInformation();
            showTerminals();
        }
        // chs, 2011-27-10 added for the possibility of purchasing a terminal
        //shows the terminals
        private void showTerminals()
        {
            lbTerminals.Items.Clear();

            foreach (Terminal terminal in this.Airport.Terminals.getTerminals().FindAll((delegate(Terminal t) { return t.Airline != null; })))
            {
                lbTerminals.Items.Add(terminal);
            }
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
            lbAirlineInfo.Items.Add(new QuickInfoValue("Total number of gates", UICreator.CreateTextBlock(this.Airport.Terminals.getNumberOfGates().ToString())));
            lbAirlineInfo.Items.Add(new QuickInfoValue("Used gates", UICreator.CreateTextBlock((this.Airport.Terminals.getNumberOfGates() - this.Airport.Terminals.getFreeGates()).ToString())));
            lbAirlineInfo.Items.Add(new QuickInfoValue("Free gates", UICreator.CreateTextBlock(this.Airport.Terminals.getFreeGates().ToString())));
            lbAirlineInfo.Items.Add(new QuickInfoValue("Monthly price per gate", UICreator.CreateTextBlock(string.Format("{0:c}", this.Airport.getGatePrice()))));

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
                lbAirlineGates.Items.Add(new AirlineGates(airline, this.Airport.Terminals.getNumberOfGates(airline), this.Airport.Terminals.getNumberOfGates(airline) - this.Airport.Terminals.getFreeGates(airline)));


            WrapPanel buttonsPanel = new WrapPanel();
            buttonsPanel.Margin = new Thickness(0, 5, 0, 0);

            panelGates.Children.Add(buttonsPanel);

            Button btnRent = new Button();
            btnRent.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnRent.Height = Double.NaN;
            btnRent.Width = Double.NaN;
            btnRent.Content = "Rent";
            btnRent.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            // chs, 2011-27-10 changed so it is only possible to rent 75 % of the gates at an airport

            btnRent.IsEnabled = this.Airport.Terminals.getFreeGates() > 0 && Convert.ToDouble(this.Airport.Terminals.getNumberOfGates(GameObject.GetInstance().HumanAirline) * 3) / 4 < this.Airport.Terminals.getNumberOfGates();
            btnRent.Click += new RoutedEventHandler(btnRent_Click);
            buttonsPanel.Children.Add(btnRent);

            Button btnRelease = new Button();
            btnRelease.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnRelease.Height = Double.NaN;
            btnRelease.Width = Double.NaN;
            btnRelease.Content = "Release";
            btnRelease.IsEnabled = this.Airport.Terminals.getNumberOfGates(GameObject.GetInstance().HumanAirline) > 0 && !(this.Airport.hasFacilities(GameObject.GetInstance().HumanAirline) && this.Airport.Terminals.getNumberOfGates(GameObject.GetInstance().HumanAirline) == 1) && !(this.Airport.Terminals.getNumberOfGates(GameObject.GetInstance().HumanAirline) == 1 && this.Airport.hasAsHomebase(GameObject.GetInstance().HumanAirline)) && !(this.Airport.Terminals.getNumberOfGates(GameObject.GetInstance().HumanAirline) == 1 && GameObject.GetInstance().HumanAirline.Airports.Count == 1) && !(this.Airport.Terminals.getRoutes(GameObject.GetInstance().HumanAirline).Count > 0 && this.Airport.Terminals.getFreeGates(GameObject.GetInstance().HumanAirline) == 0);
            btnRelease.Click += new RoutedEventHandler(btnRelease_Click);
            btnRelease.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnRelease.Margin = new Thickness(2, 0, 0, 0);
            buttonsPanel.Children.Add(btnRelease);


        }

        private void btnRelease_Click(object sender, RoutedEventArgs e)
        {

            this.Airport.Terminals.releaseGate(GameObject.GetInstance().HumanAirline);

            showGatesInformation();
        }

        private void btnRent_Click(object sender, RoutedEventArgs e)
        {
            this.Airport.Terminals.rentGate(GameObject.GetInstance().HumanAirline);

            showGatesInformation();

        }
        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Airline airline = (Airline)((Hyperlink)sender).Tag;

            PageNavigator.NavigateTo(new PageAirline(airline));


        }
        // chs, 2011-27-10 added for the possibility of purchasing a terminal
        private void btnRemoveTerminal_Click(object sender, RoutedEventArgs e)
        {
             Terminal terminal = (Terminal)((Button)sender).Tag;

             if (terminal.Gates.getUsedGate(terminal.Airline) == null)
             {
                 WPFMessageBoxResult result = WPFMessageBox.Show("Remove terminal", string.Format("Are you sure you want to remove this terminal with {0} gates?", terminal.Gates.NumberOfGates), WPFMessageBoxButtons.YesNo);

                 if (result == WPFMessageBoxResult.Yes)
                 {
                     this.Airport.removeTerminal(terminal);

                     showTerminals();
                     showGatesInformation();

                 }
             }
             else
             {
                 WPFMessageBox.Show("Removal not possible", "It is not possible to remove the terminal, since it is currently in use", WPFMessageBoxButtons.Ok);
             }
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
