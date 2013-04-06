using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TheAirline.Model.AirportModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GraphicsModel.Converters;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;

namespace TheAirline.GraphicsModel.PageModel.PageAirlinerModel.PanelAirlinersModel
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

            panelAirliner.Children.Add(PanelAirliner.createQuickInfoPanel(airliner.Type));

            this.addObject(panelAirliner);

            TextBlock txtHeader = new TextBlock();
            txtHeader.Uid = "1001";
            txtHeader.Margin = new System.Windows.Thickness(0, 5, 0, 0);
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PanelUsedAirliner", txtHeader.Uid);

            this.addObject(txtHeader);


            ListBox lbAirlineInfo = new ListBox();
            lbAirlineInfo.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbAirlineInfo.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            this.addObject(lbAirlineInfo);

            lbAirlineInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelUsedAirliner", "1002"), UICreator.CreateTextBlock(string.Format("{0} ({1} years old)", this.Airliner.BuiltDate.ToShortDateString(), this.Airliner.Age))));

            WrapPanel panelTailNumber = new WrapPanel();

            TextBlock txtTailNumber = UICreator.CreateTextBlock(this.Airliner.TailNumber);
            txtTailNumber.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            panelTailNumber.Children.Add(txtTailNumber);

            ContentControl ccFlag = new ContentControl();
            ccFlag.SetResourceReference(ContentControl.ContentTemplateProperty, "CountryFlagLongItem");
            ccFlag.Content = new CountryCurrentCountryConverter().Convert(Countries.GetCountryFromTailNumber(this.Airliner.TailNumber));
            ccFlag.Margin = new Thickness(10, 0, 0, 0);

            panelTailNumber.Children.Add(ccFlag);

            lbAirlineInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelUsedAirliner", "1003"), panelTailNumber));
            lbAirlineInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelUsedAirliner", "1004"), UICreator.CreateTextBlock(string.Format("{0:0.##} {1}", new NumberToUnitConverter().Convert(this.Airliner.Flown), new StringToLanguageConverter().Convert("km.")))));
            lbAirlineInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelUsedAirliner", "1005"), UICreator.CreateTextBlock(string.Format("{0:0.##} {1}", new NumberToUnitConverter().Convert(this.Airliner.LastServiceCheck),new StringToLanguageConverter().Convert("km.")))));

            foreach (AirlinerClass aClass in this.Airliner.Classes)
            {
                TextBlock txtClass = UICreator.CreateTextBlock(new TextUnderscoreConverter().Convert(aClass.Type, null, null, null).ToString());
                txtClass.FontWeight = FontWeights.Bold;

                lbAirlineInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelUsedAirliner", "1006"), txtClass));

                foreach (AirlinerFacility.FacilityType type in Enum.GetValues(typeof(AirlinerFacility.FacilityType)))
                {
                    AirlinerFacility facility = aClass.getFacility(type);
                    lbAirlineInfo.Items.Add(new QuickInfoValue(string.Format("{0} facilities", type), UICreator.CreateTextBlock(facility.Name)));
                }
            }

            TextBlock txtPriceHeader = new TextBlock();
            txtPriceHeader.Uid = "1101";
            txtPriceHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtPriceHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtPriceHeader.Margin = new System.Windows.Thickness(0, 5, 0, 0);
            txtPriceHeader.FontWeight = FontWeights.Bold;
            txtPriceHeader.Text = Translator.GetInstance().GetString("PanelUsedAirliner", txtPriceHeader.Uid);

            this.addObject(txtPriceHeader);


            ListBox lbPriceInfo = new ListBox();
            lbPriceInfo.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbPriceInfo.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            this.addObject(lbPriceInfo);

            /*
            lbPriceInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelUsedAirliner", "1102"), UICreator.CreateTextBlock(string.Format("{0:c}", this.Airliner.Price))));
            lbPriceInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelUsedAirliner", "1103"), UICreator.CreateTextBlock(string.Format("{0:c}", this.Airliner.LeasingPrice))));
            lbPriceInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelUsedAirliner", "1104"), UICreator.CreateTextBlock(string.Format("{0:c}", this.Airliner.Type.getMaintenance()))));
            */

            lbPriceInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelUsedAirliner", "1102"), UICreator.CreateTextBlock(new ValueCurrencyConverter().Convert(Airliner.Price).ToString())));
            lbPriceInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelUsedAirliner", "1103"), UICreator.CreateTextBlock(new ValueCurrencyConverter().Convert(this.Airliner.LeasingPrice).ToString())));
            lbPriceInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelUsedAirliner", "1104"), UICreator.CreateTextBlock(new ValueCurrencyConverter().Convert(this.Airliner.Type.getMaintenance()).ToString())));


            cbAirport = new ComboBox();
            cbAirport.SetResourceReference(ComboBox.ItemTemplateProperty, "AirportCountryItem");
            cbAirport.Background = Brushes.Transparent;
            cbAirport.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbAirport.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            List<Airport> airports = GameObject.GetInstance().HumanAirline.Airports.FindAll(a=>a.getCurrentAirportFacility(GameObject.GetInstance().HumanAirline, AirportFacility.FacilityType.Service).TypeLevel > 0 && a.getMaxRunwayLength() >= this.Airliner.Type.MinRunwaylength);
            airports = (from a in airports orderby a.Profile.Name select a).ToList();
       
            foreach (Airport airport in airports)
                cbAirport.Items.Add(airport);

            cbAirport.SelectedIndex = 0;

            lbPriceInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelUsedAirliner", "1105"), cbAirport));

            WrapPanel panelButtons = new WrapPanel();
            panelButtons.Margin = new Thickness(0, 5, 0, 0);
            this.addObject(panelButtons);

            Button btnBuy = new Button();
            btnBuy.Uid = "200";
            btnBuy.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnBuy.Height = Double.NaN;
            btnBuy.Width = Double.NaN;
            btnBuy.Content = Translator.GetInstance().GetString("PanelUsedAirliner", btnBuy.Uid);
            btnBuy.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnBuy.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnBuy.Click += new System.Windows.RoutedEventHandler(btnBuy_Click);
            panelButtons.Children.Add(btnBuy);

            Button btnLease = new Button();
            btnLease.Uid = "201";
            btnLease.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnLease.Height = Double.NaN;
            btnLease.Width = Double.NaN;
            btnLease.Content = Translator.GetInstance().GetString("PanelUsedAirliner", btnLease.Uid);
            btnLease.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnLease.Margin = new Thickness(5, 0, 0, 0);
            btnLease.Click += new RoutedEventHandler(btnLease_Click);
            panelButtons.Children.Add(btnLease);

        }

        private void btnLease_Click(object sender, RoutedEventArgs e)
        {
            Boolean contractedOrder = false;
            Boolean tryOrder = true;
            if (GameObject.GetInstance().HumanAirline.Contract != null)
            {
                if (GameObject.GetInstance().HumanAirline.Contract.Manufacturer == this.Airliner.Type.Manufacturer)
                {
                    contractedOrder = true;
          
                }
                else
                {
                    double terminationFee = GameObject.GetInstance().HumanAirline.Contract.getTerminationFee();
                    WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2010"), string.Format(Translator.GetInstance().GetString("MessageBox", "2010", "message"), GameObject.GetInstance().HumanAirline.Contract.Manufacturer.Name, terminationFee), WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                        AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -terminationFee);

                        GameObject.GetInstance().HumanAirline.Contract = null;
                    }
                    tryOrder = result == WPFMessageBoxResult.Yes;
                }
            }
            
            Airport airport = (Airport)cbAirport.SelectedItem;

            if (this.Airliner.getLeasingPrice()*2 > GameObject.GetInstance().HumanAirline.Money)
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2004"), Translator.GetInstance().GetString("MessageBox", "2004", "message"), WPFMessageBoxButtons.Ok);
            else if (airport == null)
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2002"), Translator.GetInstance().GetString("MessageBox", "2002", "message"), WPFMessageBoxButtons.Ok);

            else
            {
                if (tryOrder)
                {
                    WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2005"), string.Format(Translator.GetInstance().GetString("MessageBox", "2005", "message"), this.Airliner.Type.Name), WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                        if (Countries.GetCountryFromTailNumber(this.Airliner.TailNumber).Name != GameObject.GetInstance().HumanAirline.Profile.Country.Name)
                            this.Airliner.TailNumber = GameObject.GetInstance().HumanAirline.Profile.Country.TailNumbers.getNextTailNumber();

                        GameObject.GetInstance().HumanAirline.addAirliner(FleetAirliner.PurchasedType.Leased, this.Airliner, airport);

                        AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Rents, -this.Airliner.LeasingPrice * 2);

                        if (contractedOrder)
                            GameObject.GetInstance().HumanAirline.Contract.PurchasedAirliners++;

                        base.ParentPage.removeUsedAirliner(this.Airliner);

                        this.clearPanel();

                        
                    }
                }
            }
        }
       
        private void btnBuy_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Boolean contractedOrder = false;
            Boolean tryOrder = true;
            if (GameObject.GetInstance().HumanAirline.Contract != null)
            {
                if (GameObject.GetInstance().HumanAirline.Contract.Manufacturer == this.Airliner.Type.Manufacturer)
                    contractedOrder = true;
                else
                {
                    double terminationFee = GameObject.GetInstance().HumanAirline.Contract.getTerminationFee();
                    WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2010"), string.Format(Translator.GetInstance().GetString("MessageBox", "2010", "message"), GameObject.GetInstance().HumanAirline.Contract.Manufacturer.Name, terminationFee), WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                        AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -terminationFee);
                        GameObject.GetInstance().HumanAirline.Contract = null;
           

                    }
                    tryOrder = result == WPFMessageBoxResult.Yes;
                }
            }
            
            Airport airport = (Airport)cbAirport.SelectedItem;
        
            if (this.Airliner.getPrice() > GameObject.GetInstance().HumanAirline.Money)
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2006"), Translator.GetInstance().GetString("MessageBox", "2006", "message"), WPFMessageBoxButtons.Ok);
            else if (airport == null)
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2002"), Translator.GetInstance().GetString("MessageBox", "2002", "message"), WPFMessageBoxButtons.Ok);

            else
            {
                if (tryOrder)
                {
                    WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2007"), string.Format(Translator.GetInstance().GetString("MessageBox", "2007", "message"), this.Airliner.Type.Name), WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                        if (contractedOrder)
                            AirlineHelpers.BuyAirliner(GameObject.GetInstance().HumanAirline, this.Airliner, airport, GameObject.GetInstance().HumanAirline.Contract.Discount);
                        else
                            AirlineHelpers.BuyAirliner(GameObject.GetInstance().HumanAirline, this.Airliner, airport);


                        base.ParentPage.removeUsedAirliner(this.Airliner);

                        if (contractedOrder)
                            GameObject.GetInstance().HumanAirline.Contract.PurchasedAirliners++;

                        this.clearPanel();
                    }
                }
            }
        }
    }
}
