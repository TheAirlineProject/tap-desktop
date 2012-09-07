using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.Converters;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.PageModel.PageAirlineModel;
using TheAirline.GraphicsModel.PageModel.PageAirportModel;
using TheAirline.GraphicsModel.PageModel.PageFleetAirlinerModel.PanelFleetAirlinerModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GraphicsModel.PageModel.PageFleetAirlinerModel
{
    /// <summary>
    /// Interaction logic for PageFleetAirliner.xaml
    /// </summary>
    public partial class PageFleetAirliner : StandardPage
    {
        private FleetAirliner Airliner;
        private TextBlock txtFlown, txtPosition, txtSinceService, txtName;
        private ContentControl lblAirport;
        private StackPanel panelLeasedAirliner;
        public PageFleetAirliner(FleetAirliner airliner)
        {
            InitializeComponent();

            this.Uid = "1000";
            this.Title = Translator.GetInstance().GetString("PageFleetAirliner", this.Uid);

            this.Airliner = airliner;

            StackPanel airlinerPanel = new StackPanel();
            airlinerPanel.Margin = new Thickness(10, 0, 10, 0);

            airlinerPanel.Children.Add(createQuickInfoPanel());
            airlinerPanel.Children.Add(createAirlinerTypePanel());
            if (this.Airliner.Purchased == FleetAirliner.PurchasedType.Leased && this.Airliner.Airliner.Airline.IsHuman)
                airlinerPanel.Children.Add(createLeasedAirlinerPanel());

            StandardContentPanel panelContent = new StandardContentPanel();

            panelContent.setContentPage(airlinerPanel, StandardContentPanel.ContentLocation.Left);


            StackPanel panelSideMenu = new PanelFleetAirliner(this.Airliner);

            panelContent.setContentPage(panelSideMenu, StandardContentPanel.ContentLocation.Right);



            base.setContent(panelContent);

            base.setHeaderContent(this.Title + " - " + this.Airliner.Name);


            showPage(this);

            GameTimer.GetInstance().OnTimeChanged += new GameTimer.TimeChanged(PageFleetAirliner_OnTimeChanged);

            this.Unloaded += new RoutedEventHandler(PageFleetAirliner_Unloaded);

        }

        private void PageFleetAirliner_Unloaded(object sender, RoutedEventArgs e)
        {
            GameTimer.GetInstance().OnTimeChanged -= new GameTimer.TimeChanged(PageFleetAirliner_OnTimeChanged);

        }

        private void PageFleetAirliner_OnTimeChanged()
        {
            if (this.IsLoaded)
            {

                Airport airport = !this.Airliner.HasRoute ? null : Airports.GetAirport(this.Airliner.CurrentPosition);
                txtFlown.Text = string.Format("{0:0.##} {1}", new NumberToUnitConverter().Convert(this.Airliner.Airliner.Flown), new StringToLanguageConverter().Convert("km."));
                Run run = (Run)((Hyperlink)txtPosition.Inlines.FirstInline).Inlines.FirstInline;
                run.Text = this.Airliner.HasRoute ? (airport == null ? this.Airliner.CurrentPosition.ToString() : airport.Profile.Name) : this.Airliner.Homebase.Profile.Name;

                txtSinceService.Text = string.Format("{0:0.##} {1}", new NumberToUnitConverter().Convert(this.Airliner.Airliner.LastServiceCheck), new StringToLanguageConverter().Convert("km."));
            }

        }
        //creates the panel for leased airliner
        private Panel createLeasedAirlinerPanel()
        {
            panelLeasedAirliner = new StackPanel();
            panelLeasedAirliner.Margin = new Thickness(0, 10, 0, 0);

            TextBlock txtHeader = new TextBlock();
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush");
            txtHeader.TextAlignment = TextAlignment.Left;
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PageFleetAirliner", "1001");

            panelLeasedAirliner.Children.Add(txtHeader);

            ListBox lbQuickInfo = new ListBox();
            lbQuickInfo.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbQuickInfo.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageFleetAirliner", "1025"), UICreator.CreateTextBlock(this.Airliner.PurchasedDate.ToShortDateString())));
            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageFleetAirliner", "1026"), UICreator.CreateTextBlock(string.Format("{0:C}", this.Airliner.Airliner.getPrice()))));

            panelLeasedAirliner.Children.Add(lbQuickInfo);

            Button btnBuy = new Button();
            btnBuy.Uid = "200";
            btnBuy.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnBuy.Height = Double.NaN;
            btnBuy.Width = Double.NaN;
            btnBuy.Margin = new Thickness(0, 5, 0, 0);
            btnBuy.Content = Translator.GetInstance().GetString("PageFleetAirliner", btnBuy.Uid);
            btnBuy.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnBuy.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnBuy.Click += new System.Windows.RoutedEventHandler(btnBuy_Click);
            panelLeasedAirliner.Children.Add(btnBuy);

            return panelLeasedAirliner;

        }
        //creates the info panel for the airliner type
        private Panel createAirlinerTypePanel()
        {
            AirlinerType airliner = this.Airliner.Airliner.Type;

            StackPanel panelAirlinerType = new StackPanel();
            panelAirlinerType.Margin = new Thickness(0, 10, 0, 0);

            TextBlock txtHeader = new TextBlock();
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush");
            txtHeader.TextAlignment = TextAlignment.Left;
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PageFleetAirliner", "1001");

            panelAirlinerType.Children.Add(txtHeader);

            ListBox lbQuickInfo = new ListBox();
            lbQuickInfo.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbQuickInfo.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            panelAirlinerType.Children.Add(lbQuickInfo);

            WrapPanel panelAirlinerName = new WrapPanel();

            Image imgAirlinerImage = new Image();
            imgAirlinerImage.Source = new BitmapImage(new Uri(@"/Data/images/info.png", UriKind.RelativeOrAbsolute));
            imgAirlinerImage.Height = 16;
            imgAirlinerImage.Tag = airliner;
            imgAirlinerImage.Visibility = airliner.Image == null ? Visibility.Collapsed : Visibility.Visible;
            imgAirlinerImage.Margin = new Thickness(5, 0, 0, 0);
            imgAirlinerImage.MouseDown += new System.Windows.Input.MouseButtonEventHandler(imgAirlinerImage_MouseDown);

            RenderOptions.SetBitmapScalingMode(imgAirlinerImage, BitmapScalingMode.HighQuality);

            panelAirlinerName.Children.Add(UICreator.CreateTextBlock(airliner.Name));
            panelAirlinerName.Children.Add(imgAirlinerImage);
            
            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageFleetAirliner", "1002"), panelAirlinerName));

            ContentControl ccManufactorer = new ContentControl();
            ccManufactorer.SetResourceReference(ContentControl.ContentTemplateProperty, "ManufactorerCountryItem");
            ccManufactorer.Content = airliner.Manufacturer;

            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageFleetAirliner", "1003"), ccManufactorer));
            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageFleetAirliner", "1004"), UICreator.CreateTextBlock(new TextUnderscoreConverter().Convert(airliner.Body, null, null, null).ToString())));

            string range = string.Format("{0:0.##} {1}", new NumberToUnitConverter().Convert(airliner.Range), new StringToLanguageConverter().Convert("km."));
            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageFleetAirliner", "1005"), UICreator.CreateTextBlock(string.Format("{1} ({0})", new TextUnderscoreConverter().Convert(airliner.RangeType), range))));

            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageFleetAirliner", "1006"), UICreator.CreateTextBlock(new TextUnderscoreConverter().Convert(airliner.Engine, null, null, null).ToString())));
            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageFleetAirliner", "1007"), UICreator.CreateTextBlock(new NumberMeterToUnitConverter().Convert(airliner.Wingspan).ToString())));
            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageFleetAirliner", "1008"), UICreator.CreateTextBlock(new NumberMeterToUnitConverter().Convert(airliner.Length).ToString())));
            if (airliner.TypeAirliner == AirlinerType.TypeOfAirliner.Passenger)
            {
                lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageFleetAirliner", "1009"), UICreator.CreateTextBlock(((AirlinerPassengerType)airliner).MaxSeatingCapacity.ToString())));//SeatingCapacity.ToString())));
                lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageFleetAirliner", "1010"), UICreator.CreateTextBlock(((AirlinerPassengerType)airliner).MaxAirlinerClasses.ToString())));
            }

            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageFleetAirliner", "1011"), UICreator.CreateTextBlock(new NumberMeterToUnitConverter().Convert(airliner.MinRunwaylength).ToString())));

            if (airliner.TypeAirliner == AirlinerType.TypeOfAirliner.Passenger)
            {
                string crewRequirements = string.Format(Translator.GetInstance().GetString("PageFleetAirliner", "1012"), airliner.CockpitCrew, ((AirlinerPassengerType)airliner).CabinCrew);
                lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageFleetAirliner", "1013"), UICreator.CreateTextBlock(crewRequirements)));
            }
            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageFleetAirliner", "1014"), UICreator.CreateTextBlock(string.Format("{0:0.##} {1}", new NumberToUnitConverter().Convert(airliner.CruisingSpeed), new StringToLanguageConverter().Convert("km/t")))));
            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageFleetAirliner", "1015"), UICreator.CreateTextBlock(string.Format("{0:0.###} {1}", new FuelConsumptionToUnitConverter().Convert(airliner.FuelConsumption), new StringToLanguageConverter().Convert("l/seat/km")))));
            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageFleetAirliner", "1027"), UICreator.CreateTextBlock(string.Format("{0:0.#} {1}", new FuelUnitGtLConverter().Convert(airliner.FuelCapacity), new StringToLanguageConverter().Convert("gallons")))));
            return panelAirlinerType;

        }
        //creates the quick info panel for the fleet airliner
        private Panel createQuickInfoPanel()
        {
            Image imgEditName = new Image();
            imgEditName.Source = new BitmapImage(new Uri(@"/Data/images/edit.png", UriKind.RelativeOrAbsolute));
            imgEditName.Width = 16;
           
            RenderOptions.SetBitmapScalingMode(imgEditName, BitmapScalingMode.HighQuality);


            StackPanel panelInfo = new StackPanel();

            TextBlock txtHeader = new TextBlock();
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush");
            txtHeader.TextAlignment = TextAlignment.Left;
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PageFleetAirliner", "1016");

            panelInfo.Children.Add(txtHeader);

            DockPanel grdQuickInfo = new DockPanel();
            grdQuickInfo.Margin = new Thickness(0, 5, 0, 0);

            panelInfo.Children.Add(grdQuickInfo);


            Image imgLogo = new Image();
            imgLogo.Source = new BitmapImage(new Uri(this.Airliner.Airliner.Airline.Profile.Logo, UriKind.RelativeOrAbsolute));
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

            DockPanel panelName = new DockPanel();

            txtName = UICreator.CreateTextBlock(this.Airliner.Name);
            txtName.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            panelName.Children.Add(txtName);

            Button btnEditName = new Button();
            btnEditName.Background = Brushes.Transparent;
            btnEditName.Margin = new Thickness(5, 0, 0, 0);
            btnEditName.Visibility = this.Airliner.Airliner.Airline.IsHuman ? Visibility.Visible : System.Windows.Visibility.Collapsed;
            btnEditName.Click += new RoutedEventHandler(btnEditName_Click);
            btnEditName.Content = imgEditName;

            panelName.Children.Add(btnEditName);

            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageFleetAirliner", "1002"), panelName));

            TextBlock lnkOwner = UICreator.CreateLink(this.Airliner.Airliner.Airline.Profile.Name);
            ((Hyperlink)lnkOwner.Inlines.FirstInline).Click += new RoutedEventHandler(PageFleetAirliner_Click);
            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageFleetAirliner", "1017"), lnkOwner));

            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageFleetAirliner", "1025"), UICreator.CreateTextBlock(this.Airliner.PurchasedDate.ToShortDateString())));

            DockPanel panelHomeBase = new DockPanel();

            lblAirport = new ContentControl();
            lblAirport.MouseDown += new MouseButtonEventHandler(lblAirport_MouseDown);
            lblAirport.SetResourceReference(ContentControl.ContentTemplateProperty, "AirportCountryItem");
            lblAirport.Content = this.Airliner.Homebase;

            panelHomeBase.Children.Add(lblAirport);

            Button btnEditHomeBase = new Button();
            btnEditHomeBase.Background = Brushes.Transparent;
            btnEditHomeBase.Click += new RoutedEventHandler(btnEditHomeBase_Click);

            Image imgEdit = new Image();
            imgEdit.Source = new BitmapImage(new Uri(@"/Data/images/edit.png", UriKind.RelativeOrAbsolute));
            imgEdit.Width = 16;
            RenderOptions.SetBitmapScalingMode(imgEdit, BitmapScalingMode.HighQuality);


            btnEditHomeBase.Visibility = this.Airliner.Airliner.Airline.IsHuman ? Visibility.Visible : System.Windows.Visibility.Collapsed;

            btnEditHomeBase.Content = imgEdit;

            panelHomeBase.Children.Add(btnEditHomeBase);






            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageFleetAirliner", "1018"), panelHomeBase));

            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageFleetAirliner", "1019"), UICreator.CreateTextBlock(string.Format(Translator.GetInstance().GetString("PageFleetAirliner", "1020"), this.Airliner.Airliner.BuiltDate.ToShortDateString(), this.Airliner.Airliner.Age))));
            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageFleetAirliner", "1021"), UICreator.CreateTextBlock(this.Airliner.Airliner.TailNumber)));

            txtFlown = UICreator.CreateTextBlock(string.Format("{0:0.##} {1}", new NumberToUnitConverter().Convert(this.Airliner.Airliner.Flown), new StringToLanguageConverter().Convert("km.")));
            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageFleetAirliner", "1022"), txtFlown));

            txtSinceService = UICreator.CreateTextBlock(string.Format("{0:0.##} {1}", new NumberToUnitConverter().Convert(this.Airliner.Airliner.LastServiceCheck), new StringToLanguageConverter().Convert("km.")));
            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageFleetAirliner", "1023"), txtSinceService));


            WrapPanel panelCoordinates = new WrapPanel();


            Image imgMap = new Image();
            imgMap.Source = new BitmapImage(new Uri(@"/Data/images/map.png", UriKind.RelativeOrAbsolute));
            imgMap.Height = 16;
            imgMap.MouseDown += new MouseButtonEventHandler(imgMap_MouseDown);
            RenderOptions.SetBitmapScalingMode(imgMap, BitmapScalingMode.HighQuality);

            imgMap.Margin = new Thickness(2, 0, 0, 0);
            panelCoordinates.Children.Add(imgMap);


            txtPosition = UICreator.CreateLink(this.Airliner.HasRoute ? this.Airliner.CurrentPosition.ToString() : this.Airliner.Homebase.Profile.Name);
            ((Hyperlink)txtPosition.Inlines.FirstInline).Click += new RoutedEventHandler(PageAirliner_Click);
            txtPosition.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            panelCoordinates.Children.Add(txtPosition);


            lbQuickInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageFleetAirliner", "1024"), panelCoordinates));

            return panelInfo;

        }

        private void btnEditName_Click(object sender, RoutedEventArgs e)
        {
            String s = (String)PopUpEditAirlinerName.ShowPopUp(this.Airliner);

            if (s != null)
            {
                this.Airliner.Name = s;

                txtName.Text = this.Airliner.Name;
            }
        }
        private void btnBuy_Click(object sender, System.Windows.RoutedEventArgs e)
        {
          
            if (this.Airliner.Airliner.getPrice() > GameObject.GetInstance().HumanAirline.Money)
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2109"), Translator.GetInstance().GetString("MessageBox", "2109", "message"), WPFMessageBoxButtons.Ok);
            else
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2110"), string.Format(Translator.GetInstance().GetString("MessageBox", "2110", "message"), this.Airliner.Name), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    this.Airliner.Purchased = FleetAirliner.PurchasedType.Bought;
                    AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -this.Airliner.Airliner.getPrice());

                    panelLeasedAirliner.Children.Clear();
                }
            }
        }
        private void PageAirliner_Click(object sender, RoutedEventArgs e)
        {
            PopUpMap.ShowPopUp(this.Airliner);
        }

        private void imgMap_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PopUpMap.ShowPopUp(this.Airliner);
        }


        private void PageFleetAirliner_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageAirline(this.Airliner.Airliner.Airline));
        }

        private void btnEditHomeBase_Click(object sender, RoutedEventArgs e)
        {
            Airport airport = (Airport)PopUpHomeBase.ShowPopUp(this.Airliner);

            if (airport != null && this.Airliner.Homebase != airport)
            {
                this.Airliner.Homebase = airport;

                lblAirport.Content = this.Airliner.Homebase;

            }
        }

        private static void imgAirlinerImage_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PopUpAirlinerImage.ShowPopUp((AirlinerType)((Image)sender).Tag);
        }
        private void lblAirport_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PageNavigator.NavigateTo(new PageAirport(this.Airliner.Homebase));
        }
    }
}
