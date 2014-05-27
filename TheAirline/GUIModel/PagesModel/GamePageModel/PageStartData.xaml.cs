namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.GUIModel.ObjectsModel;
    using TheAirline.Model.AirlineModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.GeneralModel.CountryModel;

    /// <summary>
    ///     Interaction logic for PageStartData.xaml
    /// </summary>
    public partial class PageStartData : Page
    {
        #region Constructors and Destructors

        public PageStartData()
        {
            this.InitializeComponent();

            var continentAll = new Continent("100", "All continents");

            this.cbContinent.Items.Add(continentAll);

            foreach (Continent continent in Continents.GetContinents())
            {
                this.cbContinent.Items.Add(continent);
            }

            foreach (Region region in Regions.GetAllRegions())
            {
                this.cbRegion.Items.Add(region);
            }

            int maxYear = DateTime.Now.Year + 1;

            for (int i = 1960; i < maxYear; i++)
            {
                this.cbYear.Items.Insert(0, i);
            }

            this.cbYear.SelectedIndex = 0;

            this.cbDifficulty.ItemsSource = DifficultyLevels.GetDifficultyLevels();

            foreach (Airline.AirlineFocus focus in Enum.GetValues(typeof(Airline.AirlineFocus)))
            {
                this.cbFocus.Items.Add(focus);
            }
        }

        #endregion

        #region Methods

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            var frmContent = UIHelpers.FindChild<Frame>((Page)this.Tag, "frmContent");

            Boolean useRealData = this.cbReal.IsChecked.Value;

            frmContent.Navigate(
                new PageAirlineData(
                    new StartDataObject
                    {
                        MajorAirports = this.cbMajorAirports.IsChecked.Value,
                        IsPaused = this.cbPaused.IsChecked.Value,
                        Focus = (Airline.AirlineFocus)this.cbFocus.SelectedItem,
                        SameRegion = this.cbSameRegion.IsChecked.Value,
                        RandomOpponents = this.rbRandomOpponents.IsChecked.Value,
                        UseDayTurns = this.rbDayTurns.IsChecked.Value,
                        Difficulty = (DifficultyLevel)this.cbDifficulty.SelectedItem,
                        NumberOfOpponents = (int)this.cbOpponents.SelectedItem,
                        Year = (int)this.cbYear.SelectedItem,
                        Continent = (Continent)this.cbContinent.SelectedItem,
                        Region = (Region)this.cbRegion.SelectedItem,
                        RealData = useRealData
                    }) { Tag = this.Tag });
        }

        private void btnStartMenu_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageStartMenu());
        }

        private void cbContinent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedContinent = (Continent)this.cbContinent.SelectedItem;

            this.cbRegion.Items.Clear();

            if (selectedContinent.Uid == "100")
            {
                foreach (Region region in Regions.GetAllRegions().OrderBy(r => r.Name))
                {
                    this.cbRegion.Items.Add(region);
                }
            }
            else
            {
                if (selectedContinent.Regions.Count > 1)
                {
                    this.cbRegion.Items.Add(Regions.GetRegion("100"));
                }

                foreach (Region region in selectedContinent.Regions.OrderBy(r => r.Name))
                {
                    this.cbRegion.Items.Add(region);
                }
            }

            this.cbRegion.SelectedIndex = 0;
        }

        private void cbRegion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.setNumberOfOpponents();
        }

        private void cbYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var continent = (Continent)this.cbContinent.SelectedItem;

            if (continent == null)
            {
                this.cbContinent.SelectedIndex = 0;
                continent = (Continent)this.cbContinent.SelectedItem;
            }

            var region = (Region)this.cbRegion.SelectedItem;
            if (region == null)
            {
                this.cbRegion.SelectedIndex = 0;
                region = (Region)this.cbRegion.SelectedItem;
            }

            this.setNumberOfOpponents();
        }

        //sets the number of opponents
        private void setNumberOfOpponents()
        {
            if (this.cbYear.SelectedItem != null && this.cbRegion.SelectedItem != null
                && this.cbContinent.SelectedItem != null)
            {
                var year = (int)this.cbYear.SelectedItem;
                var region = (Region)this.cbRegion.SelectedItem;
                var continent = (Continent)this.cbContinent.SelectedItem;

                List<Airline> airlines =
                    Airlines.GetAirlines(
                        airline =>
                            (airline.Profile.Country.Region == region || (region.Uid == "100" && continent.Uid == "100")
                             || (region.Uid == "100" && continent.hasRegion(airline.Profile.Country.Region)))
                            && airline.Profile.Founded <= year && airline.Profile.Folded > year);

                this.cbOpponents.Items.Clear();

                for (int i = 0; i < airlines.Count; i++)
                {
                    this.cbOpponents.Items.Add(i);
                }

                this.cbOpponents.SelectedIndex = Math.Min(this.cbOpponents.Items.Count - 1, 3);
            }
        }

        #endregion
    }
}