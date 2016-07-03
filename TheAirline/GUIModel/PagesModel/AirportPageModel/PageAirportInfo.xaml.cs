using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Helpers;
using TheAirline.Models.Airports;
using TheAirline.Models.General;
using TheAirline.Models.General.Environment;
using TheAirline.Models.General.Finances;

namespace TheAirline.GUIModel.PagesModel.AirportPageModel
{
    /// <summary>
    ///     Interaction logic for PageAirportInfo.xaml
    /// </summary>
    public partial class PageAirportInfo : Page
    {
        #region Constructors and Destructors

        public PageAirportInfo(AirportMVVM airport)
        {
            Airport = airport;
            DataContext = Airport;

            ContractTypes = new List<AirportContract.ContractType>();

            foreach (AirportContract.ContractType type in Enum.GetValues(typeof(AirportContract.ContractType)))
            {
                ContractTypes.Add(type);
            }

            InitializeComponent();

            rbTerminalType.IsChecked = true;
            /*
            CollectionView viewDemands = (CollectionView)CollectionViewSource.GetDefaultView(lvDemand.ItemsSource);
            viewDemands.GroupDescriptions.Clear();

            viewDemands.GroupDescriptions.Add(new PropertyGroupDescription("Type"));

            SortDescription sortTypeDescription = new SortDescription("Type", ListSortDirection.Ascending);
            viewDemands.SortDescriptions.Add(sortTypeDescription);

            SortDescription sortPassengersDescription = new SortDescription("Passengers", ListSortDirection.Descending);
            viewDemands.SortDescriptions.Add(sortPassengersDescription);
            */
        }

        #endregion

        #region Public Properties

        public AirportMVVM Airport { get; set; }

        public List<AirportContract.ContractType> ContractTypes { get; set; }

        #endregion

        #region Methods

        private void btnBuildTerminal_Click(object sender, RoutedEventArgs e)
        {
            int gates = Convert.ToInt16(slGates.Value);
            string name = txtName.Text.Trim();

            Terminal.TerminalType terminalType = rbTerminalType.IsChecked.Value ? Terminal.TerminalType.Passenger : Terminal.TerminalType.Cargo;

            // chs, 2011-01-11 changed so a message for confirmation are shown9han
            double price = gates * Airport.TerminalGatePrice + Airport.TerminalPrice;

            if (price > GameObject.GetInstance().HumanAirline.Money)
            {
                WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2205"),
                    Translator.GetInstance().GetString("MessageBox", "2205", "message"),
                    WPFMessageBoxButtons.Ok);
            }
            else
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2206"),
                    string.Format(Translator.GetInstance().GetString("MessageBox", "2206", "message"), gates, price),
                    WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    DateTime deliveryDate = GameObject.GetInstance()
                        .GameTime.Add(new TimeSpan(gates * 10 + 60, 0, 0, 0));

                    var terminal = new Terminal(
                        Airport.Airport,
                        GameObject.GetInstance().HumanAirline,
                        name,
                        gates,
                        deliveryDate,
                        terminalType);

                    Airport.addTerminal(terminal);

                    AirlineHelpers.AddAirlineInvoice(
                        GameObject.GetInstance().HumanAirline,
                        GameObject.GetInstance().GameTime,
                        Invoice.InvoiceType.Purchases,
                        -price);
                }
            }
        }

        private void btnBuyTerminal_Click(object sender, RoutedEventArgs e)
        {
            var terminal = (AirportTerminalMVVM)((Button)sender).Tag;

            long price = terminal.Gates * Airport.Airport.GetTerminalGatePrice()
                         + Airport.Airport.GetTerminalPrice();

            if (price > GameObject.GetInstance().HumanAirline.Money)
            {
                WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2205"),
                    Translator.GetInstance().GetString("MessageBox", "2205", "message"),
                    WPFMessageBoxButtons.Ok);
            }
            else
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2211"),
                    string.Format(Translator.GetInstance().GetString("MessageBox", "2211", "message"), new ValueCurrencyConverter().Convert(price)),
                    WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    Airport.purchaseTerminal(terminal, GameObject.GetInstance().HumanAirline);
                 
                    AirlineHelpers.AddAirlineInvoice(
                        GameObject.GetInstance().HumanAirline,
                        GameObject.GetInstance().GameTime,
                        Invoice.InvoiceType.Purchases,
                        -price);

                    
                }
            }
        }

        private void btnCreateHub_Click(object sender, RoutedEventArgs e)
        {
            HubType type = HubTypes.GetHubType(HubType.TypeOfHub.Hub); //(HubType)cbHubType.SelectedItem;

            if (AirportHelpers.GetHubPrice(Airport.Airport, type) > GameObject.GetInstance().HumanAirline.Money)
            {
                WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2212"),
                    Translator.GetInstance().GetString("MessageBox", "2212", "message"),
                    WPFMessageBoxButtons.Ok);
            }
            else
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2213"),
                    string.Format(
                        Translator.GetInstance().GetString("MessageBox", "2213", "message"),
                        AirportHelpers.GetHubPrice(Airport.Airport, type)),
                    WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    Airport.addHub(new Hub(GameObject.GetInstance().HumanAirline, type));

                    AirlineHelpers.AddAirlineInvoice(
                        GameObject.GetInstance().HumanAirline,
                        GameObject.GetInstance().GameTime,
                        Invoice.InvoiceType.Purchases,
                        -AirportHelpers.GetHubPrice(Airport.Airport, type));
                }
            }
        }

        private void btnExtendContract_Click(object sender, RoutedEventArgs e)
        {
            var tContract = (ContractMVVM)((Button)sender).Tag;

            var contract = new AirportContract(
                tContract.Contract.Airline,
                tContract.Contract.Airport,
                tContract.Contract.Type,
                tContract.Contract.TerminalType,
                tContract.Contract.ContractDate,
                tContract.Contract.NumberOfGates,
                tContract.Contract.Length,
                tContract.Contract.YearlyPayment,
                tContract.Contract.AutoRenew);

            object o = PopUpExtendContract.ShowPopUp(contract);

            if (o != null)
            {
                var nContract = (AirportContract)o;

                tContract.setNumberOfGates(nContract.NumberOfGates);
                tContract.setExpireDate(nContract.ExpireDate);
                tContract.Contract.AutoRenew = nContract.AutoRenew;
            }
        }

        private void btnRemoveContract_Click(object sender, RoutedEventArgs e)
        {
            var tContract = (ContractMVVM)((Button)sender).Tag;

            AirportContract contract = tContract.Contract;

            double penaltyFee = ((contract.YearlyPayment / 12) * contract.MonthsLeft) / 10;

            List<AirportContract> contracts =
                Airport.Airport.AirlineContracts.Where(
                    a => a.Airline == GameObject.GetInstance().HumanAirline && a != contract).ToList();

            if (
                !AirportHelpers.CanFillRoutesEntries(
                    Airport.Airport,
                    GameObject.GetInstance().HumanAirline,
                    contracts,
                    Weather.Season.AllYear))
            {
                WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2224"),
                    Translator.GetInstance().GetString("MessageBox", "2224", "message"),
                    WPFMessageBoxButtons.Ok);
            }
            else if (contract.Terminal != null)
            {
                WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2226"),
                    string.Format(
                        Translator.GetInstance().GetString("MessageBox", "2226", "message"),
                        contract.Terminal.Name),
                    WPFMessageBoxButtons.Ok);
            }
            else
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2225"),
                    string.Format(
                        Translator.GetInstance().GetString("MessageBox", "2225", "message"),
                        new ValueCurrencyConverter().Convert(penaltyFee)),
                    WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    AirlineHelpers.AddAirlineInvoice(
                        GameObject.GetInstance().HumanAirline,
                        GameObject.GetInstance().GameTime,
                        Invoice.InvoiceType.Purchases,
                        -penaltyFee);

                    Airport.removeAirlineContract(tContract);

                    for (int i = 0; i < contract.NumberOfGates; i++)
                    {
                        Gate gate =
                            Airport.Airport.Terminals.GetGates()
                                .Where(g => g.Airline == GameObject.GetInstance().HumanAirline)
                                .FirstOrDefault();

                        if (gate != null)
                            gate.Airline = null;
                    }
                }
            }
        }

        private void btnRemoveHub_Click(object sender, RoutedEventArgs e)
        {
            var hub = (Hub)((Button)sender).Tag;

            WPFMessageBoxResult result = WPFMessageBox.Show(
                Translator.GetInstance().GetString("MessageBox", "2227"),
                string.Format(
                    Translator.GetInstance().GetString("MessageBox", "2227", "message"),
                    Airport.Airport.Profile.Name),
                WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                Airport.removeHub(hub);
            }
        }

        private void btnSellTerminal_Click(object sender, RoutedEventArgs e)
        {
            var terminal = (AirportTerminalMVVM)((Button)sender).Tag;

            int totalRentedGates = Airport.Airport.AirlineContracts.Sum(c => c.NumberOfGates);

            Boolean isTerminalFree = Airport.Airport.Terminals.GetNumberOfGates(terminal.Terminal.Type) - terminal.Gates
                                     >= totalRentedGates; 
            if (isTerminalFree)
            {
                // chs, 2011-31-10 changed for the possibility of having delivered and non-delivered terminals
                long price = (terminal.Gates * Airport.Airport.GetTerminalGatePrice()
                       + Airport.Airport.GetTerminalPrice()) / 2;

                //string strRemove;
                if (terminal.DeliveryDate > GameObject.GetInstance().GameTime)
                {
                    WPFMessageBoxResult result = WPFMessageBox.Show(
                   Translator.GetInstance().GetString("MessageBox", "2207"),
                   Translator.GetInstance().GetString("MessageBox", "2207","message"),
                   WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                        Airport.removeTerminal(terminal);
                    }
                }
                else
                {
                    WPFMessageBoxResult result = WPFMessageBox.Show(
                  Translator.GetInstance().GetString("MessageBox", "2208"),
                  string.Format(Translator.GetInstance().GetString("MessageBox", "2208", "message"),terminal.Gates,new ValueCurrencyConverter().Convert(price)),
                  WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                       
                        AirlineHelpers.AddAirlineInvoice(
                         GameObject.GetInstance().HumanAirline,
                         GameObject.GetInstance().GameTime,
                         Invoice.InvoiceType.Purchases,
                         price);

                        var contract = Airport.Contracts.First(f=>f.Airline == terminal.Airline && f.NumberOfGates == terminal.Gates);
                                           
                        terminal.Airline = null;

                        contract.Contract.YearlyPayment = AirportHelpers.GetYearlyContractPayment(
              Airport.Airport,
              AirportContract.ContractType.Full,
              terminal.Gates,
              20);

        

                    }
                }
               

               
            }
            else
            {
                WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2209"),
                    Translator.GetInstance().GetString("MessageBox", "2209", "message"),
                    WPFMessageBoxButtons.Ok);
            }
        }

        private void btnSignContract_Click(object sender, RoutedEventArgs e)
        {
            Terminal.TerminalType terminalType = rbTerminalContractType.IsChecked.Value ? Terminal.TerminalType.Passenger : Terminal.TerminalType.Cargo;

            int gates = Convert.ToInt16(slContractGates.Value);
            int length = Convert.ToInt16(slContractLenght.Value);

            Boolean hasCheckin =
                Airport.Airport.GetAirportFacility(
                    GameObject.GetInstance().HumanAirline,
                    AirportFacility.FacilityType.CheckIn).TypeLevel > 0;
            var contractType = (AirportContract.ContractType)cbContractType.SelectedItem;

            Boolean autoRenew = cbAutoRenew.IsChecked.Value;

            double yearlyPayment = AirportHelpers.GetYearlyContractPayment(
                Airport.Airport,
                contractType,
                gates,
                length);

            Boolean payFull = length <= 2;

            var contract = new AirportContract(
                GameObject.GetInstance().HumanAirline,
                Airport.Airport,
                contractType,
                terminalType,
                GameObject.GetInstance().GameTime,
                gates,
                length,
                yearlyPayment,
                autoRenew,
                payFull);

            if (!hasCheckin && contractType == AirportContract.ContractType.Full)
            {
                AirportFacility checkinFacility =
                    AirportFacilities.GetFacilities(AirportFacility.FacilityType.CheckIn).Find(f => f.TypeLevel == 1);

                Airport.Airport.AddAirportFacility(
                    GameObject.GetInstance().HumanAirline,
                    checkinFacility,
                    GameObject.GetInstance().GameTime);
                AirlineHelpers.AddAirlineInvoice(
                    GameObject.GetInstance().HumanAirline,
                    GameObject.GetInstance().GameTime,
                    Invoice.InvoiceType.Purchases,
                    -checkinFacility.Price);
            }

            //25 % off if paying up front
            if (contract.PayFull && contractType == AirportContract.ContractType.Full)
            {
                double payment = (contract.YearlyPayment * contract.Length) * 0.75;
                AirlineHelpers.AddAirlineInvoice(
                    GameObject.GetInstance().HumanAirline,
                    GameObject.GetInstance().GameTime,
                    Invoice.InvoiceType.Rents,
                    -payment);
                contract.YearlyPayment = 0;
            }

            for (int i = 0; i < gates; i++)
            {
                Gate gate = Airport.Airport.Terminals.GetGates().Where(g => g.Airline == null).First();
                gate.Airline = GameObject.GetInstance().HumanAirline;
            }

            Airport.addAirlineContract(contract);
        }

        #endregion

        /*
        private void btnDemandContract_Click(object sender, RoutedEventArgs e)
        {
            DemandMVVM demand = (DemandMVVM)((Button)sender).Tag;

            Airport airport = demand.Destination;

            Boolean hasCheckin = airport.getAirportFacility(GameObject.GetInstance().HumanAirline, AirportFacility.FacilityType.CheckIn).TypeLevel > 0;

            int gates = Math.Min(2, airport.Terminals.NumberOfFreeGates);

            //WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2222"), string.Format(Translator.GetInstance().GetString("MessageBox", "2222", "message"), gates, airport.Profile.Name), WPFMessageBoxButtons.YesNo);
            object o = PopUpAirportContract.ShowPopUp(airport);

            if (o != null)
            {
                AirportContract.ContractType contractType = (AirportContract.ContractType)o;

                if (!hasCheckin && contractType == AirportContract.ContractType.Full)
                {
                    AirportFacility checkinFacility = AirportFacilities.GetFacilities(AirportFacility.FacilityType.CheckIn).Find(f => f.TypeLevel == 1);

                    airport.addAirportFacility(GameObject.GetInstance().HumanAirline, checkinFacility, GameObject.GetInstance().GameTime);
                    AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -checkinFacility.Price);

                }

                double yearlyPayment = AirportHelpers.GetYearlyContractPayment(airport,contractType, gates, 2);

                AirportContract contract = new AirportContract(GameObject.GetInstance().HumanAirline, airport, contractType, GameObject.GetInstance().GameTime, gates, 2, yearlyPayment,true);

                AirportHelpers.AddAirlineContract(contract);

                for (int i = 0; i < gates; i++)
                {
                    Gate gate = airport.Terminals.getGates().Where(g => g.Airline == null).First();
                    gate.Airline = GameObject.GetInstance().HumanAirline;
                }

                demand.Contracted = true;
            }
        }
        */
    }
}