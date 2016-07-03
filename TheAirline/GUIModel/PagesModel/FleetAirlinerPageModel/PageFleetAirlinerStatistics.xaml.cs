using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Models.General;
using TheAirline.Models.General.Statistics;
using TheAirline.Models.Routes;

namespace TheAirline.GUIModel.PagesModel.FleetAirlinerPageModel
{
    /// <summary>
    ///     Interaction logic for FleetAirlinerStatistics.xaml
    /// </summary>
    public partial class PageFleetAirlinerStatistics : Page
    {
        #region Constructors and Destructors

        public PageFleetAirlinerStatistics(FleetAirlinerMVVM airliner)
        {

            Airliner = airliner;
            DataContext = Airliner;

            AirlinerStatistics = new ObservableCollection<FleetAirlinerStatisticsMVVM>();

            InitializeComponent();

            AirlinerStatistics.Add(
                new FleetAirlinerStatisticsMVVM(Airliner.Airliner, StatisticsTypes.GetStatisticsType("Passengers")));
            AirlinerStatistics.Add(
                new FleetAirlinerStatisticsMVVM(
                    Airliner.Airliner,
                    StatisticsTypes.GetStatisticsType("Passengers%")));
            AirlinerStatistics.Add(
                new FleetAirlinerStatisticsMVVM(Airliner.Airliner, StatisticsTypes.GetStatisticsType("Arrivals")));

            var incomes = new List<KeyValuePair<string, int>>();
            var expenses = new List<KeyValuePair<string, int>>();

            var types = Airliner.Airliner.Data.GetTypes();

            int elements = types.Count > 0 ?  Airliner.Airliner.Data.Values.Count / types.Count : 0;

            for (int i = 0; i < Math.Min(elements, 5); i++)
            {
                foreach (string type in types)
                {
                    var data = Airliner.Airliner.Data.GetOrderedValues(type)[i];

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

            demandSeries.Add(new SeriesData { DisplayName = displayName1, Items = incomes });
            demandSeries.Add(new SeriesData { DisplayName = displayName2, Items = expenses });

            cccDOR.DataContext = demandSeries;
            
            //useability
              double totalRouteTime = Airliner.Airliner.HasRoute ? Airliner.Airliner.Routes.SelectMany(r=>r.TimeTable.Entries.Where(e=>e.Airliner == Airliner.Airliner)).Sum(e=>e.TimeTable.Route.GetFlightTime(Airliner.Airliner.Airliner.Type).TotalMinutes) : 0;
            double weekMinutes = new TimeSpan(7,0,0,0).TotalMinutes;

            List<Route> routes = Airliner.Airliner.Routes;
            double totalFilling = routes.Sum(r => r.FillingDegree);
            double avgFilling = routes.Count == 0 ? 0 : totalFilling / routes.Count;

            TimeSpan age = GameObject.GetInstance().GameTime.Subtract(Airliner.Airliner.PurchasedDate);
            TimeSpan flownHours = Airliner.Airliner.Airliner.FlownHours;
            double inairpercent = flownHours.TotalMinutes / age.TotalMinutes;

            var values = new List<KeyValuePair<string, int>>();
            values.Add(new KeyValuePair<string, int>(Translator.GetInstance().GetString("PageFleetAirlinerStatistics", "1005"), double.IsNaN(avgFilling) ? 0 : (int)(avgFilling * 100)));
            values.Add(new KeyValuePair<string,int>(Translator.GetInstance().GetString("PageFleetAirlinerStatistics", "1008"),(int)(totalRouteTime/weekMinutes*100)));
            values.Add(new KeyValuePair<string, int>(Translator.GetInstance().GetString("PageFleetAirlinerStatistics", "1010"), (int)(inairpercent*100)));
            values.Add(new KeyValuePair<string, int>(Translator.GetInstance().GetString("PageFleetAirlinerStatistics", "1009"), (int)Airliner.Airliner.Airliner.Condition));

            rgcFilling.DataContext = new List<KeyValuePair<string, int>> { values[0] }; ;
            rgcUtilization.DataContext = new List<KeyValuePair<string,int>> {values[1]};
            rgcCondition.DataContext = new List<KeyValuePair<string,int>> {values[2]};
            rgcInair.DataContext = new List<KeyValuePair<string, int>> { values[3] };

            //statistics
            var statsSeries = new List<SeriesData>();
            List<KeyValuePair<string, int>> lastYearStats = new List<KeyValuePair<string, int>>();
            List<KeyValuePair<string, int>> currentYearStats = new List<KeyValuePair<string, int>>();

            foreach (FleetAirlinerStatisticsMVVM statsObject in AirlinerStatistics)
            {
                lastYearStats.Add(new KeyValuePair<string, int>(statsObject.Type.Name, (int)statsObject.LastYear));
                currentYearStats.Add(new KeyValuePair<string, int>(statsObject.Type.Name, (int)statsObject.CurrentYear));
            }

            statsSeries.Add(new SeriesData { DisplayName = Translator.GetInstance().GetString("PageAirlineRatings", "1004"), Items = lastYearStats });
            statsSeries.Add(new SeriesData { DisplayName = Translator.GetInstance().GetString("PageAirlineRatings", "1005"), Items = currentYearStats });

            cccStats.DataContext = statsSeries;

        }

        #endregion

        #region Public Properties

        public FleetAirlinerMVVM Airliner { get; set; }

        public ObservableCollection<FleetAirlinerStatisticsMVVM> AirlinerStatistics { get; set; }

        #endregion
    }
}