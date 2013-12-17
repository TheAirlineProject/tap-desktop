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
        private static void OnShowAllPropertyChanged(DependencyObject source,
         DependencyPropertyChangedEventArgs e)
        {
            ucCalendar control = source as ucCalendar;

            control.showMonth();
     
        }
        public static readonly DependencyProperty ShowAllProperty =
                                DependencyProperty.Register("ShowAll",
                                typeof(Boolean), typeof(ucCalendar),new FrameworkPropertyMetadata(true,OnShowAllPropertyChanged));

        [Category("Common Properties")]
        public Boolean ShowAll
        {
            get { return (Boolean)GetValue(ShowAllProperty); }
            set { SetValue(ShowAllProperty, value);  }
        }
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

     
        }
        //show current month
        private void showMonth()
        {
            if (this.MonthViewGrid != null)
            {
                this.MonthViewGrid.Children.Clear();

                int daysInMonth = DateTime.DaysInMonth(this.Date.Year, this.Date.Month);

                DateTime startDate = new DateTime(this.Date.Year, this.Date.Month, 1);

                int startDayOfWeek = (int)startDate.DayOfWeek;

                Grid grdDays = new Grid();
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
                            dbcDay.DayVisibility = System.Windows.Visibility.Collapsed;



                        Grid.SetColumn(dbcDay, i);
                        Grid.SetRow(dbcDay, j);

                        grdDays.Children.Add(dbcDay);
                    }
                }

                MonthViewGrid.Children.Add(grdDays);
            }
        }
        //shows the current month
        public void showCurrentMonth()
        {
            showMonth();
        }
        //returns all the calendaritems for the current date
        private List<CalendarItem> getCalendarItems(DateTime date)
        {
            List<CalendarItem> items = new List<CalendarItem>();

            var airlineCountries = (from a in GameObject.GetInstance().HumanAirline.Airports select a.Profile.Country).Distinct();

            var holidayGroups =
       from h in HolidayYear.GetHolidays(date)
       where (this.ShowAll || airlineCountries.Contains(h.Holiday.Country)) 
       orderby h.Date
       group h by h.Holiday.Name into gh
       select new { Holiday = gh.First(), Countries = gh };


            foreach (var hg in holidayGroups)
            {

                List<Country> countries = new List<Country>();

                 foreach (var n in hg.Countries)
                {
                     if (!countries.Contains(n.Holiday.Country))
                        countries.Add(n.Holiday.Country);
       
                }
                items.Add(new CalendarItem(CalendarItem.ItemType.Holiday, date,hg.Holiday.Holiday.Name,string.Join("\r\n",from c in countries select c.Name)));
            }

            var airliners = GameObject.GetInstance().HumanAirline.Fleet.FindAll((a => a.Airliner.BuiltDate.ToShortDateString() == date.ToShortDateString() && date > GameObject.GetInstance().GameTime));
            
            if (airliners.Count > 0)
            {
                items.Add(new CalendarItem(CalendarItem.ItemType.Airliner_Order, date, "Delivery of airliners", string.Join("\r\n", from a in airliners select a.Name)));    
            }

            foreach (CalendarItem item in CalendarItems.GetCalendarItems(date))
                items.Add(item);
         
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
   
}
