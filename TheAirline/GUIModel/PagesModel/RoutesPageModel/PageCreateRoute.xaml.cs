using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GUIModel.PagesModel.RoutesPageModel
{
    /// <summary>
    /// Interaction logic for PageCreateRoute.xaml
    /// </summary>
    public partial class PageCreateRoute : Page
    {
        public List<Airport> Airports { get; set; }
        public List<MVVMRouteClass> Classes { get; set; }
        public PageCreateRoute()
        {
            this.Classes = new List<MVVMRouteClass>();

            foreach (AirlinerClass.ClassType type in Enum.GetValues(typeof(AirlinerClass.ClassType)))
            {
              
                MVVMRouteClass rClass = new MVVMRouteClass(type, RouteAirlinerClass.SeatingType.Reserved_Seating, 1);

                this.Classes.Add(rClass);
            }
            this.Airports = GameObject.GetInstance().HumanAirline.Airports;
            this.Loaded += PageCreateRoute_Loaded;
        
            InitializeComponent();

            
        }

        private void PageCreateRoute_Loaded(object sender, RoutedEventArgs e)
        {
            //hide the tab for a route
            TabControl tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                var matchingItem =
     tab_main.Items.Cast<TabItem>()
       .Where(item => item.Tag.ToString() == "Route")
       .FirstOrDefault();

                matchingItem.Visibility = System.Windows.Visibility.Collapsed;
            }

            //sets the selected items for the facilities
            foreach (MVVMRouteClass rClass in this.Classes)
            {
                foreach (MVVMRouteFacility rFacility in rClass.Facilities)
                    rFacility.SelectedFacility = rFacility.Facilities.OrderBy(f => f.ServiceLevel).First();

            }
        }
        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            Route route = null;
            Airport destination1 = (Airport)cbDestination1.SelectedItem;
            Airport destination2 = (Airport)cbDestination2.SelectedItem;
            Airport stopover1 = (Airport)cbStopover1.SelectedItem;
            Airport stopover2 = cbStopover2.Visibility == System.Windows.Visibility.Visible ? (Airport)cbStopover2.SelectedItem : null;

            try
            {
                if (AirlineHelpers.IsRouteDestinationsOk(GameObject.GetInstance().HumanAirline, destination1, destination2, rbPassenger.IsChecked.Value ? Route.RouteType.Passenger : Route.RouteType.Cargo, stopover1, stopover2))
                {


                    Guid id = Guid.NewGuid();

                    //passenger route
                    if (rbPassenger.IsChecked.Value)
                    {
                        route = new PassengerRoute(id.ToString(), destination1, destination2, 0);

                        foreach (MVVMRouteClass rac in this.Classes)
                        {
                            ((PassengerRoute)route).getRouteAirlinerClass(rac.Type).FarePrice = rac.FarePrice;

                            foreach (MVVMRouteFacility facility in rac.Facilities)
                                ((PassengerRoute)route).getRouteAirlinerClass(rac.Type).addFacility(facility.SelectedFacility);

                        }
                    }
                    //cargo route
                    else
                    {
                        double cargoPrice = Convert.ToDouble(txtCargoPrice.Text);
                        route = new CargoRoute(id.ToString(), destination1, destination2, cargoPrice);
                    }

                    FleetAirlinerHelpers.CreateStopoverRoute(route, stopover1, stopover2);

                    GameObject.GetInstance().HumanAirline.addRoute(route);

                    TabControl tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

                    if (tab_main != null)
                    {
                        var matchingItem =
             tab_main.Items.Cast<TabItem>()
               .Where(item => item.Tag.ToString() == "Routes")
               .FirstOrDefault();

                        tab_main.SelectedItem = matchingItem;
                    }

                }
            }
            catch (Exception ex)
            {
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", ex.Message), Translator.GetInstance().GetString("MessageBox", ex.Message, "message"), WPFMessageBoxButtons.Ok);
            }
        }

        private void btnLoadConfiguration_Click(object sender, RoutedEventArgs e)
        {
            ComboBox cbConfigurations = new ComboBox();
            cbConfigurations.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbConfigurations.SelectedValuePath = "Name";
            cbConfigurations.DisplayMemberPath = "Name";
            cbConfigurations.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbConfigurations.Width = 200;

            foreach (RouteClassesConfiguration confItem in Configurations.GetConfigurations(Configuration.ConfigurationType.Routeclasses))
                cbConfigurations.Items.Add(confItem);

            cbConfigurations.SelectedIndex = 0;

            if (PopUpSingleElement.ShowPopUp(Translator.GetInstance().GetString("PageCreateRoute","1012"), cbConfigurations) == PopUpSingleElement.ButtonSelected.OK && cbConfigurations.SelectedItem != null)
            {
                
                RouteClassesConfiguration configuration = (RouteClassesConfiguration)cbConfigurations.SelectedItem;

                foreach (RouteClassConfiguration classConfiguration in configuration.getClasses())
                {
                    MVVMRouteClass rClass = this.Classes.Find(c => c.Type == classConfiguration.Type);

                     foreach (RouteFacility facility in classConfiguration.getFacilities())
                     {
                         MVVMRouteFacility rFacility = rClass.Facilities.Find(f => f.Type == facility.Type);

                         rFacility.SelectedFacility = facility;

                     }
                 
                }
            }

           
        }

        private void btnStopover1_Click(object sender, RoutedEventArgs e)
        {
            cbStopover1.SelectedItem = null;
        }

        private void btnStopover2_Click(object sender, RoutedEventArgs e)
        {
            cbStopover2.SelectedItem = null;

         }

        
    }
   
   
}
