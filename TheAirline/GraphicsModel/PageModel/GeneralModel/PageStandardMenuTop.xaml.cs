using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GraphicsModel.PageModel.PageAirlinesModel;
using TheAirline.GraphicsModel.PageModel.PageRouteModel;
using TheAirline.GraphicsModel.PageModel.PageAirlinerModel;
using TheAirline.GraphicsModel.PageModel.PageAirportsModel;
using TheAirline.GraphicsModel.PageModel.PageAirlineModel;
using TheAirline.GraphicsModel.PageModel.PageGameModel;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using System.Windows.Controls.Primitives;
using TheAirline.Model.PassengerModel;
using TheAirline.GraphicsModel.PageModel.PageAlliancesModel;
using TheAirline.Model.GeneralModel.HolidaysModel;
using TheAirline.Model.AirlineModel;

namespace TheAirline.GraphicsModel.PageModel.GeneralModel
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
       
            setNewsText();

            InitializeComponent();

            int news = GameObject.GetInstance().NewsBox.getUnreadNews().Count;

            Menu menu = this.Resources["MenuMain"] as Menu;
            menu.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            Grid.SetColumn(menu, 0);
         
            panelMain.Children.Add(menu);

       

            this.Content = base.panelMain;

            GameTimer.GetInstance().OnTimeChanged += new GameTimer.TimeChanged(PageStandardMenuTop_OnTimeChanged);

            this.Unloaded += new RoutedEventHandler(PageStandardMenuTop_Unloaded);
        }
     
        private void PageStandardMenuTop_Unloaded(object sender, RoutedEventArgs e)
        {
            GameTimer.GetInstance().OnTimeChanged -= new GameTimer.TimeChanged(PageStandardMenuTop_OnTimeChanged);

        }

        private void PageStandardMenuTop_OnTimeChanged()
        {
            
            setNewsText();
            
     
      
   
        }
        //sets the text of the news
        private void setNewsText()
        {
            int news = GameObject.GetInstance().NewsBox.getUnreadNews().Count;
    
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
            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "1003"), Translator.GetInstance().GetString("MessageBox", "1003", "message"), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
                PageNavigator.MainWindow.Close();
        }
        private void lnkNewGame_Click(object sender, RoutedEventArgs e)
        {
            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "1001"), Translator.GetInstance().GetString("MessageBox", "1001", "message"), WPFMessageBoxButtons.YesNo);

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
        private void lnkAlliances_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageAlliances());
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
        private void lnkCalendar_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageCalendar());
        }

        private void lnkHome_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageAirline(GameObject.GetInstance().HumanAirline));
        }

        private void lnkSaveGame_Click(object sender, RoutedEventArgs e)
        {
            GameTimer.GetInstance().pause();

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
            GameTimer.GetInstance().start();
            
        }

        private void lnkLoadGame_Click(object sender, RoutedEventArgs e)
        {
            GameTimer.GetInstance().pause();

            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "1002"), Translator.GetInstance().GetString("MessageBox", "1002", "message"), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                String file = (String)PopUpLoad.ShowPopUp();

                if (file != null)
                {

                   
                    LoadSaveHelpers.LoadGame(file);

                    PageNavigator.NavigateTo(new PageAirline(GameObject.GetInstance().HumanAirline));

                    HolidayYear.Clear();

                    GeneralHelpers.CreateHolidays(GameObject.GetInstance().GameTime.Year);
                }
              
            }

            GameTimer.GetInstance().start();
           
        }

    }
}
