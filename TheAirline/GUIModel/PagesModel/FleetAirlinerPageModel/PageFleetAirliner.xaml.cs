namespace TheAirline.GUIModel.PagesModel.FleetAirlinerPageModel
{
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.AirlinerModel;

    /// <summary>
    ///     Interaction logic for PageFleetAirliner.xaml
    /// </summary>
    public partial class PageFleetAirliner : Page
    {
        #region Constructors and Destructors

        public PageFleetAirliner(FleetAirliner airliner)
        {
            this.Airliner = new FleetAirlinerMVVM(airliner);
            this.Loaded += this.PageFleetAirliner_Loaded;

            this.InitializeComponent();
        }

        #endregion

        #region Public Properties

        public FleetAirlinerMVVM Airliner { get; set; }

        #endregion

        #region Methods

        private void PageFleetAirliner_Loaded(object sender, RoutedEventArgs e)
        {
            var frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            frmContent.Navigate(new PageFleetAirlinerInformation(this.Airliner) { Tag = this });

            var tab_main = UIHelpers.FindChild<TabControl>(this, "tabMenu");

            if (tab_main != null)
            {
                TabItem infoItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Information").FirstOrDefault();

                //matchingItem.IsSelected = true;
                infoItem.Header = this.Airliner.Airliner.Name;

                TabItem maintenanceItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Maintenance").FirstOrDefault();

                if (!this.Airliner.Airliner.Airliner.Airline.IsHuman)
                {
                    maintenanceItem.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void tcMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var control = (TabControl)sender;

            string selection = ((TabItem)control.SelectedItem).Tag.ToString();

            var frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            if (selection == "Information" && frmContent != null)
            {
                frmContent.Navigate(new PageFleetAirlinerInformation(this.Airliner) { Tag = this });
            }

            if (selection == "Maintenance" && frmContent != null)
            {
                frmContent.Navigate(new PageFleetAirlinerInsurances(this.Airliner) { Tag = this });
            }

            if (selection == "Statistics" && frmContent != null)
            {
                frmContent.Navigate(new PageFleetAirlinerStatistics(this.Airliner) { Tag = this });
            }
        }

        #endregion
    }
}