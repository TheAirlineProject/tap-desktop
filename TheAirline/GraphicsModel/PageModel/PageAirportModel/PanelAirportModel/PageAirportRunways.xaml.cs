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
using TheAirline.Model.AirportModel;
using System.Windows.Markup;
using System.Globalization;
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;

namespace TheAirline.GraphicsModel.PageModel.PageAirportModel.PanelAirportModel
{
    /// <summary>
    /// Interaction logic for PageAirportRunways.xaml
    /// </summary>
    public partial class PageAirportRunways : Page
    {
        private Airport Airport;
        public PageAirportRunways(Airport airport)
        {
            InitializeComponent();

            this.Language = XmlLanguage.GetLanguage(new CultureInfo(AppSettings.GetInstance().getLanguage().CultureInfo, true).IetfLanguageTag); 

            this.Airport = airport;

            StackPanel panelRunways = new StackPanel();
            panelRunways.Margin = new Thickness(0, 10, 50, 0);

            
            TextBlock txtRunwaysInfoHeader = new TextBlock();
            txtRunwaysInfoHeader.Uid = "1001";
            txtRunwaysInfoHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtRunwaysInfoHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtRunwaysInfoHeader.FontWeight = FontWeights.Bold;
            txtRunwaysInfoHeader.Text = Translator.GetInstance().GetString("PageAirportRunways", txtRunwaysInfoHeader.Uid);
            txtRunwaysInfoHeader.Margin = new Thickness(0, 10, 0, 0);

            panelRunways.Children.Add(txtRunwaysInfoHeader);


            ListBox lbRunway = new ListBox();
            lbRunway.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbRunway.ItemTemplate = this.Resources["RunwayItem"] as DataTemplate;

            foreach (Runway runway in this.Airport.Runways)
                lbRunway.Items.Add(runway);

            panelRunways.Children.Add(lbRunway);

            this.Content = panelRunways;
        }

    }
}
