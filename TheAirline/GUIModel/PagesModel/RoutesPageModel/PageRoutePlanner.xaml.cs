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
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GUIModel.PagesModel.RoutesPageModel
{
    /// <summary>
    /// Interaction logic for PageRoutePlanner.xaml
    /// </summary>
    public partial class PageRoutePlanner : Page
    {
        public FleetAirliner Airliner { get; set; }
        public List<RoutePlannerItemMVVM> AllRoutes { get; set; }
        public List<Region> AllRegions { get; set; }
        public List<Route> Routes { get; set; }
        public List<Airport> OutboundAirports { get; set; }
        public List<IntervalType> IntervalTypes
        {
            get { return Enum.GetValues(typeof(IntervalType)).Cast<IntervalType>().ToList(); }
            private set { ;}
        }
        public List<OpsType> OpsTypes 
        {
            get { return Enum.GetValues(typeof(OpsType)).Cast<OpsType>().ToList(); }
            private set { ;} 
        
        }
        public List<int> Intervals { get; set; }
        public PageRoutePlanner(FleetAirliner airliner)
        {
            this.Airliner = airliner;

            this.AllRoutes = new List<RoutePlannerItemMVVM>();
            this.Intervals = new List<int>() { 1, 2, 3, 4, 5, 6 };

            this.Routes = this.Airliner.Airliner.Airline.Routes;
            
            this.AllRegions = new List<Region>();
            this.AllRegions.Add(Regions.GetRegion("100"));

            var routeRegions = this.Routes.Select(r => r.Destination1.Profile.Country.Region).ToList();
            routeRegions.AddRange(this.Routes.Select(r => r.Destination2.Profile.Country.Region));

            foreach (Region region in routeRegions.Distinct())
                this.AllRegions.Add(region);

            foreach (Route route in this.Airliner.Airliner.Airline.Routes)
                this.AllRoutes.Add(new RoutePlannerItemMVVM(route, this.Airliner.Airliner.Type));

            this.OutboundAirports = new List<Airport>();
            var routeAirports = this.Routes.Select(r => r.Destination1).ToList();
            routeAirports.AddRange(this.Routes.Select(r => r.Destination2));

            foreach (Airport airport in routeAirports.Distinct())
                this.OutboundAirports.Add(airport);
                     
            this.Loaded += PageRoutePlanner_Loaded;

            InitializeComponent();
        }

        private void PageRoutePlanner_Loaded(object sender, RoutedEventArgs e)
        {
            TabControl tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                var matchingItem =
     tab_main.Items.Cast<TabItem>()
       .Where(item => item.Tag.ToString() == "Route")
       .FirstOrDefault();

                matchingItem.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void cbRegion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Region region = (Region)((ComboBox)sender).SelectedItem;

            if (cbRoute != null)
            {
                var source = cbRoute.Items as ICollectionView;
                source.Filter = o =>
                {
                    Route r = o as Route;
                    return r.Destination1.Profile.Country.Region == region || r.Destination2.Profile.Country.Region == region || region.Uid == "100";
                };
            }



        }

        private void cbOutbound_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Airport outbuound = (Airport)((ComboBox)sender).SelectedItem;

            var routes = this.Airliner.Airliner.Airline.Routes.Where(r => r.Destination2 == outbuound || r.Destination1 == outbuound);

            var airports = routes.Select(r => r.Destination1 == outbuound ? r.Destination2 : r.Destination1);

            if (cbHomebound != null)
            {
                cbHomebound.Items.Clear();

                foreach (Airport airport in airports)
                    cbHomebound.Items.Add(airport);

                cbHomebound.SelectedIndex = 0;
            }

        }

        private void BtnSwap_Click(object sender, RoutedEventArgs e)
        {
            Airport outbound = (Airport)cbOutbound.SelectedItem;
            Airport homebound = (Airport)cbHomebound.SelectedItem;

            cbOutbound.SelectedItem = homebound;
            cbHomebound.SelectedItem = outbound;
        }
    }
}
