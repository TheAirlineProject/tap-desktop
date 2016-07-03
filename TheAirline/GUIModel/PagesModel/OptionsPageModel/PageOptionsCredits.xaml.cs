namespace TheAirline.GUIModel.PagesModel.OptionsPageModel
{
    using System.Linq;
    using System.Reflection;
    using System.Windows.Controls;

    using TheAirline.GUIModel.PagesModel.GamePageModel;

    /// <summary>
    ///     Interaction logic for PageOptionsCredits.xaml
    /// </summary>
    public partial class PageOptionsCredits : Page
    {
        #region Constructors and Destructors

        public PageOptionsCredits()
        {
            this.InitializeComponent();

            ApplicationVersionTextBlock.Text = new GameVersionMVVM().CurrentVersion;

        }

        #endregion
    }
}