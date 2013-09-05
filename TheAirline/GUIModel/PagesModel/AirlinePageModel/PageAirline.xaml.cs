using System;
using System.Collections.Generic;
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
using TheAirline.Model.AirportModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel;
using TheAirline.GUIModel.PagesModel.RoutesPageModel;
using TheAirline.GUIModel.PagesModel.AirlinersPageModel;

namespace TheAirline.GUIModel.PagesModel.AirlinePageModel
{
    /// <summary>
    /// Interaction logic for PageAirline.xaml
    /// </summary>
    public partial class PageAirline : Page
    {
        private AirlineMVVM Airline;
        public List<RouteProfitMVVM> ProfitRoutes { get; set; }
        public List<AirlineFleetSizeMVVM> MostUsedAircrafts { get; set; }
        public List<Airport> MostGates { get; set; }

        public PageAirline(Airline airline)
        {
            this.Airline = new AirlineMVVM(airline);

            var airports = this.Airline.Airline.Airports;

            var routes = this.Airline.Airline.Routes.OrderByDescending(r => r.Balance);

            double totalProfit = routes.Sum(r => r.Balance);

            this.ProfitRoutes = new List<RouteProfitMVVM>();
            foreach (Route route in routes.Take(Math.Min(5, routes.Count())))
            {
                this.ProfitRoutes.Add(new RouteProfitMVVM(route, totalProfit));
            }
            
            this.MostGates = airports.OrderByDescending(a => a.getAirlineContracts(this.Airline.Airline).Sum(c => c.NumberOfGates)).Take(Math.Min(5, airports.Count)).ToList();
            this.MostUsedAircrafts = new List<AirlineFleetSizeMVVM>();

            var query = this.Airline.Airline.Fleet.GroupBy(a => a.Airliner.Type)
                  .Select(group =>
                        new
                        {
                            Type = group.Key,
                            Fleet = group
                        })
                  .OrderByDescending(g => g.Fleet.Count());

            foreach (var group in query)
            {
                this.MostUsedAircrafts.Add(new AirlineFleetSizeMVVM(group.Type,group.Fleet.Count()));
            }

            

            this.Loaded += PageAirline_Loaded;

            InitializeComponent();


        }

        private void PageAirline_Loaded(object sender, RoutedEventArgs e)
        {
            Frame frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            frmContent.Navigate(new PageAirlineInfo(this.Airline){ Tag = this });

            TabControl tab_main = UIHelpers.FindChild<TabControl>(this, "tcMenu");

            if (tab_main != null)
            {
                var matchingItem =
     tab_main.Items.Cast<TabItem>()
       .Where(item => item.Tag.ToString() == "Overview")
       .FirstOrDefault();

                //matchingItem.IsSelected = true;
                matchingItem.Header = this.Airline.Airline.Profile.Name ;
                matchingItem.Visibility = System.Windows.Visibility.Visible;

                tab_main.SelectedItem = matchingItem;
            }

        }
        private void tcMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl control = (TabControl)sender;

            string selection = ((TabItem)control.SelectedItem).Tag.ToString();

            Frame frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            if (selection == "Overview" && frmContent != null)
                frmContent.Navigate(new PageAirlineInfo(this.Airline) { Tag = this });

            if (selection == "Finances" && frmContent != null)
                frmContent.Navigate(new PageAirlineFinances(this.Airline) { Tag = this });

            if (selection == "Employees" && frmContent != null)
                frmContent.Navigate(new PageAirlineEmployees(this.Airline) { Tag = this });

            if (selection == "Services" && frmContent != null)
                frmContent.Navigate(new PageAirlineServices(this.Airline) { Tag = this });

            if (selection == "Ratings" && frmContent != null)
                frmContent.Navigate(new PageAirlineRatings(this.Airline) { Tag = this });

            if (selection == "Insurances" && frmContent != null)
                frmContent.Navigate(new PageAirlineInsurance(this.Airline) { Tag = this });
        }
    }
}
