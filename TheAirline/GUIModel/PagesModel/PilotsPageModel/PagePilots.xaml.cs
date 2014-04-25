using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.PilotModel;

namespace TheAirline.GUIModel.PagesModel.PilotsPageModel
{
    /// <summary>
    /// Interaction logic for PagePilots.xaml
    /// </summary>
    public partial class PagePilots : Page
    {
        public ObservableCollection<Pilot> AllPilots { get; set; }
        public PagePilots()
        {
            this.AllPilots = new ObservableCollection<Pilot>();
            Pilots.GetUnassignedPilots().ForEach(p=>this.AllPilots.Add(p));

            this.Loaded += PagePilots_Loaded;

            InitializeComponent();

            
        }
        private void btnHire_Click(object sender, RoutedEventArgs e)
        {
            Pilot pilot = (Pilot)((Button)sender).Tag;

            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2801"), Translator.GetInstance().GetString("MessageBox", "2801", "message"), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                GameObject.GetInstance().HumanAirline.addPilot(pilot);

                this.AllPilots.Remove(pilot);

                var fleetMissingPilots = GameObject.GetInstance().HumanAirline.Fleet.Where(f=>f.Pilots.Count < f.Airliner.Type.CockpitCrew);

                if (fleetMissingPilots.Count() > 0)
                {
                    ComboBox cbAirliners = new ComboBox();
                    cbAirliners.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
                    cbAirliners.Width = 200;
                    cbAirliners.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    cbAirliners.DisplayMemberPath = "Name";
                    cbAirliners.SelectedValuePath = "Name";

                    foreach (FleetAirliner airliner in fleetMissingPilots)
                        cbAirliners.Items.Add(airliner);

                    cbAirliners.SelectedIndex = 0;
                    
                    if (PopUpSingleElement.ShowPopUp(Translator.GetInstance().GetString("PagePilots", "1010"), cbAirliners) == PopUpSingleElement.ButtonSelected.OK && cbAirliners.SelectedItem != null)
                    {
                        FleetAirliner airliner = (FleetAirliner)cbAirliners.SelectedItem;

                        airliner.addPilot(pilot);
                    }
                }
            }
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
