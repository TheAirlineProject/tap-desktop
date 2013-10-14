using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GUIModel.PagesModel.AirlinersPageModel
{
    /// <summary>
    /// Interaction logic for PageUsedAirliners.xaml
    /// </summary>
    public partial class PageUsedAirliners : Page
    {
        public List<Airliner> AllAirliners { get; set; }
        public ObservableCollection<Airliner> SelectedAirliners { get; set; }
        public PageUsedAirliners()
        {
            this.Loaded += PageUsedAirliners_Loaded;

            this.AllAirliners = Airliners.GetAirlinersForSale().OrderByDescending(a => a.BuiltDate.Year).ToList();

            this.SelectedAirliners = new ObservableCollection<Airliner>();

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

        private void cbPossibleHomebase_Checked(object sender, RoutedEventArgs e)
        {
         
            var source = this.lvAirliners.Items as ICollectionView;
            source.Filter = o =>
            {
                Airliner a = o as Airliner;

                Boolean isPossible = GameObject.GetInstance().HumanAirline.Airports.FindAll(ai => ai.getCurrentAirportFacility(GameObject.GetInstance().HumanAirline, AirportFacility.FacilityType.Service).TypeLevel > 0 && ai.getMaxRunwayLength() >= a.Type.MinRunwaylength).Count > 0;
        
                return isPossible;
            };

            this.SelectedAirliners.Clear();
        }

        private void cbPossibleHomebase_Unchecked(object sender, RoutedEventArgs e)
        {
            var source = this.lvAirliners.Items as ICollectionView;
            source.Filter = o =>
            {
                Airliner a = o as Airliner;
                return true;
            };

            this.SelectedAirliners.Clear();
        }

        private void cbCompare_Checked(object sender, RoutedEventArgs e)
        {
            Airliner airliner = (Airliner)((CheckBox)sender).Tag;

            this.SelectedAirliners.Add(airliner);
        }

        private void cbCompare_Unchecked(object sender, RoutedEventArgs e)
        {
            Airliner airliner = (Airliner)((CheckBox)sender).Tag;

            this.SelectedAirliners.Remove(airliner);
        }

        private void btnCompare_Click(object sender, RoutedEventArgs e)
        {
            PopUpCompareAirliners.ShowPopUp(this.SelectedAirliners[0], this.SelectedAirliners[1]);

        }
    }
}
