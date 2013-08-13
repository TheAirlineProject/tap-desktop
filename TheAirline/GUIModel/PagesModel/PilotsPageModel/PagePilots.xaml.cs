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
using TheAirline.Model.PilotModel;

namespace TheAirline.GUIModel.PagesModel.PilotsPageModel
{
    /// <summary>
    /// Interaction logic for PagePilots.xaml
    /// </summary>
    public partial class PagePilots : Page
    {
        public List<Pilot> AllPilots { get; set; }
        public PagePilots()
        {
            this.AllPilots = Pilots.GetUnassignedPilots();
            this.Loaded += PagePilots_Loaded;

            InitializeComponent();

            
        }

        private void PagePilots_Loaded(object sender, RoutedEventArgs e)
        {
            TabControl tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                var matchingItem =
     tab_main.Items.Cast<TabItem>()
       .Where(item => item.Tag.ToString() == "Flightschool")
       .FirstOrDefault();

                matchingItem.Visibility = System.Windows.Visibility.Collapsed;

                matchingItem =
    tab_main.Items.Cast<TabItem>()
      .Where(item => item.Tag.ToString() == "Pilot")
      .FirstOrDefault();

                matchingItem.Visibility = System.Windows.Visibility.Collapsed;
           }
        }

        private void lnkPilot_Click(object sender, RoutedEventArgs e)
        {
            Pilot pilot = (Pilot)((Hyperlink)sender).Tag;

            TabControl tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                var matchingItem =
     tab_main.Items.Cast<TabItem>()
       .Where(item => item.Tag.ToString() == "Pilot")
       .FirstOrDefault();

                //matchingItem.IsSelected = true;
                matchingItem.Header = pilot.Profile.Name;
                matchingItem.Visibility = System.Windows.Visibility.Visible;

                tab_main.SelectedItem = matchingItem;
            }

            Frame frmContent = UIHelpers.FindChild<Frame>(this.Tag as Page, "frmContent");

            if (frmContent != null)
            {
                frmContent.Navigate(new PageShowPilot(pilot) { Tag = this.Tag });

            }

        }

    }
}
