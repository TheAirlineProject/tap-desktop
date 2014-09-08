namespace TheAirline.GUIModel.PagesModel.AirportPageModel
{
    using System.Windows.Controls;

    /// <summary>
    ///     Interaction logic for PageAirportGateSchedule.xaml
    /// </summary>
    public partial class PageAirportGateSchedule : Page
    {
        #region Constructors and Destructors

        public PageAirportGateSchedule(AirportMVVM airport)
        {
            this.Airport = airport;

            this.DataContext = this.Airport;

            this.InitializeComponent();
        }

        #endregion

        #region Public Properties

        public AirportMVVM Airport { get; set; }

        #endregion
    }
}