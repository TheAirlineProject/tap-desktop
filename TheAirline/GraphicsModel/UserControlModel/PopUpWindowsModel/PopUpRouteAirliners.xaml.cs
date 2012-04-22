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
        private ListBox lbEntriesDest1, lbEntriesDest2;
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

            this.Entries = new List<RouteTimeTableEntry>(this.Route.TimeTable.Entries);

            InitializeComponent();

            this.Title = string.Format("{0} - {1}", this.Route.Destination1.Profile.Name, this.Route.Destination2.Profile.Name);

            this.Width = 800;

            this.Height = 600;

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            StackPanel mainPanel = new StackPanel();
            mainPanel.Margin = new Thickness(10, 10, 10, 10);

            Grid grdEntries = UICreator.CreateGrid(2);
            mainPanel.Children.Add(grdEntries);

            lbEntriesDest1 = new ListBox();
            lbEntriesDest1.ItemTemplate = this.Resources["EntryItem"] as DataTemplate;
            lbEntriesDest1.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbEntriesDest1.Margin = new Thickness(0, 0, 5, 0);
            lbEntriesDest1.MaxHeight = 500;

            Grid.SetColumn(lbEntriesDest1, 0);
            grdEntries.Children.Add(lbEntriesDest1);

            lbEntriesDest2 = new ListBox();
            lbEntriesDest2.ItemTemplate = this.Resources["EntryItem"] as DataTemplate;
            lbEntriesDest2.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbEntriesDest2.Margin = new Thickness(5, 0, 0, 0);
            lbEntriesDest2.MaxHeight = 500;

            Grid.SetColumn(lbEntriesDest2, 1);
            grdEntries.Children.Add(lbEntriesDest2);
            
            foreach (RouteTimeTableEntry e in this.Entries.FindAll(ev => ev.Destination.Airport == ev.TimeTable.Route.Destination1))
                lbEntriesDest1.Items.Add(e);
          
            foreach (RouteTimeTableEntry e in this.Entries.FindAll(ev => ev.Destination.Airport == ev.TimeTable.Route.Destination2))
                lbEntriesDest2.Items.Add(e);
          
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
            btnUndo.Visibility = System.Windows.Visibility.Collapsed;

            buttonsPanel.Children.Add(btnUndo);

            return buttonsPanel;

        }

        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            lbEntriesDest1.Items.Clear();
            lbEntriesDest2.Items.Clear();

            this.Entries = new List<RouteTimeTableEntry>(this.Route.TimeTable.Entries);

            foreach (RouteTimeTableEntry ev in this.Entries.FindAll(ev => ev.Destination.Airport == ev.TimeTable.Route.Destination1))
                lbEntriesDest1.Items.Add(ev);

            foreach (RouteTimeTableEntry ev in this.Entries.FindAll(ev => ev.Destination.Airport == ev.TimeTable.Route.Destination2))
                lbEntriesDest2.Items.Add(ev);
      

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
           
            this.Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Route.TimeTable.Entries = new List<RouteTimeTableEntry>(this.Entries);

            foreach (RouteTimeTableEntry entry in this.Route.TimeTable.Entries)
                if (entry.Airliner != null && !entry.Airliner.Routes.Contains(entry.TimeTable.Route))
                    entry.Airliner.Routes.Add(entry.TimeTable.Route);

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

            List<FleetAirliner> airliners = new List<FleetAirliner>(GameObject.GetInstance().HumanAirline.Fleet);

            if (!airliners.Contains(entry.Airliner) && entry.Airliner != null)
                airliners.Add(entry.Airliner);
          
            return airliners;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
   
}
