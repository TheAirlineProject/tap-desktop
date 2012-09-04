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
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel.HolidaysModel;
using TheAirline.Model.AirlinerModel;

namespace TheAirline.GraphicsModel.UserControlModel.CalendarModel
{
    /// <summary>
    /// Interaction logic for ucCalendar.xaml
    /// </summary>
    public partial class ucCalendar : UserControl, INotifyPropertyChanged
    {
        /*
        public static readonly DependencyProperty DateProperty =
                                DependencyProperty.Register("Date",
                                typeof(DateTime), typeof(ucSelectButton));


        [Category("Common Properties")]
        public DateTime Date
        {
            get { return (DateTime)GetValue(DateProperty); }
            set { SetValue(DateProperty, value); }
        }
         * */
        private DateTime ___Date;
        public DateTime Date
        {
            get { return ___Date; }
            set
            {
                if (___Date != value)
                {
                    ___Date = value;
                    NotifyPropertyChange("Date");
                }
            }
        }
        public void NotifyPropertyChange(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        public ucCalendar()
        {
            this.Date = GameObject.GetInstance().GameTime;

            InitializeComponent();

            showMonth();

            //Month i header som {}

        }
        //show current month
        private void showMonth()
        {
            MonthViewGrid.Children.Clear();

            int daysInMonth = DateTime.DaysInMonth(this.Date.Year, this.Date.Month);

            DateTime startDate = new DateTime(this.Date.Year, this.Date.Month, 1);
            //            DateTime endDate = new DateTime(startDate.Year, startDate.Month, daysInMonth);

            int startDayOfWeek = (int)startDate.DayOfWeek;
            //     int endDayOfWeek = (int)endDate.DayOfWeek;

            Grid grdDays = UICreator.CreateGrid(7, 6);
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    int day = i + 7 * j + 1;
                    DayBoxControl dbcDay = new DayBoxControl();

                    if (day > startDayOfWeek && day <= daysInMonth + startDayOfWeek)
                    {
                        dbcDay.Day = day - startDayOfWeek;


                        DateTime currentDate = new DateTime(this.Date.Year, this.Date.Month, dbcDay.Day);

                        foreach (CalendarItem item in getCalendarItems(currentDate))
                        {
                            AppointmentControl aControl = new AppointmentControl();
                            aControl.Item = item;

                            dbcDay.DayAppointmentsStack.Children.Add(aControl);
                        }



                    }
                    else
                        dbcDay.DayVisibility = System.Windows.Visibility.Collapsed;//.DayVisibility = System.Windows.Visibility.Hidden;

                    //   



                    Grid.SetColumn(dbcDay, i);
                    Grid.SetRow(dbcDay, j);

                    grdDays.Children.Add(dbcDay);
                }
            }

            MonthViewGrid.Children.Add(grdDays);
        }
        //returns all the calendaritems for the current date
        private List<CalendarItem> getCalendarItems(DateTime date)
        {
            List<CalendarItem> items = new List<CalendarItem>();

            var holidayGroups =
       from n in HolidayYear.GetHolidays(date)
       orderby n.Date
       group n by n.Holiday.Name into gh
       select new { Holiday = gh.First(), Countries = gh };


            foreach (var hg in holidayGroups)
            {

                List<Country> countries = new List<Country>();

                Console.WriteLine("Group {0}:", hg.Holiday.Holiday.Name);
                foreach (var n in hg.Countries)
                {
                    countries.Add(n.Holiday.Country);
       
                }
                items.Add(new CalendarItem(CalendarItem.ItemType.Holiday, date,hg.Holiday.Holiday.Name,string.Join("\r\n",from c in countries select c.Name)));
            }

            foreach (FleetAirliner orderedAirliner in GameObject.GetInstance().HumanAirline.Fleet.FindAll((a=>a.Airliner.BuiltDate.ToShortDateString() == date.ToShortDateString() && date>GameObject.GetInstance().GameTime)))
            {
                items.Add(new CalendarItem(CalendarItem.ItemType.Airliner_Order, date, orderedAirliner.Name, string.Format("{0} is delivered",orderedAirliner.Name)));
       
            }


         
            return items;

        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            this.Date = this.Date.AddMonths(1);

            showMonth();

        }
        private void ButtonPrev_Click(object sender, RoutedEventArgs e)
        {

            this.Date = this.Date.AddMonths(-1);

            showMonth();

        }
    }
    public class CalendarItem
    {
        public string Header { get; set; }
        public DateTime Date { get; set; }
        public string Subject { get; set; }
        public enum ItemType { Holiday, Airliner_Order }
        public ItemType Type { get; set; }
        public CalendarItem(ItemType type, DateTime date, string header, string subject)
        {
            this.Type = type;
            this.Header = header;
            this.Date = date;
            this.Subject = subject;
        }

    }

}
