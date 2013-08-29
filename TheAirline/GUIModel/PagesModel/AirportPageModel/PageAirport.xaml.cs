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

namespace TheAirline.GUIModel.PagesModel.AirportPageModel
{
    /// <summary>
    /// Interaction logic for PageAirport.xaml
    /// </summary>
    public partial class PageAirport : Page
    {
        private AirportMVVM Airport;
        public List<AirportDistanceMVVM> Distances { get; set; }
        public PageAirport(Airport airport)
        {
            this.Loaded += PageAirport_Loaded;
            this.Airport = new AirportMVVM(airport);

            this.Distances = new List<AirportDistanceMVVM>();

            foreach (Airport destination in GameObject.GetInstance().HumanAirline.Airports.Where(a => a != this.Airport.Airport))
                this.Distances.Add(new AirportDistanceMVVM(destination, MathHelpers.GetDistance(destination, this.Airport.Airport)));

            InitializeComponent();
        }

       
        private void PageAirport_Loaded(object sender, RoutedEventArgs e)
        {
            Frame frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            frmContent.Navigate(new PageAirportInfo(this.Airport) { Tag = this });

            TabControl tab_main = UIHelpers.FindChild<TabControl>(this, "tcMenu");

            if (tab_main != null)
            {
                var matchingItem =
     tab_main.Items.Cast<TabItem>()
       .Where(item => item.Tag.ToString() == "Overview")
       .FirstOrDefault();

                //matchingItem.IsSelected = true;
                matchingItem.Header = this.Airport.Airport.Profile.Name;
                matchingItem.Visibility = System.Windows.Visibility.Visible;

                tab_main.SelectedItem = matchingItem;
            }

        }
        private void tcMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl control = (TabControl)sender;

            string selection = ((TabItem)control.SelectedItem).Tag.ToString();

            Frame frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            if (selection == "Overview" && frmContent != null)
                frmContent.Navigate(new PageAirportInfo(this.Airport) { Tag = this });

            if (selection == "Facilities" && frmContent != null)
                frmContent.Navigate(new PageAirportFacilities(this.Airport) { Tag = this });

            if (selection == "Traffic" && frmContent != null)
                frmContent.Navigate(new PageAirportTraffic(this.Airport) { Tag = this });

           
        }
    }
}
