namespace TheAirline.GUIModel.PagesModel.PilotsPageModel
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;

    using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
    using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.AirlinerModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.PilotModel;

    /// <summary>
    ///     Interaction logic for PagePilots.xaml
    /// </summary>
    public partial class PagePilots : Page
    {
        #region Constructors and Destructors

        public PagePilots()
        {
            this.AllPilots = new ObservableCollection<Pilot>();
            Pilots.GetUnassignedPilots().ForEach(p => this.AllPilots.Add(p));

            this.Loaded += this.PagePilots_Loaded;

            this.InitializeComponent();
        }

        #endregion

        #region Public Properties

        public ObservableCollection<Pilot> AllPilots { get; set; }

        #endregion

        #region Methods

        private void PagePilots_Loaded(object sender, RoutedEventArgs e)
        {
            var tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

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
                GameObject.GetInstance().HumanAirline.addPilot(pilot);

                this.AllPilots.Remove(pilot);

                IEnumerable<FleetAirliner> fleetMissingPilots =
                    GameObject.GetInstance().HumanAirline.Fleet.Where(f => f.Pilots.Count < f.Airliner.Type.CockpitCrew);

                if (fleetMissingPilots.Count() > 0)
                {
                    var cbAirliners = new ComboBox();
                    cbAirliners.SetResourceReference(StyleProperty, "ComboBoxTransparentStyle");
                    cbAirliners.Width = 200;
                    cbAirliners.HorizontalAlignment = HorizontalAlignment.Left;
                    cbAirliners.DisplayMemberPath = "Name";
                    cbAirliners.SelectedValuePath = "Name";

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

                        airliner.addPilot(pilot);
                    }
                }
            }
        }

        private void lnkPilot_Click(object sender, RoutedEventArgs e)
        {
            var pilot = (Pilot)((Hyperlink)sender).Tag;

            var tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                TabItem matchingItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Pilot").FirstOrDefault();

                //matchingItem.IsSelected = true;
                matchingItem.Header = pilot.Profile.Name;
                matchingItem.Visibility = Visibility.Visible;

                tab_main.SelectedItem = matchingItem;
            }

            var frmContent = UIHelpers.FindChild<Frame>(this.Tag as Page, "frmContent");

            if (frmContent != null)
            {
                frmContent.Navigate(new PageShowPilot(pilot) { Tag = this.Tag });
            }
        }

        #endregion
    }
}