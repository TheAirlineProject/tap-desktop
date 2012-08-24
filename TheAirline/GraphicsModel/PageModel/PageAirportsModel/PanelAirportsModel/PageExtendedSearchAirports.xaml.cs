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
        private ComboBox cbAirport, cbDistanceToAirport;
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

            WrapPanel panelAirportWithin = new WrapPanel();

            cbDistanceToAirport = new ComboBox();
            cbDistanceToAirport.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbDistanceToAirport.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right;
            cbDistanceToAirport.Width = 100;

            addDistanceItem(5);
            addDistanceItem(10);
            addDistanceItem(25);
            addDistanceItem(50);
            addDistanceItem(100);
            addDistanceItem(250);
            addDistanceItem(500);
            addDistanceItem(1000);
            addDistanceItem(1500);
            addDistanceItem(2000);
            addDistanceItem(2500);
            addDistanceItem(5000);
            addDistanceItem(7500);
            addDistanceItem(10000);
            addDistanceItem(15000);
            cbDistanceToAirport.Items.Add(Translator.GetInstance().GetString("General", "111"));

            cbDistanceToAirport.SelectedIndex = cbDistanceToAirport.Items.Count-1;

            panelAirportWithin.Children.Add(cbDistanceToAirport);

            TextBlock txtOf = UICreator.CreateTextBlock(Translator.GetInstance().GetString("PageExtendedSearchAirports", "1003"));
            txtOf.Margin = new Thickness(5,0,5,0);
            txtOf.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            panelAirportWithin.Children.Add(txtOf);

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

            panelAirportWithin.Children.Add(cbAirport);

            lbContent.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageExtendedSearchAirports", "1002"), panelAirportWithin));
            
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

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
          
            Airport airport = (Airport)cbAirport.SelectedItem;
            double distance;
            if (cbDistanceToAirport.SelectedItem is ComboBoxItem)
            {
                distance = (double)((ComboBoxItem)cbDistanceToAirport.SelectedItem).Tag;
            }
            else
                distance = double.MaxValue;

            List<Airport> airports = Airports.GetAirports(a => MathHelpers.GetDistance(airport.Profile.Coordinates, a.Profile.Coordinates) < distance); 

            this.ParentPage.showAirports(airports);
        }
        //adds an item to the distance box
        private void addDistanceItem(double distance)
        {
            ComboBoxItem cbItem = new ComboBoxItem();
            cbItem.Content = string.Format("{0} {1}",distance,new StringToLanguageConverter().Convert("km."));
            cbItem.Tag = distance;

            cbDistanceToAirport.Items.Add(cbItem);
        }
    }
}
