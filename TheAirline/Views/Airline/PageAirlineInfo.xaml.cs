using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Helpers;
using TheAirline.Models.Airliners;
using TheAirline.Models.Airlines;
using TheAirline.Models.Airlines.Subsidiary;
using TheAirline.Models.General;
using TheAirline.Models.General.Finances;
using TheAirline.Models.Pilots;
using TheAirline.Models.Routes;
using TheAirline.ViewModels.Airline;
using TheAirline.Views.Airline;

namespace TheAirline.GUIModel.PagesModel.AirlinePageModel
{
    /// <summary>
    ///     Interaction logic for PageAirlineInfo.xaml
    /// </summary>
    public partial class PageAirlineInfo : Page
    {

        #region Constructors and Destructors

        public PageAirlineInfo(AirlineMVVM airline)
        {
            Airline = airline;
            DataContext = Airline;
            AirlineScores = new ObservableCollection<AirlineScoreMVVM>();
        
          
            AirlineScores.Add(
               new AirlineScoreMVVM(
                   Translator.GetInstance().GetString("PageAirlineRatings", "1012"),
                   Airline.Airline.OverallScore));
            AirlineScores.Add(
                new AirlineScoreMVVM(
                    Translator.GetInstance().GetString("PageAirlineRatings", "1014"),
                    (int)StatisticsHelpers.GetOnTimePercent(Airline.Airline)));
            AirlineScores.Add(
                new AirlineScoreMVVM(
                    Translator.GetInstance().GetString("PageAirlineRatings", "1015"),
                    Math.Max(0,(int)(StatisticsHelpers.GetAirlineFillAverage(Airline.Airline) * 100))));

            InitializeComponent();

          
        }

        #endregion

        #region Public Properties

        public AirlineMVVM Airline { get; set; }

        public ObservableCollection<AirlineScoreMVVM> AirlineScores { get; set; }

        #endregion

        #region Methods

        private void btnCreateSubsidiary_Click(object sender, RoutedEventArgs e)
        {
            Object o =  PopUpCreateSubsidiary.ShowPopUp(Airline.Airline);

            if (o != null)
            {
                SubsidiaryAirline subAirline = (SubsidiaryAirline)o;
                Airline.addSubsidiaryAirline(subAirline);
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

                    Airline.removeSubsidiaryAirline(airline);
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

                    Airline.removeSubsidiaryAirline(airline);
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

                        Airline.removeAirliner(airliner);

                        AirlineHelpers.AddAirlineInvoice(
                            Airline.Airline,
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

                        Airline.removeAirliner(airliner);

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
            var airline = (Airline)cbTransferAirline.SelectedItem;
            string transferType = (cbTransferType.SelectedItem as ComboBoxItem).Content.ToString();

            double amount = slTransfer.Value;

            if (transferType == "From")
            {
                airline.Money -= amount;
                GameObject.GetInstance().AddHumanMoney(amount);
                Airline.setMaxTransferFunds(airline);
            }
            else
            {
                airline.Money += amount;
                GameObject.GetInstance().AddHumanMoney(-amount);
                Airline.setMaxTransferFunds(Airline.Airline);
            }
        }

        private void btnUpgradeLicens_Click(object sender, RoutedEventArgs e)
        {
            double upgradeLicensPrice = GeneralHelpers.GetInflationPrice(1000000);

            Airline.AirlineLicense nextLicenseType = Airline.License + 1;

            WPFMessageBoxResult result = WPFMessageBox.Show(
                Translator.GetInstance().GetString("MessageBox", "2119"),
                string.Format(
                    Translator.GetInstance().GetString("MessageBox", "2119", "message"),
                    new TextUnderscoreConverter().Convert(nextLicenseType),
                    new ValueCurrencyConverter().Convert(upgradeLicensPrice)),
                WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                Airline.License = nextLicenseType;
                Airline.Airline.License = nextLicenseType;

                AirlineHelpers.AddAirlineInvoice(
                    Airline.Airline,
                    GameObject.GetInstance().GameTime,
                    Invoice.InvoiceType.AirlineExpenses,
                    -upgradeLicensPrice);
            }
        }

        private void cbTransferType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbTransferType != null && cbTransferAirline != null
                && cbTransferAirline.SelectedItem != null)
            {
                var airline = (Airline)cbTransferAirline.SelectedItem;
                string transferType = (cbTransferType.SelectedItem as ComboBoxItem).Content.ToString();

                if (transferType == "From")
                {
                    Airline.setMaxTransferFunds(airline);
                }
                else
                {
                    Airline.setMaxTransferFunds(Airline.Airline);
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