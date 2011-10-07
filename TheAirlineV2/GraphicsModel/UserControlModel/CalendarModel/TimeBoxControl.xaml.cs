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
using TheAirlineV2.Model.AirlinerModel.RouteModel;

namespace TheAirlineV2.GraphicsModel.UserControlModel.CalendarModel
{
    /// <summary>
    /// Interaction logic for DayBoxControl.xaml
    /// </summary>
    public partial class TimeBoxControl : UserControl
    {
        public static readonly DependencyProperty EntryProperty =
                              DependencyProperty.Register("Entry",
                              typeof(RouteTimeTableEntry), typeof(TimeBoxControl));

       
        [Category("Common Properties")]
        public RouteTimeTableEntry Entry
        {
            get { return (RouteTimeTableEntry)GetValue(EntryProperty); }
            set { SetValue(EntryProperty, value); }
        }
          public TimeBoxControl()
        {
            InitializeComponent();

        
        }
    }
}
