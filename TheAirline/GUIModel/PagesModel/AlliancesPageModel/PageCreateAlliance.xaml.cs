using TheAirline.Infrastructure;
using TheAirline.Models.Airlines;
using TheAirline.Models.Airports;
using TheAirline.Models.General;

namespace TheAirline.GUIModel.PagesModel.AlliancesPageModel
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Imaging;

    using Microsoft.Win32;

    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.GeneralModel;

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
            this.InitializeComponent();

            this.cbHeadquarter.ItemsSource = GameObject.GetInstance().HumanAirline.Airports;

            this.logoPath = AppSettings.GetDataPath() + "\\graphics\\alliancelogos\\default.png";
            this.imgLogo.Source = new BitmapImage(new Uri(this.logoPath, UriKind.RelativeOrAbsolute));

            this.Loaded += this.PageCreateAlliance_Loaded;
        }

        #endregion

        #region Methods

        private void PageCreateAlliance_Loaded(object sender, RoutedEventArgs e)
        {
            var tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

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
                this.txtName.Text.Trim(),
                (Airport)this.cbHeadquarter.SelectedItem);
            alliance.Logo = this.logoPath;
            alliance.AddMember(
                new AllianceMember(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime));

            Alliances.AddAlliance(alliance);

            var tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

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
                this.logoPath = dlg.FileName;
                this.imgLogo.Source = new BitmapImage(new Uri(this.logoPath, UriKind.RelativeOrAbsolute));
            }
        }

        #endregion
    }
}