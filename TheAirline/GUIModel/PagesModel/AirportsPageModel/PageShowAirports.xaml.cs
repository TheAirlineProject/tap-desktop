using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.GUIModel.CustomControlsModel.FilterableListView;
using TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.GUIModel.PagesModel.AirportPageModel;
using TheAirline.Helpers;
using TheAirline.Infrastructure;
using TheAirline.Models.Airliners;
using TheAirline.Models.Airlines;
using TheAirline.Models.Airports;
using TheAirline.Models.General;
using TheAirline.Models.General.Finances;
using TheAirline.Models.Routes;

namespace TheAirline.GUIModel.PagesModel.AirportsPageModel
{
    /// <summary>
    ///     Interaction logic for PageShowAirports.xaml
    /// </summary>
    public partial class PageShowAirports : Page
    {
        #region Constructors and Destructors

        public PageShowAirports(List<Airport> airports)
        {
            createPage(airports);
        }

        public PageShowAirports()
        {
            createPage(Airports.GetAllActiveAirports());
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
            List<Airport> airports = SelectedAirports.Select(s => s.Airport).ToList();
            PopUpCompareAirports.ShowPopUp(airports);
        }
        private void btnShowOnMap_Click(object sender, RoutedEventArgs e)
        {
            List<Airport> airports = AllAirports.Select(a => a.Airport).ToList();
        
            PopUpMap.ShowPopUp(airports);
        }
        private void btnContract_Click(object sender, RoutedEventArgs e)
        {
            var airport = (AirportMVVM)((Button)sender).Tag;
           
            Route.RouteType airlineFocus = GameObject.GetInstance().HumanAirline.AirlineRouteFocus;

            Boolean hasCargo =
                airport.Airport.GetAirportFacility(GameObject.GetInstance().HumanAirline, AirportFacility.FacilityType.Cargo, true)
                .TypeLevel > 0;
            
            Boolean hasCheckin =
                airport.Airport.GetAirportFacility(
                    GameObject.GetInstance().HumanAirline,
                    AirportFacility.FacilityType.CheckIn).TypeLevel > 0;

            int paxGates = Math.Min(2, airport.Airport.Terminals.GetFreeGates(Terminal.TerminalType.Passenger));
            int cargoGates = Math.Min(2,airport.Airport.Terminals.GetFreeGates(Terminal.TerminalType.Cargo));

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

                        airport.Airport.AddAirportFacility(
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

                        airport.Airport.AddAirportFacility(
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
                    Gate gate = airport.Airport.Terminals.GetGates().Where(g => g.Airline == null).First();
                    gate.Airline = GameObject.GetInstance().HumanAirline;
                }

                if (!HumanAirports.Contains(airport.Airport))
                    HumanAirports.Add(airport.Airport);

                airport.IsHuman = true;
            }
        }

        private void cbAircraft_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var type = (AirlinerType)((ComboBox)sender).SelectedItem;

            if (AirportsList != null)
            {
                var source = AirportsList.Items as ICollectionView;
                source.Filter = o =>
                {
                    var a = o as AirportMVVM;
                    return a != null && a.Airport.GetMaxRunwayLength() >= type.MinRunwaylength
                           || type.Manufacturer.Name == "Dummy";
                };
            }
        }

        private void cbAirline_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var airline = (Airline)((ComboBox)sender).SelectedItem;

            if (AirportsList != null)
            {
                var source = AirportsList.Items as ICollectionView;
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
            var source = AirportsList.Items as ICollectionView;
            source.Filter = o =>
            {
                var a = o as AirportMVVM;
                return a != null && a.IsHuman;
            };
        }

        private void cbHuman_Unchecked(object sender, RoutedEventArgs e)
        {
            var source = AirportsList.Items as ICollectionView;
            source.Filter = o =>
            {
                var a = o as AirportMVVM;
                return !a.IsHuman || a.IsHuman;
            };

            cbAirlines.SelectedIndex = 0;
        }

        private void cbSelected_Checked(object sender, RoutedEventArgs e)
        {
            var airport = (AirportMVVM)((CheckBox)sender).Tag;

            SelectedAirports.Add(airport);
        }

        private void cbSelected_Unchecked(object sender, RoutedEventArgs e)
        {
            var airport = (AirportMVVM)((CheckBox)sender).Tag;

            SelectedAirports.Remove(airport);
        }

        private void clName_Click(object sender, RoutedEventArgs e)
        {
            var airport = (Airport)((Hyperlink)sender).Tag;

            PageNavigator.NavigateTo(new PageAirport(airport));
        }
      
       
        private void createPage(List<Airport> airports)
        {
            HumanAirports = new ObservableCollection<Airport>();

            foreach (Airport hAirport in GameObject.GetInstance().HumanAirline.Airports)
                HumanAirports.Add(hAirport);

            AllAirlines = new List<Airline>();
            SelectedAirports = new ObservableCollection<AirportMVVM>();
            RoutesRanges = new List<FilterValue>
                                {
                                    new FilterValue("0", 0, 0),
                                    new FilterValue("1-9", 1, 9),
                                    new FilterValue("10-24", 10, 24),
                                    new FilterValue("25+", 25, int.MaxValue)
                                };
            OperatingRanges = new List<FilterValue>
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
                new AirlineLogo(AppSettings.GetDataPath() + "\\graphics\\airlinelogos\\default.png"));

            AllAirlines.Add(dummyAirline);

            foreach (
                Airline airline in
                    Airlines.GetAllAirlines()
                        .Where(a => a != GameObject.GetInstance().HumanAirline)
                        .OrderBy(a => a.Profile.Name))
            {
                AllAirlines.Add(airline);
            }

            AllAirports = new List<AirportMVVM>();

            foreach (Airport airport in airports.OrderBy(a => a.Profile.Name))
            {
                AllAirports.Add(new AirportMVVM(airport));
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
                AirlinerType.BodyType.SingleAisle,
                AirlinerType.TypeRange.Regional,
                AirlinerType.TypeOfEngine.Jet,
                new Period<DateTime>(DateTime.Now, DateTime.Now),
                0,
                false);

            HumanAircrafts = new List<AirlinerType>();

            HumanAircrafts.Add(dummyAircraft);

            foreach (
                AirlinerType type in GameObject.GetInstance().HumanAirline.Fleet.Select(f => f.Airliner.Type).Distinct()
                )
            {
                HumanAircrafts.Add(type);
            }

            InitializeComponent();

            var tab_main = UIHelpers.FindChild<TabControl>(Tag as Page, "tabMenu");

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