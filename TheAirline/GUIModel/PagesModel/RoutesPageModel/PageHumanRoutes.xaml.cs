using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Models.Airlines;
using TheAirline.Models.General;
using TheAirline.Models.Routes;

namespace TheAirline.GUIModel.PagesModel.RoutesPageModel
{
    /// <summary>
    ///     Interaction logic for PageHumanRoutes.xaml
    /// </summary>
    public partial class PageHumanRoutes : Page
    {
        #region Constructors and Destructors

        public PageHumanRoutes()
        {
            var routes = new List<RouteMVVM>();

            foreach (Route route in GameObject.GetInstance().HumanAirline.Routes)
            {
                routes.Add(new RouteMVVM(route));
            }

            DataContext = routes;
            Loaded += PageHumanRoutes_Loaded;

            IEnumerable<Route> codesharingRoutes =
                GameObject.GetInstance()
                    .HumanAirline.Codeshares.Where(
                        c =>
                            c.Airline2 == GameObject.GetInstance().HumanAirline
                            || c.Type == CodeshareAgreement.CodeshareType.BothWays)
                    .Select(c => c.Airline1 == GameObject.GetInstance().HumanAirline ? c.Airline2 : c.Airline1)
                    .SelectMany(a => a.Routes);

            CodesharingRoutes = codesharingRoutes.ToList();

            InitializeComponent();
        }

        #endregion

        #region Public Properties

        public List<Route> CodesharingRoutes { get; set; }

        #endregion

        #region Methods

        private void PageHumanRoutes_Loaded(object sender, RoutedEventArgs e)
        {
            var tab_main = UIHelpers.FindChild<TabControl>(Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                TabItem matchingItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Route").FirstOrDefault();

                matchingItem.Visibility = Visibility.Collapsed;

                TabItem airlinerItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Airliner").FirstOrDefault();

                airlinerItem.Visibility = Visibility.Collapsed;
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            var route = (Route)((Button)sender).Tag;

            var tab_main = UIHelpers.FindChild<TabControl>(Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                TabItem matchingItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Route").FirstOrDefault();

                //matchingItem.IsSelected = true;
                matchingItem.Header = string.Format(
                    "{0}-{1}",
                    route.Destination1.Profile.Name,
                    route.Destination2.Profile.Name);
                matchingItem.Visibility = Visibility.Visible;

                tab_main.SelectedItem = matchingItem;
            }

            var frmContent = UIHelpers.FindChild<Frame>(Tag as Page, "frmContent");

            if (frmContent != null)
            {
                frmContent.Navigate(new PageShowRoute(route) { Tag = Tag });
            }
        }

        private void btnMap_Click(object sender, RoutedEventArgs e)
        {
            PopUpMap.ShowPopUp(GameObject.GetInstance().HumanAirline.Routes);
        }

        #endregion
    }
}