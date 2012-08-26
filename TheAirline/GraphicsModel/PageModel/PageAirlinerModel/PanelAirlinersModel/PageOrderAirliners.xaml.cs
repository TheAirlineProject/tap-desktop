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
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.UserControlModel;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.Model.AirportModel;

namespace TheAirline.GraphicsModel.PageModel.PageAirlinerModel.PanelAirlinersModel
{
    /// <summary>
    /// Interaction logic for PageOrderAirliner.xaml
    /// </summary>
    public partial class PageOrderAirliners : Page
    {
        private TextBlock txtPrice, txtTotalPrice, txtDiscount;
        private ListBox lbOrders;
        private ucNumericUpDown nudAirliners;
        private ComboBox cbTypes, cbAirport;
        private CheckBox cbDownPayment;//<-
        private Frame frameAirlinerInfo;
        private Dictionary<AirlinerType, int> orders;
        private DatePicker dpDate;
        private Manufacturer Manufacturer;
        public PageOrderAirliners(Manufacturer manufacturer)
        {
            this.Manufacturer = manufacturer;

            this.orders = new Dictionary<AirlinerType, int>();

            InitializeComponent();

            StackPanel mainPanel = new StackPanel();
            mainPanel.Margin = new Thickness(0, 0, 5, 0);

            TextBlock txtHeader = new TextBlock();
            txtHeader.Uid = "1001";
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PageOrderAirliners", txtHeader.Uid);

            mainPanel.Children.Add(txtHeader);

            ListBox lbManufacturers = new ListBox();
            lbManufacturers.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbManufacturers.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            cbAirport = new ComboBox();
            cbAirport.SetResourceReference(ComboBox.ItemTemplateProperty, "AirportCountryItem");
            cbAirport.Background = Brushes.Transparent;
            cbAirport.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbAirport.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            List<Airport> airports = GameObject.GetInstance().HumanAirline.Airports.FindAll(a => a.getCurrentAirportFacility(GameObject.GetInstance().HumanAirline, AirportFacility.FacilityType.Service).TypeLevel > 0);
            airports = airports.OrderBy(a => a.Profile.Name).ToList();

            foreach (Airport airport in airports)
                cbAirport.Items.Add(airport);

            cbAirport.SelectedIndex = 0;

            lbManufacturers.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageOrderAirliners", "1007"), cbAirport));

            DateTime firstDate = GameObject.GetInstance().GameTime.AddMonths(3);

            dpDate = new DatePicker();
            dpDate.SetResourceReference(DatePicker.CalendarStyleProperty, "CalendarPickerStyle");
            dpDate.DisplayDateStart = new DateTime(firstDate.Year, firstDate.Month, 1);
            dpDate.DisplayDateEnd = GameObject.GetInstance().GameTime.AddYears(5);
            dpDate.DisplayDate = firstDate;
            dpDate.SelectedDate = firstDate;

            for (int i = 1; i < firstDate.Day; i++)
                dpDate.BlackoutDates.Add(new CalendarDateRange(new DateTime(firstDate.Year, firstDate.Month, i)));


            lbManufacturers.Items.Add(new QuickInfoValue("Delivery time", dpDate));

            ContentControl ccManufacturer = new ContentControl();
            ccManufacturer.SetResourceReference(ContentControl.ContentTemplateProperty, "ManufactorerLogoItem");
            ccManufacturer.Content = this.Manufacturer;

            lbManufacturers.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageOrderAirliners", "1003"),ccManufacturer));
            lbManufacturers.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageOrderAirliners", "1004"), createOrderPanel()));

            mainPanel.Children.Add(lbManufacturers);

            TextBlock txtOrders = new TextBlock();
            txtOrders.FontWeight = FontWeights.Bold;
            txtOrders.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtOrders.Uid = "1002";
            txtOrders.Text = Translator.GetInstance().GetString("PageOrderAirliners", txtOrders.Uid);

            txtOrders.Margin = new Thickness(0, 5, 0, 0);

            mainPanel.Children.Add(txtOrders);

            lbOrders = new ListBox();
            lbOrders.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbOrders.ItemTemplate = this.Resources["AirlinerOrderItem"] as DataTemplate;
            lbOrders.MaxHeight = GraphicsHelpers.GetContentHeight() / 4;

            mainPanel.Children.Add(lbOrders);

            ListBox lbPrice = new ListBox();
            lbPrice.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbPrice.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            txtDiscount = new TextBlock();
            txtTotalPrice = new TextBlock();

            lbPrice.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageOrderAirliners", "1005"), txtDiscount));
            lbPrice.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageOrderAirliners", "1006"), txtTotalPrice));

            mainPanel.Children.Add(lbPrice);

            Button btnOrder = new Button();
            btnOrder.Uid = "200";
            btnOrder.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnOrder.Height = Double.NaN;
            btnOrder.Width = Double.NaN;
            btnOrder.Content = Translator.GetInstance().GetString("PageOrderAirliners", btnOrder.Uid);
            btnOrder.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnOrder.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnOrder.Margin = new Thickness(0, 5, 0, 0);
            btnOrder.Click += new RoutedEventHandler(btnOrder_Click);

            mainPanel.Children.Add(btnOrder);


            frameAirlinerInfo = new Frame();
            frameAirlinerInfo.Margin = new Thickness(0, 10, 0, 0);
            mainPanel.Children.Add(frameAirlinerInfo);


            this.Content = mainPanel;

            showOrders();

            (from t in AirlinerTypes.GetAllTypes() where t.Produced.From <= GameObject.GetInstance().GameTime && t.Produced.To >= GameObject.GetInstance().GameTime && t.Manufacturer == this.Manufacturer orderby t.Name select t).ToList().ForEach(m => cbTypes.Items.Add(m));

            cbTypes.SelectedIndex = 0;

        }

     
        //creates the panel for ordering of a type
        private Panel createOrderPanel()
        {
            WrapPanel panelOrderType = new WrapPanel();

            cbTypes = new ComboBox();
            cbTypes.DisplayMemberPath = "Name";
            cbTypes.SelectedValuePath = "Name";
            cbTypes.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbTypes.Background = Brushes.Transparent;
            cbTypes.Width = 200;
            cbTypes.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            cbTypes.SelectionChanged += new SelectionChangedEventHandler(cbTypes_SelectionChanged);

            panelOrderType.Children.Add(cbTypes);

            nudAirliners = new ucNumericUpDown();
            nudAirliners.Height = 30;
            nudAirliners.MaxValue = 10;
            nudAirliners.Value = 1;
            nudAirliners.ValueChanged += new RoutedPropertyChangedEventHandler<decimal>(nudAirliners_ValueChanged);
            nudAirliners.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            nudAirliners.MinValue = 1;
            nudAirliners.Margin = new Thickness(5, 0, 0, 0);

            panelOrderType.Children.Add(nudAirliners);

            txtPrice = new TextBlock();
            txtPrice.Text = string.Format("{0:C}", 0);
            txtPrice.TextAlignment = TextAlignment.Right;
            txtPrice.Width = 100;
            txtPrice.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

            panelOrderType.Children.Add(txtPrice);

            Button btnAddOrder = new Button();
            btnAddOrder.Margin = new Thickness(5, 0, 0, 0);
            btnAddOrder.Background = Brushes.Transparent;
            btnAddOrder.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            btnAddOrder.Click += new RoutedEventHandler(btnAddOrder_Click);

            Image imgAddOrder = new Image();
            imgAddOrder.Source = new BitmapImage(new Uri(@"/Data/images/add.png", UriKind.RelativeOrAbsolute));
            imgAddOrder.Height = 16;
            RenderOptions.SetBitmapScalingMode(imgAddOrder, BitmapScalingMode.HighQuality);

            btnAddOrder.Content = imgAddOrder;

            panelOrderType.Children.Add(btnAddOrder);

            return panelOrderType;

        }

        //shows the orders
        private void showOrders()
        {
            long price = 0;
            int airliners = 0;

            lbOrders.Items.Clear();

            foreach (KeyValuePair<AirlinerType, int> order in orders)
            {
                lbOrders.Items.Add(order);

                price += order.Key.Price * order.Value;
                airliners += order.Value;
            }

            txtDiscount.Text = string.Format("{0:C}", price * GeneralHelpers.GetAirlinerOrderDiscount(airliners));
            txtTotalPrice.Text = string.Format("{0:C}", price * ((1 - GeneralHelpers.GetAirlinerOrderDiscount(airliners))));



        }
        private void btnOrder_Click(object sender, RoutedEventArgs e)
        {
            Airport airport = (Airport)cbAirport.SelectedItem;

            if (airport != null)
            {
                int totalAmount = orders.Values.Sum();
                double price = orders.Keys.Sum(t => t.Price * orders[t]);

                double totalPrice = price * ((1 - GeneralHelpers.GetAirlinerOrderDiscount(totalAmount)));


                if (totalPrice > GameObject.GetInstance().HumanAirline.Money)
                {
                    WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2001"), Translator.GetInstance().GetString("MessageBox", "2001", "message"), WPFMessageBoxButtons.Ok);
                }
                else if (airport == null)
                    WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2002"), Translator.GetInstance().GetString("MessageBox", "2002", "message"), WPFMessageBoxButtons.Ok);
                else
                {
                    if (orders.Keys.Count > 0)
                    {
                        WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2008"), string.Format(Translator.GetInstance().GetString("MessageBox", "2008", "message"), totalPrice), WPFMessageBoxButtons.YesNo);

                        if (result == WPFMessageBoxResult.Yes)
                        {
                            AirlineHelpers.OrderAirliners(GameObject.GetInstance().HumanAirline, orders, airport, dpDate.SelectedDate.Value);
                            PageNavigator.NavigateTo(new PageAirliners());
                        }


                    }
                }
            }
        }  
        private void cbTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AirlinerType type = (AirlinerType)cbTypes.SelectedItem;

            if (type != null)
            {
                int number = Convert.ToInt16(nudAirliners.Value);

                txtPrice.Text = string.Format("{0:C}", type.Price * number);

                frameAirlinerInfo.Content = PanelAirliner.createQuickInfoPanel(type);
            }

        }

        private void nudAirliners_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
        {
            AirlinerType type = (AirlinerType)cbTypes.SelectedItem;

            int number = Convert.ToInt16(nudAirliners.Value);

            txtPrice.Text = string.Format("{0:C}", type.Price * number);

        }
        private void btnAddOrder_Click(object sender, RoutedEventArgs e)
        {
            AirlinerType type = (AirlinerType)cbTypes.SelectedItem;

            int number = Convert.ToInt16(nudAirliners.Value);

            if (this.orders.ContainsKey(type))
                this.orders[type] += number;
            else
                this.orders.Add(type, number);

            showOrders();
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            AirlinerType type = (AirlinerType)((Button)sender).Tag;

            this.orders.Remove(type);

            showOrders();
        }
    }

    //the converter for getting the price for a number of airliner types
    public class AirlinersPriceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            KeyValuePair<AirlinerType, int> airliners = (KeyValuePair<AirlinerType, int>)value;

            return airliners.Key.Price * airliners.Value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


}
