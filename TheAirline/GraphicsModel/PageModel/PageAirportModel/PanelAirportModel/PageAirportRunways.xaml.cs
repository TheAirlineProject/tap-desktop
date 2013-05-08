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
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;

namespace TheAirline.GraphicsModel.PageModel.PageAirportModel.PanelAirportModel
{
    /// <summary>
    /// Interaction logic for PageAirportRunways.xaml
    /// </summary>
    public partial class PageAirportRunways : Page
    {
        private Airport Airport;
        private ListBox lbRunway;//, lbRunwayBuilding;
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

            lbRunway = new ListBox();
            lbRunway.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbRunway.ItemTemplate = this.Resources["RunwayItem"] as DataTemplate;

            panelRunways.Children.Add(lbRunway);

            /*
            TextBlock txtRunwaysBuildHeader = new TextBlock();
            txtRunwaysBuildHeader.Uid = "1002";
            txtRunwaysBuildHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtRunwaysBuildHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtRunwaysBuildHeader.FontWeight = FontWeights.Bold;
            txtRunwaysBuildHeader.Text = Translator.GetInstance().GetString("PageAirportRunways", txtRunwaysBuildHeader.Uid);
            txtRunwaysBuildHeader.Margin = new Thickness(0, 5, 0, 0);

            panelRunways.Children.Add(txtRunwaysBuildHeader);

            lbRunwayBuilding = new ListBox();
            lbRunwayBuilding.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbRunwayBuilding.ItemTemplate = this.Resources["RunwayBuildItem"] as DataTemplate;

            panelRunways.Children.Add(lbRunwayBuilding);

            WrapPanel panelButtons = new WrapPanel();
            panelButtons.Margin = new Thickness(0, 5, 0, 0);

            Button btnBuildRunway = new Button();
            btnBuildRunway.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnBuildRunway.Uid = "201";
            btnBuildRunway.Height = Double.NaN;
            btnBuildRunway.Width = Double.NaN;
            btnBuildRunway.Content = Translator.GetInstance().GetString("PageAirportRunways", btnBuildRunway.Uid);
            btnBuildRunway.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnBuildRunway.Click += new RoutedEventHandler(btnBuildRunway_Click);
            btnBuildRunway.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnBuildRunway.Visibility = System.Windows.Visibility.Collapsed;
            panelButtons.Children.Add(btnBuildRunway);

            panelRunways.Children.Add(panelButtons);
            */
            this.Content = panelRunways;

            showRunways();
        }
        //shows the runways
        private void showRunways()
        {
            lbRunway.Items.Clear();
            //lbRunwayBuilding.Items.Clear();

            var buildRunways = this.Airport.Runways.FindAll(r => r.BuiltDate <= GameObject.GetInstance().GameTime);

            foreach (Runway runway in buildRunways)
                lbRunway.Items.Add(runway);

            //var buildingRunways = this.Airport.Runways.FindAll(r => r.BuiltDate > GameObject.GetInstance().GameTime);

            //foreach (Runway runway in buildingRunways)
              //  lbRunwayBuilding.Items.Add(runway);

        }
        private void btnBuildRunway_Click(object sender, RoutedEventArgs e)
        {
            Runway runway = (Runway)PopUpBuildRunway.ShowPopUp(this.Airport);

            if (runway != null)
            {
                double runwayPrice = AirportHelpers.GetAirportRunwayPrice(this.Airport, runway.Length);

                if (runwayPrice > GameObject.GetInstance().HumanAirline.Money)
                {
                    WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2219"), string.Format(Translator.GetInstance().GetString("MessageBox", "2219", "message"), this.Airport.Profile.Name, runwayPrice), WPFMessageBoxButtons.Ok);

                }
                else
                {

                    WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2218"), string.Format(Translator.GetInstance().GetString("MessageBox", "2218", "message"), this.Airport.Profile.Name, runwayPrice), WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {

                        this.Airport.Runways.Add(runway);

                        AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, runwayPrice);

                        showRunways();
                    }
                }
            }
        }

    }
}
