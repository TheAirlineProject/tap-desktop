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

namespace TheAirline.GUIModel.PagesModel.AirportsPageModel
{
    /// <summary>
    /// Interaction logic for PageSearchAirports.xaml
    /// </summary>
    public partial class PageSearchAirports : Page
    {
        public List<GeneralHelpers.Size> SizesList
        {
            get
            {
                return Enum.GetValues(typeof(GeneralHelpers.Size))
                    .Cast<GeneralHelpers.Size>()
                    .Select(v => v)
                    .ToList(); 
            }
        }

       

        public PageSearchAirports()
        {
            InitializeComponent();

            var countries = Airports.GetAllAirports().Select(a => new CountryCurrentCountryConverter().Convert(a.Profile.Country) as Country).Distinct().ToList();
            countries.Add(Countries.GetCountry("100"));

            cbCountry.ItemsSource = countries.OrderByDescending(c => c.Uid == "100").ThenBy(c => c.Name);

            var regions = countries.Select(c => c.Region).Distinct().ToList().OrderByDescending(r => r.Uid == "100").ThenBy(r => r.Name);

            cbRegion.ItemsSource = regions;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GeneralHelpers.Size size = (GeneralHelpers.Size)ccAirportSize.SelectedValue;
            ComparerControl.CompareTypes compareType = ccAirportSize.SelectedCompareType;
            
            string text = txtText.Text.Trim();
            Country country = (Country)cbCountry.SelectedItem;
            Region region = (Region)cbRegion.SelectedItem;

            var airports = Airports.GetAllAirports().Where(a => a.Profile.IATACode.StartsWith(text) && ((country.Uid == "100" && (region.Uid == "100" || a.Profile.Country.Region == region)) || new CountryCurrentCountryConverter().Convert(a.Profile.Country) as Country == country));

            if (compareType == ComparerControl.CompareTypes.Equal_to)
                airports = airports.Where(a => a.Profile.Size == size);

            if (compareType == ComparerControl.CompareTypes.Large_than)
                airports = airports.Where(a => a.Profile.Size > size);

            if (compareType == ComparerControl.CompareTypes.Lower_than)
                airports = airports.Where(a => a.Profile.Size < size);

            Frame frmContent = UIHelpers.FindChild<Frame>(this.Tag as Page, "frmContent");

            if (frmContent != null)
            {
               frmContent.Navigate(new PageShowAirports(airports.ToList()) { Tag = this.Tag });

            }
        }

        private void cbRegion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Region region = (Region)cbRegion.SelectedItem;

            var countries = region.Uid == "100" ? Airports.GetAllAirports().Select(a => new CountryCurrentCountryConverter().Convert(a.Profile.Country) as Country).Distinct().ToList() : Countries.GetCountries(region);

            countries.Add(Countries.GetCountry("100"));

            cbCountry.ItemsSource = countries.OrderByDescending(c => c.Uid == "100").ThenBy(c => c.Name);

        }
    }
}
