using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.GUIModel.PagesModel.AirlinePageModel;
using TheAirline.GUIModel.PagesModel.AirlinersPageModel;
using TheAirline.GUIModel.PagesModel.RoutesPageModel;
using TheAirline.Helpers;
using TheAirline.Models.Airlines;
using TheAirline.Models.Airports;
using TheAirline.Models.General;
using TheAirline.ViewModels.Airline;

namespace TheAirline.Views.Airline
{
    /// <summary>
    ///     Interaction logic for PageAirline.xaml
    /// </summary>
    public partial class PageAirline : INotifyPropertyChanged
    {
        #region Constructors and Destructors

        public PageAirline(Models.Airlines.Airline airline)
        {
            Airline = new AirlineMVVM(airline);

            HasCodesharing =
                Airline.Airline.Codeshares.Exists(
                    c =>
                        c.Airline1 == GameObject.GetInstance().HumanAirline
                        || c.Airline2 == GameObject.GetInstance().HumanAirline);
            CanHaveAlliance = Airline.Airline.Alliances.Count == 0
                              && GameObject.GetInstance().HumanAirline.Alliances.Count > 0;
            ShowActionMenu = !Airline.Airline.IsHuman && (CanHaveAlliance || !HasCodesharing);

            var airports = Airline.Airline.Airports;
            var routes = Airline.Airline.Routes.OrderByDescending(r => r.Balance);

            var totalProfit = routes.Sum(r => r.Balance);

            ProfitRoutes = new List<RouteProfitMVVM>();
            foreach (var route in routes.Take(Math.Min(5, routes.Count())))
            {
                ProfitRoutes.Add(new RouteProfitMVVM(route, totalProfit));
            }

            MostGates =
                airports.OrderByDescending(a => a.GetAirlineContracts(Airline.Airline).Sum(c => c.NumberOfGates))
                    .Take(Math.Min(5, airports.Count))
                    .ToList();
            MostUsedAircrafts = new List<AirlineFleetSizeMVVM>();

            var types = Airline.Airline.Fleet.Select(a => a.Airliner.Type).Distinct();

            foreach (var type in types)
            {
                var count = Airline.Airline.Fleet.Count(a => a.Airliner.Type == type);

                MostUsedAircrafts.Add(new AirlineFleetSizeMVVM(type, count));
            }

            MostUsedAircrafts =
                MostUsedAircrafts.OrderByDescending(a => a.Count)
                    .Take(Math.Min(5, MostUsedAircrafts.Count))
                    .ToList();

            Loaded += PageAirline_Loaded;

            InitializeComponent();
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Fields

        private readonly AirlineMVVM Airline;

        private bool _canhavealliance;

        private bool _hascodesharing;

        private bool _showactionmenu;

        #endregion

        #region Public Properties

        public bool CanHaveAlliance
        {
            get { return _canhavealliance; }
            set
            {
                _canhavealliance = value;
                NotifyPropertyChanged("CanHaveAlliance");
            }
        }

        public bool HasCodesharing
        {
            get { return _hascodesharing; }
            set
            {
                _hascodesharing = value;
                NotifyPropertyChanged("HasCodesharing");
            }
        }

        public List<Airport> MostGates { get; set; }

        public List<AirlineFleetSizeMVVM> MostUsedAircrafts { get; set; }

        public List<RouteProfitMVVM> ProfitRoutes { get; set; }

        public bool ShowActionMenu
        {
            get { return _showactionmenu; }
            set
            {
                _showactionmenu = value;
                NotifyPropertyChanged("ShowActionMenu");
            }
        }

        #endregion

        #region Methods

        private void NotifyPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void PageAirline_Loaded(object sender, RoutedEventArgs e)
        {
            var frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            frmContent.Navigate(new PageAirlineInfo(Airline) {Tag = this});

            var tab_main = UIHelpers.FindChild<TabControl>(this, "tcMenu");

            if (tab_main != null)
            {
                var matchingItem =
                    tab_main.Items.Cast<TabItem>().FirstOrDefault(item => item.Tag.ToString() == "Overview");

                //matchingItem.IsSelected = true;
                matchingItem.Header = Airline.Airline.Profile.Name;
                matchingItem.Visibility = Visibility.Visible;

                tab_main.SelectedItem = matchingItem;
            }
        }

        private void hlAlliance_Click(object sender, RoutedEventArgs e)
        {
            var cbAlliances = new ComboBox();
            cbAlliances.SetResourceReference(StyleProperty, "ComboBoxTransparentStyle");
            cbAlliances.SelectedValuePath = "Name";
            cbAlliances.DisplayMemberPath = "Name";
            cbAlliances.HorizontalAlignment = HorizontalAlignment.Left;
            cbAlliances.Width = 200;

            foreach (var alliance in GameObject.GetInstance().HumanAirline.Alliances)
            {
                cbAlliances.Items.Add(alliance);
            }

            cbAlliances.SelectedIndex = 0;

            if (PopUpSingleElement.ShowPopUp(Translator.GetInstance().GetString("PageAirline", "1005"), cbAlliances)
                == PopUpSingleElement.ButtonSelected.OK && cbAlliances.SelectedItem != null)
            {
                var alliance = (Alliance) cbAlliances.SelectedItem;
                if (AIHelpers.DoAcceptAllianceInvitation(Airline.Airline, alliance))
                {
                    WPFMessageBox.Show(
                        Translator.GetInstance().GetString("MessageBox", "2605"),
                        string.Format(
                            Translator.GetInstance().GetString("MessageBox", "2605", "message"),
                            Airline.Airline.Profile.Name,
                            alliance.Name),
                        WPFMessageBoxButtons.Ok);
                    alliance.AddMember(new AllianceMember(Airline.Airline, GameObject.GetInstance().GameTime));

                    Airline.Alliance = alliance;

                    CanHaveAlliance = false;
                    ShowActionMenu = !Airline.Airline.IsHuman
                                     && (CanHaveAlliance || !HasCodesharing);
                }
                else
                {
                    WPFMessageBox.Show(
                        Translator.GetInstance().GetString("MessageBox", "2606"),
                        string.Format(
                            Translator.GetInstance().GetString("MessageBox", "2606", "message"),
                            Airline.Airline.Profile.Name,
                            alliance.Name),
                        WPFMessageBoxButtons.Ok);
                }
            }
        }

        private void hlCodeSharing_Click(object sender, RoutedEventArgs e)
        {
            var o = PopUpCodeshareAgreement.ShowPopUp(Airline.Airline);

            if (o != null)
            {
                var type = (CodeshareAgreement.CodeshareType) o;
                var accepted = AirlineHelpers.AcceptCodesharing(
                    Airline.Airline,
                    GameObject.GetInstance().HumanAirline,
                    type);

                if (accepted)
                {
                    HasCodesharing = true;
                    ShowActionMenu = !Airline.Airline.IsHuman
                                     && (CanHaveAlliance || !HasCodesharing);

                    var agreement = new CodeshareAgreement(
                        Airline.Airline,
                        GameObject.GetInstance().HumanAirline,
                        type);

                    Airline.addCodeshareAgreement(agreement);
                    GameObject.GetInstance().HumanAirline.AddCodeshareAgreement(agreement);
                }
                else
                {
                    WPFMessageBox.Show(
                        Translator.GetInstance().GetString("MessageBox", "2408"),
                        string.Format(
                            Translator.GetInstance().GetString("MessageBox", "2408", "message"),
                            Airline.Airline.Profile.Name),
                        WPFMessageBoxButtons.Ok);
                }
            }
        }

        private void tcMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var control = (TabControl) sender;

            var matchingItem =
                control.Items.Cast<TabItem>().FirstOrDefault(item => item.Tag.ToString() == "Airliners");

            matchingItem.Visibility = Visibility.Collapsed;

            var selection = ((TabItem) control.SelectedItem).Tag.ToString();

            var frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            if (selection == "Overview")
            {
                frmContent?.Navigate(new PageAirlineInfo(Airline) {Tag = this});
            }

            if (selection == "Finances")
            {
                frmContent?.Navigate(new PageAirlineFinances(Airline) {Tag = this});
            }

            if (selection == "Employees")
            {
                frmContent?.Navigate(new PageAirlineEmployees(Airline) {Tag = this});
            }

            if (selection == "Services")
            {
                frmContent?.Navigate(new PageAirlineServices(Airline) {Tag = this});
            }

            if (selection == "Ratings")
            {
                frmContent?.Navigate(new PageAirlineRatings(Airline) {Tag = this});
            }

            if (selection == "Insurances")
            {
                frmContent?.Navigate(new PageAirlineInsurance(Airline) {Tag = this});
            }

            if (selection == "Fleet")
            {
                frmContent?.Navigate(new PageAirlineFleet(Airline) {Tag = this});
            }
        }

        #endregion
    }
}