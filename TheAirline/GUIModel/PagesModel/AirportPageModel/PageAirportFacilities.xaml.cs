using TheAirline.Model.GeneralModel.InvoicesModel;

namespace TheAirline.GUIModel.PagesModel.AirportPageModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
    using TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel;
    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.AirlineModel.AirlineCooperationModel;
    using TheAirline.Model.AirlinerModel.RouteModel;
    using TheAirline.Model.AirportModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.GeneralModel.Helpers;

    /// <summary>
    ///     Interaction logic for PageAirportFacilities.xaml
    /// </summary>
    public partial class PageAirportFacilities : Page
    {
        #region Constructors and Destructors

        public PageAirportFacilities(AirportMVVM airport)
        {
            this.Airport = airport;
            this.DataContext = this.Airport;

            this.FacilityTypes =
                Enum.GetValues(typeof(AirportFacility.FacilityType)).Cast<AirportFacility.FacilityType>().ToList();

            if (!airport.Airport.Terminals.AirportTerminals.Exists(t => t.Type == Terminal.TerminalType.Cargo))
                this.FacilityTypes.Remove(AirportFacility.FacilityType.Cargo);

            this.InitializeComponent();

            var view = (CollectionView)CollectionViewSource.GetDefaultView(this.lbFacilities.ItemsSource);
            view.SortDescriptions.Clear();

            var sortAirlineDescription = new SortDescription(
                "Facility.Airline.Profile.Name",
                ListSortDirection.Ascending);
            view.SortDescriptions.Add(sortAirlineDescription);

            var sortFacilityDescription = new SortDescription(
                "Facility.Facility.Shortname",
                ListSortDirection.Ascending);
            view.SortDescriptions.Add(sortFacilityDescription);
        }

        #endregion

        #region Public Properties

        public AirportMVVM Airport { get; set; }

        public List<AirportFacility.FacilityType> FacilityTypes { get; set; }

        #endregion

        #region Methods

        private void btnAddCooperation_Click(object sender, RoutedEventArgs e)
        {
            object o = PopUpAddCooperation.ShowPopUp(GameObject.GetInstance().HumanAirline, this.Airport.Airport);

            if (o != null && o is CooperationType)
            {
                var type = (CooperationType)o;

                if (type.Price < GameObject.GetInstance().HumanAirline.Money)
                {
                    var cooperation = new Cooperation(
                        type,
                        GameObject.GetInstance().HumanAirline,
                        GameObject.GetInstance().GameTime);

                    this.Airport.addCooperation(cooperation);
                }
                else
                {
                    WPFMessageBox.Show(
                        Translator.GetInstance().GetString("MessageBox", "2232"),
                        string.Format(Translator.GetInstance().GetString("MessageBox", "2232", "message")),
                        WPFMessageBoxButtons.Ok);
                }
            }
        }

        private void btnBuyFacility_Click(object sender, RoutedEventArgs e)
        {
            var facility = this.cbFacility.SelectedItem as AirportFacility;

            AirportFacility buildingFacility =
                this.Airport.Airport.GetAirlineBuildingFacility(GameObject.GetInstance().HumanAirline, facility.Type);

            if (facility.Price > GameObject.GetInstance().HumanAirline.Money)
            {
                WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2201"),
                    Translator.GetInstance().GetString("MessageBox", "2201", "message"),
                    WPFMessageBoxButtons.Ok);
            }
            else
            {
                if (buildingFacility == null)
                {
                    
                    double price = facility.Price;

                    if (this.Airport.Airport.Profile.Country != GameObject.GetInstance().HumanAirline.Profile.Country)
                    {
                        price = price * 1.25;
                    }

                    AirlineHelpers.AddAirlineInvoice(
                        GameObject.GetInstance().HumanAirline,
                        GameObject.GetInstance().GameTime,
                        Invoice.InvoiceType.Purchases,
                        -price);

                    this.Airport.addAirlineFacility(facility);

                    ICollectionView view = CollectionViewSource.GetDefaultView(this.cbFacility.ItemsSource);
                    view.Refresh();
                }
                else
                {
                    WPFMessageBox.Show(
                        Translator.GetInstance().GetString("MessageBox", "2233"),
                        Translator.GetInstance().GetString("MessageBox", "2233", "message"),
                        WPFMessageBoxButtons.Ok);
                }
            }
            /*
           AirportFacility.FacilityType type = (AirportFacility.FacilityType)cbNextFacility.SelectedItem;

           AirlineAirportFacility currentFacility = this.Airport.Airport.getAirlineAirportFacility(GameObject.GetInstance().HumanAirline, type);
            
           List<AirportFacility> facilities = AirportFacilities.GetFacilities(type);
           facilities = facilities.OrderBy(f => f.TypeLevel).ToList();

           int index = facilities.FindIndex(f => currentFacility.Facility == f);

           if (facilities.Count > index + 1)
           {
               AirportFacility facility = facilities[index + 1];
               
               if (facility.Price > GameObject.GetInstance().HumanAirline.Money)
                   WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2201"), Translator.GetInstance().GetString("MessageBox", "2201", "message"), WPFMessageBoxButtons.Ok);
               else
               {
                   WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2229"), string.Format(Translator.GetInstance().GetString("MessageBox", "2229", "message"), facility.Name, new ValueCurrencyConverter().Convert(facility.Price)), WPFMessageBoxButtons.YesNo);

                   if (result == WPFMessageBoxResult.Yes)
                   {
                       if (facility.Type == AirportFacility.FacilityType.Cargo && !GameObject.GetInstance().HumanAirline.Airports.Contains(this.Airport.Airport))
                           GameObject.GetInstance().HumanAirline.addAirport(this.Airport.Airport);

                       double price = facility.Price;

                       if (this.Airport.Airport.Profile.Country != GameObject.GetInstance().HumanAirline.Profile.Country)
                           price = price * 1.25;

                       AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -price);

                       this.Airport.addAirlineFacility(facility);


                       object o = cbNextFacility.SelectedItem;
                       cbNextFacility.SelectedIndex = cbNextFacility.SelectedIndex == cbNextFacility.Items.Count - 1 ? 0 : (cbNextFacility.SelectedIndex+1);

                       cbNextFacility.SelectedItem = o;

                   }
               }
           }
             * */
        }

        private void btnDeleteCooperation_Click(object sender, RoutedEventArgs e)
        {
            var cooperation = (Cooperation)((Button)sender).Tag;

            WPFMessageBoxResult result = WPFMessageBox.Show(
                Translator.GetInstance().GetString("MessageBox", "2231"),
                string.Format(
                    Translator.GetInstance().GetString("MessageBox", "2231", "message"),
                    cooperation.Type.Name),
                WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                this.Airport.removeCooperation(cooperation);
            }
        }

        private void btnDeleteFacility_Click(object sender, RoutedEventArgs e)
        {
            var facility = (AirlineAirportFacilityMVVM)((Button)sender).Tag;

            Boolean hasHub =
                this.Airport.Airport.GetHubs().Count(h => h.Airline == GameObject.GetInstance().HumanAirline) > 0;

            Boolean hasCargoRoute =
                GameObject.GetInstance()
                    .HumanAirline.Routes.Exists(
                        r =>
                            (r.Destination1 == this.Airport.Airport || r.Destination2 == this.Airport.Airport)
                            && r.Type == Route.RouteType.Cargo);
            Boolean airportHasCargoTerminal = this.Airport.Airport.GetCurrentAirportFacility(
                null,
                AirportFacility.FacilityType.Cargo) != null
                                              && this.Airport.Airport.GetCurrentAirportFacility(
                                                  null,
                                                  AirportFacility.FacilityType.Cargo).TypeLevel > 0;

            AirportContract contract =
                this.Airport.Contracts.Where(a => a.Airline == GameObject.GetInstance().HumanAirline) == null
                    ? null
                    : this.Airport.Contracts.Where(a => a.Airline == GameObject.GetInstance().HumanAirline)
                        .First()
                        .Contract;

            Boolean isMinimumServiceFacility = facility.Facility.Facility.TypeLevel == 1
                                               && facility.Facility.Facility.Type
                                               == AirportFacility.FacilityType.Service
                                               && this.Airport.Airport.HasAsHomebase(
                                                   GameObject.GetInstance().HumanAirline)
                                               && (contract == null
                                                   || contract.Type != AirportContract.ContractType.FullService);
            Boolean isMinimumHubFacility = facility.Facility.Facility.Type == AirportFacility.FacilityType.Service
                                           && hasHub && facility.Facility.Facility == Hub.MinimumServiceFacility
                                           && (contract == null || contract.Type == AirportContract.ContractType.Full
                                               || contract.Type != AirportContract.ContractType.MediumService);
            Boolean isMinimumCheckinFacility = facility.Facility.Facility.Type == AirportFacility.FacilityType.CheckIn
                                               && facility.Facility.Facility.TypeLevel == 1 && contract != null
                                               && contract.Type == AirportContract.ContractType.Full;

            if (isMinimumCheckinFacility)
            {
                WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2230"),
                    Translator.GetInstance().GetString("MessageBox", "2230", "message"),
                    WPFMessageBoxButtons.Ok);
            }
            else if (isMinimumServiceFacility)
            {
                WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2203"),
                    Translator.GetInstance().GetString("MessageBox", "2203", "message"),
                    WPFMessageBoxButtons.Ok);
            }
            else if (isMinimumHubFacility)
            {
                WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2214"),
                    string.Format(
                        Translator.GetInstance().GetString("MessageBox", "2214", "message"),
                        Hub.MinimumServiceFacility.Name),
                    WPFMessageBoxButtons.Ok);
            }
            else if (facility.Facility.Facility.Type == AirportFacility.FacilityType.Cargo
                     && facility.Facility.Facility.TypeLevel == 1 && hasCargoRoute && !airportHasCargoTerminal)
            {
                WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2223"),
                    Translator.GetInstance().GetString("MessageBox", "2223", "message"),
                    WPFMessageBoxButtons.Ok);
            }
            else
            {
                WPFMessageBoxResult result =
                    WPFMessageBox.Show(
                        Translator.GetInstance().GetString("MessageBox", "2204"),
                        string.Format(
                            Translator.GetInstance().GetString("MessageBox", "2204", "message"),
                            facility.Facility.Facility.Name),
                        WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    this.Airport.removeAirlineFacility(facility);
                }
            }
        }

        private void btnQuickUpgradeFacility_Click(object sender, RoutedEventArgs e)
        {
            var currentFacility = (AirlineAirportFacilityMVVM)((Button)sender).Tag;

            List<AirportFacility> facilities = AirportFacilities.GetFacilities(currentFacility.Facility.Facility.Type);
            facilities = facilities.OrderBy(f => f.TypeLevel).ToList();

            int index = facilities.FindIndex(f => currentFacility.Facility.Facility == f);

            AirportFacility facility = facilities[index + 1];

            if (facility.Price > GameObject.GetInstance().HumanAirline.Money)
            {
                WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2201"),
                    Translator.GetInstance().GetString("MessageBox", "2201", "message"),
                    WPFMessageBoxButtons.Ok);
            }
            else
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2202"),
                    string.Format(
                        Translator.GetInstance().GetString("MessageBox", "2202", "message"),
                        facility.Name,
                        new ValueCurrencyConverter().Convert(facility.Price)),
                    WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    double price = facility.Price;

                    if (this.Airport.Airport.Profile.Country != GameObject.GetInstance().HumanAirline.Profile.Country)
                    {
                        price = price * 1.25;
                    }

                    AirlineHelpers.AddAirlineInvoice(
                        GameObject.GetInstance().HumanAirline,
                        GameObject.GetInstance().GameTime,
                        Invoice.InvoiceType.Purchases,
                        -price);

                    this.Airport.addAirlineFacility(facility);
                }
            }
        }

        #endregion
    }
}