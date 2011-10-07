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

namespace TheAirline.GraphicsModel.PageModel.PageAirlinerModel.PanelAirlinersModel
{
    public class PanelNewAirliner : PanelAirliner
    {
        private AirlinerType Airliner;
        private ComboBox cbAirport;// cbName;
        private DatePicker dpDate;
        private CheckBox cbPayOnDelivery;
        private double downPaymentRate = 0.03;
        public PanelNewAirliner(PageAirliners parent, AirlinerType airliner)
            : base(parent)
        {
     

            this.Airliner = airliner;

            StackPanel panelAirliner = new StackPanel();
            
            panelAirliner.Children.Add(base.createQuickInfoPanel(airliner));

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
            //cbAirport.SetResourceReference(ComboBox.ItemTemplateProperty, "CountryFlagLongItem");
            cbAirport.Background = Brushes.Transparent;
            cbAirport.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbAirport.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            List<Airport> airports = GameObject.GetInstance().HumanAirline.Airports.FindAll((delegate(Airport airport) { return airport.getAirportFacility(GameObject.GetInstance().HumanAirline, AirportFacility.FacilityType.Service).TypeLevel > 0; }));
            airports.Sort(delegate(Airport a1, Airport a2) { return a1.Profile.Name.CompareTo(a2.Profile.Name); });

            airports.Sort(delegate(Airport a1, Airport a2) { return a1.Profile.Name.CompareTo(a2.Profile.Name); });

            foreach (Airport airport in airports)
                cbAirport.Items.Add(airport);

            cbAirport.SelectedIndex = 0;

            lbPriceInfo.Items.Add(new QuickInfoValue("Select home base", cbAirport));

            /*
            cbName = new ComboBox();
            cbName.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbName.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbName.Background = Brushes.Transparent;
            cbName.Width = 200;

            foreach (string name in AirlinerNameGenerator.GetInstance().getNames())
                cbName.Items.Add(name);

            cbName.SelectedIndex = 0;

            lbPriceInfo.Items.Add(new QuickInfoValue("Select airliner name", cbName));
             * */

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

            Button btnOrder = new Button();
            btnOrder.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnOrder.Height = 20;
            btnOrder.Width = 80;
            btnOrder.Content = "Order";
            btnOrder.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnOrder.Margin = new System.Windows.Thickness(0, 5, 0, 0);
            btnOrder.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnOrder.Click += new System.Windows.RoutedEventHandler(btnOrder_Click);
            //btnRent.IsEnabled = this.Airport.Gates.getFreeGates() > 0;
            //btnRent.Click += new RoutedEventHandler(btnRent_Click);
            this.addObject(btnOrder);






        }

     

        private void btnOrder_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            double price = this.cbPayOnDelivery.IsChecked.Value ? this.Airliner.Price * this.downPaymentRate : this.Airliner.Price;
            Airport airport = (Airport)cbAirport.SelectedItem;
            if (price > GameObject.GetInstance().HumanAirline.Money)
            {
                WPFMessageBox.Show("Not enough money", "You don't have any money to order this airliner", WPFMessageBoxButtons.Ok);
            }
            else if (airport == null)
                WPFMessageBox.Show("No homebase", "You haven't selected a home base for this airliner", WPFMessageBoxButtons.Ok);
            else
            {
                WPFMessageBoxResult result = WPFMessageBox.Show("Order airliner", string.Format("Are you sure you want to order a {0}?", this.Airliner.Name), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {

                    //string name = (string)cbName.SelectedItem;

                    //AirlinerNameGenerator.GetInstance().removeName(name);

                    Airliner airliner = new Airliner(this.Airliner, GameObject.GetInstance().HumanAirline.Profile.Country.TailNumbers.getNextTailNumber(), dpDate.SelectedDate.Value);// GameObject.GetInstance().GameTime.AddMonths(3));
                    Airliners.AddAirliner(airliner);

                    FleetAirliner.PurchasedType type = cbPayOnDelivery.IsChecked.Value ? FleetAirliner.PurchasedType.BoughtDownPayment : FleetAirliner.PurchasedType.Bought;
                    GameObject.GetInstance().HumanAirline.addAirliner(type, airliner, airliner.TailNumber, airport);

                    //                GameObject.GetInstance().HumanAirline.Money -= this.Airliner.Price;
                    //cbPayOnDelivery 
                    GameObject.GetInstance().HumanAirline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -price));

                    //Sort routes + airliners

                    this.clearPanel();
                }
            }


        }
    }
}
