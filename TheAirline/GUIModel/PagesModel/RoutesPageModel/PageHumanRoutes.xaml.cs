namespace TheAirline.GUIModel.PagesModel.RoutesPageModel
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
    using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
    using TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel.PopUpMapModel;
    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.AirlineModel;
    using TheAirline.Model.AirlinerModel;
    using TheAirline.Model.AirlinerModel.RouteModel;
    using TheAirline.Model.GeneralModel;

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

            this.DataContext = routes;
            this.Loaded += this.PageHumanRoutes_Loaded;

            IEnumerable<Route> codesharingRoutes =
                GameObject.GetInstance()
                    .HumanAirline.Codeshares.Where(
                        c =>
                            c.Airline2 == GameObject.GetInstance().HumanAirline
                            || c.Type == CodeshareAgreement.CodeshareType.Both_Ways)
                    .Select(c => c.Airline1 == GameObject.GetInstance().HumanAirline ? c.Airline2 : c.Airline1)
                    .SelectMany(a => a.Routes);

            this.CodesharingRoutes = new ObservableCollection<Route>(codesharingRoutes);
            this.SelectedRoutes = new ObservableCollection<RouteMVVM>();
            this.PriceChanges = new ObservableCollection<double>() { -90, -75, -50, -45, -35, -25, -20, -15, -10, -5, 0, 5, 10, 15, 20, 25, 35, 45, 50, 75, 100 };
            this.Classes = new ObservableCollection<MVVMRouteClass>();

            this.InitializeComponent();
        }

        #endregion

        #region Public Properties

        public ObservableCollection<Route> CodesharingRoutes { get; set; }
        public ObservableCollection<RouteMVVM> SelectedRoutes { get; set; }
        public ObservableCollection<double> PriceChanges { get; set; }
        public ObservableCollection<MVVMRouteClass> Classes { get; set; }

        #endregion

        #region Methods

        private void PageHumanRoutes_Loaded(object sender, RoutedEventArgs e)
        {
            var tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

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

            var tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

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

            var frmContent = UIHelpers.FindChild<Frame>(this.Tag as Page, "frmContent");

            if (frmContent != null)
            {
                frmContent.Navigate(new PageShowRoute(route) { Tag = this.Tag });
            }
        }

        private void btnMap_Click(object sender, RoutedEventArgs e)
        {
            PopUpMapControl.ShowPopUp(null, GameObject.GetInstance().HumanAirline.Routes);
        }

        #endregion

        private void cbRoute_Checked(object sender, RoutedEventArgs e)
        {
            RouteMVVM route = (RouteMVVM)((CheckBox)sender).Tag;

            this.SelectedRoutes.Add(route);

            if (this.SelectedRoutes.Count == 1)
            {
                this.Classes.Clear();

                foreach (AirlinerClass.ClassType cType in AirlinerClass.GetAirlinerTypes())
                {
                    if ((int)cType <= GameObject.GetInstance().GameTime.Year)
                    {
                        var mClass = new MVVMRouteClass(cType, RouteAirlinerClass.SeatingType.Free_Seating, 10);

                        this.Classes.Add(mClass);
                    }
                }

                foreach (MVVMRouteClass rClass in this.Classes)
                {
                    foreach (MVVMRouteFacility rFacility in rClass.Facilities)
                    {
                        RouteFacility tFacility = new RouteFacility("t1000", rFacility.Type, "No Change", 0, RouteFacility.ExpenseType.Fixed, 0, null);
                        rFacility.Facilities.Insert(0, tFacility);
                        rFacility.SelectedFacility = rFacility.Facilities[0];
                    }
                }

                cbChangePrice.SelectedIndex = this.PriceChanges.Count / 2;
            }
        }

        private void cbRoute_Unchecked(object sender, RoutedEventArgs e)
        {
            RouteMVVM route = (RouteMVVM)((CheckBox)sender).Tag;

            this.SelectedRoutes.Remove(route);
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            WPFMessageBoxResult result = WPFMessageBox.Show(
                   Translator.GetInstance().GetString("MessageBox", "2707"),
                   Translator.GetInstance().GetString("MessageBox", "2707", "message"),
                   WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                double value = (double)cbChangePrice.SelectedItem;

                foreach (MVVMRouteClass rClass in this.Classes)
                {
                    foreach (RouteMVVM route in this.SelectedRoutes)
                    {
                        RouteAirlinerClass raClass;

                        raClass = ((PassengerRoute)route.Route).getRouteAirlinerClass(rClass.Type);
                        
                        raClass.FarePrice = raClass.FarePrice * (1 + (value / 100));

                        foreach (MVVMRouteFacility rFacility in rClass.Facilities)
                        {
                            var sFacility = rFacility.SelectedFacility;

                            if (sFacility != null && sFacility.Uid != "t1000")
                            {
                                raClass.addFacility(sFacility);
                            }
                        }


                    }
                }

                while (this.SelectedRoutes.Count > 0)
                    this.SelectedRoutes.RemoveAt(0);
            }

        }

    }
}