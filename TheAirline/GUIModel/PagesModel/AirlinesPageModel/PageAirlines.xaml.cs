namespace TheAirline.GUIModel.PagesModel.AirlinesPageModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.AirlineModel;

    /// <summary>
    ///     Interaction logic for PageAirlines.xaml
    /// </summary>
    public partial class PageAirlines : Page
    {
        #region Constructors and Destructors

        public PageAirlines()
        {
            List<Airline> airlines = Airlines.GetAllAirlines();
            this.MostGates = new ObservableCollection<Airline>();
                airlines.OrderByDescending(
                    a =>
                        a.Airports.Sum(c => c.AirlineContracts.Where(ac => ac.Airline == a).Sum(ac => ac.NumberOfGates)))
                    .Take(Math.Min(5, airlines.Count))
                    .ToList().ForEach(a=>this.MostGates.Add(a));
                this.MostRoutes = new ObservableCollection<Airline>();
            airlines.OrderByDescending(a => a.Routes.Count).Take(Math.Min(airlines.Count, 5)).ToList().ForEach(a=>this.MostRoutes.Add(a));
            this.LargestFleet = new ObservableCollection<Airline>();
            airlines.OrderByDescending(a => a.Fleet.Count).Take(Math.Min(airlines.Count, 5)).ToList().ForEach(a => this.LargestFleet.Add(a));

            this.Loaded += this.PageAirlines_Loaded;
            this.InitializeComponent();
        }

        #endregion

        #region Public Properties

        public ObservableCollection<Airline> LargestFleet { get; set; }

        public ObservableCollection<Airline> MostGates { get; set; }

        public ObservableCollection<Airline> MostRoutes { get; set; }

        #endregion

        #region Methods

        private void PageAirlines_Loaded(object sender, RoutedEventArgs e)
        {
            var frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");
            frmContent.Navigate(new PageAirlinesStatistics { Tag = this });
        }

        private void tcMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var control = (TabControl)sender;

            string selection = ((TabItem)control.SelectedItem).Tag.ToString();

            var frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            if (selection == "Statistics" && frmContent != null)
            {
                frmContent.Navigate(new PageAirlinesStatistics { Tag = this });
            }

            if (selection == "Stocks" && frmContent != null)
            {
                frmContent.Navigate(new PageAirlinesShares { Tag = this });
            }
        }

        #endregion
    }
}