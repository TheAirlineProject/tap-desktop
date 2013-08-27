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
using TheAirline.Model.GeneralModel.StatisticsModel;

namespace TheAirline.GUIModel.PagesModel.FleetAirlinerPageModel
{
    /// <summary>
    /// Interaction logic for FleetAirlinerStatistics.xaml
    /// </summary>
    public partial class PageFleetAirlinerStatistics : Page
    {
        public ObservableCollection<FleetAirlinerStatisticsMVVM> AirlinerStatistics { get; set; }
        public FleetAirlinerMVVM Airliner { get; set; }
        public PageFleetAirlinerStatistics(FleetAirlinerMVVM airliner)
        {
            this.Airliner = airliner;
            this.DataContext = this.Airliner;

            this.AirlinerStatistics = new ObservableCollection<FleetAirlinerStatisticsMVVM>();

            InitializeComponent();

            this.AirlinerStatistics.Add(new FleetAirlinerStatisticsMVVM(this.Airliner.Airliner, StatisticsTypes.GetStatisticsType("Passengers")));
            this.AirlinerStatistics.Add(new FleetAirlinerStatisticsMVVM(this.Airliner.Airliner, StatisticsTypes.GetStatisticsType("Passengers%")));
            this.AirlinerStatistics.Add(new FleetAirlinerStatisticsMVVM(this.Airliner.Airliner, StatisticsTypes.GetStatisticsType("Arrivals")));
      
        }
    }
}
