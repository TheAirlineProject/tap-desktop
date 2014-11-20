namespace TheAirline.GUIModel.PagesModel.AirlinePageModel
{
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
    using TheAirline.GUIModel.PagesModel.AirlinersPageModel;
    using TheAirline.GUIModel.PagesModel.RoutesPageModel;
    using TheAirline.Model.AirlineModel;
    using TheAirline.Model.AirlinerModel;
    using TheAirline.Model.AirlinerModel.RouteModel;
    using TheAirline.Model.AirportModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.GeneralModel.Helpers;

    /// <summary>
    ///     Interaction logic for PageAirline.xaml
    /// </summary>
    public partial class PageAirline : Page, INotifyPropertyChanged
    {
        #region Fields

        private readonly AirlineMVVM Airline;

        private Boolean _canhavealliance;

        private Boolean _hascodesharing;

        private Boolean _showactionmenu;

        #endregion

        #region Constructors and Destructors

        public PageAirline(Airline airline)
        {
            this.Airline = new AirlineMVVM(airline);

            this.HasCodesharing =
                this.Airline.Airline.Codeshares.Exists(
                    c =>
                        c.Airline1 == GameObject.GetInstance().HumanAirline
                        || c.Airline2 == GameObject.GetInstance().HumanAirline);
            this.CanHaveAlliance = this.Airline.Airline.Alliances.Count == 0
                                   && GameObject.GetInstance().HumanAirline.Alliances.Count > 0;
            this.ShowActionMenu = !this.Airline.Airline.IsHuman && (this.CanHaveAlliance || !this.HasCodesharing);

            List<Airport> airports = this.Airline.Airline.Airports;
            IOrderedEnumerable<Route> routes = this.Airline.Airline.Routes.OrderByDescending(r => r.Balance);

            double totalProfit = routes.Sum(r => r.Balance);

            this.ProfitRoutes = new List<RouteProfitMVVM>();
            foreach (Route route in routes.Take(Math.Min(5, routes.Count())))
            {
                this.ProfitRoutes.Add(new RouteProfitMVVM(route, totalProfit));
            }

            this.MostGates =
                airports.OrderByDescending(a => a.getAirlineContracts(this.Airline.Airline).Sum(c => c.NumberOfGates))
                    .Take(Math.Min(5, airports.Count))
                    .ToList();
            this.MostUsedAircrafts = new List<AirlineFleetSizeMVVM>();

            IEnumerable<AirlinerType> types = this.Airline.Airline.Fleet.Select(a => a.Airliner.Type).Distinct();

            foreach (AirlinerType type in types)
            {
                int count = this.Airline.Airline.Fleet.Count(a => a.Airliner.Type == type);

                this.MostUsedAircrafts.Add(new AirlineFleetSizeMVVM(type, count));
            }

            this.MostUsedAircrafts =
                this.MostUsedAircrafts.OrderByDescending(a => a.Count)
                    .Take(Math.Min(5, this.MostUsedAircrafts.Count))
                    .ToList();
        
            this.Loaded += this.PageAirline_Loaded;

            this.InitializeComponent();
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public Boolean CanHaveAlliance
        {
            get
            {
                return this._canhavealliance;
            }
            set
            {
                this._canhavealliance = value;
                this.NotifyPropertyChanged("CanHaveAlliance");
            }
        }

        public Boolean HasCodesharing
        {
            get
            {
                return this._hascodesharing;
            }
            set
            {
                this._hascodesharing = value;
                this.NotifyPropertyChanged("HasCodesharing");
            }
        }

        public List<Airport> MostGates { get; set; }

        public List<AirlineFleetSizeMVVM> MostUsedAircrafts { get; set; }

        public List<RouteProfitMVVM> ProfitRoutes { get; set; }

        public Boolean ShowActionMenu
        {
            get
            {
                return this._showactionmenu;
            }
            set
            {
                this._showactionmenu = value;
                this.NotifyPropertyChanged("ShowActionMenu");
            }
        }

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

        private void PageAirline_Loaded(object sender, RoutedEventArgs e)
        {
            var frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            frmContent.Navigate(new PageAirlineInfo(this.Airline) { Tag = this });

            var tab_main = UIHelpers.FindChild<TabControl>(this, "tcMenu");

            if (tab_main != null)
            {
                TabItem matchingItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Overview").FirstOrDefault();

                //matchingItem.IsSelected = true;
                matchingItem.Header = this.Airline.Airline.Profile.Name;
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

            foreach (Alliance alliance in GameObject.GetInstance().HumanAirline.Alliances)
            {
                cbAlliances.Items.Add(alliance);
            }

            cbAlliances.SelectedIndex = 0;

            if (PopUpSingleElement.ShowPopUp(Translator.GetInstance().GetString("PageAirline", "1005"), cbAlliances)
                == PopUpSingleElement.ButtonSelected.OK && cbAlliances.SelectedItem != null)
            {
                var alliance = (Alliance)cbAlliances.SelectedItem;
                if (AIHelpers.DoAcceptAllianceInvitation(this.Airline.Airline, alliance))
                {
                    WPFMessageBox.Show(
                        Translator.GetInstance().GetString("MessageBox", "2605"),
                        string.Format(
                            Translator.GetInstance().GetString("MessageBox", "2605", "message"),
                            this.Airline.Airline.Profile.Name,
                            alliance.Name),
                        WPFMessageBoxButtons.Ok);
                    alliance.addMember(new AllianceMember(this.Airline.Airline, GameObject.GetInstance().GameTime));

                    this.Airline.Alliance = alliance;

                    this.CanHaveAlliance = false;
                    this.ShowActionMenu = !this.Airline.Airline.IsHuman
                                          && (this.CanHaveAlliance || !this.HasCodesharing);
                }
                else
                {
                    WPFMessageBox.Show(
                        Translator.GetInstance().GetString("MessageBox", "2606"),
                        string.Format(
                            Translator.GetInstance().GetString("MessageBox", "2606", "message"),
                            this.Airline.Airline.Profile.Name,
                            alliance.Name),
                        WPFMessageBoxButtons.Ok);
                }
            }
        }

        private void hlCodeSharing_Click(object sender, RoutedEventArgs e)
        {
            object o = PopUpCodeshareAgreement.ShowPopUp(this.Airline.Airline);

            if (o != null)
            {
                var type = (CodeshareAgreement.CodeshareType)o;
                Boolean accepted = AirlineHelpers.AcceptCodesharing(
                    this.Airline.Airline,
                    GameObject.GetInstance().HumanAirline,
                    type);

                if (accepted)
                {
                    this.HasCodesharing = true;
                    this.ShowActionMenu = !this.Airline.Airline.IsHuman
                                          && (this.CanHaveAlliance || !this.HasCodesharing);

                    var agreement = new CodeshareAgreement(
                        this.Airline.Airline,
                        GameObject.GetInstance().HumanAirline,
                        type);

                    this.Airline.addCodeshareAgreement(agreement);
                    GameObject.GetInstance().HumanAirline.addCodeshareAgreement(agreement);
                }
                else
                {
                    WPFMessageBox.Show(
                        Translator.GetInstance().GetString("MessageBox", "2408"),
                        string.Format(
                            Translator.GetInstance().GetString("MessageBox", "2408", "message"),
                            this.Airline.Airline.Profile.Name),
                        WPFMessageBoxButtons.Ok);
                }
            }
        }

        private void tcMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var control = (TabControl)sender;

            TabItem matchingItem =
                control.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Airliners").FirstOrDefault();

            matchingItem.Visibility = Visibility.Collapsed;

            string selection = ((TabItem)control.SelectedItem).Tag.ToString();

            var frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            if (selection == "Overview" && frmContent != null)
            {
                frmContent.Navigate(new PageAirlineInfo(this.Airline) { Tag = this });
            }

            if (selection == "Finances" && frmContent != null)
            {
                frmContent.Navigate(new PageAirlineFinances(this.Airline) { Tag = this });
            }

            if (selection == "Employees" && frmContent != null)
            {
                frmContent.Navigate(new PageAirlineEmployees(this.Airline) { Tag = this });
            }

            if (selection == "Services" && frmContent != null)
            {
                frmContent.Navigate(new PageAirlineServices(this.Airline) { Tag = this });
            }

            if (selection == "Ratings" && frmContent != null)
            {
                frmContent.Navigate(new PageAirlineRatings(this.Airline) { Tag = this });
            }

            if (selection == "Insurances" && frmContent != null)
            {
                frmContent.Navigate(new PageAirlineInsurance(this.Airline) { Tag = this });
            }

            if (selection == "Fleet" && frmContent != null)
            {
                frmContent.Navigate(new PageAirlineFleet(this.Airline) { Tag = this });
            }
        }

        #endregion
    }
}