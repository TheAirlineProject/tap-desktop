using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Helpers;
using TheAirline.Models.Airlines;
using TheAirline.Models.Airlines.Subsidiary;
using TheAirline.Models.General;
using TheAirline.Models.General.Finances;
using TheAirline.Views.Airline;

namespace TheAirline.GUIModel.PagesModel.AirlinesPageModel
{
    /// <summary>
    ///     Interaction logic for PageAirlinesShares.xaml
    /// </summary>
    public partial class PageAirlinesShares : Page, INotifyPropertyChanged
    {
        #region Fields

        private int _numberofsharestoissue;

        private AirlinesMVVM _selectedairline;

        #endregion

        #region Constructors and Destructors

        public PageAirlinesShares()
        {
            AllAirlines = new ObservableCollection<AirlinesMVVM>();
            foreach (
                Airline airline in
                    Airlines.GetAllAirlines().FindAll(a => !a.IsSubsidiary).OrderByDescending(a => a.IsHuman))
            {
                AllAirlines.Add(new AirlinesMVVM(airline));

                foreach (SubsidiaryAirline sAirline in airline.Subsidiaries)
                {
                    AllAirlines.Add(new AirlinesMVVM(sAirline));
                }
            }

            NumberOfSharesToIssue = 10000;

            InitializeComponent();
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public ObservableCollection<AirlinesMVVM> AllAirlines { get; set; }

        public int NumberOfSharesToIssue
        {
            get
            {
                return _numberofsharestoissue;
            }
            set
            {
                _numberofsharestoissue = value;
                NotifyPropertyChanged("NumberOfSharesToIssue");
            }
        }

        public AirlinesMVVM SelectedAirline
        {
            get
            {
                return _selectedairline;
            }
            set
            {
                _selectedairline = value;
                NotifyPropertyChanged("SelectedAirline");
            }
        }

        #endregion

        #region Methods

        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void btnBuyAirline_Click(object sender, RoutedEventArgs e)
        {
            AirlinesMVVM airline = SelectedAirline;

            double buyingPrice = airline.Airline.GetValue() * 100000 * 1.10;

            WPFMessageBoxResult result = WPFMessageBox.Show(
                Translator.GetInstance().GetString("MessageBox", "2113"),
                string.Format(
                    Translator.GetInstance().GetString("MessageBox", "2113", "message"),
                    airline.Airline.Profile.Name,
                    buyingPrice),
                WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                result = WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2114"),
                    string.Format(
                        Translator.GetInstance().GetString("MessageBox", "2114", "message"),
                        airline.Airline.Profile.Name,
                        buyingPrice),
                    WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    while (airline.Airline.Subsidiaries.Count > 0)
                    {
                        SubsidiaryAirline subAirline = airline.Airline.Subsidiaries[0];
                        subAirline.Profile.CEO = GameObject.GetInstance().HumanAirline.Profile.CEO;

                        subAirline.Airline = GameObject.GetInstance().HumanAirline;
                        airline.Airline.RemoveSubsidiaryAirline(subAirline);
                        GameObject.GetInstance().HumanAirline.AddSubsidiaryAirline(subAirline);
                    }
                }
                else
                {
                    while (airline.Airline.Subsidiaries.Count > 0)
                    {
                        SubsidiaryAirline subAirline = airline.Airline.Subsidiaries[0];

                        subAirline.Airline = null;

                        airline.Airline.RemoveSubsidiaryAirline(subAirline);
                    }
                }
                if (airline.Airline.License > GameObject.GetInstance().HumanAirline.License)
                {
                    GameObject.GetInstance().HumanAirline.License = airline.Airline.License;
                }

                AirlineHelpers.SwitchAirline(airline.Airline, GameObject.GetInstance().HumanAirline);

                AirlineHelpers.AddAirlineInvoice(
                    GameObject.GetInstance().HumanAirline,
                    GameObject.GetInstance().GameTime,
                    Invoice.InvoiceType.AirlineExpenses,
                    -buyingPrice);

                Airlines.RemoveAirline(airline.Airline);

                PageNavigator.NavigateTo(new PageAirline(GameObject.GetInstance().HumanAirline));
            }
        }

        private void btnBuyAsSubsidiary_Click(object sender, RoutedEventArgs e)
        {
            AirlinesMVVM airline = SelectedAirline;

            double buyingPrice = airline.Airline.GetValue() * 100000 * 1.10;

            WPFMessageBoxResult result = WPFMessageBox.Show(
                Translator.GetInstance().GetString("MessageBox", "2113"),
                string.Format(
                    Translator.GetInstance().GetString("MessageBox", "2113", "message"),
                    airline.Airline.Profile.Name,
                    buyingPrice),
                WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                List<AirlineLogo> oldLogos = airline.Airline.Profile.Logos;
                string oldColor = airline.Airline.Profile.Color;

                //creates independent airlines for each subsidiary 
                while (airline.Airline.Subsidiaries.Count > 0)
                {
                    SubsidiaryAirline subAirline = airline.Airline.Subsidiaries[0];

                    subAirline.Airline = null;

                    airline.Airline.RemoveSubsidiaryAirline(subAirline);
                }

                if (airline.Airline.License > GameObject.GetInstance().HumanAirline.License)
                {
                    GameObject.GetInstance().HumanAirline.License = airline.Airline.License;
                }

                var sAirline = new SubsidiaryAirline(
                    GameObject.GetInstance().HumanAirline,
                    airline.Airline.Profile,
                    airline.Airline.Mentality,
                    airline.Airline.MarketFocus,
                    airline.Airline.License,
                    airline.Airline.AirlineRouteFocus);

                AirlineHelpers.SwitchAirline(airline.Airline, sAirline);

                GameObject.GetInstance().HumanAirline.AddSubsidiaryAirline(sAirline);

                AirlineHelpers.AddAirlineInvoice(
                    GameObject.GetInstance().HumanAirline,
                    GameObject.GetInstance().GameTime,
                    Invoice.InvoiceType.AirlineExpenses,
                    -buyingPrice);

                Airlines.RemoveAirline(airline.Airline);
                Airlines.AddAirline(sAirline);

                sAirline.Profile.Logos = oldLogos;
                sAirline.Profile.Color = oldColor;

                foreach (AirlinePolicy policy in airline.Airline.Policies)
                {
                    sAirline.AddAirlinePolicy(policy);
                }

                sAirline.Money = airline.Airline.Money;
                sAirline.StartMoney = airline.Airline.Money;

                sAirline.Fees = new AirlineFees();

                PageNavigator.NavigateTo(new PageAirline(GameObject.GetInstance().HumanAirline));
            }
        }

        private void btnIssueShares_Click(object sender, RoutedEventArgs e)
        {
            int shares = Convert.ToInt32(slShares.Value);

            double price = AirlineHelpers.GetPricePerAirlineShare(GameObject.GetInstance().HumanAirline);

            WPFMessageBoxResult result = WPFMessageBox.Show(
                Translator.GetInstance().GetString("MessageBox", "2127"),
                string.Format(
                    Translator.GetInstance().GetString("MessageBox", "2127", "message"),
                    shares,
                    new ValueCurrencyConverter().Convert(price)),
                WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                AirlineHelpers.AddAirlineShares(GameObject.GetInstance().HumanAirline, shares, price);

                AirlinesMVVM humanAirline =
                    AllAirlines.First(a => a.Airline == GameObject.GetInstance().HumanAirline);
                humanAirline.StocksForSale += shares;
                humanAirline.Stocks += shares;

                NumberOfSharesToIssue -= shares;

                humanAirline.setOwnershipValues();
            }
        }

        private void btnPurchaseShares_Click(object sender, RoutedEventArgs e)
        {
            AirlinesMVVM airline = SelectedAirline;

            var cbShares = new ComboBox();
            cbShares.SetResourceReference(StyleProperty, "ComboBoxTransparentStyle");
            cbShares.HorizontalAlignment = HorizontalAlignment.Left;
            cbShares.Width = 200;

            int dValue = Convert.ToInt16(Convert.ToDouble(airline.StocksForSale) / 10);

            for (int i = 0; i <= airline.StocksForSale; i += dValue)
            {
                cbShares.Items.Add(i);
            }

            cbShares.SelectedIndex = 0;

            if (PopUpSingleElement.ShowPopUp(Translator.GetInstance().GetString("PageAirlinesShares", "1000"), cbShares)
                == PopUpSingleElement.ButtonSelected.OK && cbShares.SelectedItem != null)
            {
                int numberOfShares = Convert.ToInt32(cbShares.SelectedItem);

                double amount = numberOfShares * airline.StockPrice;

                AirlineHelpers.AddAirlineInvoice(
                    GameObject.GetInstance().HumanAirline,
                    GameObject.GetInstance().GameTime,
                    Invoice.InvoiceType.AirlineExpenses,
                    -amount);

                airline.addOwnership(GameObject.GetInstance().HumanAirline, numberOfShares);
            }
        }

        private void btnShowAirline_Click(object sender, RoutedEventArgs e)
        {
            SelectedAirline = (AirlinesMVVM)((Button)sender).Tag;
        }

        #endregion
    }
}