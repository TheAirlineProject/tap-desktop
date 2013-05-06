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
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.Converters;

namespace TheAirline.GraphicsModel.PageModel.PageAirportModel.PanelAirportModel
{
    /// <summary>
    /// Interaction logic for PageAirportDistances.xaml
    /// </summary>
    public partial class PageAirportDistances : Page
    {
        private Airport Airport;
        public PageAirportDistances(Airport airport)
        {
            this.Airport = airport;

            InitializeComponent();

            StackPanel panelAirports = new StackPanel();
            panelAirports.Margin = new Thickness(0, 10, 50, 0);

            ContentControl lblHeader = new ContentControl();
            lblHeader.ContentTemplate = this.Resources["AirportsHeader"] as DataTemplate;
            panelAirports.Children.Add(lblHeader);

            ListBox lbAirport = new ListBox();
            lbAirport.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbAirport.ItemTemplate = this.Resources["AirportItem"] as DataTemplate;
            lbAirport.MaxHeight = GraphicsHelpers.GetContentHeight() - 100;

            var airports = GameObject.GetInstance().HumanAirline.Airports.FindAll(a=>a!=this.Airport).OrderBy(a=>MathHelpers.GetDistance(a,this.Airport));


            foreach (Airport destAirport in airports )
                lbAirport.Items.Add(new KeyValuePair<Airport,Airport>(this.Airport,destAirport));

            panelAirports.Children.Add(lbAirport);

            this.Content = panelAirports;

   
        }
    }
    //the converter for the distance to an airport
    public class AirportDistanceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
     
            KeyValuePair<Airport, Airport> airports = (KeyValuePair<Airport, Airport>)value;

            double dist = MathHelpers.GetDistance(airports.Key.Profile.Coordinates, airports.Value.Profile.Coordinates);

            return string.Format("{0:0.##} {1}", new NumberToUnitConverter().Convert(dist), new StringToLanguageConverter().Convert("km."));
      

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
