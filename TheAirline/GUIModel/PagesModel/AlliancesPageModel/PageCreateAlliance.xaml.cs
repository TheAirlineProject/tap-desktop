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
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GUIModel.PagesModel.AlliancesPageModel
{
    /// <summary>
    /// Interaction logic for PageCreateAlliance.xaml
    /// </summary>
    public partial class PageCreateAlliance : Page
    {
        private string logoPath;
        public PageCreateAlliance()
        {
           
            InitializeComponent();

            cbHeadquarter.ItemsSource = GameObject.GetInstance().HumanAirline.Airports;
            
            logoPath = AppSettings.getDataPath() + "\\graphics\\alliancelogos\\default.png";
            imgLogo.Source = new BitmapImage(new Uri(logoPath, UriKind.RelativeOrAbsolute));

            this.Loaded += PageCreateAlliance_Loaded;
        }

        private void PageCreateAlliance_Loaded(object sender, RoutedEventArgs e)
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

        private void btnLogo_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".png";
            dlg.Filter = "Images (.png)|*.png";
            dlg.InitialDirectory = AppSettings.getDataPath() + "\\graphics\\alliancelogos\\";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                logoPath = dlg.FileName;
                imgLogo.Source = new BitmapImage(new Uri(logoPath, UriKind.RelativeOrAbsolute));

            }
        }
        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            Alliance alliance = new Alliance(GameObject.GetInstance().GameTime, rbFull.IsChecked.Value ? Alliance.AllianceType.Full : Alliance.AllianceType.Codesharing, txtName.Text.Trim(), (Airport)cbHeadquarter.SelectedItem);
            alliance.Logo = logoPath;
            alliance.addMember(new AllianceMember(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime));

            Alliances.AddAlliance(alliance);
         
            TabControl tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                var matchingItem =
     tab_main.Items.Cast<TabItem>()
       .Where(item => item.Tag.ToString() == "Alliances")
       .FirstOrDefault();

                //matchingItem.IsSelected = true;
                tab_main.SelectedItem = matchingItem;
            }
        }
    }
}
