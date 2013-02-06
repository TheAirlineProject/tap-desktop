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
using System.ComponentModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.Converters;

namespace TheAirline.GraphicsModel.PageModel.PageAirportsModel.PanelAirportsModel
{
    /// <summary>
    /// Interaction logic for PageExtendedSearchAirports.xaml
    /// </summary>
    public partial class PageExtendedSearchAirports : Page
    {
        private PageAirports ParentPage;
        private ComboBox cbAirport,cbCompareDistance;
        private TextBox txtDistance;
        private enum CompareType { Larger_than, Lower_than, Equal_to }
        public PageExtendedSearchAirports(PageAirports parent)
        {
            this.ParentPage = parent;

            InitializeComponent();

            StackPanel panelSearch = new StackPanel();
            panelSearch.Margin = new Thickness(0, 10, 50, 0);

            TextBlock txtHeader = new TextBlock();
            txtHeader.Uid = "1001";
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PageExtendedSearchAirports", txtHeader.Uid);

            panelSearch.Children.Add(txtHeader);

            ListBox lbContent = new ListBox();
            lbContent.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbContent.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            panelSearch.Children.Add(lbContent);

            
            cbAirport = new ComboBox();
            cbAirport.SetResourceReference(ComboBox.ItemTemplateProperty, "AirportCountryItem");
            cbAirport.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbAirport.IsSynchronizedWithCurrentItem = true;
            cbAirport.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbAirport.Width = 200;
           
            List<Airport> airportsList = Airports.GetAllActiveAirports();

            ICollectionView airportsView = CollectionViewSource.GetDefaultView(airportsList);
            airportsView.SortDescriptions.Add(new SortDescription("Profile.Name", ListSortDirection.Ascending));

            cbAirport.ItemsSource = airportsView;
            cbAirport.SelectedIndex = 0;
                        
            lbContent.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageExtendedSearchAirports", "1002"), cbAirport));

            WrapPanel panelDistance = new WrapPanel();

            cbCompareDistance = new ComboBox();
            createCompareComboBox(cbCompareDistance);
            panelDistance.Children.Add(cbCompareDistance);

            txtDistance = new TextBox();
            txtDistance.PreviewTextInput += new TextCompositionEventHandler(txtDistance_PreviewTextInput);
            txtDistance.Width = 100;
            txtDistance.TextAlignment = TextAlignment.Right;
            txtDistance.Background = Brushes.Transparent;
            panelDistance.Children.Add(txtDistance);

            lbContent.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageExtendedSearchAirports", "1004"), panelDistance));

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


            this.Content = panelSearch;

        }

        private void txtDistance_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
             
            try
            {
                double distance = Convert.ToDouble(e.Text);

            }
            catch
            {
                e.Handled = true;
            }
       
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

            Airport airport = (Airport)cbAirport.SelectedItem;
            double distance = txtDistance.Text.Length > 0 ? Convert.ToDouble(txtDistance.Text) : 0;

            if (AppSettings.GetInstance().getLanguage().Unit == TheAirline.Model.GeneralModel.Language.UnitSystem.Imperial)
                distance = MathHelpers.MilesToKM(distance);
        

            CompareType comparer = (CompareType)((ComboBoxItem)cbCompareDistance.SelectedItem).Tag;
         
            if (AppSettings.GetInstance().getLanguage().Unit == Model.GeneralModel.Language.UnitSystem.Imperial)
                distance = MathHelpers.MilesToKM(distance);

            List<Airport> airports = new List<Airport>();

            if (comparer == CompareType.Equal_to)
                airports = Airports.GetAirports(a => MathHelpers.GetDistance(airport.Profile.Coordinates, a.Profile.Coordinates) == distance); 

            if (comparer == CompareType.Larger_than)
                airports = Airports.GetAirports(a => MathHelpers.GetDistance(airport.Profile.Coordinates, a.Profile.Coordinates) > distance); 

            if (comparer == CompareType.Lower_than)
                airports = Airports.GetAirports(a => MathHelpers.GetDistance(airport.Profile.Coordinates, a.Profile.Coordinates) < distance); 

            this.ParentPage.showAirports(airports);
        }
        //adds an item to the distance box
        private void addDistanceItem(double distance)
        {
            ComboBoxItem cbItem = new ComboBoxItem();
            cbItem.Content = string.Format("{0} {1}",distance,new StringToLanguageConverter().Convert("km."));
            cbItem.Tag = distance;

           // cbDistanceToAirport.Items.Add(cbItem);
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
