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
using TheAirline.Model.AirlinerModel;

namespace TheAirline.GUIModel.PagesModel.FleetAirlinerPageModel
{
    /// <summary>
    /// Interaction logic for PageFleetAirliner.xaml
    /// </summary>
    public partial class PageFleetAirliner : Page
    {
        public FleetAirlinerMVVM Airliner { get; set; }
        public PageFleetAirliner(FleetAirliner airliner)
        {
            this.Airliner = new FleetAirlinerMVVM(airliner);
            this.Loaded += PageFleetAirliner_Loaded;
            
            InitializeComponent();


        }

        private void PageFleetAirliner_Loaded(object sender, RoutedEventArgs e)
        {
            Frame frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            frmContent.Navigate(new PageFleetAirlinerInformation(this.Airliner) { Tag = this });

            TabControl tab_main = UIHelpers.FindChild<TabControl>(this, "tabMenu");

            if (tab_main != null)
            {
                var infoItem =
     tab_main.Items.Cast<TabItem>()
       .Where(item => item.Tag.ToString() == "Information")
       .FirstOrDefault();

                //matchingItem.IsSelected = true;
                infoItem.Header = this.Airliner.Airliner.Name;

                var maintenanceItem =
    tab_main.Items.Cast<TabItem>()
      .Where(item => item.Tag.ToString() == "Maintenance")
      .FirstOrDefault();

                if (!this.Airliner.Airliner.Airliner.Airline.IsHuman)
                    maintenanceItem.Visibility = System.Windows.Visibility.Collapsed;

           }

        }
        private void tcMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl control = (TabControl)sender;

            string selection = ((TabItem)control.SelectedItem).Tag.ToString();

            Frame frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            if (selection == "Information" && frmContent != null)
                frmContent.Navigate(new PageFleetAirlinerInformation(this.Airliner) { Tag = this });

            if (selection == "Maintenance" && frmContent != null)
                frmContent.Navigate(new PageFleetAirlinerInsurances(this.Airliner) { Tag = this });

            if (selection == "Statistics" && frmContent != null)
                frmContent.Navigate(new PageFleetAirlinerStatistics(this.Airliner) { Tag = this });


        }
    }
}
