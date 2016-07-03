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
using TheAirline.Models.Airports;

namespace TheAirline.GUIModel.CustomControlsModel
{
    /// <summary>
    /// Interaction logic for ucTimeline.xaml
    /// </summary>
    public partial class ucTimeline : UserControl
    {
        private Point startPoint;

        public static readonly DependencyProperty DayProperty =
                                DependencyProperty.Register("Day",
                                typeof(DayOfWeek), typeof(ucTimeline));

        [Category("Common Properties")]
        public DayOfWeek Day
        {
            get { return (DayOfWeek)GetValue(DayProperty); }
            set { SetValue(DayProperty, value); }
        }

      
        public static readonly DependencyProperty EntriesProperty =
                                DependencyProperty.Register("Entries",
                                typeof(IEnumerable<TimelineEntry>), typeof(ucTimeline));

        [Category("Common Properties")]
        public ObservableCollection<TimelineEntry> Entries
        {
            get { return (ObservableCollection<TimelineEntry>)GetValue(EntriesProperty); }
            set { SetValue(EntriesProperty, value); }
        }

        // Register the routed events
        public static readonly RoutedEvent EntryDeletedEvent =
            EventManager.RegisterRoutedEvent("EntryDeleted", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(ucTimeline));

        public event RoutedEventHandler EntryDeleted
        {
            add { AddHandler(EntryDeletedEvent, value); }
            remove { RemoveHandler(EntryDeletedEvent, value); }
        }

        public static readonly RoutedEvent EntryAddedEvent =
           EventManager.RegisterRoutedEvent("EntryAdded", RoutingStrategy.Bubble,
           typeof(RoutedEventHandler), typeof(ucTimeline));

        public event RoutedEventHandler EntryAdded
        {
            add { AddHandler(EntryAddedEvent, value); }
            remove { RemoveHandler(EntryAddedEvent, value); }
        }

        public static readonly RoutedEvent EntryChangedEvent =
         EventManager.RegisterRoutedEvent("EntryChanged", RoutingStrategy.Bubble,
         typeof(RoutedEventHandler), typeof(ucTimeline));

        public event RoutedEventHandler EntryChanged
        {
            add { AddHandler(EntryChangedEvent, value); }
            remove { RemoveHandler(EntryChangedEvent, value); }
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
        private void timeline_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(null);
        }
        private void timeline_MouseMove(object sender, MouseEventArgs e)
        {
           
            // Get the current mouse position
            Point mousePos = e.GetPosition(null);
            Vector diff = startPoint - mousePos;
            
            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                Border border = (Border)sender;
                TimelineEntry entry = (TimelineEntry)border.Tag;
            
                // Initialize the drag & drop operation
                DataObject dragData = new DataObject("Entry", entry);
                DragDrop.DoDragDrop(border, dragData, DragDropEffects.Move);
            }
        }
        private void timeline_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("Entry"))
            {
                TimelineEntry entry = e.Data.GetData("Entry") as TimelineEntry; 

                Point dropPosition = e.GetPosition(this.icEntries);

                TimeSpan time = getPositionTime(dropPosition.X);

                TimelineEntry newEntry = new TimelineEntry(entry.Source, new TimeSpan((int)this.Day, time.Hours,time.Minutes,time.Seconds), entry.EndTime, entry.Text, entry.Brush, entry.ToolTip);

                object[] objects = new object[2]{entry,newEntry};

                RaiseEvent(new RoutedEventArgs(EntryChangedEvent,objects));

            }
            if (e.Data.GetDataPresent("Route"))
            {
                Point dropPosition = e.GetPosition(this.icEntries);

                TimeSpan time = getPositionTime(dropPosition.X);

                //Route route= e.Data.GetData("Route") as Route;

                Airport route = e.Data.GetData("Route") as Airport;

                TimelineDropItem entry = new TimelineDropItem(time,this.Day,e.Data.GetData("Route"));
                RaiseEvent(new RoutedEventArgs(EntryAddedEvent, entry));
   
                //ListView listView = sender as ListView;
                //listView.Items.Add(contact);
            }
        }
       
        //converts a position to time span
        private TimeSpan getPositionTime(double position)
        {
            double width = icEntries.ActualWidth;

            double dx = width / (24 * 60);

            return new TimeSpan(0, (int)(position / dx),0);
        }

      

      
       
       

        
    }
    //the class for a drop item
    public class TimelineDropItem
    {
        public TimeSpan Time { get; set; }
        public DayOfWeek Day { get; set; }
        public object Object { get; set; }
        public TimelineDropItem(TimeSpan time, DayOfWeek day, object o)
        {
            this.Object = o;
            this.Day = day;
            this.Time = time;
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
