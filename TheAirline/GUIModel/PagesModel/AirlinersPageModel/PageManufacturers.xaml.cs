using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GUIModel.PagesModel.AirlinersPageModel
{
    /// <summary>
    /// Interaction logic for PageManufacturers.xaml
    /// </summary>
    public partial class PageManufacturers : Page
    {
        public List<Manufacturer> AllManufacturers { get; set; }
        public PageManufacturers()
        {
            this.AllManufacturers = (from a in AirlinerTypes.GetAllTypes() where a.Produced.From <= GameObject.GetInstance().GameTime && a.Produced.To >= GameObject.GetInstance().GameTime orderby a.Manufacturer.Name select a.Manufacturer).Distinct().ToList();
            this.Loaded += PageManufacturers_Loaded;

            InitializeComponent();
        }

        private void PageManufacturers_Loaded(object sender, RoutedEventArgs e)
        {
            TabControl tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                var matchingItem =
     tab_main.Items.Cast<TabItem>()
       .Where(item => item.Tag.ToString() == "Manufacturer")
       .FirstOrDefault();

                matchingItem.Visibility = System.Windows.Visibility.Collapsed;

                matchingItem =
     tab_main.Items.Cast<TabItem>()
       .Where(item => item.Tag.ToString() == "Airliner")
       .FirstOrDefault();

                matchingItem.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void lnkManufacturer_Click(object sender, RoutedEventArgs e)
        {
            Manufacturer manufacturer = (Manufacturer)((Hyperlink)sender).Tag;

            TabControl tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                var matchingItem =
     tab_main.Items.Cast<TabItem>()
       .Where(item => item.Tag.ToString() == "Manufacturer")
       .FirstOrDefault();

                matchingItem.Header = manufacturer.Name;
                matchingItem.Visibility = System.Windows.Visibility.Visible;
                tab_main.SelectedItem = matchingItem;
            }

            Frame frmContent = UIHelpers.FindChild<Frame>(this.Tag as Page, "frmContent");

            if (frmContent != null)
                frmContent.Navigate(new PageManufacturer(manufacturer) { Tag = this.Tag });


        }
    }
      
}
