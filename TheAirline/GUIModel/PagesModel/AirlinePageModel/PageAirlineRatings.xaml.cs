using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.Model.GeneralModel.StatisticsModel;

namespace TheAirline.GUIModel.PagesModel.AirlinePageModel
{
    /// <summary>
    /// Interaction logic for AirlineRatings.xaml
    /// </summary>
    public partial class PageAirlineRatings : Page
    {
        public AirlineMVVM Airline { get; set; }
        public ObservableCollection<AirlineScoreMVVM> AirlineRatings { get; set; }
        public ObservableCollection<AirlineScoreMVVM> AirlineScores { get; set; }
        public ObservableCollection<AirlineStatisticsMVVM> AirlineStatistics { get; set; }
        public PageAirlineRatings(AirlineMVVM airline)
        {
            this.AirlineRatings = new ObservableCollection<AirlineScoreMVVM>();
            this.AirlineScores = new ObservableCollection<AirlineScoreMVVM>();
            this.AirlineStatistics = new ObservableCollection<AirlineStatisticsMVVM>();
            this.Airline = airline;
            this.DataContext = this.Airline;

            InitializeComponent();

            this.AirlineRatings.Add(new AirlineScoreMVVM("Security Rating", this.Airline.Airline.Ratings.SecurityRating));
            this.AirlineRatings.Add(new AirlineScoreMVVM("Safety Rating", this.Airline.Airline.Ratings.SafetyRating));
            this.AirlineRatings.Add(new AirlineScoreMVVM("Employee Happiness Rating", this.Airline.Airline.Ratings.EmployeeHappinessRating));
            this.AirlineRatings.Add(new AirlineScoreMVVM("Customer Happiness Rating", this.Airline.Airline.Ratings.CustomerHappinessRating));
            this.AirlineRatings.Add(new AirlineScoreMVVM("Maintenance Rating", this.Airline.Airline.Ratings.MaintenanceRating));

            this.AirlineScores.Add(new AirlineScoreMVVM("Overall Score", this.Airline.Airline.OverallScore));
            this.AirlineScores.Add(new AirlineScoreMVVM("Reputation", this.Airline.Airline.Reputation));
            this.AirlineScores.Add(new AirlineScoreMVVM("Flight On-time Percent", (int)StatisticsHelpers.GetOnTimePercent(this.Airline.Airline)));
            this.AirlineScores.Add(new AirlineScoreMVVM("Average Fill Degree", (int)StatisticsHelpers.GetAirlineFillAverage(this.Airline.Airline)));

            this.AirlineStatistics.Add(new AirlineStatisticsMVVM(this.Airline.Airline, StatisticsTypes.GetStatisticsType("Passengers")));
            this.AirlineStatistics.Add(new AirlineStatisticsMVVM(this.Airline.Airline, StatisticsTypes.GetStatisticsType("Passengers%")));
            this.AirlineStatistics.Add(new AirlineStatisticsMVVM(this.Airline.Airline, StatisticsTypes.GetStatisticsType("Arrivals")));
               

        }
    }
}
