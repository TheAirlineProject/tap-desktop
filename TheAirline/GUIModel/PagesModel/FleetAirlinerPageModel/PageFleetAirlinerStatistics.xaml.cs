namespace TheAirline.GUIModel.PagesModel.FleetAirlinerPageModel
{
    using System.Collections.ObjectModel;
    using System.Windows.Controls;

    using TheAirline.Model.GeneralModel.StatisticsModel;

    /// <summary>
    ///     Interaction logic for FleetAirlinerStatistics.xaml
    /// </summary>
    public partial class PageFleetAirlinerStatistics : Page
    {
        #region Constructors and Destructors

        public PageFleetAirlinerStatistics(FleetAirlinerMVVM airliner)
        {
            this.Airliner = airliner;
            this.DataContext = this.Airliner;

            this.AirlinerStatistics = new ObservableCollection<FleetAirlinerStatisticsMVVM>();

            this.InitializeComponent();

            this.AirlinerStatistics.Add(
                new FleetAirlinerStatisticsMVVM(this.Airliner.Airliner, StatisticsTypes.GetStatisticsType("Passengers")));
            this.AirlinerStatistics.Add(
                new FleetAirlinerStatisticsMVVM(
                    this.Airliner.Airliner,
                    StatisticsTypes.GetStatisticsType("Passengers%")));
            this.AirlinerStatistics.Add(
                new FleetAirlinerStatisticsMVVM(this.Airliner.Airliner, StatisticsTypes.GetStatisticsType("Arrivals")));
        }

        #endregion

        #region Public Properties

        public FleetAirlinerMVVM Airliner { get; set; }

        public ObservableCollection<FleetAirlinerStatisticsMVVM> AirlinerStatistics { get; set; }

        #endregion
    }
}