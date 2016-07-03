using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Models.Airliners;
using TheAirline.Models.General;
using TheAirline.Models.Pilots;

namespace TheAirline.GUIModel.PagesModel.PilotsPageModel
{
    /// <summary>
    ///     Interaction logic for PagePilots.xaml
    /// </summary>
    public partial class PagePilots : Page
    {
        #region Constructors and Destructors

        public PagePilots()
        {
            AllPilots = new ObservableCollection<Pilot>();
            Pilots.GetUnassignedPilots().ForEach(p => AllPilots.Add(p));

            Loaded += PagePilots_Loaded;

            InitializeComponent();
        }

        #endregion

        #region Public Properties

        public ObservableCollection<Pilot> AllPilots { get; set; }

        #endregion

        #region Methods

        private void PagePilots_Loaded(object sender, RoutedEventArgs e)
        {
            var tab_main = UIHelpers.FindChild<TabControl>(Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                TabItem matchingItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Flightschool").FirstOrDefault();

                matchingItem.Visibility = Visibility.Collapsed;

                matchingItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Pilot").FirstOrDefault();

                matchingItem.Visibility = Visibility.Collapsed;
            }
        }

        private void btnHire_Click(object sender, RoutedEventArgs e)
        {
            var pilot = (Pilot)((Button)sender).Tag;

            WPFMessageBoxResult result = WPFMessageBox.Show(
                Translator.GetInstance().GetString("MessageBox", "2801"),
                Translator.GetInstance().GetString("MessageBox", "2801", "message"),
                WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                GameObject.GetInstance().HumanAirline.AddPilot(pilot);

                AllPilots.Remove(pilot);

                IEnumerable<FleetAirliner> fleetMissingPilots =
                    GameObject.GetInstance().HumanAirline.Fleet.Where(f => f.Pilots.Count < f.Airliner.Type.CockpitCrew && pilot.Aircrafts.Exists(a=>f.Airliner.Type.AirlinerFamily == a));
                
                if (fleetMissingPilots.Count() > 0)
                {
                    var cbAirliners = new ComboBox();
                    cbAirliners.SetResourceReference(StyleProperty, "ComboBoxTransparentStyle");
                    cbAirliners.Width = 200;
                    cbAirliners.HorizontalAlignment = HorizontalAlignment.Left;
                    cbAirliners.ItemTemplate = Resources["pilotAirlinerItem"] as DataTemplate;
                  
                    foreach (FleetAirliner airliner in fleetMissingPilots)
                    {
                        cbAirliners.Items.Add(airliner);
                    }

                    cbAirliners.SelectedIndex = 0;

                    if (PopUpSingleElement.ShowPopUp(
                        Translator.GetInstance().GetString("PagePilots", "1010"),
                        cbAirliners) == PopUpSingleElement.ButtonSelected.OK && cbAirliners.SelectedItem != null)
                    {
                        var airliner = (FleetAirliner)cbAirliners.SelectedItem;

                        airliner.AddPilot(pilot);
                    }
                }
            }
        }

        private void lnkPilot_Click(object sender, RoutedEventArgs e)
        {
            var pilot = (Pilot)((Hyperlink)sender).Tag;

            var tab_main = UIHelpers.FindChild<TabControl>(Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                TabItem matchingItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Pilot").FirstOrDefault();

                //matchingItem.IsSelected = true;
                matchingItem.Header = pilot.Profile.Name;
                matchingItem.Visibility = Visibility.Visible;

                tab_main.SelectedItem = matchingItem;
            }

            var frmContent = UIHelpers.FindChild<Frame>(Tag as Page, "frmContent");

            if (frmContent != null)
            {
                frmContent.Navigate(new PageShowPilot(pilot) { Tag = Tag });
            }
        }

        #endregion
    }
   
}