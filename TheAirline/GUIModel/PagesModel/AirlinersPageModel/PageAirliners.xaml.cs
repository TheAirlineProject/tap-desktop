namespace TheAirline.GUIModel.PagesModel.AirlinersPageModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.AirlinerModel;
    using TheAirline.Model.GeneralModel;

    /// <summary>
    ///     Interaction logic for PageAirliners.xaml
    /// </summary>
    public partial class PageAirliners : Page
    {
        #region Constructors and Destructors

        public PageAirliners()
        {
            this.NewestAircrafts =
                AirlinerTypes.GetTypes(a => a.Produced.From <= GameObject.GetInstance().GameTime)
                    .OrderByDescending(a => a.Produced.From)
                    .Take(5)
                    .ToList();
            this.MostUsedAircrafts = new List<AirlineFleetSizeMVVM>();

            var query =
                GameObject.GetInstance()
                    .HumanAirline.Fleet.GroupBy(a => a.Airliner.Type)
                    .Select(group => new { Type = group.Key, Fleet = group })
                    .OrderByDescending(g => g.Fleet.Count());

            var aircrafts = query.Take(Math.Min(query.Count(), 5));

            foreach (var group in aircrafts)
            {
                this.MostUsedAircrafts.Add(new AirlineFleetSizeMVVM(group.Type, group.Fleet.Count()));
            }

            this.Loaded += this.PageAirliners_Loaded;

            this.InitializeComponent();
        }

        #endregion

        #region Public Properties

        public Hashtable AirlinersFilters { get; set; }

        public List<AirlineFleetSizeMVVM> MostUsedAircrafts { get; set; }

        public List<AirlinerType> NewestAircrafts { get; set; }

        #endregion

        #region Methods

        private void PageAirliners_Loaded(object sender, RoutedEventArgs e)
        {
            var frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            frmContent.Navigate(new PageUsedAirliners { Tag = this });
        }

        private void tcMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var control = (TabControl)sender;

            string selection = ((TabItem)control.SelectedItem).Tag.ToString();

            var frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            if (selection == "Used" && frmContent != null)
            {
                frmContent.Navigate(new PageUsedAirliners { Tag = this });
            }

            if (selection == "Order" && frmContent != null)
            {
                frmContent.Navigate(new PageManufacturers { Tag = this });
            }

            if (selection == "New" && frmContent != null)
            {
                frmContent.Navigate(new PageNewAirliners { Tag = this });
            }
            if (selection == "Leasing" && frmContent != null)
            {
                frmContent.Navigate(new PageLeasingAirliners { Tag = this });
            }

            if (selection == "Fleet" && frmContent != null)
            {
                frmContent.Navigate(new PageAirlinersHumanFleet { Tag = this });
            }
        }

        #endregion
    }
}