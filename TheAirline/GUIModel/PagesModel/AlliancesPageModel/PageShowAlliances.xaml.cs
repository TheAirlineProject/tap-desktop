namespace TheAirline.GUIModel.PagesModel.AlliancesPageModel
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;

    using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.AirlineModel;
    using TheAirline.Model.GeneralModel;

    /// <summary>
    ///     Interaction logic for PageShowAlliances.xaml
    /// </summary>
    public partial class PageShowAlliances : Page
    {
        #region Constructors and Destructors

        public PageShowAlliances()
        {
            this.AllAlliances = Alliances.GetAlliances();

            this.AllCodesharings = new ObservableCollection<CodeshareAgreementMVVM>();

            IEnumerable<CodeshareAgreement> codesharings =
                Airlines.GetAllAirlines().SelectMany(a => a.Codeshares).Distinct();

            foreach (CodeshareAgreement agreement in codesharings)
            {
                this.AllCodesharings.Add(new CodeshareAgreementMVVM(agreement));
            }

            this.InitializeComponent();

            this.Loaded += this.PageShowAlliances_Loaded;
        }

        #endregion

        #region Public Properties

        public List<Alliance> AllAlliances { get; set; }

        public ObservableCollection<CodeshareAgreementMVVM> AllCodesharings { get; set; }

        #endregion

        #region Methods

        private void PageShowAlliances_Loaded(object sender, RoutedEventArgs e)
        {
            var tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

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
                this.AllCodesharings.Remove(agreement);

                agreement.Agreement.Airline1.removeCodeshareAgreement(agreement.Agreement);
                agreement.Agreement.Airline2.removeCodeshareAgreement(agreement.Agreement);
            }
        }

        private void clName_Click(object sender, RoutedEventArgs e)
        {
            var alliance = (Alliance)((Hyperlink)sender).Tag;
            var tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                TabItem matchingItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Alliance").FirstOrDefault();

                //matchingItem.IsSelected = true;
                matchingItem.Header = alliance.Name;
                matchingItem.Visibility = Visibility.Visible;

                tab_main.SelectedItem = matchingItem;
            }

            var frmContent = UIHelpers.FindChild<Frame>(this.Tag as Page, "frmContent");

            if (frmContent != null)
            {
                frmContent.Navigate(new PageShowAlliance(alliance) { Tag = this.Tag });
            }
        }

        #endregion
    }
}