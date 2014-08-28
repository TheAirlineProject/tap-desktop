namespace TheAirline.GUIModel.PagesModel.FleetAirlinerPageModel
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows.Controls;
    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.AirlinerModel.RouteModel;
    using TheAirline.Model.GeneralModel.StatisticsModel;
    using TheAirline.Model.GeneralModel;

    /// <summary>
    ///     Interaction logic for FleetAirlinerStatistics.xaml
    /// </summary>
    public partial class PageFleetAirlinerStatistics : Page
    {
        #region Constructors and Destructors

        public PageFleetAirlinerStatistics(FleetAirlinerMVVM airliner)
        {

            this.Airliner = airliner;
            this.DataContext = this.Airliner;

            this.AirlinerStatistics = new ObservableCollection<FleetAirlinerStatisticsMVVM>();

            this.InitializeComponent();

            this.AirlinerStatistics.Add(
                new FleetAirlinerStatisticsMVVM(this.Airliner.Airliner, StatisticsTypes.GetStatisticsType("Passengers")));
            this.AirlinerStatistics.Add(
                new FleetAirlinerStatisticsMVVM(
                    this.Airliner.Airliner,
                    StatisticsTypes.GetStatisticsType("Passengers%")));
            this.AirlinerStatistics.Add(
                new FleetAirlinerStatisticsMVVM(this.Airliner.Airliner, StatisticsTypes.GetStatisticsType("Arrivals")));

            var incomes = new List<KeyValuePair<string, int>>();
            var expenses = new List<KeyValuePair<string, int>>();

            var types = this.Airliner.Airliner.Data.getTypes();

            int elements = types.Count > 0 ?  this.Airliner.Airliner.Data.Values.Count / types.Count : 0;

            for (int i = 0; i < Math.Min(elements, 5); i++)
            {
                foreach (string type in types)
                {
                    var data = this.Airliner.Airliner.Data.getOrderedValues(type)[i];

                    if (data.Value > 0)
                        incomes.Add(new KeyValuePair<string, int>(type, (int)data.Value));
                    else
                        expenses.Add(new KeyValuePair<string, int>(type, (int)Math.Abs(data.Value)));
                }

            }
            //demands
            var demandSeries = new List<SeriesData>();

            string displayName1 = Translator.GetInstance().GetString("PageFleetAirlinerStatistics", "1006");
            string displayName2 = Translator.GetInstance().GetString("PageFleetAirlinerStatistics", "1007");

            demandSeries.Add(new SeriesData() { DisplayName = displayName1, Items = incomes });
            demandSeries.Add(new SeriesData() { DisplayName = displayName2, Items = expenses });

            this.cccDOR.DataContext = demandSeries;
            
            //useability
              double totalRouteTime = this.Airliner.Airliner.HasRoute ? this.Airliner.Airliner.Routes.SelectMany(r=>r.TimeTable.Entries.Where(e=>e.Airliner == this.Airliner.Airliner)).Sum(e=>e.TimeTable.Route.getFlightTime(this.Airliner.Airliner.Airliner.Type).TotalMinutes) : 0;
            double weekMinutes = new TimeSpan(7,0,0,0).TotalMinutes;

            List<Route> routes = this.Airliner.Airliner.Routes;
            double totalFilling = routes.Sum(r => r.FillingDegree);
            double avgFilling = routes.Count == 0 ? 0 : totalFilling / routes.Count;

            TimeSpan age = GameObject.GetInstance().GameTime.Subtract(this.Airliner.Airliner.PurchasedDate);
            TimeSpan flownHours = this.Airliner.Airliner.Airliner.FlownHours;
            double inairpercent = flownHours.TotalMinutes / age.TotalMinutes;

            var values = new List<KeyValuePair<string, int>>();
            values.Add(new KeyValuePair<string, int>(Translator.GetInstance().GetString("PageFleetAirlinerStatistics", "1005"), double.IsNaN(avgFilling) ? 0 : (int)(avgFilling * 100)));
            values.Add(new KeyValuePair<string,int>(Translator.GetInstance().GetString("PageFleetAirlinerStatistics", "1008"),(int)(totalRouteTime/weekMinutes*100)));
            values.Add(new KeyValuePair<string, int>(Translator.GetInstance().GetString("PageFleetAirlinerStatistics", "1010"), (int)(inairpercent*100)));
            values.Add(new KeyValuePair<string, int>(Translator.GetInstance().GetString("PageFleetAirlinerStatistics", "1009"), (int)this.Airliner.Airliner.Airliner.Condition));

            rgcFilling.DataContext = new List<KeyValuePair<string, int>>() { values[0] }; ;
            rgcUtilization.DataContext = new List<KeyValuePair<string,int>>() {values[1]};
            rgcCondition.DataContext = new List<KeyValuePair<string,int>>() {values[2]};
            rgcInair.DataContext = new List<KeyValuePair<string, int>>() { values[3] };

            //statistics
            var statsSeries = new List<SeriesData>();
            List<KeyValuePair<string, int>> lastYearStats = new List<KeyValuePair<string, int>>();
            List<KeyValuePair<string, int>> currentYearStats = new List<KeyValuePair<string, int>>();

            foreach (FleetAirlinerStatisticsMVVM statsObject in this.AirlinerStatistics)
            {
                lastYearStats.Add(new KeyValuePair<string, int>(statsObject.Type.Name, (int)statsObject.LastYear));
                currentYearStats.Add(new KeyValuePair<string, int>(statsObject.Type.Name, (int)statsObject.CurrentYear));
            }

            statsSeries.Add(new SeriesData() { DisplayName = Translator.GetInstance().GetString("PageAirlineRatings", "1004"), Items = lastYearStats });
            statsSeries.Add(new SeriesData() { DisplayName = Translator.GetInstance().GetString("PageAirlineRatings", "1005"), Items = currentYearStats });

            this.cccStats.DataContext = statsSeries;

        }

        #endregion

        #region Public Properties

        public FleetAirlinerMVVM Airliner { get; set; }

        public ObservableCollection<FleetAirlinerStatisticsMVVM> AirlinerStatistics { get; set; }

        #endregion
    }
}