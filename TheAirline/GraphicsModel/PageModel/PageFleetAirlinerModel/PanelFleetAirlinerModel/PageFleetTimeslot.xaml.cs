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
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GraphicsModel.PageModel.PageFleetAirlinerModel.PanelFleetAirlinerModel
{
    /// <summary>
    /// Interaction logic for PageFleetTimeslot.xaml
    /// </summary>
    public partial class PageFleetTimeslot : Page
    {
        private FleetAirliner Airliner;
        private TextBlock txtDay;
        private ListBox lbTimeSlot;
        private DayOfWeek Day;
        public PageFleetTimeslot(FleetAirliner airliner)
        {
            this.Airliner = airliner;

            this.Day = GameObject.GetInstance().GameTime.DayOfWeek;

            InitializeComponent();

            StackPanel panelTimeSlot = new StackPanel();
            panelTimeSlot.Margin = new Thickness(0, 10, 50, 0);

            WrapPanel panelDayNavigator = new WrapPanel();

            Button btnPrevious = new Button();
            btnPrevious.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnPrevious.Height = 24;
            btnPrevious.Width = 32;
            btnPrevious.Content = "<";
            btnPrevious.Margin = new Thickness(2, 0, 0, 0);
            btnPrevious.Click += new RoutedEventHandler(btnPrevious_Click);
            btnPrevious.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            panelDayNavigator.Children.Add(btnPrevious);

            Button btnNext = new Button();
            btnNext.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnNext.Height = 24;
            btnNext.Margin = new Thickness(2, 0, 0, 0);
            btnNext.Width = 32;
            btnNext.Content = ">";
            btnNext.Click += new RoutedEventHandler(btnNext_Click);
            btnNext.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            panelDayNavigator.Children.Add(btnNext);

            txtDay = new TextBlock();
            txtDay.FontSize = 14;
            txtDay.FontWeight = FontWeights.Bold;
            txtDay.Margin = new Thickness(5, 0, 0, 0);
            txtDay.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            panelDayNavigator.Children.Add(txtDay);

            panelTimeSlot.Children.Add(panelDayNavigator);

            lbTimeSlot = new ListBox();
            lbTimeSlot.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            panelTimeSlot.Children.Add(lbTimeSlot);

            ScrollViewer scroller = new ScrollViewer();
            scroller.Height = GraphicsHelpers.GetContentHeight() - 100;
         
            panelTimeSlot.Children.Add(scroller);

            WrapPanel panelHours = new WrapPanel();
            scroller.Content = panelHours;

      
            ListBox lbHours = new ListBox();
            lbHours.ItemTemplate = this.Resources["HourItem"] as DataTemplate;
            lbHours.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbHours.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            panelHours.Children.Add(lbHours);
            
            for (int i = 0; i < 24; i++)
                lbHours.Items.Add(new TimeSpan(i, 0, 0));

            Canvas cnvFlights = new Canvas();
            panelHours.Children.Add(cnvFlights);


            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                 var list = (from e in this.Airliner.Routes.SelectMany(r=>r.TimeTable.Entries.FindAll(te=>te.Airliner==this.Airliner)) select e);
          
                foreach (RouteTimeTableEntry e in list)
                {
                    TimeSpan flightTime = MathHelpers.GetFlightTime(e.TimeTable.Route.Destination1.Profile.Coordinates, e.TimeTable.Route.Destination2.Profile.Coordinates, this.Airliner.Airliner.Type);

                    ContentControl ccFlight = new ContentControl();
                    ccFlight.ContentTemplate = this.Resources["FlightItem"] as DataTemplate;
                    ccFlight.Content = new KeyValuePair<double, RouteTimeTableEntry>(flightTime.TotalMinutes, e);
                    int minutes = 15 * (e.Time.Minutes / 15);
                    Canvas.SetTop(ccFlight, 60 * e.Time.Hours + e.Time.Minutes);
                    Canvas.SetLeft(ccFlight, 100 * (int)day);

                    cnvFlights.Children.Add(ccFlight);

                }
                               
            }



             
            this.Content = panelTimeSlot;

            showDayTimeSlot();
                      
 
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (this.Day < DayOfWeek.Saturday)
            {
                this.Day++;
                showDayTimeSlot();
            }
        }

        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (this.Day > DayOfWeek.Sunday)
            {
                this.Day--;
                showDayTimeSlot();
            }
        }
        //show the time slot for a specific day
        private void showDayTimeSlot()
        {
            txtDay.Text = this.Day.ToString();
    
            lbTimeSlot.Items.Clear();

            var list = (from e in this.Airliner.Routes.SelectMany(r => r.TimeTable.Entries.FindAll(te => te.Airliner == this.Airliner && te.Day == this.Day)) select e);
        
            //var list = this.Airliner.Route.TimeTable.Entries.FindAll(e => e.Airliner == this.Airliner && e.Day == this.Day );

            foreach (RouteTimeTableEntry e in list)
            {
              
                TimeSpan endTime = e.Time.Add(MathHelpers.GetFlightTime(e.TimeTable.Route.Destination1.Profile.Coordinates, e.TimeTable.Route.Destination2.Profile.Coordinates,this.Airliner.Airliner.Type));
                lbTimeSlot.Items.Add(string.Format("{0} - {1} {2}-{3}", e.Destination.Airport.Profile.Name, e.Day, e.Time,endTime));
            }

        }
    }
}
