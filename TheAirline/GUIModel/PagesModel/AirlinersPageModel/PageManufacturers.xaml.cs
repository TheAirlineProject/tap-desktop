namespace TheAirline.GUIModel.PagesModel.AirlinersPageModel
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;

    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.AirlinerModel;
    using TheAirline.Model.GeneralModel;

    /// <summary>
    ///     Interaction logic for PageManufacturers.xaml
    /// </summary>
    public partial class PageManufacturers : Page
    {
        #region Constructors and Destructors

        public PageManufacturers()
        {
            var airlinerTypes = new List<AirlinerType>(AirlinerTypes.GetAllTypes());
            this.AllManufacturers = (from a in airlinerTypes
                where
                    a.Produced.From <= GameObject.GetInstance().GameTime
                    && a.Produced.To >= GameObject.GetInstance().GameTime
                orderby a.Manufacturer.Name
                select a.Manufacturer).Distinct().ToList();
            this.Loaded += this.PageManufacturers_Loaded;

            this.InitializeComponent();
        }

        #endregion

        #region Public Properties

        public List<Manufacturer> AllManufacturers { get; set; }

        #endregion

        #region Methods

        private void PageManufacturers_Loaded(object sender, RoutedEventArgs e)
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

        private void lnkManufacturer_Click(object sender, RoutedEventArgs e)
        {
            var manufacturer = (Manufacturer)((Hyperlink)sender).Tag;

            var tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                TabItem matchingItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Manufacturer").FirstOrDefault();

                matchingItem.Header = manufacturer.Name;
                matchingItem.Visibility = Visibility.Visible;
                tab_main.SelectedItem = matchingItem;
            }

            var frmContent = UIHelpers.FindChild<Frame>(this.Tag as Page, "frmContent");

            if (frmContent != null)
            {
                frmContent.Navigate(new PageManufacturer(manufacturer) { Tag = this.Tag });
            }
        }

        #endregion
    }
}