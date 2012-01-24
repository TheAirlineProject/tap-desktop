using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GraphicsModel.PageModel.PageAirportsModel.PanelAirportsModel
{
    /// <summary>
    /// Interaction logic for PageSearchAirports.xaml
    /// </summary>
    public partial class PageSearchAirports : Page
    {
        private TextBox txtTextSearch;
        private CheckBox cbHumanAirports;
        private PageAirports ParentPage;
        private ComboBox cbCountry, cbRegion, cbSize;
        public PageSearchAirports(PageAirports parent)
        {
            InitializeComponent();

            this.ParentPage = parent;

            StackPanel panelSearch = new StackPanel();
            panelSearch.Margin = new Thickness(0, 10, 50, 0);

            TextBlock txtHeader = new TextBlock();
            txtHeader.Uid = "1001";
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PageSearchAirports", txtHeader.Uid);

            panelSearch.Children.Add(txtHeader);

            cbHumanAirports = new CheckBox();
            cbHumanAirports.Uid = "1002";
            cbHumanAirports.Content = Translator.GetInstance().GetString("PageSearchAirports", cbHumanAirports.Uid);
            cbHumanAirports.IsChecked = false;
            cbHumanAirports.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbHumanAirports.FlowDirection = System.Windows.FlowDirection.RightToLeft;
            panelSearch.Children.Add(cbHumanAirports);

            cbRegion = new ComboBox();
            cbRegion.Margin = new Thickness(0, 5, 0, 0);
            cbRegion.DisplayMemberPath = "Name";
            cbRegion.SelectedValuePath = "Name";
            cbRegion.Background = Brushes.Transparent;
            cbRegion.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbRegion.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbRegion.SelectionChanged += new SelectionChangedEventHandler(cbRegions_SelectionChanged);

            List<Region> regions = Regions.GetRegions();
            regions.Sort(delegate(Region r1, Region r2){return r1.Name.CompareTo(r2.Name);});

            // 100 is the predefined Uid for "All Regions"
            Region regionAll = Regions.GetRegion("100");
            cbRegion.Items.Add(regionAll);

            foreach (Region region in regions)
                cbRegion.Items.Add(region);

            panelSearch.Children.Add(cbRegion);

            cbCountry = new ComboBox();
            cbCountry.SetResourceReference(ComboBox.ItemTemplateProperty, "CountryFlagLongItem");
            cbCountry.Margin = new Thickness(0, 5, 0, 0);
            //cbCountries.Style = this.Resources["ComboBoxStyle"] as Style;
            cbCountry.Background = Brushes.Transparent;
            cbCountry.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbCountry.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            List<Country> countries = Countries.GetCountries();
            countries.Sort(delegate(Country c1, Country c2) { return c1.Name.CompareTo(c2.Name); });

            // 100 is the predefined Uid for "All Countries"
            Country countryAll = Countries.GetCountry("100");
            cbCountry.Items.Add(countryAll);

            foreach (Country country in countries)
                cbCountry.Items.Add(country);

            cbCountry.SelectedItem = countryAll;
            cbRegion.SelectedItem = regionAll;

            panelSearch.Children.Add(cbCountry);

            cbSize = new ComboBox();
            cbSize.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbSize.Background = Brushes.Transparent;
            cbSize.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbSize.Margin = new Thickness(0, 5, 0, 0);

            cbSize.Items.Add("All sizes");

            foreach (AirportProfile.AirportSize type in Enum.GetValues(typeof(AirportProfile.AirportSize)))
                cbSize.Items.Add(type);

            cbSize.SelectedIndex = 0;

            panelSearch.Children.Add(cbSize);

            txtTextSearch = new TextBox();
            txtTextSearch.Width = 300;
            txtTextSearch.Margin = new Thickness(0, 5, 0, 0);
            txtTextSearch.Background = Brushes.Transparent;
            txtTextSearch.Foreground = Brushes.White;
            txtTextSearch.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            panelSearch.Children.Add(txtTextSearch);

            WrapPanel panelButtons = new WrapPanel();
            panelButtons.Margin = new Thickness(0, 5, 0, 0);
            panelButtons.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            panelSearch.Children.Add(panelButtons);

            Button btnSearch = new Button();
            btnSearch.Uid = "109";
            btnSearch.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnSearch.Height = Double.NaN;
            btnSearch.Width = Double.NaN;
            btnSearch.Content = Translator.GetInstance().GetString("General", btnSearch.Uid);
            btnSearch.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnSearch.Click += new RoutedEventHandler(btnSearch_Click);

            panelButtons.Children.Add(btnSearch);

            Button btnClear = new Button();
            btnClear.Uid = "110";
            btnClear.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnClear.Width = Double.NaN;
            btnClear.Height = Double.NaN;
            btnClear.Margin = new Thickness(5, 0, 0, 0);
            btnClear.Content = Translator.GetInstance().GetString("General", btnClear.Uid);
            btnClear.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnClear.Click += new RoutedEventHandler(btnClear_Click);

            panelButtons.Children.Add(btnClear);

            this.Content = panelSearch;
        }

        private void cbRegions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cbCountry.Items.Clear();

            Region region = (Region)((ComboBox)sender).SelectedItem;

            List<Country> countries = Countries.GetCountries();
            countries.Sort(delegate(Country c1, Country c2) { return c1.Name.CompareTo(c2.Name); });

            // 100 is the predefined Uid for "All Regions"
            if (region.Uid != "100") countries = countries.FindAll(delegate(Country country) { return country.Region == region; });

            // 100 is the predefined Uid for "All Countries"
            Country countryAll = Countries.GetCountry("100");
            cbCountry.Items.Add(countryAll);

            foreach (Country country in countries)
                cbCountry.Items.Add(country);

            cbCountry.SelectedItem = countryAll;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtTextSearch.Text = "";
            cbHumanAirports.IsChecked = false;
            cbCountry.SelectedIndex = 0;
            cbRegion.SelectedIndex = 0;
            cbSize.SelectedIndex = 0;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string searchText = txtTextSearch.Text.ToUpper();

            string size = cbSize.SelectedItem.ToString();

            Country country = (Country)cbCountry.SelectedItem;

            Boolean humansOnly = cbHumanAirports.IsChecked.Value;

            List<Airport> airports = Airports.GetAirports();

            if (humansOnly) airports = airports.FindAll(delegate(Airport airport) { return GameObject.GetInstance().HumanAirline.Airports.Contains(airport); });

            if (country.Uid != "100") airports = airports.FindAll(delegate(Airport airport) { return airport.Profile.Country == country; });

            if (country.Uid == "100" && country.Region.Uid != "100") airports = airports.FindAll(delegate(Airport airport) { return airport.Profile.Country.Region == country.Region; });

            if (size != "All sizes") airports = airports.FindAll(delegate(Airport airport) { return airport.Profile.Size.ToString() == size; });

            airports = airports.FindAll((delegate(Airport airport) { return airport.Profile.Name.ToUpper().Contains(searchText) || airport.Profile.ICAOCode.ToUpper().Contains(searchText) || airport.Profile.IATACode.ToUpper().Contains(searchText) || airport.Profile.Town.ToUpper().Contains(searchText) || airport.Profile.Country.Name.ToUpper().Contains(searchText); }));

            this.ParentPage.showAirports(airports);
        }
    }
}
