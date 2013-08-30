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

namespace TheAirline.GUIModel.MasterPageModel.PopUpPageModel
{
    /// <summary>
    /// Interaction logic for WPFPopUpTest.xaml
    /// </summary>
    public partial class WPFPopUpTest : UserControl
    {
        public WPFPopUpTest()
        {
            InitializeComponent();

            DataTemplate dt = this.Resources["TestTemplate"] as DataTemplate;

            WPFPopUp.Show("Test",dt);
        }
    }
}
