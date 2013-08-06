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
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.StatisticsModel;

namespace TheAirline.GUIModel.PagesModel.AlliancesPageModel
{
    /// <summary>
    /// Interaction logic for PageAlliances.xaml
    /// </summary>
    public partial class PageAlliances : Page
    {
        public List<Alliance> HumanAlliances { get; set; }
        public List<Alliance> LargestAlliances { get; set; }
        public PageAlliances()
        {
            this.HumanAlliances = GameObject.GetInstance().HumanAirline.Alliances;

            var alliances = Alliances.GetAlliances().OrderBy(a=>a.Members.Sum(m => m.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year - 1, StatisticsTypes.GetStatisticsType("Passengers")))).ToList();

            this.LargestAlliances = alliances.Take(Math.Min(5,alliances.Count)).ToList();
            this.Loaded += PageAlliances_Loaded;
           
            InitializeComponent();

            
        }

        private void PageAlliances_Loaded(object sender, RoutedEventArgs e)
        {
            Frame frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            frmContent.Navigate(new PageShowAlliances() { Tag = this });
        }
        private void tcMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl control = (TabControl)sender;

            string selection = ((TabItem)control.SelectedItem).Tag.ToString();

            Frame frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            if (selection == "Alliances" && frmContent != null)
                frmContent.Navigate(new PageShowAlliances() { Tag = this });

            if (selection == "Create" && frmContent != null)
                frmContent.Navigate(new PageCreateAlliance() { Tag = this }); ;

        }
    }
}
