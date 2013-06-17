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
using TheAirline.Model.GeneralModel.ScenarioModel;
using TheAirline.Model.AirportModel;
using System.Threading.Tasks;

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

            Button btnNewGame = createMenuButton(Translator.GetInstance().GetString("PageFrontMenu", "200"));
            btnNewGame.Click += btnNewGame_Click;
            panelContent.Children.Add(btnNewGame);

            Button btnPlayScenario = createMenuButton(Translator.GetInstance().GetString("PageFrontMenu", "205"));
            btnPlayScenario.IsEnabled = Scenarios.GetNumberOfScenarios() > 0;
            btnPlayScenario.Click += btnPlayScenario_Click;
            panelContent.Children.Add(btnPlayScenario);

            Button btnLoadGame = createMenuButton(Translator.GetInstance().GetString("PageFrontMenu", "201"));
            btnLoadGame.Click += btnLoadGame_Click;
            panelContent.Children.Add(btnLoadGame);

            Button btnSettings = createMenuButton(Translator.GetInstance().GetString("PageFrontMenu", "202"));
            btnSettings.Click += btnSettings_Click;
            btnSettings.IsEnabled = false;
            panelContent.Children.Add(btnSettings);

            Button btnCredits = createMenuButton(Translator.GetInstance().GetString("PageFrontMenu", "203"));
            btnCredits.Click += btnCredits_Click;
           // btnCredits.IsEnabled = false;
            panelContent.Children.Add(btnCredits);

            Button btnExitGame = createMenuButton(Translator.GetInstance().GetString("PageFrontMenu", "204"));
            btnExitGame.Click += btnExitGame_Click;
            panelContent.Children.Add(btnExitGame);

            base.setTopMenu(new PageTopMenu());

            base.hideNavigator();

            base.hideBottomMenu();

            base.setContent(panelContent);

            base.setHeaderContent(Translator.GetInstance().GetString("PageFrontMenu","1001"));
            
            showPage(this);

        }

        private void btnPlayScenario_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PagePlayScenario());
        }

        private void btnExitGame_Click(object sender, RoutedEventArgs e)
        {
            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "1003"), Translator.GetInstance().GetString("MessageBox", "1003", "message"), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
                PageNavigator.MainWindow.Close();
        }

        private void btnCredits_Click(object sender, RoutedEventArgs e)
        {

            StackPanel panelCredits = new StackPanel();

            Image imgCredits = new Image();
            imgCredits.Source = new BitmapImage(new Uri(AppSettings.getDataPath() + "\\graphics\\credits.png", UriKind.RelativeOrAbsolute));
            imgCredits.Height = GraphicsHelpers.GetContentHeight()-50;
            RenderOptions.SetBitmapScalingMode(imgCredits, BitmapScalingMode.HighQuality);
            panelCredits.Children.Add(imgCredits);

             Button btnOk = new Button();
             btnOk.Uid = "100";
             btnOk.SetResourceReference(Button.StyleProperty, "RoundedButton");
             btnOk.Height = Double.NaN;
             btnOk.Width = 100;
             btnOk.Content = Translator.GetInstance().GetString("General", btnOk.Uid);
             btnOk.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
             btnOk.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
             btnOk.Margin = new Thickness(0, 10, 0, 0);

             btnOk.Click += btnOk_Click;
             
            panelCredits.Children.Add(btnOk);


            base.setContent(panelCredits);
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageFrontMenu());
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

                //LoadSaveHelpers.LoadGame(file);
                SerializedLoadSaveHelpers.LoadGame(file);

                HolidayYear.Clear();

                GeneralHelpers.CreateHolidays(GameObject.GetInstance().GameTime.Year);


                GameTimer.GetInstance().start();
                GameObjectWorker.GetInstance().start();

                PageNavigator.NavigateTo(new PageAirline(GameObject.GetInstance().HumanAirline));
               
                 Action action = () =>
                {
                    PassengerHelpers.CreateDestinationDemand();

                    var airports = Airports.GetAllAirports();
                    int count = airports.Count;

                    //var airports = Airports.GetAirports(a => a != airport && a.Profile.Town != airport.Profile.Town && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) > 50);

                    Parallel.For(0, count - 1, i =>
                    {
                        Parallel.For(i + 1, count, j =>
                        {
                            airports[j].Statics.addDistance(airports[i], MathHelpers.GetDistance(airports[j], airports[i]));

                        });
                    });
                };
                
                Task.Factory.StartNew(action);

              
            
        
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
