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
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.GraphicsModel.Converters;
using System.Diagnostics;

namespace TheAirline.GraphicsModel.PageModel.PageAirportModel.PanelAirportModel
{
    /// <summary>
    /// Interaction logic for PageAirportGates.xaml
    /// </summary>
    public partial class PageAirportGates : Page
    {
        private Airport Airport;
        private StackPanel panelGates;
        private ListBox lbTerminals, lbHubs, lbAirlineContracts;
        private Button btnHub, btnContract;
        public PageAirportGates(Airport airport)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            this.Airport = airport;

            InitializeComponent();

            // chs, 2011-27-10 added for the possibility of purchasing a terminal
            StackPanel panelGatesTerminals = new StackPanel();
            panelGatesTerminals.Margin = new Thickness(0, 10, 50, 0);

            panelGates = new StackPanel();

            panelGatesTerminals.Children.Add(panelGates);

            ScrollViewer svTerminals = new ScrollViewer();
            svTerminals.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            svTerminals.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            svTerminals.MaxHeight = (GraphicsHelpers.GetContentHeight()-100) / 4;

            StackPanel panelAirlineGates = new StackPanel();
            panelAirlineGates.Margin = new Thickness(0, 0, 10, 0);

            TextBlock txtGatesHeader = new TextBlock();
            txtGatesHeader.Uid = "1007";
            txtGatesHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtGatesHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtGatesHeader.FontWeight = FontWeights.Bold;
            txtGatesHeader.Text = Translator.GetInstance().GetString("PageAirportGates", txtGatesHeader.Uid);

            panelAirlineGates.Children.Add(txtGatesHeader);

            lbAirlineContracts = new ListBox();
            lbAirlineContracts.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbAirlineContracts.ItemTemplate = this.Resources["AirlineContractItem"] as DataTemplate;
            lbAirlineContracts.MaxHeight = GraphicsHelpers.GetContentHeight() / 4;

            panelAirlineGates.Children.Add(lbAirlineContracts);

            //List<Airline> airlines = (from a in Airlines.GetAllAirlines() where this.Airport.Terminals.getNumberOfGates(a) > 0 orderby a.Profile.Name select a).ToList();

            //foreach (Airline airline in airlines)
            //lbAirlineContracts.Items.Add(new AirlineGates(airline, this.Airport.Terminals.getNumberOfGates(airline), this.Airport.Terminals.getNumberOfGates(airline) - this.Airport.Terminals.getNumberOfFreeGates(airline)));

            var contracts = (from c in this.Airport.AirlineContracts orderby c.Airline.Profile.Name select c);

            foreach (AirportContract contract in contracts)
                lbAirlineContracts.Items.Add(contract);

            svTerminals.Content = panelAirlineGates;

            panelGatesTerminals.Children.Add(svTerminals);

            WrapPanel panelButtons = new WrapPanel();
            panelButtons.Margin = new Thickness(0, 5, 0, 0);
            panelGatesTerminals.Children.Add(panelButtons);


            Button btnTerminal = new Button();
            btnTerminal.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnTerminal.Uid = "201";
            btnTerminal.Height = Double.NaN;
            btnTerminal.Width = Double.NaN;
            btnTerminal.Content = Translator.GetInstance().GetString("PageAirportGates", btnTerminal.Uid);
            btnTerminal.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnTerminal.Click += new RoutedEventHandler(btnTerminal_Click);
            btnTerminal.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            //btnTerminal.Visibility = this.Airport.AirlineContract == null || this.Airport.AirlineContract.Airline.IsHuman ? Visibility.Visible : System.Windows.Visibility.Collapsed;
            panelButtons.Children.Add(btnTerminal);

            btnHub = new Button();
            btnHub.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnHub.Uid = "204";
            btnHub.Width = Double.NaN;
            btnHub.Height = Double.NaN;
            btnHub.Content = Translator.GetInstance().GetString("PageAirportGates", btnHub.Uid);
            btnHub.Click += new RoutedEventHandler(btnHub_Click);
            btnHub.Margin = new Thickness(5, 0, 0, 0);
            btnHub.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            panelButtons.Children.Add(btnHub);

            btnContract = new Button();
            btnContract.Margin = new Thickness(5, 0, 0, 0);
            btnContract.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnContract.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnContract.Width = Double.NaN;
            btnContract.Height = Double.NaN;
            btnContract.Uid = "206";
            btnContract.Click += btnContract_Click;
            btnContract.Content = Translator.GetInstance().GetString("PageAirportGates", btnContract.Uid);
            panelButtons.Children.Add(btnContract);
            btnContract.ToolTip = UICreator.CreateToolTip("1017");

          
            Airport allocateToAirport = Airports.GetAirports(a => a.Profile.Town == airport.Profile.Town && airport != a && a.Profile.Period.From.AddDays(30) > GameObject.GetInstance().GameTime).FirstOrDefault();

            Button btnReallocate = new Button();
            btnReallocate.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnReallocate.Uid = "205";
            btnReallocate.Width = Double.NaN;
            btnReallocate.Height = Double.NaN;
            btnReallocate.Tag = allocateToAirport;
            btnReallocate.Content = string.Format(Translator.GetInstance().GetString("PageAirportGates", btnReallocate.Uid), allocateToAirport == null ? "" : new AirportCodeConverter().Convert(allocateToAirport).ToString());
            btnReallocate.Visibility = allocateToAirport == null || this.Airport.Terminals.getNumberOfGates(GameObject.GetInstance().HumanAirline) == 0 ? Visibility.Collapsed : Visibility.Visible;
            btnReallocate.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnReallocate.Margin = new Thickness(5, 0, 0, 0);
            btnReallocate.Click += new RoutedEventHandler(btnReallocate_Click);
            panelButtons.Children.Add(btnReallocate);

            this.Content = panelGatesTerminals;

            showGatesInformation();
            showTerminals();
            showHubs();
            showContracts();
            sw.Stop();

            PerformanceCounters.AddPerformanceCounter(new PagePerformanceCounter("PageAirportGate", GameObject.GetInstance().GameTime, sw.ElapsedMilliseconds));

        }

      
        //shows the hubs
        private void showHubs()
        {
            lbHubs.Items.Clear();

            var hubs = this.Airport.getHubs().OrderBy(h=>h.Type.Name);
            
            foreach (Hub hub in hubs)
                lbHubs.Items.Add(hub);

            int contracts = this.Airport.getAirlineContracts(GameObject.GetInstance().HumanAirline).Count;

            btnHub.Visibility = contracts > 0 ? Visibility.Visible : System.Windows.Visibility.Collapsed;

       
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

            showHubs();
      
        }
        //shows the contracts
        private void showContracts()
        {
            lbAirlineContracts.Items.Clear();

            var contracts = (from c in this.Airport.AirlineContracts orderby c.Airline.Profile.Name select c);

            foreach (AirportContract contract in contracts)
                lbAirlineContracts.Items.Add(contract);

            btnContract.IsEnabled = this.Airport.Terminals.getFreeGates() > 0;

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

            ListBox lbGatesInfo = new ListBox();
            lbGatesInfo.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbGatesInfo.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");
            lbGatesInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirportGates", "1003"), UICreator.CreateTextBlock(this.Airport.Terminals.getNumberOfGates().ToString())));
            lbGatesInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirportGates", "1004"), UICreator.CreateTextBlock((this.Airport.Terminals.getNumberOfGates() - this.Airport.Terminals.getFreeGates()).ToString())));
            lbGatesInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirportGates", "1005"), UICreator.CreateTextBlock(this.Airport.Terminals.getFreeGates().ToString())));
            lbGatesInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirportGates", "1006"), UICreator.CreateTextBlock(new ValueCurrencyConverter().Convert(this.Airport.getGatePrice()).ToString())));//string.Format("{0:c}", this.Airport.getGatePrice()))));
            
            /*
            if (this.Airport.AirlineContract != null)
            {
                ContentControl ccAirline = new ContentControl();
                ccAirline.SetResourceReference(ContentControl.ContentTemplateProperty, "AirlineLogoItem");
                ccAirline.Content = this.Airport.AirlineContract.Airline;

                lbGatesInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirportGates","1009"), ccAirline));
            }*/
            
            panelGates.Children.Add(lbGatesInfo);

            Grid grdGatesHubs = UICreator.CreateGrid(2);
            grdGatesHubs.Margin = new Thickness(0, 10, 0, 0);
            panelGates.Children.Add(grdGatesHubs);

            StackPanel panelTerminals = new StackPanel();
            panelTerminals.Margin = new Thickness(0, 0, 5, 0);

            TextBlock txtTerminalsInfoHeader = new TextBlock();
            txtTerminalsInfoHeader.Uid = "1001";
            txtTerminalsInfoHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtTerminalsInfoHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtTerminalsInfoHeader.FontWeight = FontWeights.Bold;
            txtTerminalsInfoHeader.Text = Translator.GetInstance().GetString("PageAirportGates", txtTerminalsInfoHeader.Uid);
           
            panelTerminals.Children.Add(txtTerminalsInfoHeader);

            lbTerminals = new ListBox();
            lbTerminals.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbTerminals.ItemTemplate = this.Resources["TerminalItem"] as DataTemplate;
            panelTerminals.Children.Add(lbTerminals);

           
            Grid.SetColumn(panelTerminals, 0);
            grdGatesHubs.Children.Add(panelTerminals);

            StackPanel panelHubs = new StackPanel();
            panelHubs.Margin = new Thickness(5, 0, 0, 0);

            TextBlock txtAirportHubs = new TextBlock();
            txtAirportHubs.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtAirportHubs.FontWeight = FontWeights.Bold;
            txtAirportHubs.Uid = "1008";
            txtAirportHubs.Text = Translator.GetInstance().GetString("PageAirportGates", txtAirportHubs.Uid);


            panelHubs.Children.Add(txtAirportHubs);

            lbHubs = new ListBox();
            lbHubs.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbHubs.ItemTemplate = this.Resources["HubItem"] as DataTemplate;

            panelHubs.Children.Add(lbHubs);

            Grid.SetColumn(panelHubs, 1);
            grdGatesHubs.Children.Add(panelHubs);



        }
     
        private void btnContract_Click(object sender, RoutedEventArgs e)
        {
            Airport airport = this.Airport;
            Boolean hasCheckin = airport.getAirportFacility(GameObject.GetInstance().HumanAirline, AirportFacility.FacilityType.CheckIn).TypeLevel > 0;

            object o = PopUpAirportContract.ShowPopUp(airport);

            if (o != null)
            {
                if (!hasCheckin)
                {
                    AirportFacility checkinFacility = AirportFacilities.GetFacilities(AirportFacility.FacilityType.CheckIn).Find(f => f.TypeLevel == 1);

                    airport.addAirportFacility(GameObject.GetInstance().HumanAirline, checkinFacility, GameObject.GetInstance().GameTime);
                    AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -checkinFacility.Price);

                }
                AirportContract contract = (AirportContract)o;

                //25 % off if paying up front
                if (contract.PayFull)
                {
                    double payment = (contract.YearlyPayment * contract.Length) * 0.75;
                    AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline,GameObject.GetInstance().GameTime,Invoice.InvoiceType.Rents,-payment);
                    contract.YearlyPayment = 0;
                }

                this.Airport.addAirlineContract(contract);

                showGatesInformation();
                showContracts();
                showTerminals();
            }


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
                        showContracts();
                        showTerminals();

                        AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -price);



                    }
                }
            }
        }
               
      
        private void btnReallocate_Click(object sender, RoutedEventArgs e)
        {
            Airport allocateToAirport = (Airport)((Button)sender).Tag;

            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2216"), string.Format(Translator.GetInstance().GetString("MessageBox", "2216", "message"), this.Airport.Profile.Name, allocateToAirport.Profile.Name), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                AirlineHelpers.ReallocateAirport(this.Airport, allocateToAirport, GameObject.GetInstance().HumanAirline);

                PageNavigator.NavigateTo(new PageAirport(allocateToAirport));
            }
        }
        //creates the radio button for a hub type
        private RadioButton createHubType(HubType type, Panel panel)
        {
            RadioButton rbHub = new RadioButton();
            rbHub.Content = string.Format("{0} ({1})", type.Name, new ValueCurrencyConverter().Convert(AirportHelpers.GetHubPrice(this.Airport, type)));
            rbHub.GroupName = "Hub";
            rbHub.Tag = new KeyValuePair<HubType,Panel>(type,panel);
            rbHub.Checked += rbHub_Checked;
            rbHub.IsChecked = true;

            return rbHub;
        }

        private void rbHub_Checked(object sender, RoutedEventArgs e)
        {
            KeyValuePair<HubType, Panel> value = (KeyValuePair<HubType, Panel>)((RadioButton)sender).Tag;

            value.Value.Tag = value.Key;
        }
        private void btnHub_Click(object sender, RoutedEventArgs e)
        {
            StackPanel panelHubs = new StackPanel();

            foreach (HubType type in HubTypes.GetHubTypes())
            {
            
                if (AirlineHelpers.CanCreateHub(GameObject.GetInstance().HumanAirline,this.Airport,type))
                    panelHubs.Children.Add(createHubType(type,panelHubs));
            }

            if (panelHubs.Children.Count == 0)
                panelHubs.Children.Add(UICreator.CreateTextBlock("You can't etablish any hubs at this airport"));

            if (PopUpSingleElement.ShowPopUp("Select hub type", panelHubs) == PopUpSingleElement.ButtonSelected.OK && panelHubs.Tag != null)
            {
                HubType type = (HubType)panelHubs.Tag;

                if (AirportHelpers.GetHubPrice(this.Airport, type) > GameObject.GetInstance().HumanAirline.Money)
                    WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2212"), Translator.GetInstance().GetString("MessageBox", "2212", "message"), WPFMessageBoxButtons.Ok);
                else
                {

                    this.Airport.addHub(new Hub(GameObject.GetInstance().HumanAirline, type));

                    showHubs();

                    AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -AirportHelpers.GetHubPrice(this.Airport, type));
                }
            }

         

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
        private void btnRemoveHub_Click(object sender, RoutedEventArgs e)
        {
            Hub hub = (Hub)((Button)sender).Tag;

            this.Airport.removeHub(hub);

            showHubs();
        }
        private void btnEditContract_Click(object sender, RoutedEventArgs e)
        {
            AirportContract contract = (AirportContract)((Button)sender).Tag;
             object o = PopUpAirportContract.ShowPopUp(contract);
        
            if (o != null)
             {
                 this.Airport.removeAirlineContract(contract);
                 this.Airport.addAirlineContract((AirportContract)o);
                 showContracts();
             }
        }
        private void btnRemoveContract_Click(object sender, RoutedEventArgs e)
        {
            AirportContract contract = (AirportContract)((Button)sender).Tag;

            //int totalGatesAfter = this.Airport.AirlineContracts.Where(a=>a.Airline == GameObject.GetInstance().HumanAirline).Sum(c => c.NumberOfGates) - contract.NumberOfGates;

            //int routes = AirportHelpers.GetAirportRoutes(this.Airport,GameObject.GetInstance().HumanAirline).Count;

            double penaltyFee = ((contract.YearlyPayment / 12) * contract.MonthsLeft) / 10;

            var contracts = this.Airport.AirlineContracts.Where(a=>a.Airline == GameObject.GetInstance().HumanAirline && a != contract).ToList() ;

            if (!AirportHelpers.CanFillRoutesEntries(this.Airport,GameObject.GetInstance().HumanAirline,contracts))
            {
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2224"), Translator.GetInstance().GetString("MessageBox", "2224", "message"),WPFMessageBoxButtons.Ok);
            }
            else if (contract.Terminal != null)
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2226"), string.Format(Translator.GetInstance().GetString("MessageBox", "2226", "message"),contract.Terminal.Name), WPFMessageBoxButtons.Ok);
            else
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2225"), string.Format(Translator.GetInstance().GetString("MessageBox", "2225", "message"), new ValueCurrencyConverter().Convert(penaltyFee)),WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -penaltyFee);

                    this.Airport.removeAirlineContract(contract);

                    showGatesInformation();
                    showContracts();
                    showTerminals();
     
                }
            }

        }
        // chs, 2011-27-10 added for the possibility of purchasing a terminal
        private void btnRemoveTerminal_Click(object sender, RoutedEventArgs e)
        {
            Terminal terminal = (Terminal)((Button)sender).Tag;

            int totalRentedGates = this.Airport.AirlineContracts.Sum(c => c.NumberOfGates);

            Boolean isTerminalFree = this.Airport.Terminals.getNumberOfGates() - terminal.Gates.NumberOfGates >= totalRentedGates;
            if (isTerminalFree)
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

            long price = terminal.Gates.NumberOfGates * this.Airport.getTerminalGatePrice() + this.Airport.getTerminalPrice();

            if (price > GameObject.GetInstance().HumanAirline.Money)
            {
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2205"), Translator.GetInstance().GetString("MessageBox", "2205", "message"), WPFMessageBoxButtons.Ok);

            }
            else
            {

                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2211"), string.Format(Translator.GetInstance().GetString("MessageBox", "2211", "message"), price), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    terminal.purchaseTerminal(GameObject.GetInstance().HumanAirline);
                    showGatesInformation();
                    showTerminals();
                    showContracts();
                    AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -price);

                }

     
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
                    showContracts();
                    showGatesInformation();

                    AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -price);


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

