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
using TheAirlineV2.GraphicsModel.PageModel.GeneralModel;
using TheAirlineV2.Model.AirportModel;
using TheAirlineV2.Model.GeneralModel;
using TheAirlineV2.Model.AirlinerModel.RouteModel;

namespace TheAirlineV2.GraphicsModel.UserControlModel.CalendarModel
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
         * **/
        public void NotifyPropertyChange(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
        private RouteTimeTable TimeTable;
        public ucCalendar(RouteTimeTable timeTable)
        {
            this.TimeTable = timeTable;

            InitializeComponent();

            showTimeTable();

            //Month i header som {}

        }
        //show the time table
        private void showTimeTable()
        {
            TimeViewGrid.Children.Clear();

             //     int endDayOfWeek = (int)endDate.DayOfWeek;
            /*
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

                        foreach (Airport item in getAirportItems(currentDate))
                        {
                            AppointmentControl aControl = new AppointmentControl();
                            aControl.Airport = item;

                            //dbcDay.DayAppointmentsStack.Children.Add(aControl);
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
            */
            Grid grdTimes = UICreator.CreateGrid(8, 24);
        
            TimeSpan time = new TimeSpan(0, 0, 0);
            for (int i = 0; i < 24; i++)
            {

                TextBlock txtTime = UICreator.CreateTextBlock(string.Format("{0:D2}:{1:D2}", time.Hours, time.Minutes));
                txtTime.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
                txtTime.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                txtTime.Margin = new Thickness(0, 0, 5, 0);

                Grid.SetColumn(txtTime, 0);
                Grid.SetRow(txtTime, i);
                grdTimes.Children.Add(txtTime);

                time = time.Add(new TimeSpan(1, 0, 0));
            }
            int y = 0;

            time = new TimeSpan(0, 0, 0);
            for (int j = 1; j<8; j++)
            {
                time = new TimeSpan(0, 0, 0);
         
                for (int i = 0; i < 24; i++)
                {
                    TimeBoxControl control = new TimeBoxControl();
                    TimeSpan endTime = time.Add(new TimeSpan(0, 45, 0));

                    control.Entry = this.TimeTable.getEntry((DayOfWeek)j - 1, time, endTime);

                    Grid.SetRow(control, i);
                    Grid.SetColumn(control, j);

                    grdTimes.Children.Add(control);

                    y++;

                    time = time.Add(new TimeSpan(1, 0, 0));
                }
            } 
            TimeViewGrid.Children.Add(grdTimes);

        }
        //returns all the calendaritems for the current date
        private List<Airport> getAirportItems(DateTime date)
        {
            return new List<Airport>();

        }


        public event PropertyChangedEventHandler PropertyChanged;

    }

}
