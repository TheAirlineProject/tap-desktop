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
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.Converters;
using TheAirline.Model.AirlinerModel;

namespace TheAirline.GraphicsModel.PageModel.PageAirlinerModel.PanelAirlinersModel
{
    /// <summary>
    /// Interaction logic for PageSearchAirliners.xaml
    /// </summary>
    public partial class PageSearchAirliners : Page
    {
        private ComboBox cbRange, cbCapacity, cbPrice,cbYear, cbCompareRange, cbCompareCapacity, cbComparePrice, cbCompareYear, cbManufacturers;
        private enum CompareType { Larger_than, Lower_than, Equal_to,All }
        private PageAirliners ParentPage;
        public PageSearchAirliners(PageAirliners parent)
        {

            this.ParentPage = parent;

            InitializeComponent();

            StackPanel mainPanel = new StackPanel();
            mainPanel.Margin = new Thickness(0, 0, 5, 0);
  
            TextBlock txtHeader = new TextBlock();
            txtHeader.Uid = "1001";
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PageSearchAirliners", txtHeader.Uid);
            mainPanel.Children.Add(txtHeader);

            ListBox lbSearch = new ListBox();
            lbSearch.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbSearch.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");
            mainPanel.Children.Add(lbSearch);
            
            cbManufacturers = new ComboBox();
            cbManufacturers.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbManufacturers.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbManufacturers.ItemTemplate = this.Resources["ManufacturerItem"] as DataTemplate;
            cbManufacturers.Width = 200;

            lbSearch.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageSearchAirliners", "1006"), cbManufacturers));

            cbManufacturers.Items.Add(new Manufacturer("All","All",null));

            (from a in Airliners.GetAirlinersForSale() orderby a.Type.Manufacturer.Name select a.Type.Manufacturer).Distinct().ToList().ForEach(m => cbManufacturers.Items.Add(m));
           // (from a in AirlinerTypes.GetAllTypes() where a.Produced.From <= GameObject.GetInstance().GameTime && a.Produced.To >= GameObject.GetInstance().GameTime orderby a.Manufacturer.Name select a.Manufacturer).Distinct().ToList().ForEach(m => cbManufacturers.Items.Add(m));

            cbManufacturers.SelectedIndex = 0;

            WrapPanel panelRange = new WrapPanel();

            cbCompareRange = new ComboBox();
            createCompareComboBox(cbCompareRange);
            panelRange.Children.Add(cbCompareRange);

            cbRange = new ComboBox();
            cbRange.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbRange.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbRange.Width = 150;
            panelRange.Children.Add(cbRange);

            for (int i = 500; i < 12500; i += 500)
                addRangeItem(i);

            cbRange.SelectedIndex = cbRange.Items.Count - 1;

            lbSearch.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageSearchAirliners","1002"), panelRange));

            WrapPanel panelCapacity = new WrapPanel();

            cbCompareCapacity = new ComboBox();
            createCompareComboBox(cbCompareCapacity);
            panelCapacity.Children.Add(cbCompareCapacity);

            cbCapacity = new ComboBox();
            cbCapacity.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbCapacity.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbCapacity.Width = 150;
            panelCapacity.Children.Add(cbCapacity);

            for (int i = 50; i < 500; i += 50)
                addCapacityItem(i);

            cbCapacity.SelectedIndex = cbCapacity.Items.Count - 1;

            lbSearch.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageSearchAirliners", "1003"), panelCapacity));

            WrapPanel panelPrice = new WrapPanel();

            cbComparePrice = new ComboBox();
            createCompareComboBox(cbComparePrice);
            panelPrice.Children.Add(cbComparePrice);

            cbPrice = new ComboBox();
            cbPrice.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbPrice.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbPrice.Width = 150;
            panelPrice.Children.Add(cbPrice);

            addPriceItem(1000000);
            addPriceItem(5000000);
            addPriceItem(10000000);
            addPriceItem(25000000);
            addPriceItem(50000000);
            addPriceItem(100000000);
            addPriceItem(250000000);

            cbPrice.SelectedIndex = cbPrice.Items.Count - 1;

            lbSearch.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageSearchAirliners", "1004"), panelPrice));

            WrapPanel panelYear = new WrapPanel();

            cbCompareYear = new ComboBox();
            createCompareComboBox(cbCompareYear);
            panelYear.Children.Add(cbCompareYear);

            cbYear = new ComboBox();
            cbYear.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbYear.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbYear.Width = 150;
            panelYear.Children.Add(cbYear);

            int startYear = (from a in Airliners.GetAirlinersForSale() select a.BuiltDate.Year).Min(); 

            for (int i = GameObject.GetInstance().GameTime.Year; i >= startYear; i--)
                cbYear.Items.Add(i);

            cbYear.SelectedItem = startYear;

            lbSearch.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageSearchAirliners", "1005"), panelYear));

               Button btnSearch = new Button();
            btnSearch.Uid = "109";
            btnSearch.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnSearch.Height = Double.NaN;
            btnSearch.Width = Double.NaN;
            btnSearch.IsDefault = true;
            btnSearch.Content = Translator.GetInstance().GetString("General", btnSearch.Uid);
            btnSearch.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnSearch.Margin = new Thickness(0, 5, 0, 0);
            btnSearch.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnSearch.Click += new RoutedEventHandler(btnSearch_Click);

            mainPanel.Children.Add(btnSearch);

            this.Content = mainPanel;
            
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
       
        //adds a price item to the list
        private void addPriceItem(long price)
        {
            ComboBoxItem cbItem = new ComboBoxItem();
            cbItem.Content = new ValueCurrencyConverter().Convert(price).ToString();//string.Format("{0:c}", price);
            cbItem.Tag = price;

            cbPrice.Items.Add(cbItem);
        }
        //adds a range item to the list
        private void addRangeItem(double range)
        {
            ComboBoxItem cbItem = new ComboBoxItem();
            cbItem.Content = string.Format("{0} {1}", range, new StringToLanguageConverter().Convert("km."));
            cbItem.Tag = range;

            cbRange.Items.Add(cbItem);
        }
        //adds a capacity item to the list
        public void addCapacityItem(int capacity)
        {
            ComboBoxItem cbItem = new ComboBoxItem();
            cbItem.Content = string.Format("{0}", capacity);
            cbItem.Tag = capacity;

            cbCapacity.Items.Add(cbItem);

        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Manufacturer manufacturer = ((Manufacturer)cbManufacturers.SelectedItem);

            List<Airliner> airliners = manufacturer.Name == "All" ? Airliners.GetAirlinersForSale() : Airliners.GetAirlinersForSale(a=>a.Type.Manufacturer == manufacturer);

            CompareType rangeCompare = (CompareType)(((ComboBoxItem)cbCompareRange.SelectedItem).Tag);
            CompareType priceCompare = (CompareType)(((ComboBoxItem)cbComparePrice.SelectedItem).Tag);
            CompareType capacityCompare = (CompareType)(((ComboBoxItem)cbCompareCapacity.SelectedItem).Tag);
            CompareType yearCompare = (CompareType)(((ComboBoxItem)cbCompareYear.SelectedItem).Tag);

            int capacity = (int)(((ComboBoxItem)cbCapacity.SelectedItem).Tag);
            long price = (long)(((ComboBoxItem)cbPrice.SelectedItem).Tag);
            double range = (double)(((ComboBoxItem)cbRange.SelectedItem).Tag);

            if (AppSettings.GetInstance().getLanguage().Unit == TheAirline.Model.GeneralModel.Language.UnitSystem.Imperial)
                range = MathHelpers.MilesToKM(range);
            
            int year = (int)(cbYear.SelectedItem);

            switch (rangeCompare)
            {
                case CompareType.Equal_to:
                    airliners = airliners.FindAll(a => a.Type.Range == range);
                    break;
                case CompareType.Larger_than:
                    airliners = airliners.FindAll(a => a.Type.Range > range);
                    break;
                case CompareType.Lower_than:
                    airliners = airliners.FindAll(a => a.Type.Range < range);
                    break;
            }


            switch (priceCompare)
            {
                case CompareType.Equal_to:
                    airliners = airliners.FindAll(a => a.getPrice() == price);
                    break;
                case CompareType.Larger_than:
                    airliners = airliners.FindAll(a => a.getPrice() > price);
                    break;
                case CompareType.Lower_than:
                    airliners = airliners.FindAll(a => a.getPrice() < price);
                    break;
            }


            switch (capacityCompare)
            {
                case CompareType.Equal_to:
                    airliners = airliners.FindAll(a => a.Type is AirlinerPassengerType && ((AirlinerPassengerType)a.Type).MaxSeatingCapacity == capacity);
                    break;
                case CompareType.Larger_than:
                    airliners = airliners.FindAll(a => a.Type is AirlinerPassengerType && ((AirlinerPassengerType)a.Type).MaxSeatingCapacity > capacity);
                    break;
                case CompareType.Lower_than:
                    airliners = airliners.FindAll(a => a.Type is AirlinerPassengerType && ((AirlinerPassengerType)a.Type).MaxSeatingCapacity < capacity);
                    break;
            }

            switch (yearCompare)
            {
                case CompareType.Equal_to:
                    airliners = airliners.FindAll(a => a.BuiltDate.Year == year);
                    break;
                case CompareType.Larger_than:
                    airliners = airliners.FindAll(a => a.BuiltDate.Year > year);
                    break;
                case CompareType.Lower_than:
                    airliners = airliners.FindAll(a => a.BuiltDate.Year < year);
                    break;
            }

            this.ParentPage.showUsedAirliners(airliners);
        }

    }
}
