using System.ComponentModel.Composition;
using TheAirline.General.ViewModels;

namespace TheAirline.Views.Game
{
    /// <summary>
    ///     Interaction logic for PageCredits.xaml
    /// </summary>
    [Export("PageCredits")]
    public partial class PageCredits
    {
        public PageCredits()
        {
            InitializeComponent();
        }

        [Import]
        public PageCreditsViewModel ViewModel
        {
            get { return DataContext as PageCreditsViewModel; }
            set { DataContext = value; }
        }
    }
}