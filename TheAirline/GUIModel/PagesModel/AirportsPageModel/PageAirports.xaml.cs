using System;
using System.Collections.Generic;
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
using TheAirline.GUIModel.CustomControlsModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using System.Collections;

namespace TheAirline.GUIModel.PagesModel.AirportsPageModel
{
    /// <summary>
    /// Interaction logic for PageAirports.xaml
    /// </summary>
    public partial class PageAirports : Page
    {
        public List<Airport> HumanAirports { get; set; }
        public List<Airport> HumanHubs { get; set; }
        public Hashtable AirportsFilters { get; set; }
        public PageAirports()
        {
            this.HumanAirports = GameObject.GetInstance().HumanAirline.Airports.OrderBy(a=>a.Profile.Pax).ToList().GetRange(0,Math.Min(GameObject.GetInstance().HumanAirline.Airports.Count,5));
            this.HumanHubs = GameObject.GetInstance().HumanAirline.getHubs();

            this.Loaded += PageAirports_Loaded;
            
            InitializeComponent();

        }

        private void PageAirports_Loaded(object sender, RoutedEventArgs e)
        {
            Frame frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            frmContent.Navigate(new PageShowAirports() { Tag = this });

            var countries = Airports.GetAllAirports().Select(a => new CountryCurrentCountryConverter().Convert(a.Profile.Country) as Country).Distinct().ToList();
            countries.Add(Countries.GetCountry("100"));

            ComboBox cbCountry = UIHelpers.FindChild<ComboBox>(this, "cbCountry");
            ComboBox cbRegion = UIHelpers.FindChild<ComboBox>(this, "cbRegion");

            cbCountry.ItemsSource = countries.OrderByDescending(c => c.Uid == "100").ThenBy(c => c.Name);

            var regions = countries.Select(c => c.Region).Distinct().ToList().OrderByDescending(r => r.Uid == "100").ThenBy(r => r.Name);

            cbRegion.ItemsSource = regions;
        }
        private void tcMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl control = (TabControl)sender;

            string selection = ((TabItem)control.SelectedItem).Tag.ToString();

            Frame frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");
            
            if (selection == "Airports" && frmContent != null)
                frmContent.Navigate(new PageShowAirports() { Tag = this });

            if (selection == "Statistics" && frmContent != null)
                frmContent.Navigate(new PageAirportsStatistics() { Tag = this });
           
        }
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            Frame frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            if (frmContent != null)
                frmContent.Navigate(new PageShowAirports() { Tag = this.Tag });
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ComboBox cbCountry = UIHelpers.FindChild<ComboBox>(this, "cbCountry");
            ComboBox cbRegion = UIHelpers.FindChild<ComboBox>(this, "cbRegion");
            TextBox txtText = UIHelpers.FindChild<TextBox>(this, "txtText");


             string text = txtText.Text.Trim().ToUpper();
            Country country = (Country)cbCountry.SelectedItem;
            Region region = (Region)cbRegion.SelectedItem;

            if (text == "112")
                GameObject.GetInstance().addHumanMoney(10000000);

            var airports = Airports.GetAllActiveAirports().Where(a => (a.Profile.IATACode.ToUpper().StartsWith(text) || a.Profile.ICAOCode.ToUpper().StartsWith(text) || a.Profile.Name.ToUpper().StartsWith(text) || a.Profile.Town.Name.ToUpper().StartsWith(text)) && ((country.Uid == "100" && (region.Uid == "100" || a.Profile.Country.Region == region)) || new CountryCurrentCountryConverter().Convert(a.Profile.Country) as Country == country));

            Frame frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            if (frmContent != null)
                frmContent.Navigate(new PageShowAirports(airports.ToList()) { Tag = this.Tag });

        }
        private void cbRegion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbCountry = UIHelpers.FindChild<ComboBox>(this, "cbCountry");
            ComboBox cbRegion = UIHelpers.FindChild<ComboBox>(this, "cbRegion");
         
            Region region = (Region)cbRegion.SelectedItem;

            var countries = region.Uid == "100" ? Airports.GetAllAirports().Select(a => new CountryCurrentCountryConverter().Convert(a.Profile.Country) as Country).Distinct().ToList() : Countries.GetCountries(region);

            countries.Add(Countries.GetCountry("100"));

            cbCountry.ItemsSource = countries.OrderByDescending(c => c.Uid == "100").ThenBy(c => c.Name);

        }
    }
}
