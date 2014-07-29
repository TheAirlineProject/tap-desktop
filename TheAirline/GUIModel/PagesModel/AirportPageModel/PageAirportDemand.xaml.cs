namespace TheAirline.GUIModel.PagesModel.AirportPageModel
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel;
    using TheAirline.Model.AirlinerModel.RouteModel;
    using TheAirline.Model.AirportModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.GeneralModel.Helpers;

    /// <summary>
    ///     Interaction logic for PageAirportDemand.xaml
    /// </summary>
    public partial class PageAirportDemand : Page
    {
        #region Fields

        private AirportMVVM Airport;

        #endregion

        #region Constructors and Destructors

        public PageAirportDemand(AirportMVVM airport)
        {
            this.Airport = airport;
            this.DataContext = this.Airport;

            this.InitializeComponent();

            var viewDemands = (CollectionView)CollectionViewSource.GetDefaultView(this.lvDemand.ItemsSource);
            viewDemands.GroupDescriptions.Clear();

            viewDemands.GroupDescriptions.Add(new PropertyGroupDescription("Type"));

            var sortTypeDescription = new SortDescription("Type", ListSortDirection.Ascending);
            viewDemands.SortDescriptions.Add(sortTypeDescription);

            var sortPassengersDescription = new SortDescription("Passengers", ListSortDirection.Descending);
            viewDemands.SortDescriptions.Add(sortPassengersDescription);
        }

        #endregion

        #region Methods

        private void btnDemandContract_Click(object sender, RoutedEventArgs e)
        {
            var demand = (DemandMVVM)((Button)sender).Tag;

            Airport airport = demand.Destination;

            Route.RouteType airlineFocus = GameObject.GetInstance().HumanAirline.AirlineRouteFocus;

            Boolean hasCargo = 
                airport.getAirportFacility(GameObject.GetInstance().HumanAirline,AirportFacility.FacilityType.Cargo,true)
                .TypeLevel > 0;

            Boolean hasCheckin =
                airport.getAirportFacility(GameObject.GetInstance().HumanAirline, AirportFacility.FacilityType.CheckIn)
                    .TypeLevel > 0;

            int paxGates = Math.Min(2, airport.Terminals.getFreeGates(Terminal.TerminalType.Passenger));
            int cargoGates = Math.Min(2,airport.Terminals.getFreeGates(Terminal.TerminalType.Cargo));

            //WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2222"), string.Format(Translator.GetInstance().GetString("MessageBox", "2222", "message"), gates, airport.Profile.Name), WPFMessageBoxButtons.YesNo);
            object o = PopUpAirportContract.ShowPopUp(airport);

            if (o != null)
            {
                var contractType = (AirportContract.ContractType)o;

                Terminal.TerminalType terminalType;

                int gates;

                if (airlineFocus == Route.RouteType.Cargo)
                {
                    if (!hasCargo && contractType == AirportContract.ContractType.Full)
                    {
                        AirportFacility cargoFacility =
                            AirportFacilities.GetFacilities(AirportFacility.FacilityType.Cargo)
                                .Find(f => f.TypeLevel == 1);

                        airport.addAirportFacility(
                            GameObject.GetInstance().HumanAirline,
                            cargoFacility,
                            GameObject.GetInstance().GameTime);
                        AirlineHelpers.AddAirlineInvoice(
                            GameObject.GetInstance().HumanAirline,
                            GameObject.GetInstance().GameTime,
                            Invoice.InvoiceType.Purchases,
                            -cargoFacility.Price);
                    }

                    terminalType = Terminal.TerminalType.Cargo;
                    gates = cargoGates;
                }
                else
                {

                    if (!hasCheckin && contractType == AirportContract.ContractType.Full)
                    {
                        AirportFacility checkinFacility =
                            AirportFacilities.GetFacilities(AirportFacility.FacilityType.CheckIn)
                                .Find(f => f.TypeLevel == 1);

                        airport.addAirportFacility(
                            GameObject.GetInstance().HumanAirline,
                            checkinFacility,
                            GameObject.GetInstance().GameTime);
                        AirlineHelpers.AddAirlineInvoice(
                            GameObject.GetInstance().HumanAirline,
                            GameObject.GetInstance().GameTime,
                            Invoice.InvoiceType.Purchases,
                            -checkinFacility.Price);
                    }

                    terminalType = Terminal.TerminalType.Passenger;
                    gates = paxGates;

                
                }

                double yearlyPayment = AirportHelpers.GetYearlyContractPayment(airport, contractType, gates, 2);

                var contract = new AirportContract(
                    GameObject.GetInstance().HumanAirline,
                    airport,
                    contractType,
                    terminalType,
                    GameObject.GetInstance().GameTime,
                    gates,
                    2,
                    yearlyPayment,
                    true);

                AirportHelpers.AddAirlineContract(contract);

                for (int i = 0; i < gates; i++)
                {
                    Gate gate = airport.Terminals.getGates().Where(g => g.Airline == null).First();
                    gate.Airline = GameObject.GetInstance().HumanAirline;
                }

                demand.Contracted = true;
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = ((TextBox)e.Source).Text.ToUpper();

            var source = this.lvDemand.Items as ICollectionView;
            source.Filter = o =>
            {
                if (o != null)
                {
                    var a = o as DemandMVVM;
                    return a != null && a.Destination.Profile.IATACode.ToUpper().StartsWith(searchText)
                           || a.Destination.Profile.ICAOCode.ToUpper().StartsWith(searchText)
                           || a.Destination.Profile.Name.ToUpper().StartsWith(searchText);
                }
                return false;
            };
        }

        #endregion
    }
}