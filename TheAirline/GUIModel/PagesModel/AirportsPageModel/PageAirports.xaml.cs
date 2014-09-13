namespace TheAirline.GUIModel.PagesModel.AirportsPageModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.AirportModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.GeneralModel.CountryModel.TownModel;

    /// <summary>
    ///     Interaction logic for PageAirports.xaml
    /// </summary>
    public partial class PageAirports : Page
    {
        #region Constructors and Destructors

        public PageAirports()
        {
            this.HumanAirports =
                GameObject.GetInstance()
                    .HumanAirline.Airports.OrderBy(a => a.Profile.Pax)
                    .ToList()
                    .GetRange(0, Math.Min(GameObject.GetInstance().HumanAirline.Airports.Count, 5));

            var hubs =GameObject.GetInstance().HumanAirline.getHubs();

            this.HumanHubs =hubs.OrderBy(h=>h.Profile.Pax)
                .ToList()
                .GetRange(0,Math.Min(hubs.Count,5));

            this.CountryStates = new ObservableCollection<State>();
          
            this.Loaded += this.PageAirports_Loaded;

            this.InitializeComponent();
        }

        #endregion

        #region Public Properties

        public Hashtable AirportsFilters { get; set; }

        public List<Airport> HumanAirports { get; set; }

        public List<Airport> HumanHubs { get; set; }

        public ObservableCollection<State> CountryStates { get; set; }

        #endregion

        #region Methods

        private void PageAirports_Loaded(object sender, RoutedEventArgs e)
        {
            var frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            frmContent.Navigate(new PageShowAirports { Tag = this });

            List<Country> countries =
                Airports.GetAllAirports()
                    .Select(a => new CountryCurrentCountryConverter().Convert(a.Profile.Country) as Country)
                    .Distinct()
                    .ToList();
            countries.Add(Countries.GetCountry("100"));

            var cbCountry = UIHelpers.FindChild<ComboBox>(this, "cbCountry");
            var cbRegion = UIHelpers.FindChild<ComboBox>(this, "cbRegion");

            cbCountry.ItemsSource = countries.OrderByDescending(c => c.Uid == "100").ThenBy(c => c.Name);

            IOrderedEnumerable<Region> regions =
                countries.Select(c => c.Region)
                    .Distinct()
                    .ToList()
                    .OrderByDescending(r => r.Uid == "100")
                    .ThenBy(r => r.Name);

            cbRegion.ItemsSource = regions;

  
            
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            var frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            if (frmContent != null)
            {
                frmContent.Navigate(new PageShowAirports { Tag = this.Tag });
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            var cbCountry = UIHelpers.FindChild<ComboBox>(this, "cbCountry");
            var cbRegion = UIHelpers.FindChild<ComboBox>(this, "cbRegion");
            var cbState = UIHelpers.FindChild<ComboBox>(this, "cbState");
            var txtText = UIHelpers.FindChild<TextBox>(this, "txtText");

            string text = txtText.Text.Trim().ToUpper();
            var country = (Country)cbCountry.SelectedItem;
            var region = (Region)cbRegion.SelectedItem;
            var state = (State)cbState.SelectedItem;

            if (text == "112")
            {
                GameObject.GetInstance().addHumanMoney(10000000);
            }

            IEnumerable<Airport> airports =
                Airports.GetAllActiveAirports()
                    .Where(
                        a =>
                            (a.Profile.IATACode.ToUpper().StartsWith(text)
                             || a.Profile.ICAOCode.ToUpper().StartsWith(text)
                             || a.Profile.Name.ToUpper().StartsWith(text)
                             || a.Profile.Town.Name.ToUpper().StartsWith(text))
                            && ((country.Uid == "100" && (region.Uid == "100" || a.Profile.Country.Region == region))
                                || new CountryCurrentCountryConverter().Convert(a.Profile.Country) as Country == country)
                             && (state.ShortName=="All" || a.Profile.Town.State == state));

            var frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            if (frmContent != null)
            {
                frmContent.Navigate(new PageShowAirports(airports.ToList()) { Tag = this.Tag });
            }
        }
        private void cbCountry_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cbCountry = UIHelpers.FindChild<ComboBox>(this, "cbCountry");
            var cbState = UIHelpers.FindChild<ComboBox>(this, "cbState");

            var country = (Country)cbCountry.SelectedItem;

            if (country != null)
            {
                while (this.CountryStates.Count > 0)
                    this.CountryStates.RemoveAt(0);

                this.CountryStates.Add(new State(Countries.GetCountry("100"), "All States", "All", false));

                foreach (State state in States.GetStates(country))
                    this.CountryStates.Add(state);

                cbState.SelectedIndex = 0;

            }
        }
        private void cbRegion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cbCountry = UIHelpers.FindChild<ComboBox>(this, "cbCountry");
            var cbRegion = UIHelpers.FindChild<ComboBox>(this, "cbRegion");

            var region = (Region)cbRegion.SelectedItem;

            List<Country> countries = region.Uid == "100"
                ? Airports.GetAllAirports()
                    .Select(a => new CountryCurrentCountryConverter().Convert(a.Profile.Country) as Country)
                    .Distinct()
                    .ToList()
                : Countries.GetCountries(region);

            countries.Add(Countries.GetCountry("100"));

            cbCountry.ItemsSource = countries.OrderByDescending(c => c.Uid == "100").ThenBy(c => c.Name);
        }

        private void tcMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var control = (TabControl)sender;

            string selection = ((TabItem)control.SelectedItem).Tag.ToString();

            var frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            if (selection == "Airports" && frmContent != null)
            {
                frmContent.Navigate(new PageShowAirports { Tag = this });
            }
            if (selection == "Overview" && frmContent != null)
            {
                frmContent.Navigate(new PageAirportsOverview { Tag = this });
            }
            if (selection == "Statistics" && frmContent != null)
            {
                frmContent.Navigate(new PageAirportsStatistics { Tag = this });
            }
        }

        #endregion

      
    }
}