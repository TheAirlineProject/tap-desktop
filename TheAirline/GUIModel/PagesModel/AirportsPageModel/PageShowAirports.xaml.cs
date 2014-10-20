namespace TheAirline.GUIModel.PagesModel.AirportsPageModel
{
    using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.GUIModel.CustomControlsModel.FilterableListView;
using TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel;
using TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel.PopUpMapModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.GUIModel.PagesModel.AirportPageModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

    /// <summary>
    ///     Interaction logic for PageShowAirports.xaml
    /// </summary>
    public partial class PageShowAirports : Page
    {
        #region Constructors and Destructors

        public PageShowAirports(List<Airport> airports)
        {
            this.createPage(airports);
        }

        public PageShowAirports()
        {
            this.createPage(Airports.GetAllActiveAirports());
        }

        #endregion

        #region Public Properties

        public List<Airline> AllAirlines { get; set; }

        public List<AirportMVVM> AllAirports { get; set; }

        public List<AirlinerType> HumanAircrafts { get; set; }

        public ObservableCollection<Airport> HumanAirports { get; set; }

        public List<FilterValue> OperatingRanges { get; set; }

        public List<FilterValue> RoutesRanges { get; set; }

        public ObservableCollection<AirportMVVM> SelectedAirports { get; set; }

        #endregion

        #region Methods

        private void btnCompare_Click(object sender, RoutedEventArgs e)
        {
            List<Airport> airports = this.SelectedAirports.Select(s => s.Airport).ToList();
            PopUpCompareAirports.ShowPopUp(airports);
        }
        private void btnShowOnMap_Click(object sender, RoutedEventArgs e)
        {
            List<Airport> airports = this.AllAirports.Select(a => a.Airport).ToList();
        
            //PopUpMap.ShowPopUp(airports);
            PopUpMapControl.ShowPopUp(airports);
        }
        private void btnContract_Click(object sender, RoutedEventArgs e)
        {
            var airport = (AirportMVVM)((Button)sender).Tag;
           
            Route.RouteType airlineFocus = GameObject.GetInstance().HumanAirline.AirlineRouteFocus;

            Boolean hasCargo =
                airport.Airport.getAirportFacility(GameObject.GetInstance().HumanAirline, AirportFacility.FacilityType.Cargo, true)
                .TypeLevel > 0;
            
            Boolean hasCheckin =
                airport.Airport.getAirportFacility(
                    GameObject.GetInstance().HumanAirline,
                    AirportFacility.FacilityType.CheckIn).TypeLevel > 0;

            int paxGates = Math.Min(2, airport.Airport.Terminals.getFreeGates(Terminal.TerminalType.Passenger));
            int cargoGates = Math.Min(2,airport.Airport.Terminals.getFreeGates(Terminal.TerminalType.Cargo));

            //WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2222"), string.Format(Translator.GetInstance().GetString("MessageBox", "2222", "message"),gates, airport.Airport.Profile.Name), WPFMessageBoxButtons.YesNo);

            object o = PopUpAirportContract.ShowPopUp(airport.Airport);

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

                        airport.Airport.addAirportFacility(
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

                        airport.Airport.addAirportFacility(
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

                double yearlyPayment = AirportHelpers.GetYearlyContractPayment(airport.Airport, contractType, gates, 2);

                var contract = new AirportContract(
                    GameObject.GetInstance().HumanAirline,
                    airport.Airport,
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
                    Gate gate = airport.Airport.Terminals.getGates().Where(g => g.Airline == null).First();
                    gate.Airline = GameObject.GetInstance().HumanAirline;
                }

                if (!this.HumanAirports.Contains(airport.Airport))
                    this.HumanAirports.Add(airport.Airport);

                airport.IsHuman = true;
            }
        }

        private void cbAircraft_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var type = (AirlinerType)((ComboBox)sender).SelectedItem;

            if (this.AirportsList != null)
            {
                var source = this.AirportsList.Items as ICollectionView;
                source.Filter = o =>
                {
                    var a = o as AirportMVVM;
                    return a != null && a.Airport.getMaxRunwayLength() >= type.MinRunwaylength
                           || type.Manufacturer.Name == "Dummy";
                };
            }
        }

        private void cbAirline_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var airline = (Airline)((ComboBox)sender).SelectedItem;

            if (this.AirportsList != null)
            {
                var source = this.AirportsList.Items as ICollectionView;
                source.Filter = o =>
                {
                    var a = o as AirportMVVM;
                    return a != null && a.Airport.AirlineContracts.Exists(c => c.Airline == airline)
                           || airline.Profile.IATACode == "99";
                };
            }
        }

        private void cbHuman_Checked(object sender, RoutedEventArgs e)
        {
            var source = this.AirportsList.Items as ICollectionView;
            source.Filter = o =>
            {
                var a = o as AirportMVVM;
                return a != null && a.IsHuman;
            };
        }

        private void cbHuman_Unchecked(object sender, RoutedEventArgs e)
        {
            var source = this.AirportsList.Items as ICollectionView;
            source.Filter = o =>
            {
                var a = o as AirportMVVM;
                return !a.IsHuman || a.IsHuman;
            };

            this.cbAirlines.SelectedIndex = 0;
        }

        private void cbSelected_Checked(object sender, RoutedEventArgs e)
        {
            var airport = (AirportMVVM)((CheckBox)sender).Tag;

            this.SelectedAirports.Add(airport);
        }

        private void cbSelected_Unchecked(object sender, RoutedEventArgs e)
        {
            var airport = (AirportMVVM)((CheckBox)sender).Tag;

            this.SelectedAirports.Remove(airport);
        }

        private void clName_Click(object sender, RoutedEventArgs e)
        {
            var airport = (Airport)((Hyperlink)sender).Tag;

            PageNavigator.NavigateTo(new PageAirport(airport));
        }
      
       
        private void createPage(List<Airport> airports)
        {
        
            this.HumanAirports = new ObservableCollection<Airport>();

            foreach (Airport hAirport in GameObject.GetInstance().HumanAirline.Airports)
                this.HumanAirports.Add(hAirport);

            this.AllAirlines = new List<Airline>();
            this.SelectedAirports = new ObservableCollection<AirportMVVM>();
            this.RoutesRanges = new List<FilterValue>
                                {
                                    new FilterValue("0", 0, 0),
                                    new FilterValue("1-9", 1, 9),
                                    new FilterValue("10-24", 10, 24),
                                    new FilterValue("25+", 25, int.MaxValue)
                                };
            this.OperatingRanges = new List<FilterValue>
                                   {
                                       new FilterValue("0", 0, 0),
                                       new FilterValue("1-5", 1, 5),
                                       new FilterValue("6+", 5, int.MaxValue)
                                   };

            var dummyAirline = new Airline(
                new AirlineProfile("All Airlines", "99", "Blue", "", false, 1900, 1900),
                Airline.AirlineMentality.Safe,
                Airline.AirlineFocus.Domestic,
                Airline.AirlineLicense.Domestic,
                Route.RouteType.Passenger);
            dummyAirline.Profile.AddLogo(
                new AirlineLogo(AppSettings.getDataPath() + "\\graphics\\airlinelogos\\default.png"));

            this.AllAirlines.Add(dummyAirline);

            foreach (
                Airline airline in
                    Airlines.GetAllAirlines()
                        .Where(a => a != GameObject.GetInstance().HumanAirline)
                        .OrderBy(a => a.Profile.Name))
            {
                this.AllAirlines.Add(airline);
            }

            this.AllAirports = new List<AirportMVVM>();

            foreach (Airport airport in airports.OrderBy(a => a.Profile.Name))
            {
                this.AllAirports.Add(new AirportMVVM(airport));
            }

            AirlinerType dummyAircraft = new AirlinerCargoType(
                new Manufacturer("Dummy", "", null, false),
                "All Aircrafts",
                "",
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                AirlinerType.BodyType.Single_Aisle,
                AirlinerType.TypeRange.Regional,
                AirlinerType.TypeOfEngine.Jet,
                new Period<DateTime>(DateTime.Now, DateTime.Now),
                0,
                false);

            this.HumanAircrafts = new List<AirlinerType>();

            this.HumanAircrafts.Add(dummyAircraft);

            foreach (
                AirlinerType type in GameObject.GetInstance().HumanAirline.Fleet.Select(f => f.Airliner.Type).Distinct()
                )
            {
                this.HumanAircrafts.Add(type);
            }

            this.InitializeComponent();

            var tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                TabItem matchingItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Airports").FirstOrDefault();

                //matchingItem.IsSelected = true;
                tab_main.SelectedItem = matchingItem;
            }
        }

       
        #endregion
    }
}