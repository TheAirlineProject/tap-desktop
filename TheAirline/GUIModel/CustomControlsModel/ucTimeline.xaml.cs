using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for ucTimeline.xaml
    /// </summary>
    public partial class ucTimeline : UserControl
    {
        public static readonly DependencyProperty TextProperty =
                                DependencyProperty.Register("Text",
                                typeof(string), typeof(ucTimeline));

        [Category("Common Properties")]
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty EntriesProperty =
                                DependencyProperty.Register("Entries",
                                typeof(IEnumerable<TimelineEntry>), typeof(ucTimeline));

        [Category("Common Properties")]
        public IEnumerable<TimelineEntry> Entries
        {
            get { return (IEnumerable<TimelineEntry>)GetValue(EntriesProperty); }
            set { SetValue(EntriesProperty, value); }
        }

        // Register the routed event
        public static readonly RoutedEvent EntryDeletedEvent =
            EventManager.RegisterRoutedEvent("EntryDeleted", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(ucTimeline));

        public event RoutedEventHandler EntryDeleted
        {
            add { AddHandler(EntryDeletedEvent, value); }
            remove { RemoveHandler(EntryDeletedEvent, value); }
        }
 

        public ucTimeline()
        {

            this.Entries = new ObservableCollection<TimelineEntry>();

            InitializeComponent();

        }

        private void tlEntry_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                TimelineEntry entry = (TimelineEntry)((Border)sender).Tag;
            
                RaiseEvent(new RoutedEventArgs(EntryDeletedEvent,entry));
            }
        }

        private void timeline_Drop(object sender, DragEventArgs e)
        {
           
        }

        
    }
    //the converter for the time to position
    public class TimeToPositionConverter : IMultiValueConverter
    {
       
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double width = (double)values[0];
            TimeSpan ts = (TimeSpan)values[1];

            TimeSpan time = new TimeSpan(ts.Hours, ts.Minutes, ts.Seconds);

            double dx = width / (24 * 60);

            return System.Convert.ToDouble(time.TotalMinutes * dx);
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
