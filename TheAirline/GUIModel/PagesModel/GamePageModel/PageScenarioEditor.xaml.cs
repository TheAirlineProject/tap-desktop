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
using TheAirline.Model.GeneralModel.ScenarioModel;

namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    /// <summary>
    /// Interaction logic for PageScenarioEditor.xaml
    /// </summary>
    public partial class PageScenarioEditor : Page
    {
        public PageScenarioEditor()
        {
            ScenarioMVVM scenario = new ScenarioMVVM();

            InitializeComponent();
        }
        private void setScenario(Scenario scenario)
        {
             
        }
    }
}
