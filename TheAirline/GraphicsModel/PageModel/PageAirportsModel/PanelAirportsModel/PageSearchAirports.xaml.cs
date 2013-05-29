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
using TheAirline.GraphicsModel.Converters;
using TheAirline.GraphicsModel.PageModel.GeneralModel;

namespace TheAirline.GraphicsModel.PageModel.PageAirportsModel.PanelAirportsModel
{
    /// <summary>
    /// Interaction logic for PageSearchAirports.xaml
    /// </summary>
    public partial class PageSearchAirports : Page
    {
        private TextBox txtTextSearch;
        private CheckBox cbHumanAirports, cbHubs;
        private PageAirports ParentPage;
        private enum CompareType { Larger_than, Lower_than, Equal_to, All }
        private ComboBox cbCountry, cbRegion, cbSize, cbCompareSize;
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

            ListBox lbSearch = new ListBox();
            lbSearch.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbSearch.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");
            panelSearch.Children.Add(lbSearch);
            
            WrapPanel panelCheckBoxes = new WrapPanel();
            lbSearch.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageSearchAirports","1004"), panelCheckBoxes));
            
            cbHumanAirports = new CheckBox();
            cbHumanAirports.Uid = "1002";
            cbHumanAirports.Content = Translator.GetInstance().GetString("PageSearchAirports", cbHumanAirports.Uid);
            cbHumanAirports.IsChecked = false;
            cbHumanAirports.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbHumanAirports.FlowDirection = System.Windows.FlowDirection.RightToLeft;
            
            panelCheckBoxes.Children.Add(cbHumanAirports);

            cbHubs = new CheckBox();
            cbHubs.Uid = "1003";
            cbHubs.Content = Translator.GetInstance().GetString("PageSearchAirports", cbHubs.Uid);
            cbHubs.IsChecked = false;
            cbHubs.FlowDirection = System.Windows.FlowDirection.RightToLeft;
            cbHubs.Margin = new Thickness(5, 0, 0, 0);

            panelCheckBoxes.Children.Add(cbHubs);

            cbRegion = new ComboBox();
            cbRegion.DisplayMemberPath = "Name";
            cbRegion.SelectedValuePath = "Name";
            cbRegion.Width = 250;
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

            lbSearch.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageSearchAirports", "1005"), cbRegion));

            cbCountry = new ComboBox();
            cbCountry.SetResourceReference(ComboBox.ItemTemplateProperty, "CountryFlagLongItem");
            cbCountry.Width = 250;
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

            lbSearch.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageSearchAirports", "1006"), cbCountry));

          
     
            WrapPanel panelSizes = new WrapPanel();

            cbCompareSize = new ComboBox();
            createCompareComboBox(cbCompareSize);
            panelSizes.Children.Add(cbCompareSize);

            cbSize = new ComboBox();
            cbSize.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbSize.Background = Brushes.Transparent;
            cbSize.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbSize.SetResourceReference(ComboBox.ItemTemplateProperty, "TextUnderscoreTextBlock");
            cbSize.Width = 100;

            foreach (GeneralHelpers.Size type in Enum.GetValues(typeof(GeneralHelpers.Size)))
                cbSize.Items.Add(type);

            cbSize.SelectedIndex = 0;
                   
            panelSizes.Children.Add(cbSize);

            lbSearch.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageSearchAirports", "1007"), panelSizes));

            txtTextSearch = new TextBox();
            txtTextSearch.Width = 300;
            txtTextSearch.Background = Brushes.Transparent;
            txtTextSearch.Foreground = Brushes.White;
            txtTextSearch.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            lbSearch.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageSearchAirports", "1008"), txtTextSearch));

            WrapPanel panelButtons = new WrapPanel();
            panelButtons.Margin = new Thickness(0, 5, 0, 0);
            panelButtons.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            panelSearch.Children.Add(panelButtons);

            Button btnSearch = new Button();
            btnSearch.Uid = "109";
            btnSearch.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnSearch.Height = Double.NaN;
            btnSearch.Width = Double.NaN;
            btnSearch.IsDefault = true;
            btnSearch.Content = Translator.GetInstance().GetString("General", btnSearch.Uid);
            btnSearch.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnSearch.Click += new RoutedEventHandler(btnSearch_Click);
            
            panelButtons.Children.Add(btnSearch);

            Button btnClear = new Button();
            btnClear.Uid = "110";
            btnClear.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnClear.Width = Double.NaN;
            btnClear.Height = Double.NaN;
            btnClear.Margin = new Thickness(10, 0, 0, 0);
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
            cbHubs.IsChecked = false;
            cbCountry.SelectedIndex = 0;
            cbRegion.SelectedIndex = 0;
            cbSize.SelectedIndex = 0;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string searchText = txtTextSearch.Text.ToUpper();

        
            Country country = (Country)cbCountry.SelectedItem;
            Region region = (Region)cbRegion.SelectedItem;

            Boolean humansOnly = cbHumanAirports.IsChecked.Value;
            Boolean hubsOnly = cbHubs.IsChecked.Value;

            List<Airport> airports = Airports.GetAllActiveAirports();

            if (humansOnly) airports = airports.FindAll(delegate(Airport airport) { return GameObject.GetInstance().HumanAirline.Airports.Contains(airport); });

            if (hubsOnly) airports = airports.FindAll(delegate(Airport airport) { return airport.IsHub; });

            if (country.Uid != "100") airports = airports.FindAll(a => ((Country)new CountryCurrentCountryConverter().Convert(a.Profile.Country)) == country || a.Profile.Country == country);//airports.FindAll(delegate(Airport airport) { return airport.Profile.Country == country; });

            if (country.Uid == "100" && region.Uid != "100") 
                airports = airports.FindAll(delegate(Airport airport) { return airport.Profile.Country.Region == region; });

            CompareType sizeCompare = (CompareType)(((ComboBoxItem)cbCompareSize.SelectedItem).Tag);
            GeneralHelpers.Size size = (GeneralHelpers.Size)cbSize.SelectedItem;

            switch (sizeCompare)
            {
                case CompareType.Equal_to:
                    airports = airports.FindAll(a => a.Profile.Size == size);
                    break;
                case CompareType.Larger_than:
                    airports = airports.FindAll(a => a.Profile.Size > size);
                    break;
                case CompareType.Lower_than:
                    airports = airports.FindAll(a => a.Profile.Size < size);
                    break;
            }

          
            airports = airports.FindAll(a => a.Profile.Name.ToUpper().Contains(searchText) || a.Profile.ICAOCode.ToUpper().Contains(searchText) || a.Profile.IATACode.ToUpper().Contains(searchText) || a.Profile.Town.Name.ToUpper().Contains(searchText) || ((Country)new CountryCurrentCountryConverter().Convert(a.Profile.Country)).Name.ToUpper().Contains(searchText));
           // airports = airports.FindAll((delegate(Airport airport) { return airport.Profile.Name.ToUpper().Contains(searchText) || airport.Profile.ICAOCode.ToUpper().Contains(searchText) || airport.Profile.IATACode.ToUpper().Contains(searchText) || airport.Profile.Town.ToUpper().Contains(searchText) || airport.Profile.Country.Name.ToUpper().Contains(searchText); }));

            this.ParentPage.showAirports(airports);
        }
        //creats a compare type combo box
        private void createCompareComboBox(ComboBox cbCompare)
        {
            cbCompare.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbCompare.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbCompare.Margin = new Thickness(0, 0, 5, 0);
            cbCompare.Width = 100;

            foreach (CompareType type in Enum.GetValues(typeof(CompareType)))
            {
                ComboBoxItem cbItem = new ComboBoxItem();
                cbItem.Content = new TextUnderscoreConverter().Convert(type);
                cbItem.Tag = type;
                cbCompare.Items.Add(cbItem);
            }

            cbCompare.SelectedIndex = cbCompare.Items.Count - 1;

        }
    }
}
