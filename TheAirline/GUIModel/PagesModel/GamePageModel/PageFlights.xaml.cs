using TheAirline.Models.Airlines;
using TheAirline.Models.General;
using TheAirline.Models.Routes;

namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.GeneralModel;

    /// <summary>
    ///     Interaction logic for PageFlights.xaml
    /// </summary>
    public partial class PageFlights : Page
    {
        #region Constructors and Destructors

        public PageFlights()
        {
            this.AllFlights =
                Airlines.GetAllAirlines().SelectMany(a => a.Routes.SelectMany(r => r.TimeTable.Entries)).ToList();

            IEnumerable<Route> routes = Airlines.GetAllAirlines().SelectMany(a => a.Routes);

            this.InitializeComponent();

            this.Loaded += this.PageFlights_Loaded;
        }

        #endregion

        #region Public Properties

        public List<RouteTimeTableEntry> AllFlights { get; set; }

        #endregion

        #region Methods

        private void PageFlights_Loaded(object sender, RoutedEventArgs e)
        {
            var tab_main = UIHelpers.FindChild<TabControl>(this, "tabMenu");

            if (tab_main != null)
            {
                TabItem matchingItem =
                    tab_main.Items.Cast<TabItem>()
                        .Where(item => item.Tag.ToString() == GameObject.GetInstance().GameTime.DayOfWeek.ToString())
                        .FirstOrDefault();

                tab_main.SelectedItem = matchingItem;
            }
        }

        private void tcMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var control = (TabControl)sender;

            string selection = ((TabItem)control.SelectedItem).Tag.ToString();

            var day = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), selection, true);

            var lvFlights = UIHelpers.FindChild<ListView>(this, "lvFlights");

            if (lvFlights != null)
            {
                var source = lvFlights.Items as ICollectionView;
                source.Filter = delegate(object item)
                {
                    var i = item as RouteTimeTableEntry;
                    return i.Day == day;
                };
            }
        }

        #endregion
    }
}