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
using TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GUIModel.PagesModel.AirportPageModel
{
    /// <summary>
    /// Interaction logic for PageAirportDemand.xaml
    /// </summary>
    public partial class PageAirportDemand : Page
    {
        private AirportMVVM Airport;
        public PageAirportDemand(AirportMVVM airport)
        {
            this.Airport = airport;
            this.DataContext = this.Airport;
            
            InitializeComponent();

            CollectionView viewDemands = (CollectionView)CollectionViewSource.GetDefaultView(lvDemand.ItemsSource);
            viewDemands.GroupDescriptions.Clear();

            viewDemands.GroupDescriptions.Add(new PropertyGroupDescription("Type"));

            SortDescription sortTypeDescription = new SortDescription("Type", ListSortDirection.Ascending);
            viewDemands.SortDescriptions.Add(sortTypeDescription);

            SortDescription sortPassengersDescription = new SortDescription("Passengers", ListSortDirection.Descending);
            viewDemands.SortDescriptions.Add(sortPassengersDescription);

          

        }
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

                double yearlyPayment = AirportHelpers.GetYearlyContractPayment(airport, contractType, gates, 2);

                AirportContract contract = new AirportContract(GameObject.GetInstance().HumanAirline, airport, contractType, GameObject.GetInstance().GameTime, gates, 2, yearlyPayment, true);

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

             var source = lvDemand.Items as ICollectionView;
             source.Filter = o =>
             {
                 if (o != null)
                 {
                     DemandMVVM a = o as DemandMVVM;
                     return a != null && a.Destination.Profile.IATACode.ToUpper().StartsWith(searchText) || a.Destination.Profile.ICAOCode.ToUpper().StartsWith(searchText) || a.Destination.Profile.Name.ToUpper().StartsWith(searchText);
                 }
                 else
                     return false;
                     
             };

        }

    }
}
