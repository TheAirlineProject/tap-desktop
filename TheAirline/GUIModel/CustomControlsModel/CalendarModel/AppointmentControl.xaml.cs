using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using TheAirline.Infrastructure;

namespace TheAirline.GraphicsModel.UserControlModel.CalendarModel
{
    /// <summary>
    /// Interaction logic for AppointmentControl.xaml
    /// </summary>
    public partial class AppointmentControl : UserControl
    {
        public static readonly DependencyProperty ItemProperty =
                             DependencyProperty.Register("Item",
                             typeof(CalendarItem), typeof(AppointmentControl));

        [Category("Common Properties")]
        public CalendarItem Item
        {
            get { return (CalendarItem)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public AppointmentControl()
        {
            InitializeComponent();


        }
    }
    //the converter for a calendar item to a brush
    public class CalendarItemToBrush : IValueConverter
    {
        //også under match, results, + stats
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Brush brush = Brushes.DarkBlue;

            CalendarItem item = (CalendarItem)value;

            if (item != null)
            {
                if (item.Type == CalendarItem.ItemType.Holiday)
                {
                    brush = new SolidColorBrush(Colors.DarkBlue);
                    brush.Opacity = 0.5;
                }
                if (item.Type == CalendarItem.ItemType.AirlinerOrder)
                {
                    brush = new SolidColorBrush(Colors.DarkRed);
                    brush.Opacity = 0.5;
                }
                if (item.Type == CalendarItem.ItemType.AirportOpening)
                {
                    brush = new SolidColorBrush(Colors.DarkGreen);
                    brush.Opacity = 0.5;
                }
                if (item.Type == CalendarItem.ItemType.AirportClosing)
                {
                    brush = new SolidColorBrush(Colors.Orange);
                    brush.Opacity = 0.5;
                }
            }
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
