using System.ComponentModel.Composition;

namespace TheAirline.Views.Game
{
    /// <summary>
    /// Interaction logic for PageHeader.xaml
    /// </summary>
    [Export("PageHeader")]
    public sealed partial class PageHeader
    {
        public PageHeader()
        {
            InitializeComponent();
        }
    }
}
