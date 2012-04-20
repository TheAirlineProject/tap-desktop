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
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.GraphicsModel.UserControlModel.CalendarModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlineModel;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpRouteAirliners.xaml
    /// </summary>
    public partial class PopUpRouteAirliners : PopUpWindow
    {
        private Route Route;
        private ListBox lbEntries;
        private List<RouteTimeTableEntry> Entries;
        public static object ShowPopUp(Route route)
        {
            PopUpWindow window = new PopUpRouteAirliners(route);
            window.ShowDialog();

            return window.Selected;
        }
        public PopUpRouteAirliners(Route route)
        {
           
            this.Route = route;

            this.Entries = this.Route.TimeTable.Entries;
 
            InitializeComponent();

            this.Title = string.Format("{0} - {1}", this.Route.Destination1.Profile.Name, this.Route.Destination2.Profile.Name);

            this.Width = 800;

            this.Height = 600;

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            StackPanel mainPanel = new StackPanel();
            mainPanel.Margin = new Thickness(10, 10, 10, 10);

            lbEntries = new ListBox();
            lbEntries.ItemTemplate = this.Resources["EntryItem"] as DataTemplate;
            lbEntries.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbEntries.MaxHeight = 500;

            foreach (RouteTimeTableEntry e in this.Entries)
                lbEntries.Items.Add(e);
            
            mainPanel.Children.Add(lbEntries);

            mainPanel.Children.Add(createButtonsPanel());

            this.Content = mainPanel;
        }
        //creates the button panel
        private WrapPanel createButtonsPanel()
        {
            WrapPanel buttonsPanel = new WrapPanel();
            buttonsPanel.Margin = new Thickness(0, 10, 0, 0);

            Button btnOk = new Button();
            btnOk.Uid = "100";
            btnOk.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnOk.Height = Double.NaN;
            btnOk.Width = Double.NaN;
            btnOk.Content = Translator.GetInstance().GetString("General", btnOk.Uid);
            btnOk.Click += new RoutedEventHandler(btnOk_Click);
            btnOk.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");

            buttonsPanel.Children.Add(btnOk);

            Button btnCancel = new Button();
            btnCancel.Uid = "101";
            btnCancel.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnCancel.Height = Double.NaN;
            btnCancel.Margin = new Thickness(5, 0, 0, 0);
            btnCancel.Width = Double.NaN;
            btnCancel.Click += new RoutedEventHandler(btnCancel_Click);
            btnCancel.Content = Translator.GetInstance().GetString("General", btnCancel.Uid);
            btnCancel.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");

            buttonsPanel.Children.Add(btnCancel);

            Button btnUndo = new Button();
            btnUndo.Uid = "103";
            btnUndo.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnUndo.Height = Double.NaN;
            btnUndo.Width = Double.NaN;
            btnUndo.Margin = new Thickness(5, 0, 0, 0);
            btnUndo.Content = Translator.GetInstance().GetString("General", btnUndo.Uid);
            btnUndo.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnUndo.Click += new RoutedEventHandler(btnUndo_Click);

            buttonsPanel.Children.Add(btnUndo);

            return buttonsPanel;

        }

        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            this.Entries = this.Route.TimeTable.Entries;
 
            lbEntries.Items.Clear();

            foreach (RouteTimeTableEntry entry in this.Entries)
                lbEntries.Items.Add(entry);
      
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {

            this.Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            foreach (object o in lbEntries.Items)
            {
                RouteTimeTableEntry entry = (RouteTimeTableEntry)o;
            }
            this.Close();
        }

        private void cbAirliner_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RouteTimeTableEntry entry = (RouteTimeTableEntry)((ComboBox)sender).Tag;

            FleetAirliner airliner = (FleetAirliner)((ComboBox)sender).SelectedItem;
            
            entry.Airliner = airliner;
         
        }
    }
    //the converter for returning airliners for a route entry
    public class RouteAirlinersConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            RouteTimeTableEntry entry = (RouteTimeTableEntry)value;

            List<FleetAirliner> airliners = GameObject.GetInstance().HumanAirline.Fleet;//entry.TimeTable.Route.getAirliners()[0].Airliner.Airline.Fleet;
          
            return airliners;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
   
}
