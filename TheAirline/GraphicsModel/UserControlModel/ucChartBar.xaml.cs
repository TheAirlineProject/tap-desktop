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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace TheAirline.GraphicsModel.UserControlModel
{
    /// <summary>
    /// Interaction logic for ucChartBar.xaml
    /// </summary>
    public partial class ucChartBar : UserControl
    {
        public static readonly DependencyProperty ValueProperty =
                                DependencyProperty.Register("Value",
                                typeof(Double), typeof(ucChartBar));


        [Category("Common Properties")]
        public Double Value
        {
            get { return (Double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public static readonly DependencyProperty TextProperty =
                              DependencyProperty.Register("TextValue",
                              typeof(string), typeof(ucChartBar));


        [Category("Common Properties")]
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty BarColorProperty =
                              DependencyProperty.Register("BarColor",
                              typeof(Brush), typeof(ucChartBar));


        [Category("Common Properties")]
        public Brush BarColor
        {
            get { return (Brush)GetValue(BarColorProperty); }
            set { SetValue(BarColorProperty, value); }
        }

        public static readonly DependencyProperty BarHeightProperty =
                             DependencyProperty.Register("BarHeight",
                             typeof(double), typeof(ucChartBar));


        [Category("Common Properties")]
        public double BarHeight
        {
            get { return (double)GetValue(BarHeightProperty); }
            set { SetValue(BarHeightProperty, value); }
        }
       
       
        public ucChartBar()
        {
            InitializeComponent();

            this.BarColor = Brushes.DarkRed;

            this.Height = 120;
        }
    }
}
