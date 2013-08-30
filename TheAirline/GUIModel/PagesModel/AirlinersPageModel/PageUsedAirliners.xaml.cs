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
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Model.AirlinerModel;

namespace TheAirline.GUIModel.PagesModel.AirlinersPageModel
{
    /// <summary>
    /// Interaction logic for PageUsedAirliners.xaml
    /// </summary>
    public partial class PageUsedAirliners : Page
    {
        public List<Airliner> AllAirliners { get; set; }
        public PageUsedAirliners()
        {
            this.Loaded += PageUsedAirliners_Loaded;

            this.AllAirliners = Airliners.GetAirlinersForSale().OrderByDescending(a => a.BuiltDate.Year).ToList();
                
            InitializeComponent();
        }

        private void PageUsedAirliners_Loaded(object sender, RoutedEventArgs e)
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

        private void lnkAirliner_Click(object sender, RoutedEventArgs e)
        {
            Airliner airliner = (Airliner)((Hyperlink)sender).Tag;

            TabControl tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                var matchingItem =
     tab_main.Items.Cast<TabItem>()
       .Where(item => item.Tag.ToString() == "Airliner")
       .FirstOrDefault();

                matchingItem.Header = airliner.TailNumber;
                matchingItem.Visibility = System.Windows.Visibility.Visible;
                tab_main.SelectedItem = matchingItem;
            }

            Frame frmContent = UIHelpers.FindChild<Frame>(this.Tag as Page, "frmContent");

            if (frmContent != null)
                frmContent.Navigate(new PageUsedAirliner(airliner) { Tag = this.Tag });
        }
    }
}
