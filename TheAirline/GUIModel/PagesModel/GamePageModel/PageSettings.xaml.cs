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
using TheAirline.Model.GeneralModel;

namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    /// <summary>
    /// Interaction logic for PageSettings.xaml
    /// </summary>
    public partial class PageSettings : Page
    {
        public List<Language> AllLanguages { get { return Languages.GetLanguages().FindAll(l => l.IsEnabled); } private set { ;} }
        public PageSettings()
        {
            InitializeComponent();

            this.Loaded += PageSettings_Loaded;
           
        }

        private void PageSettings_Loaded(object sender, RoutedEventArgs e)
        {
            RadioButton rbFullScreen = UIHelpers.FindChild<RadioButton>(this, "rbFullScreen");
            rbFullScreen.IsChecked = Settings.GetInstance().Mode == Settings.ScreenMode.Fullscreen;

            RadioButton rbWindowed = UIHelpers.FindChild<RadioButton>(this, "rbWindowed");
            if (!rbFullScreen.IsChecked.Value)
                rbWindowed.IsChecked = true;

            ComboBox cbLanguage = UIHelpers.FindChild<ComboBox>(this, "cbLanguage");
            cbLanguage.SelectedItem = AppSettings.GetInstance().getLanguage();

                 
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            RadioButton rbFullScreen = UIHelpers.FindChild<RadioButton>(this, "rbFullScreen");
       
            if (rbFullScreen.IsChecked.Value)
            {
                Settings.GetInstance().Mode = Settings.ScreenMode.Fullscreen;
                PageNavigator.MainWindow.WindowStyle = WindowStyle.None;
                PageNavigator.MainWindow.WindowState = WindowState.Maximized;
                PageNavigator.MainWindow.Focus();
            }
            else
            {
                Settings.GetInstance().Mode = Settings.ScreenMode.Windowed;
            }

            ComboBox cbLanguage = UIHelpers.FindChild<ComboBox>(this, "cbLanguage");
            AppSettings.GetInstance().setLanguage((Language)cbLanguage.SelectedItem);

            System.IO.StreamWriter file = new System.IO.StreamWriter(AppSettings.getCommonApplicationDataPath() + "\\game.settings");
            file.WriteLine(AppSettings.GetInstance().getLanguage().Name);
            file.WriteLine(Settings.GetInstance().Mode);
            file.Close();
      
            //PageNavigator.NavigateTo(new PageStartMenu());
        }
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageStartMenu());
   
        }
        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            RadioButton rbFullScreen = UIHelpers.FindChild<RadioButton>(this, "rbFullScreen");
            rbFullScreen.IsChecked = Settings.GetInstance().Mode == Settings.ScreenMode.Fullscreen;
            
            RadioButton rbWindowed = UIHelpers.FindChild<RadioButton>(this, "rbWindowed");
            if (!rbFullScreen.IsChecked.Value)
                rbWindowed.IsChecked = true;

            ComboBox cbLanguage = UIHelpers.FindChild<ComboBox>(this, "cbLanguage");
            cbLanguage.SelectedItem = AppSettings.GetInstance().getLanguage();

       
        }
    }
}
