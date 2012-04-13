using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GraphicsModel.PageModel.PageAirlinerModel.PanelAirlinersModel
{
    public class PanelNewAirliner : PanelAirliner
    {
        private AirlinerType Airliner;
        private ComboBox cbAirport;
        private DatePicker dpDate;
        //private CheckBox cbPayOnDelivery;
        //private double downPaymentRate = 0.03;
        public PanelNewAirliner(PageAirliners parent, AirlinerType airliner)
            : base(parent)
        {


            this.Airliner = airliner;

            StackPanel panelAirliner = new StackPanel();

            panelAirliner.Children.Add(PanelAirliner.createQuickInfoPanel(airliner));

            this.addObject(panelAirliner);

            TextBlock txtHeader = new TextBlock();
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.Margin = new System.Windows.Thickness(0, 5, 0, 0);
            txtHeader.Text = "Price Information";
            txtHeader.FontWeight = FontWeights.Bold;

            this.addObject(txtHeader);


            ListBox lbPriceInfo = new ListBox();
            lbPriceInfo.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbPriceInfo.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            this.addObject(lbPriceInfo);

            lbPriceInfo.Items.Add(new QuickInfoValue("Price", UICreator.CreateTextBlock(string.Format("{0:c}", this.Airliner.Price))));
            lbPriceInfo.Items.Add(new QuickInfoValue("Yearly maintenance", UICreator.CreateTextBlock(string.Format("{0:c}", this.Airliner.getMaintenance()))));

            cbAirport = new ComboBox();
            cbAirport.SetResourceReference(ComboBox.ItemTemplateProperty, "AirportCountryItem");
            cbAirport.Background = Brushes.Transparent;
            cbAirport.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbAirport.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            List<Airport> airports = GameObject.GetInstance().HumanAirline.Airports.FindAll((delegate(Airport airport) { return airport.getAirportFacility(GameObject.GetInstance().HumanAirline, AirportFacility.FacilityType.Service).TypeLevel > 0; }));
            airports.Sort(delegate(Airport a1, Airport a2) { return a1.Profile.Name.CompareTo(a2.Profile.Name); });

            
            foreach (Airport airport in airports)
                cbAirport.Items.Add(airport);

            cbAirport.SelectedIndex = 0;

            lbPriceInfo.Items.Add(new QuickInfoValue("Select home base", cbAirport));

            DateTime firstDate = GameObject.GetInstance().GameTime.AddMonths(3);

            dpDate = new DatePicker();
            dpDate.SetResourceReference(DatePicker.CalendarStyleProperty, "CalendarPickerStyle");
            dpDate.DisplayDateStart = new DateTime(firstDate.Year, firstDate.Month, 1);
            dpDate.DisplayDateEnd = GameObject.GetInstance().GameTime.AddYears(5);
            dpDate.DisplayDate = firstDate;
            dpDate.SelectedDate = firstDate;

            for (int i = 1; i < firstDate.Day; i++)
                dpDate.BlackoutDates.Add(new CalendarDateRange(new DateTime(firstDate.Year, firstDate.Month, i)));


            lbPriceInfo.Items.Add(new QuickInfoValue("Delivery time", dpDate));

            /*
            WrapPanel panelOnDelivery = new WrapPanel();
            panelOnDelivery.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

            cbPayOnDelivery = new CheckBox();
            cbPayOnDelivery.IsChecked = false;
            cbPayOnDelivery.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

            panelOnDelivery.Children.Add(cbPayOnDelivery);
            
            TextBlock txtDeliveryPrice = UICreator.CreateTextBlock(string.Format("Down payment: {0:c}", this.Airliner.Price * this.downPaymentRate));
            txtDeliveryPrice.Margin = new Thickness(5, 0, 0, 0);

            panelOnDelivery.Children.Add(txtDeliveryPrice);

            lbPriceInfo.Items.Add(new QuickInfoValue("Pay on delivery", panelOnDelivery));
            */
            WrapPanel panelOrderButtons = new WrapPanel();
            panelOrderButtons.Margin = new Thickness(0, 5, 0, 0);

            this.addObject(panelOrderButtons);

            Button btnOrder = new Button();
            btnOrder.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnOrder.Height = Double.NaN;
            btnOrder.Width = Double.NaN;
            btnOrder.Content = "Order";
            btnOrder.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnOrder.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnOrder.Click += new System.Windows.RoutedEventHandler(btnOrder_Click);
            panelOrderButtons.Children.Add(btnOrder);


            Button btnOrderManufacturer = new Button();
            btnOrderManufacturer.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnOrderManufacturer.Height = Double.NaN;
            btnOrderManufacturer.Width = Double.NaN;
            btnOrderManufacturer.Content = "Order from manufacturer";
            btnOrderManufacturer.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnOrderManufacturer.Margin = new System.Windows.Thickness(5, 0, 0, 0);
            btnOrderManufacturer.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnOrderManufacturer.Click += new RoutedEventHandler(btnOrderManufacturer_Click);

            panelOrderButtons.Children.Add(btnOrderManufacturer);

        }
        private void btnOrderManufacturer_Click(object sender, RoutedEventArgs e)
        {
            object o = PopUpManufacturerOrder.ShowPopUp(this.Airliner.Manufacturer);

            if (o != null)
            {
                Airport airport = (Airport)cbAirport.SelectedItem;
                Dictionary<AirlinerType, int> orders = (Dictionary<AirlinerType, int>)o;

                double price = orders.Keys.Sum(t => t.Price * orders[t]);
            
                if (price > GameObject.GetInstance().HumanAirline.Money)
                {
                    WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2001"), Translator.GetInstance().GetString("MessageBox", "2001", "message"), WPFMessageBoxButtons.Ok);
                }
                else if (airport == null)
                    WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2002"), Translator.GetInstance().GetString("MessageBox", "2002", "message"), WPFMessageBoxButtons.Ok);
                else
                {
                    if (orders.Keys.Count > 0)
                    {
                   
                        AirlineHelpers.OrderAirliners(GameObject.GetInstance().HumanAirline, orders, airport, dpDate.SelectedDate.Value);
                    }
                }
            }

        }


        private void btnOrder_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            object o = PopUpOrderAirliners.ShowPopUp(this.Airliner);

            if (o != null)
            {
                int orders = (int)o;

                double price = orders * this.Airliner.Price;//this.cbPayOnDelivery.IsChecked.Value ? orders * (this.Airliner.Price * this.downPaymentRate) : orders * this.Airliner.Price;
                price = price * ((1 - GeneralHelpers.GetAirlinerOrderDiscount(orders)));

                Airport airport = (Airport)cbAirport.SelectedItem;
                if (price > GameObject.GetInstance().HumanAirline.Money)
                {
                    WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2001"), Translator.GetInstance().GetString("MessageBox", "2001", "message"), WPFMessageBoxButtons.Ok);
                }
                else if (airport == null)
                    WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2002"), Translator.GetInstance().GetString("MessageBox", "2002", "message"), WPFMessageBoxButtons.Ok);
                else
                {

                    WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2003"), string.Format(Translator.GetInstance().GetString("MessageBox", "2003", "message"), this.Airliner.Name, orders), WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                          Dictionary<AirlinerType, int> tOrders = new Dictionary<AirlinerType, int>();
                        tOrders.Add(this.Airliner, orders);

                        AirlineHelpers.OrderAirliners(GameObject.GetInstance().HumanAirline, tOrders, airport, dpDate.SelectedDate.Value);

                        this.clearPanel();
                    }
                }
            }

        }
    }
}
