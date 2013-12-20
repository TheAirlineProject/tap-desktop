using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheAirline.Model.GeneralModel;
using System.Threading;
using System.Globalization;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.Model.GeneralModel.Helpers.WorkersModel;
using TheAirline.GUIModel.HelpersModel;

namespace TheAirline
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {

            InitializeComponent();


            Setup.SetupGame();

            if (Settings.GetInstance().Mode == Settings.ScreenMode.Fullscreen)
            {
                this.WindowStyle = WindowStyle.None;
                this.WindowState = WindowState.Maximized;
                this.Focus();
            }

            PageNavigator.MainWindow = this;

            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;

            if (AppSettings.GetInstance().hasLanguage())
                frmContent.Navigate(new GUIModel.PagesModel.GamePageModel.PageStartMenu());
            else
                frmContent.Navigate(new GUIModel.PagesModel.GamePageModel.PageSelectLanguage());


        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "1003"), Translator.GetInstance().GetString("MessageBox", "1003", "message"), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                    this.Close();
            }
            if (e.Key == Key.F8)
            {
                string text = string.Format("Gameobjectworker paused: {0}\n", GameObjectWorker.GetInstance().isPaused());
                text += string.Format("Gameobjectworker finished: {0}\n", GameObjectWorker.GetInstance().IsFinish);
                text += string.Format("Gameobjectworker errored: {0}\n", GameObjectWorker.GetInstance().IsError);

                Console.WriteLine(text);

                WPFMessageBox.Show("Threads states", text, WPFMessageBoxButtons.Ok);
            }

        }

        //clears the navigator
        public void clearNavigator()
        {
            frmContent.NavigationService.LoadCompleted += new LoadCompletedEventHandler(NavigationService_LoadCompleted);

            // Remove back entries
            while (frmContent.NavigationService.CanGoBack)
                frmContent.NavigationService.RemoveBackEntry();
        }

        private void NavigationService_LoadCompleted(object sender, NavigationEventArgs e)
        {
            frmContent.NavigationService.RemoveBackEntry();

            frmContent.NavigationService.LoadCompleted -= new LoadCompletedEventHandler(NavigationService_LoadCompleted);
        }

        //returns if navigator can go forward
        public Boolean canGoForward()
        {
            return frmContent.NavigationService.CanGoForward;
        }

        //returns if navigator can go back
        public Boolean canGoBack()
        {
            return frmContent.NavigationService.CanGoBack;
        }

        //navigates to a new page
        public void navigateTo(Page page)
        {

            frmContent.Navigate(page);
            frmContent.NavigationService.RemoveBackEntry();


        }

        //moves the navigator forward
        public void navigateForward()
        {

            if (frmContent.NavigationService.CanGoForward)
                frmContent.NavigationService.GoForward();
        }

        //moves the navigator back
        public void navigateBack()
        {

            if (frmContent.NavigationService.CanGoBack)
                frmContent.NavigationService.GoBack();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //social groups + passenger happiness
        }
    }
}
