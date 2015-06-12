using System.ComponentModel.Composition;
using TheAirline.ViewModels.Game;

namespace TheAirline.Views.Game
{
    /// <summary>
    ///     Interaction logic for PageStartData.xaml
    /// </summary>
    [Export("PageStartData")]
    public sealed partial class PageStartData
    {
        #region Constructors and Destructors

        public PageStartData()
        {
            InitializeComponent();
            
            //var continentAll = new Continent("100", "All continents");

            //cbContinent.Items.Add(continentAll);

            //foreach (Continent continent in Continents.GetContinents())
            //{
            //    cbContinent.Items.Add(continent);
            //}

            //foreach (Region region in Regions.GetAllRegions())
            //{
            //    cbRegion.Items.Add(region);
            //}

            //int maxYear = DateTime.Now.Year + 1;

            //for (int i = 1960; i < maxYear; i++)
            //{
            //    cbYear.Items.Insert(0, i);
            //}

            //cbYear.SelectedIndex = 0;

            //cbDifficulty.ItemsSource = DifficultyLevels.GetDifficultyLevels();

            //foreach (Models.Airlines.Airline.AirlineFocus focus in Enum.GetValues(typeof(Models.Airlines.Airline.AirlineFocus)))
            //{
            //    cbFocus.Items.Add(focus);
            //}
        }

        [Import]
        public PageStartDataViewModel ViewModel
        {
            get { return DataContext as PageStartDataViewModel; }
            set { DataContext = value; }
        }

        #endregion

        #region Methods

        //private void btnNext_Click(object sender, RoutedEventArgs e)
        //{
        //    var frmContent = UIHelpers.FindChild<Frame>((Page)Tag, "frmContent");

        //    Boolean useRealData = cbReal.IsChecked.Value;

        //    //frmContent.Navigate(
        //    //    new PageAirlineData(
        //    //        new StartDataObject
        //    //        {
        //    //            SelectedCountries = (cbSelectFull.IsChecked.Value ? new List<Country>() : null),
        //    //            MajorAirports = cbMajorAirports.IsChecked.Value,
        //    //            InternationalAirports = !cbAllAirports.IsChecked.Value && !cbMajorAirports.IsChecked.Value,
        //    //            IsPaused = cbPaused.IsChecked.Value,
        //    //            Focus = (Models.Airlines.Airline.AirlineFocus)cbFocus.SelectedItem,
        //    //            SameRegion = cbSameRegion.IsChecked.Value,
        //    //            RandomOpponents = rbRandomOpponents.IsChecked.Value,
        //    //            UseDayTurns = rbDayTurns.IsChecked.Value,
        //    //            Difficulty = (DifficultyLevel)cbDifficulty.SelectedItem,
        //    //            NumberOfOpponents = (int)cbOpponents.SelectedItem,
        //    //            Year = (int)cbYear.SelectedItem,
        //    //            Continent = (Continent)cbContinent.SelectedItem,
        //    //            Region = (Region)cbRegion.SelectedItem,
        //    //            RealData = useRealData
        //    //        }) { Tag = Tag });
        //}

        //private void btnStartMenu_Click(object sender, RoutedEventArgs e)
        //{
        //    //PageNavigator.NavigateTo(new PageStartMenu());
        //}

        //private void cbContinent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    var selectedContinent = (Continent)cbContinent.SelectedItem;

        //    cbRegion.Items.Clear();

        //    if (selectedContinent.Uid == "100")
        //    {
        //        foreach (Region region in Regions.GetAllRegions().OrderBy(r => r.Name))
        //        {
        //            cbRegion.Items.Add(region);
        //        }
        //    }
        //    else
        //    {
        //        if (selectedContinent.Regions.Count > 1)
        //        {
        //            cbRegion.Items.Add(Regions.GetRegion("100"));
        //        }

        //        foreach (Region region in selectedContinent.Regions.OrderBy(r => r.Name))
        //        {
        //            cbRegion.Items.Add(region);
        //        }
        //    }

        //    cbRegion.SelectedIndex = 0;
        //}

        //private void cbRegion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    setNumberOfOpponents();
        //}

        //private void cbYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    var continent = (Continent)cbContinent.SelectedItem;

        //    if (continent == null)
        //    {
        //        cbContinent.SelectedIndex = 0;
        //        continent = (Continent)cbContinent.SelectedItem;
        //    }

        //    var region = (Region)cbRegion.SelectedItem;
        //    if (region == null)
        //    {
        //        cbRegion.SelectedIndex = 0;
        //        region = (Region)cbRegion.SelectedItem;
        //    }

        //    setNumberOfOpponents();
        //}

        //sets the number of opponents
        //private void setNumberOfOpponents()
        //{
        //    if (cbYear.SelectedItem != null && cbRegion.SelectedItem != null
        //        && cbContinent.SelectedItem != null)
        //    {
        //        int index = cbOpponents.SelectedIndex;
                
        //        var year = (int)cbYear.SelectedItem;
        //        var region = (Region)cbRegion.SelectedItem;
        //        var continent = (Continent)cbContinent.SelectedItem;

              

        //        List<Models.Airlines.Airline> airlines =
        //            Airlines.GetAirlines(
        //                airline =>
        //                    (airline.Profile.Country.Region == region || (region.Uid == "100" && continent.Uid == "100")
        //                     || (region.Uid == "100" && continent.HasRegion(airline.Profile.Country.Region)))
        //                    && airline.Profile.Founded <= year && airline.Profile.Folded > year);

        //        cbOpponents.Items.Clear();

        //        for (int i = 0; i < airlines.Count; i++)
        //        {
        //            cbOpponents.Items.Add(i);
        //        }

        //        if (index != -1 && index < cbOpponents.Items.Count)
        //            cbOpponents.SelectedIndex = index;
        //        else
        //            cbOpponents.SelectedIndex = Math.Min(cbOpponents.Items.Count - 1, 3);
        //    }
        //}

        #endregion
    }
}