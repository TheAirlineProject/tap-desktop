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
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.PageModel.PageAirportsModel;
using TheAirline.GraphicsModel.PageModel.PageAirlineModel;
using TheAirline.GraphicsModel.PageModel.PageGameModel;
using System.Threading;
using System.Globalization;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;


namespace TheAirline
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Frame frameMain;

        public MainWindow()
        {
           
            InitializeComponent();

            try
            {
                Setup.SetupGame();

                PageNavigator.MainWindow = this;

                this.Width = SystemParameters.PrimaryScreenWidth;
                this.Height = SystemParameters.PrimaryScreenHeight;

                Canvas mainPanel = new Canvas();

                frameMain = new Frame();
                frameMain.NavigationUIVisibility = NavigationUIVisibility.Hidden;
                //frameMain.Navigate(new PageNewGame());
                frameMain.Navigate(new PageSelectLanguage());

                Canvas.SetTop(frameMain, 0);
                Canvas.SetLeft(frameMain, 0);

                mainPanel.Children.Add(frameMain);

                this.Content = mainPanel;
            }
            catch (Exception e)
            {
                string s = e.ToString();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "1003"), Translator.GetInstance().GetString("MessageBox", "1003", "message"), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                    this.Close();
            }
        
        }

        //clears the navigator
        public void clearNavigator()
        {
            frameMain.NavigationService.LoadCompleted += new LoadCompletedEventHandler(NavigationService_LoadCompleted);

            // Remove back entries
            while (frameMain.NavigationService.CanGoBack)
                frameMain.NavigationService.RemoveBackEntry();
        }

        private void NavigationService_LoadCompleted(object sender, NavigationEventArgs e)
        {
            frameMain.NavigationService.RemoveBackEntry();

            frameMain.NavigationService.LoadCompleted -= new LoadCompletedEventHandler(NavigationService_LoadCompleted);
        }

        //returns if navigator can go forward
        public Boolean canGoForward()
        {
            return frameMain.NavigationService.CanGoForward;
        }

        //returns if navigator can go back
        public Boolean canGoBack()
        {
            return frameMain.NavigationService.CanGoBack;
        }

        //navigates to a new page
        public void navigateTo(Page page)
        {
          
            frameMain.Navigate(page);
            frameMain.NavigationService.RemoveBackEntry();

          
        }

        //moves the navigator forward
        public void navigateForward()
        {

            if (frameMain.NavigationService.CanGoForward)
                frameMain.NavigationService.GoForward();
        }

        //moves the navigator back
        public void navigateBack()
        {

            if (frameMain.NavigationService.CanGoBack)
                frameMain.NavigationService.GoBack();
        }
       
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //social groups + passenger happiness
        }
    }
}
