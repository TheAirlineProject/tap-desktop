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

namespace TheAirline.GUIModel.PagesModel.OptionsPageModel
{
    /// <summary>
    /// Interaction logic for PageOptions.xaml
    /// </summary>
    public partial class PageOptions : Page
    {
        public PageOptions()
        {
            this.Loaded += PageOptions_Loaded;
            InitializeComponent();
        }

        private void PageOptions_Loaded(object sender, RoutedEventArgs e)
        {
            Frame frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            frmContent.Navigate(new PageShowOptions() { Tag = this });
        }
        private void tcMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl control = (TabControl)sender;

            string selection = ((TabItem)control.SelectedItem).Tag.ToString();

            Frame frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            if (selection == "Options" && frmContent != null)
                frmContent.Navigate(new PageShowOptions() { Tag = this });

            if (selection == "Save" && frmContent != null)
                frmContent.Navigate(new PageSaveGame() { Tag = this });

            if (selection == "Load" && frmContent != null)
                frmContent.Navigate(new PageLoadGame() { Tag = this });


        }
    }
}
