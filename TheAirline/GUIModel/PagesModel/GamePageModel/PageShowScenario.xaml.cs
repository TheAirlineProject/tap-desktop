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
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.Model.GeneralModel.ScenarioModel;

namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    /// <summary>
    /// Interaction logic for PageShowScenario.xaml
    /// </summary>
    public partial class PageShowScenario : Page
    {
        public Scenario Scenario  { get; set; }
        public List<Scenario> AllScenarios { get; set; }
        public PageShowScenario()
        {
            this.AllScenarios = Scenarios.GetScenarios();
     
            InitializeComponent();

        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageStartMenu());
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            
            ScenarioHelpers.SetupScenario(this.Scenario);
        
        }

        private void cbScenario_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Scenario = (Scenario)((ComboBox)sender).SelectedItem;
            this.DataContext = this.Scenario;
        }
    }
}
