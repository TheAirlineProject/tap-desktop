using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GUIModel.PagesModel.AirportPageModel
{
    /// <summary>
    /// Interaction logic for PageAirportFacilities.xaml
    /// </summary>
    public partial class PageAirportFacilities : Page
    {
        public AirportMVVM Airport { get; set; }
        public List<AirportFacility.FacilityType> FacilityTypes { get; set; }
        public PageAirportFacilities(AirportMVVM airport)
        {
            this.Airport = airport;
            this.DataContext = this.Airport;

            this.FacilityTypes = Enum.GetValues(typeof(AirportFacility.FacilityType)).Cast<AirportFacility.FacilityType>().ToList();

            InitializeComponent();

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lbFacilities.ItemsSource);
            view.SortDescriptions.Clear();

            SortDescription sortAirlineDescription = new SortDescription("Airline.Profile.Name", ListSortDirection.Ascending);
            view.SortDescriptions.Add(sortAirlineDescription);

        
        }

        private void btnDeleteFacility_Click(object sender, RoutedEventArgs e)
        {
            AirlineAirportFacility facility = (AirlineAirportFacility)((Button)sender).Tag;

            Boolean hasHub = this.Airport.Airport.getHubs().Count(h => h.Airline == GameObject.GetInstance().HumanAirline) > 0;

            Boolean hasCargoRoute = GameObject.GetInstance().HumanAirline.Routes.Exists(r => (r.Destination1 == this.Airport.Airport || r.Destination2 == this.Airport.Airport) && r.Type == Model.AirlinerModel.RouteModel.Route.RouteType.Cargo);
            Boolean airportHasCargoTerminal = this.Airport.Airport.getCurrentAirportFacility(null, AirportFacility.FacilityType.Cargo) != null && this.Airport.Airport.getCurrentAirportFacility(null, AirportFacility.FacilityType.Cargo).TypeLevel > 0;

            if ((facility.Facility.TypeLevel == 1 && facility.Facility.Type == AirportFacility.FacilityType.Service && this.Airport.Airport.hasAsHomebase(GameObject.GetInstance().HumanAirline)))
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2203"), Translator.GetInstance().GetString("MessageBox", "2203", "message"), WPFMessageBoxButtons.Ok);
            else if (facility.Facility.Type == AirportFacility.FacilityType.Service && hasHub && facility.Facility == Hub.MinimumServiceFacility)
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2214"), string.Format(Translator.GetInstance().GetString("MessageBox", "2214", "message"), Hub.MinimumServiceFacility.Name), WPFMessageBoxButtons.Ok);
            else if (facility.Facility.Type == AirportFacility.FacilityType.Cargo && facility.Facility.TypeLevel == 1 && hasCargoRoute && !airportHasCargoTerminal)
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2223"), Translator.GetInstance().GetString("MessageBox", "2223", "message"), WPFMessageBoxButtons.Ok);
            else
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2204"), string.Format(Translator.GetInstance().GetString("MessageBox", "2204", "message"), facility.Facility.Name), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    this.Airport.removeAirlineFacility(facility);
                }
            }

          
        }

        private void btnBuyFacility_Click(object sender, RoutedEventArgs e)
        {
           AirportFacility.FacilityType type = (AirportFacility.FacilityType)cbNextFacility.SelectedItem;

           AirlineAirportFacility currentFacility = this.Airport.Airport.getAirlineAirportFacility(GameObject.GetInstance().HumanAirline, type);

           List<AirportFacility> facilities = AirportFacilities.GetFacilities(type);
           facilities = facilities.OrderBy(f => f.TypeLevel).ToList();

           int index = facilities.FindIndex(f => currentFacility.Facility == f);
            
            AirportFacility facility = facilities[index + 1];

           if (facility.Price > GameObject.GetInstance().HumanAirline.Money)
               WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2201"), Translator.GetInstance().GetString("MessageBox", "2201", "message"), WPFMessageBoxButtons.Ok);
           else
           {
               WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2202"), string.Format(Translator.GetInstance().GetString("MessageBox", "2202", "message"), facility.Name, facility.Price), WPFMessageBoxButtons.YesNo);

               if (result == WPFMessageBoxResult.Yes)
               {
                   double price = facility.Price;

                   if (this.Airport.Airport.Profile.Country != GameObject.GetInstance().HumanAirline.Profile.Country)
                       price = price * 1.25;

                   AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -price);

                   this.Airport.addAirlineFacility(facility);
 
               }
           }

        }
    }
}
