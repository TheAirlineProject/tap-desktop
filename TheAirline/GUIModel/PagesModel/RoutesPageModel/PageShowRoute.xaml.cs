using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private HumanRouteMVVM Route;
        public ObservableCollection<MVVMRouteClass> Classes { get; set; }
      
        public PageShowRoute(Route route)
        {

            this.Classes = new ObservableCollection<MVVMRouteClass>();

            this.Route = new HumanRouteMVVM(route);
            this.DataContext = this.Route;
             
            
             foreach (AirlinerClass.ClassType type in AirlinerClass.GetAirlinerTypes())
            {
                if (this.Route.Route is PassengerRoute)
                {
                   RouteAirlinerClass rClass = ((PassengerRoute)this.Route.Route).getRouteAirlinerClass(type);
                    MVVMRouteClass mClass = new MVVMRouteClass(type, rClass.Seating, rClass.FarePrice);

                    this.Classes.Add(mClass);
                }
            }
            
          
            InitializeComponent();
                 
        
            this.Loaded += PageShowRoute_Loaded;
        }

        private void PageShowRoute_Loaded(object sender, RoutedEventArgs e)
        {
            
            if (!this.Route.Route.IsCargoRoute)
            {


                foreach (MVVMRouteClass rClass in this.Classes)
                {
                    foreach (MVVMRouteFacility rFacility in rClass.Facilities)
                    {
                        var facility = ((PassengerRoute)this.Route.Route).Classes.Find(c => c.Type == rClass.Type).Facilities.Find(f => f.Type == rFacility.Type);
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
            if (this.Route.Route is PassengerRoute )
            {
                
                foreach (MVVMRouteClass rac in this.Classes)
                {
                    ((PassengerRoute)this.Route.Route).getRouteAirlinerClass(rac.Type).FarePrice = rac.FarePrice;

                    ((PassengerRoute)this.Route.Route).getRouteAirlinerClass(rac.Type).Facilities.Clear();

                    foreach (MVVMRouteFacility facility in rac.Facilities)
                        ((PassengerRoute)this.Route.Route).getRouteAirlinerClass(rac.Type).addFacility(facility.SelectedFacility);

                }
            }
            //cargo route
            else if (this.Route.Route is CargoRoute)
            {
                double cargoPrice = Convert.ToDouble(txtCargoPrice.Text);
                ((CargoRoute)this.Route.Route).PricePerUnit = cargoPrice;
    
            }
            //mixed route
            else if (this.Route.Route is CombiRoute)
            {
                foreach (MVVMRouteClass rac in this.Classes)
                {
                    ((CombiRoute)this.Route.Route).getRouteAirlinerClass(rac.Type).FarePrice = rac.FarePrice;

                    ((CombiRoute)this.Route.Route).getRouteAirlinerClass(rac.Type).Facilities.Clear();

                    foreach (MVVMRouteFacility facility in rac.Facilities)
                        ((CombiRoute)this.Route.Route).getRouteAirlinerClass(rac.Type).addFacility(facility.SelectedFacility);

                }

                double cargoPrice = Convert.ToDouble(txtCargoPrice.Text);
                ((CombiRoute)this.Route.Route).PricePerUnit = cargoPrice;
            }
        }

        private void btnDeleteRoute_Click(object sender, RoutedEventArgs e)
        {
            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2503"), string.Format(Translator.GetInstance().GetString("MessageBox", "2503", "message"), this.Route.Route.Destination1.Profile.Name, this.Route.Route.Destination2.Profile.Name), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                GameObject.GetInstance().HumanAirline.removeRoute(this.Route.Route);

               
                if (this.Route.Route.HasAirliner)
                    this.Route.Route.getAirliners().ForEach(a => a.removeRoute(this.Route.Route));

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


