namespace TheAirline.GUIModel.PagesModel.AirportsPageModel
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Controls;

    using TheAirline.Model.AirportModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.GeneralModel.StatisticsModel;

    /// <summary>
    ///     Interaction logic for PageAirportsStatistics.xaml
    /// </summary>
    public partial class PageAirportsStatistics : Page
    {
        #region Constructors and Destructors

        public PageAirportsStatistics()
        {
            this.AllAirports = new List<Airport>();

            StatisticsType statType = StatisticsTypes.GetStatisticsType("Passengers");

            IOrderedEnumerable<Airport> airports =
                Airports.GetAllActiveAirports()
                    .OrderByDescending(
                        a => a.Statistics.getTotalValue(GameObject.GetInstance().GameTime.Year, statType));

            foreach (Airport airport in airports.Take(20))
            {
                this.AllAirports.Add(airport);
            }

            this.InitializeComponent();
        }

        #endregion

        #region Public Properties

        public List<Airport> AllAirports { get; set; }

        #endregion
    }
}