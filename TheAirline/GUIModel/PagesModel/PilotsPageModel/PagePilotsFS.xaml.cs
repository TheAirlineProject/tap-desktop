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

namespace TheAirline.GUIModel.PagesModel.PilotsPageModel
{
    /// <summary>
    /// Interaction logic for PagePilotsFS.xaml
    /// </summary>
    public partial class PagePilotsFS : Page
    {
        public PagePilotsFS()
        {
            this.Loaded += PagePilotsFS_Loaded;
            InitializeComponent();
        }

        private void PagePilotsFS_Loaded(object sender, RoutedEventArgs e)
        {
            Frame frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            frmContent.Navigate(new PagePilots() { Tag = this });
        }

        private void tcMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl control = (TabControl)sender;

            string selection = ((TabItem)control.SelectedItem).Tag.ToString();

            Frame frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            if (selection == "Pilots" && frmContent != null)
                frmContent.Navigate(new PagePilots() { Tag = this });

            if (selection == "Flightschools" && frmContent != null)
                frmContent.Navigate(new PageFlightSchools() { Tag = this });

        
        }
    }
}
