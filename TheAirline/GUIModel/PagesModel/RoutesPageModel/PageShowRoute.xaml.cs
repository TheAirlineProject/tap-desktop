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
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GUIModel.HelpersModel;
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

            this.Route = route;
            this.DataContext = this.Route;

            foreach (AirlinerClass.ClassType type in AirlinerClass.GetAirlinerTypes())
            {
                if (this.Route is PassengerRoute)
                {
                   RouteAirlinerClass rClass = ((PassengerRoute)this.Route).getRouteAirlinerClass(type);
                    MVVMRouteClass mClass = new MVVMRouteClass(type, rClass.Seating, rClass.FarePrice);

                    this.Classes.Add(mClass);
                }
            }

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
            TabControl tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

            if (tab_main != null)
            {
               

                var airlinerItem = tab_main.Items.Cast<TabItem>()
       .Where(item => item.Tag.ToString() == "Airliner")
       .FirstOrDefault();

                airlinerItem.Visibility = System.Windows.Visibility.Collapsed;
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

        private void btnDeleteRoute_Click(object sender, RoutedEventArgs e)
        {
            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2503"), string.Format(Translator.GetInstance().GetString("MessageBox", "2503", "message"), this.Route.Destination1.Profile.Name, this.Route.Destination2.Profile.Name), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                GameObject.GetInstance().HumanAirline.removeRoute(this.Route);

               
                if (this.Route.HasAirliner)
                    this.Route.getAirliners().ForEach(a => a.removeRoute(this.Route));

                TabControl tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

                if (tab_main != null)
                {
                    var matchingItem =
         tab_main.Items.Cast<TabItem>()
           .Where(item => item.Tag.ToString() == "Routes")
           .FirstOrDefault();

                    tab_main.SelectedItem = matchingItem;
                }


            }
        }

    }
  
}


