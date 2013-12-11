using System;
using System.Collections.Generic;
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
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GUIModel.PagesModel.AirportPageModel
{
    /// <summary>
    /// Interaction logic for PageAirportInfo.xaml
    /// </summary>
    public partial class PageAirportInfo : Page
    {
        public AirportMVVM Airport { get; set; }
        public PageAirportInfo(AirportMVVM airport)
        {
            this.Airport = airport;
            this.DataContext = this.Airport;

            InitializeComponent();

            CollectionView viewTerminals = (CollectionView)CollectionViewSource.GetDefaultView(lvTerminals.ItemsSource);
            viewTerminals.GroupDescriptions.Clear();

            PropertyGroupDescription groupDescription = new PropertyGroupDescription("Type");
            viewTerminals.GroupDescriptions.Add(groupDescription);



            CollectionView viewDemands = (CollectionView)CollectionViewSource.GetDefaultView(lvDemand.ItemsSource);
            viewDemands.GroupDescriptions.Clear();

            viewDemands.GroupDescriptions.Add(new PropertyGroupDescription("Type"));
           
            SortDescription sortTypeDescription = new SortDescription("Type", ListSortDirection.Ascending);
            viewDemands.SortDescriptions.Add(sortTypeDescription);

            SortDescription sortPassengersDescription = new SortDescription("Passengers", ListSortDirection.Descending);
            viewDemands.SortDescriptions.Add(sortPassengersDescription);



        }
        private void btnSignContract_Click(object sender, RoutedEventArgs e)
        {
            int gates = Convert.ToInt16(slContractGates.Value);
            int length = Convert.ToInt16(slContractLenght.Value);

            Boolean hasCheckin = this.Airport.Airport.getAirportFacility(GameObject.GetInstance().HumanAirline, AirportFacility.FacilityType.CheckIn).TypeLevel > 0;
            double yearlyPayment = AirportHelpers.GetYearlyContractPayment(this.Airport.Airport, gates, length);

            Boolean payFull = length <= 2;

            AirportContract contract = new AirportContract(GameObject.GetInstance().HumanAirline, this.Airport.Airport, GameObject.GetInstance().GameTime, gates, length, yearlyPayment, payFull);
            
            if (!hasCheckin)
            {
                AirportFacility checkinFacility = AirportFacilities.GetFacilities(AirportFacility.FacilityType.CheckIn).Find(f => f.TypeLevel == 1);

                this.Airport.Airport.addAirportFacility(GameObject.GetInstance().HumanAirline, checkinFacility, GameObject.GetInstance().GameTime);
                AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -checkinFacility.Price);

            }

            //25 % off if paying up front
            if (contract.PayFull)
            {
                double payment = (contract.YearlyPayment * contract.Length) * 0.75;
                AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Rents, -payment);
                contract.YearlyPayment = 0;
            }

            for (int i = 0; i < gates; i++)
            {
                Gate gate = this.Airport.Airport.Terminals.getGates().Where(g => g.Airline == null).First();
                gate.Airline = GameObject.GetInstance().HumanAirline;
            }

            this.Airport.addAirlineContract(contract);

        }
        private void btnRemoveContract_Click(object sender, RoutedEventArgs e)
        {
            ContractMVVM tContract = (ContractMVVM)((Button)sender).Tag;

            AirportContract contract = tContract.Contract;

            double penaltyFee = ((contract.YearlyPayment / 12) * contract.MonthsLeft) / 10;

            var contracts = this.Airport.Airport.AirlineContracts.Where(a => a.Airline == GameObject.GetInstance().HumanAirline && a != contract).ToList();

            if (!AirportHelpers.CanFillRoutesEntries(this.Airport.Airport, GameObject.GetInstance().HumanAirline, contracts))
            {
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2224"), Translator.GetInstance().GetString("MessageBox", "2224", "message"), WPFMessageBoxButtons.Ok);
            }
            else if (contract.Terminal != null)
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2226"), string.Format(Translator.GetInstance().GetString("MessageBox", "2226", "message"), contract.Terminal.Name), WPFMessageBoxButtons.Ok);
            else
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2225"), string.Format(Translator.GetInstance().GetString("MessageBox", "2225", "message"), new ValueCurrencyConverter().Convert(penaltyFee)), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -penaltyFee);

                    this.Airport.removeAirlineContract(tContract);

                    for (int i = 0; i < contract.NumberOfGates; i++)
                    {
                        Gate gate = this.Airport.Airport.Terminals.getGates().Where(g => g.Airline == GameObject.GetInstance().HumanAirline).First();
                        gate.Airline = null;
                    }

                }
            }


        }
        private void btnBuildTerminal_Click(object sender, RoutedEventArgs e)
        {
            int gates = Convert.ToInt16(slGates.Value);
            string name = txtName.Text.Trim();

            // chs, 2011-01-11 changed so a message for confirmation are shown9han
            double price = gates * this.Airport.TerminalGatePrice + this.Airport.TerminalPrice;

            if (price > GameObject.GetInstance().HumanAirline.Money)
            {
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2205"), Translator.GetInstance().GetString("MessageBox", "2205", "message"), WPFMessageBoxButtons.Ok);
            }
            else
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2206"), string.Format(Translator.GetInstance().GetString("MessageBox", "2206", "message"), gates, price), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    DateTime deliveryDate = GameObject.GetInstance().GameTime.Add(new TimeSpan(gates * 10 + 60, 0, 0, 0));

                    Terminal terminal = new Terminal(this.Airport.Airport, GameObject.GetInstance().HumanAirline, name, gates, deliveryDate);

                    this.Airport.addTerminal(terminal);

                    AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -price);

                }
            }

        }

        private void btnCreateHub_Click(object sender, RoutedEventArgs e)
        {

            HubType type = HubTypes.GetHubType(HubType.TypeOfHub.Hub);//(HubType)cbHubType.SelectedItem;

            if (AirportHelpers.GetHubPrice(this.Airport.Airport, type) > GameObject.GetInstance().HumanAirline.Money)
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2212"), Translator.GetInstance().GetString("MessageBox", "2212", "message"), WPFMessageBoxButtons.Ok);
            else
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2213"), string.Format(Translator.GetInstance().GetString("MessageBox", "2213", "message"),AirportHelpers.GetHubPrice(this.Airport.Airport, type)), WPFMessageBoxButtons.YesNo);
                

                if (result == WPFMessageBoxResult.Yes)
                {

                    this.Airport.addHub(new Hub(GameObject.GetInstance().HumanAirline, type));

                    AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -AirportHelpers.GetHubPrice(this.Airport.Airport, type));
                }
            }

        }
        private void btnRemoveHub_Click(object sender, RoutedEventArgs e)
        {
            Hub hub = (Hub)((Button)sender).Tag;

            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2227"), string.Format(Translator.GetInstance().GetString("MessageBox", "2227", "message"),this.Airport.Airport.Profile.Name), WPFMessageBoxButtons.YesNo);
             
              if (result == WPFMessageBoxResult.Yes)
              {

                  this.Airport.removeHub(hub);
              }

        }

        private void btnExtendContract_Click(object sender, RoutedEventArgs e)
        {
            ContractMVVM tContract = (ContractMVVM)((Button)sender).Tag;

           
              WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2228"), string.Format(Translator.GetInstance().GetString("MessageBox", "2228", "message"),2), WPFMessageBoxButtons.YesNo);

              if (result == WPFMessageBoxResult.Yes)
              {
                  tContract.extendContract(2);
              }
        }

        private void btnSellTerminal_Click(object sender, RoutedEventArgs e)
        {
            AirportTerminalMVVM terminal = (AirportTerminalMVVM)((Button)sender).Tag;

            int totalRentedGates = this.Airport.Airport.AirlineContracts.Sum(c => c.NumberOfGates);

            Boolean isTerminalFree = this.Airport.Airport.Terminals.getNumberOfGates() - terminal.Gates >= totalRentedGates;
            if (isTerminalFree)
            {

                // chs, 2011-31-10 changed for the possibility of having delivered and non-delivered terminals

                string strRemove;
                if (terminal.DeliveryDate > GameObject.GetInstance().GameTime)
                    strRemove = Translator.GetInstance().GetString("MessageBox", "2207", "message");
                else
                    strRemove = string.Format(Translator.GetInstance().GetString("MessageBox", "2208", "message"), terminal.Gates);
                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2207"), strRemove, WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    this.Airport.removeTerminal(terminal);

                }
            }
            else
            {
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2209"), Translator.GetInstance().GetString("MessageBox", "2209", "message"), WPFMessageBoxButtons.Ok);
            }
        }
        private void btnBuyTerminal_Click(object sender, RoutedEventArgs e)
        {
            
            AirportTerminalMVVM terminal = (AirportTerminalMVVM)((Button)sender).Tag;

            long price = terminal.Gates * this.Airport.Airport.getTerminalGatePrice() + this.Airport.Airport.getTerminalPrice();

            if (price > GameObject.GetInstance().HumanAirline.Money)
            {
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2205"), Translator.GetInstance().GetString("MessageBox", "2205", "message"), WPFMessageBoxButtons.Ok);

            }
            else
            {

                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2211"), string.Format(Translator.GetInstance().GetString("MessageBox", "2211", "message"), price), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    terminal.purchaseTerminal(GameObject.GetInstance().HumanAirline);
             
                    AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -price);

                }


            }
        
        }

        private void btnDemandContract_Click(object sender, RoutedEventArgs e)
        {
            DemandMVVM demand = (DemandMVVM)((Button)sender).Tag;

            Airport airport = demand.Destination;

            Boolean hasCheckin = airport.getAirportFacility(GameObject.GetInstance().HumanAirline, AirportFacility.FacilityType.CheckIn).TypeLevel > 0;

            int gates = Math.Min(2, airport.Terminals.NumberOfFreeGates);

            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2222"), string.Format(Translator.GetInstance().GetString("MessageBox", "2222", "message"), gates, airport.Profile.Name), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                if (!hasCheckin)
                {
                    AirportFacility checkinFacility = AirportFacilities.GetFacilities(AirportFacility.FacilityType.CheckIn).Find(f => f.TypeLevel == 1);

                    airport.addAirportFacility(GameObject.GetInstance().HumanAirline, checkinFacility, GameObject.GetInstance().GameTime);
                    AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -checkinFacility.Price);

                }
                
                double yearlyPayment = AirportHelpers.GetYearlyContractPayment(airport, gates, 2);

                AirportContract contract = new AirportContract(GameObject.GetInstance().HumanAirline, airport, GameObject.GetInstance().GameTime, gates, 2, yearlyPayment);

                airport.addAirlineContract(contract);

                for (int i = 0; i < gates; i++)
                {
                    Gate gate = airport.Terminals.getGates().Where(g => g.Airline == null).First();
                    gate.Airline = GameObject.GetInstance().HumanAirline;
                }

                demand.Contracted = true;
            }
        }

    }
}
