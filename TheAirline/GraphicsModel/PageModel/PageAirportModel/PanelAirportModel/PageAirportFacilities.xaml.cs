using System;
using System.Collections.Generic;
using System.Globalization;
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
using TheAirline.Model.AirportModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.PageModel.PageAirlineModel;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GraphicsModel.PageModel.PageAirportModel.PanelAirportModel
{
    /// <summary>
    /// Interaction logic for PageAirportFacilities.xaml
    /// </summary>
    public partial class PageAirportFacilities : Page
    {
        private Airport Airport;
        private StackPanel panelFacilities;
        public PageAirportFacilities(Airport airport)
        {
            InitializeComponent();

            this.Language = XmlLanguage.GetLanguage(new CultureInfo(AppSettings.GetInstance().getLanguage().CultureInfo, true).IetfLanguageTag);

            this.Airport = airport;

            ScrollViewer svFacilities = new ScrollViewer();
            svFacilities.Margin = new Thickness(0, 10, 50, 0);
            svFacilities.MaxHeight = GraphicsHelpers.GetContentHeight() - 50;
            svFacilities.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            svFacilities.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            panelFacilities = new StackPanel();

            svFacilities.Content = panelFacilities;

            showFacilitiesInformation();

            this.Content = svFacilities;
        }

        //shows the facilities information
        private void showFacilitiesInformation()
        {
            panelFacilities.Children.Clear();

            TextBlock txtAirportFacilitiesHeader = new TextBlock();
            txtAirportFacilitiesHeader.Uid = "1003";
            txtAirportFacilitiesHeader.Margin = new Thickness(0, 10, 0, 0);
            txtAirportFacilitiesHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtAirportFacilitiesHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtAirportFacilitiesHeader.FontWeight = FontWeights.Bold;
            txtAirportFacilitiesHeader.Text = Translator.GetInstance().GetString("PageAirportFacilities", txtAirportFacilitiesHeader.Uid);
            panelFacilities.Children.Add(txtAirportFacilitiesHeader);

            ListBox lbAirportFacilities = new ListBox();
            lbAirportFacilities.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbAirportFacilities.ItemTemplate = this.Resources["AirportFacilityItem"] as DataTemplate;

            panelFacilities.Children.Add(lbAirportFacilities);

            var airportFacilities = this.Airport.getAirportFacilities().FindAll(f => f.Airline == null);

            foreach (AirlineAirportFacility facility in airportFacilities)
                lbAirportFacilities.Items.Add(facility);

            TextBlock txtFacilitiesHeader = new TextBlock();
            txtFacilitiesHeader.Uid = "1001";
            txtFacilitiesHeader.Margin = new Thickness(0, 10, 0, 0);
            txtFacilitiesHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtFacilitiesHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtFacilitiesHeader.FontWeight = FontWeights.Bold;
            txtFacilitiesHeader.Text = Translator.GetInstance().GetString("PageAirportFacilities", txtFacilitiesHeader.Uid);

            panelFacilities.Children.Add(txtFacilitiesHeader);

            ListBox lbAirlineAirportFacilities = new ListBox();
            lbAirlineAirportFacilities.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbAirlineAirportFacilities.ItemTemplate = this.Resources["AirlineFacilityItem"] as DataTemplate;

            panelFacilities.Children.Add(lbAirlineAirportFacilities);

            List<Airline> airlines = Airlines.GetAllAirlines();
            airlines.Sort((delegate(Airline a1, Airline a2) { return a1.Profile.Name.CompareTo(a2.Profile.Name); }));

            foreach (Airline airline in airlines)
            {
                foreach (AirportFacility.FacilityType type in Enum.GetValues(typeof(AirportFacility.FacilityType)))
                {
                    if (this.Airport.getAirportFacility(airline, type).TypeLevel != 0)
                        lbAirlineAirportFacilities.Items.Add(this.Airport.getAirlineAirportFacility(airline, type));
                }
            }

            TextBlock txtHumanFacilitiesHeader = new TextBlock();
            txtHumanFacilitiesHeader.Uid = "1002";
            txtHumanFacilitiesHeader.Margin = new Thickness(0, 10, 0, 0);
            txtHumanFacilitiesHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHumanFacilitiesHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHumanFacilitiesHeader.FontWeight = FontWeights.Bold;
            txtHumanFacilitiesHeader.Text = GameObject.GetInstance().HumanAirline.Profile.Name + " " + Translator.GetInstance().GetString("PageAirportFacilities", txtHumanFacilitiesHeader.Uid);

            panelFacilities.Children.Add(txtHumanFacilitiesHeader);

            ListBox lbHumanFacilities = new ListBox();
            lbHumanFacilities.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbHumanFacilities.ItemTemplate = this.Resources["HumanFacilityItem"] as DataTemplate;

            panelFacilities.Children.Add(lbHumanFacilities);

            foreach (AirportFacility.FacilityType type in Enum.GetValues(typeof(AirportFacility.FacilityType)))
            {
                lbHumanFacilities.Items.Add(new HumanFacilityType(this.Airport, this.Airport.getAirportFacility(GameObject.GetInstance().HumanAirline, type), getHumanNextAirportFacility(type)));
            }
        }

        //returns the next facility item for the specific type for the human airline
        private AirportFacility getHumanNextAirportFacility(AirportFacility.FacilityType type)
        {
            List<AirportFacility> facilities = AirportFacilities.GetFacilities(type);

            facilities.Sort((delegate(AirportFacility f1, AirportFacility f2) { return f1.TypeLevel.CompareTo(f2.TypeLevel); }));

            int index = facilities.FindIndex(f=>f.Uid == this.Airport.getAirportFacility(GameObject.GetInstance().HumanAirline, type).Uid);

            if (index < facilities.Count - 1)
                return facilities[index + 1];
            else
                return facilities[0];
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Airline airline = (Airline)((Hyperlink)sender).Tag;

            PageNavigator.NavigateTo(new PageAirline(airline));
        }
        private void ButtonBuy_Click(object sender, RoutedEventArgs e)
        {
            HumanFacilityType type = (HumanFacilityType)((Button)sender).Tag;

            if (type.NextFacility.Price > GameObject.GetInstance().HumanAirline.Money)
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2201"), Translator.GetInstance().GetString("MessageBox", "2201", "message"), WPFMessageBoxButtons.Ok);
            else
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2202"), string.Format(Translator.GetInstance().GetString("MessageBox", "2202", "message"), type.NextFacility.Name, type.NextFacility.Price), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    double price = type.NextFacility.Price;

                    if (this.Airport.Profile.Country != GameObject.GetInstance().HumanAirline.Profile.Country)
                        price = price * 1.25;

                    AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -price);

                    this.Airport.addAirportFacility(GameObject.GetInstance().HumanAirline, type.NextFacility, GameObject.GetInstance().GameTime.AddDays(type.NextFacility.BuildingDays));

                    showFacilitiesInformation();
                }
            }
        }
        private void ButtonSell_Click(object sender, RoutedEventArgs e)
        {
            HumanFacilityType type = (HumanFacilityType)((Button)sender).Tag;
            Boolean hasHub = this.Airport.getHubs().Count(h => h.Airline == GameObject.GetInstance().HumanAirline) > 0;

            Boolean hasCargoRoute = GameObject.GetInstance().HumanAirline.Routes.Exists(r => (r.Destination1 == this.Airport || r.Destination2 == this.Airport) && r.Type == Model.AirlinerModel.RouteModel.Route.RouteType.Cargo);
            Boolean airportHasCargoTerminal = this.Airport.getCurrentAirportFacility(null,AirportFacility.FacilityType.Cargo) != null && this.Airport.getCurrentAirportFacility(null,AirportFacility.FacilityType.Cargo).TypeLevel > 0;

            if ((type.CurrentFacility.TypeLevel == 1 && type.CurrentFacility.Type == AirportFacility.FacilityType.Service && this.Airport.hasAsHomebase(GameObject.GetInstance().HumanAirline)))
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2203"), Translator.GetInstance().GetString("MessageBox", "2203", "message"), WPFMessageBoxButtons.Ok);
            else if (type.CurrentFacility.Type == AirportFacility.FacilityType.Service && hasHub && type.CurrentFacility == Hub.MinimumServiceFacility)
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2214"), string.Format(Translator.GetInstance().GetString("MessageBox", "2214", "message"), Hub.MinimumServiceFacility.Name), WPFMessageBoxButtons.Ok);
            else if (type.CurrentFacility.Type == AirportFacility.FacilityType.Cargo && type.CurrentFacility.TypeLevel == 1 && hasCargoRoute && !airportHasCargoTerminal)
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2223"), Translator.GetInstance().GetString("MessageBox", "2223", "message"), WPFMessageBoxButtons.Ok);
            else
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2204"), string.Format(Translator.GetInstance().GetString("MessageBox", "2204", "message"), type.CurrentFacility.Name), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    this.Airport.downgradeFacility(GameObject.GetInstance().HumanAirline, type.NextFacility.Type);

                    showFacilitiesInformation();
                }
            }
        }

    }


    //the class for a facility type for the human airline
    public class HumanFacilityType
    {
        public AirportFacility CurrentFacility { get; set; }
        public AirportFacility NextFacility { get; set; }
        public Airport Airport { get; set; }
        public HumanFacilityType(Airport airport, AirportFacility current, AirportFacility next)
        {
            this.Airport = airport;
            this.CurrentFacility = current;
            this.NextFacility = next;
        }
    }

    //the converter for a facility button being enabled
    public class HumanFacilityButtonEnabled : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Boolean isEnabled = true;

            Airport airport = ((HumanFacilityType)value).Airport;

            AirportFacility.FacilityType type = ((HumanFacilityType)value).CurrentFacility.Type;

            string buttonType = (string)parameter;

            List<AirportFacility> facilities = AirportFacilities.GetFacilities(type);

            facilities.Sort((delegate(AirportFacility f1, AirportFacility f2) { return f1.TypeLevel.CompareTo(f2.TypeLevel); }));

            int index = facilities.FindIndex(f=>f.Uid == airport.getAirportFacility(GameObject.GetInstance().HumanAirline, type).Uid);
         
            if (buttonType == "Buy" && type != AirportFacility.FacilityType.CheckIn)
            {
                int gates = airport.Terminals.getNumberOfGates(GameObject.GetInstance().HumanAirline);
                isEnabled = index < facilities.Count - 1 && gates > 0 && airport.getAirlineAirportFacility(GameObject.GetInstance().HumanAirline, type).FinishedDate < GameObject.GetInstance().GameTime;
            }
            if (buttonType == "Sell" && type != AirportFacility.FacilityType.CheckIn)
            {
                isEnabled = index > 0 && airport.getAirlineAirportFacility(GameObject.GetInstance().HumanAirline, type).FinishedDate < GameObject.GetInstance().GameTime;
            }
            if (buttonType == "Buy" && type == AirportFacility.FacilityType.CheckIn)
            {
                isEnabled = index < facilities.Count - 1 && airport.getAirlineAirportFacility(GameObject.GetInstance().HumanAirline, type).FinishedDate < GameObject.GetInstance().GameTime;

            }
            if (buttonType == "Sell" && type == AirportFacility.FacilityType.CheckIn)
            {
                isEnabled = index > 0 && airport.Terminals.getNumberOfGates(GameObject.GetInstance().HumanAirline) == 0 && airport.getAirlineAirportFacility(GameObject.GetInstance().HumanAirline, type).FinishedDate < GameObject.GetInstance().GameTime;

            }
            return isEnabled;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
