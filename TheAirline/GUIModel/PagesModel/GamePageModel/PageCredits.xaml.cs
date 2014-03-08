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

namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    /// <summary>
    /// Interaction logic for PageCredits.xaml
    /// </summary>
    public partial class PageCredits : Page
    {
        public PageCredits()
        {
            InitializeComponent();
        }

        private void btnStartMenu_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageStartMenu());
        }
    }
}
