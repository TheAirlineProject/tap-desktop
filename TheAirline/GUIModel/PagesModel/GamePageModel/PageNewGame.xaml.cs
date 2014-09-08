namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    using System.Windows;
    using System.Windows.Controls;

    using TheAirline.GUIModel.HelpersModel;

    /// <summary>
    ///     Interaction logic for PageNewGame.xaml
    /// </summary>
    public partial class PageNewGame : Page
    {
        #region Constructors and Destructors

        public PageNewGame()
        {
            this.InitializeComponent();

            this.Loaded += this.PageNewGame_Loaded;
        }

        #endregion

        #region Methods

        private void PageNewGame_Loaded(object sender, RoutedEventArgs e)
        {
            var frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            frmContent.Navigate(new PageStartData { Tag = this });
        }

        private void tcMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var control = (TabControl)sender;

            string selection = ((TabItem)control.SelectedItem).Tag.ToString();

            var frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            if (selection == "New" && frmContent != null)
            {
                frmContent.Navigate(new PageStartData { Tag = this });
            }

            if (selection == "Airline" && frmContent != null)
            {
                frmContent.Navigate(new PageNewAirline());
            }

            if (selection == "Difficulty" && frmContent != null)
            {
                frmContent.Navigate(new PageCreateDifficulty { Tag = this });
            }

            if (selection == "Scenario" && frmContent != null)
            {
                frmContent.Navigate(new PageShowScenario { Tag = this });
            }
        }

        #endregion
    }
}