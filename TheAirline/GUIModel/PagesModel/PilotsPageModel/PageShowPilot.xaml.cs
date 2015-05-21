using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Helpers;
using TheAirline.Models.General;
using TheAirline.Models.Pilots;

namespace TheAirline.GUIModel.PagesModel.PilotsPageModel
{
    /// <summary>
    ///     Interaction logic for PageShowPilot.xaml
    /// </summary>
    public partial class PageShowPilot : Page
    {
        #region Constructors and Destructors

        public PageShowPilot(Pilot pilot)
        {
            Pilot = pilot;

            InitializeComponent();

            Salary = AirlineHelpers.GetPilotSalary(GameObject.GetInstance().HumanAirline, Pilot);
        }

        #endregion

        #region Public Properties

        public Pilot Pilot { get; set; }

        public double Salary { get; set; }

        #endregion

        #region Methods

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            var tab_main = UIHelpers.FindChild<TabControl>(Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                TabItem matchingItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Pilots").FirstOrDefault();

                tab_main.SelectedItem = matchingItem;
            }
        }

        private void btnHire_Click(object sender, RoutedEventArgs e)
        {
            WPFMessageBoxResult result = WPFMessageBox.Show(
                Translator.GetInstance().GetString("MessageBox", "2801"),
                Translator.GetInstance().GetString("MessageBox", "2801", "message"),
                WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                GameObject.GetInstance().HumanAirline.AddPilot(Pilot);
            }

            var tab_main = UIHelpers.FindChild<TabControl>(Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                TabItem matchingItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Pilots").FirstOrDefault();

                tab_main.SelectedItem = matchingItem;
            }
        }

        #endregion
    }
}