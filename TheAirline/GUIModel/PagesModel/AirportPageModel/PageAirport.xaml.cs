using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Helpers;
using TheAirline.Models.Airports;
using TheAirline.Models.General;

namespace TheAirline.GUIModel.PagesModel.AirportPageModel
{
    /// <summary>
    ///     Interaction logic for PageAirport.xaml
    /// </summary>
    public partial class PageAirport : Page
    {
        #region Fields

        private readonly AirportMVVM Airport;

        #endregion

        #region Constructors and Destructors

        public PageAirport(Airport airport)
        {
            Loaded += PageAirport_Loaded;
            Airport = new AirportMVVM(airport);

            Distances = new List<AirportDistanceMVVM>();

            foreach (
                Airport destination in
                    GameObject.GetInstance().HumanAirline.Airports.Where(a => a != Airport.Airport))
            {
                Distances.Add(
                    new AirportDistanceMVVM(destination, MathHelpers.GetDistance(destination, Airport.Airport)));
            }

            InitializeComponent();
        }

        #endregion

        #region Public Properties

        public List<AirportDistanceMVVM> Distances { get; set; }

        #endregion

        #region Methods

        private void PageAirport_Loaded(object sender, RoutedEventArgs e)
        {
            var frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            frmContent.Navigate(new PageAirportInfo(Airport) { Tag = this });

            var tab_main = UIHelpers.FindChild<TabControl>(this, "tcMenu");

            if (tab_main != null)
            {
                TabItem matchingItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Overview").FirstOrDefault();

                //matchingItem.IsSelected = true;
                matchingItem.Header = Airport.Airport.Profile.Name;
                matchingItem.Visibility = Visibility.Visible;

                tab_main.SelectedItem = matchingItem;
            }
        }

        private void tcMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var control = (TabControl)sender;

            string selection = ((TabItem)control.SelectedItem).Tag.ToString();

            var frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            if (selection == "Overview" && frmContent != null)
            {
                frmContent.Navigate(new PageAirportInfo(Airport) { Tag = this });
            }

            if (selection == "Facilities" && frmContent != null)
            {
                frmContent.Navigate(new PageAirportFacilities(Airport) { Tag = this });
            }

            if (selection == "Traffic" && frmContent != null)
            {
                frmContent.Navigate(new PageAirportTraffic(Airport) { Tag = this });
            }

            if (selection == "Gates" && frmContent != null)
            {
                frmContent.Navigate(new PageAirportGateSchedule(Airport) { Tag = this });
            }

            if (selection == "Demand" && frmContent != null)
            {
                frmContent.Navigate(new PageAirportDemand(Airport) { Tag = this });
            }
        }

        #endregion
    }
}