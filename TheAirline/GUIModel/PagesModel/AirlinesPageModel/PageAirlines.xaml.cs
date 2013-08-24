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

namespace TheAirline.GUIModel.PagesModel.AirlinesPageModel
{
    /// <summary>
    /// Interaction logic for PageAirlines.xaml
    /// </summary>
    public partial class PageAirlines : Page
    {
        public List<Airline> MostGates { get; set; }
        public List<Airline> MostRoutes { get; set; }
        public List<Airline> LargestFleet { get; set; }
    
        public PageAirlines()
        {
            var airlines = Airlines.GetAllAirlines();
            this.MostGates = airlines.OrderByDescending(a => a.Airports.Sum(c => c.AirlineContracts.Where(ac => ac.Airline == a).Sum(ac => ac.NumberOfGates))).Take(Math.Min(5,airlines.Count)).ToList();
            this.MostRoutes = airlines.OrderByDescending(a => a.Routes.Count).Take(Math.Min(airlines.Count,5)).ToList();
            this.LargestFleet = airlines.OrderByDescending(a => a.Fleet.Count).Take(Math.Min(airlines.Count, 5)).ToList();

            this.Loaded += PageAirlines_Loaded;
            InitializeComponent();
        }

        private void PageAirlines_Loaded(object sender, RoutedEventArgs e)
        {
            Frame frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");
            frmContent.Navigate(new PageAirlinesStatistics() { Tag = this });
        }
        private void tcMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl control = (TabControl)sender;

            string selection = ((TabItem)control.SelectedItem).Tag.ToString();

            Frame frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            if (selection == "Stastics" && frmContent != null)
                frmContent.Navigate(new PageAirlinesStatistics() { Tag = this });

          
        }
    }
}
