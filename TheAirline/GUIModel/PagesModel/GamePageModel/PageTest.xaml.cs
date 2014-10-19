using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    /// <summary>
    /// Interaction logic for PageTest.xaml
    /// </summary>
    public partial class PageTest : Page
    {
        public List<Airport> AllAirports { get; set; }
        private Stopwatch sw;
        public PageTest()
        {
            this.Loaded += PageTest_Loaded;
            sw = new Stopwatch();
            sw.Start();

            this.AllAirports = Airports.GetAllAirports();

            InitializeComponent();

         
        }

        private void PageTest_Loaded(object sender, RoutedEventArgs e)
        {
            ListView lv = UIHelpers.FindChild<ListView>(this, "AirportsList");

            double h = lv.Height;
            //lv.ItemsSource = this.AllAirports;

            sw.Stop();
            long time = sw.ElapsedMilliseconds;
        }
    }
}
