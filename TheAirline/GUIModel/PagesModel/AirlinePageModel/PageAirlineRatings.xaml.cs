namespace TheAirline.GUIModel.PagesModel.AirlinePageModel
{
    using System.Collections.ObjectModel;
    using System.Windows.Controls;

    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.GeneralModel.Helpers;
    using TheAirline.Model.GeneralModel.StatisticsModel;

    /// <summary>
    ///     Interaction logic for AirlineRatings.xaml
    /// </summary>
    public partial class PageAirlineRatings : Page
    {
        #region Constructors and Destructors

        public PageAirlineRatings(AirlineMVVM airline)
        {
            this.AirlineRatings = new ObservableCollection<AirlineScoreMVVM>();
            this.AirlineStatistics = new ObservableCollection<AirlineStatisticsMVVM>();
            this.Airline = airline;
            this.DataContext = this.Airline;

            this.InitializeComponent();

            this.AirlineRatings.Add(
                new AirlineScoreMVVM(
                    Translator.GetInstance().GetString("PageAirlineRatings", "1007"),
                    this.Airline.Airline.Ratings.SecurityRating));
            this.AirlineRatings.Add(
                new AirlineScoreMVVM(
                    Translator.GetInstance().GetString("PageAirlineRatings", "1008"),
                    this.Airline.Airline.Ratings.SafetyRating));
            this.AirlineRatings.Add(
                new AirlineScoreMVVM(
                    Translator.GetInstance().GetString("PageAirlineRatings", "1009"),
                    this.Airline.Airline.Ratings.EmployeeHappinessRating));
            this.AirlineRatings.Add(
                new AirlineScoreMVVM(
                    Translator.GetInstance().GetString("PageAirlineRatings", "1010"),
                    this.Airline.Airline.Ratings.CustomerHappinessRating));
            this.AirlineRatings.Add(
                new AirlineScoreMVVM(
                    Translator.GetInstance().GetString("PageAirlineRatings", "1011"),
                    this.Airline.Airline.Ratings.MaintenanceRating));

          this.AirlineStatistics.Add(
                new AirlineStatisticsMVVM(this.Airline.Airline, StatisticsTypes.GetStatisticsType("Passengers")));
            this.AirlineStatistics.Add(
                new AirlineStatisticsMVVM(this.Airline.Airline, StatisticsTypes.GetStatisticsType("Passengers%")));
            this.AirlineStatistics.Add(
                new AirlineStatisticsMVVM(this.Airline.Airline, StatisticsTypes.GetStatisticsType("Arrivals")));
        }

        #endregion

        #region Public Properties

        public AirlineMVVM Airline { get; set; }

        public ObservableCollection<AirlineScoreMVVM> AirlineRatings { get; set; }

       public ObservableCollection<AirlineStatisticsMVVM> AirlineStatistics { get; set; }

        #endregion
    }
}