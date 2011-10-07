using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using TheAirlineV2.Model.AirlinerModel;
using TheAirlineV2.GraphicsModel.PageModel.GeneralModel;
using System.Windows.Media;
using TheAirlineV2.Model.GeneralModel;
using TheAirlineV2.Model.AirportModel;
using System.Windows;
using TheAirlineV2.GraphicsModel.UserControlModel.MessageBoxModel;
using System.Windows.Media.Imaging;
using TheAirlineV2.GraphicsModel.Converters;

namespace TheAirlineV2.GraphicsModel.PageModel.PageAirlinerModel.PanelAirlinersModel
{
    public class PanelUsedAirliner : PanelAirliner
    {
        private Airliner Airliner;
        private ComboBox cbAirport;// cbName;
        public PanelUsedAirliner(PageAirliners parent, Airliner airliner)
            : base(parent)
        {
            this.Airliner = airliner;

           
            StackPanel panelAirliner = new StackPanel();
            //scroller.Content = panelAirliner;

            panelAirliner.Children.Add(base.createQuickInfoPanel(airliner.Type));

            //this.Children.Add(panelAirliner);

            this.addObject(panelAirliner);

            TextBlock txtHeader = new TextBlock();
            txtHeader.Margin = new System.Windows.Thickness(0, 5, 0, 0);
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = "Airliner Information";

            this.addObject(txtHeader);


            ListBox lbAirlineInfo = new ListBox();
            lbAirlineInfo.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbAirlineInfo.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            this.addObject(lbAirlineInfo);

            lbAirlineInfo.Items.Add(new QuickInfoValue("Built", UICreator.CreateTextBlock(string.Format("{0} ({1} years old)", this.Airliner.BuiltDate.ToShortDateString(), this.Airliner.Age))));

            WrapPanel panelTailNumber = new WrapPanel();

            TextBlock txtTailNumber = UICreator.CreateTextBlock(this.Airliner.TailNumber);
            txtTailNumber.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            panelTailNumber.Children.Add(txtTailNumber);

            ContentControl ccFlag = new ContentControl();
            ccFlag.SetResourceReference(ContentControl.ContentTemplateProperty, "CountryFlagLongItem");
            ccFlag.Content = Countries.GetCountryFromTailNumber(this.Airliner.TailNumber);
            ccFlag.Margin = new Thickness(10, 0, 0, 0);

            panelTailNumber.Children.Add(ccFlag);
            
            lbAirlineInfo.Items.Add(new QuickInfoValue("Tail number", panelTailNumber));
            lbAirlineInfo.Items.Add(new QuickInfoValue("Flown", UICreator.CreateTextBlock( string.Format("{0:0.##} {1}", new NumberToUnitConverter().Convert(this.Airliner.Flown), new StringToLanguageConverter().Convert("km.")))));
            lbAirlineInfo.Items.Add(new QuickInfoValue("Since last service check", UICreator.CreateTextBlock(string.Format("{0:0.##} {1}", new NumberToUnitConverter().Convert(this.Airliner.LastServiceCheck),new StringToLanguageConverter().Convert("km.")))));

            foreach (AirlinerClass aClass in this.Airliner.Classes)
            {
                TextBlock txtClass = UICreator.CreateTextBlock(new TextUnderscoreConverter().Convert(aClass.Type, null, null, null).ToString());
                txtClass.FontWeight = FontWeights.Bold;

                lbAirlineInfo.Items.Add(new QuickInfoValue("Class", txtClass));

                foreach (AirlinerFacility.FacilityType type in Enum.GetValues(typeof(AirlinerFacility.FacilityType)))
                {
                    AirlinerFacility facility = aClass.getFacility(type);//this.Airliner.getFacility(type);
                    lbAirlineInfo.Items.Add(new QuickInfoValue(string.Format("{0} facilities", type), UICreator.CreateTextBlock(facility.Name)));
                }
            }

            TextBlock txtPriceHeader = new TextBlock();
            txtPriceHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtPriceHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtPriceHeader.Margin = new System.Windows.Thickness(0, 5, 0, 0);
            txtPriceHeader.FontWeight = FontWeights.Bold;
            txtPriceHeader.Text = "Price Information";

            this.addObject(txtPriceHeader);


            ListBox lbPriceInfo = new ListBox();
            lbPriceInfo.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbPriceInfo.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            this.addObject(lbPriceInfo);

            lbPriceInfo.Items.Add(new QuickInfoValue("Price", UICreator.CreateTextBlock(string.Format("{0:c}", this.Airliner.Price))));
            lbPriceInfo.Items.Add(new QuickInfoValue("Leasing price", UICreator.CreateTextBlock(string.Format("{0:c}", this.Airliner.LeasingPrice))));
            lbPriceInfo.Items.Add(new QuickInfoValue("Yearly maintenance", UICreator.CreateTextBlock(string.Format("{0:c}", this.Airliner.Type.getMaintenance()))));


            cbAirport = new ComboBox();
            cbAirport.SetResourceReference(ComboBox.ItemTemplateProperty, "AirportCountryItem");
            //cbAirport.SetResourceReference(ComboBox.ItemTemplateProperty, "CountryFlagLongItem");
            cbAirport.Background = Brushes.Transparent;
            cbAirport.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbAirport.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            List<Airport> airports = GameObject.GetInstance().HumanAirline.Airports.FindAll((delegate(Airport airport) { return airport.getAirportFacility(GameObject.GetInstance().HumanAirline, AirportFacility.FacilityType.Service).TypeLevel > 0; }));
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

            WrapPanel panelButtons = new WrapPanel();
            panelButtons.Margin = new Thickness(0, 5, 0, 0);
            this.addObject(panelButtons);

            Button btnBuy = new Button();
            btnBuy.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnBuy.Height = 20;
            btnBuy.Width = 80;
            btnBuy.Content = "Buy";
            btnBuy.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnBuy.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnBuy.Click += new System.Windows.RoutedEventHandler(btnBuy_Click);
            //btnRent.IsEnabled = this.Airport.Gates.getFreeGates() > 0;
            //btnRent.Click += new RoutedEventHandler(btnRent_Click);
            panelButtons.Children.Add(btnBuy);

            Button btnLease = new Button();
            btnLease.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnLease.Height = 20;
            btnLease.Width = 80;
            btnLease.Content = "Lease";
            btnLease.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnLease.Margin = new Thickness(5, 0, 0, 0);
            btnLease.Click += new RoutedEventHandler(btnLease_Click);
            panelButtons.Children.Add(btnLease);

        }

        private void btnLease_Click(object sender, RoutedEventArgs e)
        {
            Airport airport = (Airport)cbAirport.SelectedItem;

            if (this.Airliner.getLeasingPrice()*2 > GameObject.GetInstance().HumanAirline.Money)
                WPFMessageBox.Show("Not enough money", "You don't have any money to lease this airliner", WPFMessageBoxButtons.Ok);
            else if (airport == null)
                WPFMessageBox.Show("No homebase", "You haven't selected a home base for this airliner", WPFMessageBoxButtons.Ok);

            else
            {
                WPFMessageBoxResult result = WPFMessageBox.Show("Lease airliner", string.Format("Are you sure you want to lease this {0}?", this.Airliner.Type.Name), WPFMessageBoxButtons.YesNo);
                
                if (result == WPFMessageBoxResult.Yes)
                {
                    //string name = (string)cbName.SelectedItem;
                    //AirlinerNameGenerator.GetInstance().removeName(name);
                    if (Countries.GetCountryFromTailNumber(this.Airliner.TailNumber).Name != GameObject.GetInstance().HumanAirline.Profile.Country.Name)
                       this.Airliner.TailNumber = GameObject.GetInstance().HumanAirline.Profile.Country.TailNumbers.getNextTailNumber();

                    GameObject.GetInstance().HumanAirline.addAirliner(FleetAirliner.PurchasedType.Leased, this.Airliner,this.Airliner.TailNumber, airport);

                    GameObject.GetInstance().HumanAirline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Rents, -this.Airliner.LeasingPrice*2));

                    base.ParentPage.showUsedAirliners();

                    this.clearPanel();
                }
            }
        }
       
        private void btnBuy_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Airport airport = (Airport)cbAirport.SelectedItem;
        
            if (this.Airliner.getPrice() > GameObject.GetInstance().HumanAirline.Money)
                WPFMessageBox.Show("Not enough money", "You don't have any money to buy this airliner", WPFMessageBoxButtons.Ok);
            else if (airport == null)
                WPFMessageBox.Show("No homebase", "You haven't selected a home base for this airliner", WPFMessageBoxButtons.Ok);
       
            else
            {
                WPFMessageBoxResult result = WPFMessageBox.Show("Buy airliner", string.Format("Are you sure you want to buy this {0}?", this.Airliner.Type.Name), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    if (Countries.GetCountryFromTailNumber(this.Airliner.TailNumber).Name != GameObject.GetInstance().HumanAirline.Profile.Country.Name)
                        this.Airliner.TailNumber = GameObject.GetInstance().HumanAirline.Profile.Country.TailNumbers.getNextTailNumber();

                    GameObject.GetInstance().HumanAirline.addAirliner(FleetAirliner.PurchasedType.Bought, this.Airliner, this.Airliner.TailNumber, airport);

                    //GameObject.GetInstance().HumanAirline.Money -= this.Airliner.getPrice();
                    GameObject.GetInstance().HumanAirline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -this.Airliner.getPrice()));

                    base.ParentPage.showUsedAirliners();

                    this.clearPanel();
                }
            }
        }
    }
}
