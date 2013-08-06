using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.InvoicesModel;
using TheAirline.Model.GeneralModel.StatisticsModel;

namespace TheAirline.GUIModel.PagesModel.RoutesPageModel
{
    /// <summary>
    /// Interaction logic for PageShowRoute.xaml
    /// </summary>
    public partial class PageShowRoute : Page
    {
        private Route Route;
        public List<MonthlyInvoice> Invoices { get; set; }
        public List<Route> Legs { get; set; }
        public PageShowRoute(Route route)
        {
            this.Route = route;
            this.DataContext = this.Route;

            this.Invoices = new List<MonthlyInvoice>();
            
            foreach (Invoice.InvoiceType type in this.Route.getRouteInvoiceTypes())
                this.Invoices.Add(new MonthlyInvoice(type,1950,1, this.Route.getRouteInvoiceAmount(type)));

            this.Legs = new List<Route>();
            this.Legs.Add(this.Route);
            this.Legs.AddRange(this.Route.Stopovers.SelectMany(s=>s.Legs));

            InitializeComponent();

            if (!route.IsCargoRoute)
                lbClasses.ItemsSource = ((PassengerRoute)route).Classes;

        }

    }
    //the converter for the statistics for a route
    public class RouteStatisticsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Route route = (Route)value;
            List<KeyValuePair<string, object>> stats = new List<KeyValuePair<string, object>>();

            if (route.Type == Route.RouteType.Passenger)
            {
                RouteAirlinerClass raClass = ((PassengerRoute)route).getRouteAirlinerClass(AirlinerClass.ClassType.Economy_Class);

                double passengers = route.Statistics.getStatisticsValue(raClass, StatisticsTypes.GetStatisticsType("Passengers"));
                double avgPassengers = route.Statistics.getStatisticsValue(raClass, StatisticsTypes.GetStatisticsType("Passengers%"));

                stats.Add(new KeyValuePair<string, object>("Total Passengers", passengers));
                stats.Add(new KeyValuePair<string, object>("Average Passengers", avgPassengers));
            
            }
            if (route.Type == Route.RouteType.Cargo)
            {
                double cargo = route.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Cargo"));
                double avgCargo = route.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Cargo%"));

                stats.Add(new KeyValuePair<string, object>("Total Cargo", cargo));
                stats.Add(new KeyValuePair<string, object>("Average Cargo", avgCargo));
            }

            stats.Add(new KeyValuePair<string,object>("Filling Degree",string.Format("{0:0.##} %", route.FillingDegree * 100)));

            return stats;
            /*
             * 
             *   if (leg.Type == Route.RouteType.Mixed || leg.Type == Model.AirlinerModel.RouteModel.Route.RouteType.Passenger)
            {
                RouteAirlinerClass raClass = ((PassengerRoute)leg).getRouteAirlinerClass(AirlinerClass.ClassType.Economy_Class);

                double passengers = leg.Statistics.getStatisticsValue(raClass, StatisticsTypes.GetStatisticsType("Passengers"));
                double avgPassengers = leg.Statistics.getStatisticsValue(raClass, StatisticsTypes.GetStatisticsType("Passengers%"));

                lbLegStatistics.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelRoute", "1009"), UICreator.CreateTextBlock(String.Format("{0:0,0}", passengers))));
                lbLegStatistics.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelRoute", "1010"), UICreator.CreateTextBlock(string.Format("{0:0.##}", avgPassengers))));
             }
            if (leg.Type == Route.RouteType.Mixed || leg.Type == Route.RouteType.Cargo)
            {
                double cargo = leg.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Cargo"));
                double avgCargo = leg.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Cargo%"));

                lbLegStatistics.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelRoute", "1013"), UICreator.CreateTextBlock(String.Format("{0:0,0}", cargo))));
                lbLegStatistics.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelRoute", "1014"), UICreator.CreateTextBlock(string.Format("{0:0.##}", avgCargo))));
        
            }
            lbLegStatistics.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelRoute", "1011"), UICreator.CreateTextBlock(string.Format("{0:0.##} %", leg.FillingDegree * 100))));
     
            lbLegStatistics.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelRoute", "1012"), UICreator.CreateTextBlock(new ValueCurrencyConverter().Convert(leg.Balance).ToString())));
*/

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
