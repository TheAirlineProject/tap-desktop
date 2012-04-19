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
            TextBlock[,] cells = new TextBlock[8, 25];

            InitializeComponent();

            ScrollViewer scroller = new ScrollViewer();
            scroller.MaxWidth = 400;
            scroller.MaxHeight = 400;

            StackPanel panelCalendar = new StackPanel();

            DockPanel panelDays = new DockPanel();
            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                TextBlock txtDay = new TextBlock();
                txtDay.Width = 100;
                txtDay.Height = 60;
                txtDay.TextAlignment = TextAlignment.Center;
                txtDay.FontWeight = FontWeights.Bold;
                txtDay.Text = day.ToString();

                panelDays.Children.Add(txtDay);
            }
            panelCalendar.Children.Add(panelDays);

            ListBox lbHours = new ListBox();
            lbHours.ItemTemplate = this.Resources["HourItem"] as DataTemplate;
            lbHours.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
          
            for (int r = 0; r < 25; r++)
            {
                WrapPanel panelColumn = new WrapPanel();
                panelColumn.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                lbHours.Items.Add(panelColumn);

                for (int c = 0; c < 8; c++)
                {

                    Border border = new Border();
                    border.BorderThickness = new Thickness(1);
                    border.BorderBrush = Brushes.Black;

                    TextBlock txtCell = new TextBlock();
                    txtCell.Width = 100;
                    txtCell.Height = 60;
                    cells[c, r] = txtCell;

                    border.Child = txtCell;
                                       
                    panelColumn.Children.Add(border);

                }
            }
            panelCalendar.Children.Add(lbHours);

            scroller.Content = panelCalendar;

            this.Content = scroller;
          

        }




        public event PropertyChangedEventHandler PropertyChanged;
       
    }

}
