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
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.PageModel.PageAirportModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Model.AirportModel;

namespace TheAirline.GUIModel.PagesModel.AirportsPageModel
{
    /// <summary>
    /// Interaction logic for PageShowAirports.xaml
    /// </summary>
    public partial class PageShowAirports : Page
    {
        public List<Airport> AllAirports { get; set; }

        public PageShowAirports(List<Airport> airports)
        {
            createPage(airports);
        }
        public PageShowAirports()
        {
            createPage(Airports.GetAllActiveAirports());
        }
        //creates the page
        private void createPage(List<Airport> airports)
        {
            TabControl tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                var matchingItem =
     tab_main.Items.Cast<TabItem>()
       .Where(item => item.Tag.ToString() == "Airports")
       .FirstOrDefault();

                tab_main.SelectedItem = matchingItem;
            }

            this.AllAirports = airports.OrderBy(a=>a.Profile.Name).ToList();

            InitializeComponent();
        }
        private void clName_Click(object sender, RoutedEventArgs e)
        {
            Airport airport = (Airport)((Hyperlink)sender).Tag;

            PageNavigator.NavigateTo(new PageAirport(airport));
        }
    }
}
