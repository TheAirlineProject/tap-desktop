using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.GUIModel.PagesModel.AirlinePageModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlineModel.SubsidiaryModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GUIModel.PagesModel.AirlinesPageModel
{
    /// <summary>
    /// Interaction logic for PageAirlinesShares.xaml
    /// </summary>
    public partial class PageAirlinesShares : Page, INotifyPropertyChanged
    {
        public ObservableCollection<AirlinesMVVM> AllAirlines { get; set; }
        private int _numberofsharestoissue;
        public int NumberOfSharesToIssue
        {
            get { return _numberofsharestoissue; }
            set { _numberofsharestoissue = value; NotifyPropertyChanged("NumberOfSharesToIssue"); }
        }
        private AirlinesMVVM _selectedairline;
        public AirlinesMVVM SelectedAirline
        {
            get { return _selectedairline; }
            set { _selectedairline = value; NotifyPropertyChanged("SelectedAirline"); }
        }
        public PageAirlinesShares()
        {
            this.AllAirlines = new ObservableCollection<AirlinesMVVM>();
            foreach (Airline airline in Airlines.GetAllAirlines().FindAll(a => !a.IsSubsidiary).OrderByDescending(a => a.IsHuman))
            {
                this.AllAirlines.Add(new AirlinesMVVM(airline));

                foreach (SubsidiaryAirline sAirline in airline.Subsidiaries)
                    this.AllAirlines.Add(new AirlinesMVVM(sAirline));
            }

            this.NumberOfSharesToIssue = 10000;

            InitializeComponent();
        }

        private void btnShowAirline_Click(object sender, RoutedEventArgs e)
        {
            this.SelectedAirline = (AirlinesMVVM)((Button)sender).Tag;

        }
        private void btnBuyAirline_Click(object sender, RoutedEventArgs e)
        {
            AirlinesMVVM airline = this.SelectedAirline;

            double buyingPrice = airline.Airline.getValue() * 100000 * 1.10;

            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2113"), string.Format(Translator.GetInstance().GetString("MessageBox", "2113", "message"), airline.Airline.Profile.Name, buyingPrice), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2114"), string.Format(Translator.GetInstance().GetString("MessageBox", "2114", "message"), airline.Airline.Profile.Name, buyingPrice), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    while (airline.Airline.Subsidiaries.Count > 0)
                    {
                        SubsidiaryAirline subAirline = airline.Airline.Subsidiaries[0];
                        subAirline.Profile.CEO = GameObject.GetInstance().HumanAirline.Profile.CEO;

                        subAirline.Airline = GameObject.GetInstance().HumanAirline;
                        airline.Airline.removeSubsidiaryAirline(subAirline);
                        GameObject.GetInstance().HumanAirline.addSubsidiaryAirline(subAirline);

                    }
                }
                else
                {
                    while (airline.Airline.Subsidiaries.Count > 0)
                    {
                        SubsidiaryAirline subAirline = airline.Airline.Subsidiaries[0];

                        subAirline.Airline = null;

                        airline.Airline.removeSubsidiaryAirline(subAirline);
                    }
                }
                if (airline.Airline.License > GameObject.GetInstance().HumanAirline.License)
                    GameObject.GetInstance().HumanAirline.License = airline.Airline.License;

                AirlineHelpers.SwitchAirline(airline.Airline, GameObject.GetInstance().HumanAirline);

                AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Airline_Expenses, -buyingPrice);

                Airlines.RemoveAirline(airline.Airline);

                PageNavigator.NavigateTo(new PageAirline(GameObject.GetInstance().HumanAirline));
            }
        }

        private void btnBuyAsSubsidiary_Click(object sender, RoutedEventArgs e)
        {
            AirlinesMVVM airline = this.SelectedAirline;

            double buyingPrice = airline.Airline.getValue() * 100000 * 1.10;

            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2113"), string.Format(Translator.GetInstance().GetString("MessageBox", "2113", "message"), airline.Airline.Profile.Name, buyingPrice), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                List<AirlineLogo> oldLogos = airline.Airline.Profile.Logos;
                string oldColor = airline.Airline.Profile.Color;

                //creates independent airlines for each subsidiary 
                while (airline.Airline.Subsidiaries.Count > 0)
                {
                    SubsidiaryAirline subAirline = airline.Airline.Subsidiaries[0];

                    subAirline.Airline = null;

                    airline.Airline.removeSubsidiaryAirline(subAirline);
                }

                if (airline.Airline.License > GameObject.GetInstance().HumanAirline.License)
                    GameObject.GetInstance().HumanAirline.License = airline.Airline.License;

                SubsidiaryAirline sAirline = new SubsidiaryAirline(GameObject.GetInstance().HumanAirline, airline.Airline.Profile, airline.Airline.Mentality, airline.Airline.MarketFocus, airline.Airline.License, airline.Airline.AirlineRouteFocus);

                AirlineHelpers.SwitchAirline(airline.Airline, sAirline);

                GameObject.GetInstance().HumanAirline.addSubsidiaryAirline(sAirline);

                AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Airline_Expenses, -buyingPrice);

                Airlines.RemoveAirline(airline.Airline);
                Airlines.AddAirline(sAirline);

                sAirline.Profile.Logos = oldLogos;
                sAirline.Profile.Color = oldColor;

                foreach (AirlinePolicy policy in airline.Airline.Policies)
                    sAirline.addAirlinePolicy(policy);

                sAirline.Money = airline.Airline.Money;
                sAirline.StartMoney = airline.Airline.Money;

                sAirline.Fees = new AirlineFees();

                PageNavigator.NavigateTo(new PageAirline(GameObject.GetInstance().HumanAirline));


            }
        }
        private void btnIssueShares_Click(object sender, RoutedEventArgs e)
        {
            int shares = Convert.ToInt32(slShares.Value);

            double price =  AirlineHelpers.GetPricePerAirlineShare(GameObject.GetInstance().HumanAirline);

            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2127"), string.Format(Translator.GetInstance().GetString("MessageBox", "2127", "message"), shares, new ValueCurrencyConverter().Convert(price)), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {


                AirlineHelpers.AddAirlineShares(GameObject.GetInstance().HumanAirline, shares,price);

                AirlinesMVVM humanAirline = this.AllAirlines.First(a => a.Airline == GameObject.GetInstance().HumanAirline);
                humanAirline.StocksForSale += shares;
                humanAirline.Stocks += shares;

                this.NumberOfSharesToIssue -= shares;

                humanAirline.setOwnershipValues();
            }

        }
        private void btnPurchaseShares_Click(object sender, RoutedEventArgs e)
        {
            AirlinesMVVM airline = this.SelectedAirline;

            ComboBox cbShares = new ComboBox();
            cbShares.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbShares.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbShares.Width = 200;

            int dValue = Convert.ToInt16(Convert.ToDouble(airline.StocksForSale) / 10);

            for (int i = 0; i <= airline.StocksForSale; i += dValue)
                cbShares.Items.Add(i);

            cbShares.SelectedIndex = 0;

            if (PopUpSingleElement.ShowPopUp(Translator.GetInstance().GetString("PageAirlinesShares", "1000"), cbShares) == PopUpSingleElement.ButtonSelected.OK && cbShares.SelectedItem != null)
            {
                int numberOfShares = Convert.ToInt32(cbShares.SelectedItem);

                double amount = numberOfShares * airline.StockPrice;

                AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Airline_Expenses, -amount);

                airline.addOwnership(GameObject.GetInstance().HumanAirline, numberOfShares);
            }

        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }




    }
}
