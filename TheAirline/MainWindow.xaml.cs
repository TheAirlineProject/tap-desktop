using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using Prism.Events;
using Prism.Logging;
using Prism.Regions;
using TheAirline.Infrastructure;
using TheAirline.Infrastructure.Enums;
using TheAirline.Infrastructure.Events;
using TheAirline.ViewModels;
using WPFLocalizeExtension.Engine;

namespace TheAirline
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    [Export]
    public sealed partial class MainWindow
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

            if (state.Mode == ScreenMode.FullScreen)
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

        public bool CanGoBack()
        {
            return false;
        }
        public bool ClearNavigator() { return false; }
        public void NavigateForward() { }
        public void NavigateBack() { }
        public void NavigateTo(Page page) { }
    }
}