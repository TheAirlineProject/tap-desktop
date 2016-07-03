using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Models.Airlines;
using TheAirline.Models.General;

namespace TheAirline.GUIModel.PagesModel.AlliancesPageModel
{
    /// <summary>
    ///     Interaction logic for PageShowAlliances.xaml
    /// </summary>
    public partial class PageShowAlliances : Page
    {
        #region Constructors and Destructors

        public PageShowAlliances()
        {
            AllAlliances = Alliances.GetAlliances();

            AllCodesharings = new ObservableCollection<CodeshareAgreementMVVM>();

            IEnumerable<CodeshareAgreement> codesharings =
                Airlines.GetAllAirlines().SelectMany(a => a.Codeshares).Distinct();

            foreach (CodeshareAgreement agreement in codesharings)
            {
                AllCodesharings.Add(new CodeshareAgreementMVVM(agreement));
            }

            InitializeComponent();

            Loaded += PageShowAlliances_Loaded;
        }

        #endregion

        #region Public Properties

        public List<Alliance> AllAlliances { get; set; }

        public ObservableCollection<CodeshareAgreementMVVM> AllCodesharings { get; set; }

        #endregion

        #region Methods

        private void PageShowAlliances_Loaded(object sender, RoutedEventArgs e)
        {
            var tab_main = UIHelpers.FindChild<TabControl>(Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                TabItem matchingItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Alliance").FirstOrDefault();

                matchingItem.Visibility = Visibility.Collapsed;
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var agreement = (CodeshareAgreementMVVM)((Button)sender).Tag;

            Airline airline = agreement.Agreement.Airline1.IsHuman
                ? agreement.Agreement.Airline2
                : agreement.Agreement.Airline1;

            WPFMessageBoxResult result = WPFMessageBox.Show(
                Translator.GetInstance().GetString("MessageBox", "2611"),
                string.Format(Translator.GetInstance().GetString("MessageBox", "2611", "message"), airline.Profile.Name),
                WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                AllCodesharings.Remove(agreement);

                agreement.Agreement.Airline1.RemoveCodeshareAgreement(agreement.Agreement);
                agreement.Agreement.Airline2.RemoveCodeshareAgreement(agreement.Agreement);
            }
        }

        private void clName_Click(object sender, RoutedEventArgs e)
        {
            var alliance = (Alliance)((Hyperlink)sender).Tag;
            var tab_main = UIHelpers.FindChild<TabControl>(Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                TabItem matchingItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Alliance").FirstOrDefault();

                //matchingItem.IsSelected = true;
                matchingItem.Header = alliance.Name;
                matchingItem.Visibility = Visibility.Visible;

                tab_main.SelectedItem = matchingItem;
            }

            var frmContent = UIHelpers.FindChild<Frame>(Tag as Page, "frmContent");

            if (frmContent != null)
            {
                frmContent.Navigate(new PageShowAlliance(alliance) { Tag = Tag });
            }
        }

        #endregion
    }
}