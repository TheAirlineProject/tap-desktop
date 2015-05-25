using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.Helpers.Workers;
using TheAirline.Infrastructure;
using TheAirline.Infrastructure.Events;
using TheAirline.Models.Airliners;
using TheAirline.Models.Airports;
using TheAirline.ViewModels;
using WPFLocalizeExtension.Engine;

namespace TheAirline
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    [Export]
    public partial class MainWindow
    {
        private readonly ILoggerFacade _logger;
        private readonly LocalizeDictionary _dictionary = LocalizeDictionary.Instance;

        [ImportingConstructor]
        public MainWindow(IEventAggregator eventAggregator, IRegionManager regionManager, ILoggerFacade logger, AppState state)
        {
            Uri mainPage;

            InitializeComponent();

            _logger = logger;

            // Subscribes to the CloseGameEvent and closes the window when triggered.
            eventAggregator.GetEvent<CloseGameEvent>().Subscribe(a => Close());

            if (state.Mode == Infrastructure.Enums.ScreenMode.FullScreen)
            {
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
                Focus();
            }

            if (string.IsNullOrEmpty(state.Language))
            {
                mainPage = new Uri("/PageSelectLanguage", UriKind.Relative);
            }
            else
            {
                _dictionary.SetCultureCommand.Execute(state.Language);
                mainPage = new Uri("/PageStartMenu", UriKind.Relative);
            }

            Width = SystemParameters.PrimaryScreenWidth;
            Height = SystemParameters.PrimaryScreenHeight;

            Loaded += (o, args) =>
            {
                _logger.Log("Navigating to default header and start menu.", Category.Debug, Priority.Medium);
                regionManager.RequestNavigate("HeaderContentRegion", new Uri("/PageHeader", UriKind.Relative));
                regionManager.RequestNavigate("MainContentRegion", mainPage);
            };

            Closing += (o, args) =>
            {
                state.SaveState();
            };

            //Setup.SetupGame();
        }

        [Import]
        public MainWindowViewModel ViewModel
        {
            get { return DataContext as MainWindowViewModel; }
            set { DataContext = value; }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F8)
            {
                string text = $"Gameobjectworker paused: {GameObjectWorker.GetInstance().IsPaused}\n";
                text += $"Gameobjectworker finished: {GameObjectWorker.GetInstance().IsFinish}\n";
                text += $"Gameobjectworker errored: {GameObjectWorker.GetInstance().IsError}\n";

                Console.WriteLine(text);

                WPFMessageBox.Show("Threads states", text, WPFMessageBoxButtons.Ok);
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F12)
            {
                //var file = new StreamWriter(AppSettings.GetCommonApplicationDataPath() + "\\theairline.log");

                if (Airports.Count() >= 5)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        Airport airport = Airports.GetAllAirports()[i];

                        //file.WriteLine("Airport demand for {0} of size {1}", airport.Profile.Name, airport.Profile.Size);
                        //Logger.Info("Airport demand for {0} of size {1}", airport.Profile.Name, airport.Profile.Size);
                        _logger.Log($"Airport demand for {airport.Profile.Name} of size {airport.Profile.Size}", Category.Info, Priority.Low);

                        foreach (Airport demand in airport.GetDestinationDemands())
                        {
                            //file.WriteLine("    Demand to {0} ({2}) is {1}", demand.Profile.Name, airport.GetDestinationPassengersRate(demand, AirlinerClass.ClassType.EconomyClass),
                            //               demand.Profile.Size);
                            //Logger.Info("Demand to {0} ({2}) is {1}", demand.Profile.Name, airport.GetDestinationPassengersRate(demand, AirlinerClass.ClassType.EconomyClass), demand.Profile.Size);
                            _logger.Log($"Demand to {demand.Profile.Name} ({airport.GetDestinationPassengersRate(demand, AirlinerClass.ClassType.EconomyClass)}) is {demand.Profile.Size}", Category.Info, Priority.Low);
                        }
                    }
                }

                WPFMessageBox.Show("Demand has been dumped", "The demand has been dumped to the log file", WPFMessageBoxButtons.Ok);

                //file.Close();
            }
        }

        //clears the navigator
        public void ClearNavigator()
        {
            //frmContent.NavigationService.LoadCompleted += NavigationService_LoadCompleted;

            //// Remove back entries
            //while (frmContent.NavigationService.CanGoBack)
            //    frmContent.NavigationService.RemoveBackEntry();
        }

        //returns if navigator can go forward
        public bool CanGoForward()
        {
            //return frmContent.NavigationService.CanGoForward;
            return false;
        }

        //returns if navigator can go back
        public bool CanGoBack()
        {
            //return frmContent.NavigationService.CanGoBack;
            return false;
        }

        //navigates to a new page
        public void NavigateTo(Page page)
        {
            //frmContent.Navigate(page);
            //frmContent.NavigationService.RemoveBackEntry();
        }

        //moves the navigator forward
        public void NavigateForward()
        {
            //if (frmContent.NavigationService.CanGoForward)
            //    frmContent.NavigationService.GoForward();
        }

        //moves the navigator back
        public void NavigateBack()
        {
            //if (frmContent.NavigationService.CanGoBack)
            //    frmContent.NavigationService.GoBack();
        }
    }
}