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
using TheAirlineV2.GraphicsModel.PageModel.GeneralModel;
using TheAirlineV2.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirlineV2.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirlineV2.Model.GeneralModel.Helpers;
using TheAirlineV2.Model.GeneralModel;
using TheAirlineV2.GraphicsModel.PageModel.PageAirlineModel;

namespace TheAirlineV2.GraphicsModel.PageModel.PageGameModel
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

            TextBlock txtStartGame = createMenuLink("START NEW GAME");
            ((Hyperlink)txtStartGame.Inlines.FirstInline).Click+=new RoutedEventHandler(lnkNewGame_Click);
            panelContent.Children.Add(txtStartGame);

            TextBlock txtLoadGame = createMenuLink("LOAD A SAVED GAME");
            ((Hyperlink)txtLoadGame.Inlines.FirstInline).Click += new RoutedEventHandler(PageFrontMenu_Click);
            panelContent.Children.Add(txtLoadGame);

            TextBlock txtSettings = createMenuLink("OPTIONS");
            txtSettings.IsEnabled = false;
            panelContent.Children.Add(txtSettings);

            TextBlock txtCredits = createMenuLink("CREDITS");
            txtCredits.IsEnabled = false;
            panelContent.Children.Add(txtCredits);

            TextBlock txtExitGame = createMenuLink("QUIT GAME");
            ((Hyperlink)txtExitGame.Inlines.FirstInline).Click += new RoutedEventHandler(lnkExitGame_Click);
            panelContent.Children.Add(txtExitGame);

            base.setTopMenu(new PageTopMenu());

            base.hideNavigator();

            base.hideBottomMenu();

            base.setContent(panelContent);

            base.setHeaderContent("Start Menu");
            
            showPage(this);

        }

        private void PageFrontMenu_Click(object sender, RoutedEventArgs e)
        {
            String file = (String)PopUpLoad.ShowPopUp();

            if (file != null)
            {
                Setup.SetupGame();

                LoadSaveHelpers.LoadGame(file);
                
                PageNavigator.ClearNavigator();

                GameTimer.GetInstance().start();

                PageNavigator.NavigateTo(new PageAirline(GameObject.GetInstance().HumanAirline));



            }
        }
        private void lnkNewGame_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageNewGame());
        }
        private void lnkExitGame_Click(object sender, RoutedEventArgs e)
        {
            WPFMessageBoxResult result = WPFMessageBox.Show("Exit game", "Are you sure you want to exit the game?", WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
                PageNavigator.MainWindow.Close();
        }
        //creates a link for the menu
        private TextBlock createMenuLink(string text)
        {
            TextBlock txtLink = new TextBlock();
            txtLink.Margin = new Thickness(0, 0, 0, 10);
            txtLink.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            
            Run run = new Run(text);
        
            run.Style = this.Resources["MenuEntryStyle"] as Style;
             
            Hyperlink hyperLink = new Hyperlink(run);
      
            txtLink.Inlines.Add(hyperLink);
            return txtLink;
        }
      
        private void run_MouseLeave(object sender, MouseEventArgs e)
        {
            Run run = (Run)sender;
            run.Foreground = (Brush)run.Tag;
        }

        private void run_MouseEnter(object sender, MouseEventArgs e)
        {
            Run run = (Run)sender;
            run.Tag = run.Foreground;
            run.SetResourceReference(Run.ForegroundProperty, "HeaderBackgroundBrush2");
        }
      
    }
}
