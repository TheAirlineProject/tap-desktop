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
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;

namespace TheAirline.GUIModel.PagesModel.RoutesPageModel
{
    /// <summary>
    /// Interaction logic for PageRoutePlanner.xaml
    /// </summary>
    public partial class PageRoutePlanner : Page
    {
        public FleetAirliner Airliner { get; set; }
        public List<RoutePlannerItemMVVM> AllRoutes { get; set; }
        public PageRoutePlanner(FleetAirliner airliner)
        {
            this.Airliner = airliner;

            this.AllRoutes = new List<RoutePlannerItemMVVM>();

            foreach (Route route in Airlines.GetAllAirlines().SelectMany(a => a.Routes))
                this.AllRoutes.Add(new RoutePlannerItemMVVM(route,this.Airliner.Airliner.Type));

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
    }
}
