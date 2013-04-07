using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.Converters;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.Model.GeneralModel.Helpers.WorkersModel;
using TheAirline.GraphicsModel.PageModel.PageGameModel;
using System.Windows.Media.Imaging;

namespace TheAirline.GraphicsModel.PageModel.GeneralModel
{
    //the page for the bottom menu
    public class PageBottomMenu : Page
    {
        private TextBlock txtMoney, txtTime;
        private List<NewsFeed> currentNews;
        public PageBottomMenu()
        {

            this.SetResourceReference(Page.BackgroundProperty, "BackgroundBottom");

            Border frameBorder = new Border();
            frameBorder.BorderBrush = Brushes.White;
            frameBorder.BorderThickness = new Thickness(2);

       
            Grid panelMain = UICreator.CreateGrid(3,1);
            panelMain.Margin = new Thickness(5, 0, 5, 0);
            panelMain.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

            TextBlock txtHuman = new TextBlock();
            txtHuman.FontWeight = FontWeights.Bold;
            txtHuman.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            
            txtHuman.Text = string.Format(Translator.GetInstance().GetString("PageBottomMenu","1000"), GameObject.GetInstance().HumanAirline.Profile.CEO, GameObject.GetInstance().HumanAirline.Profile.Name);

            Grid.SetColumn(txtHuman, 0);
            Grid.SetRow(txtHuman, 0);
            panelMain.Children.Add(txtHuman);

            WrapPanel panelTime = new WrapPanel();
       
            Image imgCalendar = new Image();
            imgCalendar.Width = 24;
            imgCalendar.Source = new BitmapImage(new Uri(@"/Data/images/calendar.png", UriKind.RelativeOrAbsolute));
            imgCalendar.MouseDown += imgCalendar_MouseDown;
            RenderOptions.SetBitmapScalingMode(imgCalendar, BitmapScalingMode.HighQuality);
            imgCalendar.ToolTip = UICreator.CreateToolTip("1013");

             panelTime.Children.Add(imgCalendar);
        
            txtTime = new TextBlock();
            txtTime.Width = 300;

            if (GameObject.GetInstance().DayRoundEnabled)
                txtTime.Text = GameObject.GetInstance().GameTime.ToLongDateString() + " " + GameObject.GetInstance().TimeZone.ShortDisplayName;
            else
                txtTime.Text = GameObject.GetInstance().GameTime.ToLongDateString() + " " + GameObject.GetInstance().GameTime.ToShortTimeString() + " " + GameObject.GetInstance().TimeZone.ShortDisplayName;//GameObject.GetInstance().GameTime.ToString("dddd MMMM dd, yyyy HH:mm", CultureInfo.CreateSpecificCulture("en-US")) + " " + GameObject.GetInstance().TimeZone.ShortDisplayName;

            txtTime.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            txtTime.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            txtTime.FontWeight = FontWeights.Bold;

            panelTime.Children.Add(txtTime);

            Grid.SetColumn(panelTime, 1);
            Grid.SetRow(panelTime, 0);
            panelMain.Children.Add(panelTime);

            txtMoney = new TextBlock();

            //txtMoney.Text = string.Format("{0:c}", GameObject.GetInstance().HumanAirline.Money);
            txtMoney.Text = new ValueCurrencyConverter().Convert(GameObject.GetInstance().HumanAirline.Money).ToString();
            txtMoney.Foreground = new Converters.ValueIsMinusConverter().Convert(GameObject.GetInstance().HumanAirline.Money, null, null, null) as Brush;

            txtMoney.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            txtMoney.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            txtMoney.FontWeight = FontWeights.Bold;

            Grid.SetColumn(txtMoney, 2);
            Grid.SetRow(txtMoney, 0);
            panelMain.Children.Add(txtMoney);

            /*
            txtNews = UICreator.CreateTextBlock("Test");
            txtNews.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            txtNews.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            txtNews.Visibility = System.Windows.Visibility.Collapsed;

            Grid.SetColumnSpan(txtNews, 3);
            Grid.SetRow(txtNews, 0);
            Grid.SetColumn(txtNews, 0);

            panelMain.Children.Add(txtNews);*/

            frameBorder.Child = panelMain;

            this.Content = frameBorder;

            currentNews = new List<NewsFeed>();

            GameTimer.GetInstance().OnTimeChangedForced += new GameTimer.TimeChanged(PageBottomMenu_OnTimeChanged);

            this.Unloaded += new RoutedEventHandler(PageBottomMenu_Unloaded);
        }

      

        private void imgCalendar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PageNavigator.NavigateTo(new PageCalendar());
        }

        private void btnCalendar_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageCalendar());
        }

        private void PageBottomMenu_Unloaded(object sender, RoutedEventArgs e)
        {

            GameTimer.GetInstance().OnTimeChangedForced -= new GameTimer.TimeChanged(PageBottomMenu_OnTimeChanged);
        }


        private void PageBottomMenu_OnTimeChanged()
        {
            if (this.IsLoaded)
            {
                if (GameObject.GetInstance().DayRoundEnabled)
                    txtTime.Text = GameObject.GetInstance().GameTime.ToLongDateString() + " " + GameObject.GetInstance().TimeZone.ShortDisplayName;
                else
                    txtTime.Text = GameObject.GetInstance().GameTime.ToLongDateString() + " " + GameObject.GetInstance().GameTime.ToShortTimeString() + " " + GameObject.GetInstance().TimeZone.ShortDisplayName;//GameObject.GetInstance().GameTime.ToString("dddd MMMM dd, yyyy HH:mm", CultureInfo.CreateSpecificCulture("en-US")) + " " + GameObject.GetInstance().TimeZone.ShortDisplayName;

               // txtMoney.Text = string.Format("{0:c}", GameObject.GetInstance().HumanAirline.Money);
                txtMoney.Text = new ValueCurrencyConverter().Convert(GameObject.GetInstance().HumanAirline.Money).ToString();
                txtMoney.Foreground = new Converters.ValueIsMinusConverter().Convert(GameObject.GetInstance().HumanAirline.Money, null, null, null) as Brush;

                if (GameObject.GetInstance().Scenario != null && (GameObject.GetInstance().Scenario.ScenarioFailed != null || GameObject.GetInstance().Scenario.IsSuccess))
                {
                    GameObjectWorker.GetInstance().cancel();

                    WPFMessageBoxResult result;
                    if (GameObject.GetInstance().Scenario.ScenarioFailed != null)
                    {
                        result = WPFMessageBox.Show(Translator.GetInstance().GetString("PageBottomMenu","1001"), GameObject.GetInstance().Scenario.ScenarioFailed.FailureText, WPFMessageBoxButtons.ContinueExit);
                        
                    }
                    else
                    {
                        result = WPFMessageBox.Show(Translator.GetInstance().GetString("PageBottomMenu","1002"), Translator.GetInstance().GetString("PageBottomMenu","1003"), WPFMessageBoxButtons.ContinueExit);
    
                    }

                    if (result == WPFMessageBoxResult.Continue)
                    {
                        GameObjectWorker.GetInstance().start();

                        GameObject.GetInstance().Scenario = null;
                    }
                    else
                    {
                        Setup.SetupGame();
                        PageNavigator.NavigateTo(new PageNewGame());
                        GameObject.RestartInstance();
                        GameTimer.RestartInstance();

                        GameTimer.GetInstance().start();
                        //GameObjectWorker.GetInstance().start();
      
                    }
           
                }

                
            }

        }
    }
}
