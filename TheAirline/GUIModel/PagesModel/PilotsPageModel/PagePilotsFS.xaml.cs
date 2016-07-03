namespace TheAirline.GUIModel.PagesModel.PilotsPageModel
{
    using System.Windows;
    using System.Windows.Controls;

    using TheAirline.GUIModel.HelpersModel;

    /// <summary>
    ///     Interaction logic for PagePilotsFS.xaml
    /// </summary>
    public partial class PagePilotsFS : Page
    {
        #region Constructors and Destructors

        public PagePilotsFS()
        {
            this.Loaded += this.PagePilotsFS_Loaded;
            this.InitializeComponent();
        }

        #endregion

        #region Methods

        private void PagePilotsFS_Loaded(object sender, RoutedEventArgs e)
        {
            var frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            frmContent.Navigate(new PagePilots { Tag = this });
        }

        private void tcMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var control = (TabControl)sender;

            string selection = ((TabItem)control.SelectedItem).Tag.ToString();

            var frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            if (selection == "Pilots" && frmContent != null)
            {
                frmContent.Navigate(new PagePilots { Tag = this });
            }

            if (selection == "Flightschools" && frmContent != null)
            {
                frmContent.Navigate(new PageFlightSchools { Tag = this });
            }
        }

        #endregion
    }
}