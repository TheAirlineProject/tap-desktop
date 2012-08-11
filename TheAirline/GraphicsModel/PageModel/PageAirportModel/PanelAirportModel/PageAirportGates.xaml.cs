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

namespace TheAirline.GraphicsModel.PageModel.PageAirportModel.PanelAirportModel
{
    /// <summary>
    /// Interaction logic for PageAirportGates.xaml
    /// </summary>
    public partial class PageAirportGates : Page
    {
        private Airport Airport;
        private StackPanel panelGates;
        private ListBox lbTerminals, lbHubs;
        private Button btnHub;
        public PageAirportGates(Airport airport)
        {
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
            svTerminals.MaxHeight = GraphicsHelpers.GetContentHeight() / 4;

            StackPanel panelTerminals = new StackPanel();
           
            TextBlock txtTerminalsInfoHeader = new TextBlock();
            txtTerminalsInfoHeader.Uid = "1001";
            txtTerminalsInfoHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtTerminalsInfoHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtTerminalsInfoHeader.FontWeight = FontWeights.Bold;
            txtTerminalsInfoHeader.Text = Translator.GetInstance().GetString("PageAirportGates", txtTerminalsInfoHeader.Uid);
            txtTerminalsInfoHeader.Margin = new Thickness(0, 10, 0, 0);

            panelTerminals.Children.Add(txtTerminalsInfoHeader);

            lbTerminals = new ListBox();
            lbTerminals.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbTerminals.ItemTemplate = this.Resources["TerminalItem"] as DataTemplate;
            panelTerminals.Children.Add(lbTerminals);

            svTerminals.Content = panelTerminals;

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

            this.Content = panelGatesTerminals;

            showGatesInformation();
            showTerminals();
            showHubs();
        }

       
        //shows the hubs
        private void showHubs()
        {
            lbHubs.Items.Clear();

            foreach (Hub hub in this.Airport.Hubs)
                lbHubs.Items.Add(hub);

            int airlineValue = (int)GameObject.GetInstance().HumanAirline.getAirlineValue()+1;

            int totalHumanHubs = Airports.GetAllAirports().Sum(a => a.Hubs.Count(h => h.Airline == GameObject.GetInstance().HumanAirline));
            double humanGatesPercent = Convert.ToDouble(this.Airport.Terminals.getNumberOfGates(GameObject.GetInstance().HumanAirline)) / Convert.ToDouble(this.Airport.Terminals.getNumberOfGates()) * 100;
            Boolean humanHub = this.Airport.Hubs.Count(h => h.Airline == GameObject.GetInstance().HumanAirline) > 0;

            
            Boolean isBuyHubEnabled = (GameObject.GetInstance().HumanAirline.Money > this.Airport.getHubPrice()) && (!humanHub) && (humanGatesPercent > 20) && (totalHumanHubs < airlineValue) && (this.Airport.Hubs.Count < (int)this.Airport.Profile.Size) && (this.Airport.getAirportFacility(GameObject.GetInstance().HumanAirline, AirportFacility.FacilityType.Service).TypeLevel >= Hub.MinimumServiceFacilities.TypeLevel);
            btnHub.Visibility = isBuyHubEnabled ? Visibility.Visible : System.Windows.Visibility.Collapsed;
        
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

            ListBox lbGatesInfo = new ListBox();
            lbGatesInfo.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbGatesInfo.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");
            lbGatesInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirportGates", "1003"), UICreator.CreateTextBlock(this.Airport.Terminals.getNumberOfGates().ToString())));
            lbGatesInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirportGates", "1004"), UICreator.CreateTextBlock((this.Airport.Terminals.getNumberOfGates() - this.Airport.Terminals.getFreeGates()).ToString())));
            lbGatesInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirportGates", "1005"), UICreator.CreateTextBlock(this.Airport.Terminals.getFreeGates().ToString())));
            lbGatesInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirportGates", "1006"), UICreator.CreateTextBlock(string.Format("{0:c}", this.Airport.getGatePrice()))));
            panelGates.Children.Add(lbGatesInfo);

            Grid grdGatesHubs = UICreator.CreateGrid(2);
            grdGatesHubs.Margin = new Thickness(0, 10, 0, 0);
            panelGates.Children.Add(grdGatesHubs);

            StackPanel panelAirlineGates = new StackPanel();
            panelAirlineGates.Margin = new Thickness(0,0,5,0);

            TextBlock txtGatesHeader = new TextBlock();
            txtGatesHeader.Uid = "1007";
            txtGatesHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtGatesHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtGatesHeader.FontWeight = FontWeights.Bold;
            txtGatesHeader.Text = Translator.GetInstance().GetString("PageAirportGates", txtGatesHeader.Uid);

            panelAirlineGates.Children.Add(txtGatesHeader);

            ListBox lbAirlineGates = new ListBox();
            lbAirlineGates.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbAirlineGates.ItemTemplate = this.Resources["AirlineGatesItem"] as DataTemplate;
            
            panelAirlineGates.Children.Add(lbAirlineGates);

            List<Airline> airlines = (from a in Airlines.GetAllAirlines() where this.Airport.Terminals.getNumberOfGates(a)>0 orderby a.Profile.Name select a).ToList();
           
            foreach (Airline airline in airlines)
                lbAirlineGates.Items.Add(new AirlineGates(airline, this.Airport.Terminals.getNumberOfGates(airline), this.Airport.Terminals.getNumberOfGates(airline) - this.Airport.Terminals.getFreeGates(airline)));

            Grid.SetColumn(panelAirlineGates,0);
            grdGatesHubs.Children.Add(panelAirlineGates);

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

                        AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -price);

                        
           
                    }
                }
            }
        }
        
       
        private void btnRentGate_Click(object sender, RoutedEventArgs e)
        {
            Terminal terminal = (Terminal)((Button)sender).Tag;
            terminal.Gates.rentGate(GameObject.GetInstance().HumanAirline);

            showGatesInformation();
            showTerminals();
            showHubs();
        }

        private void btnReleaseGate_Click(object sender, RoutedEventArgs e)
        {
            double humanGatesPercent = Convert.ToDouble(this.Airport.Terminals.getNumberOfGates(GameObject.GetInstance().HumanAirline)-1) / Convert.ToDouble(this.Airport.Terminals.getNumberOfGates()) * 100;
            Boolean humanHub = this.Airport.Hubs.Count(h => h.Airline == GameObject.GetInstance().HumanAirline) > 0;

            if (humanGatesPercent < 20 && humanHub)
            {
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2215"), Translator.GetInstance().GetString("MessageBox", "2215", "message"), WPFMessageBoxButtons.Ok);
    
            }
            else
            {
                Terminal terminal = (Terminal)((Button)sender).Tag;
                terminal.Gates.releaseGate(GameObject.GetInstance().HumanAirline);

                showGatesInformation();
                showTerminals();
                showHubs();
            }

        }
        private void btnHub_Click(object sender, RoutedEventArgs e)
        {
            

              if (this.Airport.getHubPrice() > GameObject.GetInstance().HumanAirline.Money)
              {
                  WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2212"), Translator.GetInstance().GetString("MessageBox", "2212", "message"), WPFMessageBoxButtons.Ok);
              }
              else
              {
                  WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2213"), string.Format(Translator.GetInstance().GetString("MessageBox", "2213", "message"), this.Airport.getHubPrice()), WPFMessageBoxButtons.YesNo);

                  if (result == WPFMessageBoxResult.Yes)
                  {
                      this.Airport.Hubs.Add(new Hub(GameObject.GetInstance().HumanAirline));

                      showHubs();

                      AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -this.Airport.getHubPrice());

         
        
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

            this.Airport.Hubs.Remove(hub);

            showHubs();
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

                AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -price);

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
    // chs 11-10-11: added for rent and releasing gates at a specific terminal
    //the converter for the options available for a terminal for the human user    
    public class TerminalOptionsVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility rv = Visibility.Collapsed;
            try
            {
                Terminal terminal = (Terminal)value;

                string type = (string)parameter;
                Boolean isEnabled = false;

                if (type == "Rent")
                {
                    isEnabled = terminal.Gates.getFreeGates() > 0 && terminal.Airline == null; 
                   
                }
                if (type == "Release")
                {
             
                    isEnabled = terminal.Gates.getFreeGates(GameObject.GetInstance().HumanAirline)>0 && terminal.Gates.getNumberOfGates(GameObject.GetInstance().HumanAirline) > 0 && terminal.Airline == null && !(terminal.Airport.hasFacilities(GameObject.GetInstance().HumanAirline) && terminal.Airport.Terminals.getNumberOfGates(GameObject.GetInstance().HumanAirline) == 1) && !(terminal.Airport.Terminals.getNumberOfGates(GameObject.GetInstance().HumanAirline) == 1 && terminal.Airport.hasAsHomebase(GameObject.GetInstance().HumanAirline)) && !(terminal.Airport.Terminals.getNumberOfGates(GameObject.GetInstance().HumanAirline) == 1 && GameObject.GetInstance().HumanAirline.Airports.Count == 1) && !(terminal.Airport.Terminals.getRoutes(GameObject.GetInstance().HumanAirline).Count > 0 && terminal.Airport.Terminals.getFreeGates(GameObject.GetInstance().HumanAirline) == 0);
                }
                rv = (Visibility)new BooleanToVisibilityConverter().Convert(isEnabled,null,null,null);
              
            }
            catch (Exception)
            {
               
            }
            return rv;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
}
