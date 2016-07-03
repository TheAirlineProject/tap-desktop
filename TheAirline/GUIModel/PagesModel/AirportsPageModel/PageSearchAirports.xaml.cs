using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TheAirline.GUIModel.CustomControlsModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Helpers;
using TheAirline.Models.Airports;
using TheAirline.Models.General.Countries;

namespace TheAirline.GUIModel.PagesModel.AirportsPageModel
{
    /// <summary>
    ///     Interaction logic for PageSearchAirports.xaml
    /// </summary>
    public partial class PageSearchAirports : Page
    {
        #region Constructors and Destructors

        public PageSearchAirports()
        {
            InitializeComponent();

            List<Country> countries =
                Airports.GetAllAirports()
                    .Select(a => new CountryCurrentCountryConverter().Convert(a.Profile.Country) as Country)
                    .Distinct()
                    .ToList();
            countries.Add(Countries.GetCountry("100"));

            cbCountry.ItemsSource = countries.OrderByDescending(c => c.Uid == "100").ThenBy(c => c.Name);

            IOrderedEnumerable<Region> regions =
                countries.Select(c => c.Region)
                    .Distinct()
                    .ToList()
                    .OrderByDescending(r => r.Uid == "100")
                    .ThenBy(r => r.Name);

            cbRegion.ItemsSource = regions;
        }

        #endregion

        #region Public Properties

        public List<GeneralHelpers.Size> SizesList
        {
            get
            {
                return Enum.GetValues(typeof(GeneralHelpers.Size)).Cast<GeneralHelpers.Size>().Select(v => v).ToList();
            }
        }

        #endregion

        #region Methods

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            var size = (GeneralHelpers.Size)ccAirportSize.SelectedValue;
            ComparerControl.CompareTypes compareType = ccAirportSize.SelectedCompareType;

            string text = txtText.Text.Trim();
            var country = (Country)cbCountry.SelectedItem;
            var region = (Region)cbRegion.SelectedItem;

            IEnumerable<Airport> airports =
                Airports.GetAllAirports()
                    .Where(
                        a =>
                            a.Profile.IATACode.StartsWith(text)
                            && ((country.Uid == "100" && (region.Uid == "100" || a.Profile.Country.Region == region))
                                || new CountryCurrentCountryConverter().Convert(a.Profile.Country) as Country == country));

            if (compareType == ComparerControl.CompareTypes.Equal_to)
            {
                airports = airports.Where(a => a.Profile.Size == size);
            }

            if (compareType == ComparerControl.CompareTypes.Large_than)
            {
                airports = airports.Where(a => a.Profile.Size > size);
            }

            if (compareType == ComparerControl.CompareTypes.Lower_than)
            {
                airports = airports.Where(a => a.Profile.Size < size);
            }

            var frmContent = UIHelpers.FindChild<Frame>(Tag as Page, "frmContent");

            if (frmContent != null)
            {
                frmContent.Navigate(new PageShowAirports(airports.ToList()) { Tag = Tag });
            }
        }

        private void cbRegion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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

        #endregion
    }
}