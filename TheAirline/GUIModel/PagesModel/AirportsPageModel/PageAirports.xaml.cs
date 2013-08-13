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
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GUIModel.PagesModel.AirportsPageModel
{
    /// <summary>
    /// Interaction logic for PageAirports.xaml
    /// </summary>
    public partial class PageAirports : Page
    {
        public List<Airport> HumanAirports { get; set; }
        public List<Airport> HumanHubs { get; set; }
        public PageAirports()
        {
            this.HumanAirports = GameObject.GetInstance().HumanAirline.Airports.OrderBy(a=>a.Profile.Pax).ToList().GetRange(0,Math.Min(GameObject.GetInstance().HumanAirline.Airports.Count,5));
            this.HumanHubs = GameObject.GetInstance().HumanAirline.getHubs();

            this.Loaded += PageAirports_Loaded;
            
            InitializeComponent();
        }

        private void PageAirports_Loaded(object sender, RoutedEventArgs e)
        {
            Frame frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            frmContent.Navigate(new PageShowAirports() { Tag = this });
        }
        private void tcMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl control = (TabControl)sender;

            string selection = ((TabItem)control.SelectedItem).Tag.ToString();

            Frame frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");
            
            if (selection == "Search" && frmContent != null)
                frmContent.Navigate(new PageSearchAirports() { Tag = this });
           
            if (selection == "Airports" && frmContent != null)
                frmContent.Navigate(new PageShowAirports() { Tag = this });

            if (selection == "Statistics" && frmContent != null)
                frmContent.Navigate(new PageAirportsStatistics() { Tag = this });
           
        }
    }
}
