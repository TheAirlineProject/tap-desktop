using TheAirline.Views.Game;

namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    using System.Windows;
    using System.Windows.Controls;

    using TheAirline.GUIModel.HelpersModel;

    /// <summary>
    ///     Interaction logic for PageCredits.xaml
    /// </summary>
    public partial class PageCredits : Page
    {
        #region Constructors and Destructors

        public PageCredits()
        {
            this.InitializeComponent();
            
           this.DataContext = new GameVersionMVVM();
        }

        #endregion

        #region Methods

        private void btnStartMenu_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageStartMenu());
        }

        #endregion
    }
}