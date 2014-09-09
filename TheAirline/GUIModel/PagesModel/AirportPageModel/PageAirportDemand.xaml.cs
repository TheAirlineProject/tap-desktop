namespace TheAirline.GUIModel.PagesModel.AirportPageModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel;
    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.AirlineModel;
    using TheAirline.Model.AirlinerModel;
    using TheAirline.Model.AirlinerModel.RouteModel;
    using TheAirline.Model.AirportModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.GeneralModel.Helpers;

    /// <summary>
    ///     Interaction logic for PageAirportDemand.xaml
    /// </summary>
    public partial class PageAirportDemand : Page, INotifyPropertyChanged
    {
       
        private DemandMVVM _selectedairport;
        public DemandMVVM SelectedAirport
        {
            get
            {
                return this._selectedairport;
            }
            set
            {
                this._selectedairport = value;
                this.NotifyPropertyChanged("SelectedAirport");
            }
        }
     
        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

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

        #endregion
        #region Fields

        private AirportMVVM Airport;

        #endregion

        #region Constructors and Destructors

        public PageAirportDemand(AirportMVVM airport)
        {
           

            this.Airport = airport;
            this.DataContext = this.Airport;

            this.InitializeComponent();

            var viewDemandsIntl = (CollectionView)CollectionViewSource.GetDefaultView(this.lvDemandIntl.ItemsSource);
          
            var sortTypeDescription = new SortDescription("Type", ListSortDirection.Ascending);
            viewDemandsIntl.SortDescriptions.Add(sortTypeDescription);

            var sortPassengersDescription = new SortDescription("Passengers", ListSortDirection.Descending);
            viewDemandsIntl.SortDescriptions.Add(sortPassengersDescription);

            var viewDemandsDomestic = (CollectionView)CollectionViewSource.GetDefaultView(this.lvDemandDomestic.ItemsSource);

            var sortTypeDomesticDescription = new SortDescription("Type", ListSortDirection.Ascending);
            viewDemandsDomestic.SortDescriptions.Add(sortTypeDomesticDescription);

            var sortPassengersDomesticDescription = new SortDescription("Passengers", ListSortDirection.Descending);
            viewDemandsDomestic.SortDescriptions.Add(sortPassengersDomesticDescription);


         

        }

        #endregion

        #region Methods
        private void btnDemandInfo_Click(object sender, RoutedEventArgs e)
        {
            var airport = (DemandMVVM)((Button)sender).Tag;

            this.SelectedAirport = airport;

            var demandPercent = new List<KeyValuePair<string, int>>();

            int intlDemand = this.Airport.IntlDemands.Sum(d => d.Passengers) + this.Airport.IntlDemands.Sum(d => d.Cargo);
            int domesticDemand = this.Airport.DomesticDemands.Sum(d => d.Passengers) + this.Airport.DomesticDemands.Sum(d => d.Cargo);

            demandPercent.Add(new KeyValuePair<string,int>(Translator.GetInstance().GetString("PageAirportInfo", "1029"),domesticDemand));
            demandPercent.Add(new KeyValuePair<string,int>(Translator.GetInstance().GetString("PageAirportInfo", "1057"),intlDemand));

            this.pcDemand.DataContext = demandPercent;

            this.pcGates.DataContext = this.SelectedAirport.GatesPercent;

            var demands = new List<KeyValuePair<string,int>>();

            demands.Add(new KeyValuePair<string, int>(Translator.GetInstance().GetString("PageAirportInfo", "1031"), this.SelectedAirport.Passengers));
            demands.Add(new KeyValuePair<string, int>(Translator.GetInstance().GetString("PageAirportInfo", "1032"), this.SelectedAirport.Cargo));

            var demands2 = new List<KeyValuePair<string, int>>();

            demands2.Add(new KeyValuePair<string, int>(Translator.GetInstance().GetString("PageAirportInfo", "1031"), this.SelectedAirport.Destination.GetDestinationPassengersRate(this.Airport.Airport, AirlinerClass.ClassType.EconomyClass)));
            demands2.Add(new KeyValuePair<string, int>(Translator.GetInstance().GetString("PageAirportInfo", "1032"), this.SelectedAirport.Destination.GetDestinationCargoRate(this.Airport.Airport)));

            var demandSeries = new List<SeriesData>();

            string displayName1 = string.Format("{0}-{1}", new AirportCodeConverter().Convert(this.Airport.Airport).ToString(), new AirportCodeConverter().Convert(this.SelectedAirport.Destination));
            string displayName2 = string.Format("{1}-{0}", new AirportCodeConverter().Convert(this.Airport.Airport).ToString(), new AirportCodeConverter().Convert(this.SelectedAirport.Destination));

            demandSeries.Add(new SeriesData() { DisplayName = displayName1, Items = demands });
            demandSeries.Add(new SeriesData() { DisplayName = displayName2, Items = demands2 });
         
            this.cccDemand.DataContext = demandSeries;

            var routes =  new List<KeyValuePair<string,int>>();

             var airlines = this.SelectedAirport.Destination.AirlineContracts.Select(a=>a.Airline).Distinct();

            foreach (Airline airline in airlines)
            {
                int airlineSeats = airline.Routes.Where(r=>r.Destination1 == this.SelectedAirport.Destination || r.Destination2 == this.SelectedAirport.Destination).Sum(r=>r.TimeTable.Entries.Sum(en=>en.Airliner.Airliner.GetTotalSeatCapacity()));

                routes.Add(new KeyValuePair<string,int>(airline.Profile.Name,airlineSeats / 7));
            }

            if (routes.Count == 0)
                routes.Add(new KeyValuePair<string, int>("None", 0));

            this.pcSeats.DataContext = routes;

        
       
        }
        private void btnDemandContract_Click(object sender, RoutedEventArgs e)
        {
            var demand = (DemandMVVM)((Button)sender).Tag;

            Airport airport = demand.Destination;

            Route.RouteType airlineFocus = GameObject.GetInstance().HumanAirline.AirlineRouteFocus;

            Boolean hasCargo = 
                airport.GetAirportFacility(GameObject.GetInstance().HumanAirline,AirportFacility.FacilityType.Cargo,true)
                .TypeLevel > 0;

            Boolean hasCheckin =
                airport.GetAirportFacility(GameObject.GetInstance().HumanAirline, AirportFacility.FacilityType.CheckIn)
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

                        airport.AddAirportFacility(
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

                        airport.AddAirportFacility(
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
                    Gate gate = airport.Terminals.GetGates().Where(g => g.Airline == null).First();
                    gate.Airline = GameObject.GetInstance().HumanAirline;
                }

                demand.Contracted = true;
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = ((TextBox)e.Source).Text.ToUpper();

            var source = this.lvDemandDomestic.Items as ICollectionView;
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