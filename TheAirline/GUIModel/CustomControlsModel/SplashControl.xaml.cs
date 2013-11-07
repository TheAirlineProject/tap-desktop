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

namespace TheAirline.GUIModel.CustomControlsModel
{
    /// <summary>
    /// Interaction logic for SplashControl.xaml
    /// </summary>
    public partial class SplashControl : UserControl
    {
        public static readonly DependencyProperty TextProperty =
                               DependencyProperty.Register("Text",
                               typeof(string), typeof(SplashControl));

        [Category("Common Properties")]
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public SplashControl()
        {
            InitializeComponent();
        }
    }
}
