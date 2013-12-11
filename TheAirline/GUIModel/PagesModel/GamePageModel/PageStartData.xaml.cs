using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.GUIModel.MasterPageModel.PopUpPageModel;
using TheAirline.GUIModel.ObjectsModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.CountryModel;

namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    /// <summary>
    /// Interaction logic for PageStartData.xaml
    /// </summary>
    public partial class PageStartData : Page
    {
        public PageStartData()
        {
            InitializeComponent();

            Continent continentAll = new Continent("100", "All continents");
                       
            cbContinent.Items.Add(continentAll);

            foreach (Continent continent in Continents.GetContinents())
                cbContinent.Items.Add(continent);

            foreach (Region region in Regions.GetAllRegions())
                cbRegion.Items.Add(region);

            for (int i = 1960; i < 2014; i++)
                cbYear.Items.Insert(0,i);

            cbYear.SelectedIndex = 0;

            cbDifficulty.ItemsSource = DifficultyLevels.GetDifficultyLevels();

            foreach (Airline.AirlineFocus focus in Enum.GetValues(typeof(Airline.AirlineFocus)))
                cbFocus.Items.Add(focus);
        }
        private void cbContinent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Continent selectedContinent = (Continent)cbContinent.SelectedItem;

            cbRegion.Items.Clear();

            if (selectedContinent.Uid == "100")
                foreach (Region region in Regions.GetAllRegions().OrderBy(r => r.Name))
                    cbRegion.Items.Add(region);
            else
            {
                 if (selectedContinent.Regions.Count > 1)
                    cbRegion.Items.Add(Regions.GetRegion("100"));

                foreach (Region region in selectedContinent.Regions.OrderBy(r => r.Name))
                    cbRegion.Items.Add(region);
            }


            cbRegion.SelectedIndex = 0;
        }

       
        private void cbYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Continent continent = (Continent)cbContinent.SelectedItem;

            if (continent == null)
            {
                cbContinent.SelectedIndex = 0;
                continent = (Continent)cbContinent.SelectedItem;
            }

            Region region = (Region)cbRegion.SelectedItem;
            if (region == null)
            {
                cbRegion.SelectedIndex = 0;
                region = (Region)cbRegion.SelectedItem;
            }

            setNumberOfOpponents();

        }

        private void cbRegion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            setNumberOfOpponents();
        }

        //sets the number of opponents
        private void setNumberOfOpponents()
        {
            if (cbYear.SelectedItem != null && cbRegion.SelectedItem != null && cbContinent.SelectedItem != null)
            {
                int year = (int)cbYear.SelectedItem;
                Region region = (Region)cbRegion.SelectedItem;
                Continent continent = (Continent)cbContinent.SelectedItem;

                var airlines = Airlines.GetAirlines(airline => (airline.Profile.Country.Region == region || (region.Uid == "100" && continent.Uid == "100") || (region.Uid == "100" && continent.hasRegion(airline.Profile.Country.Region))) && airline.Profile.Founded <= year && airline.Profile.Folded > year);

                cbOpponents.Items.Clear();

                for (int i = 0; i < airlines.Count; i++)
                    cbOpponents.Items.Add(i);

                cbOpponents.SelectedIndex = Math.Min(cbOpponents.Items.Count - 1, 3);
            }
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            Frame frmContent = UIHelpers.FindChild<Frame>((Page)this.Tag, "frmContent");

            Boolean useRealData = cbReal.IsChecked.Value;

            frmContent.Navigate(new PageAirlineData(new StartDataObject() { MajorAirports = cbMajorAirports.IsChecked.Value, IsPaused = cbPaused.IsChecked.Value,Focus = (Airline.AirlineFocus)cbFocus.SelectedItem, SameRegion = cbSameRegion.IsChecked.Value, RandomOpponents = rbRandomOpponents.IsChecked.Value,  UseDayTurns=rbDayTurns.IsChecked.Value, Difficulty = (DifficultyLevel)cbDifficulty.SelectedItem, NumberOfOpponents = (int)cbOpponents.SelectedItem, Year = (int)cbYear.SelectedItem, Continent = (Continent)cbContinent.SelectedItem, Region = (Region)cbRegion.SelectedItem, RealData=useRealData}) { Tag = this.Tag });
        }
        private void btnStartMenu_Click(object sender, RoutedEventArgs e)
        {
           
            PageNavigator.NavigateTo(new PageStartMenu());
        }
    }
}
