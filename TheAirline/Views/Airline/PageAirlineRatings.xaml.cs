using System.Collections.ObjectModel;
using System.Windows.Controls;
using TheAirline.Models.General;
using TheAirline.Models.General.Statistics;
using TheAirline.ViewModels.Airline;

namespace TheAirline.GUIModel.PagesModel.AirlinePageModel
{
    /// <summary>
    ///     Interaction logic for AirlineRatings.xaml
    /// </summary>
    public partial class PageAirlineRatings : Page
    {
        #region Constructors and Destructors

        public PageAirlineRatings(AirlineMVVM airline)
        {
            AirlineRatings = new ObservableCollection<AirlineScoreMVVM>();
            AirlineStatistics = new ObservableCollection<AirlineStatisticsMVVM>();
            Airline = airline;
            DataContext = Airline;

            InitializeComponent();

            AirlineRatings.Add(
                new AirlineScoreMVVM(
                    Translator.GetInstance().GetString("PageAirlineRatings", "1007"),
                    Airline.Airline.Ratings.SecurityRating));
            AirlineRatings.Add(
                new AirlineScoreMVVM(
                    Translator.GetInstance().GetString("PageAirlineRatings", "1008"),
                    Airline.Airline.Ratings.SafetyRating));
            AirlineRatings.Add(
                new AirlineScoreMVVM(
                    Translator.GetInstance().GetString("PageAirlineRatings", "1009"),
                    Airline.Airline.Ratings.EmployeeHappinessRating));
            AirlineRatings.Add(
                new AirlineScoreMVVM(
                    Translator.GetInstance().GetString("PageAirlineRatings", "1010"),
                    Airline.Airline.Ratings.CustomerHappinessRating));
            AirlineRatings.Add(
                new AirlineScoreMVVM(
                    Translator.GetInstance().GetString("PageAirlineRatings", "1011"),
                    Airline.Airline.Ratings.MaintenanceRating));

          AirlineStatistics.Add(
                new AirlineStatisticsMVVM(Airline.Airline, StatisticsTypes.GetStatisticsType("Passengers")));
            AirlineStatistics.Add(
                new AirlineStatisticsMVVM(Airline.Airline, StatisticsTypes.GetStatisticsType("Passengers%")));
            AirlineStatistics.Add(
                new AirlineStatisticsMVVM(Airline.Airline, StatisticsTypes.GetStatisticsType("Arrivals")));
        }

        #endregion

        #region Public Properties

        public AirlineMVVM Airline { get; set; }

        public ObservableCollection<AirlineScoreMVVM> AirlineRatings { get; set; }

       public ObservableCollection<AirlineStatisticsMVVM> AirlineStatistics { get; set; }

        #endregion
    }
}