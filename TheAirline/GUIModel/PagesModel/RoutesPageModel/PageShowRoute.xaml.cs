using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Models.Airliners;
using TheAirline.Models.General;
using TheAirline.Models.Routes;

namespace TheAirline.GUIModel.PagesModel.RoutesPageModel
{
    /// <summary>
    ///     Interaction logic for PageShowRoute.xaml
    /// </summary>
    public partial class PageShowRoute : Page
    {
        #region Fields

        private readonly HumanRouteMVVM Route;

        #endregion

        #region Constructors and Destructors

        public PageShowRoute(Route route)
        {
            Classes = new ObservableCollection<MVVMRouteClass>();

            Route = new HumanRouteMVVM(route);
            DataContext = Route;

            if (Route.Route.Type == Models.Routes.Route.RouteType.Helicopter)
            {
                
                 RouteAirlinerClass rClass = ((PassengerRoute)Route.Route).GetRouteAirlinerClass(AirlinerClass.ClassType.EconomyClass);
                 var mClass = new MVVMRouteClass(AirlinerClass.ClassType.EconomyClass, rClass.Seating, rClass.FarePrice);
                
                 Classes.Add(mClass);

             
            }
            if (Route.Route.Type == Models.Routes.Route.RouteType.Passenger || Route.Route.Type == Models.Routes.Route.RouteType.Mixed)
            {
                 foreach (AirlinerClass.ClassType cType in AirlinerClass.GetAirlinerTypes())
                {
                    if ((int)cType <= GameObject.GetInstance().GameTime.Year)
                    {
                          RouteAirlinerClass rClass = ((PassengerRoute)Route.Route).GetRouteAirlinerClass(cType);
                          var mClass = new MVVMRouteClass(cType, rClass.Seating, rClass.FarePrice);

                          Classes.Add(mClass);
                    }
                }
            }
          

            InitializeComponent();

            Loaded += PageShowRoute_Loaded;


        }

        #endregion

        #region Public Properties

        public ObservableCollection<MVVMRouteClass> Classes { get; set; }

        #endregion

        #region Methods

        private void tcMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Route.setFeedback();
        }

        private void PageShowRoute_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Route.Route.IsCargoRoute)
            {
                foreach (MVVMRouteClass rClass in Classes)
                {
                    foreach (MVVMRouteFacility rFacility in rClass.Facilities)
                    {
                        RouteFacility facility =
                            ((PassengerRoute)Route.Route).Classes.Find(c => c.Type == rClass.Type)
                                .Facilities.Find(f => f.Type == rFacility.Type);
                        rFacility.SelectedFacility = facility;
                    }
                }
            }
            var tab_main = UIHelpers.FindChild<TabControl>(Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                TabItem airlinerItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Airliner").FirstOrDefault();

                airlinerItem.Visibility = Visibility.Collapsed;
            }
        }

        private void btnDeleteRoute_Click(object sender, RoutedEventArgs e)
        {
            WPFMessageBoxResult result = WPFMessageBox.Show(
                Translator.GetInstance().GetString("MessageBox", "2503"),
                string.Format(
                    Translator.GetInstance().GetString("MessageBox", "2503", "message"),
                    Route.Route.Destination1.Profile.Name,
                    Route.Route.Destination2.Profile.Name),
                WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                var contract = GameObject.GetInstance().HumanAirline.SpecialContracts.Find(s => s.Routes.Contains(Route.Route));

                if (contract != null)
                {
                    contract.Routes.Remove(Route.Route);
                }

                GameObject.GetInstance().HumanAirline.RemoveRoute(Route.Route);

                if (Route.Route.HasAirliner)
                {
                    Route.Route.GetAirliners().ForEach(a => a.RemoveRoute(Route.Route));
                }

                var tab_main = UIHelpers.FindChild<TabControl>(Tag as Page, "tabMenu");

                if (tab_main != null)
                {
                    TabItem matchingItem =
                        tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Routes").FirstOrDefault();

                    tab_main.SelectedItem = matchingItem;
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            //passenger route
            if (Route.Route is PassengerRoute)
            {
                foreach (MVVMRouteClass rac in Classes)
                {
                    ((PassengerRoute)Route.Route).GetRouteAirlinerClass(rac.Type).FarePrice = rac.FarePrice;

                    ((PassengerRoute)Route.Route).GetRouteAirlinerClass(rac.Type).Facilities.Clear();

                    foreach (MVVMRouteFacility facility in rac.Facilities)
                    {
                        ((PassengerRoute)Route.Route).GetRouteAirlinerClass(rac.Type)
                            .AddFacility(facility.SelectedFacility);
                    }
                }
            }
                //cargo route
            else if (Route.Route is CargoRoute)
            {
                double cargoPrice = Convert.ToDouble(txtCargoPrice.Text);
                ((CargoRoute)Route.Route).PricePerUnit = cargoPrice;
            }
                //mixed route
            else if (Route.Route is CombiRoute)
            {
                foreach (MVVMRouteClass rac in Classes)
                {
                    ((CombiRoute)Route.Route).GetRouteAirlinerClass(rac.Type).FarePrice = rac.FarePrice;

                    ((CombiRoute)Route.Route).GetRouteAirlinerClass(rac.Type).Facilities.Clear();

                    foreach (MVVMRouteFacility facility in rac.Facilities)
                    {
                        ((CombiRoute)Route.Route).GetRouteAirlinerClass(rac.Type)
                            .AddFacility(facility.SelectedFacility);
                    }
                }

                double cargoPrice = Convert.ToDouble(txtCargoPrice.Text);
                ((CombiRoute)Route.Route).PricePerUnit = cargoPrice;
            }
        }

        #endregion

     
    }

}