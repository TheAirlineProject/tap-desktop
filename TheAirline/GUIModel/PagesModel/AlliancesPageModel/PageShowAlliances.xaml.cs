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
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GUIModel.PagesModel.AlliancesPageModel
{
    /// <summary>
    /// Interaction logic for PageShowAlliances.xaml
    /// </summary>
    public partial class PageShowAlliances : Page
    {
        public List<Alliance> AllAlliances { get; set; }
        public ObservableCollection<CodeshareAgreementMVVM> AllCodesharings { get; set; }
        public PageShowAlliances()
        {
            this.AllAlliances = Alliances.GetAlliances();

            this.AllCodesharings = new ObservableCollection<CodeshareAgreementMVVM>();

            var codesharings = Airlines.GetAllAirlines().SelectMany(a => a.Codeshares).Distinct();

            foreach (CodeshareAgreement agreement in codesharings)
                this.AllCodesharings.Add(new CodeshareAgreementMVVM(agreement));

            InitializeComponent();

            this.Loaded += PageShowAlliances_Loaded;

        }

        private void PageShowAlliances_Loaded(object sender, RoutedEventArgs e)
        {
            TabControl tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                var matchingItem =
     tab_main.Items.Cast<TabItem>()
       .Where(item => item.Tag.ToString() == "Alliance")
       .FirstOrDefault();

                matchingItem.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void clName_Click(object sender, RoutedEventArgs e)
        {
            Alliance alliance = (Alliance)((Hyperlink)sender).Tag;
            TabControl tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                var matchingItem =
     tab_main.Items.Cast<TabItem>()
       .Where(item => item.Tag.ToString() == "Alliance")
       .FirstOrDefault();

                //matchingItem.IsSelected = true;
                matchingItem.Header = alliance.Name;
                matchingItem.Visibility = System.Windows.Visibility.Visible;

                tab_main.SelectedItem = matchingItem;
            }

            Frame frmContent = UIHelpers.FindChild<Frame>(this.Tag as Page, "frmContent");

            if (frmContent != null)
            {
                frmContent.Navigate(new PageShowAlliance(alliance) { Tag = this.Tag });

            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            CodeshareAgreementMVVM agreement = (CodeshareAgreementMVVM)((Button)sender).Tag;

            Airline airline = agreement.Agreement.Airline1.IsHuman ? agreement.Agreement.Airline2 : agreement.Agreement.Airline1;

            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2611"), string.Format(Translator.GetInstance().GetString("MessageBox", "2611", "message"), airline.Profile.Name), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
              
                this.AllCodesharings.Remove(agreement);

                agreement.Agreement.Airline1.removeCodeshareAgreement(agreement.Agreement);
                agreement.Agreement.Airline2.removeCodeshareAgreement(agreement.Agreement);
            }
        }
    }
}
