using System.ComponentModel.Composition;
using TheAirline.ViewModels.Game;

namespace TheAirline.Views.Game
{
    /// <summary>
    ///     Interaction logic for PageSelectLanguage.xaml
    /// </summary>
    [Export("PageSelectLanguage")]
    public partial class PageSelectLanguage
    {
        public PageSelectLanguage()
        {
            InitializeComponent();
        }

        [Import]
        public PageSelectLanguageViewModel ViewModel
        {
            get { return DataContext as PageSelectLanguageViewModel; }
            set { DataContext = value; }
        }
    }
}