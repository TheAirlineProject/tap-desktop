using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Infrastructure;
using TheAirline.Models.Airlines;
using TheAirline.Models.Airports;
using TheAirline.Models.General;

namespace TheAirline.GUIModel.PagesModel.AlliancesPageModel
{
    /// <summary>
    ///     Interaction logic for PageCreateAlliance.xaml
    /// </summary>
    public partial class PageCreateAlliance : Page
    {
        #region Fields

        private string logoPath;

        #endregion

        #region Constructors and Destructors

        public PageCreateAlliance()
        {
            InitializeComponent();

            cbHeadquarter.ItemsSource = GameObject.GetInstance().HumanAirline.Airports;

            logoPath = AppSettings.GetDataPath() + "\\graphics\\alliancelogos\\default.png";
            imgLogo.Source = new BitmapImage(new Uri(logoPath, UriKind.RelativeOrAbsolute));

            Loaded += PageCreateAlliance_Loaded;
        }

        #endregion

        #region Methods

        private void PageCreateAlliance_Loaded(object sender, RoutedEventArgs e)
        {
            var tab_main = UIHelpers.FindChild<TabControl>(Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                TabItem matchingItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Alliance").FirstOrDefault();

                matchingItem.Visibility = Visibility.Collapsed;
            }
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            var alliance = new Alliance(
                GameObject.GetInstance().GameTime,
                txtName.Text.Trim(),
                (Airport)cbHeadquarter.SelectedItem);
            alliance.Logo = logoPath;
            alliance.AddMember(
                new AllianceMember(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime));

            Alliances.AddAlliance(alliance);

            var tab_main = UIHelpers.FindChild<TabControl>(Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                TabItem matchingItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Alliances").FirstOrDefault();

                //matchingItem.IsSelected = true;
                tab_main.SelectedItem = matchingItem;
            }
        }

        private void btnLogo_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();

            dlg.DefaultExt = ".png";
            dlg.Filter = "Images (.png)|*.png";
            dlg.InitialDirectory = AppSettings.GetDataPath() + "\\graphics\\alliancelogos\\";

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                logoPath = dlg.FileName;
                imgLogo.Source = new BitmapImage(new Uri(logoPath, UriKind.RelativeOrAbsolute));
            }
        }

        #endregion
    }
}