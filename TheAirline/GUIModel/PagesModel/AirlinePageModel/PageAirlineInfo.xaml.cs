using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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
using TheAirline.GUIModel.PagesModel.FleetAirlinerPageModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlineModel.SubsidiaryModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.Model.PilotModel;

namespace TheAirline.GUIModel.PagesModel.AirlinePageModel
{
    /// <summary>
    /// Interaction logic for PageAirlineInfo.xaml
    /// </summary>
    public partial class PageAirlineInfo : Page
    {
        public AirlineMVVM Airline { get; set; }
        private string logoPath;
        public List<Airport> AllAirports { get; set; }
        public PageAirlineInfo(AirlineMVVM airline)
        {
            this.Airline = airline;
            this.DataContext = this.Airline;
            this.AllAirports = new List<Airport>();

            InitializeComponent();

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lvFleet.ItemsSource);
            view.GroupDescriptions.Clear();

            PropertyGroupDescription groupDescription = new PropertyGroupDescription("Purchased");
            view.GroupDescriptions.Add(groupDescription);

            logoPath = AppSettings.getDataPath() + "\\graphics\\airlinelogos\\default.png";
            imgLogo.Source = new BitmapImage(new Uri(logoPath, UriKind.RelativeOrAbsolute));

            foreach (Airport airport in this.Airline.Airline.Airports.FindAll(a => a.Terminals.getFreeSlotsPercent(this.Airline.Airline) > 50))
                this.AllAirports.Add(airport);

        }

        private void btnCreateSubsidiary_Click(object sender, RoutedEventArgs e)
        {
            string iata = txtIATA.Text.ToUpper().Trim();
            string name = txtAirlineName.Text.Trim();
            Airport airport = (Airport)cbAirport.SelectedItem;
            string color = ((PropertyInfo)cbColor.SelectedItem).Name;
            Route.RouteType focus = rbPassengerType.IsChecked.Value ? Route.RouteType.Passenger : Route.RouteType.Cargo;

            string pattern = @"^[A-Za-z0-9]+$";
            Regex regex = new Regex(pattern);

            if (name.Length > 0 && iata.Length == 2 && regex.IsMatch(iata) && !Airlines.GetAllAirlines().Exists(a => a.Profile.IATACode == iata))
            {

                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2402"), Translator.GetInstance().GetString("MessageBox", "2402", "message"), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    AirlineProfile profile = new AirlineProfile(name, iata, color, GameObject.GetInstance().MainAirline.Profile.CEO, false, GameObject.GetInstance().GameTime.Year, 2199);

                    profile.Country = GameObject.GetInstance().MainAirline.Profile.Country;

                    SubsidiaryAirline subAirline = new SubsidiaryAirline(GameObject.GetInstance().MainAirline, profile, Model.AirlineModel.Airline.AirlineMentality.Safe, Model.AirlineModel.Airline.AirlineFocus.Local, Model.AirlineModel.Airline.AirlineLicense.Domestic, focus);
                    subAirline.addAirport(airport);
                    subAirline.Profile.Logos.Clear();
                    subAirline.Profile.addLogo(new AirlineLogo(logoPath));
                    subAirline.Money = slMoney.Value;

                    this.Airline.addSubsidiaryAirline(subAirline);

                }


            }
            else
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2404"), Translator.GetInstance().GetString("MessageBox", "2404", "message"), WPFMessageBoxButtons.Ok);


        }

        private void btnReleaseSubsidiary_Click(object sender, RoutedEventArgs e)
        {
            SubsidiaryAirline airline = (SubsidiaryAirline)((Button)sender).Tag;

            if (airline == GameObject.GetInstance().HumanAirline)
            {
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2112"), string.Format(Translator.GetInstance().GetString("MessageBox", "2112", "message"), airline.Profile.Name), WPFMessageBoxButtons.Ok);
            }
            else
            {

                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2118"), string.Format(Translator.GetInstance().GetString("MessageBox", "2118", "message"), airline.Profile.Name), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {

                    AirlineHelpers.MakeSubsidiaryAirlineIndependent(airline);

                    this.Airline.removeSubsidiaryAirline(airline);

                }
            }
        }

        private void btnDeleteSubsidiary_Click(object sender, RoutedEventArgs e)
        {
            SubsidiaryAirline airline = (SubsidiaryAirline)((Button)sender).Tag;

            if (airline == GameObject.GetInstance().HumanAirline)
            {
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2112"), string.Format(Translator.GetInstance().GetString("MessageBox", "2112", "message"), airline.Profile.Name), WPFMessageBoxButtons.Ok);
            }
            else
            {

                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2111"), string.Format(Translator.GetInstance().GetString("MessageBox", "2111", "message"), airline.Profile.Name), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {

                    AirlineHelpers.CloseSubsidiaryAirline(airline);

                    this.Airline.removeSubsidiaryAirline(airline);

                }
            }
        }
        private void btnLogo_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".png";
            dlg.Filter = "Images (.png)|*.png";
            dlg.InitialDirectory = AppSettings.getDataPath() + "\\graphics\\airlinelogos\\";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                logoPath = dlg.FileName;
                imgLogo.Source = new BitmapImage(new Uri(logoPath, UriKind.RelativeOrAbsolute));

            }

        }
      
        private void imgAirline_Click(object sender, MouseButtonEventArgs e)
        {
            Airline airline = (Airline)((Image)sender).Tag;

            GameObject.GetInstance().setHumanAirline(airline);
            PageNavigator.NavigateTo(new PageAirline(GameObject.GetInstance().HumanAirline));

        }

        private void btnUpgradeLicens_Click(object sender, RoutedEventArgs e)
        {
            double upgradeLicensPrice = GeneralHelpers.GetInflationPrice(1000000);

            Airline.AirlineLicense nextLicenseType = this.Airline.License + 1;

            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2119"), string.Format(Translator.GetInstance().GetString("MessageBox", "2119", "message"), new TextUnderscoreConverter().Convert(nextLicenseType), new ValueCurrencyConverter().Convert(upgradeLicensPrice)), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                this.Airline.License = nextLicenseType;
                this.Airline.Airline.License = nextLicenseType;

                AirlineHelpers.AddAirlineInvoice(this.Airline.Airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Airline_Expenses, -upgradeLicensPrice);

            }
        }

        private void btnBuyAirline_Click(object sender, RoutedEventArgs e)
        {
            double buyingPrice = this.Airline.Airline.getValue() * 1000000 * 1.10;

            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2113"), string.Format(Translator.GetInstance().GetString("MessageBox", "2113", "message"), this.Airline.Airline.Profile.Name, buyingPrice), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2114"), string.Format(Translator.GetInstance().GetString("MessageBox", "2114", "message"), this.Airline.Airline.Profile.Name, buyingPrice), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    while (this.Airline.Subsidiaries.Count > 0)
                    {
                        SubsidiaryAirline subAirline = this.Airline.Subsidiaries[0];
                        subAirline.Profile.CEO = GameObject.GetInstance().HumanAirline.Profile.CEO;

                        subAirline.Airline = GameObject.GetInstance().HumanAirline;
                        this.Airline.removeSubsidiaryAirline(subAirline);
                        GameObject.GetInstance().HumanAirline.addSubsidiaryAirline(subAirline);

                    }
                }
                else
                {
                    while (this.Airline.Subsidiaries.Count > 0)
                    {
                        SubsidiaryAirline subAirline = this.Airline.Subsidiaries[0];

                        subAirline.Airline = null;

                        this.Airline.removeSubsidiaryAirline(subAirline);
                    }
                }
                if (this.Airline.License > GameObject.GetInstance().HumanAirline.License)
                    GameObject.GetInstance().HumanAirline.License = this.Airline.License;

                AirlineHelpers.SwitchAirline(this.Airline.Airline, GameObject.GetInstance().HumanAirline);

                AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Airline_Expenses, -buyingPrice);

                Airlines.RemoveAirline(this.Airline.Airline);

                PageNavigator.NavigateTo(new PageAirline(GameObject.GetInstance().HumanAirline));
            }
        }

        private void btnBuyAsSubsidiary_Click(object sender, RoutedEventArgs e)
        {
            double buyingPrice = this.Airline.Airline.getValue() * 1000000 * 1.10;

            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2113"), string.Format(Translator.GetInstance().GetString("MessageBox", "2113", "message"), this.Airline.Airline.Profile.Name, buyingPrice), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                List<AirlineLogo> oldLogos = this.Airline.Airline.Profile.Logos;
                string oldColor = this.Airline.Airline.Profile.Color;

                //creates independent airlines for each subsidiary 
                while (this.Airline.Subsidiaries.Count > 0)
                {
                    SubsidiaryAirline subAirline = this.Airline.Subsidiaries[0];

                    subAirline.Airline = null;

                    this.Airline.removeSubsidiaryAirline(subAirline);
                }

                if (this.Airline.License > GameObject.GetInstance().HumanAirline.License)
                    GameObject.GetInstance().HumanAirline.License = this.Airline.License;

                SubsidiaryAirline sAirline = new SubsidiaryAirline(GameObject.GetInstance().HumanAirline, this.Airline.Airline.Profile, this.Airline.Airline.Mentality, this.Airline.Airline.MarketFocus, this.Airline.License, this.Airline.Airline.AirlineRouteFocus);

                AirlineHelpers.SwitchAirline(this.Airline.Airline, sAirline);

                GameObject.GetInstance().HumanAirline.addSubsidiaryAirline(sAirline);

                AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Airline_Expenses, -buyingPrice);

                Airlines.RemoveAirline(this.Airline.Airline);
                Airlines.AddAirline(sAirline);

                sAirline.Profile.Logos = oldLogos;
                sAirline.Profile.Color = oldColor;

                foreach (AirlinePolicy policy in this.Airline.Airline.Policies)
                    sAirline.addAirlinePolicy(policy);

                sAirline.Money = this.Airline.Money;
                sAirline.StartMoney = this.Airline.Money;

                sAirline.Fees = new AirlineFees();

                PageNavigator.NavigateTo(new PageAirline(GameObject.GetInstance().HumanAirline));


            }
        }

        private void btnSellAirliner_Click(object sender, RoutedEventArgs e)
        {
            FleetAirliner airliner = (FleetAirliner)((Button)sender).Tag;

            if (airliner.Status != FleetAirliner.AirlinerStatus.Stopped)
            {
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2122"), string.Format(Translator.GetInstance().GetString("MessageBox", "2122", "message")), WPFMessageBoxButtons.Ok);
            }
            else
            {
                if (airliner.Purchased == FleetAirliner.PurchasedType.Bought)
                {
                    WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2106"), string.Format(Translator.GetInstance().GetString("MessageBox", "2106", "message"), airliner.Name), WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                        if (airliner.HasRoute)
                        {
                            var routes = new List<Route>(airliner.Routes);

                            foreach (Route route in routes)
                                airliner.removeRoute(route);
                        }

                        this.Airline.removeAirliner(airliner);

                        AirlineHelpers.AddAirlineInvoice(this.Airline.Airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, airliner.Airliner.getPrice());


                        foreach (Pilot pilot in airliner.Pilots)
                            pilot.Airliner = null;

                        airliner.Pilots.Clear();


                    }
                }
                else
                {
                    WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2107"), string.Format(Translator.GetInstance().GetString("MessageBox", "2107", "message"), airliner.Name), WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                        if (airliner.HasRoute)
                        {
                            var routes = new List<Route>(airliner.Routes);

                            foreach (Route route in routes)
                                airliner.removeRoute(route);
                        }


                        this.Airline.removeAirliner(airliner);

                        foreach (Pilot pilot in airliner.Pilots)
                            pilot.Airliner = null;

                        airliner.Pilots.Clear();

                    }
                }

            }

        }

        private void cbTransferType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbTransferType != null && cbTransferAirline != null && cbTransferAirline.SelectedItem != null)
            {
                Airline airline = (Airline)cbTransferAirline.SelectedItem;
                string transferType = (cbTransferType.SelectedItem as ComboBoxItem).Content.ToString();

                if (transferType == "From")
                    this.Airline.setMaxTransferFunds(airline);
                else
                    this.Airline.setMaxTransferFunds(this.Airline.Airline);
            }
        }

        private void btnTransferFunds_Click(object sender, RoutedEventArgs e)
        {
            Airline airline = (Airline)cbTransferAirline.SelectedItem;
            string transferType = (cbTransferType.SelectedItem as ComboBoxItem).Content.ToString();

            double amount = slTransfer.Value;


            if (transferType == "From")
            {
                airline.Money -= amount;
                GameObject.GetInstance().addHumanMoney(amount);
                this.Airline.setMaxTransferFunds(airline);
            }
            else
            {
                airline.Money += amount;
                GameObject.GetInstance().addHumanMoney(-amount);
                this.Airline.setMaxTransferFunds(this.Airline.Airline);
                
            }


        }
    }
}
