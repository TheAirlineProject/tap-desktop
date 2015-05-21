using System.ComponentModel.Composition;
using TheAirline.ViewModels.Game;

namespace TheAirline.Views.Game
{
    /// <summary>
    ///     Interaction logic for PageStartMenu.xaml
    /// </summary>
    [Export("PageStartMenu")]
    public partial class PageStartMenu
    {
        #region Constructors and Destructors

        public PageStartMenu()
        {
            InitializeComponent();
        }

        #endregion

        [Import]
        public PageStartMenuViewModel ViewModel
        {
            get { return DataContext as PageStartMenuViewModel; }
            set { DataContext = value; }
        }
    }
}