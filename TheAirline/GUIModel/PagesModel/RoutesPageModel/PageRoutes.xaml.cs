using TheAirline.Models.Airliners;
using TheAirline.Models.General;
using TheAirline.Models.Routes;

namespace TheAirline.GUIModel.PagesModel.RoutesPageModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.GeneralModel;

    /// <summary>
    ///     Interaction logic for PageRoutes.xaml
    /// </summary>
    public partial class PageRoutes : Page
    {
        #region Constructors and Destructors

        public PageRoutes()
        {
            IOrderedEnumerable<Route> profitRoutes =
                GameObject.GetInstance().HumanAirline.Routes.OrderByDescending(r => r.Balance);

            double totalProfit = profitRoutes.Sum(r => r.Balance);

            this.ProfitRoutes = new List<RouteProfitMVVM>();
            foreach (Route route in profitRoutes.Take(Math.Min(5, profitRoutes.Count())))
            {
                this.ProfitRoutes.Add(new RouteProfitMVVM(route, totalProfit));
            }

            IOrderedEnumerable<Route> requestedRoutes =
                GameObject.GetInstance()
                    .HumanAirline.Routes.OrderByDescending(
                        r =>
                            r.Destination1.GetDestinationPassengersRate(
                                r.Destination2,
                                AirlinerClass.ClassType.EconomyClass)
                            + r.Destination2.GetDestinationPassengersRate(
                                r.Destination1,
                                AirlinerClass.ClassType.EconomyClass));
            this.RequestedRoutes = requestedRoutes.Take(Math.Min(5, requestedRoutes.Count())).ToList();

            IOrderedEnumerable<Route> yearToDateRoutes =
                GameObject.GetInstance()
                    .HumanAirline.Routes.OrderByDescending(
                        r =>
                            r.GetBalance(
                                new DateTime(GameObject.GetInstance().GameTime.Year, 1, 1),
                                GameObject.GetInstance().GameTime));

            double yearToDateProfit =
                yearToDateRoutes.Sum(
                    r =>
                        r.GetBalance(
                            new DateTime(GameObject.GetInstance().GameTime.Year, 1, 1),
                            GameObject.GetInstance().GameTime));

            this.YearToDateProfitRoutes = new List<RouteProfitMVVM>();
            foreach (Route route in yearToDateRoutes.Take(Math.Min(5, yearToDateRoutes.Count())))
            {
                this.YearToDateProfitRoutes.Add(new RouteProfitMVVM(route, yearToDateProfit));
            }

            DateTime lastMonthStartDate =
                new DateTime(GameObject.GetInstance().GameTime.Year, GameObject.GetInstance().GameTime.Month, 1)
                    .AddMonths(-1);
            var lastMonthEndDate = new DateTime(
                GameObject.GetInstance().GameTime.Year,
                GameObject.GetInstance().GameTime.Month,
                1);

            IOrderedEnumerable<Route> lastMonthProfitRoutes =
                GameObject.GetInstance()
                    .HumanAirline.Routes.OrderByDescending(r => r.GetBalance(lastMonthStartDate, lastMonthEndDate));

            double lastMonthProfit = lastMonthProfitRoutes.Sum(r => r.GetBalance(lastMonthStartDate, lastMonthEndDate));

            this.LastMonthProfitRoutes = new List<RouteProfitMVVM>();
            foreach (Route route in lastMonthProfitRoutes.Take(Math.Min(5, lastMonthProfitRoutes.Count())))
            {
                this.LastMonthProfitRoutes.Add(new RouteProfitMVVM(route, lastMonthProfit));
            }

            this.IncomePaxRoutes = new List<RouteIncomePerPaxMVVM>();
            foreach (Route route in GameObject.GetInstance().HumanAirline.Routes)
            {
                this.IncomePaxRoutes.Add(new RouteIncomePerPaxMVVM(route));
            }

            this.IncomePaxRoutes =
                this.IncomePaxRoutes.OrderByDescending(r => r.IncomePerPax)
                    .Take(Math.Min(5, this.IncomePaxRoutes.Count))
                    .ToList();

            this.Loaded += this.PageRoutes_Loaded;
            this.InitializeComponent();
        }

        #endregion

        #region Public Properties

        public List<RouteIncomePerPaxMVVM> IncomePaxRoutes { get; set; }

        public List<RouteProfitMVVM> LastMonthProfitRoutes { get; set; }

        public List<RouteProfitMVVM> ProfitRoutes { get; set; }

        public List<Route> RequestedRoutes { get; set; }

        public List<RouteProfitMVVM> YearToDateProfitRoutes { get; set; }

        #endregion

        #region Methods

        private void PageRoutes_Loaded(object sender, RoutedEventArgs e)
        {
            var frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            frmContent.Navigate(new PageHumanRoutes { Tag = this });
        }

        private void tcMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var control = (TabControl)sender;

            string selection = ((TabItem)control.SelectedItem).Tag.ToString();

            var frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            if (selection == "Routes" && frmContent != null)
            {
                frmContent.Navigate(new PageHumanRoutes { Tag = this });
            }

            if (selection == "Create" && frmContent != null)
            {
                frmContent.Navigate(new PageCreateRoute { Tag = this });
            }

            if (selection == "Airliners" && frmContent != null)
            {
                frmContent.Navigate(new PageAssignAirliners { Tag = this });
            }
        }

        #endregion
    }
}