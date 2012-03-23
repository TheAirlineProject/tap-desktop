using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpTest.xaml
    /// </summary>
    public partial class PopUpTest : Window
    {
        public static void ShowPopUp()
        {
            PopUpTest window = new PopUpTest();

            window.ShowDialog();
        }
        public PopUpTest()
        {
          

            InitializeComponent();
        }
    }
}
