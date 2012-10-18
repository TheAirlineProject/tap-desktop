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
        public PageFlights()
        {
            this.Entries = Airlines.GetAllAirlines().SelectMany(a => a.Routes.SelectMany(r => r.TimeTable.Entries)).OrderBy(e => e.Time).ToList();

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

            showDayEntries(DayOfWeek.Monday);

          

            //   Page show routes for All airlines + Per dag 
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
                sbDay.IsSelected = day == DayOfWeek.Monday;
                
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
        private void showDayEntries(DayOfWeek day)
        {
            var source = lbFlights.Items as ICollectionView;
            source.Filter = delegate(object item)
            {
                var i = item as RouteTimeTableEntry;
                return i.Day == day;

            };
            source.Refresh();
        }
        private void sbDay_Click(object sender, RoutedEventArgs e)
        {
            DayOfWeek day = (DayOfWeek)((ucSelectButton)sender).Tag;

            showDayEntries(day);
        }
    }
}
