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
    /// Interaction logic for ucTimeTable.xaml
    /// </summary>
    public partial class ucTimeTable : UserControl
    {
        public ObservableCollection<DateTime> Times { get; set; }
        public ObservableCollection<TimelineEntry> MondayEntries { get; set; }
        public ObservableCollection<TimelineEntry> TuesdayEntries { get; set; }
        public ObservableCollection<TimelineEntry> WednesdayEntries { get; set; }
        public ObservableCollection<TimelineEntry> ThursdayEntries { get; set; }
        public ObservableCollection<TimelineEntry> FridayEntries { get; set; }
        public ObservableCollection<TimelineEntry> SaturdayEntries { get; set; }
        public ObservableCollection<TimelineEntry> SundayEntries { get; set; }
       
        public static readonly DependencyProperty EntriesProperty =
                               DependencyProperty.Register("Entries",
                               typeof(IEnumerable<TimelineEntry>), typeof(ucTimeTable));

        [Category("Common Properties")]
        public IEnumerable<TimelineEntry> Entries
        {
            get { return (IEnumerable<TimelineEntry>)GetValue(EntriesProperty); }
            set { SetValue(EntriesProperty, value);  }
        }

        // Register the routed events
        public static readonly RoutedEvent EntryDeletedEvent =
            EventManager.RegisterRoutedEvent("EntryDeleted", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(ucTimeTable));

        public event RoutedEventHandler EntryDeleted
        {
            add { AddHandler(EntryDeletedEvent, value); }
            remove { RemoveHandler(EntryDeletedEvent, value); }
        }

        public static readonly RoutedEvent EntryAddedEvent =
            EventManager.RegisterRoutedEvent("EntryAdded", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(ucTimeTable));

        public event RoutedEventHandler EntryAdded
        {
            add { AddHandler(EntryAddedEvent, value); }
            remove { RemoveHandler(EntryAddedEvent, value); }
        }
        
        public static readonly RoutedEvent EntryChangedEvent =
              EventManager.RegisterRoutedEvent("EntryChanged", RoutingStrategy.Bubble,
              typeof(RoutedEventHandler), typeof(ucTimeTable));

        public event RoutedEventHandler EntryChanged
        {
            add { AddHandler(EntryChangedEvent, value); }
            remove { RemoveHandler(EntryChangedEvent, value); }
        }
        public ucTimeTable()
        {
            this.MondayEntries = new ObservableCollection<TimelineEntry>();
            this.TuesdayEntries = new ObservableCollection<TimelineEntry>();
            this.WednesdayEntries = new ObservableCollection<TimelineEntry>();
            this.ThursdayEntries = new ObservableCollection<TimelineEntry>();
            this.FridayEntries = new ObservableCollection<TimelineEntry>();
            this.SaturdayEntries = new ObservableCollection<TimelineEntry>();
            this.SundayEntries = new ObservableCollection<TimelineEntry>();

            this.Times = new ObservableCollection<DateTime>();

            this.Loaded += ucTimeTable_Loaded;
            
            InitializeComponent();
        }

        private void ucTimeTable_Loaded(object sender, RoutedEventArgs e)
        {
          
            for (int i = 0; i < 24; i++)
                this.Times.Add(new DateTime(1900,1,1,i,0,0));

        }

       
        //removes an entry from the time table
        public void removeTimelineEntry(TimeSpan startTime, TimeSpan endTime, string text)
        {
            if (startTime.Days != endTime.Days)
            {
                TimeSpan endTimeSplit = new TimeSpan(startTime.Days, 23, 59, 0);
                removeTimelineEntry(startTime, endTimeSplit, text);

                TimeSpan startTimeSplit = new TimeSpan(endTime.Days, 0, 0, 0);
                removeTimelineEntry(startTimeSplit, endTime, text);
            }

            var entries = new List<TimelineEntry>();

            entries.AddRange(this.TuesdayEntries);
            entries.AddRange(this.MondayEntries);
            entries.AddRange(this.WednesdayEntries);
            entries.AddRange(this.ThursdayEntries);
            entries.AddRange(this.FridayEntries);
            entries.AddRange(this.SaturdayEntries);
            entries.AddRange(this.SundayEntries);

            if (entries.Exists(e => e.StartTime == startTime && e.EndTime == endTime && e.Text == text))
            {
                if (startTime.Days == 2)
                    this.TuesdayEntries.Remove(this.TuesdayEntries.First(e => e.StartTime == startTime && e.EndTime == endTime && e.Text == text));

                if (startTime.Days == 3)
                    this.WednesdayEntries.Remove(this.WednesdayEntries.First(e => e.StartTime == startTime && e.EndTime == endTime && e.Text == text));

                if (startTime.Days == 4)
                    this.ThursdayEntries.Remove(this.ThursdayEntries.First(e => e.StartTime == startTime && e.EndTime == endTime && e.Text == text));

                if (startTime.Days == 5)
                    this.FridayEntries.Remove(this.FridayEntries.First(e => e.StartTime == startTime && e.EndTime == endTime && e.Text == text));

                if (startTime.Days == 6)
                    this.SaturdayEntries.Remove(this.SaturdayEntries.First(e => e.StartTime == startTime && e.EndTime == endTime && e.Text == text));

                if (startTime.Days == 0 || startTime.Days == 7)
                    this.SundayEntries.Remove(this.SundayEntries.First(e => e.StartTime == startTime && e.EndTime == endTime && e.Text == text));

                if (startTime.Days == 1)
                    this.MondayEntries.Remove(this.MondayEntries.First(e => e.StartTime == startTime && e.EndTime == endTime && e.Text == text));
            }
        }
        //adds an entry to the time table
        public void addTimelineEntry(object source, TimeSpan startTime, TimeSpan endTime,string text, Brush brush,string tooltip = "")
        {
            if (startTime.Days != endTime.Days)
            {
                TimeSpan endTimeSplit = new TimeSpan(startTime.Days, 23, 59, 0);
                addTimelineEntry(source, startTime, endTimeSplit, text, brush, tooltip);

                TimeSpan startTimeSplit = new TimeSpan(endTime.Days, 0, 0, 0);
                addTimelineEntry(source, startTimeSplit, endTime, text, brush, tooltip);
            }
            else
            {

                TimelineEntry entry = new TimelineEntry(source, startTime, endTime, text, brush, tooltip);

                if (entry.StartTime.Days == 2)
                    this.TuesdayEntries.Add(entry);

                if (entry.StartTime.Days == 3)
                    this.WednesdayEntries.Add(entry);

                if (entry.StartTime.Days == 4)
                    this.ThursdayEntries.Add(entry);

                if (entry.StartTime.Days == 5)
                    this.FridayEntries.Add(entry);

                if (entry.StartTime.Days == 6)
                    this.SaturdayEntries.Add(entry);

                if (entry.StartTime.Days == 0 || entry.StartTime.Days == 7)
                    this.SundayEntries.Add(entry);

                if (entry.StartTime.Days == 1)
                    this.MondayEntries.Add(entry);

            }
        }
        private void EntryAdded_Event(object sender, System.Windows.RoutedEventArgs e)
        {
          
            RaiseEvent(new RoutedEventArgs(EntryAddedEvent, e.OriginalSource));
        }
        private void EntryChanged_Event(object sender, System.Windows.RoutedEventArgs e)
        {

            RaiseEvent(new RoutedEventArgs(EntryChangedEvent, e.OriginalSource));
        }
        private void EntryDeleted_Event(object sender, System.Windows.RoutedEventArgs e)
        {
            string day = (string)((ucTimeline)sender).Tag;

            TimelineEntry entry = (TimelineEntry)e.OriginalSource;

            if (day == "Sunday")
                this.SundayEntries.Remove(entry);

            if (day == "Saturday")
                this.SaturdayEntries.Remove(entry);

            if (day == "Monday")
                this.MondayEntries.Remove(entry);

            if (day == "Tuesday")
                this.TuesdayEntries.Remove(entry);

            if (day == "Wednesday")
                this.WednesdayEntries.Remove(entry);

            if (day == "Thursday")
                this.ThursdayEntries.Remove(entry);

            if (day == "Friday")
                this.FridayEntries.Remove(entry);

            RaiseEvent(new RoutedEventArgs(EntryDeletedEvent, entry));
        }

    }
    //the converter for the time interval width
    public class TimeIntervalWidthConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double width = (double)value;

            double dx = width / (24 * 60);

            return System.Convert.ToDouble(60 * dx);

       }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
