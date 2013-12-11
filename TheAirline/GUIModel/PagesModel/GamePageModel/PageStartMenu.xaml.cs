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
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.GUIModel.MasterPageModel.PopUpPageModel;
using TheAirline.GUIModel.PagesModel.OptionsPageModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    /// <summary>
    /// Interaction logic for PageStartMenu.xaml
    /// </summary>
    public partial class PageStartMenu : Page
    {
        public PageStartMenu()
        {
            InitializeComponent();
        }

        private void btnNewGame_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageNewGame());
        }

       
        private void btnExitGame_Click(object sender, RoutedEventArgs e)
        {
            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "1003"), Translator.GetInstance().GetString("MessageBox", "1003", "message"), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
                PageNavigator.MainWindow.Close();
        }
        private void btnLoadGame_Click(object sender, RoutedEventArgs e)
        {
            /*
            //WPFPopUpLoadConfiguration conf = new WPFPopUpLoadConfiguration(Configuration.ConfigurationType.Airliner);
            
            DataTemplate template = this.Resources["TestTemplate"] as DataTemplate;

             WPFRegularPopUp.Show("Test",template,WPFPopUpButtons.YesNo);

   
             DependencyObject o= template.LoadContent();

             FrameworkElementFactory ff= template.VisualTree;
             ComboBox cb = UIHelpers.FindChild<ComboBox>(o, "cbTest");
            */
             
            
            PageNavigator.NavigateTo(new PageMMLoadGame());
        }
        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageSettings());
        }
        private void btnCredits_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageCredits());
        }
    }
}
