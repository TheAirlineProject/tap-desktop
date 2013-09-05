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
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GUIModel.PagesModel.RoutesPageModel
{
    /// <summary>
    /// Interaction logic for PageRoutes.xaml
    /// </summary>
    public partial class PageRoutes : Page
    {
        public List<RouteProfitMVVM> ProfitRoutes { get; set; }
        public List<Route> RequestedRoutes { get; set; }
        public PageRoutes()
        {
            var routes = GameObject.GetInstance().HumanAirline.Routes.OrderByDescending(r => r.Balance);

            double totalProfit = routes.Sum(r=>r.Balance);

            this.ProfitRoutes = new List<RouteProfitMVVM>();
            foreach (Route route in routes.Take(Math.Min(5,routes.Count())))
            {
                this.ProfitRoutes.Add(new RouteProfitMVVM(route,totalProfit));
            }
         
            var requestedRoutes = GameObject.GetInstance().HumanAirline.Routes.OrderByDescending(r => r.Destination1.getDestinationPassengersRate(r.Destination2,AirlinerClass.ClassType.Economy_Class) + r.Destination2.getDestinationPassengersRate(r.Destination1,AirlinerClass.ClassType.Economy_Class));
            this.RequestedRoutes = requestedRoutes.Take(Math.Min(5,routes.Count())).ToList();
         
            this.Loaded += PageRoutes_Loaded;
            InitializeComponent();
        }

        private void PageRoutes_Loaded(object sender, RoutedEventArgs e)
        {
            Frame frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            frmContent.Navigate(new PageHumanRoutes() { Tag = this });

        }
        private void tcMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl control = (TabControl)sender;

            string selection = ((TabItem)control.SelectedItem).Tag.ToString();

            Frame frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            if (selection == "Routes" && frmContent != null)
                frmContent.Navigate(new PageHumanRoutes() { Tag = this });

            if (selection == "Create" && frmContent != null)
                frmContent.Navigate(new PageCreateRoute() { Tag = this });

            if (selection == "Airliners" && frmContent != null)
                frmContent.Navigate(new PageAssignAirliners() { Tag = this });
          
           
        }
    }
}
