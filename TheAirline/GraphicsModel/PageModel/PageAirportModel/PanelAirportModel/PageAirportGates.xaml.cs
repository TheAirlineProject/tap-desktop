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
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;

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
            InitializeComponent();

            this.Airport = airport;

            // chs, 2011-27-10 added for the possibility of purchasing a terminal
            StackPanel panelGatesTerminals = new StackPanel();
            panelGatesTerminals.Margin = new Thickness(0, 10, 50, 0);

            panelGates = new StackPanel();

            panelGatesTerminals.Children.Add(panelGates);

            TextBlock txtTerminalsInfoHeader = new TextBlock();
            txtTerminalsInfoHeader.Uid = "1001";
            txtTerminalsInfoHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtTerminalsInfoHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtTerminalsInfoHeader.FontWeight = FontWeights.Bold;
            txtTerminalsInfoHeader.Text = Translator.GetInstance().GetString("PageAirportGates", txtTerminalsInfoHeader.Uid);
            txtTerminalsInfoHeader.Margin = new Thickness(0, 10, 0, 0);

            panelGatesTerminals.Children.Add(txtTerminalsInfoHeader);

            lbTerminals = new ListBox();
            lbTerminals.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbTerminals.ItemTemplate = this.Resources["TerminalItem"] as DataTemplate;

            panelGatesTerminals.Children.Add(lbTerminals);

            Button btnTerminal = new Button();
            btnTerminal.Uid = "201";
            btnTerminal.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnTerminal.Height = Double.NaN;
            btnTerminal.Width = Double.NaN;
            btnTerminal.Margin = new Thickness(0, 5, 0, 0);
            btnTerminal.Content = Translator.GetInstance().GetString("PageAirportGates", btnTerminal.Uid);
            btnTerminal.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnTerminal.Click += new RoutedEventHandler(btnTerminal_Click);
            btnTerminal.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            panelGatesTerminals.Children.Add(btnTerminal);

            this.Content = panelGatesTerminals;

            showGatesInformation();
            showTerminals();
        }


        // chs, 2011-28-10 changed to show all terminals
        //shows the terminals
        private void showTerminals()
        {
            lbTerminals.Items.Clear();

            foreach (Terminal terminal in this.Airport.Terminals.getTerminals())
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
            txtGatesInfoHeader.Uid = "1002";
            txtGatesInfoHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtGatesInfoHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtGatesInfoHeader.FontWeight = FontWeights.Bold;
            txtGatesInfoHeader.Text = Translator.GetInstance().GetString("PageAirportGates", txtGatesInfoHeader.Uid);

            panelGates.Children.Add(txtGatesInfoHeader);

            ListBox lbAirlineInfo = new ListBox();
            lbAirlineInfo.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbAirlineInfo.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");
            lbAirlineInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirportGates", "1003"), UICreator.CreateTextBlock(this.Airport.Terminals.getNumberOfGates().ToString())));
            lbAirlineInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirportGates", "1004"), UICreator.CreateTextBlock((this.Airport.Terminals.getNumberOfGates() - this.Airport.Terminals.getFreeGates()).ToString())));
            lbAirlineInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirportGates", "1005"), UICreator.CreateTextBlock(this.Airport.Terminals.getFreeGates().ToString())));
            lbAirlineInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirportGates", "1006"), UICreator.CreateTextBlock(string.Format("{0:c}", this.Airport.getGatePrice()))));

            panelGates.Children.Add(lbAirlineInfo);

            TextBlock txtGatesHeader = new TextBlock();
            txtGatesHeader.Uid = "1007";
            txtGatesHeader.Margin = new Thickness(0, 10, 0, 0);
            txtGatesHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtGatesHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtGatesHeader.FontWeight = FontWeights.Bold;
            txtGatesHeader.Text = Translator.GetInstance().GetString("PageAirportGates", txtGatesHeader.Uid);

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
            btnRent.Uid = "202";
            btnRent.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnRent.Height = Double.NaN;
            btnRent.Width = Double.NaN;
            btnRent.Content = Translator.GetInstance().GetString("PageAirportGates", btnRent.Uid);
            btnRent.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            // chs, 2011-27-10 changed so it is only possible to rent 75 % of the gates at an airport
            btnRent.IsEnabled = this.Airport.Terminals.getFreeGates() > 0 && Convert.ToDouble(this.Airport.Terminals.getNumberOfGates(GameObject.GetInstance().HumanAirline) * 3) / 4 < this.Airport.Terminals.getNumberOfGates();
            btnRent.Click += new RoutedEventHandler(btnRent_Click);
            buttonsPanel.Children.Add(btnRent);

            Button btnRelease = new Button();
            btnRelease.Uid = "203";
            btnRelease.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnRelease.Height = Double.NaN;
            btnRelease.Width = Double.NaN;
            btnRelease.Content = Translator.GetInstance().GetString("PageAirportGates", btnRelease.Uid);
            btnRelease.IsEnabled = this.Airport.Terminals.getNumberOfGates(GameObject.GetInstance().HumanAirline) > 0 && !(this.Airport.hasFacilities(GameObject.GetInstance().HumanAirline) && this.Airport.Terminals.getNumberOfGates(GameObject.GetInstance().HumanAirline) == 1) && !(this.Airport.Terminals.getNumberOfGates(GameObject.GetInstance().HumanAirline) == 1 && this.Airport.hasAsHomebase(GameObject.GetInstance().HumanAirline)) && !(this.Airport.Terminals.getNumberOfGates(GameObject.GetInstance().HumanAirline) == 1 && GameObject.GetInstance().HumanAirline.Airports.Count == 1) && !(this.Airport.Terminals.getRoutes(GameObject.GetInstance().HumanAirline).Count > 0 && this.Airport.Terminals.getFreeGates(GameObject.GetInstance().HumanAirline) == 0);
            btnRelease.Click += new RoutedEventHandler(btnRelease_Click);
            btnRelease.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnRelease.Margin = new Thickness(2, 0, 0, 0);
            buttonsPanel.Children.Add(btnRelease);
        }

        private void btnTerminal_Click(object sender, RoutedEventArgs e)
        {
            Terminal terminal = PopUpTerminal.ShowPopUp(this.Airport) as Terminal;

            if (terminal != null)
            {
                // chs, 2011-01-11 changed so a message for confirmation are shown9han
                long price = terminal.Gates.NumberOfGates * this.Airport.getTerminalGatePrice() + this.Airport.getTerminalPrice();

                if (price > GameObject.GetInstance().HumanAirline.Money)
                {
                    WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2205"), Translator.GetInstance().GetString("MessageBox", "2205", "message"), WPFMessageBoxButtons.Ok);
                }
                else
                {
                    WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2206"), string.Format(Translator.GetInstance().GetString("MessageBox", "2206", "message"), terminal.Gates.NumberOfGates, price), WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                        this.Airport.addTerminal(terminal);
                        showGatesInformation();
                        showTerminals();

                        GameObject.GetInstance().HumanAirline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -price));
                    }
                }
            }
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
            try
            {
                Airline airline = (Airline)((Hyperlink)sender).Tag;

                PageNavigator.NavigateTo(new PageAirline(airline));
            }
            catch
            {
            }
        }

        // chs, 2011-27-10 added for the possibility of purchasing a terminal
        private void btnRemoveTerminal_Click(object sender, RoutedEventArgs e)
        {
            Terminal terminal = (Terminal)((Button)sender).Tag;

            if (terminal.Gates.getUsedGate(terminal.Airline) == null)
            {
                // chs, 2011-31-10 changed for the possibility of having delivered and non-delivered terminals

                string strRemove;
                if (terminal.DeliveryDate > GameObject.GetInstance().GameTime)
                    strRemove = Translator.GetInstance().GetString("MessageBox", "2207", "message");
                else
                    strRemove = string.Format(Translator.GetInstance().GetString("MessageBox", "2208", "message"), terminal.Gates.NumberOfGates);
                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2207"), strRemove, WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    this.Airport.removeTerminal(terminal);

                    showTerminals();
                    showGatesInformation();
                }
            }
            else
            {
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2209"), Translator.GetInstance().GetString("MessageBox", "2209", "message"), WPFMessageBoxButtons.Ok);
            }
        }

        // chs 11-10-11: changed for the possibility of purchasing an existing terminal
        private void btnPurchaseTerminal_Click(object sender, RoutedEventArgs e)
        {
            Terminal terminal = (Terminal)((Button)sender).Tag;

            long price= terminal.Gates.NumberOfGates * this.Airport.getTerminalGatePrice() + this.Airport.getTerminalPrice();

            if (price > GameObject.GetInstance().HumanAirline.Money)
            {
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2205"), Translator.GetInstance().GetString("MessageBox", "2205", "message"), WPFMessageBoxButtons.Ok);
            }
            else
            {
                WPFMessageBoxResult result =  WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2211"), string.Format(Translator.GetInstance().GetString("MessageBox", "2211", "message"), price), WPFMessageBoxButtons.YesNo);
      
                if (result == WPFMessageBoxResult.Yes)
                {
                    terminal.purchaseTerminal(GameObject.GetInstance().HumanAirline);
                    showGatesInformation();
                    showTerminals();
                }
      
                GameObject.GetInstance().HumanAirline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -price));
            }
        }

        private void btnEditTerminal_Click(object sender, RoutedEventArgs e)
        {
            Terminal terminal = (Terminal)((Button)sender).Tag;

            if (!terminal.IsBuilt)
            {
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2210"), Translator.GetInstance().GetString("MessageBox", "2210", "message"), WPFMessageBoxButtons.Ok);
            }
            else
            {
                object o = PopUpTerminal.ShowPopUp(terminal);
                if (o != null)
                {
                    int gates = (int)o;

                    long price = gates * this.Airport.getTerminalGatePrice();
                    
                    terminal.extendTerminal(gates);

                    showTerminals();
                    showGatesInformation();

                    GameObject.GetInstance().HumanAirline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -price));
                }
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
