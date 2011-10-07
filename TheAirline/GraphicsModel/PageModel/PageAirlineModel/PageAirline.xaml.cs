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

namespace TheAirline.GraphicsModel.PageModel.PageAirlineModel
{
    /// <summary>
    /// Interaction logic for PageAirline.xaml
    /// </summary>
    public partial class PageAirline : StandardPage
    {
        private Airline Airline;
        public PageAirline(Airline airline)
        {
            InitializeComponent();

            this.Airline = airline;

            StackPanel airportPanel = new StackPanel();
            airportPanel.Margin = new Thickness(10, 0, 10, 0);

            airportPanel.Children.Add(createQuickInfoPanel());

            StandardContentPanel panelContent = new StandardContentPanel();

            panelContent.setContentPage(airportPanel, StandardContentPanel.ContentLocation.Left);


            StackPanel panelSideMenu = new PanelAirline(this.Airline);

            panelContent.setContentPage(panelSideMenu, StandardContentPanel.ContentLocation.Right);



            base.setContent(panelContent);

            base.setHeaderContent(this.Airline.Profile.Name);

            

            showPage(this);
            
        }
        //creates the quick info panel for the airline
        private Panel createQuickInfoPanel()
        {
            StackPanel panelInfo = new StackPanel();
            panelInfo.Margin = new Thickness(5, 0, 10, 10);

            TextBlock txtHeader = new TextBlock();
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush");
            txtHeader.TextAlignment = TextAlignment.Left;
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = "Profile";

            panelInfo.Children.Add(txtHeader);

            DockPanel grdQuickInfo = new DockPanel();
            grdQuickInfo.Margin = new Thickness(0, 5, 0, 0);

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

            lbQuickInfo.Items.Add(new QuickInfoValue("Name", UICreator.CreateTextBlock(this.Airline.Profile.Name)));
            lbQuickInfo.Items.Add(new QuickInfoValue("IATA code", UICreator.CreateTextBlock(this.Airline.Profile.IATACode)));
            lbQuickInfo.Items.Add(new QuickInfoValue("CEO", UICreator.CreateTextBlock(this.Airline.Profile.CEO)));

            ContentControl lblFlag = new ContentControl();
            lblFlag.SetResourceReference(ContentControl.ContentTemplateProperty, "CountryFlagLongItem");
            lblFlag.Content = this.Airline.Profile.Country;

            lbQuickInfo.Items.Add(new QuickInfoValue("Home country", lblFlag));
            lbQuickInfo.Items.Add(new QuickInfoValue("Airline color",UICreator.CreateColorRect(this.Airline.Profile.Color)));
            lbQuickInfo.Items.Add(new QuickInfoValue("Value",createAirlineValuePanel()));
            lbQuickInfo.Items.Add(new QuickInfoValue("Reputation",createAirlineReputationPanel()));
            lbQuickInfo.Items.Add(new QuickInfoValue("Passenger happiness", UICreator.CreateTextBlock(String.Format("{0:0.00} %",PassengerHelpers.GetPassengersHappiness(this.Airline)))));

            return panelInfo;

        }
        //creates the panel for airline value
        public WrapPanel createAirlineValuePanel()
        {
            WrapPanel panelValue = new WrapPanel();

            for (int i = 0; i <= (int)this.Airline.getValue(); i++)
            {
                Image imgValue = new Image();
                imgValue.Source = new BitmapImage(new Uri(@"/Data/images/coins.png", UriKind.RelativeOrAbsolute));
                imgValue.Height = 20;
                RenderOptions.SetBitmapScalingMode(imgValue, BitmapScalingMode.HighQuality);

                panelValue.Children.Add(imgValue);
            }
            for (int i = (int)this.Airline.getValue(); i < (int)Airline.AirlineValue.Very_high; i++)
            {
                Image imgValue = new Image();
                imgValue.Source = new BitmapImage(new Uri(@"/Data/images/coins_gray.png", UriKind.RelativeOrAbsolute));
                imgValue.Height = 20;
                RenderOptions.SetBitmapScalingMode(imgValue, BitmapScalingMode.HighQuality);

                panelValue.Children.Add(imgValue);
            }

            return panelValue;
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
        
    }
}
