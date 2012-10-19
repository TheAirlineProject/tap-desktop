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
using System.Windows.Shapes;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.Converters;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpAirportSlot.xaml
    /// </summary>
    public partial class PopUpAirportSlot : PopUpWindow
    {
        private Airport Airport;
        private Panel panelSlot;
        public static object ShowPopUp(Airport airport)
        {
            PopUpWindow window = new PopUpAirportSlot(airport);
            window.ShowDialog();

            return window.Selected;
        }
        public PopUpAirportSlot(Airport airport)
        {
            this.Airport = airport;

            InitializeComponent();

            this.Title = this.Airport.Profile.Name;

            this.Width = 800;
            
            this.Height = 400;

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            StackPanel panelMain = new StackPanel();

            WrapPanel panelDay = new WrapPanel();

            TextBlock txtDay = UICreator.CreateTextBlock("Day: ");
            //txtDay.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            txtDay.FontWeight = FontWeights.Bold;

            panelDay.Children.Add(txtDay);

            ComboBox cbDay = new ComboBox();
            cbDay.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbDay.Width = 100;
            cbDay.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            cbDay.Margin = new Thickness(0, 0, 0, 10);
            cbDay.SelectionChanged += new SelectionChangedEventHandler(cbDay_SelectionChanged);
            cbDay.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                cbDay.Items.Add(day);

            panelDay.Children.Add(cbDay);

            panelMain.Children.Add(panelDay);
            
            panelSlot = new StackPanel();
            panelMain.Children.Add(panelSlot);

            cbDay.SelectedIndex = 0;

            this.Content = panelMain;
        }

        private void cbDay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DayOfWeek day = (DayOfWeek)((ComboBox)sender).SelectedItem;
            panelSlot.Children.Clear();

            panelSlot.Children.Add(createSlotAllocationPanel(day));
        }
        //creates the panel for the slot allocation for a day
        private ScrollViewer createSlotAllocationPanel(DayOfWeek day)
        {
            ScrollViewer scroller = new ScrollViewer();
            scroller.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            scroller.MaxWidth = 800;

            StackPanel panelSlots = new StackPanel();
            panelSlots.Margin = new Thickness(0, 10, 0, 0);

         
            Dictionary<TimeSpan, KeyValuePair<int,int>> values = new Dictionary<TimeSpan, KeyValuePair<int,int>>();

            DateTime time = new DateTime(2000, 1, 1, 0, 0, 0);

            while (time.Day<2)
            {
                TimeSpan ts = new TimeSpan(time.Hour, time.Minute, 0);
                values.Add(ts, new KeyValuePair<int,int>(AirportHelpers.GetAirportTakeoffs(this.Airport, day, ts, ts.Add(new TimeSpan(0, 15, 0))).Count,AirportHelpers.GetAirportLandings(this.Airport,day,ts,ts.Add(new TimeSpan(0,15,0))).Count));

                time = time.AddMinutes(15);
            }

            panelSlots.Children.Add(createSlotsPanel(values, new TimeSpan(0, 0, 0), new TimeSpan(12, 0, 0)));
            panelSlots.Children.Add(createSlotsPanel(values,new TimeSpan(12,0,0),new TimeSpan(24,0,0)));

            scroller.Content = panelSlots;

            return scroller;
        }
        //creates the allocation panel for a specific timeslot 
        private WrapPanel createSlotsPanel(Dictionary<TimeSpan, KeyValuePair<int,int>> values, TimeSpan startTime, TimeSpan endTime)
        {
           
            WrapPanel panelSlots = new WrapPanel();
            panelSlots.Margin = new Thickness(0, 0, 0, 10);

            int maxValue = Math.Max(10,values.Max(v => v.Value.Key + v.Value.Value));

            double coeff = Convert.ToDouble(maxValue) / 100;

            var vs = (from v in values where v.Key>=startTime && v.Key<endTime select v);

            foreach (KeyValuePair<TimeSpan, KeyValuePair<int,int>> value in vs)
            {
                ucChartBar ucSerie = new ucChartBar();
                ucSerie.Value = value.Value.Key + value.Value.Value;
                ucSerie.Text = value.Key.Minutes == 0 ? new HourConverter().Convert(value.Key.Hours).ToString()  : value.Key.Minutes.ToString();
                ucSerie.FontSize = 10;
                ucSerie.FontWeight = value.Key.Minutes == 0 ? FontWeights.Bold : FontWeights.Regular;
                ucSerie.Width = value.Key.Minutes == 0 ? 30 : 25;
               
                ucSerie.ToolTip = createToolTip(value);
      
                ucSerie.BarHeight = Convert.ToDouble(value.Value.Value + value.Value.Key) / coeff;
                ucSerie.BarColor = getBarColor(value.Value.Value + value.Value.Key);

                panelSlots.Children.Add(ucSerie);
            }

            return panelSlots;

        }
        //returns the bar color for a value
        private Brush getBarColor(int value)
        {
            double maxFlightsPerSlot = 10 * this.Airport.Runways.Count;

            double slotValue = Convert.ToDouble(value) / maxFlightsPerSlot;

            Color baseColor;
            
            if (slotValue > 0.95)
                baseColor= Colors.DarkRed;
            else if (slotValue > 0.8 && slotValue <= 0.95)
                baseColor = Colors.Yellow;
            else
                baseColor = Colors.Green;

            return new SolidColorBrush(baseColor);//UICreator.CreateGradientBrush(baseColor);
        }
        //creates the tool tip for an entry
        private Border createToolTip(KeyValuePair<TimeSpan, KeyValuePair<int,int>> value)
        {
            double maxFlightsPerSlot = 10 * this.Airport.Runways.Count;

            double slotValue = Convert.ToDouble(value.Value.Value + value.Value.Key) / maxFlightsPerSlot;

            Border brdToolTip = new Border();
            brdToolTip.Margin = new Thickness(-4, 0, -4, -3);
            brdToolTip.Padding = new Thickness(5);
            brdToolTip.SetResourceReference(Border.BackgroundProperty, "HeaderBackgroundBrush2");

            StackPanel panelToolTip = new StackPanel();

            TextBlock txtTime = UICreator.CreateTextBlock(new TimeSpanConverter().Convert(value.Key).ToString());
            txtTime.FontWeight = FontWeights.Bold;
            txtTime.TextDecorations = TextDecorations.Underline;

            panelToolTip.Children.Add(txtTime);
            panelToolTip.Children.Add(UICreator.CreateTextBlock(string.Format("Departures: {0}",value.Value.Key)));
            panelToolTip.Children.Add(UICreator.CreateTextBlock(string.Format("Arrivals: {0}", value.Value.Value)));
            panelToolTip.Children.Add(UICreator.CreateTextBlock(string.Format("Percent of capacity: {0:P}",slotValue)));

            brdToolTip.Child = panelToolTip;
            
            return brdToolTip;

       
        }
    }
}
