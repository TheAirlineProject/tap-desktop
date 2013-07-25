using System;
using System.Collections.Generic;
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
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.PageModel.PageGameModel;
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
                cbYear.Items.Add(i);

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
                foreach (Region region in selectedContinent.Regions.OrderBy(r => r.Name))
                    cbRegion.Items.Add(region);

            cbRegion.SelectedIndex = 0;
        }

        private void btnCreateGame_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageFrontMenu());
        }
    }
}
