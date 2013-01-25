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

namespace TheAirline.GraphicsModel.PageModel.GeneralModel
{
    //the page for the bottom menu
    public class PageBottomMenu : Page
    {
        private TextBlock txtMoney, txtTime;
        public PageBottomMenu()
        {

            this.SetResourceReference(Page.BackgroundProperty, "BackgroundBottom");

            Border frameBorder = new Border();
            frameBorder.BorderBrush = Brushes.White;
            frameBorder.BorderThickness = new Thickness(2);

            Grid panelMain = UICreator.CreateGrid(3);
            panelMain.Margin = new Thickness(5, 0, 5, 0);
            panelMain.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

            TextBlock txtHuman = new TextBlock();
            txtHuman.FontWeight = FontWeights.Bold;
            
            txtHuman.Text = string.Format(Translator.GetInstance().GetString("PageBottomMenu","1000"), GameObject.GetInstance().HumanAirline.Profile.CEO, GameObject.GetInstance().HumanAirline.Profile.Name);

            Grid.SetColumn(txtHuman, 0);
            panelMain.Children.Add(txtHuman);


            txtTime = new TextBlock();

            if (GameObject.GetInstance().DayRoundEnabled)
                txtTime.Text = GameObject.GetInstance().GameTime.ToLongDateString() + " " + GameObject.GetInstance().TimeZone.ShortDisplayName;
            else
                txtTime.Text = GameObject.GetInstance().GameTime.ToLongDateString() + " " + GameObject.GetInstance().GameTime.ToShortTimeString() + " " + GameObject.GetInstance().TimeZone.ShortDisplayName;//GameObject.GetInstance().GameTime.ToString("dddd MMMM dd, yyyy HH:mm", CultureInfo.CreateSpecificCulture("en-US")) + " " + GameObject.GetInstance().TimeZone.ShortDisplayName;

            txtTime.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            txtTime.FontWeight = FontWeights.Bold;

            Grid.SetColumn(txtTime, 1);
            panelMain.Children.Add(txtTime);

            txtMoney = new TextBlock();

            //txtMoney.Text = string.Format("{0:c}", GameObject.GetInstance().HumanAirline.Money);
            txtMoney.Text = new ValueCurrencyConverter().Convert(GameObject.GetInstance().HumanAirline.Money).ToString();
            txtMoney.Foreground = new Converters.ValueIsMinusConverter().Convert(GameObject.GetInstance().HumanAirline.Money, null, null, null) as Brush;

            txtMoney.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            txtMoney.FontWeight = FontWeights.Bold;

            Grid.SetColumn(txtMoney, 2);
            panelMain.Children.Add(txtMoney);

            frameBorder.Child = panelMain;

            this.Content = frameBorder;

            GameTimer.GetInstance().OnTimeChangedForced += new GameTimer.TimeChanged(PageBottomMenu_OnTimeChanged);

            this.Unloaded += new RoutedEventHandler(PageBottomMenu_Unloaded);
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
