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
using TheAirlineV2.Model.GeneralModel;
using TheAirlineV2.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirlineV2.GraphicsModel.PageModel.PageAirlinesModel;
using TheAirlineV2.GraphicsModel.PageModel.PageRouteModel;
using TheAirlineV2.GraphicsModel.PageModel.PageAirlinerModel;
using TheAirlineV2.GraphicsModel.PageModel.PageAirportsModel;
using TheAirlineV2.GraphicsModel.PageModel.PageAirlineModel;
using System.ComponentModel;
using TheAirlineV2.GraphicsModel.PageModel.PageGameModel;
using TheAirlineV2.Model.GeneralModel.Helpers;
using TheAirlineV2.GraphicsModel.UserControlModel.PopUpWindowsModel;

namespace TheAirlineV2.GraphicsModel.PageModel.GeneralModel
{
    /// <summary>
    /// Interaction logic for PageStandardMenuTop.xaml
    /// </summary>
    public partial class PageStandardMenuTop : PageTopMenu
    {
        public static readonly DependencyProperty EntryProperty =
                             DependencyProperty.Register("NewsText",
                             typeof(string), typeof(PageStandardMenuTop));


        [Category("Common Properties")]
        public String NewsText
        {
            get { return (string)GetValue(EntryProperty); }
            set { SetValue(EntryProperty, value); }
        }
        public PageStandardMenuTop()
        {
            //this.MenuItemNews = new MenuItem();

            setNewsText();

            InitializeComponent();

            int news = GameObject.GetInstance().NewsBox.getUnreadNews().Count;

            Menu menu = this.Resources["MenuMain"] as Menu;
            menu.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            Grid.SetColumn(menu, 0);
         
            panelMain.Children.Add(menu);

           
          
            this.Content = base.panelMain;

            GameTimer.GetInstance().OnTimeChanged += new GameTimer.TimeChanged(PageStandardMenuTop_OnTimeChanged);
        }

        private void PageStandardMenuTop_OnTimeChanged()
        {
            setNewsText();
        }
        //sets the text of the news
        private void setNewsText()
        {
            int news = GameObject.GetInstance().NewsBox.getUnreadNews().Count;
            // ((Run)lnkNews.Inlines.FirstInline).Text = news == 0 ? "News" : string.Format("News ({0})", news);

            this.NewsText = news == 0 ? "News" : string.Format("News ({0})", news);

        }
        private void lnkNews_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageNewsBox());
        }
        
       

        private void lnkSettings_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageSettings());
        }

        private void lnkExit_Click(object sender, RoutedEventArgs e)
        {
           WPFMessageBoxResult result = WPFMessageBox.Show("Exit game", "Are you sure you want to exit the game?", WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
                PageNavigator.MainWindow.Close();
        }
        private void lnkNewGame_Click(object sender, RoutedEventArgs e)
        {
            WPFMessageBoxResult result = WPFMessageBox.Show("New game", "Are you sure you want to start a new game?", WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                GameObject.RestartInstance();
                GameTimer.RestartInstance();
                Setup.SetupGame();
                PageNavigator.NavigateTo(new PageNewGame());
            }
        }

        private void lnkTest_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageTest());
        }

        private void lnkAirlines_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageAirlines());
        }

        private void lnkRoutes_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageRoutes());
        }

        private void lnkAirliners_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageAirliners());
        }

        private void lnkAirports_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageAirports());
        }

        private void lnkHome_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageAirline(GameObject.GetInstance().HumanAirline));
        }

        private void lnkSaveGame_Click(object sender, RoutedEventArgs e)
        {
            String name = (String)PopUpSave.ShowPopUp();

            if (name != null)
            {
                GameObject.GetInstance().Name = name;

                string fileName;

                KeyValuePair<string,string> f = LoadSaveHelpers.GetSavedGames().Find(delegate(KeyValuePair<string,string> fs) { return fs.Key == name; });

                if (f.Key == null)
                {
                    Guid file = Guid.NewGuid();

                    fileName = file.ToString();

                    LoadSaveHelpers.AppendSavedFile(name, fileName);

                }
                else
                {
                    fileName = f.Value;
                }
                  

                LoadSaveHelpers.SaveGame(fileName);

            }
           
        }

        private void lnkLoadGame_Click(object sender, RoutedEventArgs e)
        {
            WPFMessageBoxResult result = WPFMessageBox.Show("Load a saved game", "Are you sure you want to load a saved game?", WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                String file = (String)PopUpLoad.ShowPopUp();

                if (file != null)
                {
                    LoadSaveHelpers.LoadGame(file);

                    PageNavigator.NavigateTo(new PageAirline(GameObject.GetInstance().HumanAirline));
                }
              
            }

           
        }

    }
}
