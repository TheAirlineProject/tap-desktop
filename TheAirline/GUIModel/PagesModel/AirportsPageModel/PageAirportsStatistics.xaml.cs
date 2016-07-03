using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using TheAirline.Models.Airports;
using TheAirline.Models.General;
using TheAirline.Models.General.Statistics;

namespace TheAirline.GUIModel.PagesModel.AirportsPageModel
{
    /// <summary>
    ///     Interaction logic for PageAirportsStatistics.xaml
    /// </summary>
    public partial class PageAirportsStatistics : Page
    {
        #region Constructors and Destructors

        public PageAirportsStatistics()
        {
            AllAirports = new List<Airport>();

            StatisticsType statType = StatisticsTypes.GetStatisticsType("Passengers");

            IOrderedEnumerable<Airport> airports =
                Airports.GetAllActiveAirports()
                    .OrderByDescending(
                        a => a.Statistics.GetTotalValue(GameObject.GetInstance().GameTime.Year, statType));

            foreach (Airport airport in airports.Take(20))
            {
                AllAirports.Add(airport);
            }

            InitializeComponent();
        }

        #endregion

        #region Public Properties

        public List<Airport> AllAirports { get; set; }

        #endregion
    }
}