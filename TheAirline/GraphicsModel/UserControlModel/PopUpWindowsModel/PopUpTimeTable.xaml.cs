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
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpTimeTable.xaml
    /// </summary>
    public partial class PopUpTimeTable : PopUpWindow
    {
        private Route Route;
        private Airline Airline;
        private RouteTimeTable TimeTable;
        private ComboBox cbHour, cbMinute, cbAirport, cbDay;
         public static object ShowPopUp(Airline airline, Route route)
        {
            PopUpTimeTable window = new PopUpTimeTable(airline, route);
            window.ShowDialog();

            return window.Selected == null ? null : window.Selected;
        }
        public PopUpTimeTable(Airline airline,Route route)
        {

            InitializeComponent();

            this.Uid = "1000";
            this.Route = route;
            this.Airline = airline;
            
            this.TimeTable = new RouteTimeTable(route);

            foreach (RouteTimeTableEntry entry in this.Route.TimeTable.Entries)
                this.TimeTable.addEntry(entry);

            this.Title = Translator.GetInstance().GetString("PopUpTimeTable", this.Uid);

            this.Width = 600;

            this.Height = 450;

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            StackPanel panelMain = new StackPanel();
            panelMain.Margin = new Thickness(10, 10, 10, 10);

            Grid grdMain = UICreator.CreateGrid(2);

            StackPanel panelOutbound = createDestinationPanel(route.Destination1);
            Grid.SetColumn(panelOutbound, 0);
            grdMain.Children.Add(panelOutbound);

            StackPanel panelInbound = createDestinationPanel(route.Destination2);
            Grid.SetColumn(panelInbound, 1);
            grdMain.Children.Add(panelInbound);

            panelMain.Children.Add(grdMain);

            if (this.Airline.IsHuman && !(route.Airliner != null && route.Airliner.Status != RouteAirliner.AirlinerStatus.Stopped))
            {
                panelMain.Children.Add(createNewEntryPanel());

                panelMain.Children.Add(createButtonsPanel());
            }
            else
                panelMain.Children.Add(createCPUButtonsPanel());
            
            this.Content = panelMain;

            showEntries();

        }
        //create the panel for a destination
        private StackPanel createDestinationPanel(Airport airport)
        {
            StackPanel destPanel = new StackPanel();

            destPanel.Margin = new Thickness(5, 0, 5, 0);
            destPanel.Children.Add(createHeader(airport));

            ListBox lbEntries = new ListBox();
            //lbEntries.ItemTemplate = this.Resources["EntryItem"] as DataTemplate;
            lbEntries.ItemTemplate = this.Resources["RouteTimeTableEntryItem"] as DataTemplate;// SetResourceReference(ListBox.ItemTemplateProperty, "RouteTimeTableEntryItem");
            lbEntries.Height = 300;
            lbEntries.Name = airport.Profile.IATACode;
            lbEntries.ItemContainerStyleSelector = new ListBoxItemStyleSelector();

         

            destPanel.Children.Add(lbEntries);

         

            
            return destPanel;

        }
        //shows the entries
        private void showEntries()
        {
            showEntries(this.Route.Destination1);
            showEntries(this.Route.Destination2);
        }
        private void showEntries(Airport airport)
        {
            
            ListBox lb = (ListBox)LogicalTreeHelper.FindLogicalNode((Panel)this.Content, airport.Profile.IATACode);
            lb.Items.Clear();

            List<RouteTimeTableEntry> entries = this.TimeTable.getEntries(airport);
            entries.Sort((delegate(RouteTimeTableEntry e1, RouteTimeTableEntry e2) { return e2.CompareTo(e1); }));

            foreach (RouteTimeTableEntry entry in entries)
                //lbEntries.Items.Add(string.Format("{0} {1:D2}:{2:D2}", entry.Day, entry.Time.Hours, entry.Time.Minutes));
                lb.Items.Add(entry);

           
        }
        //creates the buttons panel is cpu time table
        private WrapPanel createCPUButtonsPanel()
        {
            WrapPanel buttonsPanel = new WrapPanel();
            buttonsPanel.Margin = new Thickness(0, 10, 0, 0);

            Button btnOk = new Button();
            btnOk.Uid = "100";
            btnOk.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnOk.Height = Double.NaN;
            btnOk.Width = Double.NaN;
            btnOk.Content = Translator.GetInstance().GetString("General", btnOk.Uid);
            btnOk.Click += new RoutedEventHandler(btnCPUOk_Click);
            btnOk.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");

            buttonsPanel.Children.Add(btnOk);

            return buttonsPanel;
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
            this.TimeTable = new RouteTimeTable(this.Route);

            foreach (RouteTimeTableEntry entry in this.Route.TimeTable.Entries)
                this.TimeTable.addEntry(entry);

            showEntries();
   
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = null;
            this.Close();
        }
        private void btnCPUOk_Click(object sender, RoutedEventArgs e)
        {

            this.Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (isTimeTableValid())
            {
                this.Selected = this.TimeTable;
                this.Close();
            }
            else
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2504"), Translator.GetInstance().GetString("MessageBox", "2504", "message"), WPFMessageBoxButtons.Ok);
        }
        //creates the panel for adding a new entry
        private StackPanel createNewEntryPanel()
        {
            StackPanel newEntryPanel = new StackPanel();
            newEntryPanel.Margin = new Thickness(0, 10, 0, 0);
            
             
            WrapPanel entryPanel = new WrapPanel();
          
            newEntryPanel.Children.Add(entryPanel);

            cbAirport = new ComboBox();
            
            cbAirport.SetResourceReference(ComboBox.ItemTemplateProperty, "AirportCountryItem");
            //cbAirport.SetResourceReference(ComboBox.ItemTemplateProperty, "CountryFlagLongItem");
            //cbAirport.Background = Brushes.Transparent;
            cbAirport.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbAirport.Items.Add(this.Route.Destination1);
            cbAirport.Items.Add(this.Route.Destination2);
            cbAirport.SelectedIndex = 0;

            entryPanel.Children.Add(cbAirport);


            cbDay = new ComboBox();
            cbDay.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbDay.Width = 100;
            cbDay.Margin = new Thickness(10, 0, 0, 0);
            //cbFacility.IsSynchronizedWithCurrentItem = true;
            //cbFacility.DisplayMemberPath = "Name";
            //cbFacility.SelectedValuePath = "Name";
            cbDay.Items.Add("Daily");

            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                cbDay.Items.Add(day);

            cbDay.SelectedIndex = 0;

            entryPanel.Children.Add(cbDay);

            cbHour = new ComboBox();
            cbHour.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbHour.ItemStringFormat = "{0:D2}";
            cbHour.Margin = new Thickness(5, 0, 0, 0);

            for (int i = 0; i < 24; i++)
                cbHour.Items.Add(i);

            cbHour.SelectedIndex = 0;

            entryPanel.Children.Add(cbHour);

            entryPanel.Children.Add(UICreator.CreateTextBlock(":"));

            cbMinute = new ComboBox();
            cbMinute.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbMinute.ItemStringFormat = "{0:D2}";
            

            for (int i = 0; i < 60; i += 15)
                cbMinute.Items.Add(i);

            cbMinute.SelectedIndex = 0;

            entryPanel.Children.Add(cbMinute);

            Button btnAdd = new Button();
            btnAdd.Uid = "104";
            btnAdd.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnAdd.Height = Double.NaN;
            btnAdd.Width = Double.NaN;
            btnAdd.Click += new RoutedEventHandler(btnAdd_Click);
            btnAdd.Margin = new Thickness(5, 0, 0, 0);
            btnAdd.Content = Translator.GetInstance().GetString("General", btnAdd.Uid);
            btnAdd.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            btnAdd.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");


            entryPanel.Children.Add(btnAdd);

             double dist = MathHelpers.GetDistance(this.Route.Destination1.Profile.Coordinates, this.Route.Destination2.Profile.Coordinates);

             var query = from a in AirlinerTypes.GetTypes().FindAll((delegate(AirlinerType t) { return t.Produced.From < GameObject.GetInstance().GameTime.Year; }))
                         select a.CruisingSpeed;

             double maxSpeed = query.Max();

            TimeSpan minFlightTime = MathHelpers.GetFlightTime(this.Route.Destination1.Profile.Coordinates, this.Route.Destination2.Profile.Coordinates, maxSpeed).Add(RouteTimeTable.MinTimeBetweenFlights);
            newEntryPanel.Children.Add(UICreator.CreateTextBlock(string.Format("Minimum time between flights: {0:D2}:{1:D2}", minFlightTime.Hours, minFlightTime.Minutes)));
        

            return newEntryPanel;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            TimeSpan time = new TimeSpan((int)cbHour.SelectedItem,(int)cbMinute.SelectedItem,0);

            string day = cbDay.SelectedItem.ToString();

            if (day == "Daily")
                // mki, 2011-10-09 we read the FlightCode from the previous TimeTable, instad of using a dummy FlightCode "SA00". The dummy results in an exception, if the "PageRoutes" is not completely loaded new.
                this.TimeTable.addDailyEntries(new RouteEntryDestination((Airport)cbAirport.SelectedItem, this.Route.TimeTable.getRouteEntryDestinations()[cbAirport.SelectedIndex].FlightCode), time);
            else
                // mki, 2011-10-09 we read the FlightCode from the previous TimeTable, instad of using a dummy FlightCode "SA00". The dummy results in an exception, if the "PageRoutes" is not completely loaded new.
                this.TimeTable.addEntry(new RouteTimeTableEntry(this.TimeTable, (DayOfWeek)cbDay.SelectedItem, time, new RouteEntryDestination((Airport)cbAirport.SelectedItem, this.Route.TimeTable.getRouteEntryDestinations()[cbAirport.SelectedIndex].FlightCode)));

            showEntries();
        }
        //returns if the time table is valid
        private Boolean isTimeTableValid()
        {
            var query = from a in AirlinerTypes.GetTypes().FindAll((delegate(AirlinerType t) { return t.Produced.From < GameObject.GetInstance().GameTime.Year; }))
                        select a.CruisingSpeed;

            double maxSpeed = query.Max();

            TimeSpan minFlightTime = MathHelpers.GetFlightTime(this.Route.Destination1.Profile.Coordinates, this.Route.Destination2.Profile.Coordinates, maxSpeed).Add(RouteTimeTable.MinTimeBetweenFlights);
          

            Boolean isValid = true;

            List<RouteTimeTableEntry> entries = this.TimeTable.Entries;
            entries.Sort((delegate(RouteTimeTableEntry e1, RouteTimeTableEntry e2) { return e2.CompareTo(e1); }));

            if (entries.Count == 0) isValid = false;

            for (int i = 0; i < entries.Count; i++)
            {
                int next = i + 1 == entries.Count ? 0 : i + 1;
                RouteTimeTableEntry nextEntry = entries[next];

                TimeSpan difference = entries[i].getTimeDifference(nextEntry);

                if (nextEntry.Destination.Airport == entries[i].Destination.Airport || minFlightTime>difference)
                    isValid = false;
            }

            return isValid;
        }
        //creates a header
        private TextBlock createHeader(Airport airport)
        {
            TextBlock txtHeader = new TextBlock();
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush");
            txtHeader.TextAlignment = TextAlignment.Left;
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = string.Format("To {0} ({1})",airport.Profile.Name,airport.Profile.IATACode);
            
            return txtHeader;
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            RouteTimeTableEntry entry = (RouteTimeTableEntry)((Button)sender).Tag;

            this.TimeTable.removeEntry(entry);

            showEntries();
        }
    }
}
