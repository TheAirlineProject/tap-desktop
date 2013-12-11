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
using TheAirline.GUIModel.MasterPageModel.PopUpPageModel;
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
        public List<AirlinerType> HumanAircrafts { get; set; }
        public List<MVVMRouteClass> Classes { get; set; }
        public PageCreateRoute()
        {
            this.Classes = new List<MVVMRouteClass>();

            foreach (AirlinerClass.ClassType type in AirlinerClass.GetAirlinerTypes())
            {
                if ((int)type <= GameObject.GetInstance().GameTime.Year)
                {     
                    MVVMRouteClass rClass = new MVVMRouteClass(type, RouteAirlinerClass.SeatingType.Reserved_Seating, 1);

                    this.Classes.Add(rClass);
                }
            }

            this.Airports = GameObject.GetInstance().HumanAirline.Airports.OrderByDescending(a=>a==GameObject.GetInstance().HumanAirline.Airports[0]).ThenBy(a => a.Profile.Name).ToList();

            AirlinerType dummyAircraft = new AirlinerCargoType(new Manufacturer("Dummy", "", null,false), "All Aircrafts", "", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, AirlinerType.BodyType.Single_Aisle, AirlinerType.TypeRange.Regional, AirlinerType.EngineType.Jet, new Period<DateTime>(DateTime.Now, DateTime.Now), 0);

            this.HumanAircrafts = new List<AirlinerType>();

            this.HumanAircrafts.Add(dummyAircraft);


            foreach (AirlinerType type in GameObject.GetInstance().HumanAirline.Fleet.Select(f => f.Airliner.Type).Distinct())
                this.HumanAircrafts.Add(type);
                        
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

                
                    var airlinerItem = tab_main.Items.Cast<TabItem>()
           .Where(item => item.Tag.ToString() == "Airliner")
           .FirstOrDefault();

                    airlinerItem.Visibility = System.Windows.Visibility.Collapsed;
            }

            //sets the selected items for the facilities
            foreach (MVVMRouteClass rClass in this.Classes)
            {
                foreach (MVVMRouteFacility rFacility in rClass.Facilities)
                    rFacility.SelectedFacility = rFacility.Facilities.OrderBy(f => f.ServiceLevel).First();

            }
        }
        private void btnCreateNew_Click(object sender, RoutedEventArgs e)
        {
            if (createRoute())
            {
                TabControl tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

                if (tab_main != null)
                {
                    var matchingCreateItem =
         tab_main.Items.Cast<TabItem>()
           .Where(item => item.Tag.ToString() == "Create")
           .FirstOrDefault();

                    tab_main.SelectedItem = matchingCreateItem;
                }
            }
        }
        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (createRoute())
            {
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
        //creates a route and returns if success
        private Boolean createRoute()
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

                    return true;

                }
            }
            catch (Exception ex)
            {
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", ex.Message), Translator.GetInstance().GetString("MessageBox", ex.Message, "message"), WPFMessageBoxButtons.Ok);

                return false;
            }

            return false;
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

            WPFPopUpLoadConfiguration loadConfiguration = new WPFPopUpLoadConfiguration(Configuration.ConfigurationType.Routeclasses);

            if (PopUpSingleElement.ShowPopUp(Translator.GetInstance().GetString("PageCreateRoute", "1012"), cbConfigurations) == PopUpSingleElement.ButtonSelected.OK && cbConfigurations.SelectedItem != null)
            {

                RouteClassesConfiguration configuration = (RouteClassesConfiguration)cbConfigurations.SelectedItem;

                foreach (RouteClassConfiguration classConfiguration in configuration.getClasses())
                {
                    MVVMRouteClass rClass = this.Classes.Find(c => c.Type == classConfiguration.Type);

                    if (rClass != null)
                    {
                        foreach (RouteFacility facility in classConfiguration.getFacilities())
                        {
                            MVVMRouteFacility rFacility = rClass.Facilities.Find(f => f.Type == facility.Type);

                            if (rFacility != null)
                                rFacility.SelectedFacility = facility;

                        }
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

        private void cbDestination_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Airport destination1 = (Airport)cbDestination1.SelectedItem;
            Airport destination2 = (Airport)cbDestination2.SelectedItem;

            if (destination1 != null && destination2 != null)
            {
                foreach (MVVMRouteClass rClass in this.Classes)
                {
                    rClass.FarePrice = PassengerHelpers.GetPassengerPrice(destination1, destination2) * GeneralHelpers.ClassToPriceFactor(rClass.Type);
                }

                txtDistance.Text = new DistanceToUnitConverter().Convert(MathHelpers.GetDistance(destination1, destination2)).ToString();
            }
        }

        private void cbAircraft_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AirlinerType type = (AirlinerType)((ComboBox)sender).SelectedItem;

            if (cbDestination2.ItemsSource != null && cbDestination1.SelectedItem != null)
            {
                var source = cbDestination2.Items as ICollectionView;
                source.Filter = o =>
                {
                    Airport a = o as Airport;
                    return a != null && a.getMaxRunwayLength() >= type.MinRunwaylength && MathHelpers.GetDistance((Airport)cbDestination1.SelectedItem,a)<= type.Range || type.Manufacturer.Name == "Dummy"; 
                };
            }
        }



    }


}
