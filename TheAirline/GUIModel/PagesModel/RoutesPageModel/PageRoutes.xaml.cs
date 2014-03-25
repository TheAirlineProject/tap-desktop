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
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GUIModel.PagesModel.RoutesPageModel
{
    /// <summary>
    /// Interaction logic for PageRoutes.xaml
    /// </summary>
    public partial class PageRoutes : Page
    {
        public List<RouteProfitMVVM> YearToDateProfitRoutes { get; set; }
        public List<RouteProfitMVVM> LastMonthProfitRoutes { get; set; }
        public List<RouteProfitMVVM> ProfitRoutes { get; set; }
        public List<RouteIncomePerPaxMVVM> IncomePaxRoutes { get; set; }
        public List<Route> RequestedRoutes { get; set; }
        public PageRoutes()
        {
            var profitRoutes = GameObject.GetInstance().HumanAirline.Routes.OrderByDescending(r => r.Balance);

            double totalProfit = profitRoutes.Sum(r=>r.Balance);

            this.ProfitRoutes = new List<RouteProfitMVVM>();
            foreach (Route route in profitRoutes.Take(Math.Min(5,profitRoutes.Count())))
            {
                this.ProfitRoutes.Add(new RouteProfitMVVM(route,totalProfit));
            }
        
            var requestedRoutes = GameObject.GetInstance().HumanAirline.Routes.OrderByDescending(r => r.Destination1.getDestinationPassengersRate(r.Destination2,AirlinerClass.ClassType.Economy_Class) + r.Destination2.getDestinationPassengersRate(r.Destination1,AirlinerClass.ClassType.Economy_Class));
            this.RequestedRoutes = requestedRoutes.Take(Math.Min(5,requestedRoutes.Count())).ToList();

            var yearToDateRoutes = GameObject.GetInstance().HumanAirline.Routes.OrderByDescending(r => r.getBalance(new DateTime(GameObject.GetInstance().GameTime.Year, 1, 1), GameObject.GetInstance().GameTime));

            double yearToDateProfit = yearToDateRoutes.Sum(r => r.getBalance(new DateTime(GameObject.GetInstance().GameTime.Year, 1, 1), GameObject.GetInstance().GameTime));

            this.YearToDateProfitRoutes = new List<RouteProfitMVVM>();
            foreach (Route route in yearToDateRoutes.Take(Math.Min(5, yearToDateRoutes.Count())))
                this.YearToDateProfitRoutes.Add(new RouteProfitMVVM(route, yearToDateProfit));

            DateTime lastMonthStartDate = new DateTime(GameObject.GetInstance().GameTime.Year,GameObject.GetInstance().GameTime.Month,1).AddMonths(-1);
            DateTime lastMonthEndDate = new DateTime(GameObject.GetInstance().GameTime.Year,GameObject.GetInstance().GameTime.Month,1);
            
            var lastMonthProfitRoutes = GameObject.GetInstance().HumanAirline.Routes.OrderByDescending(r=>r.getBalance(lastMonthStartDate,lastMonthEndDate));

            double lastMonthProfit = lastMonthProfitRoutes.Sum(r=>r.getBalance(lastMonthStartDate,lastMonthEndDate));

            this.LastMonthProfitRoutes = new List<RouteProfitMVVM>();
            foreach (Route route in lastMonthProfitRoutes.Take(Math.Min(5,lastMonthProfitRoutes.Count())))
                this.LastMonthProfitRoutes.Add(new RouteProfitMVVM(route,lastMonthProfit));

            this.IncomePaxRoutes = new List<RouteIncomePerPaxMVVM>();
            foreach (Route route in GameObject.GetInstance().HumanAirline.Routes)
                this.IncomePaxRoutes.Add(new RouteIncomePerPaxMVVM(route));

            this.IncomePaxRoutes = this.IncomePaxRoutes.OrderByDescending(r => r.IncomePerPax).Take(Math.Min(5, this.IncomePaxRoutes.Count)).ToList();

            this.Loaded += PageRoutes_Loaded;
            InitializeComponent();
        }

        private void PageRoutes_Loaded(object sender, RoutedEventArgs e)
        {
            Frame frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            frmContent.Navigate(new PageHumanRoutes() { Tag = this });

        }
        private void tcMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl control = (TabControl)sender;

            string selection = ((TabItem)control.SelectedItem).Tag.ToString();

            Frame frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            if (selection == "Routes" && frmContent != null)
                frmContent.Navigate(new PageHumanRoutes() { Tag = this });

            if (selection == "Create" && frmContent != null)
                frmContent.Navigate(new PageCreateRoute() { Tag = this });

            if (selection == "Airliners" && frmContent != null)
                frmContent.Navigate(new PageAssignAirliners() { Tag = this });
          
           
        }
    }
}
