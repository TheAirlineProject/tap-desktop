using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.GUIModel.PagesModel.GamePageModel;

namespace TheAirline.Views.Game
{
    /// <summary>
    ///     Interaction logic for PageNewGame.xaml
    /// </summary>
    [Export("PageNewGame")]
    public sealed partial class PageNewGame
    {
        #region Constructors and Destructors

        public PageNewGame()
        {
            InitializeComponent();

            Loaded += PageNewGame_Loaded;
        }

        #endregion

        #region Methods

        private void PageNewGame_Loaded(object sender, RoutedEventArgs e)
        {
            var frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            frmContent.Navigate(new PageStartData {Tag = this});
        }

        private void tcMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var control = (TabControl) sender;

            var selection = ((TabItem) control.SelectedItem).Tag.ToString();

            var frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            if (selection == "New" && frmContent != null)
            {
                frmContent.Navigate(new PageStartData {Tag = this});
            }

            if (selection == "Airline" && frmContent != null)
            {
                frmContent.Navigate(new PageNewAirline());
            }

            if (selection == "Difficulty" && frmContent != null)
            {
                frmContent.Navigate(new PageCreateDifficulty {Tag = this});
            }

            if (selection == "Scenario" && frmContent != null)
            {
                frmContent.Navigate(new PageShowScenario {Tag = this});
            }
        }

        #endregion
    }
}