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
using System.Windows.Shapes;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpManufactorerOrder.xaml
    /// </summary>
    public partial class PopUpManufacturerOrder : PopUpWindow
    {
        private Dictionary<AirlinerType, int> orders;
        private ListBox lbOrders;
        private ComboBox cbTypes, cbManufacturers;
        private TextBlock txtPrice, txtTotalPrice, txtDiscount;
        private ucNumericUpDown nudAirliners;
        public static object ShowPopUp(Manufacturer manufacturer)
        {
            PopUpWindow window = new PopUpManufacturerOrder(manufacturer);
            window.ShowDialog();

            return window.Selected;
        }
        public PopUpManufacturerOrder(Manufacturer manufacturer)
        {
            this.orders = new Dictionary<AirlinerType, int>();

            InitializeComponent();

            this.Width = 600;

            this.Height = 400;

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            this.Title = "Manufacturer Order";

            StackPanel mainPanel = new StackPanel();
            mainPanel.Margin = new Thickness(10, 10, 10, 10);

            ListBox lbManufacturers = new ListBox();
            lbManufacturers.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbManufacturers.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            cbManufacturers = new ComboBox();
            cbManufacturers.DisplayMemberPath = "Name";
            cbManufacturers.SelectedValuePath = "Name";
            cbManufacturers.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbManufacturers.Background = Brushes.Transparent;
            cbManufacturers.Width = 200;
            cbManufacturers.SelectionChanged += new SelectionChangedEventHandler(cbManufacturers_SelectionChanged);

            (from a in AirlinerTypes.GetTypes() where a.Produced.From<=GameObject.GetInstance().GameTime.Year && a.Produced.To>=GameObject.GetInstance().GameTime.Year orderby a.Manufacturer.Name select a.Manufacturer).Distinct().ToList().ForEach(m=>cbManufacturers.Items.Add(m));

            lbManufacturers.Items.Add(new QuickInfoValue("Manufacturer", cbManufacturers));
            lbManufacturers.Items.Add(new QuickInfoValue("Add order", createOrderPanel()));

            mainPanel.Children.Add(lbManufacturers);

            TextBlock txtOrders = UICreator.CreateTextBlock("Orders");
            txtOrders.FontWeight = FontWeights.Bold;
            txtOrders.FontSize = 14;
            txtOrders.Margin = new Thickness(0, 5, 0, 0);

            mainPanel.Children.Add(txtOrders);

            lbOrders = new ListBox();
            lbOrders.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbOrders.ItemTemplate = this.Resources["AirlinerOrderItem"] as DataTemplate;
            lbOrders.MaxHeight = this.Height / 2;
              
            mainPanel.Children.Add(lbOrders);

            ListBox lbPrice = new ListBox();
            lbPrice.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbPrice.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");
           
            txtDiscount = new TextBlock();
            txtTotalPrice = new TextBlock();

            lbPrice.Items.Add(new QuickInfoValue("Discount", txtDiscount));
            lbPrice.Items.Add(new QuickInfoValue("Total price", txtTotalPrice));

            mainPanel.Children.Add(lbPrice);
            mainPanel.Children.Add(createButtonsPanel());

            this.Content = mainPanel;

            showOrders();

            cbManufacturers.SelectedItem = manufacturer;
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
        //creates the panel for the buttons
        private WrapPanel createButtonsPanel()
        {
            WrapPanel panelButtons = new WrapPanel();
            panelButtons.Margin = new Thickness(0, 10, 0, 0);

            Button btnOk = new Button();
            btnOk.Uid = "100";
            btnOk.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnOk.Height = Double.NaN;
            btnOk.Width = Double.NaN;
            btnOk.Content = Translator.GetInstance().GetString("General", btnOk.Uid);
            btnOk.Click += new RoutedEventHandler(btnOk_Click);
            btnOk.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");

            panelButtons.Children.Add(btnOk);

            Button btnCancel = new Button();
            btnCancel.Uid = "101";
            btnCancel.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnCancel.Height = Double.NaN;
            btnCancel.Width = Double.NaN;
            btnCancel.Content = Translator.GetInstance().GetString("General", btnCancel.Uid);
            btnCancel.Click += new RoutedEventHandler(btnCancel_Click);
            btnCancel.Margin = new Thickness(5, 0, 0, 0);
            btnCancel.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");

            panelButtons.Children.Add(btnCancel);

            return panelButtons;
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
                airliners+=order.Value;
            }

            cbManufacturers.IsHitTestVisible = orders.Keys.Count == 0;
            cbManufacturers.Focusable = orders.Keys.Count == 0;
                      
            txtDiscount.Text = string.Format("{0:C}", price * GeneralHelpers.GetAirlinerOrderDiscount(airliners));
            txtTotalPrice.Text = string.Format("{0:C}",price * ((1 - GeneralHelpers.GetAirlinerOrderDiscount(airliners))));


     
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = null;
            this.Close();
        }
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {

            this.Selected = this.orders;
            this.Close();

        }
        private void cbManufacturers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (cbTypes != null)
            {
                Manufacturer manufacturer = (Manufacturer)e.AddedItems[0];
                cbTypes.Items.Clear();

                (from t in AirlinerTypes.GetTypes() where t.Produced.From <= GameObject.GetInstance().GameTime.Year && t.Produced.To >= GameObject.GetInstance().GameTime.Year && t.Manufacturer == manufacturer orderby t.Name select t).ToList().ForEach(m => cbTypes.Items.Add(m));

                cbTypes.SelectedIndex = 0;
            }
        }
        private void cbTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AirlinerType type = (AirlinerType)cbTypes.SelectedItem;

            if (type != null)
            {
                int number = Convert.ToInt16(nudAirliners.Value);

                txtPrice.Text = string.Format("{0:C}", type.Price * number);
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
