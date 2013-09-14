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

namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    /// <summary>
    /// Interaction logic for PageNewGame.xaml
    /// </summary>
    public partial class PageNewGame : Page
    {
        public PageNewGame()
        {
            InitializeComponent();

            this.Loaded += PageNewGame_Loaded;
        }

        private void PageNewGame_Loaded(object sender, RoutedEventArgs e)
        {
            Frame frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            frmContent.Navigate(new PageStartData() { Tag = this });
        }

        private void tcMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl control = (TabControl)sender;

            string selection = ((TabItem)control.SelectedItem).Tag.ToString();

            Frame frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");
            
            if (selection == "New" && frmContent != null)
                frmContent.Navigate(new PageStartData() { Tag = this });

            if (selection == "Airline" && frmContent != null)
                frmContent.Navigate(new PageNewAirline());

            if (selection == "Difficulty" && frmContent != null)
                frmContent.Navigate(new PageCreateDifficulty() { Tag = this });

            if (selection == "Scenario" && frmContent != null)
                frmContent.Navigate(new PageShowScenario() { Tag = this });
        }
    }
}
