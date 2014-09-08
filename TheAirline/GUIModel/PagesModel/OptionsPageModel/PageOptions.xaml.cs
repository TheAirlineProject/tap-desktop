namespace TheAirline.GUIModel.PagesModel.OptionsPageModel
{
    using System.Windows;
    using System.Windows.Controls;

    using TheAirline.GUIModel.HelpersModel;

    /// <summary>
    ///     Interaction logic for PageOptions.xaml
    /// </summary>
    public partial class PageOptions : Page
    {
        #region Constructors and Destructors

        public PageOptions()
        {
            this.Loaded += this.PageOptions_Loaded;
            this.InitializeComponent();
        }

        #endregion

        #region Methods

        private void PageOptions_Loaded(object sender, RoutedEventArgs e)
        {
            var frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            frmContent.Navigate(new PageShowOptions { Tag = this });
        }

        private void tcMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var control = (TabControl)sender;

            string selection = ((TabItem)control.SelectedItem).Tag.ToString();

            var frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            if (selection == "Options" && frmContent != null)
            {
                frmContent.Navigate(new PageShowOptions { Tag = this });
            }

            if (selection == "Save" && frmContent != null)
            {
                frmContent.Navigate(new PageSaveGame { Tag = this });
            }

            if (selection == "Load" && frmContent != null)
            {
                frmContent.Navigate(new PageLoadGame { Tag = this });
            }

            if (selection == "Credits" && frmContent != null)
            {
                frmContent.Navigate(new PageOptionsCredits { Tag = this });
            }
        }

        #endregion
    }
}