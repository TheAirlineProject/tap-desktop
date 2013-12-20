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
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GUIModel.PagesModel.RoutesPageModel
{
    /// <summary>
    /// Interaction logic for PageHumanRoutes.xaml
    /// </summary>
    public partial class PageHumanRoutes : Page
    {
        public PageHumanRoutes()
        {
            var routes = new List<RouteMVVM>();

            foreach (Route route in GameObject.GetInstance().HumanAirline.Routes)
                routes.Add(new RouteMVVM(route));

            this.DataContext = routes;
            this.Loaded += PageHumanRoutes_Loaded;  

            InitializeComponent();
  
        }

        private void PageHumanRoutes_Loaded(object sender, RoutedEventArgs e)
        {
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
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            Route route = (Route)((Button)sender).Tag;

            TabControl tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                var matchingItem =
     tab_main.Items.Cast<TabItem>()
       .Where(item => item.Tag.ToString() == "Route")
       .FirstOrDefault();

                //matchingItem.IsSelected = true;
                matchingItem.Header = string.Format("{0}-{1}",route.Destination1.Profile.Name,route.Destination2.Profile.Name);
                matchingItem.Visibility = System.Windows.Visibility.Visible;

                tab_main.SelectedItem = matchingItem;
            }

            Frame frmContent = UIHelpers.FindChild<Frame>(this.Tag as Page, "frmContent");

            if (frmContent != null)
            {
                frmContent.Navigate(new PageShowRoute(route) { Tag = this.Tag });

            }
        }

        private void btnMap_Click(object sender, RoutedEventArgs e)
        {
            PopUpMap.ShowPopUp(GameObject.GetInstance().HumanAirline.Routes);
        }
    }
}
