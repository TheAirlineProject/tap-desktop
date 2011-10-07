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
using TheAirlineV2.Model.AirlinerModel;
using TheAirlineV2.GraphicsModel.PageModel.GeneralModel;
using TheAirlineV2.GraphicsModel.PageModel.PageAirportModel;
using TheAirlineV2.GraphicsModel.PageModel.PageFleetAirlinerModel.PanelFleetAirlinerModel;
using TheAirlineV2.Model.GeneralModel;
using TheAirlineV2.Model.AirportModel;
using TheAirlineV2.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirlineV2.GraphicsModel.PageModel.PageAirlineModel;
using System.Windows.Markup;
using System.IO;
using System.Xml;
using TheAirlineV2.GraphicsModel.Converters;

namespace TheAirlineV2.GraphicsModel.PageModel.PageFleetAirlinerModel
{
    /// <summary>
    /// Interaction logic for PageFleetAirliner.xaml
    /// </summary>
    public partial class PageFleetAirliner : StandardPage
    {
        private FleetAirliner Airliner;
        private TextBlock txtFlown, txtPosition, txtSinceService, txtName;
        private ContentControl lblAirport;
        public PageFleetAirliner(FleetAirliner airliner)
        {
          
            this.Airliner = airliner;

            InitializeComponent();

            StackPanel airlinerPanel = new StackPanel();
            airlinerPanel.Margin = new Thickness(10, 0, 10, 0);

            airlinerPanel.Children.Add(createQuickInfoPanel());
            airlinerPanel.Children.Add(createAirlinerTypePanel());

            StandardContentPanel panelContent = new StandardContentPanel();

            panelContent.setContentPage(airlinerPanel, StandardContentPanel.ContentLocation.Left);


            StackPanel panelSideMenu = new PanelFleetAirliner(this.Airliner);

            panelContent.setContentPage(panelSideMenu, StandardContentPanel.ContentLocation.Right);



            base.setContent(panelContent);

            base.setHeaderContent(this.Airliner.Name);


            showPage(this);

            GameTimer.GetInstance().OnTimeChanged += new GameTimer.TimeChanged(PageFleetAirliner_OnTimeChanged);

        }

        private void PageFleetAirliner_OnTimeChanged()
        {
            if (this.IsLoaded)
            {
               
                Airport airport = this.Airliner.RouteAirliner == null ? null : Airports.GetAirport(this.Airliner.RouteAirliner.CurrentPosition);
               // txtFlown.Text = string.Format("{0:0,0} km.", this.Airliner.Airliner.Flown);
                txtFlown.Text = string.Format("{0:0.##} {1}", new NumberToUnitConverter().Convert(this.Airliner.Airliner.Flown), new StringToLanguageConverter().Convert("km."));
                //txtPosition.Text = 
                Run run = (Run)((Hyperlink)txtPosition.Inlines.FirstInline).Inlines.FirstInline;
                run.Text = this.Airliner.HasRoute ? (airport == null ? this.Airliner.RouteAirliner.CurrentPosition.ToString() : airport.Profile.Name) : this.Airliner.Homebase.Profile.Name;
            
               // txtSinceService.Text = string.Format("{0:0,0} km.", this.Airliner.Airliner.LastServiceCheck);
                txtSinceService.Text = string.Format("{0:0.##} {1}", new NumberToUnitConverter().Convert(this.Airliner.Airliner.LastServiceCheck), new StringToLanguageConverter().Convert("km."));
            }//husk også currency
       
        }
        //creates the info panel for the airliner type
        private Panel createAirlinerTypePanel()
        {
            AirlinerType airliner = this.Airliner.Airliner.Type;

            StackPanel panelAirlinerType = new StackPanel();
            panelAirlinerType.Margin = new Thickness(0, 50, 0, 0);

            TextBlock txtHeader = new TextBlock();
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush");
            txtHeader.TextAlignment = TextAlignment.Left;
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = "Airliner Type";

            panelAirlinerType.Children.Add(txtHeader);

            ListBox lbQuickInfo = new ListBox();
            lbQuickInfo.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbQuickInfo.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            panelAirlinerType.Children.Add(lbQuickInfo);

            lbQuickInfo.Items.Add(new QuickInfoValue("Name", UICreator.CreateTextBlock(airliner.Name)));

            ContentControl ccManufactorer = new ContentControl();
            ccManufactorer.SetResourceReference(ContentControl.ContentTemplateProperty, "ManufactorerCountryItem");
            ccManufactorer.Content = airliner.Manufacturer;

            lbQuickInfo.Items.Add(new QuickInfoValue("Manufactorer", ccManufactorer));
            lbQuickInfo.Items.Add(new QuickInfoValue("Body type", UICreator.CreateTextBlock(new TextUnderscoreConverter().Convert(airliner.Body, null, null, null).ToString())));
 
            string range = string.Format("{0:0.##} {1}", new NumberToUnitConverter().Convert(airliner.Range), new StringToLanguageConverter().Convert("km."));
            lbQuickInfo.Items.Add(new QuickInfoValue("Range type", UICreator.CreateTextBlock(string.Format("{0} ({1})", new TextUnderscoreConverter().Convert(airliner.RangeType), range))));
 
            //lbQuickInfo.Items.Add(new QuickInfoValue("Range type", UICreator.CreateTextBlock(new TextUnderscoreConverter().Convert(airliner.RangeType, null, null, null).ToString())));
            lbQuickInfo.Items.Add(new QuickInfoValue("Engine type", UICreator.CreateTextBlock(new TextUnderscoreConverter().Convert(airliner.Engine, null, null, null).ToString())));
            lbQuickInfo.Items.Add(new QuickInfoValue("Wingspan", UICreator.CreateTextBlock(string.Format("{0} m.", airliner.Wingspan))));
            lbQuickInfo.Items.Add(new QuickInfoValue("Length", UICreator.CreateTextBlock(string.Format("{0} m.", airliner.Length))));
            lbQuickInfo.Items.Add(new QuickInfoValue("Passenger capacity", UICreator.CreateTextBlock(airliner.MaxSeatingCapacity.ToString())));//SeatingCapacity.ToString())));
            lbQuickInfo.Items.Add(new QuickInfoValue("Max airliner classes", UICreator.CreateTextBlock(airliner.MaxAirlinerClasses.ToString())));

            string crewRequirements = string.Format("Cockpit: {0} Cabin: {1}",airliner.CockpitCrew,airliner.CabinCrew);
            lbQuickInfo.Items.Add(new QuickInfoValue("Crew requirements", UICreator.CreateTextBlock(crewRequirements)));
            //lbQuickInfo.Items.Add(new QuickInfoValue("Range", UICreator.CreateTextBlock(string.Format("{0:0.##} {1}", new NumberToUnitConverter().Convert(airliner.Range), new StringToLanguageConverter().Convert("km.")))));
            lbQuickInfo.Items.Add(new QuickInfoValue("Cruising speed", UICreator.CreateTextBlock(string.Format("{0:0.##} {1}", new NumberToUnitConverter().Convert(airliner.CruisingSpeed), new StringToLanguageConverter().Convert("km/t")))));
            lbQuickInfo.Items.Add(new QuickInfoValue("Fuel Consumption", UICreator.CreateTextBlock(string.Format("{0:0.###} {1}", new FuelConsumptionToUnitConverter().Convert(airliner.FuelConsumption), new StringToLanguageConverter().Convert("l/seat/km")))));
          //  lbQuickInfo.Items.Add(new QuickInfoValue("Produced", UICreator.CreateTextBlock(airliner.Produced.ToString())));

            return panelAirlinerType;

        }
        //creates the quick info panel for the fleet airliner
        private Panel createQuickInfoPanel()
        {
            Image imgEditName = new Image();
            imgEditName.Source = new BitmapImage(new Uri(@"/Data/images/edit.png", UriKind.RelativeOrAbsolute));
            imgEditName.Width = 16;
            //imgEdit.Margin = new Thickness(5, 0, 0, 0);
            RenderOptions.SetBitmapScalingMode(imgEditName, BitmapScalingMode.HighQuality);
            
     
            StackPanel panelInfo = new StackPanel();
           // panelInfo.Margin = new Thickness(0,10, 0, 0);

            TextBlock txtHeader = new TextBlock();
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush");
            txtHeader.TextAlignment = TextAlignment.Left;
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = "Airliner Profile";

            panelInfo.Children.Add(txtHeader);

            DockPanel grdQuickInfo = new DockPanel();
            grdQuickInfo.Margin = new Thickness(0, 5, 0, 0);

            panelInfo.Children.Add(grdQuickInfo);


            Image imgLogo = new Image();
            imgLogo.Source = new BitmapImage(new Uri(this.Airliner.Airline.Profile.Logo, UriKind.RelativeOrAbsolute));
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
            btnEditName.Visibility = this.Airliner.Airline.IsHuman ? Visibility.Visible : System.Windows.Visibility.Collapsed;
            btnEditName.Click += new RoutedEventHandler(btnEditName_Click);
            btnEditName.Content = imgEditName;

            panelName.Children.Add(btnEditName);

            lbQuickInfo.Items.Add(new QuickInfoValue("Name", panelName));

            TextBlock lnkOwner = UICreator.CreateLink(this.Airliner.Airline.Profile.Name);
            ((Hyperlink)lnkOwner.Inlines.FirstInline).Click += new RoutedEventHandler(PageFleetAirliner_Click);
            lbQuickInfo.Items.Add(new QuickInfoValue("Owner", lnkOwner));

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
            //imgEdit.Margin = new Thickness(5, 0, 0, 0);
            RenderOptions.SetBitmapScalingMode(imgEdit, BitmapScalingMode.HighQuality);
           
             
              btnEditHomeBase.Visibility = this.Airliner.Airline.IsHuman ? Visibility.Visible : System.Windows.Visibility.Collapsed;

            btnEditHomeBase.Content = imgEdit;

            panelHomeBase.Children.Add(btnEditHomeBase);
           


          
           

            lbQuickInfo.Items.Add(new QuickInfoValue("Home base", panelHomeBase));
           // lbQuickInfo.Items.Add(new QuickInfoValue("Airliner type", UICreator.CreateTextBlock(this.Airliner.Airliner.Type.Name)));
    
            lbQuickInfo.Items.Add(new QuickInfoValue("Built", UICreator.CreateTextBlock(string.Format("{0} ({1} years old)", this.Airliner.Airliner.BuiltDate.ToShortDateString(), this.Airliner.Airliner.Age))));
            lbQuickInfo.Items.Add(new QuickInfoValue("Tail number", UICreator.CreateTextBlock(this.Airliner.Airliner.TailNumber)));
            
            txtFlown = UICreator.CreateTextBlock(string.Format("{0:0.##} {1}", new NumberToUnitConverter().Convert(this.Airliner.Airliner.Flown), new StringToLanguageConverter().Convert("km.")));
            lbQuickInfo.Items.Add(new QuickInfoValue("Flown", txtFlown));

            txtSinceService =  UICreator.CreateTextBlock(string.Format("{0:0.##} {1}", new NumberToUnitConverter().Convert(this.Airliner.Airliner.LastServiceCheck), new StringToLanguageConverter().Convert("km.")));
            lbQuickInfo.Items.Add(new QuickInfoValue("Since last service check",txtSinceService));

          
            WrapPanel panelCoordinates = new WrapPanel();


            Image imgMap = new Image();
            imgMap.Source = new BitmapImage(new Uri(@"/Data/images/map.png", UriKind.RelativeOrAbsolute));
            imgMap.Height = 16;
            imgMap.MouseDown += new MouseButtonEventHandler(imgMap_MouseDown);
            RenderOptions.SetBitmapScalingMode(imgMap, BitmapScalingMode.HighQuality);

            imgMap.Margin = new Thickness(2, 0, 0, 0);
            panelCoordinates.Children.Add(imgMap);


            txtPosition = UICreator.CreateLink(this.Airliner.HasRoute ? this.Airliner.RouteAirliner.CurrentPosition.ToString() : this.Airliner.Homebase.Profile.Name);
            ((Hyperlink)txtPosition.Inlines.FirstInline).Click += new RoutedEventHandler(PageAirliner_Click);
            txtPosition.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            panelCoordinates.Children.Add(txtPosition);


            lbQuickInfo.Items.Add(new QuickInfoValue("Current position", panelCoordinates));

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
            PageNavigator.NavigateTo(new PageAirline(this.Airliner.Airline));
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

        private void lblAirport_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PageNavigator.NavigateTo(new PageAirport(this.Airliner.Homebase));
        }
    }
}
