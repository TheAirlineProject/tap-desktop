using System.ComponentModel.Composition;
using TheAirline.ViewModels.Game;

namespace TheAirline.Views.Game
{
    /// <summary>
    ///     Interaction logic for PageSettings.xaml
    /// </summary>
    [Export("PageSettings")]
    public partial class PageSettings
    {
        #region Constructors and Destructors

        public PageSettings()
        {
            InitializeComponent();
        }

        #endregion

        [Import]
        public PageSettingsViewModel ViewModel
        {
            get { return DataContext as PageSettingsViewModel; }
            set { DataContext = value; }
        }
    }
}