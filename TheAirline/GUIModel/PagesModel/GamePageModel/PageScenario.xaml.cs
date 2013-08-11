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
using TheAirline.Model.GeneralModel.ScenarioModel;

namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    /// <summary>
    /// Interaction logic for PageScenario.xaml
    /// </summary>
    public partial class PageScenario : Page
    {
        public PageScenario()
        {
            this.Loaded += PageScenario_Loaded;
           
            InitializeComponent();

            
        }

        private void PageScenario_Loaded(object sender, RoutedEventArgs e)
        {
            TabControl tab_main = UIHelpers.FindChild<TabControl>(this, "tabMenu");

            foreach (Scenario scenario in Scenarios.GetScenarios().OrderByDescending(s=>s.Difficulty.Name))
            {
                tab_main.Items.Add(new TabItem() { Header = scenario.Name, Tag = scenario });
            }
        }

        private void tabMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabItem selectedItem = (TabItem)((TabControl)sender).SelectedItem;

            Scenario scenario = (Scenario)selectedItem.Tag;

            Frame frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            if (frmContent != null)
                frmContent.Navigate(new PageShowScenario(scenario));

        }
    }
}
