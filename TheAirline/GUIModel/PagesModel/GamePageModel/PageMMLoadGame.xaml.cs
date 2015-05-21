using TheAirline.Views.Game;

namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    using System.Windows.Controls;

    using TheAirline.GUIModel.HelpersModel;

    /// <summary>
    ///     Interaction logic for PageMMLoadGame.xaml
    /// </summary>
    public partial class PageMMLoadGame : Page
    {
        #region Constructors and Destructors

        public PageMMLoadGame()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Methods

        private void tcMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var control = (TabControl)sender;

            string selection = ((TabItem)control.SelectedItem).Tag.ToString();

            if (selection == "Back")
            {
                //PageNavigator.NavigateTo(new PageStartMenu());
            }
        }

        #endregion
    }
}