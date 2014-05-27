namespace TheAirline.GUIModel.PagesModel.AlliancesPageModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.AirlineModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.GeneralModel.StatisticsModel;

    /// <summary>
    ///     Interaction logic for PageAlliances.xaml
    /// </summary>
    public partial class PageAlliances : Page
    {
        #region Constructors and Destructors

        public PageAlliances()
        {
            this.HumanAlliances = GameObject.GetInstance().HumanAirline.Alliances;

            List<Alliance> alliances =
                Alliances.GetAlliances()
                    .OrderBy(
                        a =>
                            a.Members.Sum(
                                m =>
                                    m.Airline.Statistics.getStatisticsValue(
                                        GameObject.GetInstance().GameTime.Year - 1,
                                        StatisticsTypes.GetStatisticsType("Passengers"))))
                    .ToList();

            this.LargestAlliances = alliances.Take(Math.Min(5, alliances.Count)).ToList();
            this.Loaded += this.PageAlliances_Loaded;

            this.InitializeComponent();
        }

        #endregion

        #region Public Properties

        public List<Alliance> HumanAlliances { get; set; }

        public List<Alliance> LargestAlliances { get; set; }

        #endregion

        #region Methods

        private void PageAlliances_Loaded(object sender, RoutedEventArgs e)
        {
            var frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            frmContent.Navigate(new PageShowAlliances { Tag = this });
        }

        private void tcMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var control = (TabControl)sender;

            string selection = ((TabItem)control.SelectedItem).Tag.ToString();

            var frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            if (selection == "Alliances" && frmContent != null)
            {
                frmContent.Navigate(new PageShowAlliances { Tag = this });
            }

            if (selection == "Create" && frmContent != null)
            {
                frmContent.Navigate(new PageCreateAlliance { Tag = this });
            }
            ;
        }

        #endregion
    }
}