using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.Model.AirlineModel;
using TheAirline.GraphicsModel.PageModel.PageAirlineModel.PanelAirlineModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.AirportModel;
using TheAirline.GraphicsModel.Converters;
using TheAirline.Model.AirlineModel.SubsidiaryModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;


namespace TheAirline.GraphicsModel.PageModel.PageAirlineModel
{
    /// <summary>
    /// Interaction logic for PageAirline.xaml
    /// </summary>
    public partial class PageAirline : StandardPage
    {
        private Airline Airline;
        private ComboBox cbControlling;
        private Button btnOk;
        private TextBlock txtLicense;
        private Button btnUpgradeLicense;
        public PageAirline(Airline airline)
        {
            InitializeComponent();

            this.Uid = "1000";
            this.Title = Translator.GetInstance().GetString("PageAirline", this.Uid);

            this.Airline = airline;

            StackPanel airportPanel = new StackPanel();
            airportPanel.Margin = new Thickness(10, 0, 10, 0);

            airportPanel.Children.Add(createQuickInfoPanel());

            if (this.Airline.Contract != null)
                airportPanel.Children.Add(createManufacturerContractPanel());

            if (this.Airline.IsHuman)
                airportPanel.Children.Add(createHumanControllingPanel());

            if (GameObject.GetInstance().HumanAirline == GameObject.GetInstance().MainAirline && !this.Airline.IsHuman && !this.Airline.IsSubsidiary)
                airportPanel.Children.Add(createPurchaseAirlinePanel());

            StandardContentPanel panelContent = new StandardContentPanel();

            panelContent.setContentPage(airportPanel, StandardContentPanel.ContentLocation.Left);

            StackPanel panelSideMenu = new PanelAirline(this.Airline, this);

            panelContent.setContentPage(panelSideMenu, StandardContentPanel.ContentLocation.Right);

            base.setContent(panelContent);

            base.setHeaderContent(string.Format("{0} - {1}", this.Title, this.Airline.Profile.Name));

            showPage(this);
        }
        //creates the panel for the manufacturer contract
        private StackPanel createManufacturerContractPanel()
        {
            StackPanel panelContract = new StackPanel();
            panelContract.Margin = new Thickness(5, 10, 10, 10);

            TextBlock txtHeader = new TextBlock();
            txtHeader.Uid = "1014";
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush");
            txtHeader.TextAlignment = TextAlignment.Left;
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PageAirline", txtHeader.Uid);

            panelContract.Children.Add(txtHeader);

            ListBox lbContract = new ListBox();
            lbContract.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbContract.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");
            panelContract.Children.Add(lbContract);

            lbContract.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirline", "1015"), UICreator.CreateTextBlock(this.Airline.Contract.Manufacturer.Name)));
            lbContract.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirline", "1016"), UICreator.CreateTextBlock(this.Airline.Contract.SigningDate.ToShortDateString())));
            lbContract.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirline", "1017"), UICreator.CreateTextBlock(this.Airline.Contract.ExpireDate.ToShortDateString())));
            lbContract.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirline", "1018"), UICreator.CreateTextBlock(string.Format("{0:0.00} %", this.Airline.Contract.Discount))));
            lbContract.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirline", "1019"), UICreator.CreateTextBlock(string.Format("{0} / {1}", this.Airline.Contract.PurchasedAirliners, this.Airline.Contract.Airliners))));

            return panelContract;
        }
        //creates the quick info panel for the airline
        private Panel createQuickInfoPanel()
        {
            StackPanel panelInfo = new StackPanel();
            panelInfo.Margin = new Thickness(5, 0, 10, 0);

            TextBlock txtHeader = new TextBlock();
            txtHeader.Uid = "1001";
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush");
            txtHeader.TextAlignment = TextAlignment.Left;
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PageAirline", txtHeader.Uid);

            panelInfo.Children.Add(txtHeader);

            DockPanel grdQuickInfo = new DockPanel();
            //grdQuickInfo.Margin = new Thickness(0, 5, 0, 0);

            panelInfo.Children.Add(grdQuickInfo);

            Image imgLogo = new Image();
            imgLogo.Source = new BitmapImage(new Uri(this.Airline.Profile.Logo, UriKind.RelativeOrAbsolute));
            imgLogo.Width = 110;
            imgLogo.Margin = new Thickness(0, 0, 5, 0);
            imgLogo.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            RenderOptions.SetBitmapScalingMode(imgLogo, BitmapScalingMode.HighQuality);
            grdQuickInfo.Children.Add(imgLogo);

            StackPanel panelQuickInfo = new StackPanel();

            grdQuickInfo.Children.Add(panelQuickInfo);

            ListBox lbQuickInfo = new ListBox();
            lbQuickInfo.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbQuickInfo.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            panelQuickInfo.Children.Add(lbQuickInfo);

            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirline", "1002"), UICreator.CreateTextBlock(this.Airline.Profile.Name)));

            if (this.Airline.IsSubsidiary)
            {
                ContentControl ccParent = new ContentControl();
                ccParent.SetResourceReference(ContentControl.ContentTemplateProperty, "AirlineLogoLink");
                ccParent.Content = ((SubsidiaryAirline)this.Airline).Airline;

                lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirline", "1021"), ccParent));

            }
            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirline", "1003"), UICreator.CreateTextBlock(this.Airline.Profile.IATACode)));
            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirline", "1004"), UICreator.CreateTextBlock(this.Airline.Profile.CEO)));
            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirline", "1024"), UICreator.CreateTextBlock(this.Airline.Profile.Founded.ToString())));

            ContentControl lblFlag = new ContentControl();
            lblFlag.SetResourceReference(ContentControl.ContentTemplateProperty, "CountryFlagLongItem");
            lblFlag.Content = new CountryCurrentCountryConverter().Convert(this.Airline.Profile.Country);

            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirline", "1005"), lblFlag));
            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirline", "1006"), UICreator.CreateColorRect(this.Airline.Profile.Color)));

            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirline","1027"),UICreator.CreateTextBlock(new TextUnderscoreConverter().Convert(this.Airline.AirlineRouteFocus).ToString())));
            // chs, 2011-10-10 added fleet size to the airline profile
            TextBlock txtFleetSize = UICreator.CreateTextBlock(string.Format("{0} (+{1} in order)", this.Airline.DeliveredFleet.Count, this.Airline.Fleet.Count - this.Airline.DeliveredFleet.Count));

            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirline", "1007"), txtFleetSize));
            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirline", "1011"), UICreator.CreateTextBlock(string.Format("{0} / {1}", this.Airline.Airports.Count, Airports.GetAllAirports().Sum(a => a.getHubs().Count(h => h.Airline == this.Airline))))));
            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirline", "1008"), createAirlineValuePanel()));
            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirline", "1009"), createAirlineReputationPanel()));
            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirline", "1010"), UICreator.CreateTextBlock(String.Format("{0:0.00} %", PassengerHelpers.GetPassengersHappiness(this.Airline)))));
            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirline", "1012"), UICreator.CreateTextBlock(this.Airline.Alliances.Count > 0 ? string.Join(", ", from a in this.Airline.Alliances select a.Name) : Translator.GetInstance().GetString("PageAirline", "1013"))));

            WrapPanel panelLicens = new WrapPanel();

            txtLicense = UICreator.CreateTextBlock(new TextUnderscoreConverter().Convert(this.Airline.License).ToString());
            txtLicense.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

            panelLicens.Children.Add(txtLicense);

            if (this.Airline.IsHuman && this.Airline.License != Airline.AirlineLicense.Long_Haul)
            {

                btnUpgradeLicense = new Button();
                btnUpgradeLicense.Margin = new Thickness(5, 0, 0, 0);
                btnUpgradeLicense.Background = Brushes.Transparent;
                btnUpgradeLicense.Click += btnUpgradeLicense_Click;
                btnUpgradeLicense.ToolTip = UICreator.CreateToolTip("1014");

                Image imgUpgradeLicens = new Image();
                imgUpgradeLicens.Source = new BitmapImage(new Uri(@"/Data/images/add.png", UriKind.RelativeOrAbsolute));
                imgUpgradeLicens.Height = 16;
                RenderOptions.SetBitmapScalingMode(imgUpgradeLicens, BitmapScalingMode.HighQuality);

                btnUpgradeLicense.Content = imgUpgradeLicens;

                panelLicens.Children.Add(btnUpgradeLicense);
            }

            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirline", "1025"), panelLicens));

            return panelInfo;
        }


        //creates the panel for purchasing an airline
        private StackPanel createPurchaseAirlinePanel()
        {
            double buyingPrice = this.Airline.getValue() * 1000000 * 1.10;
            StackPanel purchasePanel = new StackPanel();
            purchasePanel.Margin = new Thickness(5, 10, 10, 10);

            TextBlock txtHeader = new TextBlock();
            txtHeader.Uid = "1022";
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush");
            txtHeader.TextAlignment = TextAlignment.Left;
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = string.Format("{0} ({1:C})", Translator.GetInstance().GetString("PageAirline", txtHeader.Uid), buyingPrice);

            purchasePanel.Children.Add(txtHeader);

            WrapPanel panelButtons = new WrapPanel();

            Button btnPurchase = new Button();
            btnPurchase.Uid = "1023";
            btnPurchase.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnPurchase.Height = Double.NaN;
            btnPurchase.Width = Double.NaN;
            btnPurchase.Content = Translator.GetInstance().GetString("PageAirline", btnPurchase.Uid);
            btnPurchase.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnPurchase.IsEnabled = GameObject.GetInstance().HumanAirline.Money > buyingPrice;
            btnPurchase.Click += new RoutedEventHandler(btnPurchase_Click);
            btnPurchase.ToolTip = UICreator.CreateToolTip("1015");

            panelButtons.Children.Add(btnPurchase);

            Button btnPurchaseAsSubsidiary = new Button();
            btnPurchaseAsSubsidiary.Uid = "1026";
            btnPurchaseAsSubsidiary.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnPurchaseAsSubsidiary.Height = Double.NaN;
            btnPurchaseAsSubsidiary.Width = Double.NaN;
            btnPurchaseAsSubsidiary.Content = Translator.GetInstance().GetString("PageAirline", btnPurchaseAsSubsidiary.Uid);
            btnPurchaseAsSubsidiary.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnPurchaseAsSubsidiary.IsEnabled = GameObject.GetInstance().HumanAirline.Money > buyingPrice;
            btnPurchaseAsSubsidiary.Margin = new Thickness(5, 0, 0, 0);
            btnPurchaseAsSubsidiary.Click += btnPurchaseAsSubsidiary_Click;
            btnPurchaseAsSubsidiary.ToolTip = UICreator.CreateToolTip("1016");

            panelButtons.Children.Add(btnPurchaseAsSubsidiary);

            purchasePanel.Children.Add(panelButtons);

            return purchasePanel;

        }



        //creates the panel for airline value
        private WrapPanel createAirlineValuePanel()
        {
            WrapPanel panelValue = new WrapPanel();
            panelValue.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            for (int i = 0; i <= (int)this.Airline.getAirlineValue(); i++)
            {
                Image imgValue = new Image();
                imgValue.Source = new BitmapImage(new Uri(@"/Data/images/coins.png", UriKind.RelativeOrAbsolute));
                imgValue.Height = 20;
                RenderOptions.SetBitmapScalingMode(imgValue, BitmapScalingMode.HighQuality);

                panelValue.Children.Add(imgValue);
            }
            for (int i = (int)this.Airline.getAirlineValue(); i < (int)Airline.AirlineValue.Very_high; i++)
            {
                Image imgValue = new Image();
                imgValue.Source = new BitmapImage(new Uri(@"/Data/images/coins_gray.png", UriKind.RelativeOrAbsolute));
                imgValue.Height = 20;
                RenderOptions.SetBitmapScalingMode(imgValue, BitmapScalingMode.HighQuality);

                panelValue.Children.Add(imgValue);
            }
            // chs, 2011-13-10 added value in $ of an airline to the value text
            //   TextBlock txtValue = UICreator.CreateTextBlock(string.Format(" ({0})", string.Format("{0:c}", this.Airline.getValue())));
            TextBlock txtValue = UICreator.CreateTextBlock(string.Format(" ({0})", new ValueCurrencyConverter().Convert(this.Airline.getValue() * 1000000).ToString()));

            txtValue.FontStyle = FontStyles.Italic;
            panelValue.Children.Add(txtValue);

            return panelValue;
        }
        //creates the panel for the human controlling airline
        private StackPanel createHumanControllingPanel()
        {
            StackPanel panelMain = new StackPanel();
            panelMain.Margin = new Thickness(5, 5, 10, 0);

            TextBlock txtHeader = new TextBlock();
            txtHeader.Uid = "1020";
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush");
            txtHeader.TextAlignment = TextAlignment.Left;
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PageAirline", txtHeader.Uid);

            panelMain.Children.Add(txtHeader);

            WrapPanel panelChangeControl = new WrapPanel();

            panelMain.Children.Add(panelChangeControl);

            cbControlling = new ComboBox();
            cbControlling.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbControlling.SetResourceReference(ComboBox.ItemTemplateProperty, "AirlineLogoItem");
            cbControlling.Width = 250;
            cbControlling.Items.Add(GameObject.GetInstance().MainAirline);

            foreach (Airline airline in GameObject.GetInstance().MainAirline.Subsidiaries)
                cbControlling.Items.Add(airline);

            cbControlling.SelectedItem = GameObject.GetInstance().HumanAirline;

            panelChangeControl.Children.Add(cbControlling);

            btnOk = new Button();
            btnOk.Uid = "116";
            btnOk.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnOk.Height = Double.NaN;
            btnOk.Width = Double.NaN;
            btnOk.Content = Translator.GetInstance().GetString("General", btnOk.Uid);
            btnOk.IsEnabled = cbControlling.Items.Count > 1;
            btnOk.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnOk.Margin = new Thickness(5, 0, 0, 0);
            btnOk.Click += new RoutedEventHandler(btnOk_Click);

            panelChangeControl.Children.Add(btnOk);

            return panelMain;

        }


        //creates the panel for airline reputation
        public WrapPanel createAirlineReputationPanel()
        {
            WrapPanel panelStars = new WrapPanel();

            for (int i = 0; i <= (int)this.Airline.getReputation(); i++)
            {
                Image imgStar = new Image();
                imgStar.Source = new BitmapImage(new Uri(@"/Data/images/star_gold.png", UriKind.RelativeOrAbsolute));
                imgStar.Height = 20;
                RenderOptions.SetBitmapScalingMode(imgStar, BitmapScalingMode.HighQuality);

                panelStars.Children.Add(imgStar);
            }

            for (int i = (int)this.Airline.getReputation(); i < (int)Airline.AirlineValue.Very_high; i++)
            {
                Image imgStar = new Image();
                imgStar.Source = new BitmapImage(new Uri(@"/Data/images/star_gray.png", UriKind.RelativeOrAbsolute));
                imgStar.Height = 20;
                RenderOptions.SetBitmapScalingMode(imgStar, BitmapScalingMode.HighQuality);

                panelStars.Children.Add(imgStar);
            }

            return panelStars;
        }
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Airline airline = (Airline)cbControlling.SelectedItem;

            if (airline != GameObject.GetInstance().HumanAirline)
            {
                GameObject.GetInstance().HumanAirline = airline;
                PageNavigator.NavigateTo(new PageAirline(GameObject.GetInstance().HumanAirline));
            }
        }
        private void btnPurchaseAsSubsidiary_Click(object sender, RoutedEventArgs e)
        {
            double buyingPrice = this.Airline.getValue() * 1000000 * 1.10;

            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2113"), string.Format(Translator.GetInstance().GetString("MessageBox", "2113", "message"), this.Airline.Profile.Name, buyingPrice), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                List<AirlineLogo> oldLogos = this.Airline.Profile.Logos;
                string oldColor = this.Airline.Profile.Color;

                //creates independent airlines for each subsidiary 
                while (this.Airline.Subsidiaries.Count > 0)
                {
                    SubsidiaryAirline subAirline = this.Airline.Subsidiaries[0];

                    subAirline.Airline = null;

                    this.Airline.removeSubsidiaryAirline(subAirline);
                }
                
                if (this.Airline.License > GameObject.GetInstance().HumanAirline.License)
                    GameObject.GetInstance().HumanAirline.License = this.Airline.License;

                SubsidiaryAirline sAirline = new SubsidiaryAirline(GameObject.GetInstance().HumanAirline, this.Airline.Profile, this.Airline.Mentality, this.Airline.MarketFocus, this.Airline.License,this.Airline.AirlineRouteFocus);
              
                AirlineHelpers.SwitchAirline(this.Airline, sAirline);

                GameObject.GetInstance().HumanAirline.addSubsidiaryAirline(sAirline);

                AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Airline_Expenses, -buyingPrice);

                Airlines.RemoveAirline(this.Airline);
                Airlines.AddAirline(sAirline);

                sAirline.Profile.Logos = oldLogos;
                sAirline.Profile.Color = oldColor;

                foreach (AirlinePolicy policy in this.Airline.Policies)
                    sAirline.addAirlinePolicy(policy);

                sAirline.Money = this.Airline.Money;
                sAirline.StartMoney = this.Airline.Money;

                sAirline.Fees = new AirlineFees();

                PageNavigator.NavigateTo(new PageAirline(GameObject.GetInstance().HumanAirline));


            }

        }
     
        private void btnPurchase_Click(object sender, RoutedEventArgs e)
        {
            double buyingPrice = this.Airline.getValue() * 1000000 * 1.10;

            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2113"), string.Format(Translator.GetInstance().GetString("MessageBox", "2113", "message"), this.Airline.Profile.Name, buyingPrice), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2114"), string.Format(Translator.GetInstance().GetString("MessageBox", "2114", "message"), this.Airline.Profile.Name, buyingPrice), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    while (this.Airline.Subsidiaries.Count > 0)
                    {
                        SubsidiaryAirline subAirline = this.Airline.Subsidiaries[0];
                        subAirline.Profile.CEO = GameObject.GetInstance().HumanAirline.Profile.CEO;

                        subAirline.Airline = GameObject.GetInstance().HumanAirline;
                        this.Airline.removeSubsidiaryAirline(subAirline);
                        GameObject.GetInstance().HumanAirline.addSubsidiaryAirline(subAirline);

                    }
                }
                else
                {
                    while (this.Airline.Subsidiaries.Count > 0)
                    {
                        SubsidiaryAirline subAirline = this.Airline.Subsidiaries[0];

                        subAirline.Airline = null;

                        this.Airline.removeSubsidiaryAirline(subAirline);
                    }
                }
                if (this.Airline.License > GameObject.GetInstance().HumanAirline.License)
                    GameObject.GetInstance().HumanAirline.License = this.Airline.License;

                AirlineHelpers.SwitchAirline(this.Airline, GameObject.GetInstance().HumanAirline);

                AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Airline_Expenses, -buyingPrice);

                Airlines.RemoveAirline(this.Airline);

                PageNavigator.NavigateTo(new PageAirline(GameObject.GetInstance().HumanAirline));
            }
        }
        private void btnUpgradeLicense_Click(object sender, RoutedEventArgs e)
        {
            double upgradeLicensPrice = GeneralHelpers.GetInflationPrice(1000000);

            Airline.AirlineLicense nextLicenseType = this.Airline.License + 1;

            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2119"), string.Format(Translator.GetInstance().GetString("MessageBox", "2119", "message"), new TextUnderscoreConverter().Convert(nextLicenseType), new ValueCurrencyConverter().Convert(upgradeLicensPrice)), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                this.Airline.License = nextLicenseType;

                AirlineHelpers.AddAirlineInvoice(this.Airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Airline_Expenses, -upgradeLicensPrice);

                btnUpgradeLicense.Visibility = this.Airline.License == Model.AirlineModel.Airline.AirlineLicense.Long_Haul ? Visibility.Collapsed : Visibility.Visible;
                txtLicense.Text = new TextUnderscoreConverter().Convert(this.Airline.License).ToString();

            }
        }
      
        public override void updatePage()
        {
            cbControlling.Items.Clear();

            cbControlling.Items.Add(GameObject.GetInstance().MainAirline);

            foreach (Airline airline in GameObject.GetInstance().MainAirline.Subsidiaries)
                cbControlling.Items.Add(airline);

            cbControlling.SelectedItem = GameObject.GetInstance().HumanAirline;

            btnOk.IsEnabled = cbControlling.Items.Count > 1;


        }
    }
}
