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
        public PageStartData()
        {
            InitializeComponent();
        }

        [Import]
        public PageStartDataViewModel ViewModel
        {
            get { return DataContext as PageStartDataViewModel; }
            set { DataContext = value; }
        }

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
    }
}