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
using TheAirline.GraphicsModel.PageModel.GeneralModel;

namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    /// <summary>
    /// Interaction logic for PageScreenMode.xaml
    /// </summary>
    public partial class PageScreenMode : Page
    {
        public PageScreenMode()
        {
            InitializeComponent();
        }

        private void btnFullscreen_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.MainWindow.WindowStyle = WindowStyle.None;
            PageNavigator.MainWindow.Focus();

            PageNavigator.NavigateTo(new PageSelectLanguage());
        }

        private void btnWindowed_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageSelectLanguage());
    
        }
    }
}
