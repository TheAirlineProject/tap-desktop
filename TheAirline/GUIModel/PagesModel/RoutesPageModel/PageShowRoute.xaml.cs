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
        public List<MVVMRouteClass> Classes { get; set; }
        public List<Route> Legs { get; set; }
         public PageShowRoute(Route route)
        {
           
             this.Classes = new List<MVVMRouteClass>();

            foreach (AirlinerClass.ClassType type in Enum.GetValues(typeof(AirlinerClass.ClassType)))
            {
                MVVMRouteClass rClass = new MVVMRouteClass(type, RouteAirlinerClass.SeatingType.Reserved_Seating, 1);

                this.Classes.Add(rClass);
            }

            this.Route = route;
            this.DataContext = this.Route;

            this.Invoices = new List<MonthlyInvoice>();

            foreach (Invoice.InvoiceType type in this.Route.getRouteInvoiceTypes())
                this.Invoices.Add(new MonthlyInvoice(type, 1950, 1, this.Route.getRouteInvoiceAmount(type)));

            this.Legs = new List<Route>();
            this.Legs.Add(this.Route);
            this.Legs.AddRange(this.Route.Stopovers.SelectMany(s => s.Legs));

            InitializeComponent();
                 
            Boolean inRoute = this.Route.getAirliners().Exists(a => a.Status != FleetAirliner.AirlinerStatus.Stopped);
                
            cbEdit.Visibility = inRoute ? Visibility.Collapsed : System.Windows.Visibility.Visible;
                       
            this.Loaded += PageShowRoute_Loaded;
        }

        private void PageShowRoute_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this.Route.IsCargoRoute)
            {


                foreach (MVVMRouteClass rClass in this.Classes)
                {
                    foreach (MVVMRouteFacility rFacility in rClass.Facilities)
                    {
                        var facility = ((PassengerRoute)this.Route).Classes.Find(c => c.Type == rClass.Type).Facilities.Find(f => f.Type == rFacility.Type);
                        rFacility.SelectedFacility = facility;
                    }

                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            //passenger route
            if (!this.Route.IsCargoRoute)
            {
                
                foreach (MVVMRouteClass rac in this.Classes)
                {
                    ((PassengerRoute)this.Route).getRouteAirlinerClass(rac.Type).FarePrice = rac.FarePrice;

                    ((PassengerRoute)this.Route).getRouteAirlinerClass(rac.Type).Facilities.Clear();

                    foreach (MVVMRouteFacility facility in rac.Facilities)
                        ((PassengerRoute)this.Route).getRouteAirlinerClass(rac.Type).addFacility(facility.SelectedFacility);

                }
            }
            //cargo route
            else
            {
                double cargoPrice = Convert.ToDouble(txtCargoPrice.Text);
                ((CargoRoute)this.Route).PricePerUnit = cargoPrice;
    
            }
        }

    }
  
}
