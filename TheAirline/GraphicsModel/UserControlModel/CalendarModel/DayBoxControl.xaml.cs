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

namespace TheAirline.GraphicsModel.UserControlModel.CalendarModel
{
    /// <summary>
    /// Interaction logic for DayBoxControl.xaml
    /// </summary>
    public partial class DayBoxControl : UserControl
    {
        public static readonly DependencyProperty DayProperty =
                              DependencyProperty.Register("Day",
                              typeof(int), typeof(DayBoxControl));

        public static readonly DependencyProperty DayVisibilityProperty =
                             DependencyProperty.Register("DayVisibility",
                             typeof(Visibility), typeof(DayBoxControl));

        [Category("Common Properties")]
        public int Day
        {
            get { return (int)GetValue(DayProperty); }
            set { SetValue(DayProperty, value); }
        }
        public Visibility DayVisibility
        {
            get { return (Visibility)GetValue(DayVisibilityProperty); }
            set { SetValue(DayVisibilityProperty, value); }
        }
        public DayBoxControl()
        {
            InitializeComponent();

            this.DayVisibility = Visibility.Visible;
        }
    }
}
