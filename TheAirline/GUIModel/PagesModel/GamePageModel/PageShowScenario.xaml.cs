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
        public PageShowScenario(Scenario scenario)
        {
            this.Scenario = scenario;
            this.DataContext = this.Scenario;

            InitializeComponent();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageStartMenu());
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            Size s = PageNavigator.MainWindow.RenderSize;

            GraphicsHelpers.SetContentHeight(s.Height / 2);
            GraphicsHelpers.SetContentWidth(s.Width / 2);

            ScenarioHelpers.SetupScenario(this.Scenario);
        
        }
    }
}
