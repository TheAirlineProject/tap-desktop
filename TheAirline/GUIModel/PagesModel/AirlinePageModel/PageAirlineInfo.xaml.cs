using TheAirline.Helpers;
using TheAirline.Models.Airliners;
using TheAirline.Models.Airlines;
using TheAirline.Models.Airlines.Subsidiary;
using TheAirline.Models.General;
using TheAirline.Models.General.Finances;
using TheAirline.Models.Pilots;
using TheAirline.Models.Routes;

namespace TheAirline.GUIModel.PagesModel.AirlinePageModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media.Imaging;

    using Microsoft.Win32;

    using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel;
    using System.Collections.ObjectModel;

    /// <summary>
    ///     Interaction logic for PageAirlineInfo.xaml
    /// </summary>
    public partial class PageAirlineInfo : Page
    {

        #region Constructors and Destructors

        public PageAirlineInfo(AirlineMVVM airline)
        {
            this.Airline = airline;
            this.DataContext = this.Airline;
            this.AirlineScores = new ObservableCollection<AirlineScoreMVVM>();
        
          
            this.AirlineScores.Add(
               new AirlineScoreMVVM(
                   Translator.GetInstance().GetString("PageAirlineRatings", "1012"),
                   this.Airline.Airline.OverallScore));
            this.AirlineScores.Add(
                new AirlineScoreMVVM(
                    Translator.GetInstance().GetString("PageAirlineRatings", "1014"),
                    (int)StatisticsHelpers.GetOnTimePercent(this.Airline.Airline)));
            this.AirlineScores.Add(
                new AirlineScoreMVVM(
                    Translator.GetInstance().GetString("PageAirlineRatings", "1015"),
                    Math.Max(0,(int)(StatisticsHelpers.GetAirlineFillAverage(this.Airline.Airline) * 100))));

            this.InitializeComponent();

          
        }

        #endregion

        #region Public Properties

        public AirlineMVVM Airline { get; set; }

        public ObservableCollection<AirlineScoreMVVM> AirlineScores { get; set; }

        #endregion

        #region Methods

        private void btnCreateSubsidiary_Click(object sender, RoutedEventArgs e)
        {
            Object o =  PopUpCreateSubsidiary.ShowPopUp(this.Airline.Airline);

            if (o != null)
            {
                SubsidiaryAirline subAirline = (SubsidiaryAirline)o;
                this.Airline.addSubsidiaryAirline(subAirline);
            }
            /*
            string iata = this.txtIATA.Text.ToUpper().Trim();
            string name = this.txtAirlineName.Text.Trim();
            var airport = (Airport)this.cbAirport.SelectedItem;
            string color = ((PropertyInfo)this.cbColor.SelectedItem).Name;
            Route.RouteType focus = this.rbPassengerType.IsChecked.Value
                ? Route.RouteType.Passenger
                : Route.RouteType.Cargo;

            string pattern = @"^[A-Za-z0-9]+$";
            var regex = new Regex(pattern);

            if (name.Length > 0 && iata.Length == 2 && regex.IsMatch(iata)
                && !Airlines.GetAllAirlines().Exists(a => a.Profile.IATACode == iata))
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2402"),
                    Translator.GetInstance().GetString("MessageBox", "2402", "message"),
                    WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    var profile = new AirlineProfile(
                        name,
                        iata,
                        color,
                        GameObject.GetInstance().MainAirline.Profile.CEO,
                        false,
                        GameObject.GetInstance().GameTime.Year,
                        2199);

                    profile.Country = airport.Profile.Country; //GameObject.GetInstance().MainAirline.Profile.Country;<

                    var subAirline = new SubsidiaryAirline(
                        GameObject.GetInstance().MainAirline,
                        profile,
                        Model.AirlineModel.Airline.AirlineMentality.Safe,
                        Model.AirlineModel.Airline.AirlineFocus.Local,
                        Model.AirlineModel.Airline.AirlineLicense.Domestic,
                        focus);
                    subAirline.addAirport(airport);
                    subAirline.Profile.Logos.Clear();
                    subAirline.Profile.AddLogo(new AirlineLogo(this.logoPath));
                    subAirline.Money = this.slMoney.Value;

                    this.Airline.addSubsidiaryAirline(subAirline);
                }
            }
            else
            {
                WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2404"),
                    Translator.GetInstance().GetString("MessageBox", "2404", "message"),
                    WPFMessageBoxButtons.Ok);
            }*/
        }

        private void btnDeleteSubsidiary_Click(object sender, RoutedEventArgs e)
        {
            var airline = (SubsidiaryAirline)((Button)sender).Tag;

            if (airline == GameObject.GetInstance().HumanAirline)
            {
                WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2112"),
                    string.Format(
                        Translator.GetInstance().GetString("MessageBox", "2112", "message"),
                        airline.Profile.Name),
                    WPFMessageBoxButtons.Ok);
            }
            else
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2111"),
                    string.Format(
                        Translator.GetInstance().GetString("MessageBox", "2111", "message"),
                        airline.Profile.Name),
                    WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    AirlineHelpers.CloseSubsidiaryAirline(airline);

                    this.Airline.removeSubsidiaryAirline(airline);
                }
            }
        }

       

        private void btnReleaseSubsidiary_Click(object sender, RoutedEventArgs e)
        {
            var airline = (SubsidiaryAirline)((Button)sender).Tag;

            if (airline == GameObject.GetInstance().HumanAirline)
            {
                WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2112"),
                    string.Format(
                        Translator.GetInstance().GetString("MessageBox", "2112", "message"),
                        airline.Profile.Name),
                    WPFMessageBoxButtons.Ok);
            }
            else
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2118"),
                    string.Format(
                        Translator.GetInstance().GetString("MessageBox", "2118", "message"),
                        airline.Profile.Name),
                    WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    AirlineHelpers.MakeSubsidiaryAirlineIndependent(airline);

                    this.Airline.removeSubsidiaryAirline(airline);
                }
            }
        }

        private void btnSellAirliner_Click(object sender, RoutedEventArgs e)
        {
            var airliner = (FleetAirliner)((Button)sender).Tag;

            if (airliner.Status != FleetAirliner.AirlinerStatus.Stopped)
            {
                WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2122"),
                    string.Format(Translator.GetInstance().GetString("MessageBox", "2122", "message")),
                    WPFMessageBoxButtons.Ok);
            }
            else
            {
                if (airliner.Purchased == FleetAirliner.PurchasedType.Bought)
                {
                    WPFMessageBoxResult result =
                        WPFMessageBox.Show(
                            Translator.GetInstance().GetString("MessageBox", "2106"),
                            string.Format(
                                Translator.GetInstance().GetString("MessageBox", "2106", "message"),
                                airliner.Name),
                            WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                        if (airliner.HasRoute)
                        {
                            var routes = new List<Route>(airliner.Routes);

                            foreach (Route route in routes)
                            {
                                airliner.RemoveRoute(route);
                            }
                        }

                        this.Airline.removeAirliner(airliner);

                        AirlineHelpers.AddAirlineInvoice(
                            this.Airline.Airline,
                            GameObject.GetInstance().GameTime,
                            Invoice.InvoiceType.Purchases,
                            airliner.Airliner.GetPrice());

                        foreach (Pilot pilot in airliner.Pilots)
                        {
                            pilot.Airliner = null;
                        }

                        airliner.Pilots.Clear();
                    }
                }
                else
                {
                    WPFMessageBoxResult result =
                        WPFMessageBox.Show(
                            Translator.GetInstance().GetString("MessageBox", "2107"),
                            string.Format(
                                Translator.GetInstance().GetString("MessageBox", "2107", "message"),
                                airliner.Name),
                            WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                        if (airliner.HasRoute)
                        {
                            var routes = new List<Route>(airliner.Routes);

                            foreach (Route route in routes)
                            {
                                airliner.RemoveRoute(route);
                            }
                        }

                        this.Airline.removeAirliner(airliner);

                        foreach (Pilot pilot in airliner.Pilots)
                        {
                            pilot.Airliner = null;
                        }

                        airliner.Pilots.Clear();
                    }
                }
            }
        }

        private void btnTransferFunds_Click(object sender, RoutedEventArgs e)
        {
            var airline = (Airline)this.cbTransferAirline.SelectedItem;
            string transferType = (this.cbTransferType.SelectedItem as ComboBoxItem).Content.ToString();

            double amount = this.slTransfer.Value;

            if (transferType == "From")
            {
                airline.Money -= amount;
                GameObject.GetInstance().AddHumanMoney(amount);
                this.Airline.setMaxTransferFunds(airline);
            }
            else
            {
                airline.Money += amount;
                GameObject.GetInstance().AddHumanMoney(-amount);
                this.Airline.setMaxTransferFunds(this.Airline.Airline);
            }
        }

        private void btnUpgradeLicens_Click(object sender, RoutedEventArgs e)
        {
            double upgradeLicensPrice = GeneralHelpers.GetInflationPrice(1000000);

            Airline.AirlineLicense nextLicenseType = this.Airline.License + 1;

            WPFMessageBoxResult result = WPFMessageBox.Show(
                Translator.GetInstance().GetString("MessageBox", "2119"),
                string.Format(
                    Translator.GetInstance().GetString("MessageBox", "2119", "message"),
                    new TextUnderscoreConverter().Convert(nextLicenseType),
                    new ValueCurrencyConverter().Convert(upgradeLicensPrice)),
                WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                this.Airline.License = nextLicenseType;
                this.Airline.Airline.License = nextLicenseType;

                AirlineHelpers.AddAirlineInvoice(
                    this.Airline.Airline,
                    GameObject.GetInstance().GameTime,
                    Invoice.InvoiceType.AirlineExpenses,
                    -upgradeLicensPrice);
            }
        }

        private void cbTransferType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.cbTransferType != null && this.cbTransferAirline != null
                && this.cbTransferAirline.SelectedItem != null)
            {
                var airline = (Airline)this.cbTransferAirline.SelectedItem;
                string transferType = (this.cbTransferType.SelectedItem as ComboBoxItem).Content.ToString();

                if (transferType == "From")
                {
                    this.Airline.setMaxTransferFunds(airline);
                }
                else
                {
                    this.Airline.setMaxTransferFunds(this.Airline.Airline);
                }
            }
        }

        private void imgAirline_Click(object sender, MouseButtonEventArgs e)
        {
            var airline = (Airline)((Image)sender).Tag;

            GameObject.GetInstance().SetHumanAirline(airline);
            PageNavigator.NavigateTo(new PageAirline(GameObject.GetInstance().HumanAirline));
        }

        #endregion
    }
}