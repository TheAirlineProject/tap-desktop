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
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.GraphicsModel.PageModel.PageAirlineModel;
using TheAirline.Model.GeneralModel.HolidaysModel;
using TheAirline.Model.GeneralModel.Helpers.WorkersModel;

namespace TheAirline.GraphicsModel.PageModel.PageGameModel
{
    /// <summary>
    /// Interaction logic for PageFrontMenu.xaml
    /// </summary>
    public partial class PageFrontMenu : StandardPage
    {
        public PageFrontMenu()
        {
             InitializeComponent();

            StackPanel panelContent = new StackPanel();
            panelContent.Margin = new Thickness(10, 0, 10, 0);
            panelContent.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            panelContent.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            Panel panelLogo = UICreator.CreateGameLogo();
            panelLogo.Margin = new Thickness(0, 0, 0, 20);

            panelContent.Children.Add(panelLogo);
           
            Button btnNewGame = createMenuButton("New Game");
            btnNewGame.Click += btnNewGame_Click;
            panelContent.Children.Add(btnNewGame);

            Button btnLoadGame = createMenuButton("Load Game");
            btnLoadGame.Click += btnLoadGame_Click;
            panelContent.Children.Add(btnLoadGame);

            Button btnSettings = createMenuButton("Settings");
            btnSettings.Click += btnSettings_Click;
            btnSettings.IsEnabled = false;
            panelContent.Children.Add(btnSettings);

            Button btnCredits = createMenuButton("Credits");
            btnCredits.Click += btnCredits_Click;
            btnCredits.IsEnabled = false;
            panelContent.Children.Add(btnCredits);

            Button btnExitGame = createMenuButton("Exit Game");
            btnExitGame.Click += btnExitGame_Click;
            panelContent.Children.Add(btnExitGame);

            base.setTopMenu(new PageTopMenu());

            base.hideNavigator();

            base.hideBottomMenu();

            base.setContent(panelContent);

            base.setHeaderContent("Start Menu");
            
            showPage(this);

        }

        private void btnExitGame_Click(object sender, RoutedEventArgs e)
        {
            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "1003"), Translator.GetInstance().GetString("MessageBox", "1003", "message"), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
                PageNavigator.MainWindow.Close();
        }

        private void btnCredits_Click(object sender, RoutedEventArgs e)
        {
            Image imgCredits = new Image();
            imgCredits.Source = new BitmapImage(new Uri(AppSettings.getDataPath() + "\\graphics\\credits.png", UriKind.RelativeOrAbsolute));
            imgCredits.Height = GraphicsHelpers.GetContentHeight();
            RenderOptions.SetBitmapScalingMode(imgCredits, BitmapScalingMode.HighQuality);
            
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void btnLoadGame_Click(object sender, RoutedEventArgs e)
        {
            String file = (String)PopUpLoad.ShowPopUp();

            if (file != null)
            {
                Setup.SetupGame();

                LoadSaveHelpers.LoadGame(file);

                PageNavigator.NavigateTo(new PageAirline(GameObject.GetInstance().HumanAirline));

                HolidayYear.Clear();

                GeneralHelpers.CreateHolidays(GameObject.GetInstance().GameTime.Year);

                GameTimer.GetInstance().start();
                GameObjectWorker.GetInstance().start();

            }
        }

        private void btnNewGame_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageNewGame());
        }

     
        //creates a button for the menu
        private Button createMenuButton(string text)
        {
            Button btnMenu = new Button();
            btnMenu.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnMenu.Height = 50;
            btnMenu.Width = 300;
            btnMenu.Content = text;//Translator.GetInstance().GetString("PageOrderAirliners", btnMenu.Uid);
            btnMenu.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnMenu.Margin = new Thickness(0, 0, 0, 10);

            return btnMenu;
        
        }
   
    }
}
