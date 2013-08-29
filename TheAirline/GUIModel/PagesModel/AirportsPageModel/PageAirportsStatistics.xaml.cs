using System;
using System.Collections.Generic;
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
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.StatisticsModel;

namespace TheAirline.GUIModel.PagesModel.AirportsPageModel
{
    /// <summary>
    /// Interaction logic for PageAirportsStatistics.xaml
    /// </summary>
    public partial class PageAirportsStatistics : Page
    {
        public List<Airport> AllAirports { get; set; }
        public PageAirportsStatistics()
        {
            this.AllAirports = new List<Airport>();

            StatisticsType statType = StatisticsTypes.GetStatisticsType("Passengers");
          
            var airports = Airports.GetAllActiveAirports().OrderByDescending(a => a.Statistics.getTotalValue(GameObject.GetInstance().GameTime.Year, statType));
           
            foreach (Airport airport in airports.Take(20))
            {
                this.AllAirports.Add(airport);
            }
   
            InitializeComponent();

         }
    }
   
}
