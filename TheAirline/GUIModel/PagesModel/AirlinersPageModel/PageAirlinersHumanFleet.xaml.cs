namespace TheAirline.GUIModel.PagesModel.AirlinersPageModel
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.AirlinerModel;
    using TheAirline.Model.GeneralModel;

    /// <summary>
    ///     Interaction logic for PageAirlinersHumanFleet.xaml
    /// </summary>
    public partial class PageAirlinersHumanFleet : Page
    {
        #region Constructors and Destructors

        public PageAirlinersHumanFleet()
        {
            this.Loaded += PageAirlinersHumanFleet_Loaded;
            this.Fleet = new ObservableCollection<FleetAirliner>();

            GameObject.GetInstance()
                .HumanAirline.Fleet.Where(f => f.Airliner.BuiltDate < GameObject.GetInstance().GameTime && f.Airliner.Status == Airliner.StatusTypes.Normal)
                .ToList().ForEach(f => this.Fleet.Add(f));

            this.OrderedFleet = new ObservableCollection<FleetAirliner>();

                GameObject.GetInstance()
                    .HumanAirline.Fleet.Where(f => f.Airliner.BuiltDate >= GameObject.GetInstance().GameTime)
                    .ToList().ForEach(f=>this.OrderedFleet.Add(f));

                this.OutleasedFleet = new ObservableCollection<FleetAirliner>();
               GameObject.GetInstance()
                   .HumanAirline.Fleet.Where(f => f.Airliner.BuiltDate < GameObject.GetInstance().GameTime && f.Airliner.Status == Airliner.StatusTypes.Leasing)
                   .ToList().ForEach(f=>this.OutleasedFleet.Add(f));
           

            this.InitializeComponent();
        }

        private void PageAirlinersHumanFleet_Loaded(object sender, RoutedEventArgs e)
        {
            var tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                TabItem matchingItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Manufacturer").FirstOrDefault();

                matchingItem.Visibility = Visibility.Collapsed;

                matchingItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Airliner").FirstOrDefault();

                matchingItem.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Public Properties

        public ObservableCollection<FleetAirliner> OutleasedFleet { get; set; }

        public ObservableCollection<FleetAirliner> Fleet { get; set; }

        public ObservableCollection<FleetAirliner> OrderedFleet { get; set; }

        #endregion
       
    }
}