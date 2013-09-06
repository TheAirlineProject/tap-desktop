using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    /// <summary>
    /// Interaction logic for PageFlights.xaml
    /// </summary>
    public partial class PageFlights : Page
    {
        public List<RouteTimeTableEntry> AllFlights { get; set; }
        public PageFlights()
        {
            this.AllFlights = Airlines.GetAllAirlines().SelectMany(a => a.Routes.SelectMany(r => r.TimeTable.Entries)).ToList();
            InitializeComponent();

            this.Loaded += PageFlights_Loaded;
        }

        private void PageFlights_Loaded(object sender, RoutedEventArgs e)
        {
            TabControl tab_main = UIHelpers.FindChild<TabControl>(this, "tabMenu");

            if (tab_main != null)
            {
                var matchingItem =
     tab_main.Items.Cast<TabItem>()
       .Where(item => item.Tag.ToString() == GameObject.GetInstance().GameTime.DayOfWeek.ToString())
       .FirstOrDefault();

               
                tab_main.SelectedItem = matchingItem;
            }
        }
        private void tcMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl control = (TabControl)sender;

            string selection = ((TabItem)control.SelectedItem).Tag.ToString();

            DayOfWeek day = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), selection, true);

            ListView lvFlights = UIHelpers.FindChild<ListView>(this, "lvFlights");

            if (lvFlights != null)
            {
                var source = lvFlights.Items as ICollectionView;
                source.Filter = delegate(object item)
                {
                    var i = item as RouteTimeTableEntry;
                    return i.Day == day;

                };
            }


        }
    }
}
