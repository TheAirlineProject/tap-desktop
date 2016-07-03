namespace TheAirline.GUIModel.PagesModel.AirportPageModel
{
    using System.Windows.Controls;

    /// <summary>
    ///     Interaction logic for PageAirportTraffic.xaml
    /// </summary>
    public partial class PageAirportTraffic : Page
    {
        #region Constructors and Destructors

        public PageAirportTraffic(AirportMVVM airport)
        {
            this.Airport = airport;
            this.DataContext = this.Airport;

            this.InitializeComponent();

            /*
            CollectionView viewTraffic = (CollectionView)CollectionViewSource.GetDefaultView(lvTraffic.ItemsSource);
            viewTraffic.GroupDescriptions.Clear();

            viewTraffic.GroupDescriptions.Add(new PropertyGroupDescription("Type"));
            */
        }

        #endregion

        #region Public Properties

        public AirportMVVM Airport { get; set; }

        #endregion
    }
}