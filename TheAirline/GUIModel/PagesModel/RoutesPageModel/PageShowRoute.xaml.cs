namespace TheAirline.GUIModel.PagesModel.RoutesPageModel
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
    using TheAirline.GUIModel.CustomControlsModel;
    using TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel;
    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.AirlinerModel;
    using TheAirline.Model.AirlinerModel.RouteModel;
    using TheAirline.Model.GeneralModel;

    /// <summary>
    ///     Interaction logic for PageShowRoute.xaml
    /// </summary>
    public partial class PageShowRoute : Page
    {
        #region Fields

        private readonly HumanRouteMVVM Route;
        private SpecialContract SelectedContract;

        #endregion

        #region Constructors and Destructors

        public PageShowRoute(Route route)
        {
            this.Classes = new ObservableCollection<MVVMRouteClass>();

            this.Route = new HumanRouteMVVM(route);
            this.DataContext = this.Route;

            if (this.Route.Route.Type == Model.AirlinerModel.RouteModel.Route.RouteType.Helicopter)
            {
                
                 RouteAirlinerClass rClass = ((PassengerRoute)this.Route.Route).getRouteAirlinerClass(AirlinerClass.ClassType.Economy_Class);
                 var mClass = new MVVMRouteClass(AirlinerClass.ClassType.Economy_Class, rClass.Seating, rClass.FarePrice);
                
                 this.Classes.Add(mClass);

             
            }
            if (this.Route.Route.Type == Model.AirlinerModel.RouteModel.Route.RouteType.Passenger || this.Route.Route.Type == Model.AirlinerModel.RouteModel.Route.RouteType.Mixed)
            {
                 foreach (AirlinerClass.ClassType cType in AirlinerClass.GetAirlinerTypes())
                {
                    if ((int)cType <= GameObject.GetInstance().GameTime.Year)
                    {
                          RouteAirlinerClass rClass = ((PassengerRoute)this.Route.Route).getRouteAirlinerClass(cType);
                          var mClass = new MVVMRouteClass(cType, rClass.Seating, rClass.FarePrice);

                          this.Classes.Add(mClass);
                    }
                }
            }

            this.Contracts = new ObservableCollection<SpecialContract>();

            var destination1 = this.Route.Route.Destination1;
            var destination2 = this.Route.Route.Destination2;

            var humanContracts = GameObject.GetInstance().HumanAirline.SpecialContracts.Where(s => s.Type.Routes.Exists(r => (r.Departure == destination1 && r.Destination == destination2) || (r.Departure == destination2 && r.Destination == destination1 && r.BothWays)));
          
            foreach (SpecialContract sc in humanContracts)
            {
                int routesCount = sc.Routes.Count(r => r.Destination1 == destination1 && r.Destination2 == destination2 && r.Type == this.Route.Route.Type);
                int contractRoutesCount = sc.Type.Routes.Count(r => r.RouteType == this.Route.Route.Type && ((r.Departure == destination1 && r.Destination == destination2) || (r.Departure == destination2 && r.Destination == destination1 && r.BothWays)));

                if (contractRoutesCount > routesCount)
                    this.Contracts.Add(sc);
            }
     
            this.InitializeComponent();

            this.Loaded += this.PageShowRoute_Loaded;


        }

        #endregion

        #region Public Properties
        public ObservableCollection<SpecialContract> Contracts { get; set; }

        public ObservableCollection<MVVMRouteClass> Classes { get; set; }
   
        #endregion

        #region Methods
        private void btnContract_Click(object sender, RoutedEventArgs e)
        {
            this.cbContract.SelectedItem = null;
        }
        private void tcMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Route.setFeedback();
        }

        private void PageShowRoute_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this.Route.Route.IsCargoRoute)
            {
                foreach (MVVMRouteClass rClass in this.Classes)
                {
                    foreach (MVVMRouteFacility rFacility in rClass.Facilities)
                    {
                        RouteFacility facility =
                            ((PassengerRoute)this.Route.Route).Classes.Find(c => c.Type == rClass.Type)
                                .Facilities.Find(f => f.Type == rFacility.Type);
                        rFacility.SelectedFacility = facility;
                    }
                }
            }
            var tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                TabItem airlinerItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Airliner").FirstOrDefault();

                airlinerItem.Visibility = Visibility.Collapsed;
            }

            var contract = this.Contracts.FirstOrDefault(c=>c.Routes.Exists(r=>r==this.Route.Route));

            this.SelectedContract = contract;

            this.cbContract.SelectedItem = this.SelectedContract;
       }
     
        private void btnDeleteRoute_Click(object sender, RoutedEventArgs e)
        {
            WPFMessageBoxResult result = WPFMessageBox.Show(
                Translator.GetInstance().GetString("MessageBox", "2503"),
                string.Format(
                    Translator.GetInstance().GetString("MessageBox", "2503", "message"),
                    this.Route.Route.Destination1.Profile.Name,
                    this.Route.Route.Destination2.Profile.Name),
                WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                var contract = GameObject.GetInstance().HumanAirline.SpecialContracts.Find(s => s.Routes.Contains(this.Route.Route));

                if (contract != null)
                {
                    contract.Routes.Remove(this.Route.Route);
                }

                GameObject.GetInstance().HumanAirline.removeRoute(this.Route.Route);

                if (this.Route.Route.HasAirliner)
                {
                    this.Route.Route.getAirliners().ForEach(a => a.removeRoute(this.Route.Route));
                }

                var tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

                if (tab_main != null)
                {
                    TabItem matchingItem =
                        tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Routes").FirstOrDefault();

                    tab_main.SelectedItem = matchingItem;
                }
            }
        }
        private void btnRouteFlights_Click(object sender, RoutedEventArgs e)
        {
            PopUpRouteFlights.ShowPopUp(this.Route.Route);
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SpecialContract contract = null;
             
            if (this.Contracts.Count > 0 && this.cbContract.SelectedItem != null)
            {
                contract = (SpecialContract)this.cbContract.SelectedItem;
            }

            if (contract != null && contract != this.SelectedContract)
                contract.Routes.Add(this.Route.Route);

            if (contract != this.SelectedContract)
                contract.Routes.Remove(this.Route.Route);

            //passenger route
            if (this.Route.Route is PassengerRoute)
            {
                foreach (MVVMRouteClass rac in this.Classes)
                {
                    ((PassengerRoute)this.Route.Route).getRouteAirlinerClass(rac.Type).FarePrice = rac.FarePrice;

                    ((PassengerRoute)this.Route.Route).getRouteAirlinerClass(rac.Type).Facilities.Clear();

                    foreach (MVVMRouteFacility facility in rac.Facilities)
                    {
                        ((PassengerRoute)this.Route.Route).getRouteAirlinerClass(rac.Type)
                            .addFacility(facility.SelectedFacility);
                    }
                }
            }
                //cargo route
            else if (this.Route.Route is CargoRoute)
            {
                double cargoPrice = Convert.ToDouble(this.txtCargoPrice.Text);
                ((CargoRoute)this.Route.Route).PricePerUnit = cargoPrice;
            }
                //mixed route
            else if (this.Route.Route is CombiRoute)
            {
                foreach (MVVMRouteClass rac in this.Classes)
                {
                    ((CombiRoute)this.Route.Route).getRouteAirlinerClass(rac.Type).FarePrice = rac.FarePrice;

                    ((CombiRoute)this.Route.Route).getRouteAirlinerClass(rac.Type).Facilities.Clear();

                    foreach (MVVMRouteFacility facility in rac.Facilities)
                    {
                        ((CombiRoute)this.Route.Route).getRouteAirlinerClass(rac.Type)
                            .addFacility(facility.SelectedFacility);
                    }
                }

                double cargoPrice = Convert.ToDouble(this.txtCargoPrice.Text);
                ((CombiRoute)this.Route.Route).PricePerUnit = cargoPrice;
            }
        }



        #endregion

      

     
    }

}