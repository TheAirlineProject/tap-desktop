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
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirlineModel;
using System.ComponentModel;
using TheAirline.GraphicsModel.UserControlModel;

namespace TheAirline.GraphicsModel.PageModel.PageFlightsModel
{
    /// <summary>
    /// Interaction logic for PageFlights.xaml
    /// </summary>
    public partial class PageFlights : StandardPage
    {
        private ListBox lbFlights;
        private List<RouteTimeTableEntry> Entries;
        private string sortCriteria;
        private DayOfWeek day;
     
        public PageFlights()
        {
            this.Entries = Airlines.GetAllAirlines().SelectMany(a => a.Routes.SelectMany(r => r.TimeTable.Entries)).ToList();//.OrderBy(e => e.Time).ToList();
            sortCriteria = "Time";

            InitializeComponent();

            this.Uid = "1000";
            this.Title = Translator.GetInstance().GetString("PageFlights", this.Uid);

            StackPanel flightsPanel = new StackPanel();
            flightsPanel.Margin = new Thickness(10, 0, 10, 0);

            flightsPanel.Children.Add(createDaysButtonsPanel());
            flightsPanel.Children.Add(createFlightsPanel());

            //  StandardContentPanel panelContent = new StandardContentPanel();

            //  panelContent.setContentPage(flightsPanel, StandardContentPanel.ContentLocation.Left);

            //   StackPanel panelSideMenu = new PanelAirport(this.Airport);

            //   panelContent.setContentPage(panelSideMenu, StandardContentPanel.ContentLocation.Right);

            base.setContent(flightsPanel);

            base.setHeaderContent(this.Title);

            showPage(this);

            day = GameObject.GetInstance().GameTime.DayOfWeek;
            showDayEntries();

      
          }
        //creates the panel for the day buttons
        private WrapPanel createDaysButtonsPanel()
        {
            WrapPanel panelButtons = new WrapPanel();

            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {

                ucSelectButton sbDay = new ucSelectButton();
                sbDay.Content = day.ToString();
                sbDay.Click += new RoutedEventHandler(sbDay_Click);
                sbDay.Tag = day;
                sbDay.IsSelected = day == GameObject.GetInstance().GameTime.DayOfWeek;
                
                panelButtons.Children.Add(sbDay);
            }

            return panelButtons;

        }

      
        //creates the panel for flights
        private StackPanel createFlightsPanel()
        {
            StackPanel panelFlights = new StackPanel();
            panelFlights.Margin = new Thickness(0, 5, 0, 0);

            ContentControl txtHeader = new ContentControl();
            txtHeader.ContentTemplate = this.Resources["FlightsHeader"] as DataTemplate;
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

            panelFlights.Children.Add(txtHeader);

            lbFlights = new ListBox();
            lbFlights.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbFlights.ItemTemplate = this.Resources["FlightItem"] as DataTemplate;
            lbFlights.ItemsSource = this.Entries;
            lbFlights.MaxHeight = GraphicsHelpers.GetContentHeight() - 100;
            
         
            panelFlights.Children.Add(lbFlights);

            return panelFlights;
          
            
        }
        //shows the entries for a specific day
        private void showDayEntries()
        {
            var source = lbFlights.Items as ICollectionView;
            source.Filter = delegate(object item)
            {
                var i = item as RouteTimeTableEntry;
                return i.Day == day;

            };
            source.SortDescriptions.Clear();
            source.SortDescriptions.Add(new SortDescription(sortCriteria, ListSortDirection.Ascending));
            source.Refresh();
        }
        private void sbDay_Click(object sender, RoutedEventArgs e)
        {
            day = (DayOfWeek)((ucSelectButton)sender).Tag;

            sortCriteria = "Time";
          
            showDayEntries();
        }
        private void Header_Click(object sender, RoutedEventArgs e)
        {
            string type = (string)((Hyperlink)sender).Tag;
            
            switch (type)
            {
                case "Airline":
                    sortCriteria = "Airliner.Airliner.Airline.Profile.Name";
                    showDayEntries();
                    break;
                case "Flight":
                    sortCriteria = "Destination.FlightCode";
                    showDayEntries();
                    break;
                case "Origin":
                    sortCriteria = "DepartureAirport.Profile.Name";
                    showDayEntries();
                    break;
                case "Destination":
                    sortCriteria = "Destination.Airport.Profile.Name";
                    showDayEntries();
                    break;
                case "Time":
                    sortCriteria = "Time";
                    showDayEntries();
                    break;
                case "Type":
                    sortCriteria = "TimeTable.Route.Type";
                    showDayEntries();
                    break;
            }
             
        }
    }
}
