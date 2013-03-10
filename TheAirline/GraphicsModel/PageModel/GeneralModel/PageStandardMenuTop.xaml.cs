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
using TheAirline.GraphicsModel.PageModel.PageFlightsModel;
using System.Windows.Threading;
using System.Threading;
using TheAirline.GraphicsModel.PageModel.PagePilotsModel;
using TheAirline.Model.GeneralModel.Helpers.WorkersModel;
using System.Collections;
using TheAirline.Model.AirlineModel.SubsidiaryModel;
using TheAirline.GraphicsModel.PageModel.PageAirlineModel.PanelAirlineModel;
using TheAirline.GraphicsModel.PageModel.PageFinancesModel;

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
            GameTimer.GetInstance().pause();
            GameObjectWorker.GetInstance().cancel();
 
            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "1001"), Translator.GetInstance().GetString("MessageBox", "1001", "message"), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                GameObjectWorker.GetInstance().cancel();

                while (GameObjectWorker.GetInstance().isBusy())
                {
                }
                PageNavigator.NavigateTo(new PageFrontMenu());
                GameObject.RestartInstance();
                GameTimer.RestartInstance();

                Setup.SetupGame();
              
              
               
            }

            GameTimer.GetInstance().start();
            //GameObjectWorker.GetInstance().start();

        }
        private void lnkFlights_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageFlights());
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
        private void lnkFinances_Click(object sender, RoutedEventArgs e)
        {
           // PageNavigator.NavigateTo(new PageFinances());
            PageNavigator.NavigateTo(new PageFinances(GameObject.GetInstance().HumanAirline));
        }
        private void lnkSaveGame_Click(object sender, RoutedEventArgs e)
        {
            
            GameTimer.GetInstance().pause();
            GameObjectWorker.GetInstance().pause();

          
            Popup popUpSplash = new Popup();
            popUpSplash.Child = createSplashWindow("Saving.........");
            popUpSplash.Placement = PlacementMode.Center;
            popUpSplash.PlacementTarget = PageNavigator.MainWindow;
            popUpSplash.IsOpen = false;
   

            String name = (String)PopUpSave.ShowPopUp();
            
            if (name != null)
            {
                popUpSplash.IsOpen = true;
                DoEvents();

                GameObject.GetInstance().Name = name;

                string fileName;

                var saves = LoadSaveHelpers.GetSavedGames();

                  
                KeyValuePair<string,string>? f = LoadSaveHelpers.GetSavedGames().Find(fs=>fs.Key == name);

                if (!f.HasValue)
                {
                    Guid file = Guid.NewGuid();

                    fileName = file.ToString();

                    LoadSaveHelpers.AppendSavedFile(name, fileName);

                }
                else
                {
                    fileName = f.Value.Value;
                }
                
                LoadSaveHelpers.SaveGame(fileName);
                //NewLoadSaveHelpers.SaveGame(fileName);

                popUpSplash.IsOpen = false;
            }
         
            GameTimer.GetInstance().start();
            GameObjectWorker.GetInstance().restart();

            
        }
        private void lnkPilots_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PagePilots());
        }
        private void lnkLoadGame_Click(object sender, RoutedEventArgs e)
        {
           Popup popUpSplash = new Popup();
            popUpSplash.Child = createSplashWindow("Loading.........");
            popUpSplash.Placement = PlacementMode.Center;
            popUpSplash.PlacementTarget = PageNavigator.MainWindow;
            popUpSplash.IsOpen = false;
     
            GameTimer.GetInstance().pause();
            GameObjectWorker.GetInstance().cancel();

            while (GameObjectWorker.GetInstance().isBusy())
            {
            }
           

            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "1002"), Translator.GetInstance().GetString("MessageBox", "1002", "message"), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                String file = (String)PopUpLoad.ShowPopUp();

                if (file != null)
                {
                    popUpSplash.IsOpen = true;
                    DoEvents();
                   
                    LoadSaveHelpers.LoadGame(file);

                    PageNavigator.NavigateTo(new PageAirline(GameObject.GetInstance().HumanAirline));

                    HolidayYear.Clear();

                    GeneralHelpers.CreateHolidays(GameObject.GetInstance().GameTime.Year);

                      Setup.SetupMergers();

                    popUpSplash.IsOpen = false;
                }
              
            }

            GameTimer.GetInstance().start();
            GameObjectWorker.GetInstance().start();
           
        }
        private void lnkCredits_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageCredits());
        }
        private void lnkPerformance_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PagePerformance());
        }
        public void DoEvents()
        {
            DispatcherFrame f = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
            (SendOrPostCallback)delegate(object arg)
            {
                DispatcherFrame fr = arg as DispatcherFrame;
                fr.Continue = false;
            }, f);
            Dispatcher.PushFrame(f);
        }
        //creates a popup with a text
        //creates the splash window
        private Border createSplashWindow(string text)
        {

            Border brdSplasInner = new Border();
            brdSplasInner.BorderBrush = Brushes.Black;
            brdSplasInner.BorderThickness = new Thickness(2, 2, 0, 0);

            Border brdSplashOuter = new Border();
            brdSplashOuter.BorderBrush = Brushes.White;
            brdSplashOuter.BorderThickness = new Thickness(0, 0, 2, 2);

            brdSplasInner.Child = brdSplashOuter;

            TextBlock txtSplash = UICreator.CreateTextBlock(text);
            txtSplash.Width = 200;
            txtSplash.Height = 100;
            txtSplash.TextAlignment = TextAlignment.Center;
            txtSplash.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtSplash.FontWeight = FontWeights.Bold;

            brdSplashOuter.Child = txtSplash;


            return brdSplasInner;

        }

       
      
    }
}
