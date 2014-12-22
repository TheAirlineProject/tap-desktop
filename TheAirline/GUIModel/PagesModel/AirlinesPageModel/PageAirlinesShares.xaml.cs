namespace TheAirline.GUIModel.PagesModel.AirlinesPageModel
{
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
    using TheAirline.GUIModel.PagesModel.AirlinePageModel;
    using TheAirline.Model.AirlineModel;
    using TheAirline.Model.AirlineModel.SubsidiaryModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.GeneralModel.Helpers;

    /// <summary>
    ///     Interaction logic for PageAirlinesShares.xaml
    /// </summary>
    public partial class PageAirlinesShares : Page, INotifyPropertyChanged
    {
        #region Fields

        private int _numberofsharestoissue;

     
        #endregion

        #region Constructors and Destructors

        public PageAirlinesShares()
        {
            this.AllAirlines = new ObservableCollection<AirlinesMVVM>();
            foreach (
                Airline airline in
                    Airlines.GetAllAirlines().FindAll(a => !a.IsSubsidiary).OrderByDescending(a => a.IsHuman))
            {
                this.AllAirlines.Add(new AirlinesMVVM(airline));

                foreach (SubsidiaryAirline sAirline in airline.Subsidiaries)
                {
                    this.AllAirlines.Add(new AirlinesMVVM(sAirline));
                }
            }

            this.NumberOfSharesToIssue = 10000;

            this.InitializeComponent();
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
                return this._numberofsharestoissue;
            }
            set
            {
                this._numberofsharestoissue = value;
                this.NotifyPropertyChanged("NumberOfSharesToIssue");
            }
        }

       
        #endregion

        #region Methods

        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void btnBuyAirline_Click(object sender, RoutedEventArgs e)
        {
          
            AirlinesMVVM airline = (AirlinesMVVM)((Button)sender).Tag;

            int humanStocks = airline.Airline.Shares.Count(s => s.Airline == GameObject.GetInstance().HumanAirline);

            double humanStocksPercent = Convert.ToDouble(humanStocks) / Convert.ToDouble(airline.Airline.Shares.Count());

            double missingStocks = 1- humanStocks;

            double buyingPrice = airline.Airline.getValue() * 100000 * missingStocks;

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
                {
                    GameObject.GetInstance().HumanAirline.License = airline.Airline.License;
                }

                AirlineHelpers.SwitchAirline(airline.Airline, GameObject.GetInstance().HumanAirline);

                AirlineHelpers.AddAirlineInvoice(
                    GameObject.GetInstance().HumanAirline,
                    GameObject.GetInstance().GameTime,
                    Invoice.InvoiceType.Airline_Expenses,
                    -buyingPrice);

                Airlines.RemoveAirline(airline.Airline);

                PageNavigator.NavigateTo(new PageAirline(GameObject.GetInstance().HumanAirline));
            }
        }

        private void btnBuyAsSubsidiary_Click(object sender, RoutedEventArgs e)
        {
            AirlinesMVVM airline = (AirlinesMVVM)((Button)sender).Tag;

            double buyingPrice = airline.Airline.getValue() * 100000 * 1.10;

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

                    airline.Airline.removeSubsidiaryAirline(subAirline);
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
                    airline.Airline.AirlineRouteFocus,
                    airline.Airline.Schedule);

                AirlineHelpers.SwitchAirline(airline.Airline, sAirline);

                GameObject.GetInstance().HumanAirline.addSubsidiaryAirline(sAirline);

                AirlineHelpers.AddAirlineInvoice(
                    GameObject.GetInstance().HumanAirline,
                    GameObject.GetInstance().GameTime,
                    Invoice.InvoiceType.Airline_Expenses,
                    -buyingPrice);

                Airlines.RemoveAirline(airline.Airline);
                Airlines.AddAirline(sAirline);

                sAirline.Profile.Logos = oldLogos;
                sAirline.Profile.Color = oldColor;

                foreach (AirlinePolicy policy in airline.Airline.Policies)
                {
                    sAirline.addAirlinePolicy(policy);
                }

                sAirline.Money = airline.Airline.Money;
                sAirline.StartMoney = airline.Airline.Money;

                sAirline.Fees = new AirlineFees();
                AirlineHelpers.CreateStandardAirlineShares(sAirline);

                PageNavigator.NavigateTo(new PageAirline(GameObject.GetInstance().HumanAirline));
            }
        }

       

        private void btnPurchaseShares_Click(object sender, RoutedEventArgs e)
        {
            AirlinesMVVM airline = (AirlinesMVVM)((Button)sender).Tag;

            double buyingPrice = airline.StockPrice * 300000 * 1.10;

            WPFMessageBoxResult result = WPFMessageBox.Show(
              Translator.GetInstance().GetString("MessageBox", "2130"),
              string.Format(
                  Translator.GetInstance().GetString("MessageBox", "2130", "message"),
                  airline.Airline.Profile.Name,
                  new ValueCurrencyConverter().Convert(buyingPrice)),
              WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                airline.purchaseShares();

                AirlineHelpers.AddAirlineInvoice(
            GameObject.GetInstance().HumanAirline,
            GameObject.GetInstance().GameTime,
            Invoice.InvoiceType.Airline_Expenses,
            -buyingPrice);
            }
         
        }
        private void btnSellShares_Click(object sender, RoutedEventArgs e)
        {
            AirlinesMVVM airline = (AirlinesMVVM)((Button)sender).Tag;

            double sellingPrice = airline.StockPrice * 300000;

            WPFMessageBoxResult result = WPFMessageBox.Show(
               Translator.GetInstance().GetString("MessageBox", "2127"),
               string.Format(
                   Translator.GetInstance().GetString("MessageBox", "2127", "message"),
                   airline.Airline.Profile.Name,
                   new ValueCurrencyConverter().Convert(sellingPrice)),
               WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                airline.sellShares();

                AirlineHelpers.AddAirlineInvoice(
            GameObject.GetInstance().HumanAirline,
            GameObject.GetInstance().GameTime,
            Invoice.InvoiceType.Airline_Expenses,
            sellingPrice);
            }
        }

       

        #endregion
    }
}