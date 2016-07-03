using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Models.Airliners;
using TheAirline.Models.General;

namespace TheAirline.GUIModel.PagesModel.AirlinersPageModel
{
    /// <summary>
    ///     Interaction logic for PageAirlinersHumanFleet.xaml
    /// </summary>
    public partial class PageAirlinersHumanFleet : Page
    {
        #region Constructors and Destructors

        public PageAirlinersHumanFleet()
        {
            this.Loaded += PageAirlinersHumanFleet_Loaded;
            this.Fleet =
                GameObject.GetInstance()
                    .HumanAirline.Fleet.Where(f => f.Airliner.BuiltDate < GameObject.GetInstance().GameTime && f.Airliner.Status == Airliner.StatusTypes.Normal)
                    .ToList();
            this.OrderedFleet =
                GameObject.GetInstance()
                    .HumanAirline.Fleet.Where(f => f.Airliner.BuiltDate >= GameObject.GetInstance().GameTime)
                    .ToList();
            this.OutleasedFleet =
               GameObject.GetInstance()
                   .HumanAirline.Fleet.Where(f => f.Airliner.BuiltDate < GameObject.GetInstance().GameTime && f.Airliner.Status == Airliner.StatusTypes.Leasing)
                   .ToList();
           

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

        public List<FleetAirliner> OutleasedFleet { get; set; }

        public List<FleetAirliner> Fleet { get; set; }

        public List<FleetAirliner> OrderedFleet { get; set; }

        #endregion
       
    }
}